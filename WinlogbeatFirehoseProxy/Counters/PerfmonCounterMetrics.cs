
namespace WinlogbeatFirehoseProxy.Counters {

	internal sealed class PerfmonCounterMetrics : IMetrics {

		private const string Category = "Winlogbeat Firehose Proxy";

		private readonly PerfmonCounter m_batchRequests = CreatePerfmonCounter( "PutRecordBatch Requests" );
		private readonly PerfmonCounter m_batchRequestErrors = CreatePerfmonCounter( "PutRecordBatch Errors" );

		private readonly PerfmonCounter m_recordsPut = CreatePerfmonCounter( "Records Put" );
		private readonly PerfmonCounter m_recordsFailed = CreatePerfmonCounter( "Records Failed" );
		private readonly PerfmonCounter m_recordsDropped = CreatePerfmonCounter( "Records Dropped" );

		public static readonly IMetrics Instance = new PerfmonCounterMetrics();
		private PerfmonCounterMetrics() { }

		void IMetrics.IncrementPutRecordBatchRequests() {
			m_batchRequests.IncrementBy( 1 );
		}

		void IMetrics.IncrementPutRecordBatchErrors() {
			m_batchRequestErrors.IncrementBy( 1 );
		}

		void IMetrics.IncrementRecordsPut( int count ) {
			m_recordsPut.IncrementBy( count );
		}

		void IMetrics.IncrementRecordsFailed( int count ) {
			m_recordsFailed.IncrementBy( count );
		}

		void IMetrics.IncrementRecordsDropped() {
			m_recordsDropped.IncrementBy( 1 );
		}

		private static PerfmonCounter CreatePerfmonCounter( string counterName ) {
			return new PerfmonCounter( Category, counterName );
		}
	}
}
