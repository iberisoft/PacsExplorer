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
        }

        public static DicomCFindRequest CreateStudyQueryRequest(IDicomQuery query)
        {
            var request = new DicomCFindRequest(DicomQueryRetrieveLevel.Study);
            query?.CopyTo(request.Dataset);
            request.Dataset.AddOrUpdate(DicomTag.StudyInstanceUID, "");
            return request;
        }

        public static DicomCFindRequest CreateSeriesQueryRequest(string studyUid, IDicomQuery query)
        {
            var request = new DicomCFindRequest(DicomQueryRetrieveLevel.Series);
            request.Dataset.AddOrUpdate(DicomTag.StudyInstanceUID, studyUid);
            query?.CopyTo(request.Dataset);
            request.Dataset.AddOrUpdate(DicomTag.SeriesInstanceUID, "");
            return request;
        }

        public static DicomCFindRequest CreateImageQueryRequest(string seriesUid, IDicomQuery query)
        {
            var request = new DicomCFindRequest(DicomQueryRetrieveLevel.Image);
            request.Dataset.AddOrUpdate(DicomTag.SeriesInstanceUID, seriesUid);
            query?.CopyTo(request.Dataset);
            request.Dataset.AddOrUpdate(DicomTag.SOPInstanceUID, "");
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

        public static DicomCGetRequest CreateStudyRetrieveRequest(string studyUid) => new DicomCGetRequest(studyUid);

        public static DicomCGetRequest CreateSeriesRetrieveRequest(string studyUid, string seriesUid) => new DicomCGetRequest(studyUid, seriesUid);

        public async Task RetrieveAsync(DicomCGetRequest request, Func<DicomDataset, Task<bool>> storeHandler)
        {
            async Task<DicomCStoreResponse> cStoreHandler(DicomCStoreRequest cStoreRequest)
            {
                var success = await storeHandler(cStoreRequest.Dataset);
                return new DicomCStoreResponse(cStoreRequest, success ? DicomStatus.Success : DicomStatus.QueryRetrieveUnableToPerformSuboperations);
            }
            m_Client.OnCStoreRequest += cStoreHandler;

            var presentationContexts = DicomPresentationContext.GetScpRolePresentationContextsFromStorageUids(DicomStorageCategory.Image, DicomTransferSyntax.ExplicitVRLittleEndian,
                DicomTransferSyntax.ImplicitVRLittleEndian, DicomTransferSyntax.ImplicitVRBigEndian);
            m_Client.AdditionalPresentationContexts.AddRange(presentationContexts);

            await m_Client.AddRequestAsync(request);
            await m_Client.SendAsync();

            m_Client.OnCStoreRequest -= cStoreHandler;
        }
    }
}
