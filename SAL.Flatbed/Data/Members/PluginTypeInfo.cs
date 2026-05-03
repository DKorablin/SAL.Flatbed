using System;
using System.Collections.Generic;
using System.Reflection;

namespace SAL.Flatbed
{
	/// <summary>Plugin type description</summary>
	public class PluginTypeInfo : PluginMemberInfo, IPluginTypeInfo
	{
		private IPluginMemberInfo[] _members;
		private IPluginTypeInfo[] _genericMembers;

		/// <summary>Type is Value type</summary>
		public Boolean IsValueType
			=> base.ReflectedType != null && base.ReflectedType.IsValueType;

		/// <summary>Type is Array type</summary>
		public Boolean IsArray
			=> base.ReflectedType != null && base.ReflectedType.IsArray;

		/// <summary>Type is Generic type</summary>
		public Boolean IsGeneric
			=> base.ReflectedType != null && base.ReflectedType.IsGenericType;

		/// <summary>Type is Enum type</summary>
		private Boolean IsEnum
			=> base.ReflectedType != null && base.ReflectedType.IsEnum;

		private Boolean IsNativeType
		{
			get
			{
				var assembly = base.Member.Module.Assembly;
				// GAC check for .NET Framework (Backward compatibility)
				if(assembly.GlobalAssemblyCache)
					return true;

				// Token check for assemblies that are part of .NET Framework, .NET Core and .NET Standard
				var publicKeyToken = assembly.GetName().GetPublicKeyToken();
				if(publicKeyToken != null && publicKeyToken.Length > 0)
				{
					var token = BitConverter.ToString(publicKeyToken).Replace("-", "").ToLower();
					if(token == "b77a5c561934e089" || token == "b03f5f7f11d50a3a" ||
						token == "7cec85d7bea7798e" || token == "cc7b13ffcd2ddd51")
						return true;
				}

				return false;
			}
		}

		/// <summary>Array of available members</summary>
		public IEnumerable<IPluginMemberInfo> Members
			=> this._members ?? (this._members = new List<IPluginMemberInfo>(this.GetMembers()).ToArray());

		/// <summary>Type Generic array</summary>
		public IEnumerable<IPluginTypeInfo> UnderlyingMembers
			=> this._genericMembers ?? (this._genericMembers = new List<IPluginTypeInfo>(this.GetUnderlyingMembers()).ToArray());

		/// <summary>Create plugin type description which describes base types</summary>
		/// <param name="pluginType">Reflected plugin type</param>
		/// <param name="target">Target where type is declared</param>
		/// <param name="parent">Parent object description where type is declared</param>
		public PluginTypeInfo(MemberInfo pluginType, Object target, PluginMemberInfo parent)
			: base(pluginType, target, parent)
		{
		}

		/// <summary>Get default element value (Enum or default value for the parameter entering the method)</summary>
		/// <returns>Array of default values</returns>
		public virtual String[] GetDefaultValues()
			=> this.IsEnum
				? Enum.GetNames(base.ReflectedType)
				: new String[] { };

		/// <summary>Get public member by specifying it's name</summary>
		/// <param name="name">Name of required member in plugin</param>
		/// /// <exception cref="ArgumentNullException">Name not specified</exception>
		/// <returns>Description of found plugin member of default(T)</returns>
		public T GetMember<T>(String name) where T : IPluginMemberInfo
		{
			if(String.IsNullOrEmpty(name))
				throw new ArgumentNullException((name));

			foreach(IPluginMemberInfo member in this.Members)
				if(String.Equals(name, member.Name, StringComparison.Ordinal) && member is T t)
					return t;

			return default;
		}

		private IEnumerable<IPluginTypeInfo> GetUnderlyingMembers()
		{
			Type type = base.ReflectedType;

			if(this.IsGeneric)
				foreach(Type member in type.GetGenericArguments())
					yield return new PluginTypeInfo(member, null, this);//TODO: Added instance parent

			if(this.IsArray)
				yield return new PluginTypeInfo(type.GetElementType(), null, this);
		}

		private IEnumerable<IPluginMemberInfo> GetMembers()
		{
			Type type = base.ReflectedType;
			if(type == null)
				yield break;
			if(this.IsNativeType)
				yield break;

			foreach(MemberInfo member in type.GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
			{
				switch(member.MemberType)
				{
				case MemberTypes.Method:
					MethodBase method = (MethodBase)member;
					if(!method.IsSpecialName)
						yield return new PluginMethodInfo(method, null, this);
					break;
				case MemberTypes.Property:
					yield return new PluginPropertyInfo((PropertyInfo)member, null, this);
					break;
				case MemberTypes.Event:
					yield return new PluginEventInfo((EventInfo)member, null, this);
					break;
				}
			}
		}
	}
}