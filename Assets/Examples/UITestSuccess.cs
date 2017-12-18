﻿using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;
using PlayQ.UITestTools;
using UnityEngine;
using Object = UnityEngine.Object;

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
    public IEnumerator DragPixels()
    {
        //Wait for scene loading
        yield return LoadScene("2");

        //get manual reference to the object
        yield return WaitFrame(1);

        yield return DragPercents("container/Scroll View/Scrollbar Vertical/Sliding Area/Handle", new Vector2(0.9f, 0f),
            10f);
    }
}

 