using Dicom;
using System;
using System.IO;

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
            if (dataset.TryGetSingleValue(DicomTag.NumberOfStudyRelatedInstances, out int instanceCount))
            {
                InstanceCount = instanceCount;
            }
        }

        public DicomFile CreateEncapsulatedPdf(string filePath)
        {
            var dataset = new DicomDataset();

            dataset.AddOrUpdate(DicomTag.PatientName, PatientName);
            dataset.AddOrUpdate(DicomTag.PatientID, PatientId);
            dataset.AddOrUpdate(DicomTag.AccessionNumber, AccessionNumber);
            if (Date != null)
            {
                dataset.AddOrUpdate(DicomTag.StudyDate, Date.Value);
            }
            dataset.AddOrUpdate(DicomTag.StudyDescription, Description);
            dataset.AddOrUpdate(DicomTag.StudyInstanceUID, Uid);

            dataset.AddOrUpdate(DicomTag.Modality, "DOC");
            dataset.AddOrUpdate(DicomTag.ConversionType, "WSD");
            dataset.AddOrUpdate(DicomTag.SeriesInstanceUID, DicomUID.Generate());
            dataset.AddOrUpdate(DicomTag.SOPClassUID, DicomUID.EncapsulatedPDFStorage);
            dataset.AddOrUpdate(DicomTag.SOPInstanceUID, DicomUID.Generate());

            dataset.AddOrUpdate(DicomTag.DocumentTitle, Path.GetFileNameWithoutExtension(filePath));
            dataset.AddOrUpdate(DicomTag.MIMETypeOfEncapsulatedDocument, "application/pdf");
            dataset.AddOrUpdate(DicomTag.EncapsulatedDocument, File.ReadAllBytes(filePath));

            return new DicomFile(dataset);
        }

        public string PatientName { get; }

        public string PatientId { get; }

        public string AccessionNumber { get; }

        public string Modality { get; }

        public DateTime? Date { get; }

        public string Description { get; }

        public string Uid { get; }

        public int? InstanceCount { get; set; }
    }
}
