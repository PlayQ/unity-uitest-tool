using System.Collections.Generic;
using System.Linq;

namespace Tests.Nodes
{
    /// <summary>
    /// Node in a tree
    /// </summary>
    public abstract class Node
    {
        public readonly string Name;
        public readonly string FullName;

        /// <summary>
        /// Parent node
        /// </summary>
        protected Node parent;
        public Node Parent { get { return parent; } }
        public bool IsRoot { get { return parent == null; } }

        public int Index { get; private set; }

        /// <summary>
        /// Child nodes
        /// </summary>
        protected List<Node> children;
        public IEnumerable<Node> Children { get { return children; } }
        public Node ChildAt(int index)
        {
            return children[index];
        }

        public Node(string name, Node parent)
        {
            children = new List<Node>();

            Name = name;
            this.parent = parent;

            if (parent == null || string.IsNullOrEmpty(parent.Name))
                FullName = Name;
            else
                FullName = string.Join(".", new string[] { parent.FullName, Name });

            if (parent != null)
            {
                Index = parent.Children.Count();

                parent.AddChildNode(this);
            }
        }

        public void AddChildNode(Node childNode)
        {
            children.Add(childNode);
        }

        public virtual bool Contains(string pattern)
        {
            return children.Any(item => item.Contains(pattern));
        }
    }
}