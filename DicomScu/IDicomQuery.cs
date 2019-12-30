using Dicom;

namespace DicomScu
{
    public interface IDicomQuery
    {
        void CopyTo(DicomDataset dataset);
    }
}
