using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace SAL.Flatbed
{
	/// <summary>Plugin description</summary>
	/// <remarks>Holds instance of loaded plugin as well as the path to the plugin loading location</remarks>
	[DebuggerDisplay("ID={ID} Name={Name}")]
	public class PluginDescription : IPluginDescription
	{
		private IPluginTypeInfo _type;

		/// <summary>Plugin identifier</summary>
		/// <exception cref="ArgumentNullException">GuidAttribute not declared on assembly level</exception>
		public String ID
		{
			get
			{
				PluginEntryPointAttribute pluginAttribute = this.GetPluginAttribute<PluginEntryPointAttribute>();
				if(pluginAttribute != null)
					return pluginAttribute.ID;
				else
				{
					GuidAttribute guid = this.GetAssemblyAttribute<GuidAttribute>();
					if(guid == null)
						throw new ArgumentNullException("GuidAttribute not specified in assembly " + this.Instance.GetType().Assembly.FullName);

					return guid.Value;
				}
			}
		}

		/// <summary>Path where plugin was loaded</summary>
		public String Source { get; }

		/// <summary>Instance of loaded plugin</summary>
		public IPlugin Instance { get; }

		/// <summary>Plugin name</summary>
		public String Name
		{
			get
			{
				PluginEntryPointAttribute pluginAttribute = this.GetPluginAttribute<PluginEntryPointAttribute>();
				return pluginAttribute == null
					? this.Assembly.GetName().Name
					: pluginAttribute.Name;
			}
		}

		/// <summary>Plugin version</summary>
		public Version Version
		{
			get
			{
				PluginEntryPointAttribute pluginAttribute = this.GetPluginAttribute<PluginEntryPointAttribute>();
				return pluginAttribute == null
					? this.Assembly.GetName().Version
					: pluginAttribute.Version;
			}
		}

		/// <summary>Plugin description</summary>
		public String Description
		{
			get
			{
				PluginEntryPointAttribute pluginAttribute = this.GetPluginAttribute<PluginEntryPointAttribute>();
				if(pluginAttribute != null)
					return pluginAttribute.Description;
				else
				{
					AssemblyDescriptionAttribute attribute = this.GetAssemblyAttribute<AssemblyDescriptionAttribute>();
					return attribute == null ? String.Empty : attribute.Description;
				}
			}
		}

		/// <summary>Company which create plugin</summary>
		public String Company
		{
			get
			{
				AssemblyCompanyAttribute attibute = this.GetAssemblyAttribute<AssemblyCompanyAttribute>();
				return attibute?.Company;
			}
		}

		/// <summary>Plugin copyright information</summary>
		public String Copyright
		{
			get
			{
				AssemblyCopyrightAttribute attribute = this.GetAssemblyAttribute<AssemblyCopyrightAttribute>();
				return attribute?.Copyright;
			}
		}

		/// <summary>Get all avalilable types to call from outside</summary>
		public IPluginTypeInfo Type
		{
			get => this._type ?? (this._type = new PluginTypeInfo(this.Instance.GetType(), this.Instance, null));
		}

		/// <summary>Assembly where plugin is hosted</summary>
		private Assembly Assembly { get => this.Instance.GetType().Assembly; }

		/// <summary>Create instance of plugin description</summary>
		/// <param name="instance">Interface for accessing plugin methods</param>
		/// <param name="source">Plugin source</param>
		public PluginDescription(IPlugin instance, String source)
		{
			if(String.IsNullOrEmpty(source))
				throw new ArgumentNullException(nameof(source));

			this.Instance = instance ?? throw new ArgumentNullException(nameof(instance));
			this.Source = source;
		}

		/// <summary>Get plugin instance attribute</summary>
		/// <typeparam name="A">Attribute type to get</typeparam>
		/// <returns>First found attribute or null</returns>
		protected A GetPluginAttribute<A>() where A : Attribute
		{
			Object[] attributes = this.Instance.GetType().GetCustomAttributes(typeof(A), false);
			return attributes.Length == 0 ? null : (A)attributes[0];
		}

		/// <summary>Get assembly instance attribute</summary>
		/// <typeparam name="A">Attribute type to get</typeparam>
		/// <returns>Forst found attribute or null</returns>
		protected A GetAssemblyAttribute<A>() where A : Attribute
		{
			Object[] attributes = this.Assembly.GetCustomAttributes(typeof(A), false);
			return attributes.Length == 0 ? null : (A)attributes[0];
		}
	}
}