using System;

namespace SAL.Flatbed
{
	/// <summary>Base host interface that is used in all public objects in hosts</summary>
	public interface IHostItem
	{
		/// <summary>An object that is encapsulated by an interface</summary>
		Object Object { get; }
	}
}