// DevToolVaultV2/Core/Services/TextExportStrategy.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using DevToolVaultV2.Core.Models;

namespace DevToolVaultV2.Core.Services
{
    public class TextExportStrategy : ITextExportStrategy
    {
        private const string Separator = "--------------------------------------------------------------------------------";

        public async Task ExportAsync(List<FileSystemItem> files, string outputPath)
        {
            var encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
            using var writer = new StreamWriter(outputPath, false, encoding);

            foreach (var item in files)
            {
                try
                {
                    // Corrigido: Usar item.FullName
                    var content = await File.ReadAllTextAsync(item.FullName, Encoding.UTF8);

                    await writer.WriteLineAsync(Separator);
                    // Corrigido: Usar item.RelativePath
                    await writer.WriteLineAsync($"# Arquivo: {item.RelativePath}");
                    await writer.WriteLineAsync(Separator);
                    await writer.WriteAsync(content);
                    if (!content.EndsWith("\n")) await writer.WriteLineAsync();
                }
                catch (Exception ex)
                {
                    await writer.WriteLineAsync(Separator);
                    // Corrigido: Usar item.RelativePath
                    await writer.WriteLineAsync($"# Arquivo: {item.RelativePath}");
                    await writer.WriteLineAsync(Separator);
                    await writer.WriteLineAsync($"// Erro ao ler arquivo: {ex.Message}\n");
                }
            }
        }
    }
}