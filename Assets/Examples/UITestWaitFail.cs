﻿using System.Collections;
 using UnityEngine.SceneManagement;
 using Object = UnityEngine.Object;
using AssertionException = PlayQ.UITestTools.Assert.AssertionException;

namespace PlayQ.UITestTools.Tests
{
    public class UITestWaitFail
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
        public IEnumerator WaitForObjectByName()
        {
            var testObject = UITestUtils.FindAnyGameObject<TestObject>().gameObject;
            Object.DestroyImmediate(testObject);

            //search for object with name "child_enabled" in parent object "container"
            //if object is not found - waits for object appearing for duration
            yield return Wait.ObjectInstantiated("container");
        }

        [UnityTest]
        public IEnumerator WaitForObjectByComponent()
        {
            var testObject = UITestUtils.FindAnyGameObject<TestObject>();
            Object.DestroyImmediate(testObject);

            yield return Wait.ObjectInstantiated<TestObject>();
        }

        [UnityTest]
        public IEnumerator WaitForObjectByComponentAndName()
        {
            var testObject = UITestUtils.FindAnyGameObject<TestObject>().gameObject;
            Object.DestroyImmediate(testObject);

            yield return Wait.ObjectInstantiated<TestObject>("container");
        }

        [UnityTest]
        public IEnumerator WaitForObjectDestractionByComponent()
        {
            //wait during interval for destraction of object with component "ObjectThatWillBeDestroyedInSecond"
            yield return Wait.ObjectDestroyed<TestObject>(1);
        }

        [UnityTest]
        public IEnumerator WaitForObjectDestractionByInstance()
        {
            var objectInstance = UITestUtils.FindAnyGameObject<TestObject>().gameObject;

            //wait during interval for object destraction by object instance
            yield return Wait.ObjectDestroyed(objectInstance);
        }

        [UnityTest]
        public IEnumerator WaitForObjectDestractionByName()
        {
            //wait during interval for object destraction by object name "Object_that_will_be_destroyed_in_second"
            yield return Wait.ObjectDestroyed("container");
        }

        [UnityTest]
        public IEnumerator WaitForObjectDisabledByName()
        {
            //check for disabled
            yield return Wait.ObjectDisabled("Object_enabled_at_start");

        }


        [UnityTest]
        public IEnumerator WaitForObjectDisabledByComponent()
        {
            //check for disabled
            yield return Wait.ObjectDisabled<TestObject>();

        }


        [UnityTest]
        public IEnumerator WaitForObjectEnabledByComponent()
        {
            var testObject = UITestUtils.FindAnyGameObject<TestObject>().gameObject;
            testObject.SetActive(false);
            //check for enabled by component type
            yield return Wait.ObjectEnabled<TestObject>();

        }

        [UnityTest]
        public IEnumerator WaitForObjectEnabledByName()
        {
            var testObject = UITestUtils.FindAnyGameObject<TestObject>().gameObject;
            testObject.SetActive(false);

            //check for enabled by path in hierarchy
            yield return Wait.ObjectEnabled("container");

        }

        [UnityTest]
        public IEnumerator WaitForButtonAccesible()
        {
            var button = UITestUtils.FindAnyGameObject("container");
            yield return Wait.ButtonAccessible(button);
        }

    }

}