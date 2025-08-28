// Core/Services/IZipExportStrategy.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using DevToolVaultV2.Core.Models;

namespace DevToolVaultV2.Core.Services
{
    public interface IZipExportStrategy
    {
        Task ExportAsync(List<FileSystemItem> files, string outputPath);
    }
}