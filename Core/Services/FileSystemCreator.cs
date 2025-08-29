using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DevToolVaultV2.Core.Services
{
    public class FileSystemCreator
    {
        public class CreationOptions
        {
            public bool CreateEmptyFiles { get; set; } = true;
            public bool OverwriteExisting { get; set; } = false;
            public string DefaultFileContent { get; set; } = string.Empty;
            public Encoding FileEncoding { get; set; } = Encoding.UTF8;
        }

        public class CreationResult
        {
            public bool IsSuccess { get; set; }
            public string ErrorMessage { get; set; }
            public List<string> CreatedDirectories { get; set; } = new List<string>();
            public List<string> CreatedFiles { get; set; } = new List<string>();
            public List<string> SkippedItems { get; set; } = new List<string>();
        }

        public CreationResult CreateFileStructure(string baseDirectory, List<TreeToMermaidConverter.TreeNode> nodes, CreationOptions options = null)
        {
            options ??= new CreationOptions();
            
            var result = new CreationResult { IsSuccess = false };

            if (string.IsNullOrWhiteSpace(baseDirectory))
            {
                result.ErrorMessage = "Base directory path cannot be empty";
                return result;
            }

            if (nodes == null || !nodes.Any())
            {
                result.ErrorMessage = "No nodes provided for creation";
                return result;
            }

            try
            {
                // Ensure base directory exists
                if (!Directory.Exists(baseDirectory))
                {
                    Directory.CreateDirectory(baseDirectory);
                    result.CreatedDirectories.Add(baseDirectory);
                }

                // Sort nodes by level to ensure parent directories are created first
                var sortedNodes = nodes.OrderBy(n => n.Level).ThenBy(n => n.FullPath).ToList();

                foreach (var node in sortedNodes)
                {
                    var fullPath = Path.Combine(baseDirectory, node.FullPath.Replace('/', Path.DirectorySeparatorChar));

                    if (node.IsDirectory)
                    {
                        CreateDirectory(fullPath, result, options);
                    }
                    else
                    {
                        CreateFile(fullPath, result, options);
                    }
                }

                result.IsSuccess = true;
            }
            catch (Exception ex)
            {
                result.ErrorMessage = ex.Message;
                result.IsSuccess = false;
            }

            return result;
        }

        public CreationResult CreateFileStructureFromMermaid(string baseDirectory, string mermaidDiagram, CreationOptions options = null)
        {
            var converter = new TreeToMermaidConverter();
            var nodes = converter.ParseMermaidToNodes(mermaidDiagram);
            
            if (!nodes.Any())
            {
                return new CreationResult
                {
                    IsSuccess = false,
                    ErrorMessage = "No valid nodes found in Mermaid diagram"
                };
            }

            return CreateFileStructure(baseDirectory, nodes, options);
        }

        private void CreateDirectory(string directoryPath, CreationResult result, CreationOptions options)
        {
            try
            {
                if (Directory.Exists(directoryPath))
                {
                    if (!options.OverwriteExisting)
                    {
                        result.SkippedItems.Add($"Directory already exists: {directoryPath}");
                        return;
                    }
                }

                Directory.CreateDirectory(directoryPath);
                
                if (!result.CreatedDirectories.Contains(directoryPath))
                {
                    result.CreatedDirectories.Add(directoryPath);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create directory '{directoryPath}': {ex.Message}", ex);
            }
        }

        private void CreateFile(string filePath, CreationResult result, CreationOptions options)
        {
            try
            {
                // Ensure parent directory exists
                var directoryPath = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                    if (!result.CreatedDirectories.Contains(directoryPath))
                    {
                        result.CreatedDirectories.Add(directoryPath);
                    }
                }

                if (File.Exists(filePath))
                {
                    if (!options.OverwriteExisting)
                    {
                        result.SkippedItems.Add($"File already exists: {filePath}");
                        return;
                    }
                }

                if (options.CreateEmptyFiles)
                {
                    File.WriteAllText(filePath, options.DefaultFileContent, options.FileEncoding);
                }
                else
                {
                    using (File.Create(filePath)) { } // Create empty file
                }

                result.CreatedFiles.Add(filePath);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create file '{filePath}': {ex.Message}", ex);
            }
        }

        public static string GetCreationSummary(CreationResult result)
        {
            var summary = new StringBuilder();
            
            if (result.IsSuccess)
            {
                summary.AppendLine("File structure created successfully!");
                summary.AppendLine($"Directories created: {result.CreatedDirectories.Count}");
                summary.AppendLine($"Files created: {result.CreatedFiles.Count}");
                
                if (result.SkippedItems.Any())
                {
                    summary.AppendLine($"Items skipped: {result.SkippedItems.Count}");
                }
            }
            else
            {
                summary.AppendLine($"Failed to create file structure: {result.ErrorMessage}");
            }

            return summary.ToString();
        }
    }
}