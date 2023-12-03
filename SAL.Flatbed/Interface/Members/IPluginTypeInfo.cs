using System;
using System.Collections.Generic;

namespace SAL.Flatbed
{
	/// <summary>Plugin type description</summary>
	public interface IPluginTypeInfo : IPluginMemberInfo
	{
		/// <summary>This is a value type</summary>
		Boolean IsValueType { get; }

		/// <summary>This is an array parameter</summary>
		Boolean IsArray { get; }

		/// <summary>This is a generic parameter</summary>
		Boolean IsGeneric { get; }

		/// <summary>Array of public properties, methods and events</summary>
		IEnumerable<IPluginMemberInfo> Members { get; }

		/// <summary>Array of generic parameters</summary>
		IEnumerable<IPluginTypeInfo> GenericMembers { get; }

		/// <summary>Get default values (Enum or default value for the parameter included in the method)</summary>
		/// <returns>Array of default values</returns>
		String[] GetDefaultValues();

		/// <summary>Get plugin public element by method name</summary>
		/// <param name="name">The name of the element in the plugin</param>
		/// <returns>Plugin public member information</returns>
		T GetMember<T>(String name) where T : IPluginMemberInfo;
	}
}