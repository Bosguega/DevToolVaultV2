﻿using System.Windows;
using DevToolVaultV2.Core.Services;

namespace DevToolVaultV2.Features.Start
{
    public partial class StartWindow : Window
    {
        public StartWindow(StartWindowViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
