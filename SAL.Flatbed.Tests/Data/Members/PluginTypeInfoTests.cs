using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Xunit;

namespace SAL.Flatbed.Tests
{
	public class PluginTypeInfoTests
	{
		private enum Color { Red, Green, Blue }

		/// <summary>A non-BCL type with a mix of properties, methods and events for member enumeration tests</summary>
		private sealed class StubType
		{
			public String Name { get; set; }
			public Int32 Value { get; }
			public event EventHandler<DataEventArgs> Changed;
			public String GetName() => this.Name;
			public void SetName(String name) => this.Name = name;
		}

		private static PluginTypeInfo CreateForType(Type type, Object target = null)
		{
			// Providing a non-null target (or parent) so the base ctor guard is satisfied
			target ??= new Object();
			return new PluginTypeInfo(type, target, null);
		}

		#region Constructor

		[Fact]
		public void Constructor_NullMember_ThrowsArgumentNullException()
		{
			Action act = () => new PluginTypeInfo(null, new Object(), null);
			act.Should().Throw<ArgumentNullException>().WithParameterName("member");
		}

		[Fact]
		public void Constructor_NullTargetAndNullParent_ThrowsArgumentException()
		{
			Action act = () => new PluginTypeInfo(typeof(String), null, null);
			act.Should().Throw<ArgumentException>();
		}

		[Fact]
		public void Constructor_ValidTypeAndTarget_DoesNotThrow()
		{
			Action act = () => CreateForType(typeof(String));
			act.Should().NotThrow();
		}

		#endregion

		#region IsValueType

		[Fact]
		public void IsValueType_ReferenceType_ReturnsFalse()
		{
			CreateForType(typeof(String)).IsValueType.Should().BeFalse();
		}

		[Fact]
		public void IsValueType_ValueType_ReturnsTrue()
		{
			CreateForType(typeof(Int32)).IsValueType.Should().BeTrue();
		}

		[Fact]
		public void IsValueType_NonTypeMember_ReturnsFalse()
		{
			// When the MemberInfo is not a Type, ReflectedType is null → false
			PropertyInfo prop = typeof(String).GetProperty(nameof(String.Length));
			var sut = new PluginTypeInfo(prop, new Object(), null);
			sut.IsValueType.Should().BeFalse();
		}

		#endregion

		#region IsArray

		[Fact]
		public void IsArray_NonArrayType_ReturnsFalse()
		{
			CreateForType(typeof(String)).IsArray.Should().BeFalse();
		}

		[Fact]
		public void IsArray_GenericListOfString_ReturnsOneEntryForString()
		{
			var sut = CreateForType(typeof(String[]));
			IPluginTypeInfo[] generics = sut.UnderlyingMembers.ToArray();
			generics.Should().HaveCount(1);
			generics[0].TypeName.Should().Be(typeof(String).FullName);
		}

		[Fact]
		public void IsArray_ArrayType_ReturnsTrue()
		{
			CreateForType(typeof(String[])).IsArray.Should().BeTrue();
		}

		#endregion

		#region IsGeneric

		[Fact]
		public void IsGeneric_NonGenericType_ReturnsFalse()
		{
			CreateForType(typeof(String)).IsGeneric.Should().BeFalse();
		}

		[Fact]
		public void IsGeneric_GenericType_ReturnsTrue()
		{
			CreateForType(typeof(List<String>)).IsGeneric.Should().BeTrue();
		}

		#endregion

		#region GetDefaultValues

		[Fact]
		public void GetDefaultValues_NonEnumType_ReturnsEmpty()
		{
			CreateForType(typeof(String)).GetDefaultValues().Should().BeEmpty();
		}

		[Fact]
		public void GetDefaultValues_EnumType_ReturnsEnumNames()
		{
			CreateForType(typeof(Color)).GetDefaultValues()
				.Should().BeEquivalentTo(Enum.GetNames(typeof(Color)));
		}

