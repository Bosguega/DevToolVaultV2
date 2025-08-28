// DevToolVaultV2_Refatorado/Core/Services/PdfExportStrategy.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DevToolVaultV2.Core.Models;
using iText.IO.Font;
using iText.IO.Font.Constants;   // ✅ precisa desse namespace para StandardFonts
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;

namespace DevToolVaultV2.Core.Services
{
    public class PdfExportStrategy : IPdfExportStrategy
    {
        private const string Separator = "--------------------------------------------------------------------------------";

        public async Task ExportAsync(List<FileSystemItem> files, string outputPath)
        {
            await Task.Run(() =>
            {
                using var writer = new PdfWriter(outputPath);
                using var pdf = new PdfDocument(writer);
                using var document = new Document(pdf);

                // ✅ Usa as constantes corretas do iText7
                var font = PdfFontFactory.CreateFont(StandardFonts.COURIER);
                var smallFont = PdfFontFactory.CreateFont(StandardFonts.COURIER, PdfEncodings.WINANSI);

                foreach (var item in files)
                {
                    try
                    {
                        // Corrigido: Usar item.FullName
                        var content = File.ReadAllText(item.FullName);

                        // Cabeçalho
                        document.Add(new Paragraph(Separator)
                             .SetFont(font)
                            .SetFontSize(8)
                            .SetMarginBottom(2));

                        // Corrigido: Usar item.RelativePath
                        document.Add(new Paragraph($"# Arquivo: {item.RelativePath}")
                            .SetFont(font)
                            .SetFontSize(10)
                            .SetMarginBottom(2));

                        document.Add(new Paragraph(Separator)
                            .SetFont(font)
                            .SetFontSize(8)
                            .SetMarginBottom(6));

                        // Conteúdo do arquivo
                        document.Add(new Paragraph(content)
                            .SetFont(smallFont)
                            .SetFontSize(9)
                            .SetMarginBottom(12));
                    }
                    catch (Exception ex)
                    {
                        document.Add(new Paragraph(Separator)
                            .SetFont(font)
                            .SetFontSize(8));

                        // Corrigido: Usar item.RelativePath
                        document.Add(new Paragraph($"# Arquivo: {item.RelativePath}")
                            .SetFont(font)
                            .SetFontSize(10));

                        document.Add(new Paragraph(Separator)
                             .SetFont(font)
                            .SetFontSize(8));

                        document.Add(new Paragraph($"Erro ao ler arquivo: {ex.Message}")
                            .SetFont(font)
                            .SetFontSize(9)
                            .SetFontColor(iText.Kernel.Colors.ColorConstants.RED));

                        document.Add(new Paragraph("\n"));
                    }
                }
            });
        }
    }
}