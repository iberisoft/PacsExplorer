using Microsoft.Extensions.Configuration;

namespace PacsExplorer
{
    class Settings
    {
        public Settings(string filePath) => Config = new ConfigurationBuilder().AddJsonFile(filePath).Build();

        private IConfiguration Config { get; }

        public string ServerHost => Config["Server:Host"];

        public string ServerAeTitle => Config["Server:AeTitle"];

        public int ServerPort => int.Parse(Config["Server:Port"]);

        public string ClientAeTitle => Config["Client:AeTitle"];
    }
}
