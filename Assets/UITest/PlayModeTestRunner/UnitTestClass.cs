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
    public class UnitTestClass
    {
        public readonly Type Type;
        public List<UnitTestMethod> TestMethods;

        public List<MethodInfo> SetUpMethods;
        public List<MethodInfo> TearDownMethods;

        public UnitTestClass(Type t)
        {
            Type = t;
        }
    }
}