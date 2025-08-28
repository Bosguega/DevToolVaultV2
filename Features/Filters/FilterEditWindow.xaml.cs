// DevToolVaultV2/Features/Filters/FilterEditWindow.xaml.cs
using DevToolVaultV2.Core.Models; // <<< Corrigido o namespace para FilterProfile
using System;
using System.IO; // Adicionado para Path e outras utilidades
using System.Linq; // Adicionado para manipulação de listas
using System.Windows;

namespace DevToolVaultV2.Features.Filters
{
    /// <summary>
    /// Lógica interna para FilterEditWindow.xaml
    /// </summary>
    public partial class FilterEditWindow : Window
    {
        // Propriedade para indicar se é uma edição
        public bool IsEditing { get; private set; }

        // Propriedades para retornar os valores editados (se necessário, embora o objeto seja passado por referência)
        // Mantendo para compatibilidade com código que possa esperar isso, mas o foco é no objeto passado.
        public string ProfileName { get; private set; }
        public string ProfileDescription { get; private set; }

        /// <summary>
        /// Construtor padrão.
        /// </summary>
        public FilterEditWindow()
        {
            InitializeComponent();
            IsEditing = false; // Assume que é um novo perfil por padrão
        }

        /// <summary>
        /// Construtor que aceita um perfil para edição.
        /// </summary>
        /// <param name="profile">O perfil a ser editado.</param>
        public FilterEditWindow(FilterProfile profile) : this()
        {
            LoadProfileData(profile);
        }

        /// <summary>
        /// Construtor que aceita um perfil e um indicador se é uma edição.
        /// </summary>
        /// <param name="profile">O perfil a ser editado ou usado como base para um novo.</param>
        /// <param name="isEditing">True se for uma edição, False se for criação.</param>
        public FilterEditWindow(FilterProfile profile, bool isEditing) : this() // Chama o construtor padrão
        {
            IsEditing = isEditing;
            if (profile != null)
            {
                LoadProfileData(profile);
            }
            // O título da janela pode ser atualizado com base em IsEditing
            this.Title = IsEditing ? "Editar Filtro" : "Novo Filtro";
        }

        /// <summary>
        /// Carrega os dados de um FilterProfile nos controles da janela.
        /// </summary>
        /// <param name="profile">O perfil cujos dados serão carregados.</param>
        private void LoadProfileData(FilterProfile profile)
        {
            txtName.Text = profile.Name ?? "";
            txtDescription.Text = profile.Description ?? "";

            // Se houver outros campos na UI para IgnorePatterns, CodeExtensions etc.
            // eles seriam preenchidos aqui também.
            // Por exemplo, se houvesse TextBoxes ou ListBoxes:
            // txtIgnorePatterns.Text = string.Join(Environment.NewLine, profile.IgnorePatterns ?? new System.Collections.Generic.List<string>());
            // txtCodeExtensions.Text = string.Join(Environment.NewLine, profile.CodeExtensions ?? new System.Collections.Generic.List<string>());
            // chkIgnoreEmptyFolders.IsChecked = profile.IgnoreEmptyFolders;
            // chkShowFileSize.IsChecked = profile.ShowFileSize;
            // chkShowSystemFiles.IsChecked = profile.ShowSystemFiles;
            // chkShowOnlyCodeFiles.IsChecked = profile.ShowOnlyCodeFiles;
        }

        /// <summary>
        /// Salva os dados dos controles da janela em um FilterProfile.
        /// </summary>
        /// <param name="profile">O perfil onde os dados serão salvos.</param>
        private void SaveProfileData(FilterProfile profile)
        {
            if (profile == null) return;

            profile.Name = txtName.Text ?? "";
            profile.Description = txtDescription.Text ?? "";

            // Se houver outros campos na UI, salvar os dados deles aqui também.
            // Por exemplo:
            // if (!string.IsNullOrWhiteSpace(txtIgnorePatterns.Text))
            // {
            //     profile.IgnorePatterns = txtIgnorePatterns.Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            // }
            // else
            // {
            //     profile.IgnorePatterns = new System.Collections.Generic.List<string>();
            // }
            //
            // if (!string.IsNullOrWhiteSpace(txtCodeExtensions.Text))
            // {
            //     profile.CodeExtensions = txtCodeExtensions.Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            // }
            // else
            // {
            //     profile.CodeExtensions = new System.Collections.Generic.List<string>();
            // }
            //
            // profile.IgnoreEmptyFolders = chkIgnoreEmptyFolders.IsChecked == true;
            // profile.ShowFileSize = chkShowFileSize.IsChecked == true;
            // profile.ShowSystemFiles = chkShowSystemFiles.IsChecked == true;
            // profile.ShowOnlyCodeFiles = chkShowOnlyCodeFiles.IsChecked == true;
        }


        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("O nome do perfil não pode estar vazio.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtName.Focus();
                return;
            }

            // Se estiver editando e o nome mudou, ou se for novo, verificar se o nome já existe pode ser útil
            // (dependendo da lógica de negócio). Isso normalmente seria verificado no gerenciador.

            // Atualiza as propriedades de retorno (para compatibilidade)
            ProfileName = txtName.Text;
            ProfileDescription = txtDescription.Text;

            this.DialogResult = true;
            // this.Close(); // Não é estritamente necessário se DialogResult for definido
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            // this.Close(); // Não é estritamente necessário se DialogResult for definido
        }
    }
}