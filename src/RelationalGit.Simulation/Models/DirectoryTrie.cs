using System.Collections.Generic;
using System.Linq;

namespace RelationalGit.Simulation
{
    public class DirectoryTrie
    {
        private readonly DirectoryTrieNode _root = new DirectoryTrieNode(".");

        public DirectoryTrie()
        {
        }

        public void Add(string filePath)
        {
            var pathParts = filePath.Split('/').Select(q => q.ToLower()).ToArray();
            _root.Add(pathParts, 0, filePath);
        }

        public string[] GetFileNeighbors(int parentIndex, string filePath)
        {
            var pathParts = filePath.Split('/').Select(q => q.ToLower()).ToArray();
            pathParts = pathParts.Take(pathParts.Length - parentIndex).ToArray();

            if (pathParts.Length == 0)
            {
                return new string[] { filePath };
            }

            return _root.GetFiles(pathParts, 0);
        }
    }

    public class DirectoryTrieNode
    {
        private readonly Dictionary<string, DirectoryTrieNode> _children = new Dictionary<string, DirectoryTrieNode>();
        private readonly List<string> _files = new List<string>();
        private readonly string _identifier = null;

        public DirectoryTrieNode(string identifier)
        {
            _identifier = identifier;
        }

        public void Add(string[] pathParts, int indexPart, string filePath)
        {
            var currentNode = this;

            while (pathParts.Length > indexPart)
            {
                if (!currentNode._children.ContainsKey(pathParts[indexPart]))
                {
                    currentNode._children[pathParts[indexPart]] = new DirectoryTrieNode(pathParts[indexPart]);
                }

                currentNode._files.Add(filePath);

                currentNode = currentNode._children[pathParts[indexPart]];
                indexPart++;
            }
        }

        internal string[] GetFiles(string[] pathParts, int indexPart)
        {
            var currentNode = this;

            while (pathParts.Length != indexPart && currentNode != null)
            {
                currentNode = currentNode._children.GetValueOrDefault(pathParts[indexPart]);
                indexPart++;
            }

            if (currentNode != null)
            {
                return currentNode._files.ToArray();
            }

            return System.Array.Empty<string>();
        }
    }
}
