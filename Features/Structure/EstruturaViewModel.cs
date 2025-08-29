﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿// DevToolVaultV2/Features/Structure/EstruturaViewModel.cs
using DevToolVaultV2.Core.Commands;
using DevToolVaultV2.Core.Models;
using DevToolVaultV2.Core.Services;
using DevToolVaultV2.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;

namespace DevToolVaultV2.Features.Structure
{
    /// <summary>
    /// ViewModel responsável por gerenciar a estrutura de arquivos e pastas.
    /// </summary>
    public class EstruturaViewModel : BaseViewModel
    {
        private readonly FileFilterManager _filterManager;
        private readonly TreeGeneratorService _treeGenerator;
        private readonly TreeToMermaidConverter _mermaidConverter;

        private List<FileSystemItem> _items;
        public List<FileSystemItem> Items
        {
            get => _items;
            set => SetProperty(ref _items, value);
        }

        private FilterProfile _activeFilterProfile;
        public FilterProfile ActiveFilterProfile
        {
            get => _activeFilterProfile;
            set => SetProperty(ref _activeFilterProfile, value);
        }

        private string _selectedPath;
        public string SelectedPath
        {
            get => _selectedPath;
            set => SetProperty(ref _selectedPath, value);
        }

        private string _treeText;
        public string TreeText
        {
            get => _treeText;
            set => SetProperty(ref _treeText, value);
        }

        public enum DisplayFormat
        {
            ASCII,
            Graphic,
            Mermaid
        }

        private DisplayFormat _selectedFormat = DisplayFormat.ASCII;
        public DisplayFormat SelectedFormat
        {
            get => _selectedFormat;
            set
            {
                if (SetProperty(ref _selectedFormat, value))
                {
                    RefreshTreeDisplay();
                    OnPropertyChanged(nameof(UseAsciiTree));
                    OnPropertyChanged(nameof(UseGraphicTree));
                    OnPropertyChanged(nameof(UseMermaidTree));
                }
            }
        }

        public bool UseAsciiTree
        {
            get => _selectedFormat == DisplayFormat.ASCII;
            set { if (value) SelectedFormat = DisplayFormat.ASCII; }
        }

        public bool UseGraphicTree
        {
            get => _selectedFormat == DisplayFormat.Graphic;
            set { if (value) SelectedFormat = DisplayFormat.Graphic; }
        }

        public bool UseMermaidTree
        {
            get => _selectedFormat == DisplayFormat.Mermaid;
            set { if (value) SelectedFormat = DisplayFormat.Mermaid; }
        }

        public string CurrentProfileName => ActiveFilterProfile?.Name ?? "Padrão";

        // Comandos
        public ICommand ExpandAllCommand { get; }
        public ICommand CollapseAllCommand { get; }
        public ICommand CheckAllCommand { get; }
        public ICommand UncheckAllCommand { get; }
        public ICommand SelectFilterCommand { get; }
        public ICommand BrowseFolderCommand { get; }
        public ICommand CopyStructureCommand { get; }
        public ICommand SaveStructureCommand { get; }
        public ICommand CloseCommand { get; }

        public EstruturaViewModel(FileFilterManager filterManager, TreeToMermaidConverter mermaidConverter)
        {
            _filterManager = filterManager;
            _mermaidConverter = mermaidConverter;
            _treeGenerator = new TreeGeneratorService(filterManager);

            // Inicializa o perfil ativo
            ActiveFilterProfile = _filterManager.GetActiveProfile();

            // Inicializa comandos
            ExpandAllCommand = new RelayCommand<object>(_ => ExpandAll());
            CollapseAllCommand = new RelayCommand<object>(_ => CollapseAll());
            CheckAllCommand = new RelayCommand<object>(_ => CheckAll());
            UncheckAllCommand = new RelayCommand<object>(_ => UncheckAll());
            SelectFilterCommand = new RelayCommand<object>(_ => SelectFilter());
            BrowseFolderCommand = new RelayCommand<object>(_ => BrowseFolder());
            CopyStructureCommand = new RelayCommand<object>(_ => CopyStructure());
            SaveStructureCommand = new RelayCommand<object>(_ => SaveStructure());
            CloseCommand = new RelayCommand<object>(_ => CloseWindow());
        }

