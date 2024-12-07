using System;
using System.Reflection;

namespace SAL.Flatbed
{
	/// <summary>Plugin event information</summary>
	public class PluginEventInfo : PluginMemberInfo, IPluginEventInfo
	{
		private new EventInfo Member { get => (EventInfo)base.Member; }

		/// <summary>Create instance of plugin event information</summary>
		/// <param name="evt">Event reflection</param>
		/// <param name="target">Object where event is declared</param>
		/// <param name="parent">Parent instance of an object</param>
		public PluginEventInfo(EventInfo evt, Object target, PluginMemberInfo parent)
			: base(evt, target, parent)
		{
		}

		/// <summary>Attach method reference to plugin event handler</summary>
		/// <param name="handler">Method reference that will be invoked when event is called</param>
		/// <exception cref="ArgumentNullException">Handler or target is null</exception>
		public void AddEventHandler(EventHandler<DataEventArgs> handler)
		{
			_ = handler ?? throw new ArgumentNullException(nameof(handler));

			Object target = base.GetTarget()
				?? throw new ArgumentNullException(nameof(target), $"Container for event {this.Name} is null");

			this.Member.AddEventHandler(target, handler);
		}

		/// <summary>Detach method reference from plugin event handler</summary>
		/// <param name="handler">Method reference to detach from plugin event</param>
		/// <exception cref="ArgumentNullException">handler or target is null</exception>
		public void RemoveEventHandler(EventHandler<DataEventArgs> handler)
		{
			_ = handler ?? throw new ArgumentNullException(nameof(handler));

			Object target = base.GetTarget()
				?? throw new ArgumentNullException(nameof(target), $"Container for event {this.Name} is null");

			this.Member.RemoveEventHandler(target, handler);
		}
	}
}