﻿using System;
 using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;
 using UnityEngine;
 using Object = UnityEngine.Object;

namespace PlayQ.UITestTools.Tests
{
    public class UITestSuccess : UITestBase
    {
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
        [TargetResolution(1024, 768)]
        public IEnumerator FindObjectByPixels()
        {
            //Wait for scene loading
            yield return LoadScene("1");

            yield return WaitFrame();

            var obj = FindObjectByPixels(630.0f, 325.0f);
            Assert.IsNotNull(obj);
            Assert.AreEqual(obj.name, "Image");
        }

        [UnityTest]
        [TargetResolution(1024, 768)]
        public IEnumerator ClickOnObject()
        {
            //Wait for scene loading
            yield return LoadScene("1");
            yield return WaitFrame();

            ClickPixels(512f, 41f);
            var openedWindow = UITestTools.FindAnyGameObject("Window");
            Assert.IsTrue(openedWindow.activeInHierarchy);
        }

        [UnityTest]
        public IEnumerator WaitForObjectOnScene()
        {
            //Wait for scene loading
            yield return LoadScene("1");

            //search for object with name "child_enabled" in parent object "container"
            //if object is not found - waits for object appearing for duration
            yield return WaitForObject("container");

            //search for object with component "Child" on it"
            //if object is not found - waits for object appearing for duration
            yield return WaitForObject<TestObject>();

            //search for object with component "Child" on it and hierarchy "container/child_enabled" 
            //if object is not found - waits for object appearing for duration
            yield return WaitForObject<TestObject>("container");
        }

        [UnityTest]
        public IEnumerator WaitForObjectDestraction()
        {
            //Wait for scene loading
            yield return LoadScene("1");

            //manually search for object in scene
            var objectInstance = UITestTools.FindAnyGameObject<TestObject>().gameObject;
            Object.Destroy(objectInstance);

            //wait during interval for destraction of object with component "ObjectThatWillBeDestroyedInSecond"
            yield return WaitForDestroy<TestObject>();

            //wait during interval for object destraction by object name "Object_that_will_be_destroyed_in_second"
            yield return WaitForDestroy("container");

            //wait during interval for object destraction by object instance
            yield return WaitForDestroy(objectInstance);
        }

        [UnityTest]
        public IEnumerator WaitForObjectDisabled()
        {
            //Wait for scene loading
            yield return LoadScene("1");

            //get manual reference to the object
            var objectInstance = UITestTools.FindAnyGameObject<TestObject>();

            //manually disable object
            objectInstance.gameObject.SetActive(false);

            //check for disabled
            yield return WaitObjectDisabled("container");

            //check for disabled
            yield return WaitObjectDisabled<TestObject>();

        }


        [UnityTest]
        public IEnumerator WaitForObjectEnabled()
        {
            //Wait for scene loading
            yield return LoadScene("1");

            //get manual reference to the object
            var objectInstance = UITestTools.FindAnyGameObject<TestObject>();

            //manually enable object
            objectInstance.gameObject.SetActive(true);

            //check for enabled by component type
            yield return WaitObjectEnabled<TestObject>();

            //check for enabled by path in hierarchy
            yield return WaitObjectEnabled("container");

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
                SetText("container/Text", "appednend text");
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
                AppendText("container/Text", "appednend text");
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
            //Wait for scene loading
            yield return LoadScene("1");
            yield return WaitFrame();

            Vector2 from = new Vector2(45, 526);
            Vector2 delta = new Vector2(934, 0);
            yield return DragPixels(from, delta);

            var handle = UITestTools.FindAnyGameObject("Handle");
            var center = UITestTools.CenterPointOfObject(handle.transform as RectTransform);

            Assert.AreEqual(center.x, 967, 0.1f);
            Assert.AreEqual(center.y, 526, 0.1f);
        }

        [UnityTest]
        [TargetResolution(1024, 768)]
        public IEnumerator DragByReference()
        {
            //Wait for scene loading
            yield return LoadScene("1");
            yield return WaitFrame();

            Vector2 delta = new Vector2(934, 0);

            var handle = UITestTools.FindAnyGameObject("Handle");
            yield return DragPixels(handle, delta);
            var center = UITestTools.CenterPointOfObject(handle.transform as RectTransform);

            Assert.AreEqual(center.x, 967, 0.1f);
            Assert.AreEqual(center.y, 526, 0.1f);
        }

        [UnityTest]
        [TargetResolution(1024, 768)]
        public IEnumerator DragByPath()
        {
            //Wait for scene loading
            yield return LoadScene("1");
            yield return WaitFrame();

            Vector2 delta = new Vector2(934, 0);

            var handle = UITestTools.FindAnyGameObject("Handle");
            yield return DragPixels("container/Slider/Handle Slide Area/Handle", delta);
            var center = UITestTools.CenterPointOfObject(handle.transform as RectTransform);

            Assert.AreEqual(center.x, 967, 0.1f);
            Assert.AreEqual(center.y, 526, 0.1f);
        }

        [UnityTest]
        [TargetResolution(1024, 768)]
        public IEnumerator ClickOnObjectFail()
        {
            //Wait for scene loading
            yield return LoadScene("1");

            yield return WaitFrame();

            try
            {
                ClickPixels(20, 20);
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
            //Wait for scene loading
            yield return LoadScene("1");
            yield return WaitFrame();
            var obj = FindObjectByPixels(20, 20);
            Assert.IsNull(obj);
        }
    }
}