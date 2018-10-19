using System;
using Amazon;
using Amazon.Runtime;

namespace WinlogbeatFirehoseProxy.Kinesis {

	internal static class FirehoseClientProviderFactory {

		public static IFirehoseClientProvider Create( ProgramArgs args ) {

			AWSCredentials roleCredentials = CreateAWSCredentials( args );
			RegionEndpoint regionEndpoint = GetRegionEndpoint( args );

			IFirehoseClientProvider firehoseFactory = new FirehoseClientProvider(
					roleCredentials,
					regionEndpoint
				);

			return firehoseFactory;
		}

		private static AWSCredentials CreateAWSCredentials( ProgramArgs args ) {

			AWSCredentials rootCredentials = FallbackCredentialsFactory
				.GetCredentials( fallbackToAnonymous: true );

			if( String.IsNullOrEmpty( args.RoleArn ) ) {
				return rootCredentials;
			}

			return new AssumeRoleAWSCredentials(
					sourceCredentials: rootCredentials,
					roleArn: args.RoleArn,
					roleSessionName: "WinlogbeatFirehoseProxy-" + Environment.MachineName,
					options: new AssumeRoleAWSCredentialsOptions {
						DurationSeconds = 3600
					}
				);
		}

		private static RegionEndpoint GetRegionEndpoint( ProgramArgs args ) {

			if( String.IsNullOrEmpty( args.RegionName ) ) {
				return FallbackRegionFactory.GetRegionEndpoint( includeInstanceMetadata: true );
			}

			return RegionEndpoint.GetBySystemName( args.RegionName );
		}
	}
}
