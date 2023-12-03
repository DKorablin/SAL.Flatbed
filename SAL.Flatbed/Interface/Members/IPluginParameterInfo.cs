using System;

namespace SAL.Flatbed
{
	/// <summary>Description of method argument</summary>
	public interface IPluginParameterInfo : IPluginTypeInfo
	{
		/// <summary>Plugin method information to which this argument belongs</summary>
		IPluginMemberInfo Method { get; }

		/// <summary>This argument is out</summary>
		Boolean IsOut { get; }
	}
}