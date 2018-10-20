using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Amazon.KinesisFirehose;
using Amazon.KinesisFirehose.Model;
using NLog;
using WinlogbeatFirehoseProxy.Counters;
using WinlogbeatFirehoseProxy.Kinesis;

namespace WinlogbeatFirehoseProxy.Api {

	public sealed class BulkController : ApiController {

		private static readonly ILogger m_log = NLog.LogManager.GetCurrentClassLogger();

		internal const int MaxBatchRecords = 500;
		internal const long MaxRecordSize = ( 1024 * 1024 ) - 256;
		internal const long MaxBatchRequestSize = 3 * 1024 * 1024; // 3 MB (less than actual batch for simplicity)
		internal static readonly Encoding Utf8Encoding = new UTF8Encoding( encoderShouldEmitUTF8Identifier: false );

		private readonly ProgramArgs m_args;
		private readonly IFirehoseClientProvider m_firehoseProvider;
		private readonly IMetrics m_metrics;

		public BulkController(
				ProgramArgs args,
				IFirehoseClientProvider firehoseProvider,
				IMetrics metrics
			) {

			m_args = args;
			m_firehoseProvider = firehoseProvider;
			m_metrics = metrics;
		}

		[HttpPost]
		[Route( "_bulk" )]
		public async Task<HttpResponseMessage> PostAsync( HttpRequestMessage request ) {

			Stopwatch watch = new Stopwatch();
			bool hadErrors = false;

			using( IAmazonKinesisFirehose firehose = m_firehoseProvider.Create() )
			using( Stream content = await request.Content.ReadAsStreamAsync() )
			using( StreamReader reader = new StreamReader( content ) ) {

				for(; ; ) {

					List<Record> records = await ReadRecordsAsync( reader );
					if( records.Count == 0 ) {
						break;
					}

					bool published = await TryPublishRecordsAsync( firehose, records );
					if( !published ) {
						hadErrors = true;
					}
				}
			}

			watch.Stop();

			var result = new {
				took = watch.ElapsedMilliseconds,
				errors = hadErrors,
				items = Enumerable.Empty<object>()
			};

			return new HttpResponseMessage( HttpStatusCode.OK ) {
				Content = JsonHttpContent.Create( result )
			};
		}

		private async Task<List<Record>> ReadRecordsAsync( StreamReader reader ) {

			List<Record> records = new List<Record>();

			long bytes = 0;
			for( int i = 0; i < MaxBatchRecords; i++ ) {

				string action = await reader.ReadLineAsync();
				if( action == null ) {
					break;
				}

				string document = await reader.ReadLineAsync();
				if( document == null ) {
					break;
				}

				MemoryStream ms = new MemoryStream();
				using( StreamWriter sw = new StreamWriter( ms, Utf8Encoding, 1024, true ) ) {
					sw.Write( document );
				}

				if( ms.Length > MaxRecordSize ) {
					m_log.Error( "Dropping log because it exceeds max record size: {0}", document );
					m_metrics.IncrementRecordsDropped();

				} else {
					records.Add( new Record { Data = ms } );
					bytes += ms.Length;
				}

				if( bytes > MaxBatchRequestSize ) {
					break;
				}
			}

			return records;
		}

		private async Task<bool> TryPublishRecordsAsync(
				IAmazonKinesisFirehose firehose,
				List<Record> records
			) {

			m_metrics.IncrementPutRecordBatchRequests();

			try {
				int recordCount = records.Count;

				var batchRequest = new PutRecordBatchRequest {
					DeliveryStreamName = m_args.DeliveryStreamName,
					Records = records
				};

				PutRecordBatchResponse response = await firehose
					.PutRecordBatchAsync( batchRequest );

				LogEventInfo log = new LogEventInfo {
					Properties = {
						{ "Response", response }
					}
				};

				int failedPutCount = response.FailedPutCount;
				if( failedPutCount == recordCount ) {

					log.Level = LogLevel.Error;
					log.Message = "Failed to put all records in batch.";

					m_metrics.IncrementRecordsFailed( failedPutCount );

				} else if( failedPutCount > 0 ) {

					log.Level = LogLevel.Warn;
					log.Message = "Failed to put some records in batch.";

					m_metrics.IncrementRecordsPut( recordCount - failedPutCount );
					m_metrics.IncrementRecordsFailed( failedPutCount );

				} else {

					log.Level = LogLevel.Debug;
					log.Message = "Successfully put all records.";

					m_metrics.IncrementRecordsPut( recordCount );
				}

				m_log.Log( log );
				return true;

			} catch( Exception err ) {
				m_log.Error( err, "Failed to put record batch." );
				m_metrics.IncrementPutRecordBatchErrors();
				return false;
			}
		}
	}
}
