# API methods
* [AsyncCheck](#asynccheck)
* [AsyncWait](#asyncwait)
  * [StartWaitingForLog](#startwaitingforlog)
  * [StartWaitingForUnityAnimation](#startwaitingforunityanimation)
  * [StopWaitingForGameLog](#stopwaitingforgamelog)
  * [StopWaitingForUnityAnimation](#stopwaitingforunityanimation)
* [Check](#check)
  * [AnimatorStateStarted](#animatorstatestarted)
  * [AverageFPS](#averagefps)
  * [CheckEnabled](#checkenabled)
  * [CheckToggle](#checktoggle)
  * [DoesNotExist](#doesnotexist)
  * [DoesNotExistOrDisabled](#doesnotexistordisabled)
  * [InputEquals](#inputequals)
  * [InputNotEquals](#inputnotequals)
  * [IsDisable](#isdisable)
  * [IsEnable](#isenable)
  * [IsExist](#isexist)
  * [MinFPS](#minfps)
  * [SourceImage](#sourceimage)
  * [TextEquals](#textequals)
  * [TextNotEquals](#textnotequals)
  * [Toggle](#toggle)
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
  * [ScrollToPosition](#scrolltoposition)
  * [SetText](#settext)
  * [SetTimescale](#settimescale)
  * [WaitDelayAndClick](#waitdelayandclick)
* [Wait](#wait)
  * [AnimationCompleted](#animationcompleted)
  * [ButtonAccessible](#buttonaccessible)
  * [Frame](#frame)
  * [ObjectDestroyed](#objectdestroyed)
  * [ObjectDisabled](#objectdisabled)
  * [ObjectDisabledOrNotExist](#objectdisabledornotexist)
  * [ObjectEnableAndInteractibleIfButton](#objectenableandinteractibleifbutton)
  * [ObjectEnabled](#objectenabled)
  * [ObjectEnabledInstantiatedAndDelay](#objectenabledinstantiatedanddelay)
  * [ObjectInstantiated](#objectinstantiated)
  * [SceneLeaded](#sceneleaded)
  * [Seconds](#seconds)
  * [WaitFor](#waitfor)
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
  * [GetStringComparator](#getstringcomparator)
  * [HeightPercentsToPixels](#heightpercentstopixels)
  * [LoadSceneForSetUp](#loadsceneforsetup)
  * [LogHierarchy](#loghierarchy)
  * [PercentsToPixels](#percentstopixels)
  * [ScreenVerticesOfObject](#screenverticesofobject)
  * [WidthPercentsToPixels](#widthpercentstopixels) 

## AsyncCheck 

 Contains methods which allow checking the state of game components and objects on the scene asynchronously
 No methods provided by default
 


## AsyncWait 

 Contains methods which allow waiting until the game components and objects on the scene reach the certain state asynchronously
 


#### StartWaitingForLog


 Starts the waiting for the log
 

Returns: Abstract async waiter

|Name | Description |
|-----|------|
|message |Expected log message|
|isRegExp |Is expected log a regular expression|
|timeout |Timeout (optional)|


---

#### StartWaitingForUnityAnimation


 Starts the waiting for unity animation to complete
 

Returns: Abstract async waiter

|Name | Description |
|-----|------|
|path |Path to `GameObject` in hierarchy|
|animationName |Animation name|
|timeout |Timeout (optional)|


---

#### StopWaitingForGameLog


 Used only for generation waiting code for StartWaitingForLog 
 



---

#### StopWaitingForUnityAnimation


 Used only for generation waiting code for StartWaitingForUnityAnimation 
 



---

## Check 

 Contains methods which allow checking the state of game components and objects on the scene
 


#### AnimatorStateStarted


 Seraches for `GameObject` by given path with `Animator` component. During a given `timeOut` waits for an animation state with specific name to become active
 

|Name | Description |
|-----|------|
|path |Path to `GameObject` in hierarchy|
|stateName |`Animator` state name|
|timeout |Timeout|


---

#### AverageFPS


 Checks average fps since the last time user called `Interact.ResetFPS()` method or since the game started. If average fps is less than `targetFPS` value, test fails
 

|Name | Description |
|-----|------|
|targetFPS |Minimum acceptable value of average fps|


---

#### CheckEnabled


 Checks that `Gameobject` by given path is present on scene and its active in hierarchy flag equals to state variable
 

|Name | Description |
|-----|------|
|path |Path to `GameObject` in hierarchy|
|state |Enable state (optional)|


---

#### CheckToggle


 Checks that `GameObject` has `Toggle` component and its `isOn` value equals to expected
 

|Name | Description |
|-----|------|
|go |`GameObject` with `Toggle` component|
|expectedIsOn |Expected value of the toggle|


---

#### DoesNotExist


 Checks that `GameObject` by given path is not present on scene
 

|Name | Description |
|-----|------|
|path |Path to `GameObject` in hierarchy|


---

#### DoesNotExist


 Checks that no `GameObject` with component `T` is present on scene
 



---

#### DoesNotExist


 Checks that `GameObject` by given path with component `T` is not present on scene
 

|Name | Description |
|-----|------|
|path |Path to `GameObject` in hierarchy|


---

#### DoesNotExistOrDisabled


 Checks that `GameObject` by given path is not present on scene or is not active
 

|Name | Description |
|-----|------|
|path |Path to `GameObject` in hierarchy|
|timeout |Timeout (optional)|
|ignoreTimeScale |Should time scale be ignored or not (optional)|


---

#### InputEquals


 Checks that `GameObject` by given path has `InputField` component attached and its variable text is equal to expected text
 

|Name | Description |
|-----|------|
|path |Path to `GameObject` in hierarchy|
|expectedText |Expected text|


---

#### InputEquals


 Checks that `GameObject` has `InputField` component attached and its variable text is equal to expected text
 

|Name | Description |
|-----|------|
|go |`GameObject` with `InputField` component|
|expectedText |Expected text|


---

#### InputNotEquals


 Checks that `GameObject` by given path has `InputField` component attached and its variable text is not equal to expected text
 

|Name | Description |
|-----|------|
|path |Path to `GameObject` in hierarchy|
|expectedText |Expected text|


---

#### InputNotEquals


 Checks that `GameObject` has `InputField` component attached and its variable text is not equal to expected text
 

|Name | Description |
|-----|------|
|go |`GameObject` with `InputField` component|
|expectedText |Expected text|


---

#### IsDisable


 Checks that `GameObject` by given path is present on scene and is not active in hierarchy
 

|Name | Description |
|-----|------|
|path |Path to `GameObject` in hierarchy|


---

#### IsDisable


 Checks that `GameObject` is not active in hierarchy
 

|Name | Description |
|-----|------|
|go |`GameObject`|


---

#### IsDisable


 Searches for `Gameobject` with component `T` and checks that it is present on scene and is not active in hierarchy
 



---

#### IsDisable


 Checks that `GameObject` by given path is present on scene, contains component `T` and is not active in hierarchy
 

|Name | Description |
|-----|------|
|path |Path to `GameObject` in hierarchy|


---

#### IsEnable


 Checks that `Gameobject` by given path is present on scene and active in hierarchy
 

|Name | Description |
|-----|------|
|path |Path to `GameObject` in hierarchy|


---

#### IsEnable


 Checks that `GameObject` is active in hierarchy
 

|Name | Description |
|-----|------|
|go |`GameObject`|


---

#### IsEnable


 Searches for `Gameobject` with component `T` and checks that it is present on scene and active in hierarchy
 



---

#### IsEnable


 Checks that `Gameobject` by given path is present on scene, contains component `T` and is active in hierarchy
 

|Name | Description |
|-----|------|
|path |Path to `GameObject` in hierarchy|


---

#### IsExist


 Checks that `GameObject` by given path is present on scene
 

|Name | Description |
|-----|------|
|path |Path to `GameObject` in hierarchy|
Returns: `GameObject`



---

#### IsExist


 Searches for `GameObject` with component `T` on scene
 

Returns: `GameObject`



---

#### IsExist


 Checks that `GameObject` by given path is present on scene and contains component `T`
 

|Name | Description |
|-----|------|
|path |Path to `GameObject` in hierarchy|
Returns: `GameObject`



---

#### MinFPS


 Checks minimum fps since the last time user called `Interact.ResetFPS()` method or since the game started. If minimum fps is less than `targetFPS` value, test fails
 

|Name | Description |
|-----|------|
|targetFPS |Minimum acceptable value of minimum fps|


---

#### SourceImage


 Seraches for `GameObject` by given path and checks the source image
 

|Name | Description |
|-----|------|
|path |Path to `GameObject` in hierarchy|
|sourceName |Source image name|


---

#### TextEquals


 Checks that `GameObject` by given path has `Text` component attached and its variable text is equal to expected text
 

|Name | Description |
|-----|------|
|path |Path to `GameObject` in hierarchy|
|expectedText |Expected text|


---

#### TextEquals


 Checks that `GameObject` has `Text` component attached and its variable text is equal to expected text
 

|Name | Description |
|-----|------|
|go |`GameObject` with `Text` component|
|expectedText |Expected text|


---

#### TextNotEquals


 Checks that `GameObject` by given path has `Text` component attached and its variable text is not equal to expected text
 

|Name | Description |
|-----|------|
|path |Path to `GameObject` in hierarchy|
|expectedText |Expected text|


---

#### TextNotEquals


 Checks that `GameObject` has `Text` component attached and its variable text is not equal to expected text
 

|Name | Description |
|-----|------|
|go |`GameObject` with `Text` component|
|expectedText |Expected text|


---

#### Toggle


 Checks that `GameObject` by given path has `Toggle` component and its `isOn` value equals to expected
 

|Name | Description |
|-----|------|
|path |Path to `GameObject` in hierarchy|
|state |Toggle state|


---

## Interact 

 Contains methods which allow interacting with game components and objects on the scene
 


#### AppendText


 Appends text to `GameObject` by path with `InputField` component attached
 

|Name | Description |
|-----|------|
|path |Path to `GameObject` in hierarchy|
|text |`Text` to set|


---

#### AppendText


 Checks if `GameObject` has `InputField` component attached, then appends given text to text variable of `InputField`
 

|Name | Description |
|-----|------|
|go |`GameObject` with `Text` component|
|text |`Text` to set|


---

#### ClearFPSMetrics


 Clears FPS data from the hard drive
 



---

#### Click


 Emulates LMB click on `Unity UI` element by given path
 

|Name | Description |
|-----|------|
|path |Path to GameObject in hierarchy|


---

#### Click


 Emulates LMB click on `Unity UI GameObject`
 

|Name | Description |
|-----|------|
|go |GameObject to click|


---

#### ClickPercents


 Finds screen space pixel coordinates by given screen size percents and uses `UnityEngine.EventSystems.EventSystem` class to raycast by resulting coordinates to find `GameObject` and perform LMB click on it
 

|Name | Description |
|-----|------|
|x |X position in screen percents|
|y |Y position in screen percents|


---

#### ClickPixels


 Uses `UnityEngine.EventSystems.EventSystem` class to raycast UI by specified coords to find `GameObject` and perform LMB click on it
 

|Name | Description |
|-----|------|
|x |X position in screen pixels|
|y |Y position in screen pixels|


---

#### DragPercents


 Obtains drag percents of `GameObject` by path
 

|Name | Description |
|-----|------|
|path |Path to `GameObject` in hierarchy|
|fromPercentX |Min percent of drag at dimension X|
|fromPercentY |Min percent of drag at dimension Y|
|toPercentX |Max percent of drag at dimension X|
|toPercentY |Max percent of drag at dimension Y|
|time |Time (optional)|


---

#### DragPercents


 Perform drag on `GameObject` by path
 

|Name | Description |
|-----|------|
|path |Path to `GameObject` in hierarchy|
|to |Finish position in percents|
|time |Drag Time (optional)|


---

#### DragPercents


 Perform drag on `GameObject`
 

|Name | Description |
|-----|------|
|go |`GameObject` to drag|
|to |Finish position in percents|
|time |Drag Time (optional)|


---

#### DragPercents


 Uses `UnityEngine.EventSystems.EventSystem` class to raycast by given coordinates to find `GameObject` and perform drag on it
 

|Name | Description |
|-----|------|
|from |Start position in percents|
|to |Finish position in percents|
|time |Drag Time (optional)|


---

#### DragPixels


 Perform drag on `GameObject` by path
 

|Name | Description |
|-----|------|
|path |Path to GameObject in hierarchy|
|to |Finish position in pixels|
|time |Drag Time (optional)|


---

#### DragPixels


 Perform drag on `GameObject`
 

|Name | Description |
|-----|------|
|go |`GameObject` to drag|
|from |Start position in percents|
|to |Finish position in percents|
|time |Drag Time (optional)|


---

#### DragPixels


 Perform drag on `GameObject`
 

|Name | Description |
|-----|------|
|go |`GameObject` to drag|
|to |Finish position in pixels|
|time |Drag Time (optional)|


---

#### DragPixels


 Uses `UnityEngine.EventSystems.EventSystem` class to raycast by given coordinates to find `GameObject` and perform drag on it
 

|Name | Description |
|-----|------|
|from |Start position in pixels|
|to |Finish position in pixels|
|time |Drag Time (optional)|


---

#### MakeScreenShot


 Makes a screenshot. Saves it to persistant data folder
 

|Name | Description |
|-----|------|
|name |Name of screenshot|


---

#### ObsoleteWaitClickAndDelay


 Obsolete. Use WaitDelayAndClick instead
 Waits for one second, then waits until `GameObject` by given path is present on scene and active in hierarchy, then emulates LMB click and finally waits for a specified delay. Fails if exceeds the given timeout
 

|Name | Description |
|-----|------|
|path |Path to GameObject in hierarchy|
|delay |Amount of time to delay|
|timeout |Timeout|


---

#### ObsoleteWaitClickAndDelayIfPossible


 Obsolete. Use WaitDelayAndClick instead
 Waits for one second, then waits until `GameObject` by given path is present on scene and active in hierarchy, then emulates LMB click and finally waits for a specified delay
 

|Name | Description |
|-----|------|
|path |Path to GameObject in hierarchy|
|delay |Amount of time to delay|
|timeout |Timeout|


---

#### ResetFPS


 Resets FPS counter
 FPS counter stores average fps, minimum and maximum FPS values since the last moment this method was called
 



---

#### SaveFPS


 Stores FPS data on the hard drive
 

|Name | Description |
|-----|------|
|tag |Measure discription|


---

#### ScrollToPosition


 Perform scroll on `GameObject` by path
 

|Name | Description |
|-----|------|
|path |Path to `GameObject` in hierarchy|
|horizontalPosition |Horizontal position|
|verticalPosition |Vertical position|
|animationDuration |Animation duration (optional)|
|timeout |Timeout (optional)|
|ignoreTimeScale |Should time scale be ignored or not (optional)|


---

#### SetText


 Finds `GameObject` by path, checks if `GameObject` has `Text` component attached, then set text variable of `Text` to given value
 

|Name | Description |
|-----|------|
|path |Path to `GameObject` in hierarchy|
|text |`Text` to set|


---

#### SetText


 Finds `GameObject` by path, checks if `GameObject` has `Text` component attached, then sets text variable of `Text` to given value
 

|Name | Description |
|-----|------|
|go |`GameObject` with `Text` component|
|text |`Text` to set|


---

#### SetTimescale


 Sets timescale
 

|Name | Description |
|-----|------|
|scale |New timescale|


---

#### WaitDelayAndClick


 Waits until `GameObject` by given path is present on scene and active in hierarchy then emulates LMB click after the specified delay. Fails if exceeds the given timeout
 

|Name | Description |
|-----|------|
|path |Path to GameObject in hierarchy|
|delay |Amount of time to delay|
|timeout |Timeout|


---

## Wait 

 Contains methods which allow waiting until the game components and objects on the scene reach the certain state
 


#### AnimationCompleted


 Waits until animation is completed
 

|Name | Description |
|-----|------|
|path |Path to 'GameObject' in hierarchy|
|animationName |Animation name|
|timeout |Timeout (optional)|
|ignoreTimeScale |Should time scale be ignored or not (optional)|


---

#### ButtonAccessible


 Waits until given 'GameObject' obtains component 'UnityEngine.UI.Button' or fails after specified timeout
 

|Name | Description |
|-----|------|
|button |'GameObject' who should be start accessible|
|timeout |Timeout (optional)|
|ignoreTimeScale |Should time scale be ignored or not (optional)|


---

#### Frame


 Waits for given amount of frames, then return
 

|Name | Description |
|-----|------|
|count |Amount of frames to wait (optional)|


---

#### ObjectDestroyed


 Waits until 'GameObject' by given path disappears from scene or fails after specified timeout
 

|Name | Description |
|-----|------|
|path |Path to `GameObject` in hierarchy|
|timeout |Timeout (optional)|
|ignoreTimeScale |Should time scale be ignored or not (optional)|


---

#### ObjectDestroyed


 Waits until 'GameObject' by given path disappears from scene or fails after specified timeout
 

|Name | Description |
|-----|------|
|gameObject |`GameObject` who should be destroyed|
|timeout |Timeout (optional)|
|ignoreTimeScale |Should time scale be ignored or not (optional)|


---

#### ObjectDestroyed


 Waits until 'GameObject' with component 'T' disappears from scene or fails after specified timeout
 

|Name | Description |
|-----|------|
|timeout |Timeout (optional)|
|ignoreTimeScale |Should time scale be ignored or not (optional)|


---

#### ObjectDisabled


 Waits until 'GameObject' by given path becomes present on scene and disabled in hierarchy or fails after specified timeout
 

|Name | Description |
|-----|------|
|path |Path to 'GameObject' in hierarchy|
|timeout |Timeout (optional)|
|ignoreTimeScale |Should time scale be ignored or not (optional)|


---

#### ObjectDisabled


 Waits until 'GameObject' with component 'T' becomes present on scene and disabled in hierarchy or fails after specified timeout
 

|Name | Description |
|-----|------|
|timeout |Timeout (optional)|
|ignoreTimeScale |Should time scale be ignored or not (optional)|


---

#### ObjectDisabledOrNotExist


 Waits until 'GameObject' by given path disappears from scene or becomes disabled in hierarchy or fails after specified timeout
 

|Name | Description |
|-----|------|
|path |Path to 'GameObject' in hierarchy|
|timeout |Timeout (optional)|
|ignoreTimeScale |Should time scale be ignored or not (optional)|


---

#### ObjectEnableAndInteractibleIfButton


 Awaits for 'GameObject' to become enabled and interactible if it is a button
 

|Name | Description |
|-----|------|
|path |Path to 'GameObject' in hierarchy|
|timeout |Timeout (optional)|
|dontFail |Whether the test should fail upon exceeding timeout (optional)|
|ignoreTimeScale |Should time scale be ignored or not (optional)|


---

#### ObjectEnabled


 Waits until 'GameObject' by given path becomes present on scene and active in hierarchy or fails after specified timeout
 

|Name | Description |
|-----|------|
|path |Path to 'GameObject' in hierarchy|
|timeout |Timeout (optional)|
|ignoreTimeScale |Should time scale be ignored or not (optional)|


---

#### ObjectEnabled


 Waits until 'GameObject' with component 'T' becomes present on scene and active in hierarchy or fails after specified timeout
 

|Name | Description |
|-----|------|
|timeout |Timeout (optional)|
|ignoreTimeScale |Should time scale be ignored or not (optional)|


---

#### ObjectEnabledInstantiatedAndDelay


 Awaits for 'GameObject' to become present on scene and active in hierarchy. Then waits during given amount of time and returns after that. Method fails after exceeding the given timeout
 

|Name | Description |
|-----|------|
|path |Path to 'GameObject' in hierarchy|
|delay |Amount of time to delay|
|timeout |Timeout (optional)|
|dontFail |If true, method will not generate exception after timeout (optional)|
|ignoreTimeScale |Should time scale be ignored or not (optional)|


---

#### ObjectInstantiated


 Awaits for 'GameObject' to become present on scene or fails after specified timeout
 

|Name | Description |
|-----|------|
|path |Path to 'GameObject' in hierarchy|
|timeout |Timeout (optional)|
|ignoreTimeScale |Should time scale be ignored or not (optional)|


---

#### ObjectInstantiated


 Waits until 'GameObject' with component 'T' becomes present on scene or fails after specified timeout
 

|Name | Description |
|-----|------|
|timeout |Timeout (optional)|
|ignoreTimeScale |Should time scale be ignored or not (optional)|


---

#### ObjectInstantiated


 Waits until 'GameObject' with component 'T' becomes present on scene or fails after specified timeout
 

|Name | Description |
|-----|------|
|path |Path to 'GameObject' in hierarchy|
|timeout |Timeout (optional)|
|ignoreTimeScale |Should time scale be ignored or not (optional)|


---

#### SceneLeaded


 Waits until scene with given name is loaded or fails after specified timeout
 

|Name | Description |
|-----|------|
|sceneName |Name of scene to load|
|timeout |Timeout (optional)|
|ignoreTimeScale |Should time scale be ignored or not (optional)|


---

#### Seconds


 Waits for specified amount of seconds
 

|Name | Description |
|-----|------|
|seconds |Count of seconds to wait|
|ignoreTimescale |Should time scale be ignored or not (optional)|


---

#### WaitFor


 Waits until given predicate returns true or fails after specified timeout
 

|Name | Description |
|-----|------|
|condition |Predicate that return true, if its condition is successfuly fulfilled|
|timeout |Timeout|
|testInfo | This label would be passed to logs if method fails|
|dontFail |If true, method will not generate exception after timeout (optional)|
|ignoreTimeScale |Should time scale be ignored or not (optional)|
[[T:System.Exception|T:System.Exception]]: 



---

## UITestUtils 

 Contains methods that are needed for multiple action methods from all classes and facilitates the test actions writing
 


#### CenterPointOfObject


 Return center point of the given `RectTransform`
 

|Name | Description |
|-----|------|
|transform |`RectTransform` component instance|
Returns: Center point of given `RectTransform`



---

#### DecodeString


 Decodes path in Test Tools format to simple path (`%/` => `/`, `%%` => `%`)
 

|Name | Description |
|-----|------|
|text |String to encode|
Returns: Encoded strings



---

#### EncodeString


 Encodes path string for Test Tools format (`/` => `%/`, `%` => `%%`)
 

|Name | Description |
|-----|------|
|text |String to encode|
Returns: Encoded strings



---

#### FindAnyGameObject


 Searches for GameObject by path in hierarchy
 

|Name | Description |
|-----|------|
|path |Path to `GameObject` in hierarchy|
Returns: active and non-active `GameObjects` or null



---

#### FindAnyGameObject


 Searches for `GameObject` that has component of `T` attached to it
 

Returns: active and non-active `GameObjects` or null



---

#### FindAnyGameObject


 Searches for GameObject that has component of the given type attached and matches the given path in hierarchy
 

|Name | Description |
|-----|------|
|path |Path to `GameObject` in hierarchy|
Returns: active and non-active `GameObjects` or null



---

#### FindComponentInParents


 Checks if given GameObject has `TComponent` component attached to it. If not - performs recursive check on its parent
 

|Name | Description |
|-----|------|
|go |Parent `GameObject`|
Returns: `TComponent` instance, or null.



---

#### FindCurrentEventSystem


 Finds `EventSystem` on the scene
 



---

#### FindEnabledGameObjectByPath


 Finds enabled `GameObject` by Path in hierarchy
 

|Name | Description |
|-----|------|
|path |Path to `GameObject` in hierarchy|
Returns: Enabled `GameObject` or null.



---

#### FindGameObjectWithComponentInParents


 Checks if given `GameObject` has `TComponent` component attached to it. If not - performs recursive check on its parent
 

|Name | Description |
|-----|------|
|go |Parent `GameObject`|
Returns: `Component` or null.



---

#### FindObjectByPercents


 Calculates pixels coordinates from percent coordinates, then Uses `UnityEngine.EventSystems.EventSystem` class to raycast by resulting coordinates to find `GameObject` that was clicked
 

|Name | Description |
|-----|------|
|x |X position in percents|
|y |Y position in percents|
Returns: GameObjects under coords or null



---

#### FindObjectByPixels


 Uses `UnityEngine.EventSystems.EventSystem` class to raycast by given coordinates to find GameObject that was clicked
 

|Name | Description |
|-----|------|
|x |X position in pixels|
|y |Y position in pixels|
|ignoreNames |set of names of object, that are ignored (optional)|
Returns: GameObjects under coords or null



---

#### GetGameObjectFullPath


 Calculates and retuns full path to `GameObject`
 

|Name | Description |
|-----|------|
|gameObject |`GameObject`|
Returns: Full path to `GameObject`

[[T:System.NullReferenceException|T:System.NullReferenceException]]: 



---

#### GetScrollElement


 Returns `Selectable` object from `GameObject` or its parent
 

|Name | Description |
|-----|------|
|go |`GameObject` to find selectable object|
|handlePosition |Pointer position|


---

#### GetStringComparator


 Gets the string comparator by specified text and regex option
 

Returns: The string comparator

|Name | Description |
|-----|------|
|text |Text|
|useRegEx |Is the specified text a regular expression|


---

#### HeightPercentsToPixels


 Transform screen percents to screen pixels
 

|Name | Description |
|-----|------|
|y |Y percents position in screen|
Returns: Screen pixels



---

#### LoadSceneForSetUp


 Loads scene by given name
 

|Name | Description |
|-----|------|
|sceneName |Scene name|


---

#### LogHierarchy


 Prints console log with a list of paths to all GameObjects on scene
 



---

#### PercentsToPixels


 Transform screen percents to screen pixels
 

Returns: Screen percents

|Name | Description |
|-----|------|
|x |X percents position in screen|
|y |Y percents position in screen|
Returns: Screen pixels



---

#### PercentsToPixels


 Transform screen percents to screen pixels
 

|Name | Description |
|-----|------|
|percents |Screen percents|
Returns: Screen pixels



---

#### ScreenVerticesOfObject


 Returns array of coordinates of screen rectangle of the given `RectTransform`
 

|Name | Description |
|-----|------|
|transform |`RectTransform` component instance|
Returns: Array of coords of screen rectangle of given `RectTransform`



---

#### WidthPercentsToPixels


 Transform screen percents to screen pixels
 

|Name | Description |
|-----|------|
|x |X percents position in screen|
Returns: Screen pixels



---
