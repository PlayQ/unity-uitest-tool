﻿using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;
using PlayQ.UITestTools;
using Object = UnityEngine.Object;

namespace PlayQ.UITestTools.Tests
{
    public class UITestWaitFail
    {
        [UnityTest]
        public IEnumerator WaitForObjectByName()
        {
            //Wait for scene loading
            yield return Interact.LoadScene("1");

            var testObject = UITestUtils.FindAnyGameObject<TestObject>().gameObject;
            Object.DestroyImmediate(testObject);

            //search for object with name "child_enabled" in parent object "container"
            //if object is not found - waits for object appearing for duration
            yield return Wait.ObjectInstantiated("container");
        }

        [UnityTest]
        public IEnumerator WaitForObjectByComponent()
        {
            yield return Interact.LoadScene("1");

            var testObject = UITestUtils.FindAnyGameObject<TestObject>().gameObject;
            Object.DestroyImmediate(testObject);

            yield return Wait.ObjectInstantiated<TestObject>();
        }

        [UnityTest]
        public IEnumerator WaitForObjectByComponentAndName()
        {
            yield return Interact.LoadScene("1");

            var testObject = UITestUtils.FindAnyGameObject<TestObject>().gameObject;
            Object.DestroyImmediate(testObject);

            yield return Wait.ObjectInstantiated<TestObject>("container");
        }

        [UnityTest]
        public IEnumerator WaitForObjectDestractionByComponent()
        {
            //Wait for scene loading
            yield return Interact.LoadScene("1");

            //wait during interval for destraction of object with component "ObjectThatWillBeDestroyedInSecond"
            yield return Wait.ObjectDestroy<TestObject>(1);
        }

        [UnityTest]
        public IEnumerator WaitForObjectDestractionByInstance()
        {
            //Wait for scene loading
            yield return Interact.LoadScene("1");

            var objectInstance = UITestUtils.FindAnyGameObject<TestObject>().gameObject;

            //wait during interval for object destraction by object instance
            yield return Wait.ObjectDestroy(objectInstance);
        }

        [UnityTest]
        public IEnumerator WaitForObjectDestractionByName()
        {
            //Wait for scene loading
            yield return Interact.LoadScene("1");

            //wait during interval for object destraction by object name "Object_that_will_be_destroyed_in_second"
            yield return Wait.ObjectDestroy("container");
        }

        [UnityTest]
        public IEnumerator WaitForObjectDisabledByName()
        {
            //Wait for scene loading
            yield return Interact.LoadScene("1");

            //check for disabled
            yield return Wait.ObjectDisabled("Object_enabled_at_start");

        }


        [UnityTest]
        public IEnumerator WaitForObjectDisabledByComponent()
        {
            //Wait for scene loading
            yield return Interact.LoadScene("1");

            //check for disabled
            yield return Wait.ObjectDisabled<TestObject>();

        }


        [UnityTest]
        public IEnumerator WaitForObjectEnabledByComponent()
        {
            //Wait for scene loading
            yield return Interact.LoadScene("1");

            var testObject = UITestUtils.FindAnyGameObject<TestObject>().gameObject;
            testObject.SetActive(false);
            //check for enabled by component type
            yield return Wait.ObjectEnabled<TestObject>();

        }

        [UnityTest]
        public IEnumerator WaitForObjectEnabledByName()
        {
            //Wait for scene loading
            yield return Interact.LoadScene("1");

            var testObject = UITestUtils.FindAnyGameObject<TestObject>().gameObject;
            testObject.SetActive(false);

            //check for enabled by path in hierarchy
            yield return Wait.ObjectEnabled("container");

        }

        [UnityTest]
        public IEnumerator WaitForButtonAccesible()
        {
            //Wait for scene loading
            yield return Interact.LoadScene("1");

            var button = UITestUtils.FindAnyGameObject("container");
            yield return Wait.ButtonAccessible(button);
        }

    }

}