﻿using System;
using System.Collections;
using System.Collections.Generic;
 using System.Diagnostics;
 using System.IO;
 using System.Reflection;
using NUnit.Framework;
 using UnityEngine;
 using UnityEngine.TestTools;
 using Debug = UnityEngine.Debug;

namespace PlayQ.UITestTools
{
    public class TestedClass
    {
        public readonly List<UnitTestMethod> TestMethods;
        public readonly Type Target;

        public readonly List<MethodInfo> SetUpMethods;
        public readonly List<MethodInfo> TearDownMethods;


        public TestedClass(Type t)
        {
            Target = t;
            SetUpMethods = GetMethodsWithCustomAttribute<SetUpAttribute>(Target);
            TearDownMethods = GetMethodsWithCustomAttribute<TearDownAttribute>(Target);
            TestMethods = new List<UnitTestMethod>();

            FillTestMethods();
        }

        private void FillTestMethods()
        {
            var asyncTests = GetMethodsWithCustomAttribute<UnityTestAttribute>(Target);
            foreach (var asyncTest in asyncTests)
            {
                TestMethods.Add(new UnitTestMethod(asyncTest, false));
            }

            var syncTests = GetMethodsWithCustomAttribute<TestAttribute>(Target);
            foreach (var syncTest in syncTests)
            {
                TestMethods.Add(new UnitTestMethod(syncTest, true));
            }
        }

        private List<MethodInfo> GetMethodsWithCustomAttribute<T>(Type targetType)
        {
            var findedMethods = new List<MethodInfo>();
            var methods = targetType.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public |
                                                BindingFlags.NonPublic);
            foreach (var method in methods)
            {
                var attrs = method.GetCustomAttributes(false);
                foreach (var attr in attrs)
                {
                    var isTestMethod = attr.GetType() == typeof(T);
                    if (isTestMethod)
                    {
                        findedMethods.Add(method);
                        break;
                    }
                }
            }
            return findedMethods;
        }
    }
}