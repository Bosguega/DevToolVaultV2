﻿﻿﻿﻿﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DevToolVaultV2.Core.Models;

namespace DevToolVaultV2.Core.Services
{
    public class TreeGeneratorService : ITreeGeneratorService
    {
        private readonly FileFilterApplier _filterApplier;

        public TreeGeneratorService(FileFilterManager filterManager)
        {
            _filterApplier = new FileFilterApplier(filterManager ?? throw new ArgumentNullException(nameof(filterManager)));
        }

        public List<FileSystemItem> GenerateTree(string rootPath)
        {
            // Use default active profile from filter manager
            return BuildFileSystemTree(rootPath, null);
        }

        public List<FileSystemItem> BuildFileSystemTree(string rootPath, FilterProfile profile)
        {
            var rootDirectory = new DirectoryInfo(rootPath);
            return CreateDirectoryNode(rootDirectory, rootPath);
        }

        private List<FileSystemItem> CreateDirectoryNode(DirectoryInfo directory, string rootPath)
        {
            var nodes = new List<FileSystemItem>();

            try
            {
                // Processar subdiretórios
                foreach (var subDir in directory.GetDirectories())
                {
                    if (_filterApplier.ShouldIgnoreDirectory(subDir))
                        continue;

                    var dirNode = new FileSystemItem
                    {
                        Name = subDir.Name,
                        FullPath = subDir.FullName,
                        RelativePath = Path.GetRelativePath(rootPath, subDir.FullName),
                        IsDirectory = true,
                        IsExpanded = false,
                        IsChecked = false,
                        Children = CreateDirectoryNode(subDir, rootPath)
                    };

                    // Definir parentesco
                    foreach (var child in dirNode.Children)
                    {
                        child.Parent = dirNode;
                    }

                    nodes.Add(dirNode);
                }

                // Processar arquivos
                foreach (var file in directory.GetFiles())
                {
                    if (_filterApplier.ShouldIgnoreFile(file))
                        continue;

                    var fileNode = new FileSystemItem
                    {
                        Name = file.Name,
                        FullPath = file.FullName,
                        RelativePath = Path.GetRelativePath(rootPath, file.FullName),
                        IsDirectory = false,
                        IsChecked = false
                    };

                    nodes.Add(fileNode);
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Ignorar diretórios sem permissão
            }

            return nodes;
        }
    }
}