using DevToolVaultV2.Features.Export;
using System.Windows;

namespace DevToolVaultV2.Features.Export
{
    public partial class ExportarCodigoWindow : Window
    {
        public ExportarCodigoWindow(ExportarCodigoViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
