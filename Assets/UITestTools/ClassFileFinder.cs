using System;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

public static class ClassFileFinder
{
    private static bool isInited;
    private static Dictionary<string, string> csFiles = new Dictionary<string, string>();
    private static HashSet<string> duplicatedFileNames = new HashSet<string>();

//    public class ClassStructure
//    {
//        public string ClassName;
//        public List<string> Methods;
//    }
    
//    public static ClassFileDetails FindClassFile(System.Type t)
//    {
//        return FindClassFile(t.Name);
//    }

//    public class Node
//    {
//        public List<Node> Inners = new List<Node>();
//        public NodeType Type = NodeType.None;
//
//        public enum NodeType
//        {
//            Namespace,
//            Class,
//            Method,
//            None
//        }
//        
//        
//        public string 
//    }
//
//    private static void ProcessCsFiles()
//    {
//        for (int i = 0; i < csFiles.Count; i++)
//        {
//            string codeFile = File.ReadAllText(csFiles[i]);
//            
//            Node root = new Node();
//            foreach (var VARIABLE in codeFile)
//            {
//                if (VARIABLE == '{')
//                {
//                    root.Inners.Add(new Node());
//                }
//            }
//        }
//    }

    private static readonly string testsFolder = Application.dataPath + "/Tests/TestSuites";

    public static string FindStackTrace(string className, string methodName, int yieldInstructionIndex)
    {
        if (!isInited)
        {
            FindAllCsFiles(testsFolder);
            isInited = true;
        }

        if (duplicatedFileNames.Contains(className))
        {
            return "Mutltiple files have simmilar name: \"" + className + "\". " +
                   "You need to use unique file names.";
        }

        string classPath;
        if (!csFiles.TryGetValue(className, out classPath))
        {
            return
                "Unknown StackTrace (class: \"" + className + "\", method: \"" + methodName + "\"). " +
                "You need to have file name equals to class name and not intersection of namespaces";
        }

        var fileText = File.ReadAllText(classPath);
        var regex = new Regex("IEnumerator\\s+" + methodName);
        var matchResult = regex.Match(fileText);

        if (!matchResult.Success)
        {
            return "Can't find method \"" + methodName + "\" in class \"" + className + "\".";
        }

        var searchIndex = matchResult.Index;
        var findedYieldInstructions = 0;
        while (findedYieldInstructions != yieldInstructionIndex)
        {
            searchIndex = fileText.IndexOf("yield", searchIndex+1, StringComparison.Ordinal);
            if (searchIndex == -1)
            {
                return "Can't find " + (findedYieldInstructions + 1) + " yield instruction in \"" + methodName +
                       "\" in class \"" + className + "\".";
            }
            if (!IsCommented(fileText, searchIndex))
                findedYieldInstructions++;
        }

        string[] strings = fileText.Split('\n');
        int charsCount = 0;
        int strIndex;
        for (strIndex = 0; strIndex < strings.Length; strIndex++)
        {
            charsCount += strings[strIndex].Length+1;
            if (charsCount > searchIndex)
            {
                break;
            }
        }

        return "\n.\n.\nFailed at: " +methodName + " (" + classPath + ":" + (strIndex + 1) + ")\n"+
               "\"" + strings[strIndex] + "\".";
//        return "\n \n\tClass: " + className +
//               "\n\tMethod: " + methodName + 
//               "\n\tLine number: " + (strIndex + 1) + 
//               "\n\"" + strings[strIndex] + "\".";
    }

    private static bool IsCommented(string text, int index)
    {
        int lastNextLineIndex = text.LastIndexOf('\n', index);

        if (lastNextLineIndex == -1)
        {
            lastNextLineIndex = 0;
        }

        int doubleSlashIndex = text.IndexOf("//", lastNextLineIndex, index - lastNextLineIndex, StringComparison.Ordinal);

        if (doubleSlashIndex != -1)
        {
            return true;
        }

        int multilineCommentOpenIndex = text.LastIndexOf("/*", index, StringComparison.Ordinal);

        if (multilineCommentOpenIndex == -1)
        {
            return false;
        }

        int multilineCommentCloseIndex = text.IndexOf("*/", multilineCommentOpenIndex, index - multilineCommentOpenIndex, StringComparison.Ordinal);

        if (multilineCommentCloseIndex == -1)
        {
            return true;
        }

        return false;
    }

    private static void FindAllCsFiles(string startDir)
    {
        foreach (string file in Directory.GetFiles(startDir))
        {
            if (file.EndsWith(".cs", StringComparison.Ordinal))
            {
                string key = Path.GetFileNameWithoutExtension(file);
                if (csFiles.ContainsKey(key))
                {
                    duplicatedFileNames.Add(key);
                    continue;
                }
                csFiles.Add(key, file);
            }
        }
        foreach (string dir in Directory.GetDirectories(startDir))
        {
            FindAllCsFiles(dir);
        }
    }
}