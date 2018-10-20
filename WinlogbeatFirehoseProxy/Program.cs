using System.ServiceProcess;
using NLog;

namespace WinlogbeatFirehoseProxy {

	internal static class Program {

		internal static int Main( string[] arguments ) {

			try {
				if( !ProgramArgs.TryParse( arguments, out ProgramArgs args ) ) {
					return 160;
				}

				ServiceBase.Run( new[] {
					new HttpWindowsService( args )
				} );

				return 0;

			} finally {
				LogManager.Shutdown();
			}
		}
	}
}
