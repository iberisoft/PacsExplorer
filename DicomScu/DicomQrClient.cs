using Dicom;
using Dicom.Network;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DicomClient = Dicom.Network.Client.DicomClient;

namespace DicomScu
{
    public class DicomQrClient
    {
        readonly DicomClient m_Client;

        public DicomQrClient(string serverHost, int serverPort, string serverAeTitle, string clientAeTitle)
        {
            m_Client = new DicomClient(serverHost, serverPort, false, clientAeTitle, serverAeTitle);
            m_Client.NegotiateAsyncOps();

            var presentationContexts = DicomPresentationContext.GetScpRolePresentationContextsFromStorageUids(DicomStorageCategory.Image, DicomTransferSyntax.ExplicitVRLittleEndian,
                DicomTransferSyntax.ImplicitVRLittleEndian, DicomTransferSyntax.ImplicitVRBigEndian);
            m_Client.AdditionalPresentationContexts.AddRange(presentationContexts);
        }

        public static DicomCFindRequest CreateStudyQueryRequest(IDicomQuery query)
        {
            var request = new DicomCFindRequest(DicomQueryRetrieveLevel.Study);
            query?.CopyTo(request.Dataset);
            request.Dataset.AddOrUpdate(DicomTag.StudyInstanceUID, "");
            return request;
        }

        public async Task<IEnumerable<DicomDataset>> QueryAsync(DicomCFindRequest request)
        {
            var datasets = new List<DicomDataset>();
            request.OnResponseReceived += (_, response) =>
            {
                if (response.Status == DicomStatus.Pending)
                {
                    datasets.Add(response.Dataset);
                }
            };

            await m_Client.AddRequestAsync(request);
            await m_Client.SendAsync();

            return datasets;
        }

        public static DicomCGetRequest CreateStudyGetRequest(string studyUid) => new DicomCGetRequest(studyUid);

        public async Task RetrieveAsync(DicomCGetRequest request, Func<DicomDataset, Task<bool>> storeHandler)
        {
            async Task<DicomCStoreResponse> cStoreHandler(DicomCStoreRequest cStoreRequest)
            {
                var success = await storeHandler(cStoreRequest.Dataset);
                return new DicomCStoreResponse(cStoreRequest, success ? DicomStatus.Success : DicomStatus.QueryRetrieveUnableToPerformSuboperations);
            }
            m_Client.OnCStoreRequest += cStoreHandler;

            try
            {
                await m_Client.AddRequestAsync(request);
                await m_Client.SendAsync();
            }
            finally
            {
                m_Client.OnCStoreRequest -= cStoreHandler;
            }
        }

        public static DicomCMoveRequest CreateStudyMoveRequest(string studyUid, string destinationAeTitle) => new DicomCMoveRequest(destinationAeTitle, studyUid);

        public async Task RetrieveAsync(DicomCMoveRequest request, Func<DicomDataset, Task<bool>> storeHandler, int destinationPort)
        {
            var server = (DicomStoreServer)DicomServer.Create<DicomStoreService, DicomStoreServer>(null, destinationPort);
            server.AeTitle = request.DestinationAE;
            server.OnCStoreRequest = cStoreRequest => storeHandler(cStoreRequest.Dataset).Result ? DicomStatus.Success : DicomStatus.QueryRetrieveUnableToPerformSuboperations;
            request.OnResponseReceived += (_, response) =>
            {
                if (response.Status == DicomStatus.Success)
                {
                    server.Dispose();
                }
            };

            await m_Client.AddRequestAsync(request);
            await m_Client.SendAsync();
        }
    }
}
