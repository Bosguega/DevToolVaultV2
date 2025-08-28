using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using DevToolVaultV2.Core.Services;
using DevToolVaultV2.Features.Structure;
using DevToolVaultV2.Features.Export;
using DevToolVaultV2.Features.Filters;
using DevToolVaultV2.Features;

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
            SelecionarTipoProjetoCommand = new RelayCommand(_ => SelecionarTipoProjeto());
            VisualizarEstruturaCommand = new RelayCommand(_ => _navigationService.Show<EstruturaWindow>());
            ExportarCodigoCommand = new RelayCommand(_ => _navigationService.Show<ExportarCodigoWindow>());
            GerenciarFiltrosCommand = new RelayCommand(_ => GerenciarFiltros());
            RecarregarFiltrosCommand = new RelayCommand(_ => RecarregarFiltros());
            SairCommand = new RelayCommand(_ => Application.Current.Shutdown());
            SobreCommand = new RelayCommand(_ => MessageBox.Show(
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

        // RelayCommand para binding
        public class RelayCommand : ICommand
        {
            private readonly System.Action<object> _execute;
            private readonly System.Func<object, bool> _canExecute;

            public RelayCommand(System.Action<object> execute, System.Func<object, bool> canExecute = null)
            {
                _execute = execute;
                _canExecute = canExecute;
            }

            public bool CanExecute(object parameter) => _canExecute?.Invoke(parameter) ?? true;
            public void Execute(object parameter) => _execute(parameter);
            public event System.EventHandler CanExecuteChanged
            {
                add => CommandManager.RequerySuggested += value;
                remove => CommandManager.RequerySuggested -= value;
            }
        }
    }
}
