using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using PlayQ.UITestTools;
using UnityEngine;

namespace Tests.Nodes
{
    
    public static class NodeFactory
    {
        private static Dictionary<string, string> fileNameToFullPath;
        public static ClassNode Build()
        {
            ClassNode rootNode = new ClassNode("Tests", null);
            
            var fileNames = Directory.GetFiles(
                Application.dataPath,
                "*.cs", SearchOption.AllDirectories);

            fileNameToFullPath = new Dictionary<string, string>();
            foreach (var fileName in fileNames)
            {
                var from = Application.dataPath.Length;
                var fullName = fileName.Substring(from);
                var name = Path.GetFileNameWithoutExtension(fullName);
                fullName = "Assets" + fullName;
                fileNameToFullPath[name] =  fullName;
            }
            
            
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    if (type.Namespace == null)
                    {
                        continue;
                    }
                    if (!type.IsClass || type.IsAbstract)
                    {
                        continue;
                    }

                    BuildClassNode(type, rootNode);
                }
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


            var classAttrs = targetType.GetCustomAttributes(false);
            string ignoreClassReason = null;
            var ignoreAttribute = classAttrs.FirstOrDefault(x => x.GetType() == typeof(IgnoreAttribute));
            if (ignoreAttribute != null)
            {
                ignoreClassReason = (ignoreAttribute as IgnoreAttribute).Reason;
                if (ignoreClassReason == String.Empty)
                {
                    ignoreClassReason = "Class is ingored";
                }
            }

            var methods = targetType.GetMethods(BindingFlags.Instance |
                                                BindingFlags.Static | 
                                                BindingFlags.Public |
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

            string filePath;
            fileNameToFullPath.TryGetValue(targetType.Name, out filePath);

            var childNode = parentNode.ChildByName(targetType.Name);
            ClassNode result;
            if (childNode == null || !(childNode is ClassNode))
            {
                result = new ClassNode(targetType, parentNode, filePath);    
            }
            else
            {
                result = (ClassNode)childNode;
                result.UpdateType(targetType);
            }

            result.SetUpMethods = setUps;
            result.TearDownMethods = tearDowns;
            result.OneTimeSetUpMethods = oneTimeSetUps;
            result.OneTimeTearDownMethods = oneTimeTearDowns;
            
            foreach (MethodInfo methodInfo in syncMethods)
            {
                BuildMethodNode(methodInfo, result, ignoreClassReason);
            }

            foreach (MethodInfo methodInfo in asyncMethods)
            {
                BuildMethodNode(methodInfo, result, ignoreClassReason);
            }
        }

        private static void BuildMethodNode(MethodInfo methodInfo, ClassNode parentNode, string ignoreReason)
        {
            MethodTestSettings settings = MethodTestSettingsFactory.Build(methodInfo, ignoreReason);

            var result = new MethodNode(settings, parentNode);
        }

        private static Node BuildClassHierarchy(Type targetType, Node rootNode)
        {
            string[] namespaceParts = targetType.Namespace != null 
                ? targetType.Namespace.Split('.')
                : null;

            Node currentNode = rootNode;

            if (namespaceParts != null)
            {
                Descend(namespaceParts, ref currentNode);
            }

            string[] classFullName = targetType.FullName
                .Split('.').Last()
                .Split('+');

            
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
                var childNode = currentNode.ChildByName(namespacePart);
                if (childNode == null)
                {    
                    childNode = new ClassNode(namespacePart, currentNode);
                }

                currentNode = childNode;
            }
        }
    }
}