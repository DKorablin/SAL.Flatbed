using System;
using System.Collections.Generic;
using System.Reflection;

namespace SAL.Flatbed
{
	/// <summary>Plugin property information</summary>
	public class PluginPropertyInfo : PluginTypeInfo, IPluginPropertyInfo
	{
		private PropertyInfo Property { get; }

		/// <summary>Property name</summary>
		public override String Name { get => this.Property.Name; }

		/// <summary>String naming of element type</summary>
		public override String TypeName { get => this.Property.PropertyType.ToString(); }

		/// <summary>Element type</summary>
		public override MemberTypes MemberType { get => this.Property.MemberType; }

		/// <summary>Count of input parameters</summary>
		public Int32 Count { get => this.Property.GetIndexParameters().Length; }

		/// <summary>This property can be read</summary>
		public Boolean CanRead { get => this.Property.CanRead; }

		/// <summary>This property can be write</summary>
		public Boolean CanWrite { get => this.Property.CanWrite; }

		/// <summary>Create instance of property description</summary>
		/// <param name="property">Property reflection</param>
		/// <param name="target">Target where property is declared</param>
		/// <param name="parent">The parent object that contains the reference to the property</param>
		/// <exception cref="ArgumentNullException">Property is required</exception>
		public PluginPropertyInfo(PropertyInfo property, Object target, PluginMemberInfo parent)
			: base(property.PropertyType, target, parent)
			=> this.Property = property ?? throw new ArgumentNullException(nameof(property));

		/// <summary>Get list of all parameters if property contains index parameters</summary>
		/// <returns>List of all index parameters</returns>
		public IEnumerable<IPluginParameterInfo> GetParameters()
		{
			foreach(ParameterInfo parameter in this.Property.GetIndexParameters())
				yield return new PluginParameterInfo(this, parameter);
		}

		/// <summary>Get property value</summary>
		/// <param name="parameters">Array of input parameters</param>
		/// <returns>Property value</returns>
		public Object Get(params Object[] parameters)
		{
			PropertyInfo info = this.Property;
			Object target = base.GetTarget()
				?? throw new ArgumentNullException(nameof(target), $"Container for property {this.Name} is null");

			return info.GetValue(target, parameters);//Don't forget about CanRead
		}

		/// <summary>Set property value</summary>
		/// <param name="value">Value to write</param>
		/// <param name="parameters">Array on input indexes</param>
		public void Set(Object value, params Object[] parameters)
		{
			PropertyInfo info = this.Property;
			Object target = base.GetTarget()
				?? throw new ArgumentNullException(nameof(target), $"Container for property {this.Name} is null");

			info.SetValue(target, value, null);//Don't forget about CanWrite
		}

		/*/// <summary>Получить публичный элемент плагина по наименованию свойства</summary>
		/// <typeparam name="T">Какого типа должен быть элемент</typeparam>
		/// <param name="name">Наименование элемента в плагине</param>
		/// <returns>Информация о публичном элементе плагина</returns>
		public T GetMember<T>(String name) where T : IPluginMemberInfo
		{
			if(String.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");

			foreach(IPluginMemberInfo member in this.GetMembers())
				if(String.Equals(name, member.Name, StringComparison.Ordinal) && member is T)
					return (T)member;

			return default(T);
		}

		/// <summary>Получить публичные методы объекта</summary>
		/// <returns>Информация по публичным объектам плагина</returns>
		public IEnumerable<IPluginMemberInfo> GetMembers()
		{
			PropertyInfo info = this._property;
			if(info.CanRead)
			{
				Object target = this.Get();

				foreach(MemberInfo member in info.GetType().GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
				{
					switch(member.MemberType)
					{
					case MemberTypes.Method:
						MethodBase method = (MethodBase)member;
						if(!method.IsSpecialName)
							yield return new PluginMethodInfo(base.Plugin, method, target);
						break;
					case MemberTypes.Property:
						yield return new PluginPropertyInfo(base.Plugin, (PropertyInfo)member, target);
						break;
					case MemberTypes.Event:
						yield return new PluginEventInfo(base.Plugin, (EventInfo)member, target);
						break;
					}
				}
			}
		}*/
	}
}