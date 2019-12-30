using Dicom;
using System;

namespace PacsExplorer
{
    class DicomStudy
    {
        public DicomStudy(DicomDataset dataset)
        {
            PatientName = dataset.GetSingleValueOrDefault(DicomTag.PatientName, "");
            PatientId = dataset.GetSingleValueOrDefault(DicomTag.PatientID, "");
            AccessionNumber = dataset.GetSingleValueOrDefault(DicomTag.AccessionNumber, "");
            Modality = dataset.GetSingleValueOrDefault(DicomTag.ModalitiesInStudy, "");
            if (dataset.TryGetSingleValue(DicomTag.StudyDate, out DateTime date))
            {
                Date = date;
            }
            Description = dataset.GetSingleValueOrDefault(DicomTag.StudyDescription, "");
            Uid = dataset.GetSingleValueOrDefault(DicomTag.StudyInstanceUID, "");
            if (dataset.TryGetSingleValue(DicomTag.NumberOfStudyRelatedInstances, out int imageCount))
            {
                ImageCount = imageCount;
            }
        }

        public string PatientName { get; }

        public string PatientId { get; }

        public string AccessionNumber { get; }

        public string Modality { get; }

        public DateTime? Date { get; }

        public string Description { get; }

        public string Uid { get; }

        public int? ImageCount { get; set; }
    }
}
