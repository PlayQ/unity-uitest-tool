UI Test Tool
====================================
Content
----
* [About](#about)
	* [Instalation](#instalation)
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

This `Test Tool` simplifies testing of `Unity` games and makes possible to run tests on mobile devices. It's based on methods, atributes and logs, simular to [NUnit](http://nunit.org).


### Instalation

Just copy `UITestTools` folder and paste wherever you want in `Assets` folder of your project.


### Flow Recorder

`Flow Recorder` is an Unity Editor extension, which can record user actions and generate code of test from it.

To use `Test Recorder` navigate to `Window => Ui test tools => Flow recorder`, or press `Ctrl+T` for Windows and `Cmd+T` for Mac.

<img src="documentation/images/recorder-interface.png" width="600">

* `Start Record` button - enables recording mode, your clicks would be recorded as actions and added to existing action list. Then you can select necessary assertation type and edit params for each recorder actions.
* `Stop Record` - disables recording mode. All recorded actions are keep safe.
* `Check` - enables/disables mode, in which clicks on ui buttons don't trigger them. Window will be colored green when check mode is enabled. 
* `Pause mode` - set time scale to 0 when enabled, useful when it needs to emulate game pause and record things.
* `Clean` - deletes all recorded actions.
* `+` button - added new "empty" user action. You can select only assertations, which don't require gameobject to proceed to such action.
* `Generate Code and Copy` - generate code for test from recorded user actions.

Recorded users actions are visually displayed as a list. 
You can reorder already recorded actions by drag and drop.

Each item of list contains next information:

<img src="documentation/images/action-item.png" width="600">

* `6` - index of assertation in list.
* `Assertation Type` - there is a drop down menu with a list of assertation types. You can see only assertation types, available for current `GameObject`. Each assertation has its own list of require arguments.
* `Description` - add description as a commentary above generated `assertation method`. 
* `Path` - path in hierarchy to `GameObject` user interacts with. `Select` button will select current `GameObject` in hierarchy.
* `TimeOut`, `IgnoreTimeScale` - arguments list specific to selected assertation, values of these arguments could be edited by user. Arguments could be type of `enum`, `int`, `float`, `bool`, `string`.
* `►` - this button applies assertation in runtime.
* `Copy` - create a copy of this asertation and set it next to it. Also you can make by selection asertation and press `Ctrl+D` for Windows and `Cmd+D` for Mac.
* `x` - button removes action from list.


### How Flow Recorder works

When you clicks on `GameObject` during `Unity` playmode `Flow Recorder` takes click coords and uses `UnityEngine.EventSystems.EventSystem` class to raycast by these coords to find `GameObject` under click. Then `Flow Recorder` looks through list of all existing `assertations` and checks which `assertation` could be applied to `GameObject`. For example, if you clicks on text - all `assertations` that compares text labels would be available. Common `assertations`, like `Is Enable` or `Is Exist` are available for any `GameObject`. By default `Flow Recorder` supports only `Unity UI`, if you want support `GameObjects` that doesn't contain any UI components or use custom `assertations` you have to implement custom `assertation method`. Please read below how to [extend](#extending-test-tool) `UI Test Tool`.


### Recording a new test

Launch the game in editor, press `Start Record` button, perform any actions you need in game. You can chang assertation types or paramerts and change ordering during recording or after it. 

When test recording is over press `Stop Record` or exit play mode. Press `Generate Code and Copy` - to obtain code for test. Then you can paste generated code to your own test class.

Also you can create new assertation for object by right click to it in hierarchy and select `Create Assertation`.

<img src="documentation/images/recorder_window.gif" width="600">


### Test Runner

To open `Test Runner Window` navigate to `Window => Play Mode Test Runner`.

<img src="documentation/images/play-mode-testrunner.png" width="600">

`Test Runner` searches for all methods with custom attribute `[UnityTest]` and shows them like a folding list. Press `Run all` to run all tests.

You can select the amount of times each test is run. For it move `Repeat Tests N Times` slider.

Also you can change TimeScalse of tests by moving `Default Timescale` slider.

One of greates feature of the `Test Runner` - run test on devices. See command line arguments for it in [Command line](#command-line) section.


##### Run specific list of tests

You can select test that you want to run and press `Run` button. Also, set of selected tests is saved to `UITestTools/PlayModeTestRunner/Resources/SelectedTests.asset` scriptable object. 

When you run tests on mobile device `Play Mode Test Runner` loads `SelectedTests` and runs only selected tests. If `SelectedTests` is not exists - all tests will executed.


##### Smoke testing

You can mark any test by `[SmokeTest]` attribute. When you click `Rum Smoke` only this test will run.

For example:

``` c#
[SmokeTest]
[UnityTest]
public IEnumerator SomeTest()
{
	// some code 
}
```


##### Screen resolution depending tests

Sometimes your tests may succeed or fail depending on different screen resolution. For example test checks if `GameObject` located under certain screen pixels. To ensure that test is run only with proper resolution you have to add `[EditorResolutionAttribute]` above test method declaration and set target resolution for Editor. It doesn't effect to run tests on devises.

Also you can set any count of `TargetResolutionAttribute` to test and if device resolution not matched with resolution in any of attributes, test will ignored. This attribute no affect to test running in Editor.

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

`Test Helper` is an `Unity Editor Extension`, that shows list of possible assertations for selected `GameObject` in `Hierarchy` as completed code line. If you use `Test Helper` in play mode, you can also obtain list of playing sound clips.

<img src="documentation/images/test-helper.png" width="600">

`Copy` - copyes generated code of assertation.
`Find playing sounds` - searches for playing sound clips.
`Select` - only for sound, focuses on `GameObject` in hierarchy which has `Audio Source` component and it's `Sound Clip` is playing.


### Command Line Arguments

To run test in Editor Mode via console use followin command:

```
Unity.exe 
 -projectPath project_path
 -executeMethod TestToolBuildScript.RunPlayModeTests
 -runOnlySelectedTests 
 -runOnlySmokeTests
 -timeScale time_scale
`-buildNumber` - build_numer
```

`-project_path` - absolute path to project folder (Unity attribute);
`-runOnlySelectedTests` - optional, runs only tests selected in `Play Mode Test Runner` window;
`-runOnlySmokeTests` - optional, runs test with `SmokeTest` attribute only;
`-timeScale` - timescale for tests;
`-buildNumber` - used as postfix for file with test metrics.


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


API methods
--------

`Test Tool` common features are split in 5 static partial classes: `Check`, `Wait`, `Interact`, `AsyncCheck` and `AsyncWait`. Any of these classes contains list of methods which accordingly check state of given object, wait for resolving of specific condition and set state to given object.

For example:

```c#
yield return Wait.ObjectEnabled("UITutorial/2_1(Clone)/CCBFile/2_1_content/speech bubble/SpeechBubble/avatar_king", 20f);
Check.CheckEnabled("UITutorial/2_1(Clone)/CCBFile/2_1_content/overlay_square");
Check.CheckEnabled("UITutorial/2_1(Clone)/CCBFile/2_1_content/overlay_square_5");
Check.CheckEnabled("UITutorial/2_1(Clone)/CCBFile/2_1_content/speech bubble/SpeechBubble/speech bubble");
Check.TextMeshProEquals("UITutorial/2_1(Clone)/CCBFile/2_1_content/CCLabelBMFontv2", "Match 4 to enchant the feather charms you need in the row.");
Check.CheckEnabled("UITutorial/2_1(Clone)/CCBFile/2_1_content/pointer finger/SwipeAnim/pointer_finger");
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


Let's create a simple assertation method, which takes GameObject, checks whether it has your custom component `LevelButton` and check stars count on it. We will create iе in own partial `Check` class.

``` c#
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
Now we can use this method in out test method like this:

``` c#
[UnityTest]
public IEnumerator SomeIntegrationTest()
{
	// some code 
	
	Check.StarsCount("Level3/Button", 2);
}
```

Let's create class that show [Flow Recorder](#flow-recorder) when and how it should display it. It should be inherited from `ShowHelperBase` class and implement `CreateGenerator()` method. `CreateGenerator` desribe what entered paramets has method, what default parametrs should be used and how to generate code. 

Also you can override `bool IsAvailable(GameObject go)` method. It will set, is assertation available for current `GameObject`. As default it will return `true`, even if `GameObject` is `null`. You can override method `Camera GetCamera()` to use not main Camera to get game object by click on Flow Recorder not from main camera.

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

Let's bind our method and class.

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

Done! Now this action will be shown in `Flow Recorder` as `Check Start Count on Button` and will apeared only by clicking to `GameObject` with `LevelButton` component.

`ShowInEditorAttribute` recieve 3 paramers in constructor:

* `Type classType` - type of `ShowHelperBase` class with data for this test assertation.
* `string description` - human friendly description of what this assertation for, this label is shown in recorder in dropdown list of assertations.
* `bool isDefault` = false - optional, very often several assertation could be applied to selected `GameObject`.  If isDefault is true, recorder tries to show this assertation as a first one.


Now when you tap on `GameObject` on a game screen, `Flow Recorder` will look through all assertation methods with an `ShowInEditor` attribute. `Flow Recorder` will find `EditorHelpers` and will pass instance of selected GameObject to `IsAvailable` method. If it returns true then assertation method will be shown in list of assertation methods in Flow Recorder UI. Then `Flow Recorder` will find one with `default` value equals true and set it as current for this assertation.


Sometimes target GameObject could contain childs, which also could receive click. It can lead to confuse, for example you tap on text, but target GameObject is an image, because it's child of Text component and receives your click instead of Text component. That's why when Flow Recorder passes target GameObject to `IsAvailabel` method and if it returns false, Flow Recored takes parent of target GameObject and passes it to `IsAwailabel` method and so on, until parent is null.



Let's define another assertation method which checks some specific game mechanics: lets assume we have a `ScreenManager` `MonoBehaviour` on scene with a string property of current active screen, and we want to check if lobby screen is active.

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


When user click on item(button, switch etc) `Flow Recorder` takes click coords and uses `UnityEngine.EventSystems.EventSystem` class to raycast by these coords to find `GameObject` under click. This works only with Unity ui objects. In case when you click on non UI object you have to define property in `Class Helper` that return specific camera to raycast. 

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
