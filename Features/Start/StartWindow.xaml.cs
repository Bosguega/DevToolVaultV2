using System.Windows;
using DevToolVaultV2.Core.Services;

namespace DevToolVaultV2.Features.Start
{
    public partial class StartWindow : Window
    {
        public StartWindow(FileFilterManager filterManager, IAppNavigationService navigationService)
        {
            InitializeComponent();
            DataContext = new StartWindowViewModel(filterManager, navigationService);
        }
    }
}
