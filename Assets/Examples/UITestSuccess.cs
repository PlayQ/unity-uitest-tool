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
        //Wait for scene loading
        yield return null;
        MakeScreenShot("some path");
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
    }
    
    [UnityTest]
    public IEnumerator WaitForObjectDestraction()
    {
        //Wait for scene loading
        yield return LoadScene("1");
     
        //check object exists at the beginning by component
        yield return WaitForObject<ObjectThatWillBeDestroyedInSecond>();

        //check object exists at the beginning by name in hierarchy
        yield return WaitForObject("Object_that_will_be_destroyed_in_second");
        
        //manually search for object in scene
         var objectInstance = Object.FindObjectOfType(typeof(ObjectThatWillBeDestroyedInSecond)) as GameObject;
        
        
        //wait during interval for destraction of object with component "ObjectThatWillBeDestroyedInSecond"
        yield return WaitForDestroy<ObjectThatWillBeDestroyedInSecond>();
        
        //wait during interval for object destraction by object name "Object_that_will_be_destroyed_in_second"
        yield return WaitForDestroy("Object_that_will_be_destroyed_in_second");
        
        //wait during interval for object destraction by object instance
        yield return WaitForDestroy(objectInstance);
    }

    [UnityTest]
    public IEnumerator WaitForObjectDisabled()
    {
        //Wait for scene loading
        yield return LoadScene("1");
        
        //ensure object enabled by component
        Check.IsEnable<EnabledAtStart>();
     
        //ensure object enabled by path in hierarchy
        Check.IsEnable("Object_enabled_at_start");

        //get manual reference to the object
        var objectInstance = Object.FindObjectOfType<EnabledAtStart>();
        
        //ensure object enabled by object instance
        Check.IsEnable(objectInstance.gameObject);
        
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
        
        //ensure object disabled by component
        Check.IsDisable<DisabledAtStart>();
     
        //ensure object disabled by path in hierarchy
        Check.IsDisable("Object_disabled_at_start");
        
        //ensure object disabled by path in hierarchy and component type
        Check.IsDisable<DisabledAtStart>("Object_disabled_at_start");

        //get manual reference to the object
        var objectInstance = Check.FindAnyGameObject<DisabledAtStart>();
        
        //ensure object disbaled by object instance
        Check.IsDisable(objectInstance.gameObject);
        
        //manually enable object
        objectInstance.gameObject.SetActive(true);

        //check for enabled by component type
        WaitObjectEnabled<DisabledAtStart>();

        //check for enabled by path in hierarchy
        WaitObjectEnabled("Object_disabled_at_start");

    }
    
    //[UnityTest]
    public IEnumerator WaitForStaff()
    {
    // Wait until object with given component appears in the scene
        yield return WaitForObject<FirstScreen>();

        // Click on button
        Click("Button-OpenSecondScreen");
        
        //Wait untio
        yield return WaitForObject("SecondScreen/Text");

        yield return WaitForObject<SecondScreen>();

        // Wait until Text component with given name appears and assert its value
        yield return WaitForObject("SecondScreen/Text");
        Check.TextEquals("SecondScreen/Text", "Second screen");

        Click("Button-Close");

        // Wait until object with given component disappears from the scene
        yield return WaitForDestroy<SecondScreen>();
    }

    //[UnityTest]
    public IEnumerator SuccessfulNetworkResponseIsDisplayedOnTheFirstScreen()
    {
        yield return WaitForObject<FirstScreen>();


        yield return WaitForObject("Button-NetworkRequest");
        Click("Button-NetworkRequest");

        // Check the response displayed on UI
        Check.TextEquals("FirstScreen/Text-Response", "Test answer");

        // Assert the requested server parameter
//        Assert.AreEqual(FirstScreen.NetworkClient.MockRequest, "i_need_data");
    }

    /*
    [UnityTest]
    public IEnumerator FailingBoolCondition()
    {
        yield return WaitForObject("FirstScreen");
        var s = Object.FindObjectOfType<FirstScreen>();

        // Wait until FirstScene component is disabled, this line will fail by timeout
        // BoolCondition can be used to wait until any condition is satisfied
        yield return WaitForCondition(() => !s.enabled);
    }
    
    */
}

 