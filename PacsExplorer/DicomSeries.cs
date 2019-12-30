using Dicom;

namespace PacsExplorer
{
    class DicomSeries
    {
        public DicomSeries(DicomDataset dataset)
        {
            Modality = dataset.GetSingleValueOrDefault(DicomTag.Modality, "");
            Description = dataset.GetSingleValueOrDefault(DicomTag.SeriesDescription, "");
            Uid = dataset.GetSingleValueOrDefault(DicomTag.SeriesInstanceUID, "");
            if (dataset.TryGetSingleValue(DicomTag.NumberOfSeriesRelatedInstances, out int imageCount))
            {
                ImageCount = imageCount;
            }
        }

        public string Modality { get; }

        public string Description { get; }

        public string Uid { get; }

        public int? ImageCount { get; set; }
    }
}
