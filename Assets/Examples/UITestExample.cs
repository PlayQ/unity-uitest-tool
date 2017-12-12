﻿using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;
using PlayQ.UITestTools;
using Object = UnityEngine.Object;

public class UITestExample : UITestBase
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
        yield return WaitForObject<FirstScreen>();

        // Wait until button with given name appears and simulate press event
        Click("Button-OpenSecondScreen");
        yield return WaitForObject("SecondScreen/Text");

        yield return WaitForObject<SecondScreen>();

        // Wait until Text component with given name appears and assert its value
        yield return WaitForObject("SecondScreen/Text");
        Check.TextEquals("SecondScreen/Text", "Second screen");

        Click("Button-Close");

        // Wait until object with given component disappears from the scene
        yield return WaitForDestroy<SecondScreen>();
    }

    [UnityTest]
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

    [UnityTest]
    public IEnumerator FailingBoolCondition()
    {
        yield return WaitForObject("FirstScreen");
        var s = Object.FindObjectOfType<FirstScreen>();

        // Wait until FirstScene component is disabled, this line will fail by timeout
        // BoolCondition can be used to wait until any condition is satisfied
        yield return WaitForCondition(() => !s.enabled);
    }
}

 