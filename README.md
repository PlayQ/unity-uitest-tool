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
	* [Class Helper](#class-helper)
	* [Helper custom attribute](#helper-custom-attribute)
* [API methods](#api-methods)
	* [Wait class](#wait-class)
	* [Check class](#check-class)
	* [Interact class](#interact-class)
	* [UITestUtils class](#uitestutils-class)
	* [Screen resolution depending tests](#screen-resolution-depending-tests)


About
---------------------

This `Test Tool` simplifies testing of `Unity` games and makes possible to run tests on mobile devices. It's based on [NUnit](http://nunit.org) plugin built-in in `Unity`.


### Instalation

Just copy `UITestTools` folder and past wherever you want in `Assets` folder of your project. 


### Flow Recorder

`Flow Recorder` is an Unity Editor extension, which can record user actions and generate code of test.

To use test recorder navigate to `Window => Ui test tools => Flow recorder`. Or press `Ctrl+T`.

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
Most of `Assertation methods` are working with `GameObjects` that instantiated of will be instantiated on scene. If `assertation method` works with `GameObject`, the first argument has to be `string`, which is path to `GameObject` in `hierarchy`.  If you want to use `assertation method` in `Flow Recorder` you have to pass a corresponding flag to custom method attribute, [described](#helper-custom-attribute) below.

If you want `assertation method` compare some `GameObject` parameters with specific values, you can pass these values as additional arguments to `assertation method`.  If you want to use `assertation method` in `Flow Recorder` you have to describe default values for these arguments in [class helper](#class-helper) described below.

`Assertation method` returns either `void` or `IEnumerable`. In first case method is executed immediately. In the second case method is execution during specific amount of frames, for example `asssertation method` would wait for some condition during timeout (usually given as argument) and fails if condition doesn't fulfill in given time. If `assertation method` passes successfully, next recorded `assertation method` is started.

`Assertation method` could be show in `Flow Recorder` if it implemented in specific way. In such case you can find your `assertation method` in `Recorder Window` drop down menu of recorded user action.

### Class Helper
If you want to use `Assertation method` in `Flow Recorder` you have to implement static class helper and custom attribute to your method. Class helper could contain next methods:

```c#
public static bool IsAvailable(GameObject go)
{
	//checks if GameObject has Text component. If it has, than assertation could be applied to this GameObject
	return go.GetComponent<Text>() != null;
}
```

`GameObject go` - instance of selected `GameObject` would be passed to this method during recording. This method check, if `assertation` could be applied to given `GameObject`. If you implement new `assertation method`, and it could be applied only to specific `GameObjects` then you have to implement it. If new `assertation` could be applied to any `GameObject`, or `assertation` doesn’t need `GameObject` at all - you can skip method implementation.

```c#
 public static List<object> GetDefautParams(GameObject go)
 {
 	List<object> result = new List<object>();
	var labelText = go.GetComponent<InputField>(); 
	result.Add(labelText.text); //takes current values from given GameObject
 	return result; 
 }
 ```
 
`GameObject go` - instance of selected `GameObject` would be passed to this method during recording. You can fetch necessary parameters from `GameObject` and add it to list of values. This method returns list of default values for arguments, that have to be passed to assertation method. Order of elements in list is the same as order of arguments in assertation method. Type of specific element of list is the same as type of appropriate argument of assertation method.  If assertation method’s first argument is path to gameobject - you must not add it to list, just skip it. If you assertation method doesn’t require additional params - return empty list.

```c#
public static Camera Camera
{
	get
    {
        // return custom game Camera to raycast to specific GameObject
    	return Map.main != null ? Map.main.mapCamera : null; 
	}
}
```

When user click on item(button, switch etc) flow recorder takes click coords and uses `UnityEngine.EventSystems.EventSystem` class to raycast by these coords to find `GameObject` under click. This works only with unity ui objects. But sometimes user clicks on non ui gameobjects. For such case you have to define property that return specific camera to raycast. If your `assertation method` are developed for unity ui, just skip implementation of this method.

#### Example of implemented Assertation Method
```c#
public static class Check
{
	//custom attribute ShowInEditor links method TextEquals to class helper CheckTextEquals.
	[ShowInEditor(typeof(CheckTextEquals), "Text equals")] 
	public static void TextEquals(string path, string expectedText) //Assertation method
	{
		CheckextEquals.CheckEquals(path, expectedText);
	}

	//class helper
	private static class CheckInputTextEquals
	{
		// Method that finds gameObject by its path
		// and checks that it has
		public static void CheckEquals(string path, string expectedText)
		{
			var go = UITestUtils.FindAnyGameObject(path); // UITestUtils is util class with helpfull methods
			Assert.AreEqual(null, go, "CheckEquals: Object " + path + " is not exist"); 
			TextEquals(go, expectedText);
		}


		public static bool IsAvailable(GameObject go)
		{
			return go.GetComponent<Text>() != null; //GameObject has to have Text component on it
		}
		
		// out assertation method requires text as an additional param
		//that's why GetDefautParams extracts this value from GameObject
		// and returns it
		public static List<object> GetDefautParams(GameObject go)
		{
			List<object> result = new List<object>();
			var labelText = go.GetComponent<Text>();
			result.Add(labelText.text);
			return result;              
		}
	}
}
```



### Helper custom attribute

To link your `Class Helper` to `assertation method` use custom attribute `[ShowInEditorAttribute]` on your method.

`ShowInEditorAttribute` first constructor  `params`:
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