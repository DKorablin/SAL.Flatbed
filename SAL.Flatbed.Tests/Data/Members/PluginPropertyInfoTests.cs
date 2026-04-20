using System;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Xunit;

namespace SAL.Flatbed.Tests
{
	public class PluginPropertyInfoTests
	{
		private sealed class Stub
		{
			public String ReadWrite { get; set; } = "initial";
			public String ReadOnly { get; } = "readonly";
			public String WriteOnly { set => this._writeOnlyValue = value; }
			private String _writeOnlyValue;
			public String GetWriteOnlyValue() => this._writeOnlyValue;
			public String this[Int32 index] => index.ToString();
		}

		private static PropertyInfo GetProp(String name)
			=> typeof(Stub).GetProperty(name, BindingFlags.Public | BindingFlags.Instance);

		private static PropertyInfo GetIndexer()
			=> typeof(Stub).GetProperties().First(p => p.GetIndexParameters().Length > 0);

		private static PluginPropertyInfo Create(String propName, Object target = null, PluginMemberInfo parent = null)
		{
			target ??= new Stub();
			return new PluginPropertyInfo(GetProp(propName), target, parent);
		}

		private static PluginPropertyInfo CreateWithNullResolvedTarget(String propName)
		{
			// Route through a parent property that returns null so GetTarget() resolves to null
			var nullStub = new NullPropertyStub();
			PropertyInfo nullProp = typeof(NullPropertyStub).GetProperty(nameof(NullPropertyStub.NullValue));
			var parent = new PluginPropertyInfo(nullProp, nullStub, null);
			return new PluginPropertyInfo(GetProp(propName), null, parent);
		}

		public sealed class NullPropertyStub
		{
			public Object NullValue => null;
		}

		#region Constructor

		[Fact]
		public void Constructor_NullProperty_ThrowsNullReferenceException()
		{
			// property.PropertyType is accessed in base ctor call before the null guard fires
			Action act = () => new PluginPropertyInfo(null, new Stub(), null);
			act.Should().Throw<Exception>();
		}

		[Fact]
		public void Constructor_NullTargetAndNullParent_ThrowsArgumentException()
		{
			Action act = () => new PluginPropertyInfo(GetProp(nameof(Stub.ReadWrite)), null, null);
			act.Should().Throw<ArgumentException>();
		}

		[Fact]
		public void Constructor_ValidPropertyAndTarget_DoesNotThrow()
		{
			Action act = () => Create(nameof(Stub.ReadWrite));
			act.Should().NotThrow();
		}

		[Fact]
		public void Constructor_NullTargetWithValidParent_DoesNotThrow()
		{
			var parent = Create(nameof(Stub.ReadWrite));
			Action act = () => new PluginPropertyInfo(GetProp(nameof(Stub.ReadOnly)), null, parent);
			act.Should().NotThrow();
		}

		#endregion

		#region Name

		[Fact]
		public void Name_ReturnsPropertyName()
		{
			var sut = Create(nameof(Stub.ReadWrite));
			sut.Name.Should().Be(nameof(Stub.ReadWrite));
		}

		#endregion

		#region TypeName

		[Fact]
		public void TypeName_ReturnsPropertyTypeName()
		{
			var sut = Create(nameof(Stub.ReadWrite));
			sut.TypeName.Should().Be(typeof(String).ToString());
		}

		#endregion

		#region AssemblyQualifiedName

		[Fact]
		public void AssemblyQualifiedName_ReturnsPropertyTypeAssemblyQualifiedName()
		{
			var sut = Create(nameof(Stub.ReadWrite));
			sut.AssemblyQualifiedName.Should().Be(typeof(String).AssemblyQualifiedName);
		}

		#endregion

		#region MemberType

		[Fact]
		public void MemberType_ReturnsProperty()
		{
			var sut = Create(nameof(Stub.ReadWrite));
			sut.MemberType.Should().Be(MemberTypes.Property);
		}

		#endregion

		#region CanRead / CanWrite

