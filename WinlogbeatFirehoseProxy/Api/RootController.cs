using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace WinlogbeatFirehoseProxy.Api {

	public sealed class RootController : ApiController {

		[Route]
		public HttpResponseMessage Get() {

			Version version = typeof( BulkController ).Assembly.GetName().Version;

			var info = new {
				name = "winlogbeat-firehose-proxy",
				version = new {
					number = version.ToString()
				}
			};

			return new HttpResponseMessage( HttpStatusCode.OK ) {
				Content = JsonHttpContent.Create( info )
			};
		}
	}
}
