using System;
using System.Reflection;
using System.Runtime.InteropServices;
using FluentAssertions;
using Xunit;

namespace SAL.Flatbed.Tests
{
	public class PluginDescriptionTests
	{
		/// <summary>Bare IPlugin with no PluginEntryPointAttribute, lives in the test assembly
		/// (no [Guid], no [AssemblyDescription], no [AssemblyCopyright], has [AssemblyCompany])</summary>
		private sealed class BarePlugin : IPlugin
		{
			public Boolean OnConnection(ConnectMode mode) => true;
			public Boolean OnDisconnection(DisconnectMode mode) => true;
		}

		/// <summary>Allows injecting a controlled Type so attribute lookups target a chosen assembly</summary>
		private sealed class TestablePluginDescription : PluginDescription
		{
			private readonly Type _instanceType;

			public TestablePluginDescription(IPlugin instance, String source, Type instanceType)
				: base(instance, source)
				=> this._instanceType = instanceType;

			internal override Type GetInstanceType() => this._instanceType;
		}

		/// <summary>Creates a description whose attribute lookups resolve against <paramref name="instanceType"/></summary>
		private static TestablePluginDescription CreateWith(Type instanceType, IPlugin instance = null)
			=> new TestablePluginDescription(instance ?? new TestSimplePlugin(), "test-source", instanceType);

		#region Constructor

		[Fact]
		public void Constructor_NullInstance_ThrowsArgumentNullException()
		{
			Action act = () => new PluginDescription(null, "source");
			act.Should().Throw<ArgumentNullException>().WithParameterName("instance");
		}

		[Fact]
		public void Constructor_NullSource_ThrowsArgumentNullException()
		{
			Action act = () => new PluginDescription(new TestSimplePlugin(), null);
			act.Should().Throw<ArgumentNullException>().WithParameterName("source");
		}

		[Fact]
		public void Constructor_EmptySource_ThrowsArgumentNullException()
		{
			Action act = () => new PluginDescription(new TestSimplePlugin(), String.Empty);
			act.Should().Throw<ArgumentNullException>().WithParameterName("source");
		}

		[Fact]
		public void Constructor_ValidArgs_SetsInstanceAndSource()
		{
			var plugin = new TestSimplePlugin();
			var sut = new PluginDescription(plugin, "my-source");
			sut.Instance.Should().BeSameAs(plugin);
			sut.Source.Should().Be("my-source");
		}

		#endregion

		#region ID

		[Fact]
		public void ID_WithPluginEntryPointAttribute_ReturnsAttributeId()
		{
			// TestSimplePlugin is decorated with [PluginEntryPoint(Id, ...)]
			var sut = CreateWith(typeof(TestSimplePlugin));
			sut.ID.Should().Be(TestSimplePlugin.Id);
		}

		[Fact]
		public void ID_WithoutPluginEntryPointAttribute_WithGuidAttribute_ReturnsGuidValue()
		{
			// SAL.Flatbed assembly carries [assembly: Guid("50fc27e8-...")]
			GuidAttribute guid = typeof(PluginDescription).Assembly.GetCustomAttribute<GuidAttribute>();
			var sut = CreateWith(typeof(PluginDescription), new TestSimplePlugin());
			sut.ID.Should().Be(guid.Value);
		}

		[Fact]
		public void ID_WithoutPluginEntryPointAttributeAndNoGuidAttribute_ThrowsArgumentNullException()
		{
			// Test assembly carries no [Guid], so the fallback guard fires
			var sut = CreateWith(typeof(BarePlugin), new BarePlugin());
			Action act = () => { String _ = sut.ID; };
			act.Should().Throw<ArgumentNullException>();
		}

		#endregion

		#region Source

		[Fact]
		public void Source_ReturnsValuePassedToConstructor()
		{
			var sut = new PluginDescription(new TestSimplePlugin(), "my-path");
			sut.Source.Should().Be("my-path");
		}

