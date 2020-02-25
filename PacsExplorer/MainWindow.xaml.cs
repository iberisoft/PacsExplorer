using Dicom;
using DicomScu;
using Microsoft.Extensions.Configuration;
using Microsoft.Win32;
using PacsExplorer.Converters;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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

            StoragePath = Path.Combine(Path.GetTempPath(), nameof(PacsExplorer));
            Directory.CreateDirectory(StoragePath);

            DataContext = this;
        }

        public DicomStudyQuery StudyQuery { get; set; } = new DicomStudyQuery();

        public string StoragePath { get; }

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
        DicomStoreClient m_DicomStoreClient;

        private void CreateDicomQrClient()
        {
            m_DicomQrClient ??= new DicomQrClient(m_Settings.QrServer.Host, m_Settings.QrServer.Port, m_Settings.QrServer.AeTitle, m_Settings.Client.AeTitle);
        }

        private void CreateDicomStoreClient()
        {
            m_DicomStoreClient ??= new DicomStoreClient(m_Settings.StoreServer.Host, m_Settings.StoreServer.Port, m_Settings.StoreServer.AeTitle, m_Settings.Client.AeTitle);
        }

        private void OpenVerifyMenu(object sender, RoutedEventArgs e)
        {
            var menu = (ContextMenu)FindResource("VerifyMenu");
            menu.PlacementTarget = (Button)sender;
            menu.Placement = PlacementMode.Bottom;
            menu.IsOpen = true;
        }

        private async void VerifyQrServer(object sender, RoutedEventArgs e)
        {
            CreateDicomQrClient();
            var success = await DoWork(async () =>
            {
                await m_DicomQrClient.VerifyAsync();
            }, true);
            if (success)
            {
                MessageBox.Show("The server is running.", Title, MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void VerifyStoreServer(object sender, RoutedEventArgs e)
        {
            CreateDicomStoreClient();
            var success = await DoWork(async () =>
            {
                await m_DicomStoreClient.VerifyAsync();
            }, true);
            if (success)
            {
                MessageBox.Show("The server is running.", Title, MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void FindStudies(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            button.Focus();

            CreateDicomQrClient();
            await DoWork(async () =>
            {
                var request = DicomQrClient.CreateStudyQueryRequest(StudyQuery);
                var datasets = await m_DicomQrClient.QueryAsync(request);
                Studies.ItemsSource = datasets.Select(dataset => new DicomStudy(dataset)).OrderByDescending(study => study.Date);
            });
        }

        private async void UploadFiles(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Multiselect = true;
            if (dialog.ShowDialog() == true)
            {
                CreateDicomStoreClient();
                await DoWork(async () =>
                {
                    await m_DicomStoreClient.StoreAsync(dialog.FileNames.Select(filePath => DicomFile.Open(filePath)));
                }, true);
            }
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

        private async Task<bool> DoWork(Func<Task> action, bool indeterminateProgress = false)
        {
            try
            {
                IsEnabled = false;
                if (indeterminateProgress)
                {
                    RetrievingProgress.IsIndeterminate = true;
                }
                await action();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            finally
            {
                IsEnabled = true;
                if (indeterminateProgress)
                {
                    RetrievingProgress.IsIndeterminate = false;
                }
            }
        }

        private async Task<bool> Save(DicomDataset dataset)
        {
            Dispatcher.Invoke(() => ++RetrievingProgress.Value);

            var file = new DicomFile(dataset);
            var filePath = Path.Combine(StoragePath, dataset.GetString(DicomTag.StudyInstanceUID), dataset.GetString(DicomTag.SOPInstanceUID) + ".dcm");
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            await file.SaveAsync(filePath);
            return true;
        }

        private void OpenFolder(DicomStudy study)
        {
            var folderPath = Path.Combine(StoragePath, study.Uid);
            Process.Start(File.Exists(m_Settings.ImageViewerPath) ? m_Settings.ImageViewerPath : "explorer", folderPath);
        }

        private void DeleteFolder(DicomStudy study)
        {
            var folderPath = Path.Combine(StoragePath, study.Uid);
            if (Directory.Exists(folderPath))
            {
                Directory.Delete(folderPath, true);
            }
        }
    }
}