		[Fact]
		public void CanRead_ReadWriteProperty_ReturnsTrue()
		{
			Create(nameof(Stub.ReadWrite)).CanRead.Should().BeTrue();
		}

		[Fact]
		public void CanRead_ReadOnlyProperty_ReturnsTrue()
		{
			Create(nameof(Stub.ReadOnly)).CanRead.Should().BeTrue();
		}

		[Fact]
		public void CanRead_WriteOnlyProperty_ReturnsFalse()
		{
			Create(nameof(Stub.WriteOnly)).CanRead.Should().BeFalse();
		}

		[Fact]
		public void CanWrite_ReadWriteProperty_ReturnsTrue()
		{
			Create(nameof(Stub.ReadWrite)).CanWrite.Should().BeTrue();
		}

		[Fact]
		public void CanWrite_ReadOnlyProperty_ReturnsFalse()
		{
			Create(nameof(Stub.ReadOnly)).CanWrite.Should().BeFalse();
		}

		[Fact]
		public void CanWrite_WriteOnlyProperty_ReturnsTrue()
		{
			Create(nameof(Stub.WriteOnly)).CanWrite.Should().BeTrue();
		}

		#endregion

		#region Count

		[Fact]
		public void Count_RegularProperty_ReturnsZero()
		{
			Create(nameof(Stub.ReadWrite)).Count.Should().Be(0);
		}

		[Fact]
		public void Count_IndexerProperty_ReturnsOne()
		{
			var sut = new PluginPropertyInfo(GetIndexer(), new Stub(), null);
			sut.Count.Should().Be(1);
		}

		#endregion

		#region GetParameters

		[Fact]
		public void GetParameters_RegularProperty_ReturnsEmpty()
		{
			Create(nameof(Stub.ReadWrite)).GetParameters().Should().BeEmpty();
		}

		[Fact]
		public void GetParameters_IndexerProperty_ReturnsOneParameter()
		{
			var sut = new PluginPropertyInfo(GetIndexer(), new Stub(), null);
			sut.GetParameters().Should().HaveCount(1);
		}

		#endregion

		#region Get

		[Fact]
		public void Get_NullResolvedTarget_ThrowsInvalidOperationException()
		{
			var sut = CreateWithNullResolvedTarget(nameof(Stub.ReadWrite));
			Action act = () => sut.Get();
			act.Should().Throw<InvalidOperationException>();
		}

		[Fact]
		public void Get_ReadWriteProperty_ReturnsValue()
		{
			var stub = new Stub();
			var sut = new PluginPropertyInfo(GetProp(nameof(Stub.ReadWrite)), stub, null);
			sut.Get().Should().Be(stub.ReadWrite);
		}

		[Fact]
		public void Get_AfterValueChanged_ReturnsNewValue()
		{
			var stub = new Stub { ReadWrite = "changed" };
			var sut = new PluginPropertyInfo(GetProp(nameof(Stub.ReadWrite)), stub, null);
			sut.Get().Should().Be("changed");
		}

		#endregion

		#region Set

		[Fact]
		public void Set_NullResolvedTarget_ThrowsInvalidOperationException()
		{
			var sut = CreateWithNullResolvedTarget(nameof(Stub.ReadWrite));
			Action act = () => sut.Set("new");
			act.Should().Throw<InvalidOperationException>();
		}

		[Fact]
		public void Set_ReadWriteProperty_ValueIsUpdated()
		{
			var stub = new Stub();
			var sut = new PluginPropertyInfo(GetProp(nameof(Stub.ReadWrite)), stub, null);
			sut.Set("updated");
			stub.ReadWrite.Should().Be("updated");
		}

		[Fact]
		public void Set_WriteOnlyProperty_ValueIsUpdated()
		{
			var stub = new Stub();
			var sut = new PluginPropertyInfo(GetProp(nameof(Stub.WriteOnly)), stub, null);
			sut.Set("written");
			stub.GetWriteOnlyValue().Should().Be("written");
		}

		#endregion
	}
}
