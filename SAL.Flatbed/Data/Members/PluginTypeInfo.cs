using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace SAL.Flatbed
{
	/// <summary>Plugin type description</summary>
	public class PluginTypeInfo : PluginMemberInfo, IPluginTypeInfo
	{
		private IPluginMemberInfo[] _members;
		private IPluginTypeInfo[] _genericMemebers;

		/// <summary>Type is Value type</summary>
		public Boolean IsValueType
		{
			get
			{
				return base.ReflectedType == null
					? false
					: base.ReflectedType.IsValueType;
			}
		}

		/// <summary>Type is Array type</summary>
		public Boolean IsArray
		{
			get
			{
				if(base.ReflectedType == null)
					return false;
				return base.ReflectedType.IsArray;
			}
		}

		/// <summary>Type is Generic type</summary>
		public Boolean IsGeneric
		{
			get
			{
				return base.ReflectedType == null
					? false
					: base.ReflectedType.IsGenericType;
			}
		}

		/// <summary>Type is Enum type</summary>
		private Boolean IsEnum
		{
			get
			{
				return base.ReflectedType == null
					? false
					: base.ReflectedType.IsEnum;
			}
		}

		private Boolean IsNativeType
		{//TODO: Придумать алгоритм определения BCL сборок
			get
			{
				//return base.Member.Module.Assembly.GetName().Name == "mscorlib";
				return base.Member.Module.Assembly.GlobalAssemblyCache;
			}
		}

		/// <summary>Array of available members</summary>
		public IEnumerable<IPluginMemberInfo> Members
		{
			get
			{
				return this._members ?? (this._members = new List<IPluginMemberInfo>(this.GetMembers()).ToArray());
			}
		}

		/// <summary>Type Generic array</summary>
		public IEnumerable<IPluginTypeInfo> GenericMembers
		{
			get
			{
				return this._genericMemebers ?? (this._genericMemebers = new List<IPluginTypeInfo>(this.GetGenericMembers()).ToArray());
			}
		}

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
		{
			return this.IsEnum
				? Enum.GetNames(base.ReflectedType)
				: new String[] { };
		}

		/// <summary>Get public member by specifying it's name</summary>
		/// <param name="name">Name of required member in plugin</param>
		/// /// <exception cref="ArgumentNullException">Name not specified</exception>
		/// <returns>Description of found plugin member of default(T)</returns>
		public T GetMember<T>(String name) where T : IPluginMemberInfo
		{
			if(String.IsNullOrEmpty(name))
				throw new ArgumentNullException((name));

			foreach(IPluginMemberInfo member in this.Members)
				if(String.Equals(name, member.Name, StringComparison.Ordinal) && member is T)
					return (T)member;

			return default(T);
		}

		private IEnumerable<IPluginTypeInfo> GetGenericMembers()
		{
			if(this.IsGeneric)
			{
				Type type = base.ReflectedType;

				foreach(Type member in type.GetGenericArguments())
					yield return new PluginTypeInfo(member, null, this);//TODO: Добавлен родитель инстанса
			}
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