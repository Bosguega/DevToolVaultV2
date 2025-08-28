using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using DevToolVaultV2.Features.Structure;
using DevToolVaultV2.Features.Export;
using DevToolVaultV2.Core.Services;

namespace DevToolVaultV2.Core.Services
{
    public class AppNavigationService : IAppNavigationService
    {
        private readonly IServiceProvider _serviceProvider;

        public AppNavigationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void Show<T>() where T : Window
        {
            if (typeof(T) == typeof(EstruturaWindow))
            {
                var vm = _serviceProvider.GetService<EstruturaViewModel>();
                var window = new EstruturaWindow(vm)
                {
                    Owner = Application.Current.MainWindow
                };
                window.Show();
                return;
            }

            if (typeof(T) == typeof(ExportarCodigoWindow))
            {
                var filterManager = _serviceProvider.GetService<FileFilterManager>();
                var exportService = _serviceProvider.GetService<IExportService>();

                var vm = new ExportarCodigoViewModel(filterManager, exportService);
                var window = new ExportarCodigoWindow(vm)
                {
                    Owner = Application.Current.MainWindow
                };
                window.Show();
                return;
            }

            var genericWindow = _serviceProvider.GetService<T>();
            genericWindow?.Show();
        }

        public void ShowDialog<T>() where T : Window
        {
            var window = _serviceProvider.GetRequiredService<T>();
            window.ShowDialog();
        }
    }
}
