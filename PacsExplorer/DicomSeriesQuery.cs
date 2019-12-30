using Dicom;
using DicomScu;

namespace PacsExplorer
{
    public class DicomSeriesQuery : IDicomQuery
    {
        public void CopyTo(DicomDataset dataset)
        {
            dataset.AddOrUpdate(DicomTag.Modality, "");
            dataset.AddOrUpdate(DicomTag.SeriesDescription, "");
            dataset.AddOrUpdate(DicomTag.NumberOfSeriesRelatedInstances, "");
        }
    }
}
