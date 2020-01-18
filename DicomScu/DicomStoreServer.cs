using Dicom.Network;
using System;

namespace DicomScu
{
    class DicomStoreServer : DicomServer<DicomStoreService>
    {
        public string AeTitle { get; set; }
        
        public Func<DicomCStoreRequest, DicomStatus> OnCStoreRequest { get; set; }
        
        protected override DicomStoreService CreateScp(INetworkStream stream)
        {
            var scp = base.CreateScp(stream);
            scp.AeTitle = AeTitle;
            scp.OnCStoreRequest = OnCStoreRequest;
            return scp;
        }
    }
}
