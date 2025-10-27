using System;

namespace SAL.Flatbed
{
	/// <summary>Mode how plugin is disconnected from host</summary>
	public enum DisconnectMode
	{
		/// <summary>The add-in was unloaded by unknown reason</summary>
		/// <remarks>If this event received it must be closed without any answers</remarks>
		Unknown = 0,
		/// <summary>The add-in was unloaded with OS</summary>
		HostShutdown = 1,
		/// <summary>The add-in was unloaded when Flatbed is closed by user</summary>
		FlatbedClosed = 2,
		/// <summary>The add-in was unloaded by a user</summary>
		UserClosed = 3,
	}
}