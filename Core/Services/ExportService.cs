// DevToolVaultV2/Core/Services/ExportService.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DevToolVaultV2.Core.Models;

namespace DevToolVaultV2.Core.Services // Corrigido: Services
{
    public class ExportService : IExportService
    {
        private readonly ITextExportStrategy _textExportStrategy;
        private readonly IMarkdownExportStrategy _markdownExportStrategy;
        private readonly IPdfExportStrategy _pdfExportStrategy;
        private readonly IZipExportStrategy _zipExportStrategy;

        public ExportService(
            ITextExportStrategy textExportStrategy,
            IMarkdownExportStrategy markdownExportStrategy,
            IPdfExportStrategy pdfExportStrategy,
            IZipExportStrategy zipExportStrategy)
        {
            _textExportStrategy = textExportStrategy;
            _markdownExportStrategy = markdownExportStrategy;
            _pdfExportStrategy = pdfExportStrategy;
            _zipExportStrategy = zipExportStrategy;
        }

        public async Task ExportAsync(List<FileSystemItem> files, string outputPath, ExportFormat format)
        {
            if (files == null || files.Count == 0)
                throw new ArgumentException("Nenhum arquivo para exportar.", nameof(files));

            if (string.IsNullOrWhiteSpace(outputPath))
                throw new ArgumentException("Caminho de saída inválido.", nameof(outputPath));

            switch (format)
            {
                case ExportFormat.Text:
                    // Corrigido: outputPath
                    await _textExportStrategy.ExportAsync(files, outputPath);
                    break;

                case ExportFormat.Markdown:
                    // Corrigido: outputPath
                    await _markdownExportStrategy.ExportAsync(files, outputPath);
                    break;

                case ExportFormat.Pdf:
                    // Corrigido: outputPath
                    await _pdfExportStrategy.ExportAsync(files, outputPath);
                    break;

                case ExportFormat.Zip:
                    // Corrigido: outputPath
                    await _zipExportStrategy.ExportAsync(files, outputPath);
                    break;

                default:
                    throw new ArgumentException($"Formato de exportação inválido: {format}");
            }
        }
    }
}