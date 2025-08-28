using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DevToolVaultV2.Core.Models;

namespace DevToolVaultV2.Core.Services
{
    public class FileFilterApplier
    {
        private FileFilterManager _filterManager;

        public FileFilterApplier(FileFilterManager filterManager)
        {
            _filterManager = filterManager ?? throw new ArgumentNullException(nameof(filterManager));
        }

        public bool ShouldIgnoreFile(FileInfo file)
        {
            var profile = _filterManager.GetActiveProfile();
            if (profile == null) return false;

            // Verifica se deve mostrar apenas arquivos de código
            if (profile.ShowOnlyCodeFiles && !IsCodeFile(file, profile))
                return true;

            // Verifica padrões de ignorar
            if (MatchesIgnorePattern(file.Name, profile.IgnorePatterns))
                return true;

            // Verifica arquivos de sistema
            if (!profile.ShowSystemFiles && IsSystemFile(file))
                return true;

            return false;
        }

        public bool ShouldIgnoreDirectory(DirectoryInfo directory)
        {
            var profile = _filterManager.GetActiveProfile();
            if (profile == null) return false;

            // Verifica padrões de ignorar
            if (MatchesIgnorePattern(directory.Name, profile.IgnorePatterns))
                return true;

            // Verifica pastas vazias
            if (profile.IgnoreEmptyFolders && IsEmptyDirectory(directory, profile))
                return true;

            // Verifica pastas de sistema
            if (!profile.ShowSystemFiles && IsSystemDirectory(directory))
                return true;

            return false;
        }

        private bool IsCodeFile(FileInfo file, FilterProfile profile)
        {
            return profile.CodeExtensions.Contains(file.Extension, StringComparer.OrdinalIgnoreCase);
        }

        private bool MatchesIgnorePattern(string name, List<string> ignorePatterns)
        {
            return ignorePatterns.Any(pattern =>
            {
                // Verifica padrões de extensão (*.txt, .dll)
                if (pattern.StartsWith("*."))
                {
                    var ext = pattern.Substring(1);
                    return name.EndsWith(ext, StringComparison.OrdinalIgnoreCase);
                }

                // Verifica padrões de nome (bin, obj, build)
                return name.Equals(pattern, StringComparison.OrdinalIgnoreCase);
            });
        }

        private bool IsSystemFile(FileInfo file)
        {
            var attributes = file.Attributes;
            return attributes.HasFlag(FileAttributes.Hidden) ||
                   attributes.HasFlag(FileAttributes.System);
        }

        private bool IsSystemDirectory(DirectoryInfo directory)
        {
            var attributes = directory.Attributes;
            return attributes.HasFlag(FileAttributes.Hidden) ||
                   attributes.HasFlag(FileAttributes.System);
        }

        private bool IsEmptyDirectory(DirectoryInfo directory, FilterProfile profile)
        {
            try
            {
                var items = directory.GetFileSystemInfos();

                // Se não deve mostrar arquivos de sistema, filtra-os
                if (!profile.ShowSystemFiles)
                {
                    items = items.Where(item =>
                        !item.Attributes.HasFlag(FileAttributes.Hidden) &&
                        !item.Attributes.HasFlag(FileAttributes.System)).ToArray();
                }

                return items.Length == 0;
            }
            catch (UnauthorizedAccessException)
            {
                // Se não tiver acesso, considera não vazio para evitar exclusão acidental
                return false;
            }
        }
    }
}