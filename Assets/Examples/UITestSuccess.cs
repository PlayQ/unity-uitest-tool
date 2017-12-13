using System;
using System.Collections;
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
        LoadSceneForSetUp("TestableGameScene");
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
        yield return null;
        MakeScreenShot("some path");
    }

    [UnityTest]
    public IEnumerator FindObjectByPixels()
    {
        yield return WaitFrame();
        var obj = FindObjectByPixels(UnityEngine.Screen.width / 2f, UnityEngine.Screen.height / 2f);
        Assert.IsNotNull(obj);
        Assert.AreEqual(obj.name, "Image");
    }

    [UnityTest]
    public IEnumerator FindObjectByPercents()
    {
        yield return WaitFrame();
        var obj = FindObjectByPercents(0.5f, 0.5f);
        Assert.IsNotNull(obj);
        Assert.AreEqual(obj.name, "Image");
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
        yield return WaitForDestroy<ObjectThatWillBeDestroyed>();
        
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
        WaitObjectDisabled("Object_enabled_at_start");

        //check for disabled
        WaitObjectDisabled<EnabledAtStart>();

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
        WaitObjectEnabled<DisabledAtStart>();

        //check for enabled by path in hierarchy
        WaitObjectEnabled("Object_disabled_at_start");

    }

    [UnityTest]
    public IEnumerator CheckObjectDisabled()
    {
        //Wait for scene loading
        yield return LoadScene("1");
        
        //ensure object disabled by component
        Check.IsDisable<DisabledAtStart>();
     
        //ensure object disabled by path in hierarchy
        Check.IsDisable("Object_disabled_at_start");
        
        //ensure object disabled by path in hierarchy and component type
        Check.IsDisable<DisabledAtStart>("Object_disabled_at_start");

        //get manual reference to the object
        var objectInstance = UITestTools.FindAnyGameObject<DisabledAtStart>();
        
        //ensure object disbaled by object instance
        Check.IsDisable(objectInstance.gameObject);
    }
    
    

    [UnityTest]
    public IEnumerator CheckObjectEnabled()
    {
        //Wait for scene loading
        yield return LoadScene("1");
        
        //get manual reference to the object
        var objectInstance = UITestTools.FindAnyGameObject<EnabledAtStart>();
        
        //ensure object enabled by component
        Check.IsEnable<EnabledAtStart>();
     
        //ensure object enabled by path in hierarchy
        Check.IsEnable("Object_enabled_at_start");

        //ensure object enabled by path in hierarchy and component type
        Check.IsEnable<EnabledAtStart>("Object_enabled_at_start");
        
        //ensure object enabled by object instance
        Check.IsEnable(objectInstance.gameObject);
    }

    [UnityTest]
    public IEnumerator CheckObjectExists()
    {
        //Wait for scene loading
        yield return LoadScene("1");
        
        Check.IsExist("Object_disabled_at_start");
        
        Check.IsExist<DisabledAtStart>();
        
        Check.IsExist<DisabledAtStart>("Object_disabled_at_start");
    }
    
    [UnityTest]
    public IEnumerator CheckObjectDontExists()
    {
        //Wait for scene loading
        yield return LoadScene("1");
        
        Check.IsNotExist("NonExist");
        
        Check.IsNotExist<NonExist>();
        
        Check.IsNotExist<NonExist>("NonExist");
    }
    
    [UnityTest]
    public IEnumerator CheckForToggle()
    {
        //Wait for scene loading
        yield return LoadScene("1");
        
        Check.IsToggle("container/Toggle");

        Check.IsNotToggle("container/child_enabled");

    }

}

 