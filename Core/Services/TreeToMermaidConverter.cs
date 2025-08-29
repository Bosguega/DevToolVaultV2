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

        // ------------------- TREE → MERMAID -------------------
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
                // Pré-conversão: transforma ASCII “sujo” em linhas com '?' para níveis
                var preConvertedLines = PreConvertTree(treeText);

                var mermaidBuilder = new StringBuilder();
                mermaidBuilder.AppendLine("graph TD");

                var nodeCounter = 0;
                var parsedNodes = new List<TreeNode>();
                var parentStack = new Stack<(string nodeId, int level, string path)>();

                foreach (var line in preConvertedLines)
                {
                    var trimmedLine = line.TrimEnd();
                    if (string.IsNullOrEmpty(trimmedLine)) continue;

                    // Nível = número de '?'
                    int level = trimmedLine.TakeWhile(c => c == '?').Count();
                    string itemName = trimmedLine.Substring(level).Trim();
                    bool isDirectory = itemName.EndsWith("/") || !itemName.Contains(".");

                    if (string.IsNullOrEmpty(itemName)) continue;

                    var nodeId = $"node{++nodeCounter}";
                    var displayName = isDirectory ? $"📁 {itemName}" : $"📄 {itemName}";

                    // Pop stack até encontrar o parent correto
                    while (parentStack.Count > 0 && parentStack.Peek().level >= level)
                    {
                        parentStack.Pop();
                    }

                    // Monta caminho completo e linhas do Mermaid
                    string fullPath;
                    if (parentStack.Count == 0)
                    {
                        fullPath = itemName;
                        mermaidBuilder.AppendLine($"    {nodeId}[\"{displayName}\"]");
                    }
                    else
                    {
                        var parent = parentStack.Peek();
                        fullPath = $"{parent.path}/{itemName}";
                        mermaidBuilder.AppendLine($"    {parent.nodeId} --> {nodeId}[\"{displayName}\"]");
                    }

                    var treeNode = new TreeNode
                    {
                        Name = itemName,
                        IsDirectory = isDirectory,
                        Level = level,
                        NodeId = nodeId,
                        FullPath = fullPath
                    };
                    parsedNodes.Add(treeNode);

                    if (isDirectory)
                        parentStack.Push((nodeId, level, fullPath));
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

        // ------------------- PRÉ-CONVERSOR -------------------
        private List<string> PreConvertTree(string treeText)
        {
            var result = new List<string>();
            if (string.IsNullOrWhiteSpace(treeText)) return result;

            var lines = treeText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var trimmed = line.TrimEnd();
                if (string.IsNullOrWhiteSpace(trimmed)) continue;

                int level = 0;
                int i = 0;
                while (i < line.Length)
                {
                    char c = line[i];
                    if (c == ' ' || c == '│' || c == '─' || c == '├' || c == '└')
                    {
                        if ((i % 4) == 0) level++;
                        i++;
                    }
                    else break;
                }

                string name = Regex.Replace(trimmed, @"^[\s\u2500-\u257F\-\|\+`'\*/\\]*", "").Trim();
                var simplifiedLine = new string('?', level) + " " + name;
                result.Add(simplifiedLine);
            }

            return result;
        }

        // ------------------- MERMAID → NODES -------------------
        public List<TreeNode> ParseMermaidToNodes(string mermaidDiagram)
        {
            var nodes = new List<TreeNode>();
            if (string.IsNullOrWhiteSpace(mermaidDiagram)) return nodes;

            try
            {
                var lines = mermaidDiagram.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                var nodeMap = new Dictionary<string, TreeNode>();
                var relationships = new List<(string parent, string child)>();

                foreach (var line in lines.Skip(1)) // ignora "graph TD"
                {
                    var trimmed = line.Trim();
                    if (string.IsNullOrEmpty(trimmed)) continue;

                    var nodeDef = Regex.Match(trimmed, @"(\w+)\[""([^""]+)""\]");
                    var rel = Regex.Match(trimmed, @"(\w+)\s*-->\s*(\w+)\[""([^""]+)""\]");

                    if (rel.Success)
                    {
                        var parentId = rel.Groups[1].Value;
                        var childId = rel.Groups[2].Value;
                        var childDisplay = rel.Groups[3].Value;

                        relationships.Add((parentId, childId));

                        if (!nodeMap.ContainsKey(childId))
                        {
                            nodeMap[childId] = new TreeNode
                            {
                                NodeId = childId,
                                Name = childDisplay.Substring(2).Trim(),
                                IsDirectory = childDisplay.StartsWith("📁"),
                                Level = 0
                            };
                        }
                    }
                    else if (nodeDef.Success)
                    {
                        var nodeId = nodeDef.Groups[1].Value;
                        var display = nodeDef.Groups[2].Value;

                        if (!nodeMap.ContainsKey(nodeId))
                        {
                            nodeMap[nodeId] = new TreeNode
                            {
                                NodeId = nodeId,
                                Name = display.Substring(2).Trim(),
                                IsDirectory = display.StartsWith("📁"),
                                Level = 0
                            };
                        }
                    }
                }

                CalculateNodeLevelsAndPaths(nodeMap, relationships);
                nodes = nodeMap.Values.OrderBy(n => n.Level).ThenBy(n => n.FullPath).ToList();
            }
            catch
            {
                nodes = new List<TreeNode>();
            }

            return nodes;
        }

        private void CalculateNodeLevelsAndPaths(Dictionary<string, TreeNode> nodeMap, List<(string parent, string child)> relationships)
        {
            var parentChildMap = relationships.GroupBy(r => r.parent)
                .ToDictionary(g => g.Key, g => g.Select(r => r.child).ToList());

            var childParentMap = relationships.ToDictionary(r => r.child, r => r.parent);

            var rootNodes = nodeMap.Keys.Where(id => !childParentMap.ContainsKey(id)).ToList();
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
    }
}
