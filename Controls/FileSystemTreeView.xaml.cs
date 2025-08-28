// DevToolVaultV2/Controls/FileSystemTreeView.xaml.cs
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DevToolVaultV2.Core.Models; // Certifique-se de que FileSystemItem está acessível

namespace DevToolVaultV2.Controls
{
    public partial class FileSystemTreeView : UserControl
    {
        public FileSystemTreeView()
        {
            InitializeComponent();
        }

        public List<FileSystemItem> Items
        {
            get => (List<FileSystemItem>)GetValue(ItemsProperty);
            set => SetValue(ItemsProperty, value);
        }

        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register(nameof(Items), typeof(List<FileSystemItem>), typeof(FileSystemTreeView));

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            // A lógica está implementada na classe FileSystemItem
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            // A lógica está implementada na classe FileSystemItem
        }

        // --- Métodos adicionados para compatibilidade com EstruturaWindow ---
        public void LoadDirectory(/*string path, FileFilterManager filterManager*/)
        {
            // Esta lógica deve idealmente estar no ViewModel
            // Considere remover a chamada de EstruturaWindow.xaml.cs
            // ou redirecioná-la para o ViewModel
            //throw new NotImplementedException("Loading logic should be in ViewModel");
            // Ou implementar uma lógica básica aqui (não recomendado para MVVM)
            // Items = new TreeGeneratorService(filterManager).GenerateTree(path);
        }

        public void SetAllExpanded(bool isExpanded)
        {
            if (treeView.ItemsSource is IEnumerable<FileSystemItem> items)
            {
                SetAllExpandedRecursive(items, isExpanded);
            }
        }

        private void SetAllExpandedRecursive(IEnumerable<FileSystemItem> items, bool isExpanded)
        {
            foreach (var item in items)
            {
                item.IsExpanded = isExpanded;
                if (item.Children != null && item.Children.Any())
                {
                    SetAllExpandedRecursive(item.Children, isExpanded);
                }
            }
        }

        public void SetAllItemsChecked(bool isChecked)
        {
            if (treeView.ItemsSource is IEnumerable<FileSystemItem> items)
            {
                SetAllItemsCheckedRecursive(items, isChecked);
            }
        }

        private void SetAllItemsCheckedRecursive(IEnumerable<FileSystemItem> items, bool isChecked)
        {
            foreach (var item in items)
            {
                item.IsChecked = isChecked; // Isso acionará o INPC e potencialmente atualizações dos pais
                if (item.Children != null && item.Children.Any())
                {
                    SetAllItemsCheckedRecursive(item.Children, isChecked);
                }
            }
        }
        // --- Fim dos métodos adicionados ---
    }
}