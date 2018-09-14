﻿using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;
using PlayQ.UITestTools;
 using UnityEngine.SceneManagement;
 using Object = UnityEngine.Object;

namespace PlayQ.UITestTools.Tests
{
    public class UITestWaitFail
    {
        [UnityTest]
        public IEnumerator WaitForObjectByName()
        {
            yield return null;
            SceneManager.LoadScene("1", LoadSceneMode.Additive);

            var testObject = UITestUtils.FindAnyGameObject<TestObject>().gameObject;
            Object.DestroyImmediate(testObject);

            //search for object with name "child_enabled" in parent object "container"
            //if object is not found - waits for object appearing for duration
            yield return Wait.ObjectInstantiated("container");
        }

        [UnityTest]
        public IEnumerator WaitForObjectByComponent()
        {
            yield return null;
            SceneManager.LoadScene("1", LoadSceneMode.Additive);

            var testObject = UITestUtils.FindAnyGameObject<TestObject>().gameObject;
            Object.DestroyImmediate(testObject);

            yield return Wait.ObjectInstantiated<TestObject>();
        }

        [UnityTest]
        public IEnumerator WaitForObjectByComponentAndName()
        {
            yield return null;
            SceneManager.LoadScene("1", LoadSceneMode.Additive);

            var testObject = UITestUtils.FindAnyGameObject<TestObject>().gameObject;
            Object.DestroyImmediate(testObject);

            yield return Wait.ObjectInstantiated<TestObject>("container");
        }

        [UnityTest]
        public IEnumerator WaitForObjectDestractionByComponent()
        {
            yield return null;
            SceneManager.LoadScene("1", LoadSceneMode.Additive);

            //wait during interval for destraction of object with component "ObjectThatWillBeDestroyedInSecond"
            yield return Wait.ObjectDestroyed<TestObject>(1);
        }

        [UnityTest]
        public IEnumerator WaitForObjectDestractionByInstance()
        {
            yield return null;
            SceneManager.LoadScene("1", LoadSceneMode.Additive);

            var objectInstance = UITestUtils.FindAnyGameObject<TestObject>().gameObject;

            //wait during interval for object destraction by object instance
            yield return Wait.ObjectDestroyed(objectInstance);
        }

        [UnityTest]
        public IEnumerator WaitForObjectDestractionByName()
        {
            yield return null;
            SceneManager.LoadScene("1", LoadSceneMode.Additive);

            //wait during interval for object destraction by object name "Object_that_will_be_destroyed_in_second"
            yield return Wait.ObjectDestroyed("container");
        }

        [UnityTest]
        public IEnumerator WaitForObjectDisabledByName()
        {
            yield return null;
            SceneManager.LoadScene("1", LoadSceneMode.Additive);

            //check for disabled
            yield return Wait.ObjectDisabled("Object_enabled_at_start");

        }


        [UnityTest]
        public IEnumerator WaitForObjectDisabledByComponent()
        {
            yield return null;
            SceneManager.LoadScene("1", LoadSceneMode.Additive);

            //check for disabled
            yield return Wait.ObjectDisabled<TestObject>();

        }


        [UnityTest]
        public IEnumerator WaitForObjectEnabledByComponent()
        {
            yield return null;
            SceneManager.LoadScene("1", LoadSceneMode.Additive);

            var testObject = UITestUtils.FindAnyGameObject<TestObject>().gameObject;
            testObject.SetActive(false);
            //check for enabled by component type
            yield return Wait.ObjectEnabled<TestObject>();

        }

        [UnityTest]
        public IEnumerator WaitForObjectEnabledByName()
        {
            yield return null;
            SceneManager.LoadScene("1", LoadSceneMode.Additive);

            var testObject = UITestUtils.FindAnyGameObject<TestObject>().gameObject;
            testObject.SetActive(false);

            //check for enabled by path in hierarchy
            yield return Wait.ObjectEnabled("container");

        }

        [UnityTest]
        public IEnumerator WaitForButtonAccesible()
        {
            yield return null;
            SceneManager.LoadScene("1", LoadSceneMode.Additive);

            var button = UITestUtils.FindAnyGameObject("container");
            yield return Wait.ButtonAccessible(button);
        }

    }

}