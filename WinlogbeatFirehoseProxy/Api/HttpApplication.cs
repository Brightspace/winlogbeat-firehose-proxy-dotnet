using System;
using System.Web.Http;
using Microsoft.Owin.Hosting;
using Ninject;
using Ninject.Web.Common.OwinHost;
using Ninject.Web.WebApi.OwinHost;
using NLog;
using WinlogbeatFirehoseProxy.Counters;
using WinlogbeatFirehoseProxy.Kinesis;

namespace WinlogbeatFirehoseProxy.Api {

	public sealed class HttpApplication {

		private static readonly ILogger m_log = NLog.LogManager.GetCurrentClassLogger();

		public static IDisposable Start( ProgramArgs args ) {

			StartOptions options = new StartOptions { Port = args.Port };

			IDisposable server = WebApp.Start( options, appBuilder => {

				appBuilder.UseNinjectMiddleware( () => {

					StandardKernel kernel = new StandardKernel();

					kernel
						.Bind<ProgramArgs>()
						.ToConstant( args )
						.InSingletonScope();

					kernel
						.Bind<IFirehoseClientProvider>()
						.ToMethod( ctxt => FirehoseClientProviderFactory.Create( args ) )
						.InSingletonScope();

					kernel
						.Bind<IMetrics>()
						.ToConstant( PerfmonCounterMetrics.Instance )
						.InSingletonScope();

					return kernel;
				} );

				HttpConfiguration appConfig = new HttpConfiguration();
				appConfig.MapHttpAttributeRoutes();

				appBuilder.UseNinjectWebApi( appConfig );
			} );

			m_log.Info( "Listening on port {0}", args.Port );

			return server;
		}
	}
}
