﻿using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;
using PlayQ.UITestTools;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace PlayQ.UITestTools.Tests
{
    public class UITestCheckSuccess : UITestBase
    {
        [UnityTest]
        public IEnumerator CheckObjectDisabled()
        {
            //Wait for scene loading
            yield return LoadScene("1");

            var testObject = UITestTools.FindAnyGameObject<TestObject>();
            testObject.gameObject.SetActive(false);

            //ensure object disabled by component
            Check.IsDisable<TestObject>();

            //ensure object disabled by path in hierarchy
            Check.IsDisable("container");

            //ensure object disabled by path in hierarchy and component type
            Check.IsDisable<TestObject>("container");

            //ensure object disbaled by object instance
            Check.IsDisable(testObject.gameObject);
        }

        [UnityTest]
        public IEnumerator CheckObjectDisabledFailCases()
        {
            //Wait for scene loading
            yield return LoadScene("1");

            var testObject = UITestTools.FindAnyGameObject<TestObject>();
            testObject.gameObject.SetActive(true);

            try
            {
                Check.IsDisable<TestObject>();
            }
            catch (AssertionException ex)
            {
                if (!ex.Message.EndsWith("enabled"))
                {
                    throw ex;
                }
            }

            try
            {
                Check.IsDisable("container");
            }
            catch (AssertionException ex)
            {
                if (!ex.Message.EndsWith("enabled"))
                {
                    throw ex;
                }
            }

            try
            {
                Check.IsDisable<TestObject>("container");
            }
            catch (AssertionException ex)
            {
                if (!ex.Message.EndsWith("enabled"))
                {
                    throw ex;
                }
            }



            try
            {
                Check.IsDisable(testObject.gameObject);
            }
            catch (AssertionException ex)
            {
                if (!ex.Message.EndsWith("enabled"))
                {
                    throw ex;
                }
            }
        }


        [UnityTest]
        public IEnumerator CheckObjectEnabled()
        {
            //Wait for scene loading
            yield return LoadScene("1");

            //get manual reference to the object
            var testObject = UITestTools.FindAnyGameObject<TestObject>();
            testObject.gameObject.SetActive(true);

            //ensure object enabled by component
            Check.IsEnable<TestObject>();

            //ensure object enabled by path in hierarchy
            Check.IsEnable("container");

            //ensure object enabled by path in hierarchy and component type
            Check.IsEnable<TestObject>("container");

            //ensure object enabled by object instance
            Check.IsEnable(testObject.gameObject);


        }

        [UnityTest]
        public IEnumerator CheckObjectEnabledFailCases()
        {
            //Wait for scene loading
            yield return LoadScene("1");

            var testObject = UITestTools.FindAnyGameObject<TestObject>();
            testObject.gameObject.SetActive(false);

            try
            {
                Check.IsEnable<TestObject>();
            }
            catch (AssertionException ex)
            {
                if (!ex.Message.EndsWith("Object disabled"))
                {
                    throw ex;
                }
            }


            try
            {
                Check.IsEnable("container");
            }
            catch (AssertionException ex)
            {
                if (!ex.Message.EndsWith("Object disabled"))
                {
                    throw ex;
                }
            }

            try
            {
                Check.IsEnable<TestObject>("container");
            }
            catch (AssertionException ex)
            {
                if (!ex.Message.EndsWith("Object disabled"))
                {
                    throw ex;
                }
            }

            try
            {
                Check.IsEnable(testObject.gameObject);
            }
            catch (AssertionException ex)
            {
                if (!ex.Message.EndsWith("Object disabled"))
                {
                    throw ex;
                }
            }
        }

        [UnityTest]
        public IEnumerator CheckObjectExists()
        {
            //Wait for scene loading
            yield return LoadScene("1");


            Check.IsExist("container");
            Check.IsExist<TestObject>();
            Check.IsExist<TestObject>("container");
        }

        [UnityTest]
        public IEnumerator CheckObjectExistsFailCases()
        {
            //Wait for scene loading
            yield return LoadScene("1");

            var testObject = UITestTools.FindAnyGameObject<TestObject>();
            Object.DestroyImmediate(testObject.gameObject);

            try
            {
                Check.IsNotExist("container");
            }
            catch (AssertionException ex)
            {
                if (!ex.Message.EndsWith("not exist."))
                {
                    throw ex;
                }
            }

            try
            {
                Check.IsExist<TestObject>();
            }
            catch (AssertionException ex)
            {
                if (!ex.Message.EndsWith("not exist."))
                {
                    throw ex;
                }
            }

            try
            {
                Check.IsExist<TestObject>("container");
            }
            catch (AssertionException ex)
            {
                if (!ex.Message.EndsWith("not exist."))
                {
                    throw ex;
                }
            }
        }

        [UnityTest]
        public IEnumerator CheckObjectDontExists()
        {
            //Wait for scene loading
            yield return LoadScene("1");

            var testObject = UITestTools.FindAnyGameObject<TestObject>().gameObject;
            Object.DestroyImmediate(testObject);

            Check.IsNotExist("container");
            Check.IsNotExist<TestObject>();
            Check.IsNotExist<TestObject>("container");


        }

        [UnityTest]
        public IEnumerator CheckObjectDontExistsFailCases()
        {
            //Wait for scene loading
            yield return LoadScene("1");

            try
            {
                Check.IsNotExist("container");
            }
            catch (AssertionException ex)
            {
                if (!ex.Message.EndsWith(" exists."))
                {
                    throw ex;
                }
            }

            try
            {
                Check.IsNotExist<TestObject>();
            }
            catch (AssertionException ex)
            {
                if (!ex.Message.EndsWith(" exists."))
                {
                    throw ex;
                }
            }

            try
            {
                Check.IsNotExist<TestObject>("container");
            }
            catch (AssertionException ex)
            {
                if (!ex.Message.EndsWith(" exists."))
                {
                    throw ex;
                }
            }
        }

        [UnityTest]
        public IEnumerator CheckForToggle()
        {
            //Wait for scene loading
            yield return LoadScene("1");

            var testToggle = UITestTools.FindAnyGameObject<Toggle>();

            testToggle.isOn = true;
            Check.IsToggleOn("container/Toggle");

            testToggle.isOn = false;
            Check.IsToggleOff("container/Toggle");
        }

        [UnityTest]
        public IEnumerator CheckForToggleOffFailCases()
        {
            //Wait for scene loading
            yield return LoadScene("1");

            var testToggle = UITestTools.FindAnyGameObject<Toggle>();

            try
            {
                testToggle.isOn = true;
                Check.IsToggleOff("container/Toggle");
            }
            catch (AssertionException ex)
            {
                if (!ex.Message.EndsWith("is ON."))
                {
                    throw ex;
                }
            }

            try
            {
                Check.IsToggleOff("container");
            }
            catch (AssertionException ex)
            {
                if (!ex.Message.EndsWith("has no Toggle component."))
                {
                    throw ex;
                }
            }

        }

        [UnityTest]
        public IEnumerator CheckForToggleOnFailCases()
        {
            //Wait for scene loading
            yield return LoadScene("1");

            try
            {
                var testToggle = UITestTools.FindAnyGameObject<Toggle>();
                testToggle.isOn = false;

                Check.IsToggleOn("container/Toggle");
            }
            catch (AssertionException ex)
            {
                if (!ex.Message.EndsWith("is OFF."))
                {
                    throw ex;
                }
            }

            try
            {
                Check.IsToggleOn("container");
            }
            catch (AssertionException ex)
            {
                if (!ex.Message.EndsWith("has no Toggle component."))
                {
                    throw ex;
                }
            }
        }

        [UnityTest]
        public IEnumerator CheckTextEquals()
        {
            //Wait for scene loading
            yield return LoadScene("1");

            var testText = UITestTools.FindAnyGameObject<Text>();
            testText.text = "text";

            Check.TextEquals("container/Text", "text");
            Check.TextNotEquals("container/Text", "random text");



            try
            {
                Check.TextEquals("container/Text", "random text");
            }
            catch (AssertionException ex)
            {
                if (!ex.Message.EndsWith("text dont't match"))
                {
                    throw ex;
                }
            }

            try
            {
                Check.TextEquals("container", "text");
            }
            catch (AssertionException ex)
            {
                if (!ex.Message.EndsWith("no Text attached"))
                {
                    throw ex;
                }
            }


            try
            {
                Check.TextNotEquals("container/Text", "text");
            }
            catch (AssertionException ex)
            {
                if (!ex.Message.EndsWith("text matches but must not"))
                {
                    throw ex;
                }
            }

            try
            {
                Check.TextNotEquals("container", "text");
            }
            catch (AssertionException ex)
            {
                if (!ex.Message.EndsWith("no Text attached"))
                {
                    throw ex;
                }
            }
        }



    }

}