using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Moq;
using Xunit;

namespace SAL.Flatbed.Tests
{
	public class PluginStorageTests
	{
		private static readonly Assembly TestAssembly = Assembly.GetExecutingAssembly();

		private static Mock<IHost> CreateHostMock() => new Mock<IHost>();

		private static PluginStorage CreateStorage(IHost host = null)
		{
			host ??= CreateHostMock().Object;
			return new PluginStorage(host);
		}

		private static Mock<IPluginDescription> CreateMockDescription(String id, IPlugin instance = null)
		{
			var typeMock = new Mock<IPluginTypeInfo>();
			var descMock = new Mock<IPluginDescription>();
			descMock.Setup(d => d.ID).Returns(id);
			descMock.Setup(d => d.Name).Returns("TestPlugin");
			descMock.Setup(d => d.Source).Returns("test-source");
			descMock.Setup(d => d.Instance).Returns(instance);
			descMock.Setup(d => d.Type).Returns(typeMock.Object);
			return descMock;
		}

		private static Mock<IPluginDescription> CreateKernelMockDescription(String id, IPlugin instance = null)
		{
			var typeMock = new Mock<IPluginTypeInfo>();
			typeMock.Setup(t => t.InstanceOf<IPluginKernel>()).Returns(true);
			var descMock = new Mock<IPluginDescription>();
			descMock.Setup(d => d.ID).Returns(id);
			descMock.Setup(d => d.Name).Returns("TestKernelPlugin");
			descMock.Setup(d => d.Source).Returns("test-source");
			descMock.Setup(d => d.Instance).Returns(instance);
			descMock.Setup(d => d.Type).Returns(typeMock.Object);
			return descMock;
		}

		#region Constructor

		[Fact]
		public void Constructor_NullHost_ThrowsArgumentNullException()
		{
			Action act = () => new PluginStorage(null);
			act.Should().Throw<ArgumentNullException>().WithParameterName("host");
		}

		[Fact]
		public void Constructor_ValidHost_CreatesInstance()
		{
			var storage = CreateStorage();
			storage.Should().NotBeNull();
		}

		#endregion

		#region IndexerPluginId

		[Fact]
		public void IndexerPluginId_NullId_ReturnsNull()
		{
			var storage = CreateStorage();
			storage[(String)null].Should().BeNull();
		}

		[Fact]
		public void IndexerPluginId_EmptyId_ReturnsNull()
		{
			var storage = CreateStorage();
			storage[String.Empty].Should().BeNull();
		}

		[Fact]
		public void IndexerPluginId_NonExistentId_ReturnsNull()
		{
			var storage = CreateStorage();
			storage["non-existent"].Should().BeNull();
		}

		[Fact]
		public void IndexerPluginId_ExistingId_ReturnsPlugin()
		{
			var storage = CreateStorage();
			var desc = CreateMockDescription("my-plugin-id").Object;
			storage.LoadPlugin(desc, ConnectMode.Startup);
			storage["my-plugin-id"].Should().BeSameAs(desc);
		}

		#endregion

		#region Count

		[Fact]
		public void Count_InitiallyZero()
		{
			var storage = CreateStorage();
			storage.Count.Should().Be(0);
		}

		[Fact]
		public void Count_AfterLoadingMultiplePlugins_ReturnsCorrectCount()
		{
			var storage = CreateStorage();
			storage.LoadPlugin(CreateMockDescription("id-1").Object, ConnectMode.Startup);
			storage.LoadPlugin(CreateMockDescription("id-2").Object, ConnectMode.Startup);
			storage.Count.Should().Be(2);
		}

		#endregion

		#region PluginProvider

		[Fact]
		public void PluginProvider_NoProviderLoaded_ReturnsNull()
		{
			var storage = CreateStorage();
			storage.PluginProvider.Should().BeNull();
		}

		#endregion

		#region LoadPlugin(IPluginDescription, ConnectMode)