		#endregion

		#region Members

		[Fact]
		public void Members_UserDefinedType_ContainsPublicProperties()
		{
			var sut = CreateForType(typeof(StubType));
			sut.Members.OfType<IPluginPropertyInfo>().Select(p => p.Name)
				.Should().Contain(nameof(StubType.Name))
				.And.Contain(nameof(StubType.Value));
		}

		[Fact]
		public void Members_UserDefinedType_ContainsPublicMethods()
		{
			var sut = CreateForType(typeof(StubType));
			sut.Members.OfType<IPluginMethodInfo>().Select(m => m.Name)
				.Should().Contain(nameof(StubType.GetName))
				.And.Contain(nameof(StubType.SetName));
		}

		[Fact]
		public void Members_UserDefinedType_ContainsPublicEvents()
		{
			var sut = CreateForType(typeof(StubType));
			sut.Members.OfType<IPluginEventInfo>().Select(e => e.Name)
				.Should().Contain(nameof(StubType.Changed));
		}
		#endregion

		#region GenericMembers

		[Fact]
		public void GenericMembers_NonGenericType_ReturnsEmpty()
		{
			CreateForType(typeof(String)).UnderlyingMembers.Should().BeEmpty();
		}

		[Fact]
		public void GenericMembers_GenericListOfString_ReturnsOneEntryForString()
		{
			var sut = CreateForType(typeof(List<String>));
			IPluginTypeInfo[] generics = sut.UnderlyingMembers.ToArray();
			generics.Should().HaveCount(1);
			generics[0].TypeName.Should().Be(typeof(String).FullName);
		}

		[Fact]
		public void GenericMembers_GenericDictionary_ReturnsTwoEntries()
		{
			var sut = CreateForType(typeof(Dictionary<String, Int32>));
			sut.UnderlyingMembers.Should().HaveCount(2);
		}

		#endregion

		#region GetMember

		[Fact]
		public void GetMember_NullName_ThrowsArgumentNullException()
		{
			var sut = CreateForType(typeof(StubType));
			Action act = () => sut.GetMember<IPluginMemberInfo>(null);
			act.Should().Throw<ArgumentNullException>();
		}

		[Fact]
		public void GetMember_EmptyName_ThrowsArgumentNullException()
		{
			var sut = CreateForType(typeof(StubType));
			Action act = () => sut.GetMember<IPluginMemberInfo>(String.Empty);
			act.Should().Throw<ArgumentNullException>();
		}

		[Fact]
		public void GetMember_ExistingPropertyName_ReturnsPropertyInfo()
		{
			var sut = CreateForType(typeof(StubType));
			sut.GetMember<IPluginPropertyInfo>(nameof(StubType.Name)).Should().NotBeNull();
		}

		[Fact]
		public void GetMember_ExistingMethodName_ReturnsMethodInfo()
		{
			var sut = CreateForType(typeof(StubType));
			sut.GetMember<IPluginMethodInfo>(nameof(StubType.GetName)).Should().NotBeNull();
		}

		[Fact]
		public void GetMember_ExistingMemberNameWrongType_ReturnsDefault()
		{
			// Name exists as a property, but requested as a method — should return null
			var sut = CreateForType(typeof(StubType));
			sut.GetMember<IPluginMethodInfo>(nameof(StubType.Name)).Should().BeNull();
		}

		[Fact]
		public void GetMember_NonExistentName_ReturnsDefault()
		{
			var sut = CreateForType(typeof(StubType));
			sut.GetMember<IPluginMemberInfo>("DoesNotExist").Should().BeNull();
		}

		[Fact]
		public void GetMember_IsCaseSensitive()
		{
			var sut = CreateForType(typeof(StubType));
			// "name" (lowercase) should not match "Name"
			sut.GetMember<IPluginPropertyInfo>("name").Should().BeNull();
		}

		#endregion
	}
}
