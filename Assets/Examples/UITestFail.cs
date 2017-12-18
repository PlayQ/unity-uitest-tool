using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;
using PlayQ.UITestTools;
using Object = UnityEngine.Object;


namespace PlayQ.UITestTools.Tests
{
    public class UITestFail : UITestBase
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
            yield return LoadScene("2");
        }



        [UnityTest]
        public IEnumerator FindObjectByPixels()
        {
            //Wait for scene loading
            yield return LoadScene("1");

            yield return WaitFrame();

            var obj = FindObjectByPixels(20, 20);
            Assert.IsNotNull(obj);
            Assert.AreEqual(obj.name, "Image");
        }

        [UnityTest]
        public IEnumerator FindObjectByPercents()
        {
            //Wait for scene loading
            yield return LoadScene("1");

            yield return WaitFrame();

            var obj = FindObjectByPercents(20f / UnityEngine.Screen.width, 20f / UnityEngine.Screen.height);
            Assert.IsNotNull(obj);
            Assert.AreEqual(obj.name, "Image");
        }

        [UnityTest]
        public IEnumerator ClickOnObject()
        {
            //Wait for scene loading
            yield return LoadScene("1");

            yield return WaitFrame();

            ClickPixels(20, 20);
        }

        [UnityTest]
        public IEnumerator WaitForObjectByName()
        {
            //Wait for scene loading
            yield return LoadScene("1");

            //search for object with name "child_enabled" in parent object "container"
            //if object is not found - waits for object appearing for duration
            yield return WaitForObject("non_exist");
        }

        [UnityTest]
        public IEnumerator WaitForObjectByComponent()
        {
            yield return LoadScene("1");

            yield return WaitForObject<NonExist>();
        }

        [UnityTest]
        public IEnumerator WaitForObjectByComponentAndName()
        {
            yield return LoadScene("1");

            yield return WaitForObject<NonExist>("non_exist");
        }

        [UnityTest]
        public IEnumerator WaitForObjectDestractionByComponent()
        {
            //Wait for scene loading
            yield return LoadScene("1");

            //wait during interval for destraction of object with component "ObjectThatWillBeDestroyedInSecond"
            yield return WaitForDestroy<ObjectThatWillBeDestroyed>(1);
        }

        [UnityTest]
        public IEnumerator WaitForObjectDestractionByInstance()
        {
            //Wait for scene loading
            yield return LoadScene("1");

            var objectInstance = UITestTools.FindAnyGameObject<ObjectThatWillBeDestroyed>().gameObject;

            //wait during interval for object destraction by object instance
            yield return WaitForDestroy(objectInstance);
        }

        [UnityTest]
        public IEnumerator WaitForObjectDestractionByName()
        {
            //Wait for scene loading
            yield return LoadScene("1");

            //wait during interval for object destraction by object name "Object_that_will_be_destroyed_in_second"
            yield return WaitForDestroy("Object_that_will_be_destroyed");
        }

        [UnityTest]
        public IEnumerator WaitForObjectDisabledByName()
        {
            //Wait for scene loading
            yield return LoadScene("1");

            //check for disabled
            yield return WaitObjectDisabled("Object_enabled_at_start");

        }


        [UnityTest]
        public IEnumerator WaitForObjectDisabledByComponent()
        {
            //Wait for scene loading
            yield return LoadScene("1");

            //check for disabled
            yield return WaitObjectDisabled<EnabledAtStart>();

        }


        [UnityTest]
        public IEnumerator WaitForObjectEnabledByComponent()
        {
            //Wait for scene loading
            yield return LoadScene("1");
            //check for enabled by component type
            yield return WaitObjectEnabled<DisabledAtStart>();

        }

        [UnityTest]
        public IEnumerator WaitForObjectEnabledByName()
        {
            //Wait for scene loading
            yield return LoadScene("1");

            //check for enabled by path in hierarchy
            yield return WaitObjectEnabled("Object_disabled_at_start");

        }

        [UnityTest]
        public IEnumerator WaitForButtonAccesible()
        {
            //Wait for scene loading
            yield return LoadScene("1");

            var button = UITestTools.FindAnyGameObject("non_exist");
            yield return ButtonAccessible(button);
        }
    }
}

 