using System;
using System.Collections.Generic;

namespace SAL.Flatbed
{
	/// <summary>The class of empty arguments passed to the client</summary>
	public class DataEmptyEventArgs : DataEventArgs
	{
		/// <summary>Zero data</summary>
		public override Int32 Count { get => 0; }

		/// <summary>Empty keys array</summary>
		public override IEnumerable<String> Keys { get { yield break; } }

		/// <summary>Get data from arguments</summary>
		/// <typeparam name="T">Type of required data</typeparam>
		/// <param name="key">Key by which to get data</param>
		/// <returns>Data by argument</returns>
		public override T GetData<T>(String key)
			=> default;

		/// <summary>The empty argument constructor is only available <see cref="DataEventArgs"/></summary>
		internal DataEmptyEventArgs() { }
	}
}