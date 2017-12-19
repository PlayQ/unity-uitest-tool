﻿using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;
using PlayQ.UITestTools;
using Object = UnityEngine.Object;

namespace PlayQ.UITestTools.Tests
{
    public class UITestWaitFail : UITestBase
    {
        [UnityTest]
        public IEnumerator WaitForLoadScene()
        {
            //Wait for scene loading
            yield return LoadScene("non exist scene");
        }

        [UnityTest]
        public IEnumerator WaitForObjectByName()
        {
            //Wait for scene loading
            yield return LoadScene("1");

            var testObject = UITestTools.FindAnyGameObject<TestObject>().gameObject;
            Object.DestroyImmediate(testObject);

            //search for object with name "child_enabled" in parent object "container"
            //if object is not found - waits for object appearing for duration
            yield return WaitForObject("container");
        }

        [UnityTest]
        public IEnumerator WaitForObjectByComponent()
        {
            yield return LoadScene("1");

            var testObject = UITestTools.FindAnyGameObject<TestObject>().gameObject;
            Object.DestroyImmediate(testObject);

            yield return WaitForObject<TestObject>();
        }

        [UnityTest]
        public IEnumerator WaitForObjectByComponentAndName()
        {
            yield return LoadScene("1");

            var testObject = UITestTools.FindAnyGameObject<TestObject>().gameObject;
            Object.DestroyImmediate(testObject);

            yield return WaitForObject<TestObject>("container");
        }

        [UnityTest]
        public IEnumerator WaitForObjectDestractionByComponent()
        {
            //Wait for scene loading
            yield return LoadScene("1");

            //wait during interval for destraction of object with component "ObjectThatWillBeDestroyedInSecond"
            yield return WaitForDestroy<TestObject>(1);
        }

        [UnityTest]
        public IEnumerator WaitForObjectDestractionByInstance()
        {
            //Wait for scene loading
            yield return LoadScene("1");

            var objectInstance = UITestTools.FindAnyGameObject<TestObject>().gameObject;

            //wait during interval for object destraction by object instance
            yield return WaitForDestroy(objectInstance);
        }

        [UnityTest]
        public IEnumerator WaitForObjectDestractionByName()
        {
            //Wait for scene loading
            yield return LoadScene("1");

            //wait during interval for object destraction by object name "Object_that_will_be_destroyed_in_second"
            yield return WaitForDestroy("container");
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
            yield return WaitObjectDisabled<TestObject>();

        }


        [UnityTest]
        public IEnumerator WaitForObjectEnabledByComponent()
        {
            //Wait for scene loading
            yield return LoadScene("1");

            var testObject = UITestTools.FindAnyGameObject<TestObject>().gameObject;
            testObject.SetActive(false);
            //check for enabled by component type
            yield return WaitObjectEnabled<TestObject>();

        }

        [UnityTest]
        public IEnumerator WaitForObjectEnabledByName()
        {
            //Wait for scene loading
            yield return LoadScene("1");

            var testObject = UITestTools.FindAnyGameObject<TestObject>().gameObject;
            testObject.SetActive(false);

            //check for enabled by path in hierarchy
            yield return WaitObjectEnabled("container");

        }

        [UnityTest]
        public IEnumerator WaitForButtonAccesible()
        {
            //Wait for scene loading
            yield return LoadScene("1");

            var button = UITestTools.FindAnyGameObject("container");
            yield return ButtonAccessible(button);
        }

    }

}