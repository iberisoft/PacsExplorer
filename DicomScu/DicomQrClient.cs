using Dicom;
using Dicom.Network;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DicomScu
{
    public class DicomQrClient : DicomBaseClient
    {
        public DicomQrClient(string serverHost, int serverPort, string serverAeTitle, string clientAeTitle)
            : base(serverHost, serverPort, serverAeTitle, clientAeTitle) { }

        public static DicomCFindRequest CreateStudyQueryRequest(IDicomQuery query)
        {
            var request = new DicomCFindRequest(DicomQueryRetrieveLevel.Study);
            request.Dataset.AddOrUpdate(DicomTag.StudyInstanceUID, "");
            query?.CopyTo(request.Dataset);
            return request;
        }

        public static DicomCFindRequest CreateSeriesQueryRequest(string studyUid, IDicomQuery query)
        {
            var request = new DicomCFindRequest(DicomQueryRetrieveLevel.Series);
            request.Dataset.AddOrUpdate(DicomTag.StudyInstanceUID, studyUid);
            request.Dataset.AddOrUpdate(DicomTag.SeriesInstanceUID, "");
            query?.CopyTo(request.Dataset);
            return request;
        }

        public async Task<List<DicomDataset>> QueryAsync(DicomCFindRequest request)
        {
            var datasets = new List<DicomDataset>();
            request.OnResponseReceived += (_, response) =>
            {
                if (response.Status == DicomStatus.Pending)
                {
                    datasets.Add(response.Dataset);
                }
            };

            var client = CreateClient();
            await client.AddRequestAsync(request);
            await client.SendAsync();

            return datasets;
        }

        public static DicomCGetRequest CreateStudyGetRequest(string studyUid) => new DicomCGetRequest(studyUid);

        public static DicomCGetRequest CreateSeriesGetRequest(string studyUid, string seriesUid) => new DicomCGetRequest(studyUid, seriesUid);

        public async Task RetrieveAsync(DicomCGetRequest request, Func<DicomDataset, Task<bool>> storeHandler)
        {
            async Task<DicomCStoreResponse> cStoreHandler(DicomCStoreRequest cStoreRequest)
            {
                var success = await storeHandler(cStoreRequest.Dataset);
                return new DicomCStoreResponse(cStoreRequest, success ? DicomStatus.Success : DicomStatus.QueryRetrieveUnableToPerformSuboperations);
            }

            var client = CreateClient();
            var presentationContexts = DicomPresentationContext.GetScpRolePresentationContextsFromStorageUids(DicomStorageCategory.Image, DicomTransferSyntax.ExplicitVRLittleEndian,
                DicomTransferSyntax.ImplicitVRLittleEndian, DicomTransferSyntax.ImplicitVRBigEndian);
            client.AdditionalPresentationContexts.AddRange(presentationContexts);
            presentationContexts = DicomPresentationContext.GetScpRolePresentationContextsFromStorageUids(DicomStorageCategory.Document, DicomTransferSyntax.ExplicitVRLittleEndian,
                DicomTransferSyntax.ImplicitVRLittleEndian, DicomTransferSyntax.ImplicitVRBigEndian);
            client.AdditionalPresentationContexts.AddRange(presentationContexts);
            try
            {
                client.OnCStoreRequest += cStoreHandler;
                await client.AddRequestAsync(request);
                await client.SendAsync();
            }
            finally
            {
                client.OnCStoreRequest -= cStoreHandler;
            }
        }

        public static DicomCMoveRequest CreateStudyMoveRequest(string studyUid, string destinationAeTitle) => new DicomCMoveRequest(destinationAeTitle, studyUid);
        
        public static DicomCMoveRequest CreateSeriesMoveRequest(string studyUid, string seriesUid, string destinationAeTitle) => new DicomCMoveRequest(destinationAeTitle, studyUid, seriesUid);

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

            var client = CreateClient();
            await client.AddRequestAsync(request);
            await client.SendAsync();
        }
    }
}
