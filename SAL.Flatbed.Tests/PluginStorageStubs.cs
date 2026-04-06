using System;
using System.Reflection;

namespace SAL.Flatbed.Tests
{
	/// <summary>Marker interface used as an unresolvable dependency in tests</summary>
	public interface ITestUnresolvableService { }

	/// <summary>Test plugin dependency interface</summary>
	public interface ITestDependency : IPlugin { }

	/// <summary>Non-plugin class for negative type-loading tests</summary>
	public class NotAPlugin { }

	[PluginEntryPoint(TestSimplePlugin.Id, "1.0.0.0", "TestSimplePlugin", "")]
	public class TestSimplePlugin : IPlugin
	{
		public const String Id = "A1B2C3D4-0000-0000-0000-000000000001";

		public String GetMessage() => "hello";

		public Boolean OnConnection(ConnectMode mode) => true;

		public Boolean OnDisconnection(DisconnectMode mode) => true;
	}

	[PluginEntryPoint(TestKernelPlugin.Id, "1.0.0.0", "TestKernelPlugin", "")]
	public class TestKernelPlugin : IPluginKernel
	{
		public const String Id = "A1B2C3D4-0000-0000-0000-000000000002";

		public Boolean OnConnection(ConnectMode mode) => true;

		public Boolean OnDisconnection(DisconnectMode mode) => true;
	}

	[PluginEntryPoint(TestProviderPlugin.Id, "1.0.0.0", "TestProviderPlugin", "")]
	public class TestProviderPlugin : IPluginProvider
	{
		public const String Id = "A1B2C3D4-0000-0000-0000-000000000003";

		public IPluginProvider ParentProvider { get; set; }

		public Boolean OnConnection(ConnectMode mode) => true;

		public Boolean OnDisconnection(DisconnectMode mode) => true;

		public void LoadPlugins() { }

		public Assembly ResolveAssembly(String assemblyName) => null;
	}

	[PluginEntryPoint(TestSettingsPlugin.Id, "1.0.0.0", "TestSettingsPlugin", "")]
	public class TestSettingsPlugin : IPlugin, IPluginSettings
	{
		public const String Id = "A1B2C3D4-0000-0000-0000-000000000004";

		public Object Settings => new Object();

		public Boolean OnConnection(ConnectMode mode) => true;

		public Boolean OnDisconnection(DisconnectMode mode) => true;
	}

	[PluginEntryPoint(TestHostDependentPlugin.Id, "1.0.0.0", "TestHostDependentPlugin", "")]
	public class TestHostDependentPlugin : IPlugin
	{
		public const String Id = "A1B2C3D4-0000-0000-0000-000000000005";

		public IHost Host { get; }

		public TestHostDependentPlugin(IHost host) => this.Host = host;

		public Boolean OnConnection(ConnectMode mode) => true;

		public Boolean OnDisconnection(DisconnectMode mode) => true;
	}

	[PluginEntryPoint(TestUnresolvablePlugin.Id, "1.0.0.0", "TestUnresolvablePlugin", "")]
	public class TestUnresolvablePlugin : IPlugin
	{
		public const String Id = "A1B2C3D4-0000-0000-0000-000000000006";

		public TestUnresolvablePlugin(ITestUnresolvableService dep) { }

		public Boolean OnConnection(ConnectMode mode) => true;

		public Boolean OnDisconnection(DisconnectMode mode) => true;
	}

	[PluginEntryPoint(TestDependencyPlugin.Id, "1.0.0.0", "TestDependencyPlugin", "")]
	public class TestDependencyPlugin : ITestDependency
	{
		public const String Id = "A1B2C3D4-0000-0000-0000-000000000007";

		public Boolean OnConnection(ConnectMode mode) => true;

		public Boolean OnDisconnection(DisconnectMode mode) => true;
	}

	[PluginEntryPoint(TestDependencyUserPlugin.Id, "1.0.0.0", "TestDependencyUserPlugin", "")]
	public class TestDependencyUserPlugin : IPlugin
	{
		public const String Id = "A1B2C3D4-0000-0000-0000-000000000008";

		public ITestDependency Dependency { get; }

		public TestDependencyUserPlugin(ITestDependency dep) => this.Dependency = dep;

		public Boolean OnConnection(ConnectMode mode) => true;

		public Boolean OnDisconnection(DisconnectMode mode) => true;
	}
}