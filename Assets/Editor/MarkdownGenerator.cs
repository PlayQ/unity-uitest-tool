using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using UnityEditor;
using UnityEngine;


public class MarkdownGenerator : AssetPostprocessor
{
    static readonly string markdownPath = "/APIREADME.md";
    static readonly string xmlPath = "/Documentation.XML";

    [MenuItem("Tools/Generate UITools API markdown")]
    static void GenerateMarkdownButtonClick()
    {
        Generate();
    }

    /*
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        Generate();
    }
    */

    static void Generate()
    {
        string path = Application.dataPath;
        string rootPath = path.Substring(0, path.LastIndexOf('/'));
        path = string.Concat(rootPath, xmlPath);

        if (!File.Exists(path))
        {
            EditorUtility.DisplayDialog("MarkdownGenerator", string.Format("ERROR: NO MARKDOWN FOUND AT {0}", path), "Ok");

            return;
        }

        var xml = File.ReadAllText(path);
        var doc = XDocument.Parse(xml);
        var md = doc.Root.ToMarkDown();

        path = string.Concat(rootPath, markdownPath);

        File.WriteAllText(path, md);

        EditorUtility.DisplayDialog("MarkdownGenerator", "MARKDOWN SUCCESSFULLY GENERATED!", "Ok");
    }
}

//https://gist.github.com/lontivero/593fc51f1208555112e0
public static class XmlToMarkdown
{
    static readonly string methodPattern = "M:PlayQ.UITestTools.";
    static readonly string classPattern = "T:PlayQ.UITestTools.";
    static readonly string paramRepetitivePattern = "|Name | Description |\n|-----|------|\n";
    static readonly string[] allowedClasses = { "Check", "AsyncCheck", "Wait", "AsyncWait", "Interact", "UITestUtils" };

    class TypeMarkdown
    {
        public string ClassName;
        public XElement ClassNode;
        public List<XElement> Nodes;
    }

