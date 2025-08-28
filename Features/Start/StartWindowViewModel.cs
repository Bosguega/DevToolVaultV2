﻿using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using DevToolVaultV2.Core.Services;
using DevToolVaultV2.Features.Structure;
using DevToolVaultV2.Features.Export;
using DevToolVaultV2.Features.Filters;
using DevToolVaultV2.Core.Commands;

namespace DevToolVaultV2.Features.Start
{
    public class StartWindowViewModel
    {
        private FileFilterManager _filterManager;
        private readonly IAppNavigationService _navigationService;

        public ObservableCollection<CardItem> Cards { get; set; }
        public string FiltroAtual { get; private set; }

        // Comandos de menu
        public ICommand SelecionarTipoProjetoCommand { get; }
        public ICommand VisualizarEstruturaCommand { get; }
        public ICommand ExportarCodigoCommand { get; }
        public ICommand GerenciarFiltrosCommand { get; }
        public ICommand RecarregarFiltrosCommand { get; }
        public ICommand SairCommand { get; }
        public ICommand SobreCommand { get; }

        public StartWindowViewModel(FileFilterManager filterManager, IAppNavigationService navigationService)
        {
            _filterManager = filterManager;
            _navigationService = navigationService;

            // Cards
            LoadCards();

            // Menu Commands
            SelecionarTipoProjetoCommand = new RelayCommand<object>(_ => SelecionarTipoProjeto());
            VisualizarEstruturaCommand = new RelayCommand<object>(_ => _navigationService.Show<EstruturaWindow>());
            ExportarCodigoCommand = new RelayCommand<object>(_ => _navigationService.Show<ExportarCodigoWindow>());
            GerenciarFiltrosCommand = new RelayCommand<object>(_ => GerenciarFiltros());
            RecarregarFiltrosCommand = new RelayCommand<object>(_ => RecarregarFiltros());
            SairCommand = new RelayCommand<object>(_ => Application.Current.Shutdown());
            SobreCommand = new RelayCommand<object>(_ => MessageBox.Show(
                "DevToolVaultV2 v1.0\n\nFerramentas de desenvolvimento em um só lugar.\n\nDesenvolvido por: Seu Nome",
                "Sobre", MessageBoxButton.OK, MessageBoxImage.Information));

            UpdateFiltroAtual();
        }

        private void LoadCards()
        {
            Cards = new ObservableCollection<CardItem>
            {
                new CardItem
                {
                    Icon = "📁",
                    Title = "Estrutura de Pastas",
                    Description = "Visualize e exporte a estrutura de diretórios do seu projeto.",
                    OpenCommand = VisualizarEstruturaCommand
                },
                new CardItem
                {
                    Icon = "📦",
                    Title = "Exportar Código",
                    Description = "Exporte seu código-fonte para diversos formatos (TXT, MD, PDF, ZIP).",
                    OpenCommand = ExportarCodigoCommand
                }
            };
        }

        public void UpdateFiltroAtual()
        {
            var activeProfile = _filterManager.GetActiveProfile();
            FiltroAtual = activeProfile != null ? $"Filtro Atual: {activeProfile.Name}" : "Filtro Atual: Padrão";
        }

        private void SelecionarTipoProjeto()
        {
            var selectorWindow = new ProjectTypeSelectorWindow(_filterManager)
            {
                Owner = Application.Current.MainWindow
            };
            if (selectorWindow.ShowDialog() == true)
            {
                _filterManager.SetActiveProfile(selectorWindow.SelectedProfile);
                UpdateFiltroAtual();
                MessageBox.Show($"Tipo de projeto definido como: {selectorWindow.SelectedProfile.Name}",
                    "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void GerenciarFiltros()
        {
            var filterWindow = new FilterManagerWindow(_filterManager)
            {
                Owner = Application.Current.MainWindow
            };
            filterWindow.ShowDialog();
            UpdateFiltroAtual();
        }

        public void RecarregarFiltros()
        {
            // Substitui o manager antigo por um novo
            _filterManager = new FileFilterManager();
            UpdateFiltroAtual();
            MessageBox.Show("Filtros recarregados com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
