using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Xunit;

namespace SAL.Flatbed.Tests
{
	public class PluginMethodInfoTests
	{
		private sealed class Stub
		{
			public String StringValue { get; } = "hello";

			public void VoidNoArgs() { }
			public String ReturnString() => this.StringValue;
			public Int32 Add(Int32 a, Int32 b) => a + b;
			public void ThrowingMethod() => throw new InvalidOperationException("boom");
			public void OutParam(out Int32 value) { value = 42; }
			public void ArrayInput(Int32[] values) { }
		}

		private static MethodInfo GetMethod(String name)
			=> typeof(Stub).GetMethod(name);

		private static PluginMethodInfo Create(String methodName, Object target)
			=> new PluginMethodInfo(GetMethod(methodName), target, null);

		#region Constructor

		[Fact]
		public void Constructor_NullMethod_ThrowsArgumentNullException()
		{
			Action act = () => new PluginMethodInfo(null, new Stub(), null);
			act.Should().Throw<ArgumentNullException>().WithParameterName("member");
		}

		[Fact]
		public void Constructor_NullTargetAndNullParent_ThrowsArgumentException()
		{
			Action act = () => new PluginMethodInfo(GetMethod(nameof(Stub.VoidNoArgs)), null, null);
			act.Should().Throw<ArgumentException>();
		}

		[Fact]
		public void Constructor_ValidMethodAndTarget_DoesNotThrow()
		{
			Action act = () => new PluginMethodInfo(GetMethod(nameof(Stub.VoidNoArgs)), new Stub(), null);
			act.Should().NotThrow();
		}

		[Fact]
		public void Constructor_NullTargetWithValidParent_DoesNotThrow()
		{
			var parent = Create(nameof(Stub.VoidNoArgs), new Stub());
			Action act = () => new PluginMethodInfo(GetMethod(nameof(Stub.ReturnString)), null, parent);
			act.Should().NotThrow();
		}

		#endregion

		#region Count

		[Fact]
		public void Count_MethodWithNoParameters_ReturnsZero()
		{
			var sut = Create(nameof(Stub.VoidNoArgs), new Stub());
			sut.Count.Should().Be(0);
		}

		[Fact]
		public void Count_MethodWithTwoParameters_ReturnsTwo()
		{
			var sut = Create(nameof(Stub.Add), new Stub());
			sut.Count.Should().Be(2);
		}

		#endregion

		#region ReturnType

		[Fact]
		public void ReturnType_VoidMethod_ReturnsNull()
		{
			var sut = Create(nameof(Stub.VoidNoArgs), new Stub());
			sut.ReturnType.Should().BeNull();
		}

		[Fact]
		public void ReturnType_NonVoidMethod_ReturnsTypeInfo()
		{
			var sut = Create(nameof(Stub.ReturnString), new Stub());
			sut.ReturnType.Should().NotBeNull();
		}

		[Fact]
		public void ReturnType_NonVoidMethod_TypeNameMatchesReturnType()
		{
			var sut = Create(nameof(Stub.ReturnString), new Stub());
			sut.ReturnType.TypeName.Should().Be(typeof(String).FullName);
		}

		#endregion

		#region GetParameters

		[Fact]
		public void GetParameters_MethodWithNoParameters_ReturnsEmpty()
		{
			var sut = Create(nameof(Stub.VoidNoArgs), new Stub());
			sut.GetParameters().Should().BeEmpty();
		}

		[Fact]
		public void GetParameters_MethodWithTwoParameters_ReturnsTwoItems()
		{
			var sut = Create(nameof(Stub.Add), new Stub());
			sut.GetParameters().Should().HaveCount(2);
		}

		[Fact]
		public void GetParameters_MethodWithTwoParameters_NamesMatch()
		{
			var sut = Create(nameof(Stub.Add), new Stub());
			IEnumerable<String> names = sut.GetParameters().Select(p => p.Name);
			names.Should().Equal("a", "b");
		}

		[Fact]
		public void GetParameters_OutParameter_IsOutReturnsTrue()
		{
			var sut = Create(nameof(Stub.OutParam), new Stub());
			sut.GetParameters().Single().IsOut.Should().BeTrue();
		}

		[Fact]
		public void GetParameters_ArrayParameter_IsArrayReturnsTrue()
		{
			var sut = Create(nameof(Stub.ArrayInput), new Stub());
			sut.GetParameters().Single().IsArray.Should().BeTrue();
		}

		[Fact]
		public void GetParameters_EachParameter_MethodReferencePointsBackToInfo()
		{
			var sut = Create(nameof(Stub.Add), new Stub());
			foreach(IPluginParameterInfo param in sut.GetParameters())
				param.Method.Should().BeSameAs(sut);
		}

		#endregion

		#region Invoke

		[Fact]
		public void Invoke_VoidMethod_ReturnsNull()
		{
			var sut = Create(nameof(Stub.VoidNoArgs), new Stub());
			sut.Invoke().Should().BeNull();
		}

		[Fact]
		public void Invoke_MethodWithReturnValue_ReturnsValue()
		{
			var stub = new Stub();
			var sut = Create(nameof(Stub.ReturnString), stub);
			sut.Invoke().Should().Be(stub.StringValue);
		}

		[Fact]
		public void Invoke_MethodWithParameters_PassesArgumentsCorrectly()
		{
			var sut = Create(nameof(Stub.Add), new Stub());
			sut.Invoke(3, 4).Should().Be(7);
		}

		[Fact]
		public void Invoke_NullTarget_ReturnsNull()
		{
			// Arrange: use a property parent whose getter returns null so GetTarget() returns null
			var stub = new PluginMethodInfoTests.NullPropertyStub();
			PropertyInfo prop = typeof(NullPropertyStub).GetProperty(nameof(NullPropertyStub.NullValue));
			var parent = new PluginPropertyInfo(prop, stub, null);
			var sut = new PluginMethodInfo(GetMethod(nameof(Stub.ReturnString)), null, parent);
			sut.Invoke().Should().BeNull();
		}

		[Fact]
		public void Invoke_ThrowingMethod_ExceptionContainsMethodNameAndTypeName()
		{
			var sut = Create(nameof(Stub.ThrowingMethod), new Stub());
			Action act = () => sut.Invoke();
			act.Should().Throw<Exception>()
				.Which.Data.Contains("MethodName").Should().BeTrue();
		}

		[Fact]
		public void Invoke_ThrowingMethod_ExceptionDataContainsTypeName()
		{
			var sut = Create(nameof(Stub.ThrowingMethod), new Stub());
			Action act = () => sut.Invoke();
			act.Should().Throw<Exception>()
				.Which.Data.Contains("TypeName").Should().BeTrue();
		}

		#endregion

		/// <summary>Helper stub whose property returns null, allowing GetTarget() to resolve to null.</summary>
		public sealed class NullPropertyStub
		{
			public Object NullValue => null;
		}
	}
}
