using System.IO;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WinlogbeatFirehoseProxy.Api {

	public static class JsonHttpContent {

		private static readonly Encoding DefaultEncoding = new UTF8Encoding( encoderShouldEmitUTF8Identifier: false );
		private const string DefaultMediaType = "application/json";

		public static JsonHttpContent<T> Create<T>(
				T content
			) {

			return new JsonHttpContent<T>(
				content,
				DefaultEncoding,
				DefaultMediaType,
				new JsonSerializer()
			);
		}
	}

	public sealed class JsonHttpContent<T> : System.Net.Http.HttpContent {

		private readonly Encoding m_encoding;
		private readonly JsonSerializer m_serializer;

		internal JsonHttpContent(
				T content,
				Encoding encoding,
				string mediaType,
				JsonSerializer serializer
			) {

			Headers.ContentType = new MediaTypeHeaderValue( mediaType ) {
				CharSet = encoding.WebName
			};

			Content = content;
			m_encoding = encoding;
			m_serializer = serializer;
		}

		public T Content { get; }

		protected override bool TryComputeLength( out long length ) {
			length = default( long );
			return false;
		}

		protected override Task SerializeToStreamAsync( Stream stream, TransportContext context ) {
			SerializeTo( stream );
			return Task.CompletedTask;
		}

		private void SerializeTo( Stream stream ) {

			using( StreamWriter sw = new StreamWriter(
					stream: stream,
					encoding: m_encoding,
					bufferSize: 1024,
					leaveOpen: true
				) ) {

				using( JsonTextWriter jsonW = new JsonTextWriter( sw ) ) {
					jsonW.CloseOutput = false;

					m_serializer.Serialize( jsonW, Content );
				}
			}
		}
	}
}