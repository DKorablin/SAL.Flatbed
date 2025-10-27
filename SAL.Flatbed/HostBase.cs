using System;
using System.Reflection;

namespace SAL.Flatbed
{
	/// <summary>Basic abstract host class that is used in all host providers</summary>
	public abstract class HostBase : IHost, IDisposable
	{
		private PluginStorage _storage;

		/// <summary>An object that is encapsulated by an interface</summary>
		public abstract Object Object { get; }

		/// <summary>Array of loaded plugins</summary>
		public virtual IPluginStorage Plugins
		{
			get
			{
				if(this._storage == null)
				{
					this._storage = new PluginStorage(this);
					AppDomain.CurrentDomain.AssemblyResolve += this.CurrentDomain_AssemblyResolve;
				}
				return this._storage;
			}
		}

		/// <summary>Unload all loaded plugins</summary>
		/// <param name="reason">Reason why plugins are unloaded</param>
		public abstract void UnloadPlugins(DisconnectMode reason);

		/// <summary>Get object from session by key</summary>
		/// <param name="name">Object key in session</param>
		/// <returns>Value from session by the key or null</returns>
		public abstract Object GetSessionValue(String name);

		/// <summary>Add object to session</summary>
		/// <param name="name">Object key in session</param>
		/// <param name="value">Value to add to the session by the key</param>
		public abstract void SetSessionValue(String name, Object value);

		/// <summary>Resolve assembly from loaded plugins</summary>
		/// <param name="sender">Sender assembly</param>
		/// <param name="args">Arguments with assembly identity data</param>
		/// <returns>Resolved assembly from loaded plugins</returns>
		public virtual Assembly CurrentDomain_AssemblyResolve(Object sender, ResolveEventArgs args)
		{
			foreach(IPluginDescription plugin in this.Plugins)
				if(plugin.Instance != null)
				{
					Assembly asm = plugin.Instance.GetType().Assembly;
					if(asm.FullName == args.Name)
						return asm;
				}

			return this.Plugins.PluginProvider?.ResolveAssembly(args.Name);
		}

		/// <summary>Dispose host and unload all loaded plugins</summary>
		public void Dispose()
		{
			this.UnloadPlugins(DisconnectMode.HostShutdown);
			foreach(IPluginDescription settings in this.Plugins.FindPluginType<ISettingsPluginProvider>())
				settings.Instance.OnDisconnection(DisconnectMode.FlatbedClosed);

			IPluginProvider plugins = this.Plugins.PluginProvider;
			while(plugins != null)
			{
				plugins.OnDisconnection(DisconnectMode.FlatbedClosed);
				plugins = plugins.ParentProvider;
			}

			AppDomain.CurrentDomain.AssemblyResolve -= new ResolveEventHandler(this.CurrentDomain_AssemblyResolve);

			GC.SuppressFinalize(this);
		}
	}
}