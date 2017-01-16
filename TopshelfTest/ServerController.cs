using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using ITCC.HTTP.Common.Enums;
using ITCC.HTTP.Server.Core;
using ITCC.HTTP.Server.Enums;
using ITCC.HTTP.Server.Files;
using ITCC.HTTP.SslConfigUtil.Core.Enums;
using ITCC.Logging.Core;
using Newtonsoft.Json;
using Server = ITCC.HTTP.Server.Core.StaticServer<object>;
using ServerConfiguration = ITCC.HTTP.Server.Core.HttpServerConfiguration<object>;

namespace TopshelfTest
{
    internal class ServerController
    {
        #region public

        public void Start()
        {
            var config = GetConfig();
            var startStatus = Server.Start(config);
            if (startStatus != ServerStartStatus.Ok)
            {
                Logger.LogEntry("START", LogLevel.Error, $"Error starting server: {startStatus}");
                return;
            }
            RegisterHandlers();
            Logger.LogEntry("START", LogLevel.Info, "Server started");
        }

        public void Stop()
        {
            Server.Stop();
            Logger.LogEntry("START", LogLevel.Info, "Server stopped");
        }
        #endregion

        #region private

        private void RegisterHandlers()
        {
            Server.AddRequestProcessor(new RequestProcessor<object>
            {
                AuthorizationRequired = false,
                Handler = (account, request) => Task.FromResult(new HandlerResult(HttpStatusCode.OK, "Hello world")),
                Method = HttpMethod.Get,
                SubUri = "hello"
            });
            Server.AddRequestProcessor(new RequestProcessor<object>
            {
                AuthorizationRequired = false,
                Handler = (account, request) =>
                {
                    throw new NotImplementedException();
                },
                Method = HttpMethod.Get,
                SubUri = "error"
            });
        }

        private ServerConfiguration GetConfig() => new ServerConfiguration
        {
            Port = 8888,
            Protocol = Protocol.Http,
            AllowGeneratedCertificates = true,
            CertificateBindType = BindType.SubjectName,
            LogBodyReplacePatterns = new List<Tuple<string, string>>
            {
                new Tuple<string, string>("(\"Token\":\")([\\w\\d]+)(\")", "$1REMOVED_FROM_LOG$3")
            },
            LogProhibitedQueryParams = new List<string> { "password" },
            LogProhibitedHeaders = new List<string> { "Authorization" },
            ServerName = "ITCC Test",
            StatisticsEnabled = true,
            SubjectName = "localhost",
            FilesEnabled = true,
            FilesNeedAuthorization = false,
            FilesBaseUri = "files",
            FileSections = new List<FileSection>
            {
                new FileSection
                {
                    Folder = "Test",
                    MaxFileSize = -1,
                    Name = "Test"
                }
            },
            FilesLocation = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Pictures",
            FilesPreprocessingEnabled = false,
            FilesCompressionEnabled = false,
            FilesPreprocessorThreads = -1,
            BodyEncoders = new List<BodyEncoder>
            {
                new BodyEncoder
                {
                    AutoGzipCompression = true,
                    ContentType = "application/xml",
                    Encoding = Encoding.UTF8,
                    Serializer = o =>
                    {
                        using (var stringWriter = new StringWriter())
                        {
                            using (var xmlWriter = XmlWriter.Create(stringWriter))
                            {
                                var xmlSerializer = new XmlSerializer(o.GetType());
                                xmlSerializer.Serialize(xmlWriter, o);
                            }
                            return stringWriter.ToString();
                        }
                    },
                    IsDefault = false
                },
                new BodyEncoder
                {
                    AutoGzipCompression = true,
                    ContentType = "application/json",
                    Encoding = Encoding.UTF8,
                    Serializer = o => JsonConvert.SerializeObject(o,
                        new JsonSerializerSettings {ReferenceLoopHandling = ReferenceLoopHandling.Serialize}),
                    IsDefault = true
                },
                new BodyEncoder
                {
                    AutoGzipCompression = false,
                    ContentType = "text/plain",
                    Encoding = Encoding.UTF8,
                    Serializer = o => o.ToString(),
                    IsDefault = false
                }
            },
            CriticalMemoryValue = 1024
        };

        #endregion
    }
}
