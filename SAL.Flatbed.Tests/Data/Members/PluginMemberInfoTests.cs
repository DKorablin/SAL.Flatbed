using System;
using System.Reflection;
using FluentAssertions;
using Xunit;

namespace SAL.Flatbed.Tests
{
	public class PluginMemberInfoTests
	{
		private sealed class TestablePluginMemberInfo : PluginMemberInfo
		{
			public TestablePluginMemberInfo(MemberInfo member, Object target, PluginMemberInfo parent)
				: base(member, target, parent) { }

			public new Object GetTarget() => base.GetTarget();
		}

		private sealed class Stub
		{
			public Object Result { get; } = new Object();
			public Object GetResult() => this.Result;
		}

		private static TestablePluginMemberInfo CreateWithTarget(MemberInfo member = null, Object target = null)
		{
			member ??= typeof(String);
			target ??= new Object();
			return new TestablePluginMemberInfo(member, target, null);
		}

		#region Constructor

		[Fact]
		public void Constructor_NullMember_ThrowsArgumentNullException()
		{
			Action act = () => new TestablePluginMemberInfo(null, new Object(), null);
			act.Should().Throw<ArgumentNullException>().WithParameterName("member");
		}

		[Fact]
		public void Constructor_NullTargetAndNullParent_ThrowsArgumentException()
		{
			Action act = () => new TestablePluginMemberInfo(typeof(String), null, null);
			act.Should().Throw<ArgumentException>();
		}

		[Fact]
		public void Constructor_ValidMemberAndTarget_DoesNotThrow()
		{
			Action act = () => new TestablePluginMemberInfo(typeof(String), new Object(), null);
			act.Should().NotThrow();
		}

		[Fact]
		public void Constructor_NullTargetWithValidParent_DoesNotThrow()
		{
			var parent = CreateWithTarget();
			Action act = () => new TestablePluginMemberInfo(typeof(String), null, parent);
			act.Should().NotThrow();
		}

		#endregion

		#region Name

		[Fact]
		public void Name_WithType_ReturnsTypeName()
		{
			var sut = CreateWithTarget(typeof(String));
			sut.Name.Should().Be(nameof(String));
		}

		[Fact]
		public void Name_WithPropertyInfo_ReturnsPropertyName()
		{
			PropertyInfo prop = typeof(String).GetProperty(nameof(String.Length));
			var sut = CreateWithTarget(prop);
			sut.Name.Should().Be(nameof(String.Length));
		}

		#endregion

		#region TypeName

		[Fact]
		public void TypeName_MemberIsType_ReturnsTypeFullName()
		{
			var sut = CreateWithTarget(typeof(String));
			sut.TypeName.Should().Be(typeof(String).FullName);
		}

		[Fact]
		public void TypeName_MemberIsProperty_ReturnsReflectedTypeFullName()
		{
			PropertyInfo prop = typeof(String).GetProperty(nameof(String.Length));
			var sut = CreateWithTarget(prop);
			sut.TypeName.Should().Be(typeof(String).FullName);
		}

		#endregion

		#region AssemblyQualifiedName

		[Fact]
		public void AssemblyQualifiedName_MemberIsType_ReturnsTypeAssemblyQualifiedName()
		{
			var sut = CreateWithTarget(typeof(String));
			sut.AssemblyQualifiedName.Should().Be(typeof(String).AssemblyQualifiedName);
		}

		[Fact]
		public void AssemblyQualifiedName_MemberIsProperty_ReturnsReflectedTypeAssemblyQualifiedName()
		{
			PropertyInfo prop = typeof(String).GetProperty(nameof(String.Length));
			var sut = CreateWithTarget(prop);
			sut.AssemblyQualifiedName.Should().Be(typeof(String).AssemblyQualifiedName);
		}

		#endregion

		#region MemberType

		[Fact]
		public void MemberType_WithType_ReturnsTypeInfo()
		{
			var sut = CreateWithTarget(typeof(String));
			sut.MemberType.Should().Be(MemberTypes.TypeInfo);
		}

		[Fact]
		public void MemberType_WithProperty_ReturnsProperty()
		{
			PropertyInfo prop = typeof(String).GetProperty(nameof(String.Length));
			var sut = CreateWithTarget(prop);
			sut.MemberType.Should().Be(MemberTypes.Property);
		}

		#endregion

		#region InstanceOf<T>

