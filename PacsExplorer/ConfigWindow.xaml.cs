using System;
using System.Windows;
using System.Windows.Controls;

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
                QrServerPort.GetBindingExpression(TextBox.TextProperty).UpdateSource();
                StoreServerAeTitle.GetBindingExpression(TextBox.TextProperty).UpdateSource();
                StoreServerPort.GetBindingExpression(TextBox.TextProperty).UpdateSource();
                ClientAeTitle.GetBindingExpression(TextBox.TextProperty).UpdateSource();
                ClientPort.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            }
        }
    }
}
