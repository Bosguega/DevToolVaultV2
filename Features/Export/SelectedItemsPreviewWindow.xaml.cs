using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using DevToolVaultV2.Core.Models;

namespace DevToolVaultV2.Features.Export
{
    public partial class SelectedItemsPreviewWindow : Window, INotifyPropertyChanged
    {
        private List<FileSystemItem> _selectedItems;
        private string _headerText;
        private string _fileListText;
        
        public string HeaderText
        {
            get => _headerText;
            set
            {
                _headerText = value;
                OnPropertyChanged(nameof(HeaderText));
            }
        }
        
        public string FileListText
        {
            get => _fileListText;
            set
            {
                _fileListText = value;
                OnPropertyChanged(nameof(FileListText));
            }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;

        public SelectedItemsPreviewWindow(List<FileSystemItem> selectedItems)
        {
            InitializeComponent();
            _selectedItems = selectedItems ?? new List<FileSystemItem>();
            DataContext = this;
            LoadItems();
        }

        private void LoadItems()
        {
            // Filter to only include files (not directories)
            var filesOnly = _selectedItems.Where(item => !item.IsDirectory).ToList();
            
            var count = filesOnly.Count;
            HeaderText = $"Arquivos selecionados ({count}):\n{new string('=', 50)}";
            
            var sb = new StringBuilder();
            
            foreach (var item in filesOnly.OrderBy(i => i.RelativePath ?? i.FullPath))
            {
                var path = item.RelativePath ?? Path.GetRelativePath(Environment.CurrentDirectory, item.FullPath);
                sb.AppendLine($"- {path}");
            }
            
            FileListText = sb.ToString();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}