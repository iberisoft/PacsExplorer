using Dicom;

namespace PacsExplorer
{
    class DicomSeries
    {
        public DicomSeries(DicomDataset dataset)
        {
            Modality = dataset.GetSingleValueOrDefault(DicomTag.Modality, "");
            Number = dataset.GetSingleValueOrDefault(DicomTag.SeriesNumber, "");
            Description = dataset.GetSingleValueOrDefault(DicomTag.SeriesDescription, "");
            Uid = dataset.GetSingleValueOrDefault(DicomTag.SeriesInstanceUID, "");
            if (dataset.TryGetSingleValue(DicomTag.NumberOfSeriesRelatedInstances, out int instanceCount))
            {
                InstanceCount = instanceCount;
            }
        }

        public string Modality { get; }

        public string Number { get; set; }

        public string Description { get; }

        public string Uid { get; }

        public int? InstanceCount { get; set; }
    }
}
