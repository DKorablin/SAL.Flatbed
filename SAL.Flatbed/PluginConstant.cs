using System;

namespace SAL.Flatbed
{
	/// <summary>Constants related to the core of the plugin</summary>
	public struct PluginConstant
	{
		/// <summary>Path to the plugin's base interface</summary>
		public static String PluginInterface = typeof(IPlugin).FullName;

		/// <summary>Default trace source name for basic SAL assembly</summary>
		public const String TraceSourceName = "SAL.Core";
	}
}