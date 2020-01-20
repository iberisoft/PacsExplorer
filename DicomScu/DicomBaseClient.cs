using System;
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
    }
}
