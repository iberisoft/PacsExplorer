namespace PacsExplorer
{
    class Settings
    {
        public ServerSettings Server { get; set; }

        public ClientSettings Client { get; set; }

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
