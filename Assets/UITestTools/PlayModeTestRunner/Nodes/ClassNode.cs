using System;
using System.Collections.Generic;
using System.Reflection;

namespace Tests.Nodes
{
    public class ClassNode : Node, IAllMethodsEnumerable, IAllClassesEnumerable
    {
        public readonly Type Type;

        private List<MethodInfo> setUpMethods;
        public IEnumerable<MethodInfo> SetUpMethods { get { return setUpMethods; } set { setUpMethods = (List<MethodInfo>)value; } }

        private List<MethodInfo> oneTimeSetUpMethods;
        public IEnumerable<MethodInfo> OneTimeSetUpMethods
        { 
            get 
            {
                return oneTimeSetUpMethods;  
            }
            set
            { 
                oneTimeSetUpMethods = (List<MethodInfo>)value;

                oneTimeSetUpMethods.Sort((a, b) => 
                {
                    return NamespaceDepth(a).CompareTo(NamespaceDepth(b));
                });
            } 
        }

        private List<MethodInfo> tearDownMethods;
        public IEnumerable<MethodInfo> TearDownMethods { get { return tearDownMethods; } set { tearDownMethods = (List<MethodInfo>)value; } }

        private List<MethodInfo> oneTimeTearDownMethods;
        public IEnumerable<MethodInfo> OneTimeTearDownMethods 
        {
            get 
            {
                return oneTimeTearDownMethods;
            }
            set
            {
                oneTimeTearDownMethods = (List<MethodInfo>)value;

                oneTimeTearDownMethods.Sort((a, b) =>
                {
                    return -(NamespaceDepth(a).CompareTo(NamespaceDepth(b)));
                });
            }
        }

        private int NamespaceDepth(MethodInfo info)
        {
            Type baseType = info.DeclaringType.BaseType;

            if (baseType == typeof(System.Object))
                return 0;

            return baseType.FullName.Split('.').Length;
        }

        public ClassNode(Type type, Node parentNode = null) : base (type.Name, parentNode)
        {
            this.Type = type;
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