// DevToolVaultV2/Core/ViewModels/BaseViewModel.cs
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DevToolVaultV2.Core.ViewModels
{
    /// <summary>
    /// Classe base para todos os ViewModels, implementa INotifyPropertyChanged.
    /// </summary>
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Dispara a notificação de propriedade para a UI.
        /// </summary>
        /// <param name="propertyName">Nome da propriedade</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Atualiza o valor de um campo e dispara a notificação se o valor mudou.
        /// </summary>
        /// <typeparam name="T">Tipo da propriedade</typeparam>
        /// <param name="field">Referência do campo privado</param>
        /// <param name="value">Novo valor</param>
        /// <param name="propertyName">Nome da propriedade (opcional)</param>
        /// <returns>True se o valor foi alterado</returns>
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
