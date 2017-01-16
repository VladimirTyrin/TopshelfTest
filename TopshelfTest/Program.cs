using System;
using ITCC.Logging.Core;
using ITCC.Logging.Core.Loggers;
using ITCC.Logging.Windows.Loggers;
using Topshelf;

namespace TopshelfTest
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            InitLoggers();

            HostFactory.Run(x =>                                 
            {
                x.Service<ServerController>(s =>                        
                {
                    s.ConstructUsing(name => new ServerController());     
                    s.WhenStarted(controller => controller.Start());              
                    s.WhenStopped(controller => controller.Stop());               
                });
                x.RunAsLocalSystem();      
                                      
                x.SetDescription("Sample Topshelf Host");        
                x.SetDisplayName("Topshelf test");                       
                x.SetServiceName("Topshelf test");

                x.EnableServiceRecovery(configurator =>
                {
                    configurator.RestartService(0);
                });

                x.OnException(exception => Logger.LogException("ERROR", LogLevel.Error, exception));
            });
        }

        private static void InitLoggers()
        {
            Logger.Level = LogLevel.Trace;

            if (Environment.UserInteractive)
                Logger.RegisterReceiver(new ColouredConsoleLogger());

            Logger.RegisterReceiver(new DebugLogger());
        }
    }
}
