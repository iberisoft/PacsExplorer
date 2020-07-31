using Dicom;
using DicomScu;

namespace PacsExplorer
{
    public class DicomSeriesQuery : IDicomQuery
    {
        public string Modality { get; set; } = "";

        public string Number { get; set; } = "";

        public string Description { get; set; } = "";

        public void CopyTo(DicomDataset dataset)
        {
            dataset.AddOrUpdate(DicomTag.Modality, Modality);
            dataset.AddOrUpdate(DicomTag.SeriesNumber, Number);
            dataset.AddOrUpdate(DicomTag.SeriesDescription, Description);
            dataset.AddOrUpdate(DicomTag.NumberOfSeriesRelatedInstances, "");
        }
    }
}
