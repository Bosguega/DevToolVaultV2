// Core/Services/ITextExportStrategy.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using DevToolVaultV2.Core.Models;

namespace DevToolVaultV2.Core.Services
{
    public interface ITextExportStrategy
    {
        Task ExportAsync(List<FileSystemItem> files, string outputPath);
    }
}