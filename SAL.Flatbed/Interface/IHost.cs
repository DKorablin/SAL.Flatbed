using System;

namespace SAL.Flatbed
{
	/// <summary>Basic host interface that resolved in any host</summary>
	public interface IHost : IHostItem
	{
		#region Properties
		/// <summary>Array of loaded plugins</summary>
		IPluginStorage Plugins { get; }

		#endregion Properties
		#region Methods
		/// <summary>Unload all loaded plugins by invoking <see cref="IPlugin.OnDisconnection"/> method</summary>
		/// <param name="reason">Reason why plugins are unloaded</param>
		void UnloadPlugins(DisconnectMode reason);

		/// <summary>Add session variable</summary>
		/// <param name="name">The key of the object to be added to the session</param>
		/// <param name="value">The value of the object added to the session</param>
		void SetSessionValue(String name, Object value);

		/// <summary>Get object from session</summary>
		/// <param name="name">The key of an object from session</param>
		/// <returns>Values from session by the key</returns>
		Object GetSessionValue(String name);
		#endregion Methods
	}
}