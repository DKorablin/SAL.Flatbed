using System;
using System.Collections.Generic;

namespace SAL.Flatbed
{
	/// <summary>Plugin property information</summary>
	public interface IPluginPropertyInfo : IPluginTypeInfo
	{
		/// <summary>Count of index parameters</summary>
		Int32 Count { get; }

		/// <summary>This property can be read</summary>
		Boolean CanRead { get; }

		/// <summary>Value can be set to this property</summary>
		Boolean CanWrite { get; }

		/// <summary>Get an array of all property index parameters</summary>
		/// <returns>Array of all index parameters</returns>
		IEnumerable<IPluginParameterInfo> GetParameters();

		/// <summary>Get plugin property value</summary>
		/// <param name="parameters">Array of all input index parameters</param>
		/// <returns>Property value</returns>
		Object Get(params Object[] parameters);

		/// <summary>Set property value</summary>
		/// <param name="value">Property value to set</param>
		/// <param name="parameters">Array of index parameters (If property contains indexed parameters)</param>
		void Set(Object value, params Object[] parameters);
	}
}