		[Fact]
		public void InstanceOf_Generic_TargetImplementsType_ReturnsTrue()
		{
			var stub = new Stub();
			var sut = new TestablePluginMemberInfo(typeof(String), stub, null);
			sut.InstanceOf<Stub>().Should().BeTrue();
		}

		[Fact]
		public void InstanceOf_Generic_TargetDoesNotImplementType_ReturnsFalse()
		{
			var stub = new Stub();
			var sut = new TestablePluginMemberInfo(typeof(String), stub, null);
			sut.InstanceOf<String>().Should().BeFalse();
		}

		[Fact]
		public void InstanceOf_Generic_NullTarget_ReturnsFalse()
		{
			var parent = CreateWithTarget();
			var sut = new TestablePluginMemberInfo(typeof(String), null, parent);
			sut.InstanceOf<Stub>().Should().BeFalse();
		}

		#endregion

		#region InstanceOf(Type)

		[Fact]
		public void InstanceOf_Type_NullType_ThrowsArgumentNullException()
		{
			var sut = CreateWithTarget();
			Action act = () => sut.InstanceOf(null);
			act.Should().Throw<ArgumentNullException>().WithParameterName("type");
		}

		[Fact]
		public void InstanceOf_Type_NullTarget_ReturnsFalse()
		{
			var parent = CreateWithTarget();
			var sut = new TestablePluginMemberInfo(typeof(String), null, parent);
			sut.InstanceOf(typeof(Object)).Should().BeFalse();
		}

		[Fact]
		public void InstanceOf_Type_MemberIsNotType_ReturnsFalse()
		{
			PropertyInfo prop = typeof(String).GetProperty(nameof(String.Length));
			var sut = new TestablePluginMemberInfo(prop, new Object(), null);
			sut.InstanceOf(typeof(Object)).Should().BeFalse();
		}

		[Fact]
		public void InstanceOf_Type_MemberTypeImplementsInterface_ReturnsTrue()
		{
			var sut = new TestablePluginMemberInfo(typeof(String), new Object(), null);
			sut.InstanceOf(typeof(IComparable)).Should().BeTrue();
		}

		[Fact]
		public void InstanceOf_Type_MemberTypeDoesNotImplementInterface_ReturnsFalse()
		{
			var sut = new TestablePluginMemberInfo(typeof(String), new Object(), null);
			sut.InstanceOf(typeof(IDisposable)).Should().BeFalse();
		}

		#endregion

		#region GetTarget

		[Fact]
		public void GetTarget_WithTarget_ReturnsTarget()
		{
			var expected = new Object();
			var sut = new TestablePluginMemberInfo(typeof(String), expected, null);
			sut.GetTarget().Should().BeSameAs(expected);
		}

		[Fact]
		public void GetTarget_NullTargetWithPropertyParent_ReturnsPropertyValue()
		{
			var stub = new Stub();
			PropertyInfo prop = typeof(Stub).GetProperty(nameof(Stub.Result));
			var parent = new PluginPropertyInfo(prop, stub, null);
			var sut = new TestablePluginMemberInfo(typeof(String), null, parent);
			sut.GetTarget().Should().BeSameAs(stub.Result);
		}

		[Fact]
		public void GetTarget_NullTargetWithMethodParent_ReturnsMethodResult()
		{
			var stub = new Stub();
			MethodInfo method = typeof(Stub).GetMethod(nameof(Stub.GetResult));
			var parent = new PluginMethodInfo(method, stub, null);
			var sut = new TestablePluginMemberInfo(typeof(String), null, parent);
			sut.GetTarget().Should().BeSameAs(stub.Result);
		}

		[Fact]
		public void GetTarget_NullTargetWithEventParent_ThrowsInvalidOperationException()
		{
			EventInfo evt = typeof(Console).GetEvent(nameof(Console.CancelKeyPress));
			var parent = new PluginEventInfo(evt, new Object(), null);
			var sut = new TestablePluginMemberInfo(typeof(String), null, parent);
			Action act = () => sut.GetTarget();
			act.Should().Throw<InvalidOperationException>();
		}

		[Fact]
		public void GetTarget_NullTargetWithTypeParent_DelegatesToParentGetTarget()
		{
			var expected = new Object();
			var grandParent = new PluginTypeInfo(typeof(String), expected, null);
			var sut = new TestablePluginMemberInfo(typeof(Int32), null, grandParent);
			sut.GetTarget().Should().BeSameAs(expected);
		}

		#endregion
	}
}
