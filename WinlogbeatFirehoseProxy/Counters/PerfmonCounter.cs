using System;
using System.Diagnostics;
using NLog;

namespace WinlogbeatFirehoseProxy.Counters {

	internal sealed class PerfmonCounter {

		private static readonly ILogger m_log = NLog.LogManager.GetCurrentClassLogger();

		private readonly string m_counterCategory;
		private readonly string m_counterName;

		public PerfmonCounter(
				string counterCategory,
				string counterName
			) {

			m_counterCategory = counterCategory;
			m_counterName = counterName;
		}

		public void IncrementBy( long count ) {

			try {
				using( PerformanceCounter perfmonCounter = new PerformanceCounter(
						categoryName: m_counterCategory,
						counterName: m_counterName,
						readOnly: false
					) ) {

					perfmonCounter.IncrementBy( count );
				}

			} catch( Exception err ) {
				m_log.Error( err, $"Failed to increment counter \\{ m_counterCategory }\\{ m_counterName }" );
			}
		}
	}
}
