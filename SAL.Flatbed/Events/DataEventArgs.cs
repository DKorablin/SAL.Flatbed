using System;
using System.Collections.Generic;

namespace SAL.Flatbed
{
	/// <summary>Arguments passed through the event</summary>
	public abstract class DataEventArgs : EventArgs
	{
		/// <summary>Empty arguments</summary>
		public static new readonly DataEventArgs Empty = new DataEmptyEventArgs();

		/// <summary>Version</summary>
		public virtual Int32 Version { get => 0; }

		/// <summary>Arguments count</summary>
		public abstract Int32 Count { get; }

		/// <summary>Get all keys</summary>
		/// <returns>Array of all keys</returns>
		public abstract IEnumerable<String> Keys { get; }

		/// <summary>Get data from event</summary>
		/// <typeparam name="T">Type of expected data</typeparam>
		/// <param name="key">Key data</param>
		/// <returns>Value</returns>
		public abstract T GetData<T>(String key);
	}
}