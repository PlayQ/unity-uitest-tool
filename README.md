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
	* [Implementing own Assertation method](#implementing-own-assertation-method)
	* [ShowInEditor custom attribute](#showineditor-custom-attribute)


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
Unity.exe -projectPath project_path -executeMethod TestToolBuildScript.RunPlayModeTests -runOnlySelectedTests -runOnlySmokeTests
```

`project_path` - absolute path to project folder

`-runOnlySelectedTests` - optional, runs only tests selected in `Play Mode Test Runner` window.

`-runOnlySmokeTests` - optional, runs test with [SmokeTest] attribute only.

This is common script, which we use on teamcity to make build:
```
%UNITY_PATH% \
 -quit \
 -batchmode \
 -serial $USINGSERIAL \
 -username $USINGUSER \
 -password $USINGPASS \
 -out %teamcity.build.checkoutDir%/builds/android/CharmKing.apk \
 -projectPath %teamcity.build.checkoutDir% \
 -logFile %teamcity.build.checkoutDir%/unity.log \
 -buildNumber $intversion \
 -keystoreName "%ANDROID_KEYSTORE_FILE%" \
 -keystorePass "%ANDROID_KEYSTORE_PASS%" \
 -keyaliasName "%ANDROID_KEYALIAS_NAME%" \
 -keyaliasPass "%ANDROID_KEYALIAS_PASS%" \
 -addDefines $ADD_DEFINES \
 -buildMode $BUILD_MODE \
 -reporter TeamCity \
 -buildTarget android \
 -target %TARGET_ANDROID% \
 -androidSDKPath %ANDROID_SDK_HOME% \
 -jdkPath %env.JAVA_HOME% \
 -executeMethod PlayQ.Build.CommandLineBuild.Build \
```

To make test build add following parameters to previous statement:

`-makeTestBuild` - it will run tests when your app starts.

`-runOnlySelectedTests` - optional, runs only tests selected in `Play Mode Test Runner` window.

`-runOnlySmokeTests` - optional, runs test with [SmokeTest] attribute only. 


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

List of all assertations Comming soon...


Extending Test Tool
-----------

You can extend all of 5 classes with actions (`Check`, `Wait`, `Interact`, `AsyncCheck` and `AsyncWait`) because thay are partial.


### Implementing own Assertation method

Let's create a simple assertation method, which takes GameObject, checks whether it has your custom component `LevelButton` and check stars count on it. We will ckreate in in own partial `Check` class.

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

But it's too much of manual work to find necessary GameObject in editor's scene, copy it's full path and paste into test code. [Flow Recorder](#flow-recorder) can do this for you. Let's create class that show `Flow Recorder` when and how it should display it. It should be inherited from `ShowHelperBase` class and implement `CreateGenerator`.

Also you can override `bool IsAvailable(GameObject go)` method. It will set, is assertation available for current `GameObject`. As default it will return `true`, event `GameObject` is `null`. You can override method `Camera GetCamera()` to use not main Camera to get game object by click on Flow Recorder not from main camera. 

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
[ShowInEditor("Is Exist")]
public static void IsExist(string path)
{    
    //UITestUtils is a class with a bunch of helpfull methods. For example unity's 
    //GameObject.Find searches only for enabled GameObjects.
    //FindAnyGameObject can find disabled GameObjects as well. 
    var go = UITestUtils.FindAnyGameObject(path);
    if (go == null)
    {
        Assert.Fail("IsExist: Object with path " + path + " does not exist.");
    }
}
```

Custom attribute `ShowInEditor` means, that this method is shown in Flow Recorder. The first `string` parameter of attribute constructor is a description. Description is shown in Flow Recorder, when you pick assertation method you want to use. 

Let's define another assertation method which checks some specific game mechanics: lets assume we have a `ScreenManager` monobehaviour on scene with a string property of current active screen, and we want to check if lobby screen is active.

``` c#
[ShowInEditor("Check current screen", false)]
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

The second parameter of ShowInEditor's constructor is a `string pathInPresent` and it's set to `false`. Parameter `pathInPresent` is set to `true` by default and means that if the first argument of assertation method is type of `string` - it's a path to GameObject. In our case it's just a string value and not a path, so you have to set it to `false`. That doesn't looks very nice, but it's important for code generation, and spares you from unnecessary description of assertation methods parameters. You can read more about it below.

Very often you have to pass more that 1 argument to assertation method to check things you need. Let's implement method, that checks text on our UI component.

``` c#
public static void TextEquals(string path, string expectedText)
{
    var go = UITestUtils.FindAnyGameObject(path);
    Assert.AreEqual(null, go, "InputEquals: Object " + path + " is not exist");
    var t = go.GetComponent<Text>();
    if (t == null)
    {
        Assert.Fail("TextEquals: Object " + go.name + " has no Text attached");
    }
    if (t.text != expectedText)
    {
        Assert.Fail("TextEquals: Label " + go.name + " actual text: " + t.text + ", expected "+expectedText+", text dont't match");
    }
}
```
Example in test:

``` c#
[UnityTest]
public IEnumerator SomeIntegrationTest()
{
	// some code 
	Check.TextEquals("MainUI/PLayerInfo/PlayerClass", "warrior");
}
```
If you record your actions with Flow Recorder, it generates such code for you. When you tap on GameObject with text, Flow Recorder checks if GameObject contains MonoBehaviour with a type of `UnityEngine.UI.Text` and if check is successfull Flow Recorder takes GameObject's path in hierarchy and text value and substitutes these values into method call code, so it looks like this:

`Check.TextEquals("MainUI/PLayerInfo/PlayerClass", "warrior");`

But how Flow Recorder would know which component to serach and which value to substitute into method call code. You have to provide all information about it in a specific `helper class`. You can name this class like you want, `helper class` is just a term for class with metadata related to specific assertation method.

Let's define a static class helper and name it `CheckTextEquals`. Also we have to link class `CheckTextEquals` to method `TextEquals` with a help of `ShowInEditor` attribute.

``` c#
[ShowInEditor(typeof(CheckTextEquals), "Text equals")]
public static void TextEquals(string path, string expectedText)
{
    // assertation code
}

static class CheckTextEquals
{
    //some methods will be implemented
}
```

`TextEquals` can't be applied to any GameObject, but only to that one with `Text` component. So we have to define in class `CheckTextEquals` public static method `bool IsAvailable(GameObject go)`. Method name, argument and return type is processed by reflection, so they must be exactly like described. 

``` c#
[ShowInEditor(typeof(CheckTextEquals), "Text equals")]
public static void TextEquals(string path, string expectedText)
{
    // assertation code
}

static class CheckTextEquals
{
    public static bool IsAwailable(GameObjec go)
    {
        return go.GetComponent<Text>() != null;	
    }
}
```

Now when you tap on GameObject on a game screen, Flow Recorder will look through all assertation methods with an `ShowInEditor` attribute. If type is passed to the first parameter of attribute consctructor, Flow Recorder will check, if given type has method with name `IsAwailable` and if so, Flow Recorder will pass instance of selected GameObject to `IsAwailable` method. If it returns true then assertation method will be shown in list of assertation methods in Flow Recorder UI.
If there is no `IsAwailable` method or `ShowInEditor` has no type passed to first parameter Flow Recorder assume that assertation method is common, and could be aplied to any GameObject and assertation method will be shown in list of methods.

Sometimes target GameObject, in our case it's a GameObject with a `Text` component, could contain childs, which also could receive click. Child could be also with alpha 0 or disabled, so you will not notice them. It can lead to confuse, for example you tap on text, but target GameObject is an image, because it's child of Text component and receives your click instead of Text component. That's why when Flow Recorder passes target GameObject to `IsAwailabel` method and if it returns false, Flow Recored takes parent of target GameObject and passes it to `IsAwailabel` method and so on, until parent is null.

When you click on GameObject with `Text` component Flow Recorder takes it text values and substitute it to the call code. To implement such behaviour you have to implement method 

`public static List<object> GetDefautParams(GameObject go)`

in `CheckTextEquals` class. This method takes target GameOBject, obtains necessary values from it and returns list of values for assertation method arguments. Method name, return type and parameter type are important, because they are processed by reflection.
 
 
``` c#
[ShowInEditor(typeof(CheckTextEquals), "Text equals")]
public static void TextEquals(string path, string expectedText)
{
    // assertation code
}

static class CheckTextEquals
{
    public static bool IsAwailable(GameObjec go)
    {
        return go.GetComponent<Text>() != null;	
    }

    public static List<object> GetDefautParams(GameObject go)
    {
        List<object> result = new List<object>();
        var labelText = go.GetComponent<Text>();
        result.Add(labelText.text);
        return result;
    }
}
```

Assertation `TextEquals` takes two arguments: path to GameObject and expected text, but `GetDefautParams` return list with only one value - expected text. That is because if first argument of assertation method is path to GameObject it's substituted automatically for you, so you must not add path to object list in `GetDefautParams` method. Also ShowInEditor's third parameter is `pathIsPresent` flag. It's true by default, because most of assertation method are working with GameObjects. Flag means that first string argument is path to GameObject and not a regular string value.

All assertation methods described above are checking something immediately. But what if we want to wait for some condition during given timeout. In such case assertaton method has to return `IEnumerator`. Let's implement method that waits until GameObject is enabled.

``` c#
[ShowInEditor(typeof(WaitForEnabledClass), "Wait for enabled")]
public static IEnumerator WaitForEnabled(string path, float timeOut)
{
    yield return WaitForEnabledClass.WaitFor(() =>
    {
        var obj = GameObject.Find(path); //finds only enabled gameobjects
        return obj != null && obj.activeInHierarchy;

    }, timeOut);
}

static class WaitForEnabledClass
{
    // helper method which awaits for condition during timeout
    public static IEnumerator WaitFor(Func<bool> condition, float timeout)
    {
        float time = 0;
        while (!condition())
        {
            time += Time.unscaledDeltaTime;
            if (time > timeout)
            {
                throw new Exception("Operation timed out");
            }
            yield return null;
        }
    }

    public static List<object> GetDefautParams(GameObject go)
    {
        List<object> result = new List<object>();
        var defaultTimeout = 2f; //default. this values could be edited in Flow Recorder Window.
        result.Add(defaultTimeout);
        return result;
    }
}
```

Example in test method:

``` c#
[UnityTest]
public IEnumerator SomeIntegrationTest()
{
	// some code 
	yield return Wait.WaitForEnabled("MainUI/PLayerInfo/PlayerClass", 10f);
}
```


When user click on item(button, switch etc) Flow Recorder takes click coords and uses `UnityEngine.EventSystems.EventSystem` class to raycast by these coords to find `GameObject` under click. This works only with unity ui objects. In case when you click on non UI object you have to define property in `Class Helper` that return specific camera to raycast. 

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


### ShowInEditor custom attribute

`ShowInEditorAttribute` constructor  params:
* `Type classType` - type of class helper. Optional, skip it if you just want to add method to flow recorder, without any specific parameters, except probably path.
* `string description` - human friendly description of what this assertation for, this label is shown in recorder in dropdown list of assertations.
* `bool pathIsPresent` = true - optional, if first param of assertation method is path to `GameObject`, set it to true. If assertation method doesn’t use any `GameObject` - set false.
* `bool isDefault` = false - optional, very often several assertation could be applied to selected `GameObject`.  If isDefault is true, recorder tries to show this assertation as a first one.

To add more features, which are not common and related only to your project, extend these classes by creating new partial classes and add new `assertation methods`. If you want to add `assertation method` to `Flow Recorder` - add class helper near the `assertation method`. Try to separate methods by its assignment: if method checks some condition - it has to be added to `Check` class, if methods awaits for some action - it has to be added to `Wait` class, and if method changes some values in game - it has to be added to `Interact` class.
