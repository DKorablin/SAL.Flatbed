using System;

namespace SAL.Flatbed
{
	/// <summary>Base plugin description interface which provides extended information about the plugin</summary>
	public interface IPluginDescription
	{
		/// <summary>Plugin unique identifier</summary>
		String ID { get; }

		/// <summary>Source where plugin was loaded</summary>
		String Source { get; }

		/// <summary>Created plugin instance</summary>
		IPlugin Instance { get; }

		/// <summary>Get all available plugin members to call from outside</summary>
		IPluginTypeInfo Type { get; }

		/// <summary>Plugin name</summary>
		String Name { get; }

		/// <summary>Plugin version</summary>
		Version Version { get; }

		/// <summary>Plugin description</summary>
		String Description { get; }

		/// <summary>Company that created the plugin</summary>
		String Company { get; }

		/// <summary>Plugin copyright</summary>
		String Copyright { get; }
	}
}