		[Fact]
		public void LoadPlugin_Description_NullDescription_ThrowsArgumentNullException()
		{
			var storage = CreateStorage();
			Action act = () => storage.LoadPlugin((IPluginDescription)null, ConnectMode.Startup);
			act.Should().Throw<ArgumentNullException>().WithParameterName("plugin");
		}

		[Fact]
		public void LoadPlugin_Description_DuplicateId_ThrowsArgumentException()
		{
			var storage = CreateStorage();
			var desc = CreateMockDescription("dup-id").Object;
			storage.LoadPlugin(desc, ConnectMode.Startup);
			Action act = () => storage.LoadPlugin(desc, ConnectMode.Startup);
			act.Should().Throw<ArgumentException>();
		}

		[Fact]
		public void LoadPlugin_Description_NullInstance_DoesNotThrow()
		{
			var storage = CreateStorage();
			Action act = () => storage.LoadPlugin(CreateMockDescription("null-instance-id", null).Object, ConnectMode.Startup);
			act.Should().NotThrow();
		}

		[Fact]
		public void LoadPlugin_Description_StartupMode_NonProvider_DoesNotCallOnConnection()
		{
			var storage = CreateStorage();
			var pluginMock = new Mock<IPlugin>();
			storage.LoadPlugin(CreateMockDescription("startup-id", pluginMock.Object).Object, ConnectMode.Startup);
			pluginMock.Verify(p => p.OnConnection(It.IsAny<ConnectMode>()), Times.Never);
		}

		[Fact]
		public void LoadPlugin_Description_AfterStartupMode_CallsOnConnection()
		{
			var storage = CreateStorage();
			var pluginMock = new Mock<IPlugin>();
			pluginMock.Setup(p => p.OnConnection(ConnectMode.AfterStartup)).Returns(true);
			storage.LoadPlugin(CreateMockDescription("after-startup-id", pluginMock.Object).Object, ConnectMode.AfterStartup);
			pluginMock.Verify(p => p.OnConnection(ConnectMode.AfterStartup), Times.Once);
		}

		[Fact]
		public void LoadPlugin_Description_PluginProvider_CallsLoadPluginsAndSetsProvider()
		{
			var storage = CreateStorage();
			var providerMock = new Mock<IPluginProvider>();
			providerMock.Setup(p => p.OnConnection(It.IsAny<ConnectMode>())).Returns(true);
			storage.LoadPlugin(CreateMockDescription("provider-id", providerMock.Object).Object, ConnectMode.Startup);
			providerMock.Verify(p => p.LoadPlugins(), Times.Once);
			storage.PluginProvider.Should().BeSameAs(providerMock.Object);
		}

		#endregion

		#region LoadPlugin(Assembly, string source, ConnectMode)

		[Fact]
		public void LoadPlugin_Assembly_NullAssembly_ThrowsArgumentNullException()
		{
			var storage = CreateStorage();
			Action act = () => storage.LoadPlugin((Assembly)null, "source", ConnectMode.Startup);
			act.Should().Throw<ArgumentNullException>().WithParameterName("assembly");
		}

		[Fact]
		public void LoadPlugin_Assembly_EmptySource_ThrowsArgumentNullException()
		{
			var storage = CreateStorage();
			Action act = () => storage.LoadPlugin(TestAssembly, String.Empty, ConnectMode.Startup);
			act.Should().Throw<ArgumentNullException>();
		}

		[Fact]
		public void LoadPlugin_Assembly_ValidAssembly_LoadsPublicPluginTypes()
		{
			var storage = CreateStorage();
			storage.LoadPlugin(TestAssembly, "test", ConnectMode.Startup);
			storage.Count.Should().BeGreaterThan(0);
			storage[TestSimplePlugin.Id].Should().NotBeNull();
			storage[TestKernelPlugin.Id].Should().NotBeNull();
		}

		#endregion

		#region LoadPlugin(Assembly, string type, string source, ConnectMode)

