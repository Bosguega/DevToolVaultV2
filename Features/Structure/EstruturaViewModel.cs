// DevToolVaultV2/Features/Structure/EstruturaViewModel.cs
using DevToolVaultV2.Core.Commands;
using DevToolVaultV2.Core.Models;
using DevToolVaultV2.Core.Services;
using DevToolVaultV2.Core.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace DevToolVaultV2.Features.Structure
{
    /// <summary>
    /// ViewModel responsável por gerenciar a estrutura de arquivos e pastas.
    /// </summary>
    public class EstruturaViewModel : BaseViewModel
    {
        private readonly FileFilterManager _filterManager;

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

        // Comandos
        public ICommand ExpandAllCommand { get; }
        public ICommand CollapseAllCommand { get; }
        public ICommand CheckAllCommand { get; }
        public ICommand UncheckAllCommand { get; }
        public ICommand SelectFilterCommand { get; }

        public EstruturaViewModel(FileFilterManager filterManager)
        {
            _filterManager = filterManager;

            // Inicializa o perfil ativo (usando método GetActiveProfile() que você deve garantir que exista)
            ActiveFilterProfile = _filterManager.GetActiveProfile();

            // Inicializa comandos
            ExpandAllCommand = new RelayCommand(ExpandAll);
            CollapseAllCommand = new RelayCommand(CollapseAll);
            CheckAllCommand = new RelayCommand(CheckAll);
            UncheckAllCommand = new RelayCommand(UncheckAll);
            SelectFilterCommand = new RelayCommand(SelectFilter);
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
            }
        }

        /// <summary>
        /// Retorna somente pastas (IsDirectory) ou arquivos, útil para bindings de UI.
        /// </summary>
        public IEnumerable<FileSystemItem> Folders => Items?.Where(i => i.IsDirectory) ?? Enumerable.Empty<FileSystemItem>();
        public IEnumerable<FileSystemItem> Files => Items?.Where(i => !i.IsDirectory) ?? Enumerable.Empty<FileSystemItem>();
    }
}
