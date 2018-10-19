using NLog;
using WinlogbeatFirehoseProxy.Api;

namespace WinlogbeatFirehoseProxy.Console {

	internal static class Program {

		private static readonly ILogger m_logger = NLog.LogManager.GetCurrentClassLogger();

		internal static int Main( string[] arguments ) {

			try {
				if( !ProgramArgs.TryParse( arguments, out ProgramArgs args ) ) {
					ProgramArgs.WriteOptions( System.Console.Out );
					return 160;
				}

				using( HttpApplication.Start( args ) ) {
					System.Console.ReadLine();
					return 0;
				}

			} finally {
				LogManager.Shutdown();
			}
		}
	}
}