		[Fact]
		public void LoadPlugin_AssemblyType_NullAssembly_ThrowsArgumentNullException()
		{
			var storage = CreateStorage();
			Action act = () => storage.LoadPlugin(null, typeof(TestSimplePlugin).FullName, "source", ConnectMode.Startup);
			act.Should().Throw<ArgumentNullException>().WithParameterName("assembly");
		}

		[Fact]
		public void LoadPlugin_AssemblyType_EmptyType_ThrowsArgumentNullException()
		{
			var storage = CreateStorage();
			Action act = () => storage.LoadPlugin(TestAssembly, String.Empty, "source", ConnectMode.Startup);
			act.Should().Throw<ArgumentNullException>();
		}

		[Fact]
		public void LoadPlugin_AssemblyType_EmptySource_ThrowsArgumentNullException()
		{
			var storage = CreateStorage();
			Action act = () => storage.LoadPlugin(TestAssembly, typeof(TestSimplePlugin).FullName, String.Empty, ConnectMode.Startup);
			act.Should().Throw<ArgumentNullException>();
		}

		[Fact]
		public void LoadPlugin_AssemblyType_TypeNotFound_ThrowsArgumentNullException()
		{
			var storage = CreateStorage();
			Action act = () => storage.LoadPlugin(TestAssembly, "SAL.Flatbed.Tests.NonExistentType", "source", ConnectMode.Startup);
			act.Should().Throw<ArgumentNullException>();
		}

		[Fact]
		public void LoadPlugin_AssemblyType_NonPluginType_ThrowsArgumentException()
		{
			var storage = CreateStorage();
			Action act = () => storage.LoadPlugin(TestAssembly, typeof(NotAPlugin).FullName, "source", ConnectMode.Startup);
			act.Should().Throw<ArgumentException>();
		}

		[Fact]
		public void LoadPlugin_AssemblyType_ValidType_LoadsPlugin()
		{
			var storage = CreateStorage();
			storage.LoadPlugin(TestAssembly, typeof(TestSimplePlugin).FullName, "test", ConnectMode.Startup);
			storage.Count.Should().Be(1);
			storage[TestSimplePlugin.Id].Should().NotBeNull();
		}

		#endregion

		#region UnloadPlugin

		[Fact]
		public void UnloadPlugin_NullPlugin_ThrowsArgumentNullException()
		{
			var storage = CreateStorage();
			Action act = () => storage.UnloadPlugin(null);
			act.Should().Throw<ArgumentNullException>().WithParameterName("plugin");
		}

		[Fact]
		public void UnloadPlugin_NullInstance_ReturnsFalseAndPluginRemains()
		{
			var storage = CreateStorage();
			var desc = CreateMockDescription("null-inst-id", null).Object;
			storage.LoadPlugin(desc, ConnectMode.Startup);
			storage.UnloadPlugin(desc).Should().BeFalse();
			storage.Count.Should().Be(1);
		}

		[Fact]
		public void UnloadPlugin_OnDisconnectionReturnsFalse_ReturnsFalseAndPluginRemains()
		{
			var storage = CreateStorage();
			var pluginMock = new Mock<IPlugin>();
			pluginMock.Setup(p => p.OnDisconnection(DisconnectMode.UserClosed)).Returns(false);
			var desc = CreateMockDescription("stays-id", pluginMock.Object).Object;
			storage.LoadPlugin(desc, ConnectMode.Startup);
			storage.UnloadPlugin(desc).Should().BeFalse();
			storage.Count.Should().Be(1);
		}

		[Fact]
		public void UnloadPlugin_OnDisconnectionReturnsTrue_ReturnsTrueAndPluginRemoved()
		{
			var storage = CreateStorage();
			var pluginMock = new Mock<IPlugin>();
			pluginMock.Setup(p => p.OnDisconnection(DisconnectMode.UserClosed)).Returns(true);
			var desc = CreateMockDescription("removed-id", pluginMock.Object).Object;
			storage.LoadPlugin(desc, ConnectMode.Startup);
			storage.UnloadPlugin(desc).Should().BeTrue();
			storage.Count.Should().Be(0);
		}

