using System;
using System.Reflection;

namespace SAL.Flatbed
{
	/// <summary>Plugin loader provider interface</summary>
	public interface IPluginProvider : IPlugin
	{
		/// <summary>Parent provider who is responsible for loading current plugin</summary>
		/// <remarks>When new provider found it will set as primary provider and current provider which loads new provider will be set as parent provider</remarks>
		IPluginProvider ParentProvider { get; set; }

		/// <summary>Load plugins from storage</summary>
		void LoadPlugins();

		/// <summary>Resolve assembly for plugin from current plugin provider</summary>
		/// <param name="assemblyName">Assembly name to load</param>
		/// <returns>Found assembly or null if assembly not found</returns>
		Assembly ResolveAssembly(String assemblyName);
	}
}