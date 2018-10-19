using Amazon;
using Amazon.KinesisFirehose;
using Amazon.Runtime;

namespace WinlogbeatFirehoseProxy.Kinesis {

	public sealed class FirehoseClientProvider : IFirehoseClientProvider {

		private readonly AWSCredentials m_credentials;
		private readonly RegionEndpoint m_region;

		public FirehoseClientProvider(
				AWSCredentials credentials,
				RegionEndpoint region
			) {

			m_credentials = credentials;
			m_region = region;
		}

		IAmazonKinesisFirehose IFirehoseClientProvider.Create() {
			return new AmazonKinesisFirehoseClient( m_credentials, m_region );
		}
	}
}
