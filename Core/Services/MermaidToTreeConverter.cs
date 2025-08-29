using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DevToolVaultV2.Core.Services
{
    public class MermaidToTreeConverter
    {
        public class ConversionResult
        {
            public string TreeText { get; set; }
            public bool IsSuccess { get; set; }
            public string ErrorMessage { get; set; }
            public List<TreeNode> ParsedNodes { get; set; }
        }

        public class TreeNode
        {
            public string Name { get; set; }
            public bool IsDirectory { get; set; }
            public int Level { get; set; }
            public string NodeId { get; set; }
            public string FullPath { get; set; }
            public List<TreeNode> Children { get; set; } = new List<TreeNode>();
        }

        public ConversionResult ConvertMermaidToTree(string mermaidDiagram, bool useAsciiFormat = true)
        {
            if (string.IsNullOrWhiteSpace(mermaidDiagram))
            {
                return new ConversionResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Mermaid diagram is empty",
                    TreeText = string.Empty,
                    ParsedNodes = new List<TreeNode>()
                };
            }

            try
            {
                var nodes = ParseMermaidDiagram(mermaidDiagram);
                if (!nodes.Any())
                {
                    return new ConversionResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "No valid nodes found in Mermaid diagram",
                        TreeText = string.Empty,
                        ParsedNodes = new List<TreeNode>()
                    };
                }

                var treeText = useAsciiFormat 
                    ? GenerateAsciiTree(nodes) 
                    : GenerateIconTree(nodes);

                return new ConversionResult
                {
                    IsSuccess = true,
                    TreeText = treeText,
                    ParsedNodes = nodes,
                    ErrorMessage = null
                };
            }
            catch (Exception ex)
            {
                return new ConversionResult
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message,
                    TreeText = $"Error converting Mermaid to tree: {ex.Message}",
                    ParsedNodes = new List<TreeNode>()
                };
            }
        }

        private List<TreeNode> ParseMermaidDiagram(string mermaidDiagram)
        {
            var nodeMap = new Dictionary<string, TreeNode>();
            var relationships = new List<(string parent, string child)>();

            var lines = mermaidDiagram.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines.Skip(1)) // Skip "graph TD" or similar
            {
                var trimmedLine = line.Trim();
                if (string.IsNullOrEmpty(trimmedLine) || 
                    trimmedLine.StartsWith("graph") || 
                    trimmedLine.StartsWith("flowchart"))
                    continue;

                // Parse node definitions: nodeX["📁 FolderName"] or parentNode --> childNode["📄 FileName"]
                var nodeDefMatch = Regex.Match(trimmedLine, @"(\w+)\[""([^""]+)""\]");
                var relationMatch = Regex.Match(trimmedLine, @"(\w+)\s*-->\s*(\w+)\[""([^""]+)""\]");

                if (relationMatch.Success)
                {
                    var parentId = relationMatch.Groups[1].Value;
                    var childId = relationMatch.Groups[2].Value;
                    var childDisplay = relationMatch.Groups[3].Value;

                    relationships.Add((parentId, childId));

                    if (!nodeMap.ContainsKey(childId))
                    {
                        var isDir = childDisplay.StartsWith("📁");
                        var name = ExtractNodeName(childDisplay);

                        nodeMap[childId] = new TreeNode
                        {
                            NodeId = childId,
                            Name = name,
                            IsDirectory = isDir,
                            Level = 0 // Will be calculated later
                        };
                    }
                }
                else if (nodeDefMatch.Success)
                {
                    var nodeId = nodeDefMatch.Groups[1].Value;
                    var display = nodeDefMatch.Groups[2].Value;

                    if (!nodeMap.ContainsKey(nodeId))
                    {
                        var isDir = display.StartsWith("📁");
                        var name = ExtractNodeName(display);

                        nodeMap[nodeId] = new TreeNode
                        {
                            NodeId = nodeId,
                            Name = name,
                            IsDirectory = isDir,
                            Level = 0 // Root level
                        };
                    }
                }
            }

            // Build hierarchy and calculate levels
            BuildHierarchy(nodeMap, relationships);

            return nodeMap.Values.OrderBy(n => n.Level).ThenBy(n => n.FullPath).ToList();
        }

        private string ExtractNodeName(string displayText)
        {
            // Remove emojis and clean the name
            var cleaned = Regex.Replace(displayText, @"^[📁📄📂📃📋📊📈📉📌📍📎📏📐📑📒📓📔📕📖📗📘📙📚📛📜📝]\s*", "");
            return cleaned.Trim();
        }

        private void BuildHierarchy(Dictionary<string, TreeNode> nodeMap, List<(string parent, string child)> relationships)
        {
            var parentChildMap = relationships.GroupBy(r => r.parent)
                .ToDictionary(g => g.Key, g => g.Select(r => r.child).ToList());

            var childParentMap = relationships.ToDictionary(r => r.child, r => r.parent);

            // Find root nodes (nodes with no parent)
            var rootNodes = nodeMap.Keys.Where(id => !childParentMap.ContainsKey(id)).ToList();

            // Calculate levels and build hierarchy using BFS
            var queue = new Queue<(string nodeId, int level, string path)>();
            foreach (var rootId in rootNodes)
            {
                if (nodeMap.TryGetValue(rootId, out var rootNode))
                {
                    queue.Enqueue((rootId, 0, rootNode.Name));
                    rootNode.Level = 0;
                    rootNode.FullPath = rootNode.Name;
                }
            }

            while (queue.Count > 0)
            {
                var (nodeId, level, path) = queue.Dequeue();

                if (parentChildMap.TryGetValue(nodeId, out var children))
                {
                    foreach (var childId in children)
                    {
                        if (nodeMap.TryGetValue(childId, out var childNode))
                        {
                            childNode.Level = level + 1;
                            childNode.FullPath = $"{path}/{childNode.Name}";
                            
                            // Add to parent's children list
                            if (nodeMap.TryGetValue(nodeId, out var parentNode))
                            {
                                parentNode.Children.Add(childNode);
                            }
                            
                            queue.Enqueue((childId, level + 1, childNode.FullPath));
                        }
                    }
                }
            }
        }

        private string GenerateAsciiTree(List<TreeNode> nodes)
        {
            var result = new StringBuilder();
            var rootNodes = nodes.Where(n => n.Level == 0).OrderBy(n => n.Name).ToList();

            foreach (var rootNode in rootNodes)
            {
                GenerateAsciiTreeRecursive(rootNode, result, "", true);
            }

            return result.ToString();
        }

        private void GenerateAsciiTreeRecursive(TreeNode node, StringBuilder result, string prefix, bool isLast)
        {
            // Current node
            result.AppendLine($"{prefix}{(isLast ? "└── " : "├── ")}{node.Name}");

            // Children
            var children = node.Children.OrderBy(c => c.IsDirectory ? 0 : 1).ThenBy(c => c.Name).ToList();
            for (int i = 0; i < children.Count; i++)
            {
                var child = children[i];
                var isLastChild = i == children.Count - 1;
                var newPrefix = prefix + (isLast ? "    " : "│   ");
                GenerateAsciiTreeRecursive(child, result, newPrefix, isLastChild);
            }
        }

        private string GenerateIconTree(List<TreeNode> nodes)
        {
            var result = new StringBuilder();
            var rootNodes = nodes.Where(n => n.Level == 0).OrderBy(n => n.Name).ToList();

            foreach (var rootNode in rootNodes)
            {
                GenerateIconTreeRecursive(rootNode, result, "");
            }

            return result.ToString();
        }

        private void GenerateIconTreeRecursive(TreeNode node, StringBuilder result, string indent)
        {
            var icon = node.IsDirectory ? "📁" : "📄";
            result.AppendLine($"{indent}{icon} {node.Name}");

            // Children
            var children = node.Children.OrderBy(c => c.IsDirectory ? 0 : 1).ThenBy(c => c.Name).ToList();
            foreach (var child in children)
            {
                GenerateIconTreeRecursive(child, result, indent + "  ");
            }
        }

        public ConversionResult ConvertMermaidToTreeText(string mermaidDiagram)
        {
            return ConvertMermaidToTree(mermaidDiagram, useAsciiFormat: true);
        }

        public ConversionResult ConvertMermaidToIconTree(string mermaidDiagram)
        {
            return ConvertMermaidToTree(mermaidDiagram, useAsciiFormat: false);
        }
    }
}