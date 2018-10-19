using System.ServiceProcess;

namespace WinlogbeatFirehoseProxy {

	internal static class Program {

		internal static void Main( string[] args ) {

			ServiceBase.Run( new[] {
				new HttpWindowsService()
			} );
		}
	}
}
