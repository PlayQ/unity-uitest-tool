using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace PlayQ.UITestTools
{
    [Serializable]
    public class UserFlowModel : ISerializationCallbackReceiver
    {
        private MonoBehaviour mb;
        public const string UI_TEST_SCREEN_BLOCKER = "UI_TEST_SCREEN_BLOCKER";

        private List<Assertation> assertations =
            new List<Assertation>();

        private List<MethodInfo> forceRaycastMethods =
            new List<MethodInfo>();


        public List<UserActionInfo> UserActions { get; private set; }

        public UserFlowModel()
        {
            UserActions = new List<UserActionInfo>();
        }

        public void FetchAssertationMethods()
        {
            assertations.Clear();

            var waitMethods = typeof(Wait).GetMethods(BindingFlags.Public | BindingFlags.Static);
            var checkAssertationMethods = typeof(Check).GetMethods(BindingFlags.Public | BindingFlags.Static);
            var interactionMethods = typeof(Interact).GetMethods(BindingFlags.Public | BindingFlags.Static);
            var asyncCheckMethods = typeof(AsyncCheck).GetMethods(BindingFlags.Public | BindingFlags.Static);
            var asyncWaitMethods = typeof(AsyncWait).GetMethods(BindingFlags.Public | BindingFlags.Static);
            
            var forceRaycastMethodsAll = typeof(RaycastChanger).GetMethods(BindingFlags.Public | BindingFlags.Static).ToList();

            var allMethods = new List<MethodInfo>();
            allMethods.AddRange(waitMethods);
            allMethods.AddRange(checkAssertationMethods);
            allMethods.AddRange(interactionMethods);
            allMethods.AddRange(asyncCheckMethods);
            allMethods.AddRange(asyncWaitMethods);

            foreach (var raycastMethod in forceRaycastMethodsAll)
            {
                var attributes = raycastMethod.GetCustomAttributes(false);
                foreach (var attr in attributes)
                {
                    if (attr is RaycastChangerAttribute)
                    {
                        var parameters = raycastMethod.GetParameters();
                        if (parameters.Length == 1 && parameters[0].ParameterType == typeof(bool))
                        {
                            forceRaycastMethods.Add(raycastMethod);
                        }
                        else
                        {
                            Debug.LogWarning("Wrong signature for method: " + raycastMethod.Name);
                        }
                    }
                }
            }

            foreach (var assertationMethod in allMethods)
            {
                var attributes = assertationMethod.GetCustomAttributes(false);
                foreach (var attr in attributes)
                {
                    var showInEditorAttr = attr as ShowInEditorAttribute;

                    if (showInEditorAttr != null)
                    {
                        var helper = Activator.CreateInstance(showInEditorAttr.ClassType) as ShowHelperBase;
                        if (helper == null)
                        {
                            Debug.LogError("assertation helper class " + showInEditorAttr.ClassType +
                                           " is not derived from " + typeof(ShowHelperBase) +
                                           " for method " + assertationMethod.Name);
                            continue;
                        }

                        Assertation assertation = new Assertation
                        {
                            methodInfo = assertationMethod,
                            AssertationMethodDescription = showInEditorAttr.Description,
                            IsDefault = showInEditorAttr.IsDefault,
                            Helper = helper
                        };

                        assertations.Add(assertation);
                    }
                }
            }

            Debug.Log("[UserFlowModel] OnFETCH");
        }


        private GameObject FindAvailableInParents(GameObject targetGameobject, ShowHelperBase helper)
        {
            var result = false;
            var targetTransform = targetGameobject.transform;
            while (!result && targetTransform)
            {
                result = helper.IsAvailable(targetTransform.gameObject);
                if (!result)
                {
                    targetTransform = targetTransform.parent;
                }
            }

            return targetTransform ? targetTransform.gameObject : null;
        }

        public void CleanFlow()
        {
            UserActions.Clear();
            WaitVariablesContainer.Clear();
        }

        public void ForceEnableRaycast(bool isEnabled)
        {
            foreach (var methodInfo in forceRaycastMethods)
            {
                methodInfo.Invoke(null, new object[] {isEnabled});
            }
        }

        public void CreateNoGameobjectAction()
        {
            var gameObjectToAssertation = new Dictionary<Assertation, GameObject>();
            foreach (var assertation in assertations)
            {
                if (assertation.Helper.IsAvailable(null))
                {
                    gameObjectToAssertation[assertation] = null;
                }
            }

            if (gameObjectToAssertation.Count > 0)
            {
                var newAction = new UserActionInfo
                {
                    DeviceDPI = Screen.dpi,
                    DeviceResolution = new Vector2(Screen.width, Screen.height)
                };
                SetGeneratorsToUserAction(gameObjectToAssertation, newAction);
                UserActions.Add(newAction);
            }
        }
        

        public UserActionInfo HandleGameObject(GameObject go)
        {
            var newAction = new UserActionInfo();
            var gameObjectToAssertation = new Dictionary<Assertation, GameObject>();
            foreach (var assertation in assertations)
            {
                var adjustedGo = FindAvailableInParents(go, assertation.Helper);
                if (adjustedGo)
                {
                    gameObjectToAssertation[assertation] = adjustedGo;
                }
            }

            SetGeneratorsToUserAction(gameObjectToAssertation, newAction);
            return AddUserActionAndSetSelectedAssertation(newAction);
        }

        public void HandleClick(Vector2 pixelPos)
        {
            var gameObjectToAssertation = FindGameObjectToAssertationByClick(pixelPos);

            var percentPos = new Vector2(pixelPos.x / Screen.width, pixelPos.y / Screen.height);
            var newAction = new UserActionInfo
            {
                DeviceDPI = Screen.dpi,
                DeviceResolution = new Vector2(Screen.width, Screen.height),
                PixelPos = pixelPos,
                PercentPos = percentPos
            };

            SetGeneratorsToUserAction(gameObjectToAssertation, newAction);
            AddUserActionAndSetSelectedAssertation(newAction);
        }


        private UserActionInfo AddUserActionAndSetSelectedAssertation(UserActionInfo newAction)
        {
            if (newAction.CodeGenerators.Count > 0)
            {
                var selectedIndex = newAction.AvailableAssertations.FindIndex(assertation => assertation.IsDefault);
                if (selectedIndex < 0)
                {
                    selectedIndex = newAction.CodeGenerators.FindIndex(generator =>
                    {
                        var allGenerators = generator.CalculateGeneratorSequence();
                        return allGenerators.Any(gen => gen.GetType() == typeof(ParameterPathToGameObject));
                    });
                }

                if (selectedIndex < 0)
                {
                    selectedIndex = newAction.AvailableAssertations.FindIndex(assertation =>
                    {
                        return assertation.Helper is Interact.ButtonWaitDelayAndClick;
                    });
                }

                newAction.SelectedIndex = selectedIndex >= 0 ? selectedIndex : 0;
                UserActions.Add(newAction);
                return newAction;
            }

            return null;
        }


        private Dictionary<Assertation, GameObject> FindGameObjectToAssertationByClick(Vector2 pos)
        {
            Dictionary<Assertation, GameObject> result = new Dictionary<Assertation, GameObject>();

            foreach (var assertation in assertations)
            {
                GameObject go = null;
                var camera = assertation.Helper.GetCamera();
                if (camera != null)
                {
                    var hit = Physics2D.Raycast(camera.ScreenToWorldPoint(Input.mousePosition),
                        Vector2.zero);
                    if (hit.collider != null)
                    {
                        go = hit.collider.gameObject;
                    }
                }
                else
                {
                    go = UITestUtils.FindObjectByPixels(pos.x, pos.y, new HashSet<string> {UI_TEST_SCREEN_BLOCKER});
                }

                if (go)
                {
                    var adjustedGo = FindAvailableInParents(go, assertation.Helper);
                    if (adjustedGo)
                    {
                        result[assertation] = adjustedGo;
                    }
                }
            }

            return result;
        }


        private void SetGeneratorsToUserAction(
            Dictionary<Assertation, GameObject> gameObjectToAssertation,
            UserActionInfo userActionInfo)
        {
            userActionInfo.CodeGenerators = new List<AbstractGenerator>();
            userActionInfo.AvailableAssertations = new List<Assertation>();
            foreach (var gameObjectToAssert in gameObjectToAssertation)
            {
                var generator = gameObjectToAssert.Key.Helper.CreateGenerator(gameObjectToAssert.Value);
                userActionInfo.CodeGenerators.Add(generator);
                userActionInfo.AvailableAssertations.Add(gameObjectToAssert.Key);
            }
        }



        public string GeneratedCode()
        {
            if (UserActions.Count == 0)
            {
                return null;
            }

            var cb = new TestCodeBuilder();
            cb.NewLine("[Timeout(2000000)]");
            cb.NewLine("[UnityTest]");
            cb.NewLine("public IEnumerator Test()");
            cb.OpenBrace();

            for (int i = 0; i < UserActions.Count; i++)
            {
                var action = UserActions[i];
                if (!String.IsNullOrEmpty(action.Description))
                {
                    cb.NewLine("//" + action.Description);
                }
                cb.NewLine(action.GenerateCode());
            }

            cb.CloseBrace();

            WaitVariablesContainer.Clear();
            return cb.ToString();
        }

        public void OnBeforeSerialize()
        {
            Debug.Log("[UserFlowModel] OnBeforeSerialize");
        }

        public void OnAfterDeserialize()
        {
            Debug.Log("[UserFlowModel] OnAfterDeserialize");
            FetchAssertationMethods();
        }

        public void ApplyAction(UserActionInfo userAction)
        {
            var assertationToApply = userAction.SelectedAssertation;
            if (assertationToApply.methodInfo == null)
            {
                var fullNameToApply = AbstractGenerator.MethodFullName(assertationToApply.methodInfo);
                var requiredAssertation = assertations.FirstOrDefault(assertation =>
                {
                    var fullName = AbstractGenerator.MethodFullName(assertation.methodInfo);
                    return fullName == fullNameToApply;
                });

                if (requiredAssertation != null)
                {
                    userAction.AvailableAssertations[userAction.SelectedIndex] = requiredAssertation;
                }
                else
                {
                    Debug.LogWarning("can't find assertation method for method with name: " + fullNameToApply);
                    return;
                }
            }

            var generators = userAction.SelectedCodeGenerator.CalculateGeneratorSequence();
            var parameters = generators
                .Where(gen => gen is AbstractParameter)
                .Select(gen => (gen as AbstractParameter).ParameterValueToObject()).ToArray();



            if (assertationToApply.methodInfo.ReturnType == typeof(IEnumerator))
            {
                var result = (IEnumerator) assertationToApply.methodInfo.Invoke(null, parameters);
                if (!mb)
                {
                    var go = new GameObject();
                    go.name = "UI_TEST_TOOLS_COROUTINE_HELPER";
                    mb = go.AddComponent<EmptyMono>();
                }

                mb.StartCoroutine(result);
            }
            else
            {
                assertationToApply.methodInfo.Invoke(null, parameters);
            }

        }
    }

    public class EmptyMono : MonoBehaviour
    {
        private void Awake()
        {
            DontDestroyOnLoad(this);
        }
    }

    public class TestCodeBuilder
    {
        private int tab;
        private StringBuilder sb = new StringBuilder();

        override public string ToString()
        {
            return sb.ToString();
        }

        public void NewLine(string text = null)
        {
            if (sb.Length != 0)
            {
                sb.Append("\n");
            }

            for (int i = 0; i < tab; i++)
            {
                sb.Append("\t");
            }

            if (!String.IsNullOrEmpty(text))
            {
                sb.Append(text);
            }
        }

        public void NewLine(StringBuilder sb)
        {
            sb.Append("\n");
            for (int i = 0; i < tab; i++)
            {
                sb.Append("\t");
            }

            if (sb != null)
            {
                sb.Append(sb);
            }
        }

        public void OpenBrace()
        {
            sb.Append("\n");
            for (int i = 0; i < tab; i++)
            {
                sb.Append("\t");
            }

            sb.Append("{");
            tab++;
        }

        public void CloseBrace()
        {
            sb.Append("\n");
            tab--;
            for (int i = 0; i < tab; i++)
            {
                sb.Append("\t");
            }

            sb.Append("}");
        }

    }

    [Serializable]
    public class UserActionInfo : ISerializationCallbackReceiver
    {
        public Vector2 DeviceResolution;
        public float DeviceDPI;
        public string Description;

        public Vector2 PixelPos;
        public Vector2 PercentPos;
        public Vector2 InchPos;
        public List<Assertation> AvailableAssertations = new List<Assertation>();
        public List<AbstractGenerator> CodeGenerators = new List<AbstractGenerator>();
        private string codeGeneratorsSerialized;
        public int SelectedIndex;

        public string GenerateCode()
        {
            return SelectedCodeGenerator.GenerateCode(SelectedAssertation.methodInfo);
        }

        public AbstractGenerator SelectedCodeGenerator
        {
            get
            {
                if (CodeGenerators.Count == 0)
                {
                    return null;
                }

                return CodeGenerators[SelectedIndex];
            }
        }

        public Assertation SelectedAssertation
        {
            get
            {
                if (AvailableAssertations.Count == 0 || SelectedIndex < 0 || SelectedIndex >= AvailableAssertations.Count)
                {
                    return null;
                }

                return AvailableAssertations[SelectedIndex];
            }
        }

        public UserActionInfo()
        {
        }

        public UserActionInfo(UserActionInfo copy)
        {
            Description = copy.Description;
            DeviceResolution = new Vector2(copy.DeviceResolution.x, copy.DeviceResolution.y);
            DeviceDPI = copy.DeviceDPI;
            PixelPos = copy.PixelPos;
            PercentPos = copy.PercentPos;
            InchPos = copy.InchPos;
            SelectedIndex = copy.SelectedIndex;
            if (copy.CodeGenerators != null)
            {
                CodeGenerators = new List<AbstractGenerator>();
                foreach (var generator in copy.CodeGenerators)
                {
                    var copyGenerator = generator.Clone();
                    CodeGenerators.Add(copyGenerator);
                }
            }

            AvailableAssertations = copy.AvailableAssertations;
        }

        public void OnBeforeSerialize()
        {
            codeGeneratorsSerialized = JToken.FromObject(CodeGenerators,
                new JsonSerializer
                {
                    TypeNameHandling = TypeNameHandling.All,
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                    Formatting = Formatting.Indented,
                    NullValueHandling = NullValueHandling.Ignore
                }).ToString();
            Debug.Log(codeGeneratorsSerialized);
        }
        
        
        public void OnAfterDeserialize()
        {
            var temp = JToken.Parse(codeGeneratorsSerialized)
                .ToObject<List<AbstractGenerator>>(new JsonSerializer
                {
                    TypeNameHandling = TypeNameHandling.All,
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                    Formatting = Formatting.Indented,
                    NullValueHandling = NullValueHandling.Ignore
                });

            
            if (temp != null)
            {
                List<int> toRemove = new List<int>();
                for (int i = 0; i < temp.Count; i++)
                {
                    if (temp[i] == null)
                    {
                        toRemove.Add(i);
                    }
                }

                foreach (var index in toRemove)
                {
                    temp.RemoveAt(index);
                }
            }
            CodeGenerators = temp;

            //AvailableAssertations.RemoveAll(item => item.methodInfo == null);
        }
    }

    [Serializable]
    public class Assertation : ISerializationCallbackReceiver
    {
        private string serialized;
        public bool IsDefault;
        public MethodInfo methodInfo;
        public string AssertationMethodDescription;
        public ShowHelperBase Helper;

        private string methodName;
        private string methodDeclareClass;
        private string helperShowInEditorType;
        private List<string> methodsParametersTypeNames = new List<string>();

        public void OnBeforeSerialize()
        {
            methodName = methodInfo.Name;
            methodDeclareClass = methodInfo.DeclaringType.FullName;
            methodsParametersTypeNames = methodInfo.GetParameters()
                .Select(par => par.ParameterType.FullName).ToList();

            helperShowInEditorType = Helper.GetType().FullName;
        }
        
        public void OnAfterDeserialize()
        {
            var declareType = typeof(Interact).Assembly.GetType(methodDeclareClass);
            if (declareType == null || String.IsNullOrEmpty(helperShowInEditorType))
            {
                return;
            }
            
            var helperType = typeof(Interact).Assembly.GetType(helperShowInEditorType);
            if (helperType == null)
            {
                return;
            }

            Helper = Activator.CreateInstance(helperType) as ShowHelperBase;
            
           
            var methodsInfo = declareType.GetMethods(BindingFlags.Public |  BindingFlags.Static);
            methodInfo = methodsInfo.FirstOrDefault(method =>
            {
                if (method.Name == methodName)
                {
                    var actualParameters = method.GetParameters();
                    if (actualParameters.Length == methodsParametersTypeNames.Count)
                    {
                        for (int i = 0; i < actualParameters.Length; i++)
                        {
                            if (actualParameters[i].ParameterType.FullName !=
                                methodsParametersTypeNames[i])
                            {
                                return false;
                            }
                        }
                        return true;
                    }
                }
                return false;
            });
        }
    }
    
}
