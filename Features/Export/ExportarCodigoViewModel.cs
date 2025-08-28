using DevToolVaultV2.Core.Models;
using DevToolVaultV2.Core.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DevToolVaultV2.Features.Export
{
    public class ExportarCodigoViewModel : INotifyPropertyChanged
    {
        private readonly FileFilterManager _filterManager;
        private readonly IExportService _exportService;
        private readonly TreeGeneratorService _treeGenerator;

        private ObservableCollection<FileSystemItem> _fileSystemItems;
        private string _currentPath;
        private bool _isLoading;
        private bool _selectAll;

        public ObservableCollection<FileSystemItem> FileSystemItems
        {
            get => _fileSystemItems;
            set => SetProperty(ref _fileSystemItems, value);
        }

        public string CurrentPath
        {
            get => _currentPath;
            set => SetProperty(ref _currentPath, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public bool SelectAll
        {
            get => _selectAll;
            set
            {
                if (SetProperty(ref _selectAll, value))
                    SetAllItemsChecked(_selectAll);
            }
        }

        // Comandos
        public ICommand SelectFolderCommand { get; }
        public ICommand ExportTextCommand { get; }
        public ICommand ExportPdfCommand { get; }
        public ICommand ExportZipCommand { get; }
        public ICommand ExpandAllCommand { get; }
        public ICommand CollapseAllCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public ExportarCodigoViewModel(FileFilterManager filterManager, IExportService exportService)
        {
            _filterManager = filterManager ?? throw new ArgumentNullException(nameof(filterManager));
            _exportService = exportService ?? throw new ArgumentNullException(nameof(exportService));
            _treeGenerator = new TreeGeneratorService(_filterManager);

            FileSystemItems = new ObservableCollection<FileSystemItem>();

            SelectFolderCommand = new RelayCommand(async _ => await SelectFolderAsync());
            ExportTextCommand = new RelayCommand(async _ => await ExportAsync(ExportFormat.Text));
            ExportPdfCommand = new RelayCommand(async _ => await ExportAsync(ExportFormat.Pdf));
            ExportZipCommand = new RelayCommand(async _ => await ExportAsync(ExportFormat.Zip));
            ExpandAllCommand = new RelayCommand(_ => SetAllExpanded(true));
            CollapseAllCommand = new RelayCommand(_ => SetAllExpanded(false));
        }

        private async Task SelectFolderAsync()
        {
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            if (!string.IsNullOrWhiteSpace(CurrentPath) && Directory.Exists(CurrentPath))
                dialog.SelectedPath = CurrentPath;

            if (dialog.ShowDialog() == true)
            {
                CurrentPath = dialog.SelectedPath;
                await LoadDirectoryAsync(CurrentPath);
            }
        }

        public async Task LoadDirectoryAsync(string path)
        {
            if (!Directory.Exists(path)) return;

            IsLoading = true;
            try
            {
                var items = await Task.Run(() => _treeGenerator.GenerateTree(path));
                FileSystemItems = new ObservableCollection<FileSystemItem>(items);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar diretório: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ExportAsync(ExportFormat format)
        {
            var selectedItems = new List<FileSystemItem>();
            GetSelectedItemsRecursive(FileSystemItems, selectedItems);

            if (!selectedItems.Any())
            {
                MessageBox.Show("Nenhum item selecionado.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var saveDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = format switch
                {
                    ExportFormat.Text => "TXT (*.txt)|*.txt",
                    ExportFormat.Pdf => "PDF (*.pdf)|*.pdf",
                    ExportFormat.Zip => "ZIP (*.zip)|*.zip",
                    _ => "Arquivo (*)|*.*"
                },
                FileName = $"Export_{Path.GetFileName(CurrentPath)}"
            };

            if (saveDialog.ShowDialog() == true)
            {
                IsLoading = true;
                try
                {
                    await _exportService.ExportAsync(selectedItems, saveDialog.FileName, format);
                    MessageBox.Show($"Exportação concluída: {saveDialog.FileName}", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro durante exportação: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }

        private void GetSelectedItemsRecursive(IEnumerable<FileSystemItem> items, List<FileSystemItem> selectedItems)
        {
            foreach (var item in items)
            {
                if (item.IsChecked == true)
                    selectedItems.Add(item);
                if (item.Children != null)
                    GetSelectedItemsRecursive(item.Children, selectedItems);
            }
        }

        private void SetAllItemsChecked(bool isChecked)
        {
            void CheckAll(IEnumerable<FileSystemItem> items)
            {
                foreach (var item in items)
                {
                    item.IsChecked = isChecked;
                    if (item.Children != null)
                        CheckAll(item.Children);
                }
            }
            CheckAll(FileSystemItems);
        }

        private void SetAllExpanded(bool isExpanded)
        {
            void ExpandAll(IEnumerable<FileSystemItem> items)
            {
                foreach (var item in items)
                {
                    item.IsExpanded = isExpanded;
                    if (item.Children != null)
                        ExpandAll(item.Children);
                }
            }
            ExpandAll(FileSystemItems);
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
            return true;
        }

        public class RelayCommand : ICommand
        {
            private readonly Action<object> _execute;
            private readonly Func<object, bool> _canExecute;

            public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
            {
                _execute = execute;
                _canExecute = canExecute;
            }

            public bool CanExecute(object parameter) => _canExecute?.Invoke(parameter) ?? true;
            public void Execute(object parameter) => _execute(parameter);
            public event EventHandler CanExecuteChanged
            {
                add => CommandManager.RequerySuggested += value;
                remove => CommandManager.RequerySuggested -= value;
            }
        }
    }
}
