using System;
using System.Diagnostics;

namespace SAL.Flatbed
{
	/// <summary>Wraps <see cref="TraceSource"/> behind <see cref="ITraceSource"/></summary>
	internal sealed class TraceSourceAdapter : ITraceSource
	{
		private readonly TraceSource _source;

		/// <param name="name">Trace source name</param>
		/// <exception cref="ArgumentNullException"><paramref name="name"/> is null or empty</exception>
		public TraceSourceAdapter(String name)
		{
			if(String.IsNullOrEmpty(name))
				throw new ArgumentNullException(nameof(name));

			this._source = new TraceSource(name);
			this._source.Switch.Level = SourceLevels.All;
			this._source.Listeners.Remove("Default");
			this._source.Listeners.AddRange(Trace.Listeners);
		}

		/// <inheritdoc/>
		void ITraceSource.TraceEvent(TraceEventType eventType, Int32 id, String message)
			=> this._source.TraceEvent(eventType, id, message);

		/// <inheritdoc/>
		void ITraceSource.TraceEvent(TraceEventType eventType, Int32 id, String format, params Object[] args)
			=> this._source.TraceEvent(eventType, id, format, args);

		/// <inheritdoc/>
		void ITraceSource.TraceData(TraceEventType eventType, Int32 id, Object data)
			=> this._source.TraceData(eventType, id, data);
	}
}