
namespace SAL.Flatbed
{
	/// <summary>Plugin interface that implements loading/saving of plugin settings</summary>
	public interface ISettingsPluginProvider : IPlugin
	{
		/// <summary>Get plugin settings instance based on base plugin interface</summary>
		/// <param name="plugin">Plugin interface for which to create a plugin settings interface</param>
		/// <returns>Plugin settings interface</returns>
		ISettingsProvider GetSettingsProvider(IPlugin plugin);
	}
}