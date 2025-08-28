using DevToolVaultV2.Core.Models;
using DevToolVaultV2.Core.Services;
using System;
using System.Windows;

namespace DevToolVaultV2.Features.Filters
{
    public partial class FilterManagerWindow : Window
    {
        private readonly FileFilterManager _filterManager;

        public FilterManagerWindow(FileFilterManager filterManager)
        {
            InitializeComponent();
            _filterManager = filterManager ?? throw new ArgumentNullException(nameof(filterManager));
            LoadProfiles();
        }

        private void LoadProfiles()
        {
            lstProfiles.ItemsSource = null;
            lstProfiles.ItemsSource = _filterManager.GetProfiles();
        }

        private void BtnNew_Click(object sender, RoutedEventArgs e)
        {
            var newProfile = new FilterProfile
            {
                Name = "Novo Perfil",
                Description = "Descrição do novo perfil",
                IgnorePatterns = new System.Collections.Generic.List<string>(),
                CodeExtensions = new System.Collections.Generic.List<string>(),
                IgnoreEmptyFolders = true,
                ShowFileSize = false,
                ShowSystemFiles = false,
                ShowOnlyCodeFiles = false,
                IsBuiltIn = false
            };

            var editWindow = new FilterEditWindow(newProfile, false)
            {
                Owner = this
            };

            if (editWindow.ShowDialog() == true)
            {
                try
                {
                    _filterManager.SaveProfile(newProfile);
                    LoadProfiles();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao criar o perfil: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (lstProfiles.SelectedItem is FilterProfile selectedProfile)
            {
                if (selectedProfile.IsBuiltIn)
                {
                    MessageBox.Show("Perfis embutidos não podem ser editados.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var profileToEdit = new FilterProfile
                {
                    Name = selectedProfile.Name,
                    Description = selectedProfile.Description,
                    IgnorePatterns = new System.Collections.Generic.List<string>(selectedProfile.IgnorePatterns),
                    CodeExtensions = new System.Collections.Generic.List<string>(selectedProfile.CodeExtensions),
                    IgnoreEmptyFolders = selectedProfile.IgnoreEmptyFolders,
                    ShowFileSize = selectedProfile.ShowFileSize,
                    ShowSystemFiles = selectedProfile.ShowSystemFiles,
                    ShowOnlyCodeFiles = selectedProfile.ShowOnlyCodeFiles,
                    IsBuiltIn = selectedProfile.IsBuiltIn
                };

                var editWindow = new FilterEditWindow(profileToEdit, true) { Owner = this };
                if (editWindow.ShowDialog() == true)
                {
                    try
                    {
                        selectedProfile.Name = profileToEdit.Name;
                        selectedProfile.Description = profileToEdit.Description;
                        selectedProfile.IgnorePatterns = new System.Collections.Generic.List<string>(profileToEdit.IgnorePatterns);
                        selectedProfile.CodeExtensions = new System.Collections.Generic.List<string>(profileToEdit.CodeExtensions);
                        selectedProfile.IgnoreEmptyFolders = profileToEdit.IgnoreEmptyFolders;
                        selectedProfile.ShowFileSize = profileToEdit.ShowFileSize;
                        selectedProfile.ShowSystemFiles = profileToEdit.ShowSystemFiles;
                        selectedProfile.ShowOnlyCodeFiles = profileToEdit.ShowOnlyCodeFiles;

                        _filterManager.SaveProfile(selectedProfile);
                        LoadProfiles();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Erro ao salvar o perfil: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Selecione um perfil para editar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (lstProfiles.SelectedItem is FilterProfile selectedProfile)
            {
                if (selectedProfile.IsBuiltIn)
                {
                    MessageBox.Show("Perfis embutidos não podem ser excluídos.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var result = MessageBox.Show($"Tem certeza que deseja excluir o perfil '{selectedProfile.Name}'?",
                                             "Confirmar Exclusão",
                                             MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        _filterManager.DeleteProfile(selectedProfile);
                        LoadProfiles();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Erro ao excluir o perfil: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Selecione um perfil para excluir.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnSetActive_Click(object sender, RoutedEventArgs e)
        {
            if (lstProfiles.SelectedItem is FilterProfile selectedProfile)
            {
                try
                {
                    _filterManager.SetActiveProfile(selectedProfile);
                    MessageBox.Show($"Perfil '{selectedProfile.Name}' definido como ativo.", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao definir o perfil como ativo: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Selecione um perfil para definir como ativo.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
