using System.Collections.Generic;

namespace Tests.Nodes
{
    public class HierarchicalNode : Node, IAllClassesEnumerable, IAllMethodsEnumerable
    {
        public HierarchicalNode(string name, Node parent = null) : base(name, parent)
        {
        }

        public IEnumerable<ClassNode> AllClasses
        {
            get
            {
                foreach (Node node in children)
                {
                    ClassNode classNode = node as ClassNode;

                    if (classNode != null)
                        yield return classNode;

                    IAllClassesEnumerable enumerableNode = node as IAllClassesEnumerable;

                    if (enumerableNode != null)
                        foreach (ClassNode childClassNode in enumerableNode.AllClasses)
                            yield return childClassNode;
                }
            }
        }

        public IEnumerable<MethodNode> AllMethods
        {
            get
            {
                foreach (Node node in children)
                {
                    MethodNode methodNode = node as MethodNode;

                    if (methodNode != null)
                        yield return methodNode;

                    IAllMethodsEnumerable enumerableNode = node as IAllMethodsEnumerable;

                    if (enumerableNode != null)
                        foreach (MethodNode childMethodNode in enumerableNode.AllMethods)
                            yield return childMethodNode;
                }
            }
        }
    }
}