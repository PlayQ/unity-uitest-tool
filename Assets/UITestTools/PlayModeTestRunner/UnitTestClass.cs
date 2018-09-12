﻿using System;
 using System.Collections.Generic;
 using System.Reflection;

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

        /*
        public bool ContainceTestWithFilter(string s)
        {
            foreach (var testMethod in TestMethods)
            {
                if (testMethod.Filter(s))
                    return true;
            }
            return false;
        }
        */
    }
}