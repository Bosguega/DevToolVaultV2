// Features/Filters/ProjectTypeSelectorWindow.xaml.cs
using System.Windows;
using DevToolVaultV2.Features.Filters;
using DevToolVaultV2.Core.Models;
using DevToolVaultV2.Core.Services;

namespace DevToolVaultV2.Features.Filters
{
    public partial class ProjectTypeSelectorWindow : Window
    {
        public FilterProfile SelectedProfile { get; private set; }
        private readonly FileFilterManager _filterManager;

        public ProjectTypeSelectorWindow(FileFilterManager filterManager)
        {
            InitializeComponent();
            _filterManager = filterManager;
            lstProfiles.ItemsSource = _filterManager.GetProfiles();
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            if (lstProfiles.SelectedItem is FilterProfile profile)
            {
                SelectedProfile = profile;
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("Selecione um tipo de projeto.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}