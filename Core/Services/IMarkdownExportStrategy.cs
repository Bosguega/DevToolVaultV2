// Core/Services/IMarkdownExportStrategy.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using DevToolVaultV2.Core.Models;

namespace DevToolVaultV2.Core.Services
{
    public interface IMarkdownExportStrategy
    {
        Task ExportAsync(List<FileSystemItem> files, string outputPath);
    }
}