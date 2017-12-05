using UnityEngine;
using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using System.Linq;

public class UITestExample : UITest
{
    [SetUp]
    public void Init()
    {
        LoadSceneForSetUp("TestableGameScene");
    }
    
    [UnityTest]
    public IEnumerator SecondScreenCanBeOpenedFromTheFirstOne()
    {
        yield return LoadScene("1");

        // Wait until object with given component appears in the scene
        yield return WaitForObjectAppeared<FirstScreen>();

        // Wait until button with given name appears and simulate press event
        yield return WaitAndPress("Button-OpenSecondScreen");

        yield return WaitForObjectAppeared<SecondScreen>();
        
        // Wait until Text component with given name appears and assert its value
        yield return AssertLabel("SecondScreen/Text", "Second screen");

        yield return WaitAndPress("Button-Close");

        // Wait until object with given component disappears from the scene
        yield return WaitForObjectDisappeared<SecondScreen>();
    }

    [UnityTest]
    public IEnumerator SuccessfulNetworkResponseIsDisplayedOnTheFirstScreen()
    {
        yield return WaitFor(new ObjectAppeared<FirstScreen>());

        // Predefine the mocked server response
        FirstScreen.NetworkClient.MockResponse = "Success!";

        yield return WaitAndPress("Button-NetworkRequest");

        // Check the response displayed on UI
        yield return AssertLabel("FirstScreen/Text-Response", "Success!");

        // Assert the requested server parameter
        Assert.AreEqual(FirstScreen.NetworkClient.MockRequest, "i_need_data");
    }

    [UnityTest]
    public IEnumerator FailingBoolCondition()
    {
        yield return WaitForObjectAppeared("FirstScreen");
        var s = Object.FindObjectOfType<FirstScreen>();

        // Wait until FirstScene component is disabled, this line will fail by timeout
        // BoolCondition can be used to wait until any condition is satisfied
        yield return WaitForCondition(() => !s.enabled);
    }
}

 