using System;
using System.Runtime.InteropServices;

namespace SAL.Flatbed
{
	/// <summary>Description attribute of the entry point for the plugin class, if the plugin has multiple plugin implementation classes</summary>
	/// <see cref="IPlugin"/>
	[ComVisible(true)]
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public sealed class PluginEntryPointAttribute : Attribute
	{
		/// <summary>Plugin class identifier</summary>
		public String ID { get; }

		/// <summary>Plugin class version</summary>
		public Version Version { get; }

		/// <summary>Plugin class name</summary>
		public String Name { get; }

		/// <summary>Plugin class description</summary>
		public String Description { get; }

		/// <summary>Create instance of plugin class arrtibute suppying additional description</summary>
		/// <param name="id">Identifier</param>
		/// <param name="version">Version</param>
		/// <param name="name">Plugin class friendly name</param>
		/// <param name="description">Plugin class description</param>
		/// <exception cref="ArgumentNullException">id required</exception>
		public PluginEntryPointAttribute(String id, String version, String name, String description)
		{
			if(String.IsNullOrEmpty(id))
				throw new ArgumentNullException(nameof(id));

			this.ID = id;
			this.Version = new Version(version);
			this.Name = name;
			this.Description = description;
		}
	}
}