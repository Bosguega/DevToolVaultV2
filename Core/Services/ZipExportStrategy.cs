// DevToolVaultV2_Refatorado/Core/Services/ZipExportStrategy.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using DevToolVaultV2.Core.Models;

namespace DevToolVaultV2.Core.Services
{
    public class ZipExportStrategy : IZipExportStrategy
    {
        public async Task ExportAsync(List<FileSystemItem> files, string outputPath)
        {
            await Task.Run(() =>
            {
                // Corrigido: Removido espaço na string
                var tempDir = Path.Combine(Path.GetTempPath(), "DevToolVaultV2_Temp_" + Guid.NewGuid());
                Directory.CreateDirectory(tempDir);

                try
                {
                    Parallel.ForEach(files, item =>
                    {
                        try
                        {
                            var safePath = SanitizePath(item.RelativePath);
                            if (string.IsNullOrEmpty(safePath)) return;

                            // Corrigido: targetDir
                            var targetDir = Path.Combine(tempDir, safePath);
                            Directory.CreateDirectory(targetDir);
                            // Corrigido: Usar item.FullName
                            File.Copy(item.FullName, Path.Combine(targetDir, item.Name), true);
                        }
                        catch { /* Ignora */ }
                    });

                    if (File.Exists(outputPath)) File.Delete(outputPath);
                    ZipFile.CreateFromDirectory(tempDir, outputPath);
                }
                finally
                {
                    if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
                }
            });
        }

        private string SanitizePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return string.Empty;
            return string.Join(Path.DirectorySeparatorChar.ToString(),
                path.Split(Path.DirectorySeparatorChar)
                    .Where(p => !string.IsNullOrEmpty(p) && p != "." && p != ".."));
        }
    }
}