using NLog;
using WinlogbeatFirehoseProxy.Api;

namespace WinlogbeatFirehoseProxy.Console {

	internal static class Program {

		private static readonly ILogger m_logger = NLog.LogManager.GetCurrentClassLogger();

		internal static void Main( string[] args ) {

			try {
				FirehoseProxyConfig config = new FirehoseProxyConfig(
					deliveryStreamName: "win-event-log",
					regionName: "us-east-1",
					roleArn: "arn:aws:iam::857609596104:role/win_event_log_publisher"
				);

				using( HttpApplication.Start( config ) ) {
				
					m_logger.Info( "Listening at http://localhost:9200" );
					System.Console.ReadLine();
				}

			} finally {
				LogManager.Shutdown();
			}
		}
	}
}
