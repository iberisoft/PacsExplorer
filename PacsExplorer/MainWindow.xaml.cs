using Dicom;
using DicomScu;
using Microsoft.Extensions.Configuration;
using PacsExplorer.Converters;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace PacsExplorer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly Settings m_Settings;

        public MainWindow()
        {
            InitializeComponent();

            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            m_Settings = config.Get<Settings>();

            DataContext = this;
        }

        public DicomStudyQuery StudyQuery { get; set; } = new DicomStudyQuery();

        public string StoragePath { get; set; } = @"D:\PacsExplorer";

        private void Studies_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyName == nameof(DicomStudy.Date))
            {
                var column = (DataGridBoundColumn)e.Column;
                var binding = (Binding)column.Binding;
                binding.Converter = new DateConverter();
            }
        }

        DicomQrClient m_DicomQrClient;

        private async void Find(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            button.Focus();

            if (m_DicomQrClient == null)
            {
                m_DicomQrClient = new DicomQrClient(m_Settings.Server.Host, m_Settings.Server.Port, m_Settings.Server.AeTitle, m_Settings.Client.AeTitle);
            }

            await DoWork(async () =>
            {
                var request = DicomQrClient.CreateStudyQueryRequest(StudyQuery);
                var datasets = await m_DicomQrClient.QueryAsync(request);
                Studies.ItemsSource = datasets.Select(dataset => new DicomStudy(dataset)).OrderByDescending(study => study.Date);
            });
        }

        private async void OpenStudy(object sender, RoutedEventArgs e)
        {
            var study = (DicomStudy)Studies.SelectedItem;
            RetrievingProgress.Value = 0;
            RetrievingProgress.Maximum = study.ImageCount ?? 0;

            await DoWork(async () =>
            {
                DeleteFolder(study);
                if (CGetOption.IsChecked == true)
                {
                    var request = DicomQrClient.CreateStudyGetRequest(study.Uid);
                    await m_DicomQrClient.RetrieveAsync(request, Save);
                }
                else
                {
                    var request = DicomQrClient.CreateStudyMoveRequest(study.Uid, m_Settings.Client.AeTitle);
                    await m_DicomQrClient.RetrieveAsync(request, Save, m_Settings.Client.Port);
                }
                OpenFolder(study);
            });
        }

        private async Task DoWork(Func<Task> action)
        {
            try
            {
                IsEnabled = false;
                await action();
            }
            finally
            {
                IsEnabled = true;
            }
        }

        private async Task<bool> Save(DicomDataset dataset)
        {
            Dispatcher.Invoke(() => ++RetrievingProgress.Value);

            if (Directory.Exists(StoragePath))
            {
                var file = new DicomFile(dataset);
                var filePath = Path.Combine(StoragePath, dataset.GetString(DicomTag.StudyInstanceUID), dataset.GetString(DicomTag.SOPInstanceUID) + ".dcm");
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                await file.SaveAsync(filePath);
                return true;
            }
            else
            {
                return false;
            }
        }

        private void OpenFolder(DicomStudy study)
        {
            if (Directory.Exists(StoragePath))
            {
                var folderPath = Path.Combine(StoragePath, study.Uid);
                Process.Start(File.Exists(m_Settings.ImageViewerPath) ? m_Settings.ImageViewerPath : "explorer", folderPath);
            }
        }

        private void DeleteFolder(DicomStudy study)
        {
            if (Directory.Exists(StoragePath))
            {
                var folderPath = Path.Combine(StoragePath, study.Uid);
                if (Directory.Exists(folderPath))
                {
                    Directory.Delete(folderPath, true);
                }
            }
        }
    }
}
