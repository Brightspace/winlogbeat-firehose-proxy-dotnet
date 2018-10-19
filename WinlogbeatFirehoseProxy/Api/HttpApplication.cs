using System;
using System.Web.Http;
using Amazon;
using Amazon.Runtime;
using Microsoft.Owin.Hosting;
using Ninject;
using Ninject.Web.Common.OwinHost;
using Ninject.Web.WebApi.OwinHost;
using WinlogbeatFirehoseProxy.Kinesis;

namespace WinlogbeatFirehoseProxy.Api {

	public sealed class HttpApplication {

		public static IDisposable Start( FirehoseProxyConfig config ) {

			return WebApp.Start( "http://localhost:9200", appBuilder => {

				appBuilder.UseNinjectMiddleware( () => {

					StandardKernel kernel = new StandardKernel();

					kernel
						.Bind<FirehoseProxyConfig>()
						.ToConstant( config );

					kernel
						.Bind<IFirehoseClientProvider>()
						.ToMethod( ctxt => CreateFirehoseClientFactory( config ) );

					return kernel;
				} );

				HttpConfiguration appConfig = new HttpConfiguration();
				appConfig.MapHttpAttributeRoutes();

				appBuilder.UseNinjectWebApi( appConfig );
			} );
		}

		private static IFirehoseClientProvider CreateFirehoseClientFactory( FirehoseProxyConfig config ) {

			AWSCredentials rootCredentials = FallbackCredentialsFactory
				.GetCredentials( fallbackToAnonymous: true );

			AWSCredentials roleCredentials = new AssumeRoleAWSCredentials(
					sourceCredentials: rootCredentials,
					roleArn: config.RoleArn,
					roleSessionName: "WinlogbeatFirehoseProxy-" + Environment.MachineName,
					options: new AssumeRoleAWSCredentialsOptions() {
						DurationSeconds = 3600
					}
				);

			RegionEndpoint regionEndpoint;
			if( String.IsNullOrEmpty( config.RegionName ) ) {
				regionEndpoint = FallbackRegionFactory.GetRegionEndpoint( true );
			} else {
				regionEndpoint = RegionEndpoint.GetBySystemName( config.RegionName );
			}

			IFirehoseClientProvider firehoseFactory = new FirehoseClientProvider(
					roleCredentials,
					regionEndpoint
				);

			return firehoseFactory;
		}
	}
}