		[Fact]
		public void UnloadPlugin_Success_FiresPluginUnloadedEvent()
		{
			var storage = CreateStorage();
			var pluginMock = new Mock<IPlugin>();
			pluginMock.Setup(p => p.OnDisconnection(DisconnectMode.UserClosed)).Returns(true);
			var desc = CreateMockDescription("event-id", pluginMock.Object).Object;
			storage.LoadPlugin(desc, ConnectMode.Startup);

			PluginEventArgs receivedArgs = null;
			storage.PluginUnloaded += (_, e) => receivedArgs = e;
			storage.UnloadPlugin(desc);

			receivedArgs.Should().NotBeNull();
			receivedArgs.Plugin.Should().BeSameAs(desc);
		}

		[Fact]
		public void UnloadPlugin_NotInCollection_ThrowsArgumentException()
		{
			var storage = CreateStorage();
			var pluginMock = new Mock<IPlugin>();
			pluginMock.Setup(p => p.OnDisconnection(DisconnectMode.UserClosed)).Returns(true);
			var desc = CreateMockDescription("orphan-id", pluginMock.Object).Object;
			Action act = () => storage.UnloadPlugin(desc);
			act.Should().Throw<ArgumentException>();
		}

		#endregion

		#region RemovePlugins

		[Fact]
		public void RemovePlugins_ClearsAllPlugins()
		{
			var storage = CreateStorage();
			storage.LoadPlugin(CreateMockDescription("id-1").Object, ConnectMode.Startup);
			storage.LoadPlugin(CreateMockDescription("id-2").Object, ConnectMode.Startup);
			storage.RemovePlugins();
			storage.Count.Should().Be(0);
		}

		#endregion

		#region InitializePlugins

		[Fact]
		public void InitializePlugins_CallsOnConnectionForNonKernelPlugins()
		{
			var storage = CreateStorage();
			var pluginMock = new Mock<IPlugin>();
			pluginMock.Setup(p => p.OnConnection(ConnectMode.Startup)).Returns(true);
			storage.LoadPlugin(CreateMockDescription("init-id", pluginMock.Object).Object, ConnectMode.Startup);

			storage.InitializePlugins();

			pluginMock.Verify(p => p.OnConnection(ConnectMode.Startup), Times.Once);
		}

		[Fact]
		public void InitializePlugins_KernelPluginsInitializedBeforeOthers()
		{
			var storage = CreateStorage();
			var callOrder = new List<String>();

			var kernelMock = new Mock<IPluginKernel>();
			kernelMock.Setup(p => p.OnConnection(ConnectMode.Startup))
				.Callback(() => callOrder.Add("kernel"))
				.Returns(true);
			storage.LoadPlugin(CreateKernelMockDescription("kernel-id", kernelMock.Object).Object, ConnectMode.Startup);

			var normalMock = new Mock<IPlugin>();
			normalMock.Setup(p => p.OnConnection(ConnectMode.Startup))
				.Callback(() => callOrder.Add("normal"))
				.Returns(true);
			storage.LoadPlugin(CreateMockDescription("normal-id", normalMock.Object).Object, ConnectMode.Startup);

			storage.InitializePlugins();

			callOrder.Should().ContainInOrder("kernel", "normal");
		}

		[Fact]
		public void InitializePlugins_KernelPluginsNotCalledTwice()
		{
			var storage = CreateStorage();
			var kernelMock = new Mock<IPluginKernel>();
			kernelMock.Setup(p => p.OnConnection(ConnectMode.Startup)).Returns(true);
			storage.LoadPlugin(CreateKernelMockDescription("kernel-id", kernelMock.Object).Object, ConnectMode.Startup);

			storage.InitializePlugins();

			kernelMock.Verify(p => p.OnConnection(ConnectMode.Startup), Times.Once);
		}

		[Fact]
		public void InitializePlugins_FiresPluginsLoadedEvent()
		{
			var storage = CreateStorage();
			Boolean fired = false;
			storage.PluginsLoaded += (_, _) => fired = true;

			storage.InitializePlugins();

			fired.Should().BeTrue();
		}

