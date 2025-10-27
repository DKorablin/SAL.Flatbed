using System;

namespace SAL.Flatbed
{
	/// <summary>Useful utilities to work with plugins</summary>
	public struct PluginUtils
	{
		/// <summary>Type checking for compliance with the minimum requirements of the plugin interface</summary>
		/// <param name="pluginType">Class type resolves plugin interface</param>
		/// <exception cref="ArgumentNullException">pluginType is null</exception>
		/// <returns>The type meets minimal requirements for the plugin implementation</returns>
		public static Boolean IsPluginType(Type pluginType)
		{
			if(pluginType == null)
				throw new ArgumentNullException(nameof(pluginType));

			if(!pluginType.IsPublic || !pluginType.IsClass)//pluginType.IsAbstract
				return false;

			//if(pluginType.IsAssignableFrom(typeof(IPlugin)) && pluginType.IsClass)//To eliminate all interface inheritance
			Type typeInterface = pluginType.GetInterface(PluginConstant.PluginInterface, false);
			return typeInterface != null;
		}

		/// <summary>Checking the inheritance of one type from another</summary>
		/// <param name="sourceType">Source type which need to be implemented in <paramref name="targetType"/></param>
		/// <param name="targetType">Target type which need to be resolved in <paramref name="sourceType"/></param>
		/// <exception cref="ArgumentNullException">sourceType or targetType is null</exception>
		/// <returns><paramref name="targetType"/> implements the object required for <paramref name="sourceType"/></returns>
		public static Boolean InstanceOf(Type sourceType, Type targetType)
		{
			_ = sourceType ?? throw new ArgumentNullException(nameof(sourceType));
			_ = targetType ?? throw new ArgumentNullException(nameof(targetType));

			if(sourceType.IsInterface)
			{//Search all interfaces
				foreach(Type iface in targetType.GetInterfaces())
					if(iface.Equals(sourceType))
						return true;
			} else
			{//Search by parents
				if(sourceType.Equals(targetType))
					return true;

				if(sourceType.IsSubclassOf(targetType))
					return true;
			}

			return false;
		}
	}
}