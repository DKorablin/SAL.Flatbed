using System;
using System.Collections.Generic;
using System.IO;

namespace SAL.Flatbed
{
	/// <summary>Plugin storage interface which is responsible for save and load plugin settings values</summary>
	/// <remarks>Values cn be stored an many storage objects like XML,SQL ets...</remarks>
	public interface ISettingsProvider
	{
		/// <summary>Gets plugin settings value by key</summary>
		/// <param name="key">Plugin settings key</param>
		/// <returns>Value stored in the storage or null if plugin settings is not found</returns>
		Object LoadAssemblyParameter(String key);

		/// <summary>Gets plugin settings value from big storage as a stream</summary>
		/// <param name="key">Plugin settings key from big object</param>
		/// <returns>Data of the large value or null of value is not found</returns>
		Stream LoadAssemblyBlob(String key);

		/// <summary>Loading all parameters from storage and fill settings object</summary>
		/// <param name="settings">Plugin settings object</param>
		void LoadAssemblyParameters<T>(T settings) where T : class;

		/// <summary>Receive all parameters for plugin</summary>
		/// <returns>Array of all known parameters</returns>
		IEnumerable<KeyValuePair<String,Object>> LoadAssemblyParameters();

		/// <summary>Save all plugin parameters in the storage</summary>
		void SaveAssemblyParameters();

		/// <summary>Save plugin parameter by key and value pair</summary>
		/// <remarks>By saving values of type <see cref="Enum"/> the're converted to the base type</remarks>
		/// <param name="key">Plugin settings key</param>
		/// <param name="value">Plugin settings value</param>
		void SaveAssemblyParameter(String key, Object value);

		/// <summary>Save a big plugin parameter value inside a storage</summary>
		/// <param name="key">Plugin settings key</param>
		/// <param name="value">Stream to big value</param>
		void SaveAssemblyBlob(String key, Stream value);

		/// <summary>Remove big plugin parameter value by key</summary>
		/// <param name="key">Plugin settings key to remove</param>
		/// <returns>Deleting big plugin parameter succeeded</returns>
		Boolean RemoveAssemblyBlob(String key);

		/// <summary>Delete plugin parameter by key value from storage</summary>
		/// <param name="key">Plugin settings key to remove</param>
		/// <returns>Deleting plugin parameter succeeded</returns>
		Boolean RemoveAssemblyParameter(String key);

		/// <summary>Removes all plugin parameters from storage</summary>
		void RemoveAssemblyParameter();
	}
}