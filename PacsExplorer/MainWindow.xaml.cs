using Dicom;
using DicomScu;
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
        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;
        }

        public DicomStudyQuery StudyQuery { get; set; } = new DicomStudyQuery();

        public DicomSeriesQuery SeriesQuery { get; set; } = new DicomSeriesQuery();

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
                var settings = new Settings("appsettings.json");
                m_DicomQrClient = new DicomQrClient(settings.ServerHost, settings.ServerPort, settings.ServerAeTitle, settings.ClientAeTitle);
            }

            await DoWork(async () =>
            {
                var request = DicomQrClient.CreateStudyQueryRequest(StudyQuery);
                var datasets = await m_DicomQrClient.QueryAsync(request);
                Studies.ItemsSource = datasets.Select(dataset => new DicomStudy(dataset)).OrderByDescending(study => study.Date);
            });
        }

        private async void Studies_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var study = (DicomStudy)Studies.SelectedItem;
            if (study == null)
            {
                Series.ItemsSource = null;
                return;
            }

            await DoWork(async () =>
            {
                var request = DicomQrClient.CreateSeriesQueryRequest(study.Uid, SeriesQuery);
                var datasets = await m_DicomQrClient.QueryAsync(request);
                Series.ItemsSource = datasets.Select(dataset => new DicomSeries(dataset));
            });
        }

        private async void RetrieveStudy(object sender, RoutedEventArgs e)
        {
            var study = (DicomStudy)Studies.SelectedItem;
            RetrievingProgress.Maximum = study.ImageCount ?? 0;

            await DoWork(async () =>
            {
                var request = DicomQrClient.CreateStudyRetrieveRequest(study.Uid);
                await m_DicomQrClient.RetrieveAsync(request, Save);
                ShowFolder(study);
            });
        }

        private async void RetrieveSeries(object sender, RoutedEventArgs e)
        {
            var study = (DicomStudy)Studies.SelectedItem;
            var series = (DicomSeries)Series.SelectedItem;
            RetrievingProgress.Maximum = series.ImageCount ?? 0;

            await DoWork(async () =>
            {
                var request = DicomQrClient.CreateSeriesRetrieveRequest(study.Uid, series.Uid);
                await m_DicomQrClient.RetrieveAsync(request, Save);
                ShowFolder(study);
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

        private void ShowFolder(DicomStudy study)
        {
            if (Directory.Exists(StoragePath))
            {
                var folderPath = Path.Combine(StoragePath, study.Uid);
                Process.Start("explorer", folderPath);
            }
        }
    }
}
