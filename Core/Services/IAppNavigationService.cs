using System.Windows;

namespace DevToolVaultV2.Core.Services
{
    public interface IAppNavigationService
    {
        void Show<T>() where T : Window;
        void ShowDialog<T>() where T : Window;
    }
}