		[Fact]
		public void InitializePlugins_ExceptionInOnePlugin_ContinuesInitializingOthers()
		{
			var storage = CreateStorage();
			var throwingMock = new Mock<IPlugin>();
			throwingMock.Setup(p => p.OnConnection(It.IsAny<ConnectMode>())).Throws<InvalidOperationException>();
			storage.LoadPlugin(CreateMockDescription("throwing-id", throwingMock.Object).Object, ConnectMode.Startup);

			var normalMock = new Mock<IPlugin>();
			normalMock.Setup(p => p.OnConnection(ConnectMode.Startup)).Returns(true);
			storage.LoadPlugin(CreateMockDescription("normal-id", normalMock.Object).Object, ConnectMode.Startup);

			Action act = () => storage.InitializePlugins();
			act.Should().NotThrow();
			normalMock.Verify(p => p.OnConnection(ConnectMode.Startup), Times.Once);
		}

		#endregion

		#region FindPluginType

		[Fact]
		public void FindPluginType_ReturnsOnlyMatchingPlugins()
		{
			var storage = CreateStorage();
			var kernelDesc = CreateKernelMockDescription("kernel-id").Object;
			var normalDesc = CreateMockDescription("normal-id").Object;
			storage.LoadPlugin(kernelDesc, ConnectMode.Startup);
			storage.LoadPlugin(normalDesc, ConnectMode.Startup);

			var result = storage.FindPluginType<IPluginKernel>().ToList();

			result.Should().HaveCount(1);
			result.Should().Contain(kernelDesc);
			result.Should().NotContain(normalDesc);
		}

		#endregion

		#region SendMessage

		[Fact]
		public void SendMessage_PluginNotFound_ThrowsArgumentException()
		{
			var storage = CreateStorage();
			Action act = () => storage.SendMessage("non-existent", "SomeMethod");
			act.Should().Throw<ArgumentException>().WithParameterName("pluginId");
		}

		[Fact]
		public void SendMessage_MethodNotFound_ThrowsArgumentException()
		{
			var storage = CreateStorage();
			storage.LoadPlugin(TestAssembly, typeof(TestSimplePlugin).FullName, "test", ConnectMode.Startup);
			Action act = () => storage.SendMessage(TestSimplePlugin.Id, "NonExistentMethod");
			act.Should().Throw<ArgumentException>().WithParameterName("message");
		}

		[Fact]
		public void SendMessage_ValidMethod_ReturnsResult()
		{
			var storage = CreateStorage();
			storage.LoadPlugin(TestAssembly, typeof(TestSimplePlugin).FullName, "test", ConnectMode.Startup);
			var result = storage.SendMessage(TestSimplePlugin.Id, "GetMessage");
			result.Should().Be("hello");
		}

		#endregion

		#region Settings

		[Fact]
		public void Settings_NullPlugin_ThrowsArgumentNullException()
		{
			var storage = CreateStorage();
			Action act = () => storage.Settings(null);
			act.Should().Throw<ArgumentNullException>().WithParameterName("plugin");
		}

		[Fact]
		public void Settings_PluginDoesNotImplementIPluginSettings_ThrowsArgumentException()
		{
			var storage = CreateStorage();
			var plugin = new Mock<IPlugin>().Object;
			Action act = () => storage.Settings(plugin);
			act.Should().Throw<ArgumentException>();
		}

		[Fact]
		public void Settings_ValidPlugin_ReturnsProvider()
		{
			var storage = CreateStorage();
			var expectedProvider = new Mock<ISettingsProvider>().Object;
			var settingsPluginProviderMock = new Mock<ISettingsPluginProvider>();
			settingsPluginProviderMock.Setup(p => p.GetSettingsProvider(It.IsAny<IPlugin>())).Returns(expectedProvider);
			storage.SetSettingsProvider(CreateMockDescription("settings-provider-id", settingsPluginProviderMock.Object).Object);

			var result = storage.Settings(new TestSettingsPlugin());

			result.Should().BeSameAs(expectedProvider);
		}

