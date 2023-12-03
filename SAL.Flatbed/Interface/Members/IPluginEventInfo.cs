using System;

namespace SAL.Flatbed
{
	/// <summary>Plugin event description</summary>
	public interface IPluginEventInfo : IPluginMemberInfo
	{
		/// <summary>Attach callback method to plugin event</summary>
		/// <param name="handler">Method who will process plugin event</param>
		void AddEventHandler(EventHandler<DataEventArgs> handler);

		/// <summary>Detach attached callback method from plugin event</summary>
		/// <param name="handler">Method who will be detached from plugin event</param>
		void RemoveEventHandler(EventHandler<DataEventArgs> handler);
	}
}