using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine.TestTools;
using PlayQ.UITestTools;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace PlayQ.UITestTools.Tests
{
    public class UITestCheck
    {
        [UnityTest]
        public IEnumerator CheckObjectDisabled()
        {
            yield return null;
            SceneManager.LoadScene("1", LoadSceneMode.Additive); // unload scene in tearsdown

            var testObject = UITestUtils.FindAnyGameObject<TestObject>();
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
            yield return null;
            SceneManager.LoadScene("1", LoadSceneMode.Additive);

            var testObject = UITestUtils.FindAnyGameObject<TestObject>();
            testObject.gameObject.SetActive(true);

            try
            {
                Check.IsDisable<TestObject>();
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
                Check.IsDisable("container");
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
                Check.IsDisable<TestObject>("container");
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
                Check.IsDisable(testObject.gameObject);
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
           SceneManager.LoadScene("1", LoadSceneMode.Additive);

            //get manual reference to the object
            var testObject = UITestUtils.FindAnyGameObject<TestObject>();
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
            yield return null;
           SceneManager.LoadScene("1", LoadSceneMode.Additive);

            var testObject = UITestUtils.FindAnyGameObject<TestObject>();
            testObject.gameObject.SetActive(false);

            try
            {
                Check.IsEnable<TestObject>();
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
                Check.IsEnable("container");
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
                Check.IsEnable<TestObject>("container");
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
                Check.IsEnable(testObject.gameObject);
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
           SceneManager.LoadScene("1", LoadSceneMode.Additive);

            Check.IsExist("container");
            Check.IsExist<TestObject>();
            Check.IsExist<TestObject>("container");
        }

        [UnityTest]
        public IEnumerator CheckObjectExistsFailCases()
        {
            yield return null;
           SceneManager.LoadScene("1", LoadSceneMode.Additive);

            var testObject = UITestUtils.FindAnyGameObject<TestObject>();
            Object.DestroyImmediate(testObject.gameObject);

            try
            {
                Check.DoesNotExist("container");
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
                Check.IsExist<TestObject>();
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
                Check.IsExist<TestObject>("container");
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
           SceneManager.LoadScene("1", LoadSceneMode.Additive);

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
           SceneManager.LoadScene("1", LoadSceneMode.Additive);

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
           SceneManager.LoadScene("1", LoadSceneMode.Additive);

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
           SceneManager.LoadScene("1", LoadSceneMode.Additive);

            var testToggle = UITestUtils.FindAnyGameObject<Toggle>();

            try
            {
                testToggle.isOn = true;
                Check.Toggle("container/Toggle", false);
                Assert.Fail();
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
           SceneManager.LoadScene("1", LoadSceneMode.Additive);

            try
            {
                var testToggle = UITestUtils.FindAnyGameObject<Toggle>();
                testToggle.isOn = false;

                Check.Toggle("container/Toggle", true);
                Assert.Fail();
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
           SceneManager.LoadScene("1", LoadSceneMode.Additive);

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