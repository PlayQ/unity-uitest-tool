# API methods
* [Check](#check)
  * [AnimatorStateStarted](#animatorstatestarted)
  * [CheckAverageFPS](#checkaveragefps)
  * [CheckEnabled](#checkenabled)
  * [CheckMinFPS](#checkminfps)
  * [CheckToggle](#checktoggle)
  * [DoesNotExist](#doesnotexist)
  * [DoesNotExistOrDisabled](#doesnotexistordisabled)
  * [InputEquals](#inputequals)
  * [InputNotEquals](#inputnotequals)
  * [IsDisable](#isdisable)
  * [IsEnable](#isenable)
  * [IsExist](#isexist)
  * [IsNotExist](#isnotexist)
  * [TextEquals](#textequals)
  * [TextNotEquals](#textnotequals)
* [Interact](#interact)
  * [AppendText](#appendtext)
  * [ClearFPSMetrics](#clearfpsmetrics)
  * [Click](#click)
  * [ClickPercents](#clickpercents)
  * [ClickPixels](#clickpixels)
  * [DragPercents](#dragpercents)
  * [DragPixels](#dragpixels)
  * [MakeScreenShot](#makescreenshot)
  * [ObsoleteWaitClickAndDelay](#obsoletewaitclickanddelay)
  * [ObsoleteWaitClickAndDelayIfPossible](#obsoletewaitclickanddelayifpossible)
  * [ResetFPS](#resetfps)
  * [SaveFPS](#savefps)
  * [SetText](#settext)
  * [SetTimescale](#settimescale)
  * [WaitDelayAndClick](#waitdelayandclick)
* [UITestUtils](#uitestutils)
  * [CenterPointOfObject](#centerpointofobject)
  * [DecodeString](#decodestring)
  * [EncodeString](#encodestring)
  * [FindAnyGameObject](#findanygameobject)
  * [FindComponentInParents](#findcomponentinparents)
  * [FindCurrentEventSystem](#findcurrenteventsystem)
  * [FindEnabledGameObjectByPath](#findenabledgameobjectbypath)
  * [FindGameObjectWithComponentInParents](#findgameobjectwithcomponentinparents)
  * [FindObjectByPercents](#findobjectbypercents)
  * [FindObjectByPixels](#findobjectbypixels)
  * [GetGameObjectFullPath](#getgameobjectfullpath)
  * [GetScrollElement](#getscrollelement)
  * [HeightPercentsToPixels](#heightpercentstopixels)
  * [LoadSceneForSetUp](#loadsceneforsetup)
  * [LogHierarchy](#loghierarchy)
  * [PercentsToPixels](#percentstopixels)
  * [ScreenVerticesOfObject](#screenverticesofobject)
  * [WidthPercentsToPixels](#widthpercentstopixels)
* [Wait](#wait)
  * [ButtonAccessible](#buttonaccessible)
  * [Frame](#frame)
  * [ObjectDestroy](#objectdestroy)
  * [ObjectDisabled](#objectdisabled)
  * [ObjectEnabled](#objectenabled)
  * [ObjectEnabledInstantiatedAndDelay](#objectenabledinstantiatedanddelay)
  * [ObjectInstantiated](#objectinstantiated)
  * [SceneLeaded](#sceneleaded)
  * [Seconds](#seconds)
  * [WaitFor](#waitfor) 

## Check 

#### AnimatorStateStarted

 Seraches for `GameObject` by given path with `Animator` component. Waits during given `timeOut` until animation state with given name becames active. 

|Name | Description |
|-----|------|
|path: |Path to `GameObject` in hierarchy|
|stateName: |`Animator` state name|
|timeout: |Timeout|
Returns: 



---

#### CheckAverageFPS

 Checks average fps since moment user last time called `Interact.ResetFPS()` method or since game started. If average fps less than `targetFPS` test will faled. 

|Name | Description |
|-----|------|
|targetFPS: |Minimum allowable value of average fps|


---

#### CheckEnabled

 Checks `Gameobject` by path is present on scene and it's active in hierarchy flag is equals state variable. 

|Name | Description |
|-----|------|
|path: |Path to `GameObject` in hierarchy|
|state: |Enable state|


---

#### CheckMinFPS

 Checks minimum fps since moment user last time called `Interact.ResetFPS()` method or since game started. If minimum fps less than `targetFPS` test will faled. 

|Name | Description |
|-----|------|
|targetFPS: |Minimum allowable value of minimum fps|


---

#### CheckToggle

 Checks thart `GameObject` by given path has `Toggle` component and it's `isOn` value is equals to expected. 

|Name | Description |
|-----|------|
|path: |Path to `GameObject` in hierarchy|
|state: |Toggle state|


---

#### CheckToggle

 Checks thart `GameObject` has `Toggle` component and it's `isOn` value is equals to expected. 

|Name | Description |
|-----|------|
|go: |`GameObject` with `Toggle` component|
|expectedIsOn: ||


---

#### DoesNotExist

 Searches for any `GameObject` on scene with component `T`. Fails if found one. 



---

#### DoesNotExist

 Checks `GameObject` by give path and component `T` is not present on scene. 

|Name | Description |
|-----|------|
|path: |Path to `GameObject` in hierarchy|


---

#### DoesNotExistOrDisabled

 Checks `GameObject` by give path is not present on scene or it not active. 

|Name | Description |
|-----|------|
|path: |Path to `GameObject` in hierarchy|
|timeout: |Timeout|
|ignoreTimeScale: |Should we ignore time scale or not|
Returns: 



---

#### InputEquals

 Checks `GameObject` by given path has `InputField` component and it's variable text is equals to expected text. 

|Name | Description |
|-----|------|
|path: |Path to `GameObject` in hierarchy|
|expectedText: |Expected text|


---

#### InputEquals

 Checks `GameObject` has `InputField` component and it's variable text is equals to expected text. 

|Name | Description |
|-----|------|
|go: |`GameObject` with `InputField` component|
|expectedText: |Expected text|


---

#### InputNotEquals

 Checks `GameObject` by given path has `InputField` component and it's variable text is not equals to expected text. 

|Name | Description |
|-----|------|
|path: |Path to `GameObject` in hierarchy|
|expectedText: |Expected text|


---

#### InputNotEquals

 Checks `GameObject` has `InputField` component and it's variable text is not equals to expected text. 

|Name | Description |
|-----|------|
|go: |`GameObject` with `InputField` component|
|expectedText: |Expected text|


---

#### IsDisable

 Checks `GameObject` is not active in hierarchy. 

|Name | Description |
|-----|------|
|go: |`GameObject`|


---

#### IsDisable

 Searches for `Gameobject` with component `T` and checks it is present on scene and not active in hierarchy. 



---

#### IsDisable

 Checks `GameObject` by path is present on scene, contains component `T` and not active in hierarchy. 

|Name | Description |
|-----|------|
|path: |Path to `GameObject` in hierarchy|


---

#### IsEnable

 Checks `GameObject` is active in hierarchy. 

|Name | Description |
|-----|------|
|go: |`GameObject`|


---

#### IsEnable

 Searches for `Gameobject` with component `T` and checks it is present on scene and active in hierarchy. 



---

#### IsEnable

 Checks `Gameobject` by path is present on scene, contains component `T` and active in hierarchy. 

|Name | Description |
|-----|------|
|path: |Path to `GameObject` in hierarchy|


---

#### IsExist

 Checks `GameObject` by give path is present on scene. 

|Name | Description |
|-----|------|
|path: |Path to `GameObject` in hierarchy|
Returns: `GameObject`



---

#### IsExist

 Searches for `GameObject` with component `T` on scene. 

Returns: 



---

#### IsExist

 Checks `GameObject` by give path is present on scene and contains component `T`. 

|Name | Description |
|-----|------|
|path: |Path to `GameObject` in hierarchy|
Returns: 



---

#### IsNotExist

 Checks `GameObject` by give path and component `T` is not present on scene. 

|Name | Description |
|-----|------|
|path: |Path to `GameObject` in hierarchy|


---

#### TextEquals

 Checks `GameObject` by given path has `Text` component and it's variable text is equals to expected text. 

|Name | Description |
|-----|------|
|path: |Path to `GameObject` in hierarchy|
|expectedText: |Expected text|
\ 

---

#### TextEquals

 Checks `GameObject` has `Text` component and it's variable text is equals to expected text. 

|Name | Description |
|-----|------|
|go: |`GameObject` with `Text` component|
|expectedText: |Expected text|


---

#### TextNotEquals

 Checks `GameObject` by given path has `Text` component and it's variable text is not equals to expected text. 

|Name | Description |
|-----|------|
|path: |Path to `GameObject` in hierarchy|
|expectedText: |Expected text|


---

#### TextNotEquals

 Checks `GameObject` has `Text` component and it's variable text is not equals to expected text. 

|Name | Description |
|-----|------|
|go: |`GameObject` with `Text` component|
|expectedText: |Expected text|


---

## Interact 

#### AppendText

 Appends text to `GameObject` by path with `InputField` component. 

|Name | Description |
|-----|------|
|path: |Path to `GameObject` in hierarchy|
|text: |`Text` to set|


---

#### AppendText

 Checks if `GameObject` has `InputField` component. Fails if not. Then appends given text to text variable of `InputField` 

|Name | Description |
|-----|------|
|go: |`GameObject` with `Text` component|
|text: |`Text` to set|


---

#### ClearFPSMetrics

 Store FPS data to the disc. 



---

#### Click

 Emulates click on `Unity UI` element by path. 

|Name | Description |
|-----|------|
|path: |Path to GameObject in hierarchy|


---

#### Click

 Emulates click on `Unity UI GameObject`. 

|Name | Description |
|-----|------|
|go: |GameObject to click|


---

#### ClickPercents

 Find pixel coordinates by screen percents and uses `UnityEngine.EventSystems.EventSystem` class to raycast by these coords to find `GameObject` and perform click on it. 

|Name | Description |
|-----|------|
|x: |X position in screen percents|
|y: |Y position in screen percents|


---

#### ClickPixels

 Uses `UnityEngine.EventSystems.EventSystem` class to raycast by these coords to find `GameObject` and perform click on it. 

|Name | Description |
|-----|------|
|x: |X position in screen pixels|
|y: |Y position in screen pixels|


---

#### DragPercents

 Perform drag on `GameObject` by path. 

|Name | Description |
|-----|------|
|path: |Path to `GameObject` in hierarchy|
|to: |Finish position in percents|
|time: |Drag Time|
Returns: 



---

#### DragPercents

 Perform drag on `GameObject`. 

|Name | Description |
|-----|------|
|go: |`GameObject` to drag|
|to: |Finish position in percents|
|time: |Drag Time|
Returns: 



---

#### DragPercents

 Uses `UnityEngine.EventSystems.EventSystem` class to raycast by these coords to find `GameObject` and perform drag on it. 

|Name | Description |
|-----|------|
|from: |Start position in percents|
|to: |Finish position in percents|
|time: |Drag Time|
Returns: 



---

#### DragPixels

 Perform drag on `GameObject` by path. 

|Name | Description |
|-----|------|
|path: |Path to GameObject in hierarchy|
|to: |Finish position in pixels|
|time: |Drag Time|
Returns: 



---

#### DragPixels

 Perform drag on `GameObject`. 

|Name | Description |
|-----|------|
|go: |`GameObject` to drag|
|from: |Start position in percents|
|to: |Finish position in percents|
|time: |Drag Time|
Returns: 



---

#### DragPixels

 Perform drag on `GameObject`. 

|Name | Description |
|-----|------|
|go: |`GameObject` to drag|
|to: |Finish position in pixels|
|time: |Drag Time|
Returns: 



---

#### DragPixels

 Uses `UnityEngine.EventSystems.EventSystem` class to raycast by these coords to find `GameObject` and perform drag on it. 

|Name | Description |
|-----|------|
|from: |Start position in pixels|
|to: |Finish position in pixels|
|time: |Drag Time|
Returns: 



---

#### MakeScreenShot

 Makes screenshot. Saves it to persistant data folder. 

|Name | Description |
|-----|------|
|name: |Name of screenshot|
Returns: 



---

#### ObsoleteWaitClickAndDelay

 Waits until `GameObject` by path is present on scene, active in hierarchy, the delay durin delay, then emulate click. Fails on timeout. 

|Name | Description |
|-----|------|
|path: |Path to GameObject in hierarchy|
|delay: |Amount of time to delay|
|timeout: |Timeout|
Returns: 



---

#### ObsoleteWaitClickAndDelayIfPossible

 Waits until `GameObject` by path is present on scene, active in hierarchy, the delay durin delay, then emulate click. Doesn't fail on timeout. 

|Name | Description |
|-----|------|
|path: |Path to GameObject in hierarchy|
|delay: |Amount of time to delay|
|timeout: |Timeout|
Returns: 



---

#### ResetFPS

 Reset FPS. FPS counter counts average fps, minimum and maximum FPS since the moment this method is called. 



---

#### SaveFPS

 Store FPS data to the disc. 

|Name | Description |
|-----|------|
|tag: |Measure discription|


---

#### SetText

 Finds `GameObject` by path, checks if `GameObject` has `Text` component, fail if it has not. If not fail, then set text variable of `Text` to given value. 

|Name | Description |
|-----|------|
|path: |Path to `GameObject` in hierarchy|
|text: |`Text` to set|


---

#### SetText

 Finds `GameObject` by path, checks if `GameObject` has `Text` component, fail if it has not. If not fail, then set text variable of `Text` to given value. 

|Name | Description |
|-----|------|
|go: |`GameObject` with `Text` component|
|text: |`Text` to set|


---

#### SetTimescale

 Sets timescale. 

|Name | Description |
|-----|------|
|scale: |New timescale|


---

#### WaitDelayAndClick

 Waits until `GameObject` by path is present on scene, active in hierarchy, the delay durin delay, then emulate click. Fails on timeout. 

|Name | Description |
|-----|------|
|path: |Path to GameObject in hierarchy|
|delay: |Amount of time to delay|
|timeout: |Timeout|
Returns: 



---

## UITestUtils 
 This class contains helper methods to work with GameObjects 


#### CenterPointOfObject

 Return center point of given `RectTransform`. 

|Name | Description |
|-----|------|
|transform: |`RectTransform` component instance|
Returns: Center point of given `RectTransform`



---

#### DecodeString

 Decode path int Test Tools format to simple path (`%/` => `/`, `%%` => `%`). 

|Name | Description |
|-----|------|
|text: |String to encode|
Returns: Encoded strings



---

#### EncodeString

 Encode path string for Test Tools format (`/` => `%/`, `%` => `%%`). 

|Name | Description |
|-----|------|
|text: |String to encode|
Returns: Encoded strings



---

#### FindAnyGameObject

 Searches for GameObject by path in hierarchy. 

|Name | Description |
|-----|------|
|path: |Path to `GameObject` in hierarchy|
Returns: active and non-active `GameObjects` or null



---

#### FindAnyGameObject

 Searches for `GameObject` that has component of `T`. 

Returns: active and non-active `GameObjects` or null



---

#### FindAnyGameObject

 Searches for GameObject that has component of T and by path in hierarchy. 

|Name | Description |
|-----|------|
|path: |Path to `GameObject` in hierarchy|
Returns: active and non-active `GameObjects` or null



---

#### FindComponentInParents

 Checks if given GameObject has `TComponent` component on it. If not - checks it's parent recursively. 

|Name | Description |
|-----|------|
|go: |Parent `GameObject`|
Returns: `TComponent` instance, or null.



---

#### FindCurrentEventSystem

 Find `EventSystem` on the scene. 

Returns: 



---

#### FindEnabledGameObjectByPath

 Find enabled `GameObject` by Path in hierarchy. 

|Name | Description |
|-----|------|
|path: |Path to `GameObject` in hierarchy|
Returns: Enabled `GameObject` or null.



---

#### FindGameObjectWithComponentInParents

 Checks if given `GameObject` has `TComponent` component on it. If not - checks it's parent recursively. 

|Name | Description |
|-----|------|
|go: |Parent `GameObject`|
Returns: `Component` or null.



---

#### FindObjectByPercents

 Get pixels coords from percent coords, then Uses `UnityEngine.EventSystems.EventSystem` class to raycast by these coords to find `GameObject` under click. 

|Name | Description |
|-----|------|
|x: |X position in percents|
|y: |Y position in percents|
Returns: GameObjects under coords or null



---

#### FindObjectByPixels

 Uses `UnityEngine.EventSystems.EventSystem` class to raycast by these coords to find GameObject under click. 

|Name | Description |
|-----|------|
|x: |X position in pixels|
|y: |Y position in pixels|
|ignoreNames: |set of names of object, that are ignored|
Returns: GameObjects under coords or null



---

#### GetGameObjectFullPath

 Calculate and retun full path to `GameObject`. 

|Name | Description |
|-----|------|
|gameObject: |`GameObject`|
Returns: Full path to `GameObject`

[[T:System.NullReferenceException|T:System.NullReferenceException]]: 



---

#### GetScrollElement

 Return `Selectable` object from `GameObject` or his parent. 

|Name | Description |
|-----|------|
|go: |`GameObject` to find selectable object|
|handlePosition: |Pointer position|
Returns: 



---

#### HeightPercentsToPixels

 Transform screen percents to screen pixels 

|Name | Description |
|-----|------|
|y: |Y percents position in screen|
Returns: Screen pixels



---

#### LoadSceneForSetUp

 Loads scene by given name. 

|Name | Description |
|-----|------|
|sceneName: |Scene name|


---

#### LogHierarchy

 Prins in console log with a list of path to all GameObject on scene. 

Returns: 



---

#### PercentsToPixels

 Transform screen percents to screen pixels 

Returns: Screen percents

|Name | Description |
|-----|------|
|x: |X percents position in screen|
|y: |Y percents position in screen|
Returns: Screen pixels



---

#### PercentsToPixels

 Transform screen percents to screen pixels 

|Name | Description |
|-----|------|
|percents: |Screen percents|
Returns: Screen pixels



---

#### ScreenVerticesOfObject

 Returns array of coords of screen rectangle of given `RectTransform`. 

|Name | Description |
|-----|------|
|transform: |`RectTransform` component instance|
Returns: Array of coords of screen rectangle of given `RectTransform`



---

#### WidthPercentsToPixels

 Transform screen percents to screen pixels 

|Name | Description |
|-----|------|
|x: |X percents position in screen|
Returns: Screen pixels



---

## Wait 

#### ButtonAccessible

 Waits until given 'GameObject' has component 'UnityEngine.UI.Button' attached or fails by timeout. 

|Name | Description |
|-----|------|
|button: |'GameObject' who should be start accessible|
|timeout: |Timeout|
|ignoreTimeScale: |Should we ignore time scale or not|
Returns: 



---

#### Frame

 Waits for given amount of frames, then returns. 

|Name | Description |
|-----|------|
|count: |Amount of frames to wait|
Returns: 



---

#### ObjectDestroy

 Waits until 'GameObject' with given path is not present in scene or fails by timeout. 

|Name | Description |
|-----|------|
|path: |Path to `GameObject` in hierarchy|
|timeout: |Timeout|
|ignoreTimeScale: |Should we ignore time scale or not|
Returns: 



---

#### ObjectDestroy

 Waits until 'GameObject' with given path is not present in scene or fails by timeout. 

|Name | Description |
|-----|------|
|gameObject: |`GameObject` who should be destroyed|
|timeout: |Timeout|
|ignoreTimeScale: |Should we ignore time scale or not|
Returns: 



---

#### ObjectDestroy

 Waits until 'GameObject' with component 'T' is not present in scene or fails by timeout. 

|Name | Description |
|-----|------|
|timeout: |Timeout|
|ignoreTimeScale: |Should we ignore time scale or not|
Returns: 



---

#### ObjectDisabled

 Waits until 'GameObject' by given path is present on scene and disabled in hierarchy or fails by timeout. 

|Name | Description |
|-----|------|
|path: |Path to 'GameObject' in hierarchy|
|timeout: |Timeout|
|ignoreTimeScale: |Should we ignore time scale or not|
Returns: 



---

#### ObjectDisabled

 Waits until 'GameObject' with component 'T' is present on scene and disabled in hierarchy or fails by timeout. 

|Name | Description |
|-----|------|
|timeout: |Timeout|
|ignoreTimeScale: |Should we ignore time scale or not|
Returns: 



---

#### ObjectEnabled

 Waits until 'GameObject' with given path is present on scene and active in hierarchy or fails by timeout. 

|Name | Description |
|-----|------|
|path: |Path to 'GameObject' in hierarchy|
|timeout: |Timeout|
|ignoreTimeScale: |Should we ignore time scale or not|
Returns: 



---

#### ObjectEnabled

 Waits until 'GameObject' with component 'T' is present on scene and active in hierarchy or fails by timeout. 

|Name | Description |
|-----|------|
|timeout: |Timeout|
|ignoreTimeScale: |Should we ignore time scale or not|
Returns: 



---

#### ObjectEnabledInstantiatedAndDelay

 Awaits for 'GameObject' being present on scene and active in hierarchy. Then waits during given amount of time and returns after that. Method fails by timeout. 

|Name | Description |
|-----|------|
|path: |Path to 'GameObject' in hierarchy|
|delay: |Amount of time to delay|
|timeout: |Timeout|
|dontFail: |If true, method will not generate exception after timeout|
|ignoreTimeScale: |Should we ignore time scale or not|
Returns: 



---

#### ObjectInstantiated

 Awaits for 'GameObject' being present on scene or fails by timeout. 

|Name | Description |
|-----|------|
|path: |Path to 'GameObject' in hierarchy|
|timeout: |Timeout|
|ignoreTimeScale: |Should we ignore time scale or not|
Returns: 



---

#### ObjectInstantiated

 Waits until 'GameObject' with component 'T' is present on scene or fails by timeout. 

|Name | Description |
|-----|------|
|timeout: |Timeout|
|ignoreTimeScale: |Should we ignore time scale or not|
Returns: 



---

#### ObjectInstantiated

 Waits until 'GameObject' with component 'T' is present on scene or fails by timeout. 

|Name | Description |
|-----|------|
|path: |Path to 'GameObject' in hierarchy|
|timeout: |Timeout|
Returns: 



---

#### SceneLeaded

 Waits until scene with given name is loaded or fails by timout. 

|Name | Description |
|-----|------|
|sceneName: |Name of scene to load|
|timeout: ||
|ignoreTimeScale: |Should we ignore time scale or not|
Returns: 



---

#### Seconds

 Waits for seconds. 

|Name | Description |
|-----|------|
|seconds: |Count of seconds to wait|
|ignoreTimescale: |Count of seconds to wait ignorining unity timescalse|
Returns: 



---

#### WaitFor

 Waits until given predicate returns true or fails by timeout. 

|Name | Description |
|-----|------|
|condition: |Predicate that return true, if its condition is successfuly fulfilled.|
|timeout: |Timeout|
|testInfo: | This label would be passed to logs if method fails.|
|dontFail: |If true, method will not generate exception after timeout|
|ignoreTimeScale: |Should we ignore time scale or not|
Returns: 

[[T:System.Exception|T:System.Exception]]: 



---
