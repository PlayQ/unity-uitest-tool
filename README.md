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
	* [Helper Window](#helper-window)
* [Extending Test Tool](#extending-test-tool)
	* [Implementing Assertation method](#implementing-assertation-method)
	* [ShowInEditor custom attribute](#showineditor-custom-attribute)
* [API methods](#api-methods)
	* [Wait class](#wait-class)
	* [Check class](#check-class)
	* [Interact class](#interact-class)
	* [UITestUtils class](#uitestutils-class)
	* [Screen resolution depending tests](#screen-resolution-depending-tests)


About
---------------------

This `Test Tool` simplifies testing of `Unity` games and makes possible to run tests on mobile devices. It's based on methods, atributes and logs, simular to [NUnit](http://nunit.org).


### Instalation

Just copy `UITestTools` folder and paste wherever you want in `Assets` folder of your project.


### Flow Recorder

`Flow Recorder` is an Unity Editor extension, which can record user actions and generate code of test from it.

To use `Test Recorder` navigate to `Window => Ui test tools => Flow recorder`, or press `Ctrl+T` for Windows and `Cmd+T` for Mac.

<img src="documentation/images/recorder-interface.png" width="600">

* `Start Record` button - enables recording mode, your clicks would be recorded as actions and added to existing action list. You can select necessary assertation and edit params for each actions.
* `Stop Record` - disables recording mode. All recorded actions are keep safe.
* `Check` - enables/disables mode, in which clicks on ui buttons don't trigger them. Window will be colored green when check mode is enabled. 
* `Pause mode` - set time scale to 0 when enabled, useful when it needs to emulate game pause and record things.
* `Clean` - deletes all recorded actions.
* `+` button - added new "empty" user action. You can select only assertations, which don't require gameobject to proceed to such action.
* `Generate Code and Copy` - generate code for test from recorded user actions.

Recorded users actions are visually displayed as a list, each item of list contains next information:

<img src="documentation/images/action-item.png" width="600">

* `Assertation Type` - there is a drop down menu with a list of assertation types. Each assertation has its own list of require arguments.
* `Description` - add description as a commentary above generated `assertation method`. 
* `Path` - path in hierarchy to gameobject user interacts with.
* `Delay`, `timeOut` - arguments list specific to selected assertation, values of these arguments could be edited by user. Arguments could be type of `enum`, `int`, `float`, `bool`, `string`.
* `►` - this button applies assertation in runtime.
* `x` - button removes action from list.

You can reorder already recorded actions by drag and drop.


### How Flow Recorder works

When you clicks on `GameObject` during `Unity` playmode `Flow Recorder` takes click coords and uses `UnityEngine.EventSystems.EventSystem` class to raycast by these coords to find `GameObject` under click. Then `Flow Recorder` looks through list of all existing `assertations` and checks which `assertation` could be applied to `GameObject`. For example, if you clicks on text - all `assertations` that compares text labels would be available. Common `assertations`, like `Is Enable` or `Is Exist` are available for any `GameObject`. By default `Flow Recorder` supports only `Unity UI`, if you want support `GameObjects` that doesn't contain any UI components or use custom `assertations` you have to implement custom `assertation method`. Please read below how to [extend](#extending-test-tool) `UI Test Tool`.


### Recording a new test

Launch the game in editor, press `start record` button, perform any actions you need in game, then press `stop record` or exit play mode. Press `Generate code and copy` - to obtain code for test. Then you can paste generated code to your own test class.


### Test Runner

We use custom Test Runner to make possible to:
* Run test on the real devices
* Run custom set of tests
* Run only tests with `[SmokeTest]` attribute  
* Run only tests whos target resolution matches mobile device resolution.
* Change editor Game window resolution to match test target resolution.
It is needed to perform tests on mobile devices, because build-in unity test runner can perform tests only in editor. To open `Test Runner Window` navigate to `Window => Play Mode Test Runner`.

<img src="documentation/images/play-mode-testrunner.png" width="600">

`Custom Test Runner` searches for all methods with custom attribute `[UnityTest]` and shows them like a folding list.
You can select test that you want to run and press `Run` button. Also, set of selected tests is saved to `UITestTools/PlayModeTestRunner/Resources/SelectedTests.asset` scriptable object. When you run tests on mobile device `Play Mode Test Runner` loads `SelectedTests` and runs only selected tests. If `SelectedTests` is not exists - all tests are executed.

##### Screen resolution depending tests
Sometimes your tests may succeed or fail depending on different screen resolution. For exmple test checks if gameobject located under certain screen pixels. To ensure that test is run only with proper resolution you have to add `[TargetResolutionAttribute]` above test method declaration and set target resolution. For example:
```c#
[TargetResolutionAttribute(1920, 1080)]
[UnityTest]
public IEnumerator SomeTest()
{....}
```

If test is run on mobile device, custom `Test Runner` will ignore with target resolution different from device resolution. If tests is run in editor custom `Test Runner` will chage Unity Game Window resolution to target resolution.

To run test in editor mode via console use followin command:

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



### Helper Window

`Test Helper` is an `Unity Editor Extension`, that shows list of possible assertations for selected `GameObject` in `Hierarchy`. If you use `Test Helper` in play mode, you can also obtain list of playing sound clips.

<img src="documentation/images/test-helper.png" width="600">

`Copy` - copyes generated code of assertation.
`Find playing sounds` - searches for playing sound clips.
`Select` - only for sound, focuses on `GameObject` in hierarchy which has `Audio Source` component and it's `Sound Clip` is playing.


Extending Test Tool
-----------

`Test Tool` common features are split in 3 static partial classes: `Check`, `Wait` and `Interact`. Any of these classes contains list of methods which accordingly check state of given object, wait for resolving of specific condition and set state to given object.


### Implementing Assertation method

Let's create a simple assertation method, which takes GameObject and checks whether it's present on scene.

``` c#
public static void IsExist(string path) //path is a full path of GameObject in hierarchy on scene
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
Now we can use this method in out test method like this:

``` c#
[UnityTest]
public IEnumerator SomeIntegrationTest()
{
	// some code 
	
	Check.IsExist("UIRoot/Users_Panel/UserPic")
}
```

But it's too much of manual work to find necessary GameObject in editor's scene, copy it's full path and paste into test code. [Flow Recorder](#flow-recorder) can do this for you. Let's modify our assertation method, so it will be awailable in Flow Recorder.

``` c#
[ShowInEditor("Is Exist")]
public static void IsExist(string path)
{
    // assertation code
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


API methods
--------

### Wait class
```c#
public static IEnumerator ObjectInstantiated(string path, float waitTimeout = 2f)
```
Awaits for `GameObject` by `path` in hierarchy being present on scene. Method fails by timeout.

```c#
public static IEnumerator ObjectEnabledInstantiatedAndDelay(string path, float delay, float waitTimeout = 2f)
```

Awaits for `GameObject` by `path` in hierarchy being present on scene and active in hierarchy. Then waits during given amount of time by `delay` argument and returns after that. Method fails by timeout.

```c#
public static IEnumerator ObjectEnabledInstantiatedAndDelayIfPossible(string path, float delay, float waitTimeout = 2f)
````

Awaits for `GameObject` by `path` in hierarchy being present on scene and active in hierarchy. Then waits during given amount of time by `delay` argument and returns after that. Method doesn't fail by timeout.

```c#
public static IEnumerator Frame(int count = 1)
```
Waits for given amount of frames, then returns.

```c#
public static IEnumerator WaitFor(Func<bool> condition, float timeout, string testInfo, bool dontFail = false)
```

Waits until given delegate returns true. Fails by timeout. 

`Func<bool> condition` - delegate that return true, if its condition is successfuly fulfilled.

`string testInfo` - label would be passed to logs if method fails.

`bool dontFail` - method doesn’t fail by time out if this flag is true


```c#
public static IEnumerator ObjectInstantiated<T>(float waitTimeout = 2f) where T : Component
```
Waits until `GameObject` with component `T` is present on scene. Fails by timeout.

```c#
public static IEnumerator ObjectInstantiated<T>(string path, float waitTimeout = 2f) where T : Component
```
Waits until `GameObject` with component `T` and given path in hierarchy is present on scene. Fails by timeout.

```c#
public static IEnumerator ObjectDestroy<T>(float waitTimeout = 2f) where T : Component
```
Waits until `GameObject` with component `T` is not present in scene. Fails by timeout.

```c#
public static IEnumerator ObjectDestroy(string path, float waitTimeout = 2f)
```
Waits until `GameObject` by given path is not present in scene. Fails by timeout

```c#
public static IEnumerator ObjectDestroy(GameObject gameObject, float waitTimeout = 2f)
```
Waits until given `GameObject` is not present in scene. Fails by timeout.

```c#
public static IEnumerator Condition(Func<bool> func, float waitTimeout = 2f)
```
Waits until `delegate` returns true. Fails by timeout.

```c#
public static IEnumerator ButtonAccessible(GameObject button, float waitTimeout = 2f)
```
Waits until given `GameObject` has component `UnityEngine.UI.Button` attached.

```c#
public static IEnumerator ObjectEnabled(string path, float waitTimeout = 2f)
```
Waits until `GameObject` by given path is present on scene and active in hierarchy. Fails by timeout.

```c#
public static IEnumerator ObjectEnabled<T>(float waitTimeout = 2f) where T : Component
```
Waits until `GameObject` with component `T` is present on scene. Fails by timeout.

```c#
public static IEnumerator ObjectDisabled<T>(float waitTimeout = 2f) where T : Component
```
Waits until `GameObject` with component `T` is present on scene and disabled in hierarchy. Fails by timeout.

```c#
public static IEnumerator ObjectDisabled(string path, float waitTimeout = 2f)
```
Waits until `GameObject` by given `path` is present on scene and disabled in hierarchy. Fails by timeout.

```c#
public static IEnumerator Seconds(float seconds)
```
Waits for seconds.

```c#
public static IEnumerator SceneLoaded(string sceneName, float waitTimeout = 2f)
```
Waits until scene with given name is loaded. Fails by timout.


### Check class

```c#
public static void TextEquals(string path, string expectedText)
```
Checks `GameObject` by given path has `Text` component and it's variable `text` is equals to expected text.

```c#
public static void TextNotEquals(string path, string expectedText)
```
Checks `GameObject` by given path has `Text` component and it's variable `text` is not equals to expected text.


```c#
public static void TextEquals(GameObject go, string expectedText)
```
Checks `GameObject` has `Text` component and it's variable `text` is equals to expected text.


```c#
public static void TextNotEquals(GameObject go, string expectedText)
```
Checks `GameObject` has `Text` component and it's variable `text` is not equals to expected text.


```c#
public static void InputEquals(GameObject go, string expectedText)
```
Checks `GameObject` has `InputField` component and it's variable `text` is equals to expected text.


```c#
public static void InputNotEquals(GameObject go, string expectedText)
```
Checks `GameObject` has `InputField` component and it's variable `text` is not equals to expected text.


```c#
public static void InputEquals(string path, string expectedText)
```
Checks `GameObject` by given path has `InputField` component and it's variable `text` is equals to expected text.


```c#
public static void InputNotEquals(string path, string expectedText)
```
Checks `GameObject` by given path has `InputField` component and it's variable `text` is not equals to expected text.


```c#
public static void IsEnable(string path)
```
Checks `Gameobject` by path is present on scene and active in hierarchy.


```c#
public static void IsEnable<T>(string path) where T : Component
```
Checks `Gameobject` by path is present on scene, contains component `T` and active in hierarchy.


```c#
public static void IsEnable<T>() where T : Component
```
Searches for `Gameobject` with component `T` and checks it is present on scene and active in hierarchy.


```c#
public static void IsEnable(GameObject go)
```
Checks `GameObject` is active in hierarchy.


```c#
public static void CheckAverageFPS(float targetFPS)
```
Checks average fps since moment user last time called [ResetFPS()](#reset-fps) method or since game started.


```c#
public static void CheckMinFPS(float targetFPS)
```
Checks minimal fps since moment user last time called [ResetFPS()](#reset-fps) method or since game started.



```c#
public static void IsDisable(string path)
```
Checks `Gameobject` by path is present on scene and not active in hierarchy.


```c#
public static void IsDisable<T>(string path) where T : Component
```
Checks `Gameobject` by path is present on scene, contains component `T` and not active in hierarchy.



```c#
public static void IsDisable<T>() where T : Component
```
Searches for `Gameobject` with component `T` and checks it is present on scene and not active in hierarchy.


```c#
public static void IsDisable(GameObject go)
```
Checks `GameObject` is not active in hierarchy.


```c#
public static void CheckEnabled(string path, bool state = true)
```
Checks `Gameobject` by path is present on scene and it's active in hierarchy flag is equals `state` variable.


```c#
public static GameObject IsExist(string path)
```
Checks `GameObject` by give path is present on scene.


```c#
public static GameObject IsExist<T>(string path) where T : Component
```
Checks `GameObject` by give path is present on scene and contains component `T`.


```c#
public static GameObject IsExist<T>() where T : Component
```
Searches for `GameObject` with component `T` on scene.


```c#
public static void IsNotExist(string path)
```
Checks `GameObject` by give path is not present on scene.


```c#
public static void IsNotExist<T>(string path) where T : Component
```
Checks `GameObject` by give path and component `T` is not present on scene.


```c#
public static void IsNotExist<T>() where T : Component
```
Searches for any `GameObject` on scene with component `T`. Fails if found one.


```c#
public static void CheckToggle(GameObject go, bool expectedIsOn)
```
Checks thart `GameObject` has `Toggle` component and it's `isOn` value is equals to expected.


```c#
public static void CheckToggle(string path, bool state)
```
Checks thart `GameObject` by given path has `Toggle` component and it's `isOn` value is equals to expected.


```c#
public static IEnumerator AnimatorStateStarted(string path, string stateName, float timeout)
```
Seraches for `GameObject` by given path with `Animator` component. Waits during given timeOut until animation state with given name becames active.


### Interact class

```c#
public static IEnumerator LoadScene(string sceneName, float waitTimeout = 2f)
```
Loads scene by given name. Fails it loading lasts longer than timeout.


```c#
public static void MakeScreenShot(string name)
```
Makes screenshot. Saves it to persistant data path.


```c#
public static IEnumerator MakeScreenshotAndCompare(string screenShotName, string referenceName, Vector2 size, float treshold = 1)
```
Saves screenshot by `screenShotName` then loads screenshot by `referenceName`, and compares it pixel to pixel. 

`Vector2 size` - size of saved screenshot. This parameter will be removed later.

`float treshold` - percentage of missmatches during pisex comparison. If for example this value is set to `1` - every pixel must match to referense, otherwise method fails. If values is set to 0.5f - at least 50% of all pixels must match. 


```c#
public static void ResetFPS()
```
#### Reset FPS
FPS counter counts average fps, minimum and maximum FPS since the moment this method is called.


```c#
public static void SetTimescale(float scale)
```
Sets timescale.


```c#
public static void Click(string path)
```
Emulates click on `Unity UI` element by path.


```c#
public static void Click(GameObject go)
```
Emulates click on `Unity UI` GameObject.


```c#
public static IEnumerator WaitDelayAndClick(string path, float delay, float timeOut)
```
Waits until `GameObject` by path is present on scene, active in hierarchy, the delay durin `delay`, then emulate click. Fails on timeout.


```c#
public static IEnumerator WaitDelayAndClickIfPossible(string path, float delay, float timeOut)
```
Waits until `GameObject` by path is present on scene, active in hierarchy, the delay durin `delay`, then emulate click. Doesn't fail on timeout.


```c#
public static void ClickPixels(int x, int y)
```
Uses `UnityEngine.EventSystems.EventSystem` class to raycast by these coords to find `GameObject` and perform click on it.


```c#
public static void ClickPercents(float x, float y)
```
First find pixel coordinates by screen percents. Then uses `UnityEngine.EventSystems.EventSystem` class to raycast by these coords to find `GameObject` and perform click on it.


```c#
public static void SetText(GameObject go, string text)
```
Checks if `GameObject` has `Text` component, fail if it has not. If not fail, then set `text` variable of `Text` to given value.


```c#
public static void SetText(string path, string text)
```
Finds `GameObject` by path, checks if `GameObject` has `Text` component, fail if it has not. If not fail, then set `text` variable of `Text` to given value.


```c#
public static void AppendText(GameObject go, string text)
```
Checks if `GameObject` has `InputField` component. Fails if not. Then appends given `text` to `text` variable of `InputField`


```c#
public static void AppendText(string path, string text)
```
Appends text to `GameObject` by path with `InputField` component.


### UITestUtils class
This class contains helper methods to work with `GameObjects` 

```c#
public static string LogHierarchy()
```
Prins in console log with a list of path to all GameObject on scene.


```c#
public static GameObject FindObjectByPixels(float x, float y, HashSet<string> ignoreNames = null)
```
Uses `UnityEngine.EventSystems.EventSystem` class to raycast by these coords to find `GameObject` under click. `ignoreNames` - set of names of object, that are ignored.


```c#
public static GameObject FindObjectByPercents(float x, float y)
```
Get pixels coords from percent coords. Then Uses `UnityEngine.EventSystems.EventSystem` class to raycast by these coords to find `GameObject` under click.


```c#
public static GameObject FindGameObjectWithComponentInParents<TComponent>(GameObject go)
```
Checks if given `GameObject` has `TComponent` component on it. If not - checks it's parent recursively. Retusn `GameObject` with give component, or null.


```c#
public static TComponent FindComponentInParents<TComponent>(GameObject go)
```
Checks if given `GameObject` has `TComponent` component on it. If not - checks it's parent recursively. Retusn `TComponent` instance, or null.

```c#
public static string GetGameObjectFullPath(GameObject gameObject)
```
Returns full path in hierarchy of given `GameObject`.


```c#
public static GameObject FindAnyGameObject<T>(String path) where T : Component
```
Searches for `GameObject` that has component of `T` and by path in hierarchy. Searches for active and non-active `GameObjects`.


```c#
public static GameObject FindAnyGameObject(string path)
```
Searches for `GameObject` by path in hierarchy. Searches for active and non-active `GameObjects`.


```c#
public static T FindAnyGameObject<T>() where T : Component
```
Searches for `GameObject` that has component of `T`. Searches for active and non-active `GameObjects`.

```c#
public static Vector2 CenterPointOfObject(RectTransform transform)
```
Return center point of given `RectTransform`.


```c#
public static Vector2[] ScreenVerticesOfObject(RectTransform transform)
```
Returns array of coords of screen rectangle of given RectTransform.
