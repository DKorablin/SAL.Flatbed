using System;
using System.Reflection;

namespace SAL.Flatbed
{
	/// <summary>Usefull utilities to work with plugins</summary>
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

			//if(pluginType.IsAssignableFrom(typeof(IPlugin)) && pluginType.IsClass)//Чтобы исключить все наследования интерфейсов
			Type typeInterface = pluginType.GetInterface(PluginConstant.PluginInterface, false);
			return typeInterface != null;
		}

		/// <summary>Checking the inheritance of one type from another</summary>
		/// <param name="sourceType">Source type wich need to be implemented in <paramref name="targetType"/></param>
		/// <param name="targetType">Target type wich need to be resolved in <paramref name="sourceType"/></param>
		/// <exception cref="ArgumentNullException">sourceType or targetType is null</exception>
		/// <returns><paramref name="targetType"/> implements the object required for <paramref name="sourceType"/></returns>
		public static Boolean InstanceOf(Type sourceType, Type targetType)
		{
			if(sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if(targetType == null)
				throw new ArgumentNullException(nameof(targetType));

			if(sourceType.IsInterface)
			{//Поиск по всем интерфейсам
				foreach(Type iface in targetType.GetInterfaces())
					if(iface.Equals(sourceType))
						return true;
			} else
			{//Поиск по родителям
				if(sourceType.Equals(targetType))
					return true;

				if(sourceType.IsSubclassOf(targetType))
					return true;
			}

			return false;
		}
	}
}