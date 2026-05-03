using System;
using System.Diagnostics;

namespace SAL.Flatbed
{
	/// <summary>Abstraction over a trace/logging sink, compatible with all .NET versions</summary>
	public interface ITraceSource
	{
		/// <summary>Writes a trace event to the trace listeners</summary>
		/// <param name="eventType">Trace event type</param>
		/// <param name="id">Numeric event identifier</param>
		/// <param name="message">Message to write</param>
		void TraceEvent(TraceEventType eventType, Int32 id, String message);

		/// <summary>Writes a formatted trace event to the trace listeners</summary>
		/// <param name="eventType">Trace event type</param>
		/// <param name="id">Numeric event identifier</param>
		/// <param name="format">Message format string</param>
		/// <param name="args">Format arguments</param>
		void TraceEvent(TraceEventType eventType, Int32 id, String format, params Object[] args);

		/// <summary>Writes trace data to the trace listeners</summary>
		/// <param name="eventType">Trace event type</param>
		/// <param name="id">Numeric event identifier</param>
		/// <param name="data">Data object to write</param>
		void TraceData(TraceEventType eventType, Int32 id, Object data);
	}
}