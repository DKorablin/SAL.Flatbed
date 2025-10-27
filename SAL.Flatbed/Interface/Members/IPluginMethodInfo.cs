using System;
using System.Collections.Generic;

namespace SAL.Flatbed
{
	/// <summary>Description of plugin method</summary>
	public interface IPluginMethodInfo : IPluginMemberInfo
	{
		/// <summary>The count of arguments required to invoke the method.</summary>
		Int32 Count { get; }

		/// <summary>Return type description</summary>
		IPluginTypeInfo ReturnType { get; }

		/// <summary>Get all method arguments</summary>
		/// <returns>List of all input arguments</returns>
		IEnumerable<IPluginParameterInfo> GetParameters();

		/// <summary>Invoke plugin method and transfer array of input arguments</summary>
		/// <param name="parameters">Array input arguments</param>
		/// <returns>Invocation result</returns>
		Object Invoke(params Object[] parameters);
	}
}