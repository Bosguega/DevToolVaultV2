﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using DevToolVaultV2.Core.Models;
using DevToolVaultV2.Core.Services;
using DevToolVaultV2.Core.Commands;
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
        private int _selectedItemsCount;

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
                {
                    SetAllItemsChecked(_selectAll);
                    UpdateSelectedItemsCount();
                }
            }
        }

        public int SelectedItemsCount
        {
            get => _selectedItemsCount;
            private set => SetProperty(ref _selectedItemsCount, value);
        }

        public string CurrentFilterName => _filterManager.GetActiveProfile()?.Name ?? "Padrão";

        // Comandos
        public ICommand SelectFolderCommand { get; }
        public ICommand SelectFilterCommand { get; }
        public ICommand ExportSelectedCommand { get; }
        public ICommand PreviewSelectedCommand { get; }
        public ICommand ExpandAllCommand { get; }
        public ICommand CollapseAllCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public ExportarCodigoViewModel(FileFilterManager filterManager, IExportService exportService)
        {
            _filterManager = filterManager ?? throw new ArgumentNullException(nameof(filterManager));
            _exportService = exportService ?? throw new ArgumentNullException(nameof(exportService));
            _treeGenerator = new TreeGeneratorService(_filterManager);

            FileSystemItems = new ObservableCollection<FileSystemItem>();
            
            // Subscribe to collection changes to update selected count
            FileSystemItems.CollectionChanged += (s, e) => UpdateSelectedItemsCount();

            SelectFolderCommand = new RelayCommand<object>(async _ => await SelectFolderAsync());
            SelectFilterCommand = new RelayCommand<object>(_ => SelectFilter());
            ExportSelectedCommand = new RelayCommand<object>(async _ => await ExportSelectedAsync());
            PreviewSelectedCommand = new RelayCommand<object>(_ => PreviewSelected());
            ExpandAllCommand = new RelayCommand<object>(_ => SetAllExpanded(true));
            CollapseAllCommand = new RelayCommand<object>(_ => SetAllExpanded(false));
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
                // Generate tree in background thread
                var items = await Task.Run(() => _treeGenerator.GenerateTree(path));
                
                // Update UI on dispatcher thread
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    // Clear and rebuild the collection to ensure UI updates
                    FileSystemItems.Clear();
                    foreach (var item in items)
                    {
                        FileSystemItems.Add(item);
                    }
                    
                    // Expand first level items for better user experience
                    foreach (var item in FileSystemItems)
                    {
                        if (item.IsDirectory && item.Children?.Any() == true)
                        {
                            item.IsExpanded = true;
                        }
                        // Subscribe to property changes for counting
                        SubscribeToItemChanges(item);
                    }
                    
                    UpdateSelectedItemsCount();
                });
                
                // Notify property changed to refresh bindings
                OnPropertyChanged(nameof(FileSystemItems));
                OnPropertyChanged(nameof(CurrentFilterName));
            }
            catch (Exception ex)
            {
                // Show error message on UI thread
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show($"Erro ao carregar diretório: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async void SelectFilter()
        {
            var selectorWindow = new Features.Filters.ProjectTypeSelectorWindow(_filterManager)
            {
                Owner = Application.Current.MainWindow
            };
            
            if (selectorWindow.ShowDialog() == true && selectorWindow.SelectedProfile != null)
            {
                _filterManager.SetActiveProfile(selectorWindow.SelectedProfile);
                OnPropertyChanged(nameof(CurrentFilterName));
                
                // Reload the directory with new filter if a path is selected
                if (!string.IsNullOrWhiteSpace(CurrentPath))
                {
                    await LoadDirectoryAsync(CurrentPath);
                }
            }
        }

        private async Task ExportSelectedAsync()
        {
            var selectedItems = new List<FileSystemItem>();
            GetSelectedItemsRecursive(FileSystemItems, selectedItems);
            
            // Filter to only include files (not directories)
            var filesOnly = selectedItems.Where(item => !item.IsDirectory).ToList();

            if (!filesOnly.Any())
            {
                MessageBox.Show("Nenhum arquivo selecionado. Apenas arquivos podem ser exportados.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Direct format selection via save dialog filter
            var saveDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Arquivo de Texto (*.txt)|*.txt|Arquivo Markdown (*.md)|*.md|Arquivo PDF (*.pdf)|*.pdf|Arquivo ZIP (*.zip)|*.zip",
                FilterIndex = 1, // Default to TXT
                FileName = $"Export_{Path.GetFileName(CurrentPath)}_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}",
                Title = $"Exportar {filesOnly.Count} arquivos - Selecione o formato"
            };

            if (saveDialog.ShowDialog() == true)
            {
                // Determine format based on selected filter
                var selectedFormat = saveDialog.FilterIndex switch
                {
                    1 => ExportFormat.Text,
                    2 => ExportFormat.Markdown,
                    3 => ExportFormat.Pdf,
                    4 => ExportFormat.Zip,
                    _ => ExportFormat.Text
                };
                
                IsLoading = true;
                try
                {
                    await _exportService.ExportAsync(filesOnly, saveDialog.FileName, selectedFormat);
                    MessageBox.Show($"Exportação concluída com sucesso!\n\nFormato: {GetFormatDisplayName(selectedFormat)}\nArquivo: {saveDialog.FileName}\nArquivos exportados: {filesOnly.Count}", 
                        "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
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

        private void PreviewSelected()
        {
            var selectedItems = new List<FileSystemItem>();
            GetSelectedItemsRecursive(FileSystemItems, selectedItems);
            
            // Filter to only include files (not directories)
            var filesOnly = selectedItems.Where(item => !item.IsDirectory).ToList();

            if (!filesOnly.Any())
            {
                MessageBox.Show("Nenhum arquivo selecionado para visualizar. Apenas arquivos são exibidos na visualização.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var previewWindow = new SelectedItemsPreviewWindow(filesOnly)
            {
                Owner = Application.Current.MainWindow
            };
            previewWindow.ShowDialog();
        }

        private string GetFormatDisplayName(ExportFormat format)
        {
            return format switch
            {
                ExportFormat.Text => "Texto (.txt)",
                ExportFormat.Markdown => "Markdown (.md)", 
                ExportFormat.Pdf => "PDF (.pdf)",
                ExportFormat.Zip => "ZIP (.zip)",
                _ => "Desconhecido"
            };
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
            UpdateSelectedItemsCount();
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
        
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        private void UpdateSelectedItemsCount()
        {
            var selectedItems = new List<FileSystemItem>();
            GetSelectedItemsRecursive(FileSystemItems, selectedItems);
            // Count only files, not directories
            SelectedItemsCount = selectedItems.Count(item => !item.IsDirectory);
        }
        
        private void SubscribeToItemChanges(FileSystemItem item)
        {
            item.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(FileSystemItem.IsChecked))
                {
                    UpdateSelectedItemsCount();
                }
            };
            
            if (item.Children != null)
            {
                foreach (var child in item.Children)
                {
                    SubscribeToItemChanges(child);
                }
            }
        }
    }
}
