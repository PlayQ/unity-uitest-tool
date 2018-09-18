﻿using System;
 using System.Collections;
 using UnityEngine;
 using UnityEngine.SceneManagement;
 using Object = UnityEngine.Object;
using AssertionException = PlayQ.UITestTools.Assert.AssertionException;

namespace PlayQ.UITestTools.Tests
{
    public class UITestSuccess
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
        public IEnumerator Screen()
        {
            yield return null;

            Interact.MakeScreenShot("some path");
        }

        [UnityTest]
        [TargetResolution(1024, 768)]
        public IEnumerator FindObjectByPixels()
        {
            yield return null;

            var obj = UITestUtils.FindObjectByPixels(630.0f, 325.0f);
            Assert.IsNotNull(obj);
            Assert.AreEqual(obj.name, "Image");
        }

        [UnityTest]
        [TargetResolution(1024, 768)]
        public IEnumerator ClickOnObject()
        {
            yield return null;

            Interact.ClickPixels(512, 41);
            var openedWindow = UITestUtils.FindAnyGameObject("Window");
            Assert.IsTrue(openedWindow.activeInHierarchy);
        }

        [UnityTest]
        public IEnumerator WaitForObjectOnScene()
        {
            //search for object with name "child_enabled" in parent object "container"
            //if object is not found - waits for object appearing for duration
            yield return Wait.ObjectInstantiated("container");

            //search for object with component "Child" on it"
            //if object is not found - waits for object appearing for duration
            yield return Wait.ObjectInstantiated<TestObject>();

            //search for object with component "Child" on it and hierarchy "container/child_enabled" 
            //if object is not found - waits for object appearing for duration
            yield return Wait.ObjectInstantiated<TestObject>("container");
        }

        [UnityTest]
        public IEnumerator WaitForObjectDestraction()
        {
            //manually search for object in scene
            var objectInstance = UITestUtils.FindAnyGameObject<TestObject>().gameObject;
            Object.Destroy(objectInstance);

            //wait during interval for destraction of object with component "ObjectThatWillBeDestroyedInSecond"
            yield return Wait.ObjectDestroyed<TestObject>();

            //wait during interval for object destraction by object name "Object_that_will_be_destroyed_in_second"
            yield return Wait.ObjectDestroyed("container");

            //wait during interval for object destraction by object instance
            yield return Wait.ObjectDestroyed(objectInstance);
        }

        [UnityTest]
        public IEnumerator WaitForObjectDisabled()
        {
            //get manual reference to the object
            var objectInstance = UITestUtils.FindAnyGameObject<TestObject>();

            //manually disable object
            objectInstance.gameObject.SetActive(false);

            //check for disabled
            yield return Wait.ObjectDisabled("container");

            //check for disabled
            yield return Wait.ObjectDisabled<TestObject>();

        }


        [UnityTest]
        public IEnumerator WaitForObjectEnabled()
        {
            //get manual reference to the object
            var objectInstance = UITestUtils.FindAnyGameObject<TestObject>();

            //manually enable object
            objectInstance.gameObject.SetActive(true);

            //check for enabled by component type
            yield return Wait.ObjectEnabled<TestObject>();

            //check for enabled by path in hierarchy
            yield return Wait.ObjectEnabled("container");

        }

        [UnityTest]
        public IEnumerator WaitForButtonAccesible()
        {
            var button = UITestUtils.FindAnyGameObject("container/Button");

            yield return Wait.ButtonAccessible(button);
        }

        [UnityTest]
        public IEnumerator CheckAppendText()
        {
            yield return null;

            Interact.AppendText("container/InputField", "appednend text");
            Interact.SetText("container/InputField", "appednend text");

            try
            {
                Interact.SetText("container/Text", "appednend text");
                Assert.Fail();
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
                Interact.AppendText("container/Text", "appednend text");
                Assert.Fail();
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
        [TargetResolution(1024, 768)]
        public IEnumerator DragByCoords()
        {
            Vector2 from = new Vector2(45, 526);
            Vector2 to = new Vector2(934, 0);
            yield return Interact.DragPixels(from, to);

            var handle = UITestUtils.FindAnyGameObject("Handle");
            var center = UITestUtils.CenterPointOfObject(handle.transform as RectTransform);

            Assert.AreEqual(center.x, 934, 0.1f);
            Assert.AreEqual(center.y, 526, 0.1f);
        }

        [UnityTest]
        [TargetResolution(1024, 768)]
        public IEnumerator DragByReference()
        {
            Vector2 to = new Vector2(934, 0);

            var handle = UITestUtils.FindAnyGameObject("Handle");
            yield return Interact.DragPixels(handle, to);
            var center = UITestUtils.CenterPointOfObject(handle.transform as RectTransform);

            Assert.AreEqual(center.x, 934, 0.1f);
            Assert.AreEqual(center.y, 526, 0.1f);
        }

        [UnityTest]
        [TargetResolution(1024, 768)]
        public IEnumerator DragByPath()
        {
            Vector2 to = new Vector2(934, 0);

            var handle = UITestUtils.FindAnyGameObject("Handle");
            yield return Interact.DragPixels("container/Slider/Handle Slide Area/Handle", to);
            var center = UITestUtils.CenterPointOfObject(handle.transform as RectTransform);

            Assert.AreEqual(center.x, 934, 0.1f);
            Assert.AreEqual(center.y, 526, 0.1f);
        }

        [UnityTest]
        [TargetResolution(1024, 768)]
        public IEnumerator ClickOnObjectFail()
        {
            yield return null;

            try
            {
                Interact.ClickPixels(20, 20);
                Assert.Fail();
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex.Message.Contains("Cannot click to pixels"));
            }
        }

        [UnityTest]
        [TargetResolution(1024, 768)]
        public IEnumerator FindObjectByPixelsFail()
        {
            yield return null;

            var obj = UITestUtils.FindObjectByPixels(20, 20);
            Assert.IsNull(obj);
        }
    }
}