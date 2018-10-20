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
using WinlogbeatFirehoseProxy.Kinesis;

namespace WinlogbeatFirehoseProxy.Api {

	public sealed class BulkController : ApiController {

		internal const long MaxRecordSize = ( 1024 * 1024 ) - 256;
		internal const long MaxBatchRequestSize = 3 * 1024 * 1024; // 3 MB
		internal static readonly Encoding Utf8Encoding = new UTF8Encoding( encoderShouldEmitUTF8Identifier: false );

		private static readonly ILogger m_log = NLog.LogManager.GetCurrentClassLogger();

		private readonly ProgramArgs m_args;
		private readonly IFirehoseClientProvider m_firehoseProvider;

		public BulkController(
				ProgramArgs args,
				IFirehoseClientProvider firehoseProvider
			) {

			m_args = args;
			m_firehoseProvider = firehoseProvider;
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

					var batchRequest = new PutRecordBatchRequest {
						DeliveryStreamName = m_args.DeliveryStreamName,
						Records = records
					};

					try {
						PutRecordBatchResponse response = await firehose
							.PutRecordBatchAsync( batchRequest );

						LogEventInfo log = new LogEventInfo {
							Properties = {
								{ "Response", response }
							}
						};

						int failedPutCount = response.FailedPutCount;
						if( failedPutCount == records.Count ) {

							log.Level = LogLevel.Error;
							log.Message = "All records failed to put";

						} else if( failedPutCount > 0 ) {

							log.Level = LogLevel.Warn;
							log.Message = "Some records failed to put";

						} else {

							log.Level = LogLevel.Debug;
							log.Message = "Successfully put all records";
						}

						m_log.Log( log );

					} catch( Exception err ) {
						m_log.Error( err, "Failed to put record batch." );

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

		private static async Task<List<Record>> ReadRecordsAsync( StreamReader reader ) {

			List<Record> records = new List<Record>();

			long bytes = 0;
			for( int i = 0; i < 500; i++ ) {

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

				if( ms.Length <= MaxRecordSize ) {

					records.Add( new Record { Data = ms } );
					bytes += ms.Length;
				}

				if( bytes > MaxBatchRequestSize ) {
					break;
				}
			}

			return records;
		}
	}
}
