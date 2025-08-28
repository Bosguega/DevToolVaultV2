﻿// Core/Services/ITreeGeneratorService.cs
using DevToolVaultV2.Core.Models;// Para FilterProfile
using System.Collections.Generic;

namespace DevToolVaultV2.Core.Services
{
    public interface ITreeGeneratorService
    {
        List<FileSystemItem> BuildFileSystemTree(string rootPath, FilterProfile profile);
        List<FileSystemItem> GenerateTree(string rootPath);
    }
}