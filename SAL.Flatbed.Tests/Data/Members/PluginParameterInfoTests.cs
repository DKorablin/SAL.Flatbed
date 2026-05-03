using System;
using System.Reflection;
using FluentAssertions;
using Xunit;

namespace SAL.Flatbed.Tests
{
	public class PluginParameterInfoTests
	{
		private enum Color { Red, Green, Blue }

		private static class Methods
		{
#pragma warning disable CA1801
			public static void NoDefault(Int32 value) { }
			public static void WithIntDefault(Int32 value = 99) { }
			public static void WithStringDefault(String value = "hi") { }
			public static void WithEnumParam(Color color) { }
			public static void WithEnumDefault(Color color = Color.Green) { }
			public static void OutParam(out Int32 value) { value = 0; }
			public static void RegularParam(Int32 value) { }
#pragma warning restore CA1801
		}

		private static ParameterInfo GetParam(String methodName, Int32 index = 0)
			=> typeof(Methods).GetMethod(methodName).GetParameters()[index];

		private static PluginMethodInfo MakeParentMethod(String methodName)
			=> new PluginMethodInfo(typeof(Methods).GetMethod(methodName), new Object(), null);

		private static PluginParameterInfo Create(String methodName, Int32 paramIndex = 0)
		{
			var parent = MakeParentMethod(methodName);
			return new PluginParameterInfo(parent, GetParam(methodName, paramIndex));
		}

		#region Constructor

		[Fact]
		public void Constructor_NullMember_ThrowsArgumentException()
		{
			// null member propagates as null target+parent into PluginMemberInfo, which throws ArgumentException first
			Action act = () => new PluginParameterInfo(null, GetParam(nameof(Methods.NoDefault)));
			act.Should().Throw<ArgumentException>();
		}

		[Fact]
		public void Constructor_NullParameter_Throws()
		{
			// parameter.ParameterType is accessed in the base ctor call before the null guard fires
			var parent = MakeParentMethod(nameof(Methods.NoDefault));
			Action act = () => new PluginParameterInfo(parent, null);
			act.Should().Throw<Exception>();
		}

		[Fact]
		public void Constructor_ValidArgs_DoesNotThrow()
		{
			Action act = () => Create(nameof(Methods.NoDefault));
			act.Should().NotThrow();
		}

		#endregion

		#region Method

		[Fact]
		public void Method_ReturnsOwningMethodInfo()
		{
			var parent = MakeParentMethod(nameof(Methods.NoDefault));
			var sut = new PluginParameterInfo(parent, GetParam(nameof(Methods.NoDefault)));
			sut.Method.Should().BeSameAs(parent);
		}

		#endregion

		#region Name

		[Fact]
		public void Name_ReturnsParameterName()
		{
			var sut = Create(nameof(Methods.NoDefault));
			sut.Name.Should().Be("value");
		}

		#endregion

		#region MemberType

		[Fact]
		public void MemberType_AlwaysReturnsTypeInfo()
		{
			var sut = Create(nameof(Methods.NoDefault));
			sut.MemberType.Should().Be(MemberTypes.TypeInfo);
		}

		#endregion

		#region IsOut

		[Fact]
		public void IsOut_RegularParameter_ReturnsFalse()
		{
			var sut = Create(nameof(Methods.RegularParam));
			sut.IsOut.Should().BeFalse();
		}

		[Fact]
		public void IsOut_OutParameter_ReturnsTrue()
		{
			var sut = Create(nameof(Methods.OutParam));
			sut.IsOut.Should().BeTrue();
		}

		#endregion

		#region GetDefaultValues

		[Fact]
		public void GetDefaultValues_NoDefault_ReturnsEmpty()
		{
			var sut = Create(nameof(Methods.NoDefault));
			sut.GetDefaultValues().Should().BeEmpty();
		}

		[Fact]
		public void GetDefaultValues_IntDefault_ReturnsThatValue()
		{
			var sut = Create(nameof(Methods.WithIntDefault));
			sut.GetDefaultValues().Should().ContainSingle().Which.Should().Be("99");
		}

		[Fact]
		public void GetDefaultValues_StringDefault_ReturnsThatValue()
		{
			var sut = Create(nameof(Methods.WithStringDefault));
			sut.GetDefaultValues().Should().ContainSingle().Which.Should().Be("hi");
		}

		[Fact]
		public void GetDefaultValues_EnumParamNoDefault_ReturnsEnumNames()
		{
			// No default value: base PluginTypeInfo.GetDefaultValues returns Enum.GetNames
			var sut = Create(nameof(Methods.WithEnumParam));
			sut.GetDefaultValues().Should().BeEquivalentTo(Enum.GetNames(typeof(Color)));
		}

		[Fact]
		public void GetDefaultValues_EnumParamWithDefault_ContainsDefaultAndEnumNames()
		{
			var sut = Create(nameof(Methods.WithEnumDefault));
			String[] result = sut.GetDefaultValues();
			// First entry is the default value string, rest are enum names
			result.Should().Contain(Color.Green.ToString());
			result.Should().Contain(Enum.GetNames(typeof(Color)));
		}

		#endregion
	}
}
