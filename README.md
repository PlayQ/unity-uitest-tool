UI Test Tool
====================================
Content
----
* [About](#about)
	* [Installation](#installation)
	* [Flow Recorder](#flow-recorder)
	* [How Flow Recorder works](#how-flow-recorder-works)
	* [Recording a new test](#recording-a-new-test)
	* [Test Runner](#test-runner)
		* [Run specific list of tests](#run-specific-list-of-tests)
		* [Smoke testing](#smoke-testing)
		* [Screen resolution depending tests](#screen-resolution-depending-tests)
	* [Helper Window](#helper-window)
	* [Command Line Arguments](#command-line-arguments)
* [API methods](#api-methods)
* [Extending Test Tool](#extending-test-tool)
	* [Example 1](#example-1) 
	* [ShowInEditor](#showineditor)
	* [How Recording works with Assertations](#how-recording-works-with-assertations)
	* [Example 2](#example-2)
	* [Another Camera](#another-camera)


UI Test Tool
====================================
Content
----
* [About](#about)
	* [Installation](#installation)
	* [Flow Recorder](#flow-recorder)
	* [How Flow Recorder works](#how-flow-recorder-works)
	* [Recording a new test](#recording-a-new-test)
	* [Test Runner](#test-runner)
		* [Run specific list of tests](#run-specific-list-of-tests)
		* [Smoke testing](#smoke-testing)
		* [Screen resolution depending tests](#screen-resolution-depending-tests)
	* [Helper Window](#helper-window)
	* [Command Line Arguments](#command-line-arguments)
* [API methods](#api-methods)
* [Extending Test Tool](#extending-test-tool)


About
---------------------

This `Test Tool` simplifies testing of `Unity` games and allows to run tests on mobile devices. It's based on methods, attributes, and logs, similar to those in [NUnit](http://nunit.org).


### Installation

Just copy `UITestTools` folder and paste it wherever you want inside `Assets` folder of your project.


### Flow Recorder

`Flow Recorder` is an Unity Editor extension that allows to record user actions and generate test's source code from them.

To use `Test Recorder` navigate to `Window => Ui test tools => Flow recorder`, or press `Ctrl+T` for Windows and `Cmd+T` for Mac.

<img src="documentation/images/recorder-interface.png" width="600">

* `Start Record` button - enables recording mode. Once enabled, user's mouse input (LMB clicks) is recorded into test actions and appended to test's action list.
* `Stop Record` - disables recording mode. All previously recorded actions are serialized automatically.
* `Check` - toggles mode, in which clicks on UI buttons don't trigger them. The window will be colored green when check mode is enabled.
* `Pause mode` - sets current timescale to 0 in play mode. Used in situations where user needs to emulate game pause yet still be able to record new actions.
* `Clean` - deletes all previously recorded actions.
* `+` button - manually adds new "empty" user action. Provides access to actions that don't require `GameObject` to be clicked
* `Generate Code and Copy` - generate source code for test based on recorded user actions.

Recorded users actions are visually displayed as a list. User can edit recorded actions by selecting necessary assertation type, editing arguments, drag-and-dropping actions inside the list to change execution order, adding or removing actions manually.

Each action contains the following information:

<img src="documentation/images/action-item.png" width="600">

* `6` - action's index in the list.
* `Assertation Type` - a drop-down menu with a list of assertation types, available for current `GameObject`. Each assertation has its own list of required arguments.
* `Description` - a brief description to selected `assertation method`. Results in a commentary above assertation method call in the generated source code
* `Path` - path in the scene hierarchy to the `GameObject` user interacted with. `Select` button allows to select the specified `GameObject`
* `TimeOut`, `IgnoreTimeScale` - selected assertation's arguments list. Argument values can be edited by user. Available arguments types are `enum`, `int`, `float`, `bool`, `string`.
* `►` - taplies assertation in runtime.
* `Copy` - duplicates selected asertation and places the copy next to it. Hotkeys are `Ctrl+D` for Windows and `Cmd+D` for Mac.
* `x` - removes selected action from list.


### How Flow Recorder works

When you perform a LMB click in the `Game` window during `Unity` play mode, `Flow Recorder` obtains click coordinates and uses `UnityEngine.EventSystems.EventSystem` class to raycast by these coordinates to find the `GameObject` user clicked. Then `Flow Recorder` looks through a list of all existing `assertations` and checks which `assertation` could be applied to the `GameObject`.

For example, if user clicked on UI text - all `assertations` that compare text labels become available. Common `assertations`, like `Is Enable` or `Is Exist` are available for any `GameObject`. By default, `Flow Recorder` supports only `Unity UI` raycast targets. If a support for `GameObject`s that don't have any UI components attached or use of custom `assertations` is required you have to implement custom `assertation method`. Please read below how to [extend](#extending-test-tool) `UI Test Tool`.


### Recording a new test

Launch the game from the editor, press `Start Record` button to start recording actions, perform any desired actions in the game window, then press 'Stop Record' button to stop recording. You can change assertation types or parameters and change action order during the test recording or any time after it. Press `Generate Code and Copy` to generate and obtain the source code for the test. Then you can paste the generated code to your own test class.

Also, you can create new assertation for the object by `Right Click`ing it in the hierarchy and selecting `Create Assertation`.

<img src="documentation/images/recorder_window.gif" width="600">


### Test Runner

To open `Test Runner Window` navigate to `Window => Play Mode Test Runner`.

<img src="documentation/images/play-mode-testrunner.png" width="600">

`Test Runner` searches the project for all methods with the attribute `[UnityTest]` and represents them as a tree structure with namespaces and classes as branches and methods as leaves. Press `Run all` to run all tests.

You can select the number of times each test is run by dragging the `Repeat Tests N Times` slider.

Also, you can change the `TimeScale` value applied to game environment before the test run by dragging the `Default Timescale` slider.

One of the greatest features of the `Test Runner` tool is that it allows to run tests directly on target devices. See command line arguments for it in [Command line](#command-line-arguments) section.


##### Run specific list of tests

You can select specific tests that you want to run and press the `Run` button. The set of selected tests is saved to `UITestTools/PlayModeTestRunner/Resources/SelectedTests.asset` scriptable object. 

When you run tests on mobile device `Play Mode Test Runner` tool loads `SelectedTests` asset and performs only selected tests. If `SelectedTests` asset does not exist - all tests will be performed.


##### Smoke testing

You can mark any test with `[SmokeTest]` attribute. Clicking the `Rum Smoke` button allows to perform only tests marked with this attribute.

For example:

``` c#
[SmokeTest]
[UnityTest]
public IEnumerator SomeTest()
{
	// some code 
}
```


##### Screen resolution dependant tests

Sometimes, tests may succeed or fail depending on the different screen resolution. For example, the test checks whether a specific `GameObject` is located at the certain screen pixel coordinates. To ensure that the test is run only at the proper resolution you have to add `[EditorResolutionAttribute]` attribute above test method declaration and specify target resolution for Editor. It doesn't affect tests that are performed on devices.

Also, you can add any amount of `TargetResolutionAttribute` attributes to test. If device resolution matches not any of them, the test is ignored. If `EditorResolutionAttribute` is not set, `TargetResolutionAttribute` values are used for Editor Resolution.

For example:
```c#
[TargetResolution(1920, 1080)]
[TargetResolution(1334, 750)]
[TargetResolution(1136, 640)]
[EditorResolution(1920, 1080)]
[UnityTest]
public IEnumerator SomeTest()
{....}
```

### Helper Window

`Test Helper` is an `Unity Editor Extension`, that shows a list of possible assertations for selected `GameObject` in `Hierarchy` as completed code line. If you use `Test Helper` in play mode, you can also obtain the list of playing sound clips.

<img src="documentation/images/test-helper.png" width="600">

* `Copy` - copies generated code of assertation.
* `Find playing sounds` - searches for playing sound clips.
* `Select` - only for sound, focuses on `GameObject` in hierarchy which has `Audio Source` component and it's `Sound Clip` is playing.


### Command Line Arguments

To run tests in Editor Mode via command line use following command:

```
Unity.exe 
 -projectPath project_path
 -executeMethod TestToolBuildScript.RunPlayModeTests
 -runOnlySelectedTests 
 -runOnlySmokeTests
 -timeScale time_scale
`-buildNumber` - build_numer
```

* `-projectPath` - absolute path to project folder (Unity attribute);
* `-runOnlySelectedTests` - optional, runs only tests selected in `Play Mode Test Runner` window;
* `-runOnlySmokeTests` - optional, runs test with `SmokeTest` attribute only;
* `-timeScale` - timescale for tests;
* `-buildNumber` - used as postfix for file with test metrics.


To make test build to run test on the device use `TestToolBuildScript.TestBuild` method instead of `TestToolBuildScript.RunPlayModeTests`:

```
Unity.exe 
 -projectPath project_path 
 -testBuildPath result_path
 -executeMethod TestToolBuildScript.TestBuild
 -runOnlySelectedTests 
 -runOnlySmokeTests
 -buildNumber build_numer
 -timeScale time_scale
```

All arguments are the same.


API methods
--------

`Test Tool` common features are split to 5 static partial classes: `Check`, `Wait`, `Interact`, `AsyncCheck` and `AsyncWait`. Any of these classes contains a list of methods which accordingly check the state of given object, wait for resolving of specific condition and set state to given object.

For example:

```c#
yield return Wait.ObjectEnabled("SpeechBubble/avatar_king", 20f);
Check.CheckEnabled("content/overlay_square");
Check.CheckEnabled("content/overlay_square_5");
Check.CheckEnabled("content/speech bubble/SpeechBubble/speech bubble");
Check.TextMeshProEquals("content/CCLabelBMFontv2", "Match 4 to enchant the feather charms you need in the row.");
Check.CheckEnabled("content/pointer finger/SwipeAnim/pointer_finger");
yield return Interact.SwipeCell(4, 7, Interact.SwipeCellClass.SwipeDirection.Down, true, 1f);
```

`Async` assertions used for checking and waiting during which you want to do some interactions.

For example:

```c#
var soundCheck = AsyncCheck.CheckSoundPlaying("button_sound");
yield return Interact.WaitDelayAndClick("LayoutComponent/ButtonCherryTree", 0, 20f);
yield return soundCheck;
```

### [See full list of all assertations here!](APIREADME.md)


Extending Test Tool
-----------

You can extend all of 5 classes with actions (`Check`, `Wait`, `Interact`, `AsyncCheck` and `AsyncWait`) because thay are partial.


### Example 1

Let's create a simple assertation method, which takes `GameObject`, checks whether it has your custom component `LevelButton` and check stars count on it. We will create it in own partial `Check` class.

``` c#
public static partial Check
{

...

    public static void StarsCount(string path, int startCount) //path is a full path of GameObject in hierarchy on scene
    {
        var go = IsExist(path); //if object not exist, exception will be thrown and test failed
        var levelButton = go.GetComponent<LevelButton>();
        //lest fail test, if component doesn't present on the object
        if (levelButton == null)
        {
            Assert.Fail("StarsCount: " + path + " object is exist, but LevelButton component not found.");
        }
        Assert.AreEqual(levelButton.StartCount, startCount, "Start Count is not equals.");
    }
}
```
Now we can use this method in our test method like this:

``` c#
[UnityTest]
public IEnumerator SomeIntegrationTest()
{
	// some code 
	
	Check.StarsCount("Level3/Button", 2);
}
```

Let's create class that shows [Flow Recorder](#flow-recorder) when and how it should display it. It should be inherited from `ShowHelperBase` class and implement `CreateGenerator()` method. `CreateGenerator` desribe what entered paramets has method, what default parameters should be used and how to generate code. 

You can override `bool IsAvailable(GameObject go)` method. This class will set, is assertation available for current `GameObject`. As default it will return `true`, even if `GameObject` is `null`. 

```c#
private class CheckStarsCount : ShowHelperBase
{
    public override bool IsAvailable(GameObject go)
    {
        if (!go)
        {
            return false; //not available if game object is null
        }
	//not available if game object doen't containce LevelButton component
        return go.GetComponent<LevelButton>() != null;
    }

    public override AbstractGenerator CreateGenerator(GameObject go)
    {
        var levelButton = go.GetComponent<LevelButton>();
	//tell Flow Recorder about params and default values
        return VoidMethod.Path(go).Int(levelButton.StartCount);
	//if method is IEnumerator, use IEnumeratorMethod instead VoidMethod.
    }
}
```

Let's bind our method to class.

``` c#
[ShowInEditor(typeof(CheckStarsCount), "Check Start Count on Button")]
public static void StarsCount(string path, int startCount) //path is a full path of GameObject in hierarchy on scene
{
    var go = IsExist(path); //if object not exist, exception will be thrown and test failed
    var levelButton = go.GetComponent<LevelButton>();
    //lest fail test, if component doesn't present on the object
    if (levelButton == null)
    {
        Assert.Fail("StarsCount: " + path + " object is exist, but LevelButton component not found.");
    }
    Assert.AreEqual(levelButton.StartCount, startCount, "Start Count is not equals.");
}
```

Done! Now this action will be shown in `Flow Recorder` as `Check Start Count on Button` and will appeared only by clicking to `GameObject` with `LevelButton` component.


### ShowInEditor

`ShowInEditorAttribute` used to bind method and helper class. It receive 3 parameters in constructor:

* `Type classType` - a type of `ShowHelperBase` class with data for this test assertation.
* `string description` - human-friendly description of what this assertation for, this label is shown in recorder in dropdown list of assertations.
* `bool isDefault` - optional (false), very often several assertations could be applied to selected `GameObject`.  If isDefault is true, recorder tries to show this assertation as a first one.


### How Recording works with Assertations

When user tap on `GameObject` on a game screen, `Flow Recorder` will look through all assertation methods with a `ShowInEditor` attribute. `Flow Recorder` will find `EditorHelpers` and will pass an instance of selected GameObject to `IsAvailable` method. If it returns `true` then assertation method will be shown in the list of assertation methods in `Flow Recorder` UI. Then `Flow Recorder` will find one with `default` value equals `true` and set it as current for this assertation.


Sometimes target `GameObject` could contain children, which also could receive a click. It can lead to confusing, for example, you tap on text, but target `GameObject` is an image because it's child of `Text` component and receives your click instead of `Text` component. That's why when `Flow Recorder` passes target `GameObject` to `IsAvailable` method and if it returns `false`, `Flow Recorded` takes parent of target `GameObject` and passes it to `IsAvailable` method and so on, until the parent is null.

### Example 2

Let's define another assertation method which checks some specific game mechanics: let's assume we have a `ScreenManager` `MonoBehaviour` on the scene with a string property of the current active screen, and we want to check if lobby screen is active.

``` c#
[ShowInEditor(typeof(CheckIsScreenActive), "Check Current Screen")]
public static void IsScreenActive(string screenName)
{
    // FindAnyGameObject has overload to seach for GameObject with given type of component.
    var screenManager = UITestUtils.FindAnyGameObject<ScreenManager>();
    if (!screenManager)
    {
        Assert.Fail("IsScreenActive: no ScreenManager found");
    }
    Assert.AreEquals(screenManager.CurrentScreen, screenName);
}

private class CheckIsScreenActive : ShowHelperBase
{
    public override AbstractGenerator CreateGenerator(GameObject go)
    {
        return VoidMethod.String(ScreenManager.CurrentScreen);
    }
}
```

Example in test:

``` c#
[UnityTest]
public IEnumerator SomeIntegrationTest()
{
	// some code 
	
	Check.IsScreenActive("Lobby_Screen");
}
```

### Another Camera

When user click on item (button, switch etc) `Flow Recorder` takes click coordinates and uses `UnityEngine.EventSystems.EventSystem` class to make raycast by these coords to find `GameObject` under click. This works only with Unity UI objects. In case when you click on non UI object you have to define property in Helper class that return specific camera to raycast. 

```c#
static class MapLocationClick
{
    public static Camera Camera
    {
        get
        {
            // return custom game Camera to raycast to specific GameObject
            return Map.mapCamera; 
        }
    }
}
```
