using System;

namespace SAL.Flatbed
{
	/// <summary>Base interface for all plugins</summary>
	public interface IPlugin
	{
		/// <summary>Plugin attached to host</summary>
		/// <param name="mode">How plugin was attached to host</param>
		/// <returns>
		/// Successful connection of the module to the system.
		/// Otherwise, the process of disconnecting the module from the system will immediately be called
		/// </returns>
		Boolean OnConnection(ConnectMode mode);

		/// <summary>Invoked when module will be disconnected</summary>
		/// <param name="mode">Mode how plugin is disconnected</param>
		/// <returns>Plugin is disconnected succesfully</returns>
		Boolean OnDisconnection(DisconnectMode mode);
	}
}