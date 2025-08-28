// DevToolVaultV2_Refatorado/Core/Services/BuiltInProfiles.cs
using DevToolVaultV2.Core.Models; // Assumindo que FilterProfile está aqui
using System.Collections.Generic;

namespace DevToolVaultV2.Core.Services
{
    /// <summary>
    /// Fornece acesso aos perfis de filtro embutidos padrão.
    /// </summary>
    public static class BuiltInProfiles
    {
        /// <summary>
        /// Obtém a lista de perfis embutidos.
        /// </summary>
        /// <returns>Uma lista de FilterProfile embutidos.</returns>
        public static List<FilterProfile> GetProfiles()
        {
            return new List<FilterProfile>
            {
                new FilterProfile
                {
                    Name = "Flutter",
                    Description = "Filtro para projetos Flutter",
                    IgnorePatterns = new List<string>
                    {
                        "android", "ios", "linux", "macos", "windows", "web",
                        "build", "dart_tool", ".dart_tool", ".pub", ".flutter-plugins",
                        ".idea", ".vscode", ".vs", ".git", ".svn", ".hg", ".bzr", "_darcs",
                        "node_modules", "bower_components", "jspm_packages", ".nuget", "packages",
                        "*.tmp", "*.temp", "*.log", "*.cache"
                    },
                    CodeExtensions = new List<string> { ".dart", ".yaml", ".yml", ".json", ".xml", ".html", ".css", ".js", ".ts" },
                    IgnoreEmptyFolders = true,
                    ShowFileSize = false,
                    ShowSystemFiles = false,
                    ShowOnlyCodeFiles = false,
                    IsBuiltIn = true
                },
                new FilterProfile
                {
                    Name = ".NET",
                    Description = "Filtro para projetos .NET",
                    IgnorePatterns = new List<string>
                    {
                        "bin", "obj", "Debug", "Release", "x64", "x86", "AnyCPU", ".vs", "vs", ".vscode",
                        "*.exe", "*.dll", "*.pdb", "*.config", "*.exe.config",
                        "*.manifest", "*.application", "*.deploy",
                        ".DS_Store", "Thumbs.db", "desktop.ini",
                        "*.log", "*.cache", "*.tmp", "node_modules"
                    },
                    CodeExtensions = new List<string> { ".cs", ".vb", ".fs", ".xaml", ".xml", ".json", ".config", ".cshtml", ".razor" },
                    IgnoreEmptyFolders = true,
                    ShowFileSize = false,
                    ShowSystemFiles = false,
                    ShowOnlyCodeFiles = false,
                    IsBuiltIn = true
                },
                new FilterProfile
                {
                    Name = "Node.js",
                    Description = "Filtro para projetos Node.js",
                    IgnorePatterns = new List<string>
                    {
                        "node_modules", ".nyc_output", "coverage", ".cache", "dist", "build", "out", "*.map",
                        ".DS_Store", "Thumbs.db", "desktop.ini", "*.log", "*.cache", "*.tmp"
                    },
                    CodeExtensions = new List<string> { ".ts", ".tsx", ".jsx", ".js", ".json", ".md", ".yml", ".yaml" },
                    IgnoreEmptyFolders = true,
                    ShowFileSize = false,
                    ShowSystemFiles = false,
                    ShowOnlyCodeFiles = false,
                    IsBuiltIn = true
                },
                new FilterProfile
                {
                    Name = "Android",
                    Description = "Filtro para projetos Android",
                    IgnorePatterns = new List<string>
                    {
                        "build", ".gradle", ".idea", "captures", ".cxx", "app/build", "app/build/intermediates", "app/build/generated",
                        "*.apk", "*.aab", "*.jar", "*.aar", "*.dex", "*.R.java", "*.BuildConfig.java",
                        ".DS_Store", "Thumbs.db", "desktop.ini", "*.log", "*.cache", "*.tmp"
                    },
                    CodeExtensions = new List<string> { ".java", ".kt", ".xml", ".gradle", ".properties", ".json" },
                    IgnoreEmptyFolders = true,
                    ShowFileSize = false,
                    ShowSystemFiles = false,
                    ShowOnlyCodeFiles = false,
                    IsBuiltIn = true
                },
                new FilterProfile
                {
                    Name = "Web",
                    Description = "Filtro para projetos Web",
                    IgnorePatterns = new List<string>
                    {
                        "dist", "build", "out", ".cache", ".tmp", "node_modules", ".nyc_output", "coverage",
                        "*.min.js", "*.min.css", "*.bundle.js", "*.bundle.css", "*.map",
                        ".DS_Store", "Thumbs.db", "desktop.ini", "*.log", "*.cache", "*.tmp"
                    },
                    CodeExtensions = new List<string> { ".html", ".css", ".scss", ".sass", ".less", ".js", ".ts", ".jsx", ".tsx", ".json", ".md", ".yml", ".yaml" },
                    IgnoreEmptyFolders = true,
                    ShowFileSize = false,
                    ShowSystemFiles = false,
                    ShowOnlyCodeFiles = false,
                    IsBuiltIn = true
                },
                new FilterProfile
                {
                    Name = "WPF",
                    Description = "Filtro para projetos WPF",
                    IgnorePatterns = new List<string>
                    {
                        "bin", "obj", "Debug", "Release", "x64", "x86", "AnyCPU", ".vs", "vs", ".vscode",
                        "*.exe", "*.dll", "*.pdb", "*.config", "*.exe.config",
                        "*.manifest", "*.application", "*.deploy",
                        "*.Designer.cs", "*.g.cs", "*.g.i.cs", "*.i.cs", // Arquivos gerados
                        ".DS_Store", "Thumbs.db", "desktop.ini", "*.log", "*.cache", "*.tmp"
                    },
                    CodeExtensions = new List<string> { ".xaml", ".cs", ".vb", ".xml", ".json", ".config" },
                    IgnoreEmptyFolders = true,
                    ShowFileSize = false,
                    ShowSystemFiles = false,
                    ShowOnlyCodeFiles = false,
                    IsBuiltIn = true
                }
                // Adicione mais perfis embutidos aqui, se desejar, copiando o padrão acima
            };
        }
    }
}