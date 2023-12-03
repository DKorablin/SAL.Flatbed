using System;
using System.Collections.Generic;
using System.Reflection;

namespace SAL.Flatbed
{
	/// <summary>Plugin storage interface which is responsible fo storing, loading and invoking plugin members</summary>
	public interface IPluginStorage : IEnumerable<IPluginDescription>
	{
		#region Properties
		/// <summary>Get plugin by identifier</summary>
		/// <param name="pluginId">Plugin identifier</param>
		/// <returns>Plugin interface that was fount by plugin ID</returns>
		IPluginDescription this[String pluginId] { get; }

		/// <summary>Count of all loaded plugins</summary>
		Int64 Count{get;}

		/// <summary>The last plugin provider that was loaded from plugin source</summary>
		IPluginProvider PluginProvider { get; }
		#endregion Properties
		#region Events
		/// <summary>Event is called when all plugins was loaded</summary>
		event EventHandler PluginsLoaded;

		/// <summary>Event is called when plugin is unloaded</summary>
		event EventHandler<PluginEventArgs> PluginUnloaded;
		#endregion Events
		#region Methods

		/// <summary>Get plugin settings by base plugin interface</summary>
		/// <param name="plugin">Base plugin interface which required to get all plugin settings</param>
		/// <returns>Settings provider interface</returns>
		ISettingsProvider Settings(IPlugin plugin);

		/// <summary>Remove all plugins from collection unloading the from host</summary>
		void RemovePlugins();

		/// <summary>Add plugin to collection</summary>
		/// <param name="assembly">Loaded assembly</param>
		/// <param name="source">Source where plugin was loaded</param>
		/// <param name="mode">How plugin was loaded</param>
		void LoadPlugin(Assembly assembly, String source, ConnectMode mode);

		/// <summary>Add plugin to collection from assembly, using exact type</summary>
		/// <param name="assembly">Assemby that hosts plugin interface</param>
		/// <param name="type">Type that resolves interface <see cref="IPlugin"/></param>
		/// <param name="source">Source where plugin was loaded</param>
		/// <param name="mode">How plugin was loaded</param>
		void LoadPlugin(Assembly assembly, String type, String source, ConnectMode mode);

		/// <summary>Add plugin to collection</summary>
		/// <param name="plugin">Interface of base plugin description</param>
		/// <param name="mode">How plugin was loaded</param>
		/// <exception cref="ArgumentException">Plugin with such identifier already loaded</exception>
		void LoadPlugin(IPluginDescription plugin, ConnectMode mode);

		/// <summary>Unload plugin from host and remove from collection</summary>
		/// <param name="plugin">Base plugin interface that need to be unloaded from collection</param>
		Boolean UnloadPlugin(IPluginDescription plugin);

		/// <summary>Initialize all plugins after loadeing</summary>
		void InitializePlugins();

		/// <summary>Fond all plugins with specific typed</summary>
		/// <typeparam name="T">Required plugin type to find in plugins collection</typeparam>
		/// <returns>Array of found plugins</returns>
		IEnumerable<IPluginDescription> FindPluginType<T>() where T : IPlugin;

		/// <summary>Send message to plugin</summary>
		/// <param name="pluginId">Plugin identifier</param>
		/// <param name="message">message wich transfer to plugin</param>
		/// <param name="args">Arguments transferred with message</param>
		/// <returns>Result when plugin recieve and process this message</returns>
		Object SendMessage(String pluginId, String message, params Object[] args);

		/// <summary>Set settings provider</summary>
		/// <param name="plugin">Base plugin interface that resolves base interface as settings provider</param>
		void SetSetingsProvider(IPluginDescription plugin);

		/// <summary>Set plugins provider</summary>
		/// <param name="plugin">Plugin that is installed as a new plugin provider</param>
		void SetPluginProvider(IPluginDescription plugin);
		#endregion Methods
	}
}