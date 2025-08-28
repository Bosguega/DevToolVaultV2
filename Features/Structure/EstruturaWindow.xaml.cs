// DevToolVaultV2/Features/Structure/EstruturaWindow.xaml.cs
using System.Windows;
using DevToolVaultV2.Core.Services; // Se ainda for usado para algo específico no code-behind
using Microsoft.Extensions.DependencyInjection; // Para IServiceProvider
using System;
using System.Windows.Controls;
using DevToolVaultV2.Converters;

namespace DevToolVaultV2.Features.Structure
{
    public partial class EstruturaWindow : Window
    {
        // Construtor atualizado para receber o ViewModel injetado
        public EstruturaWindow(EstruturaViewModel viewModel) // ViewModel injetado
        {
            InitializeComponent();
            // Define o DataContext para o ViewModel injetado
            DataContext = viewModel;
        }
    }
}