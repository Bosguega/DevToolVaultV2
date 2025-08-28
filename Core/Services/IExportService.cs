// Core/Services/IExportService.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using DevToolVaultV2.Core.Models;

namespace DevToolVaultV2.Core.Services
{
    public interface IExportService
    {
        Task ExportAsync(List<FileSystemItem> files, string outputPath, ExportFormat format);
    }

    public enum ExportFormat
    {
        Text,
        Markdown,
        Pdf,
        Zip
    }
}