using Dicom;
using Dicom.Network;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DicomScu
{
    public class DicomStoreClient : DicomBaseClient
    {
        public DicomStoreClient(string serverHost, int serverPort, string serverAeTitle, string clientAeTitle)
            : base(serverHost, serverPort, serverAeTitle, clientAeTitle) { }

        public async Task StoreAsync(IEnumerable<DicomFile> files)
        {
            var client = CreateClient();
            foreach (var file in files)
            {
                var request = new DicomCStoreRequest(file);
                await client.AddRequestAsync(request);
            }
            await client.SendAsync();
        }
    }
}