        private void BrowseFolder()
        {
            var dialog = new VistaFolderBrowserDialog();
            if (!string.IsNullOrWhiteSpace(SelectedPath) && Directory.Exists(SelectedPath))
                dialog.SelectedPath = SelectedPath;

            if (dialog.ShowDialog() == true)
            {
                SelectedPath = dialog.SelectedPath;
                LoadDirectoryStructure();
            }
        }

        private void LoadDirectoryStructure()
        {
            if (string.IsNullOrWhiteSpace(SelectedPath) || !Directory.Exists(SelectedPath))
                return;

            try
            {
                Items = _treeGenerator.GenerateTree(SelectedPath);
                RefreshTreeDisplay();
                OnPropertyChanged(nameof(CurrentProfileName));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar estrutura: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshTreeDisplay()
        {
            if (Items == null) return;
            
            TreeText = SelectedFormat switch
            {
                DisplayFormat.ASCII => GenerateAsciiTreeText(Items),
                DisplayFormat.Graphic => GenerateIconTreeText(Items),
                DisplayFormat.Mermaid => GenerateMermaidTreeText(Items),
                _ => GenerateAsciiTreeText(Items)
            };
        }

        private string GenerateMermaidTreeText(List<FileSystemItem> items)
        {
            if (items == null || !items.Any()) return string.Empty;
            
            // Convert FileSystemItems to a clean textual tree format (without icons)
            var treeText = GenerateCleanTreeText(items);
            
            // Use TreeToMermaidConverter to convert to Mermaid format
            var result = _mermaidConverter.ConvertTreeToMermaid(treeText);
            
            return result.IsSuccess ? result.MermaidDiagram : $"Error generating Mermaid: {result.ErrorMessage}";
        }

        private string GenerateCleanTreeText(List<FileSystemItem> items)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < items.Count; i++)
            {
                bool isLast = i == items.Count - 1;
                GenerateCleanTreeTextRecursive(items[i], sb, "", isLast);
            }
            return sb.ToString();
        }

        private void GenerateCleanTreeTextRecursive(FileSystemItem item, StringBuilder sb, string prefix, bool isLast)
        {
            // Current item - no icons, just ASCII tree structure
            string connector = isLast ? "└── " : "├── ";
            sb.AppendLine($"{prefix}{connector}{item.Name}");

            // Children
            if (item.Children != null && item.Children.Any())
            {
                string childPrefix = prefix + (isLast ? "    " : "│   ");
                for (int i = 0; i < item.Children.Count; i++)
                {
                    bool isLastChild = i == item.Children.Count - 1;
                    GenerateCleanTreeTextRecursive(item.Children[i], sb, childPrefix, isLastChild);
                }
            }
        }

        private string GenerateIconTreeText(List<FileSystemItem> items)
        {
            var sb = new StringBuilder();
            foreach (var item in items)
            {
                GenerateIconTreeTextRecursive(item, sb, 0);
            }
            return sb.ToString();
        }

        private void GenerateIconTreeTextRecursive(FileSystemItem item, StringBuilder sb, int level)
        {
            var indent = string.Empty;
            for (int i = 0; i < level; i++)
                indent += "  ";
            
            var icon = item.IsDirectory ? "📁" : "📄";
            sb.AppendLine($"{indent}{icon} {item.Name}");

            if (item.Children != null)
            {
                foreach (var child in item.Children)
                {
                    GenerateIconTreeTextRecursive(child, sb, level + 1);
                }
            }
        }

