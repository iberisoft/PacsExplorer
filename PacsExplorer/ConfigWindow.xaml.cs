using System;
using System.Windows;
using System.Windows.Controls;
using Xceed.Wpf.Toolkit;

namespace PacsExplorer
{
    /// <summary>
    /// Interaction logic for ConfigWindow.xaml
    /// </summary>
    public partial class ConfigWindow : Window
    {
        public ConfigWindow()
        {
            InitializeComponent();
        }

        private void Submit(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (DialogResult == true)
            {
                ServerHost.GetBindingExpression(TextBox.TextProperty).UpdateSource();
                QrServerAeTitle.GetBindingExpression(TextBox.TextProperty).UpdateSource();
                QrServerPort.GetBindingExpression(IntegerUpDown.ValueProperty).UpdateSource();
                StoreServerAeTitle.GetBindingExpression(TextBox.TextProperty).UpdateSource();
                StoreServerPort.GetBindingExpression(IntegerUpDown.ValueProperty).UpdateSource();
                ClientAeTitle.GetBindingExpression(TextBox.TextProperty).UpdateSource();
                ClientPort.GetBindingExpression(IntegerUpDown.ValueProperty).UpdateSource();
            }
        }
    }
}