		#endregion

		#region Name

		[Fact]
		public void Name_WithPluginEntryPointAttribute_ReturnsAttributeName()
		{
			var sut = CreateWith(typeof(TestSimplePlugin));
			sut.Name.Should().Be("TestSimplePlugin");
		}

		[Fact]
		public void Name_WithoutPluginEntryPointAttribute_ReturnsAssemblyName()
		{
			var sut = CreateWith(typeof(BarePlugin), new BarePlugin());
			sut.Name.Should().Be(typeof(BarePlugin).Assembly.GetName().Name);
		}

		#endregion

		#region Version

		[Fact]
		public void Version_WithPluginEntryPointAttribute_ReturnsAttributeVersion()
		{
			var sut = CreateWith(typeof(TestSimplePlugin));
			sut.Version.Should().Be(new Version(1, 0, 0, 0));
		}

		[Fact]
		public void Version_WithoutPluginEntryPointAttribute_ReturnsAssemblyVersion()
		{
			var sut = CreateWith(typeof(BarePlugin), new BarePlugin());
			sut.Version.Should().Be(typeof(BarePlugin).Assembly.GetName().Version);
		}

		#endregion

		#region Description

		[Fact]
		public void Description_WithPluginEntryPointAttribute_ReturnsAttributeDescription()
		{
			// TestSimplePlugin has [PluginEntryPoint(..., description: "")]
			var sut = CreateWith(typeof(TestSimplePlugin));
			sut.Description.Should().Be(String.Empty);
		}

		[Fact]
		public void Description_WithoutPluginEntryPointAttribute_WithAssemblyDescriptionAttribute_ReturnsAssemblyDescription()
		{
			// SAL.Flatbed assembly carries [assembly: AssemblyDescription("...")]
			AssemblyDescriptionAttribute attr = typeof(PluginDescription).Assembly.GetCustomAttribute<AssemblyDescriptionAttribute>();
			var sut = CreateWith(typeof(PluginDescription), new TestSimplePlugin());
			sut.Description.Should().Be(attr.Description);
		}

		[Fact]
		public void Description_WithoutPluginEntryPointAttributeAndNoAssemblyDescription_ReturnsEmptyString()
		{
			// Test assembly has no AssemblyDescriptionAttribute
			var sut = CreateWith(typeof(BarePlugin), new BarePlugin());
			sut.Description.Should().Be(String.Empty);
		}

		#endregion

		#region Company

		[Fact]
		public void Company_WithAssemblyCompanyAttribute_ReturnsCompanyValue()
		{
			// Test assembly has AssemblyCompanyAttribute
			AssemblyCompanyAttribute attr = typeof(BarePlugin).Assembly.GetCustomAttribute<AssemblyCompanyAttribute>();
			var sut = CreateWith(typeof(BarePlugin), new BarePlugin());
			sut.Company.Should().Be(attr.Company);
		}

		#endregion

		#region Copyright

		[Fact]
		public void Copyright_WithAssemblyCopyrightAttribute_ReturnsCopyrightValue()
		{
			// SAL.Flatbed assembly carries [assembly: AssemblyCopyright("...")]
			AssemblyCopyrightAttribute attr = typeof(PluginDescription).Assembly.GetCustomAttribute<AssemblyCopyrightAttribute>();
			var sut = CreateWith(typeof(PluginDescription), new TestSimplePlugin());
			sut.Copyright.Should().Be(attr.Copyright);
		}

		#endregion

		#region Type

		[Fact]
		public void Type_ReturnsNonNull()
		{
			var sut = CreateWith(typeof(TestSimplePlugin));
			sut.Type.Should().NotBeNull();
		}

		[Fact]
		public void Type_NameMatchesPluginClassName()
		{
			var sut = CreateWith(typeof(TestSimplePlugin));
			sut.Type.Name.Should().Be(nameof(TestSimplePlugin));
		}

		#endregion
	}
}