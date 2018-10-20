using System.Collections.Generic;
using System.IO;
using NDesk.Options;
using NLog;

namespace WinlogbeatFirehoseProxy {

	public sealed class ProgramArgs {

		private static readonly ILogger m_log = NLog.LogManager.GetCurrentClassLogger();

		private static readonly OptionSet<ProgramArgs> m_options =
			new OptionSet<ProgramArgs>()
			.Add(
				prototype: "deliveryStreamName=",
				description: "The name of the firehose delivery stream",
				action: ( args, value ) => { args.DeliveryStreamName = value; }
			)
			.Add(
				prototype: "regionName=",
				description: "The aws region of the firehose delivery stream (optional)",
				action: ( args, value ) => { args.RegionName = value; }
			)
			.Add(
				prototype: "roleArn=",
				description: "The role to assume to via STS (optional)",
				action: ( args, value ) => { args.RoleArn = value; }
			)
			.Add(
				prototype: "port=",
				description: "The port to listen on (defaults to 9200)",
				action: ( args, value ) => { args.Port = int.Parse( value ); }
			);

		public static bool TryParse(
				IEnumerable<string> arguments,
				out ProgramArgs args
			) {

			args = m_options.Parse( arguments, out List<string> extras );

			if( extras.Count > 0 ) {
				m_log.Error( "Unkonwn arguments provided: {0}", string.Join( " ", extras ) );
				args = null;
				return false;
			}

			if( string.IsNullOrEmpty( args.DeliveryStreamName ) ) {
				m_log.Error( "DeliveryStreamName is a required. Provide --deliveryStreamName argument." );
				args = null;
				return false;
			}

			return true;
		}

		public static void WriteOptions( TextWriter writer ) {
			m_options.WriteOptionDescriptions( writer );
		}

		public string DeliveryStreamName { get; private set; }
		public string RegionName { get; private set; }
		public string RoleArn { get; private set; }
		public int Port { get; private set; } = 9200;
	}
}
