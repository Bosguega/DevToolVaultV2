﻿using System;
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
                var vm = _serviceProvider.GetRequiredService<EstruturaViewModel>();
                var window = new EstruturaWindow(vm)
                {
                    Owner = Application.Current.MainWindow
                };
                window.Show();
                return;
            }

            if (typeof(T) == typeof(ExportarCodigoWindow))
            {
                var vm = _serviceProvider.GetRequiredService<ExportarCodigoViewModel>();
                var window = new ExportarCodigoWindow(vm)
                {
                    Owner = Application.Current.MainWindow
                };
                window.Show();
                return;
            }

            // For other windows, try to get them directly from DI
            var genericWindow = _serviceProvider.GetService<T>();
            if (genericWindow != null)
            {
                genericWindow.Owner = Application.Current.MainWindow;
                genericWindow.Show();
            }
        }

        public void ShowDialog<T>() where T : Window
        {
            var window = _serviceProvider.GetRequiredService<T>();
            window.ShowDialog();
        }
    }
}
