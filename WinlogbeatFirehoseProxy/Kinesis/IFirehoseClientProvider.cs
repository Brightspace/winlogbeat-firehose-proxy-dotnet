using Amazon.KinesisFirehose;

namespace WinlogbeatFirehoseProxy.Kinesis {

	public interface IFirehoseClientProvider {

		IAmazonKinesisFirehose Create();
	}
}
