using System;
using System.ServiceProcess;
using NLog;
using WinlogbeatFirehoseProxy.Api;

namespace WinlogbeatFirehoseProxy {

	public partial class HttpWindowsService : ServiceBase {

		private static readonly ILogger m_log = NLog.LogManager.GetCurrentClassLogger();

		private readonly ProgramArgs m_args;
		private IDisposable m_httpApp;

		public HttpWindowsService( ProgramArgs args ) {
			m_args = args;

			InitializeComponent();
		}

		protected override void OnStart( string[] arguments ) {

			try {
				m_httpApp = HttpApplication.Start( m_args );

			} catch( Exception err ) {
				m_log.Error( err, "Failed to start http application" );
				this.ExitCode = 574;
				this.Stop();
			}
		}

		protected override void OnStop() {

			try {
				m_httpApp?.Dispose();
			} catch( Exception err ) {
				m_log.Error( err, "Failed to dispose http application" );
			}

			LogManager.Shutdown();
		}
	}
}
