using System;
#pragma warning disable 0108

namespace SAL.Flatbed
{
	/// <summary>Interface for plugin settings or child plugin object (if required)</summary>
	public interface IPluginSettings
	{
		/// <summary>Object that store all settins</summary>
		/// <remarks>Property is hardcoded in constants, because access goes through reflection to the generic</remarks>
		Object Settings { get; }
	}

	/// <summary>Strongly typed settings interface or child plugin object</summary>
	/// <typeparam name="T">Settings object strongly type</typeparam>
	public interface IPluginSettings<T> : IPluginSettings
	{
		/// <summary>Strongly typed settings object</summary>
		T Settings { get; }
	}
}