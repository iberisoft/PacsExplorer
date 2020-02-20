using Dicom.Network;
using System;
using System.Threading.Tasks;
using DicomClient = Dicom.Network.Client.DicomClient;

namespace DicomScu
{
    public abstract class DicomBaseClient
    {
        public DicomBaseClient(string serverHost, int serverPort, string serverAeTitle, string clientAeTitle)
        {
            CreateClient = () =>
            {
                var client = new DicomClient(serverHost, serverPort, false, clientAeTitle, serverAeTitle);
                client.NegotiateAsyncOps();
                return client;
            };
        }

        protected Func<DicomClient> CreateClient { get; }

        public async Task VerifyAsync()
        {
            var client = CreateClient();
            var request = new DicomCEchoRequest();
            await client.AddRequestAsync(request);
            await client.SendAsync();
        }
    }
}
