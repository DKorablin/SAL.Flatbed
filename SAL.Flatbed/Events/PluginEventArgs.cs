using System;

namespace SAL.Flatbed
{
	/// <summary>Arguments for plugin-related events</summary>
	public class PluginEventArgs : EventArgs
	{
		private readonly IPluginDescription _plugin;

		/// <summary>Plugin interface on which a specific event occurs</summary>
		public IPluginDescription Plugin { get { return this._plugin; } }

		/// <summary>Creating event arguments above the plugin</summary>
		/// <param name="plugin">Plugin with which a certain event occurs</param>
		public PluginEventArgs(IPluginDescription plugin)
		{
			this._plugin = plugin ?? throw new ArgumentNullException(nameof(plugin));
		}
	}
}