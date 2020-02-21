using Dicom;
using DicomScu;
using System;

namespace PacsExplorer
{
    public class DicomStudyQuery : IDicomQuery
    {
        public string ComponentName { get; set; } = "";

        public string ComponentId { get; set; } = "";

        public string AccessionNumber { get; set; } = "";

        public string Modality { get; set; } = "";

        public DateTime StartDate { get; set; } = DateTime.Today.AddYears(-1);

        public DateTime EndDate { get; set; } = DateTime.Today;

        public string Description { get; set; } = "";

        public void CopyTo(DicomDataset dataset)
        {
            dataset.AddOrUpdate(DicomTag.PatientName, ComponentName);
            dataset.AddOrUpdate(DicomTag.PatientID, ComponentId);
            dataset.AddOrUpdate(DicomTag.AccessionNumber, AccessionNumber);
            dataset.AddOrUpdate(DicomTag.ModalitiesInStudy, Modality);
            dataset.AddOrUpdate(DicomTag.StudyDate, new DicomDateRange(StartDate, EndDate));
            dataset.AddOrUpdate(DicomTag.StudyDescription, Description);
            dataset.AddOrUpdate(DicomTag.NumberOfStudyRelatedInstances, "");
        }
    }
}
