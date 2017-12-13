using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;
using PlayQ.UITestTools;
using UnityEngine;
using Object = UnityEngine.Object;

public class UITestCheckSuccess : UITestBase
{
  


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
    public IEnumerator CheckObjectDisabledFailCases()
    {
        //Wait for scene loading
        yield return LoadScene("1");
        
        try {
            Check.IsDisable<EnabledAtStart>();
        } catch (AssertionException ex) {
            if(!ex.Message.EndsWith("enabled")) {
                throw ex;
            }
        }
        
        try {
            Check.IsDisable("Object_enabled_at_start");
        } catch (AssertionException ex) {
            if(!ex.Message.EndsWith("enabled")) {
                throw ex;
            }
        }
        
        try {
            Check.IsDisable<EnabledAtStart>("Object_enabled_at_start");
        } catch (AssertionException ex) {
            if(!ex.Message.EndsWith("enabled")) {
                throw ex;
            }
        }
        
        var objectInstance = UITestTools.FindAnyGameObject<EnabledAtStart>();
        
        try {
            Check.IsDisable(objectInstance.gameObject);
        } catch (AssertionException ex) {
            if(!ex.Message.EndsWith("enabled")) {
                throw ex;
            }
        }
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
    public IEnumerator CheckObjectEnabledFailCases()
    {
        //Wait for scene loading
        yield return LoadScene("1");
        
        try {
            Check.IsEnable<DisabledAtStart>();
        } catch (AssertionException ex) {
            if(!ex.Message.EndsWith("Object disabled")) {
                throw ex;
            }
        }
        
        
        try {
            Check.IsEnable("Object_disabled_at_start");
        } catch (AssertionException ex) {
            if(!ex.Message.EndsWith("Object disabled")) {
                throw ex;
            }
        }
        
        try {
            Check.IsEnable<DisabledAtStart>("Object_disabled_at_start");
        } catch (AssertionException ex) {
            if(!ex.Message.EndsWith("Object disabled")) {
                throw ex;
            }
        }
        
        var objectInstance = UITestTools.FindAnyGameObject<DisabledAtStart>();
        try {
            Check.IsEnable(objectInstance.gameObject);
        } catch (AssertionException ex) {
            if(!ex.Message.EndsWith("Object disabled")) {
                throw ex;
            }
        }
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
    public IEnumerator CheckObjectExistsFailCases()
    {
        //Wait for scene loading
        yield return LoadScene("1");
        
        try {
            Check.IsNotExist("NonExist");
        } catch (AssertionException ex) {
            if(!ex.Message.EndsWith("not exist.")) {
                throw ex;
            }
        }
        
        try {
            Check.IsExist<NonExist>();
        } catch (AssertionException ex) {
            if(!ex.Message.EndsWith("not exist.")) {
                throw ex;
            }
        }
        
        try {
            Check.IsExist<NonExist>("NonExist");
        } catch (AssertionException ex) {
            if(!ex.Message.EndsWith("not exist.")) {
                throw ex;
            }
        }
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
    public IEnumerator CheckObjectDontExistsFailCases()
    {
        //Wait for scene loading
        yield return LoadScene("1");
        
        try {
            Check.IsNotExist("Object_enabled_at_start");
        } catch (AssertionException ex) {
            if(!ex.Message.EndsWith(" exists.")) {
                throw ex;
            }
        }
        
        try {
            Check.IsNotExist<EnabledAtStart>();
        } catch (AssertionException ex) {
            if(!ex.Message.EndsWith(" exists.")) {
                throw ex;
            }
        }
        
        try {
            Check.IsNotExist<EnabledAtStart>("Object_enabled_at_start");
        } catch (AssertionException ex) {
            if(!ex.Message.EndsWith(" exists.")) {
                throw ex;
            }
        }
    }
    
    [UnityTest]
    public IEnumerator CheckForToggle()
    {
        //Wait for scene loading
        yield return LoadScene("1");
        
        Check.IsToggleOn("container/Toggle_on");
        Check.IsToggleOff("container/Toggle_off");
    }

    [UnityTest]
    public IEnumerator CheckForToggleOffFailCases()
    {
        //Wait for scene loading
        yield return LoadScene("1");
        
        try {
            Check.IsToggleOff("container/Toggle_on");
        } catch (AssertionException ex) {
            if(!ex.Message.EndsWith("is ON.")) {
                throw ex;
            }
        }
        
        try {
            Check.IsToggleOff("Object_enabled_at_start");
        } catch (AssertionException ex) {
            if(!ex.Message.EndsWith("has no Toggle component.")) {
                throw ex;
            }
        }
        
    }
    
    [UnityTest]
    public IEnumerator CheckForToggleOnFailCases()
    {
        //Wait for scene loading
        yield return LoadScene("1");
        
        try {
            Check.IsToggleOn("container/Toggle_off");
        } catch (AssertionException ex) {
            if(!ex.Message.EndsWith("is OFF.")) {
                throw ex;
            }
        }
        
        try {
            Check.IsToggleOn("Object_enabled_at_start");
        } catch (AssertionException ex) {
            if(!ex.Message.EndsWith("has no Toggle component.")) {
                throw ex;
            }
        }
    }

}

 