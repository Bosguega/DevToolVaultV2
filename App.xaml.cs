// App.xaml.cs
using System;
using System.Windows;
using System.Windows.Threading; // Adicione este using
using DevToolVaultV2.Features.Filters;
using DevToolVaultV2.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using DevToolVaultV2.Features.Structure;
using DevToolVaultV2.Core.Models; // Adicione se não estiver

namespace DevToolVaultV2
{
    public partial class App : Application
    {
        public IServiceProvider ServiceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            // Adiciona manipuladores globais de exceção
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            var services = new ServiceCollection();

            // Serviços
            services.AddSingleton<FileFilterManager>();
            services.AddSingleton<FileStatistics>();
            services.AddSingleton<IAppNavigationService, AppNavigationService>();
            services.AddSingleton<ITreeGeneratorService, TreeGeneratorService>();
            services.AddSingleton<TreeToMermaidConverter>();
            services.AddSingleton<MermaidToTreeConverter>();
            services.AddSingleton<FileSystemCreator>();

            // Estratégias de exportação
            services.AddTransient<ITextExportStrategy, TextExportStrategy>();
            services.AddTransient<IMarkdownExportStrategy, MarkdownExportStrategy>();
            services.AddTransient<IPdfExportStrategy, PdfExportStrategy>();
            services.AddTransient<IZipExportStrategy, ZipExportStrategy>();
            services.AddSingleton<IExportService, ExportService>();

            // Janelas
            services.AddTransient<Features.Start.StartWindow>();
            services.AddTransient<Features.Structure.EstruturaWindow>();
            services.AddTransient<Features.Export.ExportarCodigoWindow>();
            services.AddTransient<Features.Filters.FilterManagerWindow>();
            services.AddTransient<Features.Filters.ProjectTypeSelectorWindow>();

            // ViewModels
            services.AddTransient<DevToolVaultV2.Features.Start.StartWindowViewModel>();
            services.AddTransient<DevToolVaultV2.Features.Structure.EstruturaViewModel>();
            services.AddTransient<DevToolVaultV2.Features.Export.ExportarCodigoViewModel>();
            services.AddTransient<DevToolVaultV2.Features.TreetoFiles.TreetoFilesViewModel>();

            ServiceProvider = services.BuildServiceProvider();

            var mainWindow = ServiceProvider.GetRequiredService<Features.Start.StartWindow>();
            MainWindow = mainWindow; // Set as MainWindow for proper Owner property
            mainWindow.Show();

            base.OnStartup(e);
        }

        // Manipulador para exceções na thread UI (Dispatcher)
        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // Logar o erro (pode usar System.Diagnostics.Debug.WriteLine ou MessageBox para debug)
            MessageBox.Show($"Erro não tratado na UI Thread: {e.Exception.Message}\n\nStack Trace:\n{e.Exception.StackTrace}", "Erro Fatal", MessageBoxButton.OK, MessageBoxImage.Error);

            // Marca como tratado para evitar o fechamento (cuidado: pode deixar o app em estado inconsistente)
            // e.Handled = true; 

            // Ou deixe o aplicativo fechar após mostrar o erro:
            e.Handled = false; // Padrão, permite o fechamento
        }

        // Manipulador para exceções em outras threads
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            string errorMessage = "Erro não tratado em thread de background.";
            if (e.ExceptionObject is Exception ex)
            {
                errorMessage = $"Erro não tratado: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}";
            }
            else
            {
                errorMessage = $"Erro não tratado (objeto não Exception): {e.ExceptionObject?.ToString() ?? "null"}";
            }

            MessageBox.Show(errorMessage, "Erro Fatal", MessageBoxButton.OK, MessageBoxImage.Error);
            // O aplicativo provavelmente fechará após este manipulador
        }


        protected override void OnExit(ExitEventArgs e)
        {
            if (ServiceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
            base.OnExit(e);
        }
    }
}