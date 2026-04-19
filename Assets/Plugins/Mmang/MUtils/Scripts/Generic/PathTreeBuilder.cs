using System.Collections.Generic;
using System.Linq;

namespace Mmang.Util
{
    public static class PathTreeBuilder
    {
        public class Node
        {
            public string Name;
            public string FullPath;
            public bool IsLeaf;
            public Dictionary<string, Node> Children { get; private set; } = new();

            public Node(string name, string fullPath, bool isLeaf)
            {
                Name = name;
                FullPath = fullPath;
                IsLeaf = isLeaf;
            }
        }

        public class Node<T>
        {
            public string Name;
            public string FullPath;
            public bool IsLeaf;
            public T UserData;
            public Dictionary<string, Node<T>> Children { get; private set; } = new();

            public Node(string name, string fullPath, bool isLeaf, T userData = default)
            {
                Name = name;
                FullPath = fullPath;
                IsLeaf = isLeaf;
                UserData = userData;
            }
        }
        
        public static Node Build(IEnumerable<string> paths)
        {
            var rootNode = new Node("", "", false);
            var rootNodes = rootNode.Children;

            foreach (var path in paths)
            {
                if (string.IsNullOrEmpty(path)) continue;

                var parts = path.Replace("\\", "/").Split('/');
                
                Dictionary<string, Node> currentLevel = rootNodes;

                for (int i = 0; i < parts.Length; i++)
                {
                    string part = parts[i];
                    bool isLastPart = i == parts.Length - 1;
                    
                    string currentFullPath = string.Join("/", parts.Take(i + 1));

                    if (!currentLevel.ContainsKey(part))
                    {
                        var newNode = new Node(part, currentFullPath, isLastPart);
                        currentLevel.Add(part, newNode);
                    }
                    else if (isLastPart)
                    {
                        currentLevel[part].IsLeaf = true;
                    }

                    // 深入下一层
                    currentLevel = currentLevel[part].Children;
                }
            }

            return rootNode;
        }

        public static Node<T> Build<T>(IEnumerable<(string, T)> pairs)
        {
            var rootNode = new Node<T>("", "", false);
            var rootNodes = rootNode.Children;

            foreach (var pair in pairs)
            {
                string path = pair.Item1;
                if (string.IsNullOrEmpty(path)) continue;

                var parts = path.Replace("\\", "/").Split('/');
                
                Dictionary<string, Node<T>> currentLevel = rootNodes;

                for (int i = 0; i < parts.Length; i++)
                {
                    string part = parts[i];
                    bool isLastPart = i == parts.Length - 1;
                    
                    string currentFullPath = string.Join("/", parts.Take(i + 1));

                    if (!currentLevel.ContainsKey(part))
                    {
                        var userData = isLastPart ? pair.Item2 : default;
                        var newNode = new Node<T>(part, currentFullPath, isLastPart, userData);
                        currentLevel.Add(part, newNode);
                    }
                    else if (isLastPart)
                    {
                        currentLevel[part].UserData = pair.Item2;
                        currentLevel[part].IsLeaf = true;
                    }

                    // 深入下一层
                    currentLevel = currentLevel[part].Children;
                }
            }

            return rootNode;
        }
    }
}