        private string GenerateAsciiTreeText(List<FileSystemItem> items)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < items.Count; i++)
            {
                bool isLast = i == items.Count - 1;
                GenerateAsciiTreeTextRecursive(items[i], sb, "", isLast);
            }
            return sb.ToString();
        }

        private void GenerateAsciiTreeTextRecursive(FileSystemItem item, StringBuilder sb, string prefix, bool isLast)
        {
            // Current item
            string connector = isLast ? "└── " : "├── ";
            sb.AppendLine($"{prefix}{connector}{item.Name}");

            // Children
            if (item.Children != null && item.Children.Any())
            {
                string childPrefix = prefix + (isLast ? "    " : "│   ");
                for (int i = 0; i < item.Children.Count; i++)
                {
                    bool isLastChild = i == item.Children.Count - 1;
                    GenerateAsciiTreeTextRecursive(item.Children[i], sb, childPrefix, isLastChild);
                }
            }
        }

        private void CopyStructure()
        {
            if (string.IsNullOrWhiteSpace(TreeText))
            {
                MessageBox.Show("Nenhuma estrutura para copiar. Selecione uma pasta primeiro.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                Clipboard.SetText(TreeText);
                MessageBox.Show("Estrutura copiada para a área de transferência!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao copiar estrutura: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveStructure()
        {
            if (string.IsNullOrWhiteSpace(TreeText))
            {
                MessageBox.Show("Nenhuma estrutura para salvar. Selecione uma pasta primeiro.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var saveDialog = new SaveFileDialog
            {
                Filter = "Arquivo de Texto (*.txt)|*.txt|Arquivo Markdown (*.md)|*.md|Arquivo PDF (*.pdf)|*.pdf|Arquivo ZIP (*.zip)|*.zip|Todos os arquivos (*.*)|*.*",
                DefaultExt = "txt",
                FileName = "estrutura_pastas.txt"
            };

            if (saveDialog.ShowDialog() == true)
            {
                try
                {
                    var extension = Path.GetExtension(saveDialog.FileName).ToLowerInvariant();
                    
                    switch (extension)
                    {
                        case ".txt":
                            SaveAsText(saveDialog.FileName);
                            break;
                        case ".md":
                            SaveAsMarkdown(saveDialog.FileName);
                            break;
                        case ".pdf":
                            SaveAsPdf(saveDialog.FileName);
                            break;
                        case ".zip":
                            SaveAsZip(saveDialog.FileName);
                            break;
                        default:
                            // Default to text format
                            SaveAsText(saveDialog.FileName);
                            break;
                    }
                    
                    MessageBox.Show($"Estrutura salva em: {saveDialog.FileName}", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao salvar estrutura: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SaveAsText(string fileName)
        {
            File.WriteAllText(fileName, TreeText, Encoding.UTF8);
        }

        private void SaveAsMarkdown(string fileName)
        {
            var markdownContent = GenerateMarkdownContent();
            File.WriteAllText(fileName, markdownContent, Encoding.UTF8);
        }

        private void SaveAsPdf(string fileName)
        {
            // Use the existing PDF export strategy
            var mockItems = CreateMockFileSystemItems();
            var pdfStrategy = new PdfExportStrategy();
            Task.Run(() => pdfStrategy.ExportAsync(mockItems, fileName)).Wait();
        }

        private void SaveAsZip(string fileName)
        {
            // Create a temporary text file and zip it
            var tempDir = Path.Combine(Path.GetTempPath(), "DevToolVaultV2_Structure_" + Guid.NewGuid());
            Directory.CreateDirectory(tempDir);
            
            try
            {
                var tempTextFile = Path.Combine(tempDir, "estrutura_pastas.txt");
                File.WriteAllText(tempTextFile, TreeText, Encoding.UTF8);
                
                if (File.Exists(fileName)) File.Delete(fileName);
                System.IO.Compression.ZipFile.CreateFromDirectory(tempDir, fileName);
            }
            finally
            {
                if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
            }
        }

        private string GenerateMarkdownContent()
        {
            var sb = new StringBuilder();
            sb.AppendLine("# Estrutura de Pastas");
            sb.AppendLine();
            sb.AppendLine($"**Pasta:** {SelectedPath}");
            sb.AppendLine($"**Filtro:** {CurrentProfileName}");
            sb.AppendLine($"**Data:** {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
            sb.AppendLine();
            sb.AppendLine("## Estrutura");
            sb.AppendLine();
            sb.AppendLine("```");
            sb.AppendLine(TreeText);
            sb.AppendLine("```");
            
            return sb.ToString();
        }

        private List<FileSystemItem> CreateMockFileSystemItems()
        {
            // Create a single mock item containing the tree structure as content
            var mockItem = new FileSystemItem
            {
                Name = "estrutura_pastas.txt",
                FullPath = Path.Combine(Path.GetTempPath(), "estrutura_pastas.txt"),
                RelativePath = "estrutura_pastas.txt",
                IsDirectory = false
            };
            
            // Write the tree text to a temporary file for PDF generation
            File.WriteAllText(mockItem.FullPath, TreeText, Encoding.UTF8);
            
            return new List<FileSystemItem> { mockItem };
        }

        private void CloseWindow()
        {
            // Close will be handled by the window itself
            Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.DataContext == this)?.Close();
        }

        /// <summary>
        /// Expande todos os itens da árvore.
        /// </summary>
        private void ExpandAll()
        {
            if (Items == null) return;
            foreach (var item in Items)
            {
                ExpandRecursive(item);
            }
        }

        private void ExpandRecursive(FileSystemItem item)
        {
            item.IsExpanded = true;
            if (item.Children != null)
            {
                foreach (var child in item.Children)
                    ExpandRecursive(child);
            }
        }

        /// <summary>
        /// Colapsa todos os itens da árvore.
        /// </summary>
        private void CollapseAll()
        {
            if (Items == null) return;
            foreach (var item in Items)
            {
                CollapseRecursive(item);
            }
        }

        private void CollapseRecursive(FileSystemItem item)
        {
            item.IsExpanded = false;
            if (item.Children != null)
            {
                foreach (var child in item.Children)
                    CollapseRecursive(child);
            }
        }

        /// <summary>
        /// Marca todos os itens.
        /// </summary>
        private void CheckAll()
        {
            if (Items == null) return;
            foreach (var item in Items)
            {
                CheckRecursive(item, true);
            }
        }

        /// <summary>
        /// Desmarca todos os itens.
        /// </summary>
        private void UncheckAll()
        {
            if (Items == null) return;
            foreach (var item in Items)
            {
                CheckRecursive(item, false);
            }
        }

        private void CheckRecursive(FileSystemItem item, bool isChecked)
        {
            item.IsChecked = isChecked;
            if (item.Children != null)
            {
                foreach (var child in item.Children)
                    CheckRecursive(child, isChecked);
            }
        }

        /// <summary>
        /// Abre a janela para selecionar o filtro ativo.
        /// </summary>
        private void SelectFilter()
        {
            var selectorWindow = new Features.Filters.ProjectTypeSelectorWindow(_filterManager);
            bool? result = selectorWindow.ShowDialog();
            if (result == true && selectorWindow.SelectedProfile != null)
            {
                ActiveFilterProfile = selectorWindow.SelectedProfile;
                _filterManager.SetActiveProfile(ActiveFilterProfile);
                OnPropertyChanged(nameof(CurrentProfileName));
                
                // Refresh the tree with new filter
                if (!string.IsNullOrWhiteSpace(SelectedPath))
                {
                    LoadDirectoryStructure();
                }
            }
        }

        /// <summary>
        /// Retorna somente pastas (IsDirectory) ou arquivos, útil para bindings de UI.
        /// </summary>
        public IEnumerable<FileSystemItem> Folders => Items?.Where(i => i.IsDirectory) ?? Enumerable.Empty<FileSystemItem>();
        public IEnumerable<FileSystemItem> Files => Items?.Where(i => !i.IsDirectory) ?? Enumerable.Empty<FileSystemItem>();
    }
}
