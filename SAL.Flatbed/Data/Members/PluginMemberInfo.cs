using System;
using System.Diagnostics;
using System.Reflection;

namespace SAL.Flatbed
{
	/// <summary>Description of public plugin member</summary>
	[DebuggerDisplay("Name={Name} (Type={TypeName})")]
	public class PluginMemberInfo : IPluginMemberInfo
	{
		private TraceSource _trace;

		/// <summary>Member name</summary>
		public virtual String Name { get => this.Member.Name; }

		/// <summary>Full member name</summary>
		public virtual String TypeName
		{
			get => this.Member.ReflectedType == null
				? ((Type)this.Member).FullName
				: this.Member.ReflectedType.FullName;
		}

		/// <summary>Member type</summary>
		public virtual MemberTypes MemberType { get => this.Member.MemberType; }

		/// <summary>Reflected member information</summary>
		protected MemberInfo Member { get; }

		/// <summary>Reflected member type</summary>
		protected Type ReflectedType { get => this.Member as Type; }

		private PluginMemberInfo Parent { get; }

		/// <summary>Object instance where reference is stored</summary>
		private Object Target { get; }

		/// <summary>Trace instance</summary>
		protected internal TraceSource Trace
		{
			get => this._trace ?? (this._trace = PluginMemberInfo.CreateTraceSource(PluginConstant.TraceSourceName));
		}

		/// <summary>Create instance of plugin member information</summary>
		/// <param name="member">Member information reflection</param>
		/// <param name="target">Object where member declared</param>
		/// <param name="parent">Parent object instance information</param>
		public PluginMemberInfo(MemberInfo member, Object target, PluginMemberInfo parent)
		{
			if(target == null && parent == null)
				throw new ArgumentNullException(nameof(target) + " || " + nameof(parent), "target or parent is required for invocation process");

			this.Member = member ?? throw new ArgumentNullException(nameof(member));
			this.Target = target;
			this.Parent = parent;
		}

		/// <summary>Checking if an interface implementation exists in a plugin</summary>
		/// <typeparam name="T">The type of class or interface that is supposed to be implemented in the plugin</typeparam>
		/// <returns>Target implements this type</returns>
		public Boolean InstanceOf<T>()
			=> this.Target is T;

		/// <summary>Check for inheritance from specific type</summary>
		/// <param name="type">Check for inheritance of this type</param>
		/// <returns>Inherit from argument type</returns>
		public Boolean InstanceOf(Type type)
		{
			_ = type ?? throw new ArgumentNullException(nameof(type));

			if(this.Target == null)
				return false;

			Type thisType = this.ReflectedType;
			return thisType != null && PluginUtils.InstanceOf(type, thisType);
		}

		/// <summary>Get the object instance for reflection invocation</summary>
		/// <returns>Reference to instance</returns>
		protected virtual Object GetTarget()
		{
			if(this.Target != null)
				return this.Target;
			else if(this.Parent == null)
				throw new InvalidOperationException("Istance is not specified");

			switch(this.Parent.MemberType)
			{
			case MemberTypes.Property:
				IPluginPropertyInfo property = (IPluginPropertyInfo)this.Parent;
				return property.Get();//TODO: Add args length check
			case MemberTypes.Method:
				IPluginMethodInfo method = (IPluginMethodInfo)this.Parent;
				return method.Invoke();//TODO: Add args length check
			case MemberTypes.Event:
			case MemberTypes.Constructor:
			case MemberTypes.Custom:
				throw new InvalidOperationException($"Can't extract type instance from {this.Parent.MemberType}: {this.Parent.Name}");
			default:
				return this.Parent.GetTarget();
			}
		}

		private static TraceSource CreateTraceSource(String name)
		{
			TraceSource result = new TraceSource(name);
			result.Switch.Level = SourceLevels.All;
			result.Listeners.Remove("Default");
			result.Listeners.AddRange(System.Diagnostics.Trace.Listeners);
			return result;
		}
	}
}