﻿using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using AssertionException = PlayQ.UITestTools.Assert.AssertionException;
using Object = UnityEngine.Object;

namespace PlayQ.UITestTools.Tests
{
    public class UITestCheck
    {
        [SetUp]
        public IEnumerator LoadSceneSetUp()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;

            sceneLoaded = false;

            SceneManager.LoadScene("1", LoadSceneMode.Additive);

            while (!sceneLoaded)
            {
                yield return null;
            }
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            sceneLoaded = true;

            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private bool sceneLoaded;

        [TearDown]
        public IEnumerator UnloadSceneTearDown()
        {
            SceneManager.sceneUnloaded += OnSceneUnloaded;

            sceneUnloaded = false;

            SceneManager.UnloadSceneAsync("1");

            while (!sceneUnloaded)
            {
                yield return null;
            }
        }

        void OnSceneUnloaded(Scene scene)
        {
            sceneUnloaded = true;

            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }

        private bool sceneUnloaded;

        [UnityTest]
        public IEnumerator CheckObjectDisabled()
        {
            yield return null;

            var testObject = UITestUtils.FindAnyGameObject<TestObject>();
            testObject.gameObject.SetActive(false);

            //ensure object disabled by component
            Check.IsDisabled<TestObject>();

            //ensure object disabled by path in hierarchy
            Check.IsDisabled("container");

            //ensure object disabled by path in hierarchy and component type
            Check.IsDisabled<TestObject>("container");

            //ensure object disbaled by object instance
            Check.IsDisabled(testObject.gameObject);
        }

        [UnityTest]
        public IEnumerator CheckObjectDisabledFailCases()
        {
            yield return null;

            var testObject = UITestUtils.FindAnyGameObject<TestObject>();
            testObject.gameObject.SetActive(true);

            try
            {
                Check.IsDisabled<TestObject>();
                Assert.Fail();
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
                Check.IsDisabled("container");
                Assert.Fail();
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
                Check.IsDisabled<TestObject>("container");
                Assert.Fail();
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
                Check.IsDisabled(testObject.gameObject);
                Assert.Fail();
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
            yield return null;

            //get manual reference to the object
            var testObject = UITestUtils.FindAnyGameObject<TestObject>();
            testObject.gameObject.SetActive(true);

            //ensure object enabled by component
            Check.IsEnabled<TestObject>();

            //ensure object enabled by path in hierarchy
            Check.IsEnabled("container");

            //ensure object enabled by path in hierarchy and component type
            Check.IsEnabled<TestObject>("container");

            //ensure object enabled by object instance
            Check.IsEnabled(testObject.gameObject);
        }

        [UnityTest]
        public IEnumerator CheckObjectEnabledFailCases()
        {
            yield return null;
           

            var testObject = UITestUtils.FindAnyGameObject<TestObject>();
            testObject.gameObject.SetActive(false);

            try
            {
                Check.IsEnabled<TestObject>();
                Assert.Fail();
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
                Check.IsEnabled("container");
                Assert.Fail();
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
                Check.IsEnabled<TestObject>("container");
                Assert.Fail();
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
                Check.IsEnabled(testObject.gameObject);
                Assert.Fail();
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
            yield return null;
           
            Check.IsExists("container");
            Check.IsExists<TestObject>();
            Check.IsExists<TestObject>("container");
        }

        [UnityTest]
        public IEnumerator CheckObjectExistsFailCases()
        {
            yield return null;
           
            var testObject = UITestUtils.FindAnyGameObject<TestObject>();
            Object.DestroyImmediate(testObject.gameObject);

            try
            {
                Check.IsExists("container");
                Assert.Fail();
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
                Check.IsExists<TestObject>();
                Assert.Fail();
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
                Check.IsExists<TestObject>("container");
                Assert.Fail();
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
            yield return null;
           
            var testObject = UITestUtils.FindAnyGameObject<TestObject>().gameObject;
            Object.DestroyImmediate(testObject);

            Check.DoesNotExist("container");
            Check.DoesNotExist<TestObject>();
            Check.DoesNotExist<TestObject>("container");


        }

        [UnityTest]
        public IEnumerator CheckObjectDontExistsFailCases()
        {
            yield return null;
           
            try
            {
                Check.DoesNotExist("container");
                Assert.Fail();
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
                Check.DoesNotExist<TestObject>();
                Assert.Fail();
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
                Check.DoesNotExist<TestObject>("container");
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
            yield return null;
           
            var testToggle = UITestUtils.FindAnyGameObject<Toggle>();

            testToggle.isOn = true;
            Check.Toggle("container/Toggle", true);

            testToggle.isOn = false;
            Check.Toggle("container/Toggle", false);
        }

        [UnityTest]
        public IEnumerator CheckForToggleOffFailCases()
        {
            yield return null;
           
            var testToggle = UITestUtils.FindAnyGameObject<Toggle>();

            try
            {
                testToggle.isOn = true;
                Check.Toggle("container/Toggle", false);
                Assert.Fail();
            }
            catch (AssertionException ex)
            {
                if (!ex.Message.EndsWith("but expected is Off"))
                {
                    throw ex;
                }
            }

            Object.DestroyImmediate(testToggle);

            try
            {
                Check.Toggle("container/Toggle", false);
                Assert.Fail();
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
            yield return null;

            Toggle testToggle = null;

            try
            {
                testToggle = UITestUtils.FindAnyGameObject<Toggle>();
                testToggle.isOn = false;

                Check.Toggle("container/Toggle", true);
                Assert.Fail();
            }
            catch (AssertionException ex)
            {
                if (!ex.Message.EndsWith("but expected is On"))
                {
                    throw ex;
                }
            }

            Object.DestroyImmediate(testToggle);

            try
            {
                Check.Toggle("container", true);
                Assert.Fail();
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
            yield return null;
           
            var testText = UITestUtils.FindAnyGameObject<Text>();
            testText.text = "text";

            Check.TextEquals("container/Text", "text");
            Check.TextNotEquals("container/Text", "random text");
            
            try
            {
                Check.TextEquals("container/Text", "random text");
                Assert.Fail();
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
                Assert.Fail();
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
                Assert.Fail();
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
                Assert.Fail();
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