using DevToolVaultV2.Core.ViewModels;
using DevToolVaultV2.Core.Commands;
using DevToolVaultV2.Core.Services;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DevToolVaultV2.Features.TreetoFiles
{
    public class TreetoFilesViewModel : BaseViewModel
    {
        private readonly TreeToMermaidConverter _mermaidConverter;
        private readonly MermaidToTreeConverter _treeConverter;
        private readonly FileSystemCreator _fileSystemCreator;
        
        private string _inputTreeText;
        private string _mermaidOutput;
        private string _selectedDirectory;
        private bool _createEmptyFiles;
        private bool _isProcessing;

        public string InputTreeText
        {
            get => _inputTreeText;
            set
            {
                if (SetProperty(ref _inputTreeText, value))
                {
                    ConvertToMermaid();
                }
            }
        }

        public string MermaidOutput
        {
            get => _mermaidOutput;
            set => SetProperty(ref _mermaidOutput, value);
        }

        public string SelectedDirectory
        {
            get => _selectedDirectory;
            set => SetProperty(ref _selectedDirectory, value);
        }

        public bool CreateEmptyFiles
        {
            get => _createEmptyFiles;
            set => SetProperty(ref _createEmptyFiles, value);
        }

        public bool IsProcessing
        {
            get => _isProcessing;
            set => SetProperty(ref _isProcessing, value);
        }

        // Commands
        public ICommand SelectDirectoryCommand { get; }
        public ICommand ConvertToMermaidCommand { get; }
        public ICommand ConvertToTreeCommand { get; }
        public ICommand CreateFilesCommand { get; }
        public ICommand ClearCommand { get; }
        public ICommand CloseCommand { get; }

        public TreetoFilesViewModel(TreeToMermaidConverter mermaidConverter, MermaidToTreeConverter treeConverter, FileSystemCreator fileSystemCreator)
        {
            _mermaidConverter = mermaidConverter;
            _treeConverter = treeConverter;
            _fileSystemCreator = fileSystemCreator;
            
            CreateEmptyFiles = true;
            
            SelectDirectoryCommand = new RelayCommand<object>(_ => SelectDirectory());
            ConvertToMermaidCommand = new RelayCommand<object>(_ => ConvertToMermaid());
            ConvertToTreeCommand = new RelayCommand<object>(_ => ConvertMermaidToTree());
            CreateFilesCommand = new RelayCommand<object>(_ => CreateFilesAndFolders(), _ => CanCreateFiles());
            ClearCommand = new RelayCommand<object>(_ => ClearAll());
            CloseCommand = new RelayCommand<object>(_ => CloseWindow());
        }

        private void SelectDirectory()
        {
            var dialog = new VistaFolderBrowserDialog
            {
                Description = "Selecione o diretório onde criar a estrutura de arquivos"
            };

            if (!string.IsNullOrWhiteSpace(SelectedDirectory) && Directory.Exists(SelectedDirectory))
                dialog.SelectedPath = SelectedDirectory;

            if (dialog.ShowDialog() == true)
            {
                SelectedDirectory = dialog.SelectedPath;
            }
        }

        private void ConvertToMermaid()
        {
            if (string.IsNullOrWhiteSpace(InputTreeText))
            {
                MermaidOutput = string.Empty;
                return;
            }

            var result = _mermaidConverter.ConvertTreeToMermaid(InputTreeText);
            MermaidOutput = result.MermaidDiagram;
        }

        private void ConvertMermaidToTree()
        {
            if (string.IsNullOrWhiteSpace(MermaidOutput))
            {
                MessageBox.Show("Nenhum diagrama Mermaid encontrado para converter.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = _treeConverter.ConvertMermaidToTreeText(MermaidOutput);
            if (result.IsSuccess)
            {
                InputTreeText = result.TreeText;
                MessageBox.Show("Mermaid convertido para árvore ASCII com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show($"Erro ao converter Mermaid para árvore:\n{result.ErrorMessage}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanCreateFiles()
        {
            return !string.IsNullOrWhiteSpace(SelectedDirectory) && 
                   !string.IsNullOrWhiteSpace(MermaidOutput) && 
                   !IsProcessing;
        }

        private void CreateFilesAndFolders()
        {
            if (!CanCreateFiles()) return;

            IsProcessing = true;
            try
            {
                var options = new FileSystemCreator.CreationOptions
                {
                    CreateEmptyFiles = CreateEmptyFiles,
                    OverwriteExisting = false,
                    DefaultFileContent = string.Empty,
                    FileEncoding = Encoding.UTF8
                };

                var result = _fileSystemCreator.CreateFileStructureFromMermaid(SelectedDirectory, MermaidOutput, options);

                if (result.IsSuccess)
                {
                    var summary = FileSystemCreator.GetCreationSummary(result);
                    MessageBox.Show($"Estrutura de arquivos criada com sucesso em:\n{SelectedDirectory}\n\n{summary}", 
                        "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show($"Erro ao criar estrutura de arquivos:\n{result.ErrorMessage}", 
                        "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao criar estrutura de arquivos:\n{ex.Message}", 
                    "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsProcessing = false;
            }
        }

        private void ClearAll()
        {
            InputTreeText = string.Empty;
            MermaidOutput = string.Empty;
            SelectedDirectory = string.Empty;
        }

        private void CloseWindow()
        {
            // The window will handle its own closing
            Application.Current.Windows.OfType<TreetoFilesWindow>().FirstOrDefault()?.Close();
        }
    }
}