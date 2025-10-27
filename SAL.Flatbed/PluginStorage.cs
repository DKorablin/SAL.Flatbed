using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace SAL.Flatbed
{
	/// <summary>Basic plugin storage class</summary>
	public class PluginStorage : IPluginStorage
	{
		private volatile TraceSource _trace;

		private readonly Object _sync = new Object();
		private readonly Object _settingsLock = new Object();
		private readonly Dictionary<IPlugin, ISettingsProvider> _pluginSettings = new Dictionary<IPlugin, ISettingsProvider>();
		private readonly List<ISettingsPluginProvider> _settingsProvider = new List<ISettingsPluginProvider>();

		private List<UnresolvedPlugin> _unresolvedPlugins;
		private IPluginDescription _pluginProvider;

		private IHost Host { get; }

		private TraceSource Trace
			=> this._trace ?? (this._trace = PluginStorage.CreateTraceSource(PluginConstant.TraceSourceName));

		/// <summary>Storage for all loaded plugins</summary>
		private IDictionary<String, IPluginDescription> Plugins { get; } = new Dictionary<String, IPluginDescription>();

		/// <summary>Plugins that can't be loaded for various reasons</summary>
		/// <remarks>Required input parameters not found for these plugins</remarks>
		private List<UnresolvedPlugin> UnresolvedPlugins
			=> this._unresolvedPlugins ?? (this._unresolvedPlugins = new List<UnresolvedPlugin>());

		/// <summary>Gets plugin by ID</summary>
		/// <param name="pluginId">Plugin ID</param>
		/// <remarks>Plugin is is stored in <see cref="System.Runtime.InteropServices.GuidAttribute"/></remarks>
		/// <returns>Found plugin description by plugin ID or null if plugin is not found or pluginId is null</returns>
		public IPluginDescription this[String pluginId]
		{
			get => String.IsNullOrEmpty(pluginId)
				? null
				: this.Plugins.TryGetValue(pluginId, out IPluginDescription result) ? result : null;
		}

		/// <summary>Count of all loaded plugins</summary>
		public Int64 Count => this.Plugins.Count;

		/// <summary>Active plugin loader interface</summary>
		public IPluginProvider PluginProvider => (IPluginProvider)this._pluginProvider?.Instance;

		/// <summary>Event is fired when all plugins are loaded after host launched</summary>
		public event EventHandler PluginsLoaded;

		/// <summary>Event is fired when plugin is unloaded</summary>
		public event EventHandler<PluginEventArgs> PluginUnloaded;

		/// <summary>Create instance basic storage instance and specify host instance</summary>
		/// <param name="host">Application host</param>
		/// <exception cref="ArgumentNullException"><paramref name="host"/> is null</exception>
		public PluginStorage(IHost host)
			=> this.Host = host ?? throw new ArgumentNullException(nameof(host));

		/// <summary>Search for plugins with specific type</summary>
		/// <typeparam name="T">Type of plugin to search</typeparam>
		/// <returns>Array of found plugins</returns>
		public virtual IEnumerable<IPluginDescription> FindPluginType<T>() where T : IPlugin
		{
			foreach(IPluginDescription plugin in this.Plugins.Values)
				if(plugin.Type.InstanceOf<T>())
					yield return plugin;
		}

		private IEnumerable<IPluginDescription> FindPluginTypeI(Type parent)
		{
			foreach(IPluginDescription plugin in this.Plugins.Values)
				if(plugin.Type.InstanceOf(parent))
					yield return plugin;
		}

		/// <summary>Transfer message to plugin by pluginId and method name</summary>
		/// <param name="pluginId">Plugin id</param>
		/// <param name="message">Public method name in the plugin</param>
		/// <param name="args">Method arguments</param>
		/// <returns>Result returned from method or null if method returns void or null</returns>
		/// <exception cref="ArgumentException">Plugin or member not found</exception>
		public virtual Object SendMessage(String pluginId, String message, params Object[] args)
		{
			this.Trace.TraceInformation("Sending message {0} to plugin ID = {1}", message, pluginId);

			IPluginDescription plugin = this[pluginId];
			if(plugin == null)
			{
				var ex = new ArgumentException($"Plugin {pluginId} not found", nameof(pluginId));
				this.Trace.TraceEvent(TraceEventType.Warning, 1, ex.Message);
				throw ex;
			}

			IPluginMethodInfo member = plugin.Type.GetMember<IPluginMethodInfo>(message);
			if(member == null)
			{
				var ex = new ArgumentException($"Method {message} not found in plugin {pluginId}", nameof(message));
				this.Trace.TraceEvent(TraceEventType.Warning, 1, ex.Message);
				throw ex;
			}

			try
			{
				return member.Invoke(args);
			} catch(Exception ex)
			{
				this.Trace.TraceEvent(TraceEventType.Error, 1, "Error invoking {0} on plugin {1}: {2}", message, pluginId, ex);
				throw;
			}
		}

		/// <summary>Event invoker after all plugins are loaded before host is completely loaded</summary>
		protected virtual void OnPluginsLoaded()
			=> this.SafeInvoke(this.PluginsLoaded, this, EventArgs.Empty);

		/// <summary>Event invoker after plugin is unloaded from host</summary>
		/// <param name="plugin">Plugin that was unloaded</param>
		protected virtual void OnPluginUnloaded(IPluginDescription plugin)
			=> this.SafeInvoke(this.PluginUnloaded, this, new PluginEventArgs(plugin));

		/// <summary>Load plugin(s) from assembly</summary>
		/// <param name="assembly">Assembly that will be searched for <see cref="IPlugin"/> instances</param>
		/// <param name="source">Plugin loading source</param>
		/// <param name="mode">Plugin loading mode. (In the runtime or before host is completely loaded)</param>
		/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is null</exception>
		/// <exception cref="ArgumentNullException"><paramref name="source"/> is null</exception>
		public virtual void LoadPlugin(Assembly assembly, String source, ConnectMode mode)
		{
			_ = assembly ?? throw new ArgumentNullException(nameof(assembly));
			if(String.IsNullOrEmpty(source))
				throw new ArgumentNullException(nameof(source));

			this.Trace.TraceInformation("Loading assembly {0} from {1} with mode {2}", assembly.FullName, source, mode);

			foreach(Type pluginType in assembly.GetTypes())
				if(PluginUtils.IsPluginType(pluginType))
					this.LoadPluginType(pluginType, source, mode);
		}

		/// <summary>Add plugin to loaded plugins collection</summary>
		/// <param name="plugin">Basic plugin and assembly description</param>
		/// <param name="mode">How plugin was loaded</param>
		/// <exception cref="ArgumentNullException"><paramref name="plugin"/> is null</exception>
		/// <exception cref="ArgumentException"><paramref name="plugin"/> with same <see cref="IPluginDescription.ID"/> already loaded</exception>
		public virtual void LoadPlugin(IPluginDescription plugin, ConnectMode mode)
		{
			_ = plugin ?? throw new ArgumentNullException(nameof(plugin));

			lock(_sync)
			{
				if(this[plugin.ID] != null)
					throw new ArgumentException($"Plugin {plugin.ID} already loaded", nameof(plugin));

				this.Trace.TraceInformation("Loading {0} (ID={1}) from {2} with mode {3} ...", plugin.Name, plugin.ID, plugin.Source, mode);

				this.Plugins.Add(plugin.ID, plugin);
			}

			if(plugin.Instance != null)
			{
				if(plugin.Instance is IPluginProvider provider)
				{//This is a plugin provider. We need to initialize it and request provider to load plugins
					this.SetPluginProvider(plugin);
					plugin.Instance.OnConnection(mode);
					provider.LoadPlugins();
				} else if(mode != ConnectMode.Startup)
					plugin.Instance.OnConnection(mode);//Initialize plugin after interface loaded
			}
		}

		/// <summary>Add plugin to collection from build using concrete type</summary>
		/// <param name="assembly">Assembly where plugin is located</param>
		/// <param name="type">Type that implements the interface <see cref="IPlugin"/></param>
		/// <param name="source">Plugin loading source</param>
		/// <param name="mode">Plugin loading mode</param>
		/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is null</exception>
		/// <exception cref="ArgumentNullException"><paramref name="type"/> is null or empty string</exception>
		/// <exception cref="ArgumentNullException"><paramref name="source"/> is null or empty string</exception>
		public virtual void LoadPlugin(Assembly assembly, String type, String source, ConnectMode mode)
		{
			_ = assembly ?? throw new ArgumentNullException(nameof(assembly));

			if(String.IsNullOrEmpty(type))
				throw new ArgumentNullException(nameof(type));
			if(String.IsNullOrEmpty(source))
				throw new ArgumentNullException(nameof(source));

			Type pluginType = assembly.GetType(type)
				?? throw new ArgumentNullException(type, $"Type not found in the assembly '{source}'");

			if(!PluginUtils.IsPluginType(pluginType))
				throw new ArgumentException(pluginType.FullName, $"Assembly {assembly.FullName}. Type must be public class and inherit interface {PluginConstant.PluginInterface}");

			this.Trace.TraceInformation("Loading type {0} from assembly '{1}'", pluginType.FullName, assembly.FullName);

			this.LoadPluginType(pluginType, source, mode);
		}

		private Boolean WrapAndLoadPlugin(IPlugin plugin, String source, ConnectMode mode)
		{
			IPluginDescription pluginBase = new PluginDescription(plugin, source);
			IPluginDescription pluginLoaded = this[pluginBase.ID];
			if(pluginLoaded == null)//Adding only unique plugins
			{
				this.LoadPlugin(pluginBase, mode);
				return true;
			} else
			{
				Trace.TraceEvent(TraceEventType.Warning, 10, "Plugin Excluded: {0}({1}). Loaded: {2}({3}) Same ID={4}. Duplicate plugin found",
					pluginBase.Name,
					pluginBase.Source,
					pluginLoaded.Name,
					pluginLoaded.Source,
					pluginBase.ID);
				return false;
			}
		}

		/// <summary>Initialize all plugins. Invoked after specifying folder with assemblies</summary>
		public virtual void InitializePlugins()
		{
			//Initializing Kernel Plugins
			foreach(IPluginDescription plugin in this.FindPluginType<IPluginKernel>())
				try
				{
					plugin.Instance?.OnConnection(ConnectMode.Startup);
				} catch(Exception exc)
				{
					this.Trace.TraceData(TraceEventType.Error, 1, exc);
				}
			//TODO: If you want to delegate plugin initialization to the kernel, you need to extend (add IPluginKernelEx) the IPluginKernel plugin interface, in which all plugins will be initialized
			// if(this.Kernel is IPluginKernelEx) if(!((IPluginKernelEx)this.Kernel).InitializeEx()) Application.Exit(); else foreach(var plugin in this.Plugins)...

			//Initializing all plugins except Kernel plugins
			foreach(IPluginDescription plugin in this.Plugins.Values)
				if(!plugin.Type.InstanceOf<IPluginKernel>())
					try
					{
						plugin.Instance?.OnConnection(ConnectMode.Startup);
					} catch(Exception exc)
					{
						this.Trace.TraceData(TraceEventType.Error, 1, exc);
					}

			this.OnPluginsLoaded();
		}

		/// <summary>Remove plugin from plugins array</summary>
		/// <param name="plugin">Base plugin description to remove from loaded plugins</param>
		/// <exception cref="ArgumentNullException">plugin can't be null</exception>
		/// <exception cref="ArgumentException">plugin not found in the loaded plugins collection</exception>
		public virtual Boolean UnloadPlugin(IPluginDescription plugin)
		{
			_ = plugin ?? throw new ArgumentNullException(nameof(plugin));

			Boolean isDisconnected = plugin.Instance.OnDisconnection(DisconnectMode.UserClosed);
			if(isDisconnected)
			{
				this.OnPluginUnloaded(plugin);

				this.Trace.TraceInformation("Unloading plugin ID = {0}", plugin.ID);

				if(!this.Plugins.Remove(plugin.ID))
					throw new ArgumentException($"Plugin {plugin.ID} not found in the collection");
			}

			return isDisconnected;
		}

		/// <summary>Remove all plugins from collection</summary>
		public virtual void RemovePlugins()
			=> this.Plugins.Clear();

		/// <inheritdoc/>
		void IPluginStorage.SetSetingsProvider(IPluginDescription plugin)
			=> this.SetSettingsProvider(plugin);

		/// <summary>Set new settings provider</summary>
		/// <param name="plugin">Plugin that is installed as a settings provider</param>
		/// <exception cref="ArgumentNullException">Plugin can't be null</exception>
		/// <exception cref="ArgumentException">Removed plugin can't be set as a settings provider</exception>
		public void SetSettingsProvider(IPluginDescription plugin)
		{
			_ = plugin ?? throw new ArgumentNullException(nameof(plugin), "Plugin is null");
			if(plugin.Instance == null)
				throw new ArgumentException($"Remote plugin {plugin.ID} cant be se as Settings Provider", nameof(plugin));

			//Installing the parent bootloader
			this.Trace.TraceInformation("Set Settings Provider ID = {0}", plugin.ID);

			this._settingsProvider.Insert(0, (ISettingsPluginProvider)plugin.Instance);
		}

		/// <summary>Get plugin settings provider</summary>
		/// <param name="plugin">Plugin for which get plugin settings provider</param>
		/// <exception cref="ArgumentException"><c>plugin</c> does not inherits <c>IPluginSettings</c></exception>
		/// <exception cref="ArgumentNullException"><c>((IPluginSettings)plugin).Settings</c> is null</exception>
		/// <returns>Settings provider fixed for this plugin</returns>
		public ISettingsProvider Settings(IPlugin plugin)
		{
			_ = plugin ?? throw new ArgumentNullException(nameof(plugin));

			if(!this._pluginSettings.TryGetValue(plugin, out ISettingsProvider result))
				lock(this._settingsLock)
					if(!this._pluginSettings.TryGetValue(plugin, out result))
					{
						if(!(plugin is IPluginSettings))
							throw new ArgumentException("Plugin does not support customization");
						/*if(settings.Settings == null)//We can't check this property, otherwise we'll get the StackOverflowException. Because the Settings haven't been created yet.
							throw new ArgumentNullException("Plugin.Settings");*/

						foreach(ISettingsPluginProvider pluginProvider in this._settingsProvider)
						{
							if(plugin != pluginProvider)//To avoid selecting yourself as the recipient of settings
								result = pluginProvider.GetSettingsProvider(plugin);
							if(result != null)
							{
								this._pluginSettings.Add(plugin, result);
								break;
							}
						}
					}
			return result;
		}

		/// <summary>Install plugin provider</summary>
		/// <param name="plugin">Plugin that is installed as a plugin provider</param>
		/// <exception cref="ArgumentNullException">Plugin can't be null</exception>
		/// <exception cref="ArgumentException">Removed plugin can't be used as plugin provider</exception>
		public void SetPluginProvider(IPluginDescription plugin)
		{
			_ = plugin ?? throw new ArgumentNullException(nameof(plugin), "Plugin provider is null");
			if(plugin.Instance == null)
				throw new ArgumentException($"Remote plugin {plugin.ID} cant be se as Settings Provider", nameof(plugin));

			//Installing the parent bootloader
			this.Trace.TraceInformation("Set Plugin Provider ID = {0}", plugin.ID);

			if(this._pluginProvider != null)
				((IPluginProvider)plugin.Instance).ParentProvider = (IPluginProvider)this._pluginProvider.Instance;
			this._pluginProvider = plugin;
		}

		/// <summary>Get stream of all loaded plugins</summary>
		/// <returns>Enumerated type interface</returns>
		public IEnumerator<IPluginDescription> GetEnumerator()
		{
			foreach(IPluginDescription plugin in this.Plugins.Values)
				yield return plugin;
		}

		/// <summary>Get an enumerator for plugins</summary>
		/// <returns>Enumerated type interface</returns>
		IEnumerator IEnumerable.GetEnumerator()
			=> this.GetEnumerator();

		private void LoadPluginType(Type pluginType, String source, ConnectMode mode)
		{
			IPlugin plugin = this.CreatePluginInstance(pluginType);
			if(plugin == null)
			{
				this.Trace.TraceEvent(TraceEventType.Warning, 10, "Plugin: {0}:{1}. Unresolved references.", pluginType, source);
				this.UnresolvedPlugins.Add(new UnresolvedPlugin(pluginType, source, mode));
			} else if(this.WrapAndLoadPlugin(plugin, source, mode)
				&& this._unresolvedPlugins != null)
			{
				for(Int32 loop = this._unresolvedPlugins.Count - 1; loop >= 0; loop--)
				{
					var unresolved = this._unresolvedPlugins[loop];
					IPlugin uPlugin = this.CreatePluginInstance(unresolved.PluginType);
					if(uPlugin != null && this.WrapAndLoadPlugin(uPlugin, unresolved.Source, unresolved.Mode))
					{
						this._unresolvedPlugins.RemoveAt(loop);
						loop = this._unresolvedPlugins.Count;//We start the cycle from the beginning, because the element might have already been loaded.
					}
				}
			}
		}

		private IPlugin CreatePluginInstance(Type pluginType)
		{
			IPlugin result = null;
			foreach(ConstructorInfo ctor in pluginType.GetConstructors(BindingFlags.Public | BindingFlags.Instance))
			{
				ParameterInfo[] parameterTypes = ctor.GetParameters();
				if(parameterTypes.Length == 0)
				{
					result = (IPlugin)Activator.CreateInstance(pluginType);
					break;
				} else
				{//FEATURE: New code
					result = this.ResolveAndCreate(pluginType, ctor);
					if(result != null)
						break;
				}
			}

			return result;
		}

		/// <summary>Try to resolve all dependencies and create plugin instance</summary>
		/// <param name="pluginType">Plugin type that resolves <see cref="IPlugin"/> interface</param>
		/// <param name="ctor">Type constructor that need to be resolved</param>
		/// <returns>Created plugin instance</returns>
		private IPlugin ResolveAndCreate(Type pluginType, ConstructorInfo ctor)
		{
			ParameterInfo[] parameterTypes = ctor.GetParameters();
			Object[] args = new Object[parameterTypes.Length];

			for(Int32 loop = 0; loop < parameterTypes.Length; loop++)
			{
				ParameterInfo parameter = parameterTypes[loop];
				Type parameterType = parameter.ParameterType;
				if(PluginUtils.InstanceOf(parameterType, this.Host.GetType()))
					args[loop] = this.Host;
				else
				{
					if(parameterType == typeof(IEnumerable<>))//TODO: It is necessary to postpone it for post-processing
						parameterType = parameterType.GetGenericArguments()[0];

					List<Object> instances = new List<Object>();
					foreach(var pluginDescription in this.FindPluginTypeI(parameterType))
						instances.Add(pluginDescription.Instance);

					switch(instances.Count)
					{
					case 1:
						args[loop] = instances[0];
						break;
					case 0:
						this.Trace.TraceInformation("Plugin {0}.ctor[{1}](...{2}...) unresolved reference", pluginType, ctor, parameter.ParameterType);
						return null;
					default:
						//TODO: Plugins with input arrays must be initialized after initialization but before loading.
						if(parameter.ParameterType == typeof(IEnumerable<>))
							args[loop] = instances.ToArray();
						break;
					}
				}
			}

			return (IPlugin)ctor.Invoke(args);
		}

		private void SafeInvoke(Delegate eventDelegate, params Object[] args)
		{
			if(eventDelegate == null)
				return;

			foreach(Delegate subscriber in eventDelegate.GetInvocationList())
				try
				{
					subscriber.DynamicInvoke(args);
				} catch(Exception exc)
				{
					this.Trace.TraceData(TraceEventType.Error, 1, exc);
				}
		}

		private static TraceSource CreateTraceSource(String name)
		{
			TraceSource result = new TraceSource(name);
			result.Switch.Level = SourceLevels.All;
			result.Listeners.Remove("Default");
			result.Listeners.AddRange(System.Diagnostics.Trace.Listeners);
			return result;
		}
	}
}