using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DevToolVaultV2.Core.Services
{
    public class TreeToMermaidConverter
    {
        public class ConversionResult
        {
            public string MermaidDiagram { get; set; }
            public List<TreeNode> ParsedNodes { get; set; }
            public bool IsSuccess { get; set; }
            public string ErrorMessage { get; set; }
        }

        public class TreeNode
        {
            public string Name { get; set; }
            public bool IsDirectory { get; set; }
            public int Level { get; set; }
            public string NodeId { get; set; }
            public string FullPath { get; set; }
        }

        public ConversionResult ConvertTreeToMermaid(string treeText)
        {
            if (string.IsNullOrWhiteSpace(treeText))
            {
                return new ConversionResult 
                { 
                    IsSuccess = false, 
                    ErrorMessage = "Input tree text is empty",
                    MermaidDiagram = string.Empty,
                    ParsedNodes = new List<TreeNode>()
                };
            }

            try
            {
                var lines = treeText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                var mermaidBuilder = new StringBuilder();
                mermaidBuilder.AppendLine("graph TD");

                var nodeCounter = 0;
                var nodeMap = new Dictionary<string, string>();
                var parentStack = new Stack<(string name, int level, string nodeId, string path)>();
                var parsedNodes = new List<TreeNode>();

                foreach (var line in lines)
                {
                    var cleanLine = line.TrimEnd();
                    if (string.IsNullOrEmpty(cleanLine)) continue;

                    var level = GetIndentationLevel(cleanLine);
                    var itemName = ExtractItemName(cleanLine);
                    var isDirectory = IsDirectory(cleanLine, itemName);

                    if (string.IsNullOrEmpty(itemName)) continue;

                    var nodeId = $"node{++nodeCounter}";
                    var displayName = isDirectory ? $"📁 {itemName}" : $"📄 {itemName}";
                    nodeMap[itemName] = nodeId;

                    // Build full path
                    var pathSegments = new List<string>();
                    var tempStack = parentStack.ToArray().Reverse().ToArray();
                    foreach (var parent in tempStack.Where(p => p.level < level))
                    {
                        pathSegments.Add(parent.name);
                    }
                    pathSegments.Add(itemName);
                    var fullPath = string.Join("/", pathSegments);

                    var treeNode = new TreeNode
                    {
                        Name = itemName,
                        IsDirectory = isDirectory,
                        Level = level,
                        NodeId = nodeId,
                        FullPath = fullPath
                    };
                    parsedNodes.Add(treeNode);

                    // Pop stack until we find the correct parent level
                    while (parentStack.Count > 0 && parentStack.Peek().level >= level)
                    {
                        parentStack.Pop();
                    }

                    if (parentStack.Count == 0)
                    {
                        // Root node
                        mermaidBuilder.AppendLine($"    {nodeId}[\"{displayName}\"]");
                    }
                    else
                    {
                        var parent = parentStack.Peek();
                        mermaidBuilder.AppendLine($"    {parent.nodeId} --> {nodeId}[\"{displayName}\"]");
                    }

                    if (isDirectory)
                    {
                        parentStack.Push((itemName, level, nodeId, fullPath));
                    }
                }

                return new ConversionResult
                {
                    IsSuccess = true,
                    MermaidDiagram = mermaidBuilder.ToString(),
                    ParsedNodes = parsedNodes,
                    ErrorMessage = null
                };
            }
            catch (Exception ex)
            {
                return new ConversionResult
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message,
                    MermaidDiagram = $"Error converting to Mermaid: {ex.Message}",
                    ParsedNodes = new List<TreeNode>()
                };
            }
        }

        public List<TreeNode> ParseMermaidToNodes(string mermaidDiagram)
        {
            var nodes = new List<TreeNode>();
            if (string.IsNullOrWhiteSpace(mermaidDiagram))
                return nodes;

            try
            {
                var lines = mermaidDiagram.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                var nodeMap = new Dictionary<string, TreeNode>();
                var relationships = new List<(string parent, string child)>();

                foreach (var line in lines.Skip(1)) // Skip "graph TD"
                {
                    var trimmedLine = line.Trim();
                    if (string.IsNullOrEmpty(trimmedLine)) continue;

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
                            var name = childDisplay.Substring(2).Trim(); // Remove emoji and space
                            
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
                            var name = display.Substring(2).Trim(); // Remove emoji and space
                            
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

                // Calculate levels and paths
                CalculateNodeLevelsAndPaths(nodeMap, relationships);
                nodes = nodeMap.Values.OrderBy(n => n.Level).ThenBy(n => n.FullPath).ToList();
            }
            catch (Exception)
            {
                // Return empty list on parsing error
                nodes = new List<TreeNode>();
            }

            return nodes;
        }

        private void CalculateNodeLevelsAndPaths(Dictionary<string, TreeNode> nodeMap, List<(string parent, string child)> relationships)
        {
            var parentChildMap = relationships.GroupBy(r => r.parent)
                .ToDictionary(g => g.Key, g => g.Select(r => r.child).ToList());
            
            var childParentMap = relationships.ToDictionary(r => r.child, r => r.parent);

            // Find root nodes (nodes with no parent)
            var rootNodes = nodeMap.Keys.Where(id => !childParentMap.ContainsKey(id)).ToList();

            // Calculate levels using BFS
            var queue = new Queue<(string nodeId, int level, string path)>();
            foreach (var rootId in rootNodes)
            {
                queue.Enqueue((rootId, 0, nodeMap[rootId].Name));
                nodeMap[rootId].Level = 0;
                nodeMap[rootId].FullPath = nodeMap[rootId].Name;
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
                            queue.Enqueue((childId, level + 1, childNode.FullPath));
                        }
                    }
                }
            }
        }

        private int GetIndentationLevel(string line)
        {
            var match = Regex.Match(line, @"^(\s*[├└│\s\-]*)\s*");
            return match.Success ? match.Groups[1].Value.Length / 4 : 0; // Assume 4 spaces per level
        }

        private string ExtractItemName(string line)
        {
            // Remove tree symbols and extract the actual name
            var cleaned = Regex.Replace(line, @"^[\s├└│\-]*", "");
            cleaned = Regex.Replace(cleaned, @"^[📁📄]\s*", "");
            return cleaned.Trim();
        }

        private bool IsDirectory(string line, string itemName)
        {
            return line.Contains("📁") || 
                   line.EndsWith("/") || 
                   itemName.EndsWith("/") ||
                   (!itemName.Contains(".") && !line.Contains("📄")) ||
                   Regex.IsMatch(line, @"^[\s├└│\-]*[^.]*$");
        }
    }
}