		[Fact]
		public void Settings_CalledTwice_CachesResult()
		{
			var storage = CreateStorage();
			var expectedProvider = new Mock<ISettingsProvider>().Object;
			var settingsPluginProviderMock = new Mock<ISettingsPluginProvider>();
			settingsPluginProviderMock.Setup(p => p.GetSettingsProvider(It.IsAny<IPlugin>())).Returns(expectedProvider);
			storage.SetSettingsProvider(CreateMockDescription("settings-provider-id", settingsPluginProviderMock.Object).Object);

			var plugin = new TestSettingsPlugin();
			_ = storage.Settings(plugin);
			_ = storage.Settings(plugin);

			settingsPluginProviderMock.Verify(p => p.GetSettingsProvider(plugin), Times.Once);
		}

		#endregion

		#region SetSettingsProvider

		[Fact]
		public void SetSettingsProvider_NullPlugin_ThrowsArgumentNullException()
		{
			var storage = CreateStorage();
			Action act = () => storage.SetSettingsProvider(null);
			act.Should().Throw<ArgumentNullException>().WithParameterName("plugin");
		}

		[Fact]
		public void SetSettingsProvider_NullInstance_ThrowsArgumentException()
		{
			var storage = CreateStorage();
			Action act = () => storage.SetSettingsProvider(CreateMockDescription("id", null).Object);
			act.Should().Throw<ArgumentException>().WithParameterName("plugin");
		}

		#endregion

		#region SetPluginProvider

		[Fact]
		public void SetPluginProvider_NullPlugin_ThrowsArgumentNullException()
		{
			var storage = CreateStorage();
			Action act = () => storage.SetPluginProvider(null);
			act.Should().Throw<ArgumentNullException>().WithParameterName("plugin");
		}

		[Fact]
		public void SetPluginProvider_NullInstance_ThrowsArgumentException()
		{
			var storage = CreateStorage();
			Action act = () => storage.SetPluginProvider(CreateMockDescription("id", null).Object);
			act.Should().Throw<ArgumentException>().WithParameterName("plugin");
		}

		[Fact]
		public void SetPluginProvider_WhenProviderAlreadySet_SetsParentOnNewProvider()
		{
			var storage = CreateStorage();

			var firstProviderMock = new Mock<IPluginProvider>();
			storage.SetPluginProvider(CreateMockDescription("first-id", firstProviderMock.Object).Object);

			var secondProviderMock = new Mock<IPluginProvider>();
			storage.SetPluginProvider(CreateMockDescription("second-id", secondProviderMock.Object).Object);

			secondProviderMock.VerifySet(p => p.ParentProvider = firstProviderMock.Object, Times.Once);
		}

		#endregion

		#region GetEnumerator

		[Fact]
		public void GetEnumerator_ReturnsAllLoadedPlugins()
		{
			var storage = CreateStorage();
			var desc1 = CreateMockDescription("id-1").Object;
			var desc2 = CreateMockDescription("id-2").Object;
			storage.LoadPlugin(desc1, ConnectMode.Startup);
			storage.LoadPlugin(desc2, ConnectMode.Startup);

			var plugins = storage.ToList();

			plugins.Should().HaveCount(2);
			plugins.Should().Contain(desc1);
			plugins.Should().Contain(desc2);
		}

		[Fact]
		public void GetEnumerator_EmptyStorage_ReturnsEmptySequence()
		{
			var storage = CreateStorage();
			storage.ToList().Should().BeEmpty();
		}

		#endregion

		#region Dependency Resolution

		[Fact]
		public void LoadPlugin_PluginWithHostDependency_InjectsHostAndLoadsPlugin()
		{
			var hostMock = CreateHostMock();
			var storage = new PluginStorage(hostMock.Object);
			storage.LoadPlugin(TestAssembly, typeof(TestHostDependentPlugin).FullName, "test", ConnectMode.Startup);

			storage.Count.Should().Be(1);
			((TestHostDependentPlugin)storage[TestHostDependentPlugin.Id].Instance).Host.Should().BeSameAs(hostMock.Object);
		}

