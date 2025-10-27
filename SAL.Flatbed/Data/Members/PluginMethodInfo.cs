using System;
using System.Collections.Generic;
using System.Reflection;

namespace SAL.Flatbed
{
	/// <summary>Description of public plugin method</summary>
	public class PluginMethodInfo : PluginMemberInfo, IPluginMethodInfo
	{
		private new MethodBase Member { get => (MethodBase)base.Member; }

		/// <summary>Method invocation result type</summary>
		public IPluginTypeInfo ReturnType
		{
			get
			{
				MethodInfo method = (MethodInfo)this.Member;
				return method.ReturnType == null || method.ReturnType == typeof(void)
					? null
					: new PluginTypeInfo(method.ReturnType, null, this);
			}
		}

		/// <summary>Create instance of plugin method description</summary>
		/// <param name="method">Method reflection</param>
		/// <param name="target">Target where method is declared</param>
		/// <param name="parent">Parent description where target is declared</param>
		public PluginMethodInfo(MethodBase method, Object target, PluginMemberInfo parent)
			: base(method, target, parent)
		{
		}

		/// <summary>Input arguments count</summary>
		public Int32 Count { get => this.Member.GetParameters().Length; }

		/// <summary>Get all arguments that method accept</summary>
		/// <returns>Array of all method arguments</returns>
		public IEnumerable<IPluginParameterInfo> GetParameters()
		{
			MethodBase method = this.Member;

			foreach(ParameterInfo parameter in method.GetParameters())
				yield return new PluginParameterInfo(this, parameter);
		}

		/// <summary>Invoke plugin method and get result</summary>
		/// <param name="parameters">Array of input parameters</param>
		/// <returns>Result of method invocation</returns>
		public Object Invoke(params Object[] parameters)
		{
			Object target = base.GetTarget();
			if(target == null)
				return null;//TODO: Added due to generics. Stack: PluginTypeInfo.GetGenericMembers -> PluginMethodInfo.Invoke
			else
			{
				try
				{
					return this.Member.Invoke(target, parameters);
				} catch(Exception exc)
				{
					exc.Data.Add("MethodName", base.Name);
					exc.Data.Add("TypeName", base.TypeName);
					throw;
				}
			}
		}
	}
}