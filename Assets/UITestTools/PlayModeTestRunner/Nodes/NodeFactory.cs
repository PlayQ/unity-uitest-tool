using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PlayQ.UITestTools;
using UnityEngine;
using SpecificTestType = PlayQ.UITestTools.PlayModeTestRunner.SpecificTestType;

namespace Tests.Nodes
{
    public static class NodeFactory
    {
        public static Node Build()
        {
            Node rootNode = new HierarchicalNode(string.Empty, null);

            var targetAssembly = Assembly.GetAssembly(typeof(PlayModeTestRunner));
            var types = targetAssembly.GetTypes();

            foreach (var type in types)
            {
                if (type.IsAbstract)
                    continue;

                BuildClassNode(type, rootNode);
            }

            return rootNode;
        }

        private static void BuildClassNode(Type targetType, Node rootNode)
        {
            List<MethodInfo> syncMethods = new List<MethodInfo>();
            List<MethodInfo> asyncMethods = new List<MethodInfo>();
            List<MethodInfo> setUps = new List<MethodInfo>();
            List<MethodInfo> tearDowns = new List<MethodInfo>();
            List<MethodInfo> oneTimeSetUps = new List<MethodInfo>();
            List<MethodInfo> oneTimeTearDowns = new List<MethodInfo>();

            var methods = targetType.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public |
                                                BindingFlags.NonPublic);

            foreach (var method in methods)
            {
                var attrs = method.GetCustomAttributes(false);
                foreach (var attr in attrs)
                {
                    if (attr.GetType() == typeof(TestAttribute))
                    {
                        syncMethods.Add(method);

                        continue;
                    }

                    if (attr.GetType() == typeof(UnityTestAttribute))
                    {
                        asyncMethods.Add(method);

                        continue;
                    }

                    if (attr.GetType() == typeof(SetUpAttribute))
                    {
                        setUps.Add(method);

                        continue;
                    }

                    if (attr.GetType() == typeof(TearDownAttribute))
                    {
                        tearDowns.Add(method);

                        continue;
                    }

                    if (attr.GetType() == typeof(OneTimeSetUpAttribute))
                    {
                        oneTimeSetUps.Add(method);

                        continue;
                    }

                    if (attr.GetType() == typeof(OneTimeTearDownAttribute))
                    {
                        oneTimeTearDowns.Add(method);

                        continue;
                    }
                }
            }

            if (syncMethods.Count == 0 && asyncMethods.Count == 0)
            {
                return;
            }

            Node parentNode = BuildClassHierarchy(targetType, rootNode);

            ClassNode result = new ClassNode(targetType, parentNode);

            result.SetUpMethods = setUps;
            result.TearDownMethods = tearDowns;
            result.OneTimeSetUpMethods = oneTimeSetUps;
            result.OneTimeTearDownMethods = oneTimeTearDowns;

            foreach (MethodInfo methodInfo in syncMethods)
            {
                BuildMethodNode(methodInfo, result);
            }

            foreach (MethodInfo methodInfo in asyncMethods)
            {
                BuildMethodNode(methodInfo, result);
            }
        }

        private static void BuildMethodNode(MethodInfo methodInfo, Node parentNode)
        {
            MethodTestSettings settings = MethodTestSettingsFactory.Build(methodInfo);

            var result = new MethodNode(settings, parentNode);
        }

        private static Node BuildClassHierarchy(Type targetType, Node rootNode)
        {
            string[] namespaceParts = targetType.Namespace != null ? targetType.Namespace.Split('.', '+') : null;

            Node currentNode = rootNode;

            if (namespaceParts != null)
            {
                Descend(namespaceParts, ref currentNode);
            }

            string[] classFullName = targetType.FullName.Split('.').Last().Split('+');

            if (classFullName.Length > 1)
            {
                string[] classHostPath = new string[classFullName.Length - 1];
                Array.Copy(classFullName, classHostPath, classHostPath.Length);

                Descend(classHostPath, ref currentNode);
            }

            return currentNode;
        }

        private static void Descend(string[] path, ref Node currentNode)
        {
            foreach (string namespacePart in path)
            {
                var childNode = currentNode.Children.FirstOrDefault(item => item.Name == namespacePart);

                if (childNode == null)
                {
                    childNode = BuildHierarchicalNode(namespacePart, currentNode);
                }

                currentNode = childNode;
            }
        }

        private static HierarchicalNode BuildHierarchicalNode(string name, Node parent)
        {
            return new HierarchicalNode(name, parent);
        }
    }
}