// DevToolVaultV2_Refatorado/Core/Services/FileFilterManager.cs
using DevToolVaultV2.Core.Models; // Assumindo que FilterProfile está aqui
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace DevToolVaultV2.Core.Services
{
    public class FileFilterManager // Ou FileFilterManagerService
    {
        private readonly string _filtersDirectory;
        private readonly List<FilterProfile> _profiles;
        private FilterProfile _activeProfile;

        public event EventHandler ActiveProfileChanged;

        public FileFilterManager(string filtersDirectory = null)
        {
            _filtersDirectory = filtersDirectory ?? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "DevToolVaultV2", "Filters");

            _profiles = new List<FilterProfile>();
            EnsureDirectoryExists();
            LoadDefaultFilters(); // Carrega os perfis embutidos
            LoadCustomProfiles(); // Carrega os perfis personalizados do usuário
        }

        private void EnsureDirectoryExists()
        {
            if (!Directory.Exists(_filtersDirectory))
                Directory.CreateDirectory(_filtersDirectory);
        }

        /// <summary>
        /// Carrega os perfis embutidos padrão.
        /// </summary>
        private void LoadDefaultFilters()
        {
            var builtInProfiles = BuiltInProfiles.GetProfiles(); // <<< Usando a nova classe
            foreach (var profile in builtInProfiles)
            {
                // Garante que os perfis embutidos sejam marcados como tal
                profile.IsBuiltIn = true;
                _profiles.Add(profile);
            }
        }

        /// <summary>
        /// Carrega os perfis personalizados salvos pelo usuário.
        /// </summary>
        private void LoadCustomProfiles()
        {
            try
            {
                var filterFiles = Directory.GetFiles(_filtersDirectory, "*.json");
                foreach (var file in filterFiles)
                {
                    try
                    {
                        var json = File.ReadAllText(file);
                        var profile = JsonSerializer.Deserialize<FilterProfile>(json);
                        if (profile != null)
                        {
                            // Garante que perfis carregados não sejam considerados embutidos
                            profile.IsBuiltIn = false;
                            _profiles.Add(profile);
                        }
                    }
                    catch
                    {
                        // Ignora arquivos inválidos
                    }
                }
            }
            catch
            {
                // Ignora erros ao carregar perfis
            }
        }

        /// <summary>
        /// Recarrega os perfis a partir do armazenamento padrão (embutidos e do arquivo).
        /// </summary>
        public void ReloadProfiles()
        {
            var oldActiveProfileName = _activeProfile?.Name;

            _profiles.Clear();
            LoadDefaultFilters(); // Recarrega os perfis embutidos
            LoadCustomProfiles(); // Recarrega os perfis salvos pelo usuário

            // Tenta restaurar o perfil ativo
            if (!string.IsNullOrEmpty(oldActiveProfileName))
            {
                var profileToActivate = _profiles.FirstOrDefault(p => p.Name.Equals(oldActiveProfileName, StringComparison.OrdinalIgnoreCase));
                if (profileToActivate != null)
                {
                    SetActiveProfile(profileToActivate);
                }
                else
                {
                    // Se não encontrar, define o primeiro perfil como ativo, ou null
                    SetActiveProfile(_profiles.FirstOrDefault());
                }
            }
            // Se oldActiveProfileName for null/empty, mantém _activeProfile como null.
        }


        public IEnumerable<FilterProfile> GetProfiles() => _profiles;

        public FilterProfile GetActiveProfile() => _activeProfile;

        public void SetActiveProfile(FilterProfile profile)
        {
            if (profile != null && !_profiles.Contains(profile))
            {
                throw new ArgumentException("O perfil não está na lista de perfis gerenciados.", nameof(profile));
            }

            _activeProfile = profile;
            ActiveProfileChanged?.Invoke(this, EventArgs.Empty);
        }

        public void SaveProfile(FilterProfile profile)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));

            if (profile.IsBuiltIn)
            {
                throw new ArgumentException("Perfis embutidos não podem ser salvos.", nameof(profile));
            }

            // Se for um novo perfil (não está na lista), adiciona
            if (!_profiles.Contains(profile))
            {
                profile.IsBuiltIn = false; // Garante que novos perfis não sejam embutidos
                _profiles.Add(profile);
            }

            try
            {
                var fileName = Path.Combine(_filtersDirectory, $"{profile.Name}.json");
                var json = JsonSerializer.Serialize(profile, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(fileName, json);
            }
            catch (Exception ex)
            {
                // Em uma aplicação real, você pode querer logar o erro ou lançar uma exceção mais específica
                System.Diagnostics.Debug.WriteLine($"Erro ao salvar perfil: {ex.Message}");
                throw; // Relança para que o chamador possa tratar
            }
        }

        public void DeleteProfile(FilterProfile profile)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));

            if (profile.IsBuiltIn)
            {
                throw new ArgumentException("Perfis embutidos não podem ser excluídos.", nameof(profile));
            }

            if (_profiles.Remove(profile))
            {
                try
                {
                    var fileName = Path.Combine(_filtersDirectory, $"{profile.Name}.json");
                    if (File.Exists(fileName))
                    {
                        File.Delete(fileName);
                    }
                }
                catch (Exception ex)
                {
                    // Em uma aplicação real, você pode querer logar o erro
                    System.Diagnostics.Debug.WriteLine($"Erro ao excluir arquivo do perfil: {ex.Message}");
                    // Pode optar por relançar ou apenas continuar
                }

                // Se o perfil excluído era o ativo, defina outro ou null
                if (_activeProfile == profile)
                {
                    SetActiveProfile(_profiles.FirstOrDefault());
                }
            }
        }
    }
}