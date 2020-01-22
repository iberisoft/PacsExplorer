using Dicom;
using DicomScu;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PacsGateway
{
    public class DicomService
    {
        readonly Settings m_Settings;
        readonly DicomQrClient m_DicomQrClient;

        public DicomService(IConfiguration config)
        {
            m_Settings = config.Get<Settings>();
            m_DicomQrClient = new DicomQrClient(m_Settings.QrServer.Host, m_Settings.QrServer.Port, m_Settings.QrServer.AeTitle, m_Settings.Client.AeTitle);
        }

        public async Task<int> GetSeriesCount(string id)
        {
            var request = DicomQrClient.CreateSeriesQueryRequest(id, null);
            var series = await m_DicomQrClient.QueryAsync(request);
            return series.Count();
        }

        public async Task OpenStudy(string id, bool move)
        {
            DeleteFolder(id);
            if (!move)
            {
                var request = DicomQrClient.CreateStudyGetRequest(id);
                await m_DicomQrClient.RetrieveAsync(request, Save);
            }
            else
            {
                var request = DicomQrClient.CreateStudyMoveRequest(id, m_Settings.Client.AeTitle);
                await m_DicomQrClient.RetrieveAsync(request, Save, m_Settings.Client.Port);
            }
            OpenFolder(id);
        }

        private async Task<bool> Save(DicomDataset dataset)
        {
            if (Directory.Exists(m_Settings.StoragePath))
            {
                var file = new DicomFile(dataset);
                var filePath = Path.Combine(m_Settings.StoragePath, dataset.GetString(DicomTag.StudyInstanceUID), dataset.GetString(DicomTag.SOPInstanceUID) + ".dcm");
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                await file.SaveAsync(filePath);
                return true;
            }
            else
            {
                return false;
            }
        }

        private void OpenFolder(string id)
        {
            if (Directory.Exists(m_Settings.StoragePath))
            {
                var folderPath = Path.Combine(m_Settings.StoragePath, id);
                Process.Start(File.Exists(m_Settings.ImageViewerPath) ? m_Settings.ImageViewerPath : "explorer", folderPath);
            }
        }

        private void DeleteFolder(string id)
        {
            if (Directory.Exists(m_Settings.StoragePath))
            {
                var folderPath = Path.Combine(m_Settings.StoragePath, id);
                if (Directory.Exists(folderPath))
                {
                    Directory.Delete(folderPath, true);
                }
            }
        }
    }
}