		[Fact]
		public void LoadPlugin_PluginWithUnresolvableDependency_IsNotLoaded()
		{
			var storage = CreateStorage();
			storage.LoadPlugin(TestAssembly, typeof(TestUnresolvablePlugin).FullName, "test", ConnectMode.Startup);
			storage.Count.Should().Be(0);
		}

		[Fact]
		public void LoadPlugin_DeferredPlugin_ResolvedAfterDependencyIsLoaded()
		{
			var storage = CreateStorage();
			storage.LoadPlugin(TestAssembly, typeof(TestDependencyUserPlugin).FullName, "test", ConnectMode.Startup);
			storage.Count.Should().Be(0, "dependency not yet loaded");

			storage.LoadPlugin(TestAssembly, typeof(TestDependencyPlugin).FullName, "test", ConnectMode.Startup);

			storage.Count.Should().Be(2, "both the dependency and the dependent plugin should be loaded");
			storage[TestDependencyPlugin.Id].Should().NotBeNull();
			storage[TestDependencyUserPlugin.Id].Should().NotBeNull();
		}

		[Fact]
		public void LoadPlugin_DeferredPlugin_DependencyIsInjected()
		{
			var storage = CreateStorage();
			storage.LoadPlugin(TestAssembly, typeof(TestDependencyUserPlugin).FullName, "test", ConnectMode.Startup);
			storage.LoadPlugin(TestAssembly, typeof(TestDependencyPlugin).FullName, "test", ConnectMode.Startup);

			var userPlugin = (TestDependencyUserPlugin)storage[TestDependencyUserPlugin.Id].Instance;
			userPlugin.Dependency.Should().NotBeNull();
			userPlugin.Dependency.Should().BeSameAs(storage[TestDependencyPlugin.Id].Instance);
		}

		#endregion

		#region IndexerPlugin

		[Fact]
		public void IndexerPlugin_NullPlugin_ReturnsNull()
		{
			var storage = CreateStorage();
			storage[(IPlugin)null].Should().BeNull();
		}

		[Fact]
		public void IndexerPlugin_NonExistentPlugin_ReturnsNull()
		{
			var storage = CreateStorage();
			storage[new Mock<IPlugin>().Object].Should().BeNull();
		}

		[Fact]
		public void IndexerPlugin_ExistingPlugin_ReturnsDescription()
		{
			var storage = CreateStorage();
			var pluginMock = new Mock<IPlugin>();
			var desc = CreateMockDescription("plugin-inst-id", pluginMock.Object).Object;
			storage.LoadPlugin(desc, ConnectMode.Startup);
			storage[pluginMock.Object].Should().BeSameAs(desc);
		}

		[Fact]
		public void IndexerPlugin_MatchesByInstance_NotById()
		{
			var storage = CreateStorage();
			var pluginMock1 = new Mock<IPlugin>();
			var pluginMock2 = new Mock<IPlugin>();
			storage.LoadPlugin(CreateMockDescription("id-1", pluginMock1.Object).Object, ConnectMode.Startup);
			storage.LoadPlugin(CreateMockDescription("id-2", pluginMock2.Object).Object, ConnectMode.Startup);
			storage[pluginMock1.Object].Should().BeSameAs(storage["id-1"]);
			storage[pluginMock2.Object].Should().BeSameAs(storage["id-2"]);
		}

		[Fact]
		public void IndexerPlugin_AfterUnload_ReturnsNull()
		{
			var storage = CreateStorage();
			var pluginMock = new Mock<IPlugin>();
			pluginMock.Setup(p => p.OnDisconnection(DisconnectMode.UserClosed)).Returns(true);
			var desc = CreateMockDescription("unloaded-id", pluginMock.Object).Object;
			storage.LoadPlugin(desc, ConnectMode.Startup);
			storage.UnloadPlugin(desc);
			storage[pluginMock.Object].Should().BeNull();
		}