    public static string ToMarkDown(this XNode e)
    {
        var formatTemplates = new Dictionary<string, string>
                {
                    {"doc", "# {0} \n{1}"},
                    {"type", "## {0} \n{1}"},
                    {"field", "##### {0}\n\n{1}\n\n---\n"},
                    {"property", "##### {0}\n\n{1}\n\n---\n"},
                    {"method", "#### {0}\n\n{1}\n\n---\n"},
                    {"event", "##### {0}\n\n{1}\n\n---\n"},
                    {"summary", "{0}\n\n"},
                    {"remarks", "\n\n>{0}\n\n"},
                    {"example", "_C# code_\n\n```c#\n{0}\n```\n\n"},
                    {"seePage", "[[{1}|{0}]]"},
                    {"seeAnchor", "[{1}]({0})"},
                    {"param", "|Name | Description |\n|-----|------|\n|{0}: |{1}|\n" },
                    {"exception", "[[{0}|{0}]]: {1}\n\n" },
                    {"returns", "Returns: {0}\n\n"},
                    {"none", ""}
                };

        var NodeToMarkdown = new Func<string, XElement, string[]>((attributeName, node) => new[]
            {
                    node.Attribute(attributeName).Value,
                    node.Nodes().ToMarkDown()
            });

        var CheckForClassNode = new Action<XElement, string, Dictionary<string, TypeMarkdown>>((node, fullMethodName, result) =>
        {
            if (fullMethodName.Contains(classPattern))
            {
                string className = fullMethodName.Substring(classPattern.Length);

                if (className.Split('.').Length > 1)
                    return;

                if (!allowedClasses.Contains(className))
                    return;

                if (!result.ContainsKey(className))
                {
                    result.Add(className, new TypeMarkdown() { ClassName = className, Nodes = new List<XElement>() });
                }

                result[className].ClassNode = node;
            }
        });

        var SplitMethodNodesByClasses = new Func<IEnumerable<XElement>, Dictionary<string, TypeMarkdown>>((nodes) =>
        {
            Dictionary<string, TypeMarkdown> result = new Dictionary<string, TypeMarkdown>();

            foreach (var node in nodes)
            {
                string fullMethodName = node.Attribute("name").Value;

                if (!fullMethodName.Contains(methodPattern))
                {
                    CheckForClassNode(node, fullMethodName, result);

                    continue;
                }

                string methodAndClassName = fullMethodName.Substring(methodPattern.Length);

                string className = methodAndClassName.Split('.')[0];

                if (!allowedClasses.Contains(className))
                    continue;

                if (!result.ContainsKey(className))
                {
                    result.Add(className, new TypeMarkdown() { ClassName = className, Nodes = new List<XElement>() });
                }

                result[className].Nodes.Add(node);
            }

            return result;
        });

        var DocNodeToMarkdown = new Func<XElement, string[]>((node) =>
            {
                string header = "API methods";

                string result = string.Empty;

                IEnumerable<XElement> elements = node.Element("members").Elements("member");

                Dictionary<string, TypeMarkdown> methodsByClasses = SplitMethodNodesByClasses(elements);

                foreach (string className in methodsByClasses.Keys)
                {
                    header = string.Join("\n", new [] { header, string.Format("* [{0}](#{1})", className, className.ToLower().Replace(' ', '-')) });

                    string classResult = string.Empty;

                    if (methodsByClasses[className].ClassNode == null)
                    {
                        classResult = string.Format("## {0} \n", className);
                    }
                    else
                        classResult = methodsByClasses[className].ClassNode.ToMarkDown(); //string.Format("## {0} \n", className);;

                    methodsByClasses[className].Nodes.Sort((a, b) => a.Attribute("name").Value.CompareTo(b.Attribute("name").Value));

                    string previousMethodName = string.Empty;

                    foreach (var method in methodsByClasses[className].Nodes)
                    {
                        var methodMarkdown = method.ToMarkDown();
                        string methodName = methodMarkdown
                        .Split('\n')[0] //#### TextEquals
                        .Split(' ')[1]; //TextEquals

                        if (methodName != previousMethodName)
                            header = string.Join("\n", new[] { header, string.Format("  * [{0}](#{1})", methodName, methodName.ToLower().Replace(' ', '-')) });

                        previousMethodName = methodName;

                        classResult = string.Join("\n", new[] { classResult, methodMarkdown });
                    }

                    result = string.Join("\n", new[] { result, classResult });
                }

                //Removes that ugly lonely slash
                result = result.Replace(">\\", ">");

                return new[]
                {
                    header,
                    result
                };
            });

        var MethodNodeToMarkdown = new Func<XElement, string[]>((node) =>
            {
                string result = node.Nodes().ToMarkDown();

                int firstParamOccurence = result.IndexOf(paramRepetitivePattern, StringComparison.Ordinal);

                if (firstParamOccurence != -1)
                {
                    result = result.Replace(paramRepetitivePattern, string.Empty);

                    result = result.Insert(firstParamOccurence, paramRepetitivePattern);
                }

                string methodName = node
                    .Attribute("name").Value //M:PlayQ.UITestTools.Check.IsEnable``1(System.String)
                    .Substring(methodPattern.Length) //Check.IsEnable``1(System.String)
                    .Split('(')[0] //Check.IsEnable``1
                    .Split('.').Last() //IsEnable``1
                    .Split('`')[0]; //IsEnable

                return new[]
                {
                    methodName,
                    result
                };

            });

        var ClassNodeToMarkdown = new Func<XElement, string[]>((node) =>
            {
                string fullClassName = node.Attribute("name").Value;

                string className = fullClassName.Substring(classPattern.Length);

                if (className.Split('.').Length > 1) //To avoid nested classes
                    className = string.Empty;

                return new[] 
                {
                    className,
                    node.Nodes().ToMarkDown()
                };
            });

        var NodeToMarkdownMethods = new Dictionary<string, Func<XElement, IEnumerable<string>>>
                {
                    {"doc", x=>DocNodeToMarkdown(x)},
                    {"type", x=>ClassNodeToMarkdown(x)},
                    {"field", x=> NodeToMarkdown("name", x)},
                    {"property", x=> NodeToMarkdown("name", x)},
                    {"method",x=>MethodNodeToMarkdown(x)},
                    {"event", x=>NodeToMarkdown("name", x)},
                    {"summary", x=> new[]{ x.Nodes().ToMarkDown() }},
                    {"remarks", x => new[]{x.Nodes().ToMarkDown()}},
                    {"example", x => new[]{x.Value.ToCodeBlock()}},
                    {"seePage", x=> NodeToMarkdown("cref", x) },
                    {"seeAnchor", x=> { var xx = NodeToMarkdown("cref", x); xx[0] = xx[0].ToLower(); return xx; }},
                    {"param", x => NodeToMarkdown("name", x) },
                    {"exception", x => NodeToMarkdown("cref", x) },
                    {"returns", x => new[]{x.Nodes().ToMarkDown()}},
                    {"none", x => new string[0]}
        };

        string name;

        if (e.NodeType == XmlNodeType.Element)
        {
            var el = (XElement)e;
            name = el.Name.LocalName;
            if (name == "member")
            {
                switch (el.Attribute("name").Value[0])
                {
                    case 'F': name = "field"; break;
                    case 'P': name = "property"; break;
                    case 'T': name = "type"; break;
                    case 'E': name = "event"; break;
                    case 'M': name = "method"; break;
                    default: name = "none"; break;
                }
            }
            if (name == "see")
            {
                var anchor = el.Attribute("cref").Value.StartsWith("!:#");
                name = anchor ? "seeAnchor" : "seePage";
            }

            if (!NodeToMarkdownMethods.ContainsKey(name))
            {
                //Debug.LogError(string.Format("MarkdownCreator: UNABLE TO FIND KEY {0}", name));

                return string.Empty;
            }

            var vals = NodeToMarkdownMethods[name](el).ToArray();

            string str = "";

            switch (vals.Length)
            {
                case 1: str = string.Format(formatTemplates[name], vals[0]); break;
                case 2: str = string.Format(formatTemplates[name], vals[0], vals[1]); break;
                case 3: str = string.Format(formatTemplates[name], vals[0], vals[1], vals[2]); break;
                case 4: str = string.Format(formatTemplates[name], vals[0], vals[1], vals[2], vals[3]); break;
            }

            return str;
        }

        if (e.NodeType == XmlNodeType.Text)
        {
            return Regex.Replace(((XText)e).Value.Replace('\n', ' '), @"\s+", " ");
        }

        return string.Empty;
    }


    internal static string ToMarkDown(this IEnumerable<XNode> es)
    {
        return es.Aggregate("", (current, x) => current + x.ToMarkDown());
    }

    static string ToCodeBlock(this string s)
    {
        var lines = s.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var blank = lines[0].TakeWhile(x => x == ' ').Count() - 4;
        return string.Join("\n",
                           lines
                           .Select(
                               x => new string(
                                   x.SkipWhile((y, i) => i < blank)
                                   .ToArray()
                                  )
                              )
                           .ToArray()
                          );
    }
}