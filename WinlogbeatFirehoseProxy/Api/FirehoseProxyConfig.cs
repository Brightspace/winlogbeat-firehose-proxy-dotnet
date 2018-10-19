namespace WinlogbeatFirehoseProxy.Api {

	public sealed class FirehoseProxyConfig {

		public FirehoseProxyConfig(
				string deliveryStreamName,
				string regionName,
				string roleArn
			) {

			DeliveryStreamName = deliveryStreamName;
			RegionName = regionName;
			RoleArn = roleArn;
		}

		public string DeliveryStreamName { get; }
		public string RegionName { get; }
		public string RoleArn { get; }
	}
}