		#endregion

		#region GetTraceSource

		[Fact]
		public void GetTraceSource_NullName_ThrowsArgumentNullException()
		{
			var storage = CreateStorage();
			Action act = () => storage.GetTraceSource(null);
			act.Should().Throw<ArgumentNullException>().WithParameterName("name");
		}

		[Fact]
		public void GetTraceSource_EmptyName_ThrowsArgumentNullException()
		{
			var storage = CreateStorage();
			Action act = () => storage.GetTraceSource(String.Empty);
			act.Should().Throw<ArgumentNullException>().WithParameterName("name");
		}

		[Fact]
		public void GetTraceSource_ValidName_ReturnsNonNullITraceSource()
		{
			var storage = CreateStorage();
			storage.GetTraceSource("test-source").Should().NotBeNull().And.BeAssignableTo<ITraceSource>();
		}

		[Fact]
		public void GetTraceSource_CalledTwiceWithSameName_ReturnsDifferentInstances()
		{
			var storage = CreateStorage();
			var first = storage.GetTraceSource("test-source");
			var second = storage.GetTraceSource("test-source");
			first.Should().NotBeSameAs(second);
		}

		[Fact]
		public void GetTraceSource_IsVirtual_SubclassCanOverride()
		{
			var traceMock = new Mock<ITraceSource>().Object;
			var storage = new FixedTraceSourceStorage(CreateHostMock().Object, traceMock);
			storage.GetTraceSource("any").Should().BeSameAs(traceMock);
		}

		#endregion

		#region TraceSource Dependency Resolution

		[Fact]
		public void LoadPlugin_PluginWithTraceSourceDependency_IsLoaded()
		{
			// ITraceSource resolution in ResolveAndCreate is not yet implemented —
			var storage = CreateStorage();
			storage.LoadPlugin(TestAssembly, typeof(TestTraceSourceDependentPlugin).FullName, "test", ConnectMode.Startup);
			storage.Count.Should().Be(1);
			storage[TestTraceSourceDependentPlugin.Id].Should().NotBeNull();
		}

		[Fact]
		public void LoadPlugin_PluginWithTraceSourceDependency_TraceSourceIsInjected()
		{
			var storage = CreateStorage();
			storage.LoadPlugin(TestAssembly, typeof(TestTraceSourceDependentPlugin).FullName, "test", ConnectMode.Startup);
			var plugin = (TestTraceSourceDependentPlugin)storage[TestTraceSourceDependentPlugin.Id].Instance;
			plugin.Trace.Should().NotBeNull();
		}

		[Fact]
		public void LoadPlugin_PluginWithTraceSourceDependency_UsesAssemblyNameAsTraceName()
		{
			var traceMock = new Mock<ITraceSource>().Object;
			String capturedName = null;
			var storage = new FixedTraceSourceStorage(CreateHostMock().Object, traceMock, name => capturedName = name);
			storage.LoadPlugin(TestAssembly, typeof(TestTraceSourceDependentPlugin).FullName, "test", ConnectMode.Startup);
			capturedName.Should().Be(TestAssembly.GetName().Name);
		}

		#endregion
	}

	/// <summary>Overrides <see cref="PluginStorage.GetTraceSource"/> to return a fixed instance,
	/// used to verify the method is correctly declared as virtual.
	/// An optional <paramref name="onGetTraceSource"/> callback captures the name passed to the method.</summary>
	internal sealed class FixedTraceSourceStorage : PluginStorage
	{
		private readonly ITraceSource _trace;
		private readonly Action<String> _onGetTraceSource;

		public FixedTraceSourceStorage(IHost host, ITraceSource trace, Action<String> onGetTraceSource = null) : base(host)
		{
			this._trace = trace;
			this._onGetTraceSource = onGetTraceSource;
		}

		public override ITraceSource GetTraceSource(String name)
		{
			this._onGetTraceSource?.Invoke(name);
			return this._trace;
		}
	}
}
