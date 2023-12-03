using System;
using System.Reflection;

namespace SAL.Flatbed
{
	/// <summary>Description about the plugin member for call from outside</summary>
	public interface IPluginMemberInfo
	{
		/// <summary>Member name</summary>
		String Name { get; }

		/// <summary>Full member name</summary>
		String TypeName { get; }

		/// <summary>Member type</summary>
		MemberTypes MemberType { get; }

		/// <summary>Check that member type that it's inherits from specific interface</summary>
		/// <typeparam name="T">The type of class or interface that is supposed to be implemented in the plugin</typeparam>
		/// <returns>Member implements specified interface</returns>
		Boolean InstanceOf<T>();

		/// <summary>Check inheritance from specific type</summary>
		/// <param name="type">The type that will check</param>
		/// <returns>This member is inherit this type</returns>
		Boolean InstanceOf(Type type);
	}
}