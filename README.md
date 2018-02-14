New DocumentUI Test Tool
====================================
Content
----
* [About](#about)
	* [Instalation](#instalation)	  
	* [Flow Recorder](#flow-recorder)	
	* [How Flow Recorder works](#how-flow-recorder-works)	 
	* [Recording a new test](#recording-a-new-test)	 
	* [Custom Test Runner](#custom-test-runner)	
	* [Helper Window](#helper-window)
* [Extending Test Tool](#extending-test-tool)
	* [Implementing Assertation method](#implementing-assertation-method)
	* [Class Helper](#class-helper)
	* [Helper custom attribute](#helper-custom-attribute)
* [API methods](#api-methods)
	* [Wait class](#wait-class)
	* [Check class](#check-class)


About
---
This `Test Tool` simplifies testing of `Unity` games and makes possible to run tests on mobile devices. It's based on [NUnit](http://nunit.org) plugin built-in in `Unity`.

##### Instalation

Just copy folder `UITestTools` and past wherever you want in `Assets` folder of your project. 


##### Flow Recorder

`Flow Recorder` is an Unity Editor extension, which can record user actions and generate test.

To use test recorder navigate to `Window => Ui test tools => Flow recorder`. Or press `Ctrl+T`.

<img src="documentation/images/recorder-interface.png" width="600">

`Start Record` button - enables recording mode, user's click would be recorded as actions, you can select necessary assertation for this actions.

`Stop Record` - disables recording mode. All recorded actions are keep safe.

`Check` - enables mode, in which clicks on ui buttons don't trigger them. It useful to record several assertation for interacting ui.

`Pause mode` - set time scale to 0 when enabled, useful when it needs to emulate game pause and record things.

`Clean` - deletes all recorded actions.

`+` button - added new "empty" user action. You can select only assertations, which don't require gameobject to proceed to such action.

`Generate Code and copy` - generate code from recorded user actions.

<img src="documentation/images/action-item.png" width="600">

Recorded users actions are visually displayed as a list, each item of list contains next information:
1. `Assertation Type` - there is a drop down menu with a list of assertation types. Each assertation has its own list of require arguments.
2. `Description` - add description as a commentary above generated `assertation method`. 
3. `Path` - path in hierarchy to gameobject user interacts with.
4. `Delay`, `timeOut` - arguments list specific to selected assertation, values of these arguments could be edited by user. Arguments could be type of `enum`, `int`, `float`, `bool`, `string`.
5. `►` - this button applies assertation in runtime.
6. `x` - button removes action from list.

User can reorder already recorded actions by drag and drop.

##### How Flow Recorder works
When user clicks on `GameObject` during in `Unity` playmode `Flow Recorder` takes click coords and uses `UnityEngine.EventSystems.EventSystem` class to raycast by these coords to find `GameObject` under click. Then `Flow Recorder` looks through list of all existing `assertations` and checks which `assertation` could be applied to `GameObject`. For example, if user clicks on text - all `assertations` that compares text labels would be available. Common `assertations`, like `Check Object Enabled` or `check object exists` are available for any `GameObject`. By default `Flow Recorder` supports only `Unity UI`, if you want support `GameObjects` that doesn't contain any UI components you have to implement custom `assertation method`. Please read below how to [extend](#extending-test-tool) `UI Test Tool`.

##### Recording a new test
Launch game in editor, press `start record` button, perform any actions you need in game, then press `stop record` or exit play mode. Press `Generate code and copy` - to obtain test code. Then you can past generated code to your own test class.


##### Custom Test Runner
Is required to perform tests on mobile devices, because build-in unity test runner can perform tests only in editor. To open  `Test Runner Window` navigate to `Window => Play Mode Test Runner`.

<img src="documentation/images/play-mode-test-runner" width="600">

`Custom Test Runner` searches for all methods with custom attribute `[UnityTest]` and shows them like a folding list.
You can select test that you want to run and press `Run` button. Also, set of selected tests is saved to `UITestTools/PlayModeTestRunner/Resources/SelectedTests.asset` scriptable object. When you run tests on mobile device `Play Mode Test Runner` loads `SelectedTests` and runs only selected tests. If `SelectedTests` is not exists - all tests are executed.


##### Helper Window
This section will be updated soon.

Extending Test Tool
-----------
`Test Tool` common features are split in 3 static partial classes: `Check`, `Wait` and `Interact`. Any of these classes contains list of methods which accordingly check state of given object, wait for resolving of specific condition and set state to given object.


##### Implementing Assertation method
Most of `Assertation methods` are working with `GameObjects` that instantiated of will be instantiated on scene. If `assertation method` works with `GameObject`, the first argument has to be `string`, which is path to `GameObject` in `hierarchy`.  If you want to use `assertation method` in `Flow Recorder` you have to pass a corresponding flag to custom method attribute, [described](#helper-custom-attribute) below.

If you want `assertation method` compare some `GameObject` parameters with specific values, you can pass these values as additional arguments to `assertation method`.  If you want to use `assertation method` in `Flow Recorder` you have to describe default values for these arguments in [class helper](#class-helper) described below.

`Assertation method` returns either `void` or `IEnumerable`. In first case method is executed immediately. In the second case method is execution during specific amount of frames, for example `asssertation method` would wait for some condition during timeout (usually given as argument) and fails if condition doesn't fulfill in given time. If `assertation method` passes successfully, next recorded `assertation method` is started.

`Assertation method` could be show in `Flow Recorder` if it implemented in specific way. In such case you can find your `assertation method` in `Recorder Window` drop down menu of recorded user action.

##### Class Helper
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

###### Example of implemented Assertation Method
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



##### Helper custom attribute

To link your `Class Helper` to `assertation method` use custom attribute `[ShowInEditorAttribute]` on your method.

`ShowInEditorAttribute` first constructor  `params`:
* `Type classType` - type of class helper. Optional, skip it if you just want to add method to flow recorder, without any specific parameters, except probably path.
* `string description` - human friendly description of what this assertation for, this label is shown in recorder in dropdown list of assertations.
* `bool pathIsPresent` = true - optional, if first param of assertation method is path to `GameObject`, set it to true. If assertation method doesn’t use any `GameObject` - set false.
* `bool isDefault` = false - optional, very often several assertation could be applied to selected `GameObject`.  If isDefault is true, recorder tries to show this assertation as a first one.

To add more features, which are not common and related only to your project, extend these classes by creating new partial classes and add new `assertation methods`. If you want to add `assertation method` to `Flow Recorder` - add class helper near the `assertation method`. Try to separate methods by its assignment: if method checks some condition - it has to be added to `Check` class, if methods awaits for some action - it has to be added to `Wait` class, and if method changes some values in game - it has to be added to `Interact` class.


API methods
--------

##### Wait class
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


##### Check class

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
Checks average fps since moment user last time called [ResetFPS()](#resets fps) method or since game started.


```c#
public static void CheckMinFPS(float targetFPS)
```
Checks minimal fps since moment user last time called [ResetFPS()](#resets fps) method or since game started.



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




















