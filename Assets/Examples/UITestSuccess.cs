using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;
using PlayQ.UITestTools;
using UnityEngine;
using Object = UnityEngine.Object;


namespace PlayQ.UITestTools.Tests
{
    public class UITestSuccess : UITestBase
    {
        [SetUp]
        public void Init()
        {
            // LoadSceneForSetUp("TestableGameScene");
        }

        [UnityTest]
        public IEnumerator LoadScene()
        {
            //Wait for scene loading
            yield return LoadScene("1");
        }

        [UnityTest]
        public IEnumerator Screen()
        {
            //Wait for scene loading
            yield return LoadScene("1");

            MakeScreenShot("some path");
        }

        [UnityTest]
        public IEnumerator FindObjectByPixels()
        {
            //Wait for scene loading
            yield return LoadScene("1");

            yield return WaitFrame();

            var obj = FindObjectByPixels(598, 583);
            Assert.IsNotNull(obj);
            Assert.AreEqual(obj.name, "Image");
        }

        [UnityTest]
        public IEnumerator FindObjectByPercents()
        {
            //Wait for scene loading
            yield return LoadScene("1");

            yield return WaitFrame();

            var obj = FindObjectByPercents(598f / UnityEngine.Screen.width, 583f / UnityEngine.Screen.height);
            Assert.IsNotNull(obj);
            Assert.AreEqual(obj.name, "Image");
        }

        [UnityTest]
        public IEnumerator ClickOnObject()
        {
            //Wait for scene loading
            yield return LoadScene("1");

            yield return WaitFrame();

            ClickPixels(598, 583);
        }

        [UnityTest]
        public IEnumerator WaitForObjectOnScene()
        {
            //Wait for scene loading
            yield return LoadScene("1");

            //search for object with name "child_enabled" in parent object "container"
            //if object is not found - waits for object appearing for duration
            yield return WaitForObject("container/child_enabled");

            //search for object with component "Child" on it"
            //if object is not found - waits for object appearing for duration
            yield return WaitForObject<Child>();

            //search for object with component "Child" on it and hierarchy "container/child_enabled" 
            //if object is not found - waits for object appearing for duration
            yield return WaitForObject<Child>("container/child_with_component");
        }

        [UnityTest]
        public IEnumerator WaitForObjectDestraction()
        {
            //Wait for scene loading
            yield return LoadScene("1");

            //manually search for object in scene
            var objectInstance = UITestTools.FindAnyGameObject<ObjectThatWillBeDestroyed>().gameObject;
            Object.Destroy(objectInstance);

            //wait during interval for destraction of object with component "ObjectThatWillBeDestroyedInSecond"
            yield return WaitForDestroy<ObjectThatWillBeDestroyed>(10);

            //wait during interval for object destraction by object name "Object_that_will_be_destroyed_in_second"
            yield return WaitForDestroy("Object_that_will_be_destroyed");

            //wait during interval for object destraction by object instance
            yield return WaitForDestroy(objectInstance);
        }

        [UnityTest]
        public IEnumerator WaitForObjectDisabled()
        {
            //Wait for scene loading
            yield return LoadScene("1");

            //get manual reference to the object
            var objectInstance = UITestTools.FindAnyGameObject<EnabledAtStart>();

            //manually disable object
            objectInstance.gameObject.SetActive(false);

            //check for disabled
            yield return WaitObjectDisabled("Object_enabled_at_start");

            //check for disabled
            yield return WaitObjectDisabled<EnabledAtStart>();

        }


        [UnityTest]
        public IEnumerator WaitForObjectEnabled()
        {
            //Wait for scene loading
            yield return LoadScene("1");

            //get manual reference to the object
            var objectInstance = UITestTools.FindAnyGameObject<DisabledAtStart>();

            //manually enable object
            objectInstance.gameObject.SetActive(true);

            //check for enabled by component type
            yield return WaitObjectEnabled<DisabledAtStart>();

            //check for enabled by path in hierarchy
            yield return WaitObjectEnabled("Object_disabled_at_start");

        }

        [UnityTest]
        public IEnumerator WaitForButtonAccesible()
        {
            //Wait for scene loading
            yield return LoadScene("1");

            var button = UITestTools.FindAnyGameObject("container/Button");
            yield return ButtonAccessible(button);
        }

        [UnityTest]
        public IEnumerator CheckAppendText()
        {
            //Wait for scene loading
            yield return LoadScene("1");

            AppendText("container/InputField", "appednend text");

            SetText("container/InputField", "appednend text");


            try
            {
                SetText("container/Text 1", "appednend text");
            }
            catch (AssertionException ex)
            {
                if (!ex.Message.EndsWith("have not InputField component."))
                {
                    throw;
                }
            }

            try
            {
                AppendText("container/Text 1", "appednend text");
            }
            catch (AssertionException ex)
            {
                if (!ex.Message.EndsWith("have not InputField component."))
                {
                    throw;
                }
            }
        }

        [UnityTest]
        public IEnumerator DragPixels()
        {
            //Wait for scene loading
            yield return LoadScene("2");

            //get manual reference to the object
            yield return WaitFrame(1);

            yield return DragPercents("Container/Scroll View/Viewport/Content/Image", new Vector2(0.5f, 2f), 10f);
        }
    }
}

 