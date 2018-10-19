using System;
using System.ServiceProcess;
using WinlogbeatFirehoseProxy.Api;

namespace WinlogbeatFirehoseProxy {

	public partial class HttpWindowsService : ServiceBase {

		private IDisposable m_httpApp;

		public HttpWindowsService() {
			InitializeComponent();
		}

		protected override void OnStart( string[] args ) {

			FirehoseProxyConfig config = new FirehoseProxyConfig(
				deliveryStreamName: "win-event-log",
				regionName: "us-east-1",
				roleArn: "arn:aws:iam::857609596104:role/win_event_log_publisher"
			);

			m_httpApp = HttpApplication.Start( config );
		}

		protected override void OnStop() {
			m_httpApp.Dispose();
		}
	}
}
