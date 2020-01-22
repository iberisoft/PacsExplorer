namespace PacsGateway
{
    class Settings
    {
        public ServerSettings QrServer { get; set; }

        public ClientSettings Client { get; set; }

        public string ImageViewerPath { get; set; }

        public string StoragePath { get; set; }

        public class ServerSettings
        {
            public string Host { get; set; }

            public string AeTitle { get; set; }

            public int Port { get; set; }
        }

        public class ClientSettings
        {
            public string AeTitle { get; set; }

            public int Port { get; set; }
        }
    }
}
