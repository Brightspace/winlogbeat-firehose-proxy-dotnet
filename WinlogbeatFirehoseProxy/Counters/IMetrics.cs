namespace WinlogbeatFirehoseProxy.Counters {

	public interface IMetrics {

		void IncrementPutRecordBatchRequests();
		void IncrementPutRecordBatchErrors();

		void IncrementRecordsPut( int count );
		void IncrementRecordsFailed( int count );
		void IncrementRecordsDropped();
	}
}
