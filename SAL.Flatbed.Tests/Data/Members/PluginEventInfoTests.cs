using System;
using System.Reflection;
using FluentAssertions;
using Xunit;

namespace SAL.Flatbed.Tests
{
	public class PluginEventInfoTests
	{
		private sealed class StubWithEvent
		{
			public event EventHandler<DataEventArgs> TestEvent;

			public void Raise() => this.TestEvent?.Invoke(this, DataEventArgs.Empty);
		}

		private sealed class StubWithNullProperty
		{
			public Object NullValue => null;
		}

		private static EventInfo GetTestEventInfo()
			=> typeof(StubWithEvent).GetEvent(nameof(StubWithEvent.TestEvent));

		private static PluginEventInfo CreateWithTarget(StubWithEvent target)
			=> new PluginEventInfo(GetTestEventInfo(), target, null);

		/// <summary>Returns a PluginEventInfo whose GetTarget() resolves to null (via a parent property that returns null)</summary>
		private static PluginEventInfo CreateWithNullResolvedTarget()
		{
			var stub = new StubWithNullProperty();
			PropertyInfo prop = typeof(StubWithNullProperty).GetProperty(nameof(StubWithNullProperty.NullValue));
			var parent = new PluginPropertyInfo(prop, stub, null);
			return new PluginEventInfo(GetTestEventInfo(), null, parent);
		}

		#region Constructor

		[Fact]
		public void Constructor_NullEventInfo_ThrowsArgumentNullException()
		{
			Action act = () => new PluginEventInfo(null, new StubWithEvent(), null);
			act.Should().Throw<ArgumentNullException>().WithParameterName("member");
		}

		[Fact]
		public void Constructor_NullTargetAndNullParent_ThrowsArgumentException()
		{
			Action act = () => new PluginEventInfo(GetTestEventInfo(), null, null);
			act.Should().Throw<ArgumentException>();
		}

		[Fact]
		public void Constructor_ValidEventAndTarget_DoesNotThrow()
		{
			Action act = () => new PluginEventInfo(GetTestEventInfo(), new StubWithEvent(), null);
			act.Should().NotThrow();
		}

		[Fact]
		public void Constructor_NullTargetWithValidParent_DoesNotThrow()
		{
			var parent = new PluginEventInfo(GetTestEventInfo(), new StubWithEvent(), null);
			Action act = () => new PluginEventInfo(GetTestEventInfo(), null, parent);
			act.Should().NotThrow();
		}

		#endregion

		#region AddEventHandler

		[Fact]
		public void AddEventHandler_NullHandler_ThrowsArgumentNullException()
		{
			var sut = CreateWithTarget(new StubWithEvent());
			Action act = () => sut.AddEventHandler(null);
			act.Should().Throw<ArgumentNullException>().WithParameterName("handler");
		}

		[Fact]
		public void AddEventHandler_NullResolvedTarget_ThrowsInvalidOperationException()
		{
			var sut = CreateWithNullResolvedTarget();
			EventHandler<DataEventArgs> handler = (s, e) => { };
			Action act = () => sut.AddEventHandler(handler);
			act.Should().Throw<InvalidOperationException>();
		}

		[Fact]
		public void AddEventHandler_ValidHandler_HandlerIsInvoked()
		{
			var stub = new StubWithEvent();
			var sut = CreateWithTarget(stub);
			Boolean called = false;
			EventHandler<DataEventArgs> handler = (s, e) => called = true;

			sut.AddEventHandler(handler);
			stub.Raise();

			called.Should().BeTrue();
		}

		#endregion

		#region RemoveEventHandler

		[Fact]
		public void RemoveEventHandler_NullHandler_ThrowsArgumentNullException()
		{
			var sut = CreateWithTarget(new StubWithEvent());
			Action act = () => sut.RemoveEventHandler(null);
			act.Should().Throw<ArgumentNullException>().WithParameterName("handler");
		}

		[Fact]
		public void RemoveEventHandler_NullResolvedTarget_ThrowsInvalidOperationException()
		{
			var sut = CreateWithNullResolvedTarget();
			EventHandler<DataEventArgs> handler = (s, e) => { };
			Action act = () => sut.RemoveEventHandler(handler);
			act.Should().Throw<InvalidOperationException>();
		}

		[Fact]
		public void RemoveEventHandler_AttachedHandler_HandlerIsNoLongerInvoked()
		{
			var stub = new StubWithEvent();
			var sut = CreateWithTarget(stub);
			Int32 callCount = 0;
			EventHandler<DataEventArgs> handler = (s, e) => callCount++;

			sut.AddEventHandler(handler);
			stub.Raise();

			sut.RemoveEventHandler(handler);
			stub.Raise();

			callCount.Should().Be(1);
		}

		[Fact]
		public void RemoveEventHandler_UnattachedHandler_DoesNotThrow()
		{
			var stub = new StubWithEvent();
			var sut = CreateWithTarget(stub);
			EventHandler<DataEventArgs> handler = (s, e) => { };

			Action act = () => sut.RemoveEventHandler(handler);
			act.Should().NotThrow();
		}

		#endregion
	}
}
