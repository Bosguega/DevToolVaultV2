
using System.Collections.Generic;

namespace DevToolVaultV2.Core.Models
{
    public class FilterProfile
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<string> IgnorePatterns { get; set; } = new();
        public List<string> CodeExtensions { get; set; } = new();
        public bool IgnoreEmptyFolders { get; set; }
        public bool ShowFileSize { get; set; }
        public bool ShowSystemFiles { get; set; }
        public bool ShowOnlyCodeFiles { get; set; }
        public bool IsBuiltIn { get; set; }
    }
}