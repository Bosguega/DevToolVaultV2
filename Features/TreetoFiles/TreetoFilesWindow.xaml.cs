using System.Windows;

namespace DevToolVaultV2.Features.TreetoFiles
{
    public partial class TreetoFilesWindow : Window
    {
        public TreetoFilesWindow(TreetoFilesViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}