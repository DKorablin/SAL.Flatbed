using System;

namespace SAL.Flatbed
{
	/// <summary>Description of unresolverd plugin</summary>
	/// <remarks>If pluigin .ctor reference are not resolved, then plugin will be skipped</remarks>
	internal class UnresolvedPlugin
	{
		/// <summary>Type of plugin</summary>
		public Type PluginType { get; }

		/// <summary>Plugin loading source</summary>
		public String Source { get; }

		/// <summary>Mode as plugin was loaded</summary>
		public ConnectMode Mode { get; }

		/// <summary>Create instance of unresolved plugin description with basic information</summary>
		/// <param name="pluginType">Type of plugin</param>
		/// <param name="source">Source where plugin is located</param>
		/// <param name="mode">How plugin was loaded</param>
		public UnresolvedPlugin(Type pluginType, String source, ConnectMode mode)
		{
			this.PluginType = pluginType;
			this.Source = source;
			this.Mode = mode;
		}
	}
}