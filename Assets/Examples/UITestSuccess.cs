﻿﻿using System;
 using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;
 using UnityEngine;
 using UnityEngine.SceneManagement;
 using Object = UnityEngine.Object;

namespace PlayQ.UITestTools.Tests
{
    public class UITestSuccess
    {
        [UnityTest]
        public IEnumerator Screen()
        {
            yield return null;
SceneManager.LoadScene("1", LoadSceneMode.Additive);

            Interact.MakeScreenShot("some path");
        }

        [UnityTest]
        [TargetResolution(1024, 768)]
        public IEnumerator FindObjectByPixels()
        {
            yield return null;
            SceneManager.LoadScene("1", LoadSceneMode.Additive);
            yield return Wait.Frame();

            var obj = UITestUtils.FindObjectByPixels(630.0f, 325.0f);
            Assert.IsNotNull(obj);
            Assert.AreEqual(obj.name, "Image");
        }

        [UnityTest]
        [TargetResolution(1024, 768)]
        public IEnumerator ClickOnObject()
        {
            yield return null;
            SceneManager.LoadScene("1", LoadSceneMode.Additive);
            yield return Wait.Frame();

            Interact.ClickPixels(512, 41);
            var openedWindow = UITestUtils.FindAnyGameObject("Window");
            Assert.IsTrue(openedWindow.activeInHierarchy);
        }

        [UnityTest]
        public IEnumerator WaitForObjectOnScene()
        {
            yield return null;
            SceneManager.LoadScene("1", LoadSceneMode.Additive);

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
            yield return null;
            SceneManager.LoadScene("1", LoadSceneMode.Additive);

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
            yield return null;
            SceneManager.LoadScene("1", LoadSceneMode.Additive);

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
            yield return null;
            SceneManager.LoadScene("1", LoadSceneMode.Additive);

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
            yield return null;
            SceneManager.LoadScene("1", LoadSceneMode.Additive);

            var button = UITestUtils.FindAnyGameObject("container/Button");
            yield return Wait.ButtonAccessible(button);
        }

        [UnityTest]
        public IEnumerator CheckAppendText()
        {
            yield return null;
            SceneManager.LoadScene("1", LoadSceneMode.Additive);

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
            yield return null;
            SceneManager.LoadScene("1", LoadSceneMode.Additive);
            yield return Wait.Frame();

            Vector2 from = new Vector2(45, 526);
            Vector2 delta = new Vector2(934, 0);
            yield return Interact.DragPixels(from, delta);

            var handle = UITestUtils.FindAnyGameObject("Handle");
            var center = UITestUtils.CenterPointOfObject(handle.transform as RectTransform);

            Assert.AreEqual(center.x, 967, 0.1f);
            Assert.AreEqual(center.y, 526, 0.1f);
        }

        [UnityTest]
        [TargetResolution(1024, 768)]
        public IEnumerator DragByReference()
        {
            yield return null;
            SceneManager.LoadScene("1", LoadSceneMode.Additive);
            yield return Wait.Frame();

            Vector2 delta = new Vector2(934, 0);

            var handle = UITestUtils.FindAnyGameObject("Handle");
            yield return Interact.DragPixels(handle, delta);
            var center = UITestUtils.CenterPointOfObject(handle.transform as RectTransform);

            Assert.AreEqual(center.x, 967, 0.1f);
            Assert.AreEqual(center.y, 526, 0.1f);
        }

        [UnityTest]
        [TargetResolution(1024, 768)]
        public IEnumerator DragByPath()
        {
            yield return null;
            SceneManager.LoadScene("1", LoadSceneMode.Additive);
            yield return Wait.Frame();

            Vector2 delta = new Vector2(934, 0);

            var handle = UITestUtils.FindAnyGameObject("Handle");
            yield return Interact.DragPixels("container/Slider/Handle Slide Area/Handle", delta);
            var center = UITestUtils.CenterPointOfObject(handle.transform as RectTransform);

            Assert.AreEqual(center.x, 967, 0.1f);
            Assert.AreEqual(center.y, 526, 0.1f);
        }

        [UnityTest]
        [TargetResolution(1024, 768)]
        public IEnumerator ClickOnObjectFail()
        {
            yield return null;
            SceneManager.LoadScene("1", LoadSceneMode.Additive);
            yield return Wait.Frame();

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
            SceneManager.LoadScene("1", LoadSceneMode.Additive);
            yield return Wait.Frame();
            var obj = UITestUtils.FindObjectByPixels(20, 20);
            Assert.IsNull(obj);
        }
    }
}