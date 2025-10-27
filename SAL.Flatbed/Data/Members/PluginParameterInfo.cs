using System;
using System.Collections.Generic;
using System.Reflection;

namespace SAL.Flatbed
{
	/// <summary>Plugin method argument information</summary>
	public class PluginParameterInfo : PluginTypeInfo, IPluginParameterInfo
	{
		private ParameterInfo Parameter { get; }

		/// <summary>Information about plugin method which argument belongs</summary>
		public IPluginMemberInfo Method { get; }

		/// <summary>Argument marked as output parameter</summary>
		public Boolean IsOut { get => this.Parameter.IsOut; }

		/// <summary>Argument name</summary>
		public override String Name { get => this.Parameter.Name; }

		/// <summary>Argument type</summary>
		public override MemberTypes MemberType { get => MemberTypes.TypeInfo; }

		/// <summary>Create instance of method argument description</summary>
		/// <param name="member">Method or property to which the argument or belongs</param>
		/// <param name="parameter">Argument reflection</param>
		public PluginParameterInfo(PluginMemberInfo member, ParameterInfo parameter)
			: base(parameter.ParameterType, null, member)
		{
			this.Method = member ?? throw new ArgumentNullException(nameof(member));
			this.Parameter = parameter ?? throw new ArgumentNullException(nameof(parameter));
		}

		/// <summary>In conjunction with default values from the base type, return the default value of the method parameter</summary>
		/// <returns>Array of default values</returns>
		public override String[] GetDefaultValues()
		{
			List<String> result = new List<String>();
			if(this.Parameter.DefaultValue != null && this.Parameter.DefaultValue != DBNull.Value)
				result.Add(this.Parameter.DefaultValue.ToString());

			result.AddRange(base.GetDefaultValues());

			return result.ToArray();
		}
	}
}