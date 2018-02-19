using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace PlayQ.UITestTools
{
    [Serializable]
    public class UserFlowModel : ISerializationCallbackReceiver
    {
        private MonoBehaviour mb;
        public const string UI_TEST_SCREEN_BLOCKER = "UI_TEST_SCREEN_BLOCKER";
        
        private List<MethodInfoExtended> assertationMethods =
            new List<MethodInfoExtended>();
        
        private List<MethodInfo> forceRaycastMethods =
            new List<MethodInfo>();

      
        public List<UserActionInfo> UserActions { get; private set; }

        public UserFlowModel()
        {
            UserActions = new List<UserActionInfo>();
        }

        public void FetchAssertationMethods()
        {
            assertationMethods = new List<MethodInfoExtended>();

            var testBaseMethods = typeof(UITestBase).GetMethods(BindingFlags.Public | BindingFlags.Static);
            var waitMethods = typeof(Wait).GetMethods(BindingFlags.Public | BindingFlags.Static);
            var checkAssertationMethods = typeof(Check).GetMethods(BindingFlags.Public | BindingFlags.Static);
            var interactionMethods = typeof(Interact).GetMethods(BindingFlags.Public | BindingFlags.Static);
            
            var forceRaycastMethods = typeof(RaycastChanger).GetMethods(BindingFlags.Public | BindingFlags.Static).ToList();

            var allMethods = new List<MethodInfo>();
            allMethods.AddRange(testBaseMethods);
            allMethods.AddRange(waitMethods);
            allMethods.AddRange(checkAssertationMethods);
            allMethods.AddRange(interactionMethods);

            foreach (var raycastMethod in forceRaycastMethods)
            {
                var attributes = raycastMethod.GetCustomAttributes(false);
                foreach (var attr in attributes)
                {
                    if (attr is RaycastChangerAttribute)
                    {
                        var parameters = raycastMethod.GetParameters();
                        if (parameters.Length == 1 && parameters[0].ParameterType == typeof(bool))
                        {
                            this.forceRaycastMethods.Add(raycastMethod);
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
                        var helperMethodsType = showInEditorAttr.ClassType;

                        var asseratationParamList = assertationMethod.GetParameters();
                        if (showInEditorAttr.PathIsPresent && (asseratationParamList.Length == 0 ||
                                                               asseratationParamList[0].ParameterType !=
                                                               typeof(string)))
                        {
                            Debug.LogWarning("[UserFlowModel] method " + assertationMethod.Name +
                                             " must has first parameter type of string");
                            continue;
                        }

                        MethodInfoExtended methodInfoExtended = new MethodInfoExtended
                        {
                            AssertationMethod = assertationMethod,
                            AssertationMethodDescription = showInEditorAttr.Descryption,
                            PathIsPresent = showInEditorAttr.PathIsPresent,
                            IsDefault = showInEditorAttr.IsDefault
                        };

                        if (helperMethodsType == null)
                        {
                            assertationMethods.Add(methodInfoExtended);
                            continue;
                        }

//                        var helperMethods = helperMethodsType.GetMethods(BindingFlags.Public | BindingFlags.Static);
                        methodInfoExtended.HelperMethodsType = helperMethodsType;
                        var isAvailable = helperMethodsType.GetMethod("IsAvailable");
                        if (isAvailable != null)
                        {
                            ParameterInfo[] paramList = isAvailable.GetParameters();
                            if (paramList.Length == 1 && paramList[0].ParameterType == typeof(GameObject))
                            {
                                methodInfoExtended.CanShowInEditorMethod = isAvailable;
                            }
                        }


                        var getDefautParams = helperMethodsType.GetMethod("GetDefautParams");
                        if (getDefautParams != null)
                        {
                            ParameterInfo[] paramList = getDefautParams.GetParameters();
                            if (paramList.Length == 1 && paramList[0].ParameterType == typeof(GameObject))
                            {
                                methodInfoExtended.ParametersValuesMethod = getDefautParams;
                            }
                        }

                        assertationMethods.Add(methodInfoExtended);
                    }
                }
            }
            
            Debug.Log("[UserFlowModel] OnFETCH");
        }
        
        
        private GameObject FindAvailableInParents(GameObject targetGameobject, MethodInfo checkMethodInfo)
        {
            var result = false;
            var targetTransform = targetGameobject.transform;
            while (!result && targetTransform)
            {
                result = (bool) checkMethodInfo.Invoke(null, new object[] {targetTransform.gameObject});
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
            var newActionAssertationMethods = new List<MethodInfoExtended>();
            foreach (var methodInfoExt in assertationMethods)
            {
                if (methodInfoExt.CanShowInEditorMethod == null)
                {
                    if (methodInfoExt.PathIsPresent)
                    {
                        continue;
                    }
                    List<object> paramsValues = null;
                    var methodRetunsParams = methodInfoExt.ParametersValuesMethod;
                    if (methodRetunsParams == null)
                    {
                        paramsValues = new List<object>();
                    }
                    else
                    {
                        try
                        {
                            paramsValues = (List<object>) methodRetunsParams.Invoke(null, new object[]{null});
                        }
                        catch (Exception ex)
                        {
                            Debug.LogWarning("can't extract parameters for: "+ methodInfoExt.AssertationMethodFullName);
                            continue;
                        }
                            
                    }
                    var isConsistent = CheckMethodParamConsistance(methodInfoExt, paramsValues);
                    if (!isConsistent)
                    {
                        continue;
                    }
                    var newMethodInfoExt = methodInfoExt.Clone();
                    newMethodInfoExt.ParametersValues = paramsValues;
                    newMethodInfoExt.MapParameterNamesToParameterValues();
                    newActionAssertationMethods.Add(newMethodInfoExt);
                }
            }
            if (newActionAssertationMethods.Count == 0)
            {
                Debug.LogWarning("no assertation methods found for no gameObject action");
                return;
            }
            var newAction = new UserActionInfo
            {
                DeviceDPI = Screen.dpi,
                DeviceResolution = new Vector2(Screen.width, Screen.height),
                MethodsInfoExtended = newActionAssertationMethods
            };
            UserActions.Add(newAction);
        }

        public UserActionInfo HandleGameObject(GameObject go)
        {
            var newAction = new UserActionInfo();
            Dictionary<MethodInfoExtended, GameObject> gameObjectToMethodInfo = new Dictionary<MethodInfoExtended, GameObject>();
            foreach (var methodInfoExt in assertationMethods)
            {
                gameObjectToMethodInfo[methodInfoExt] = go;
            }
            var newActionAssertationMethods = FindAvailableAssertations(gameObjectToMethodInfo); 

            newAction.MethodsInfoExtended =
                ExtractParametersValuesForMethods(newActionAssertationMethods, gameObjectToMethodInfo);
            
            
            if (newAction.MethodsInfoExtended.Count > 0)
            {
                var selectedIndex = newAction.MethodsInfoExtended.FindIndex(assertation => assertation.IsDefault);
                if (selectedIndex < 0)
                {
                    selectedIndex = newAction.MethodsInfoExtended.FindIndex(
                        assertation =>assertation.HelperMethodsType == typeof(Interact.ButtonWaitDelayAndClick));
                }
                newAction.SelectedAssertationIndex = selectedIndex >= 0 ? selectedIndex : 0;
                return newAction;
            }

            return null;
        }

        
        public void HandleClick(Vector2 pixelPos)
        {
            var gameObjectToMethodInfo = FindGameObjectsForClick(pixelPos);
            
            var percentPos = new Vector2(pixelPos.x / Screen.width, pixelPos.y / Screen.height);
            var newAction = new UserActionInfo
            {
                DeviceDPI = Screen.dpi,
                DeviceResolution = new Vector2(Screen.width, Screen.height),
                PixelPos = pixelPos,
                PercentPos = percentPos
            };

            var newActionAssertationMethods = FindAvailableAssertations(gameObjectToMethodInfo); 

            newAction.MethodsInfoExtended =
                ExtractParametersValuesForMethods(newActionAssertationMethods, gameObjectToMethodInfo);
            
            if (newAction.MethodsInfoExtended.Count > 0)
            {
                var selectedIndex = newAction.MethodsInfoExtended.FindIndex(assertation => assertation.IsDefault);
                if (selectedIndex < 0)
                {
                    selectedIndex = newAction.MethodsInfoExtended.FindIndex(
                        assertation =>assertation.HelperMethodsType == typeof(Interact.ButtonWaitDelayAndClick));
                }
                newAction.SelectedAssertationIndex = selectedIndex >= 0 ? selectedIndex : 0;
                UserActions.Add(newAction);
            }
        }


        private Dictionary<MethodInfoExtended, GameObject> FindGameObjectsForClick(Vector2 pos)
        {
            Dictionary<MethodInfoExtended, GameObject> result = new Dictionary<MethodInfoExtended, GameObject>();
            
            foreach (var methodInfoExt in assertationMethods)
            {
                Camera camera = null;
                if (methodInfoExt.HelperMethodsType != null)
                {
                    var methodInfo = methodInfoExt.HelperMethodsType.GetMethods(BindingFlags.Public | BindingFlags.Static)
                        .FirstOrDefault(method =>
                    {
                        return method.ReturnType == typeof(Camera) && method.GetParameters().Length == 0;
                    });

                    if (methodInfo != null)
                    {
                        camera = methodInfo.Invoke(null, new object[0]) as Camera;
                        if (camera == null)
                        {
                            Debug.LogWarning("Class " + methodInfoExt.HelperMethodsType + " has method returning Camera," +
                                             " but camera is null. Method " + methodInfoExt.AssertationMethod.Name + " will be skipped");
                            
                            continue;
                        }
                    }
                }

                if (camera != null)
                {
                    var hit = Physics2D.Raycast(camera.ScreenToWorldPoint(Input.mousePosition),
                        Vector2.zero);
                    if (hit.collider != null)
                    {
                        result.Add(methodInfoExt, hit.collider.gameObject);
                    }
                }
                else
                {
                    var gameObject = UITestUtils.FindObjectByPixels(pos.x, pos.y, new HashSet<string>{UI_TEST_SCREEN_BLOCKER});
                    if (gameObject != null)
                    {
                        result.Add(methodInfoExt, gameObject);   
                    }
                }
            }
            return result;
        }
        
        private List<MethodInfoExtended> FindAvailableAssertations(
            Dictionary<MethodInfoExtended, GameObject> gameObjectToMethodInfo, 
            HashSet<Type> exclude = null)
        {
            
            var newActionAssertationMethods = new List<MethodInfoExtended>();

            foreach (var methodInfoExt in assertationMethods)
            {
                if (!gameObjectToMethodInfo.ContainsKey(methodInfoExt))
                {
                    continue;
                }
                    
                if (exclude != null && exclude.Contains(methodInfoExt.HelperMethodsType))
                {
                    continue;
                }

                var checkMethodInfo = methodInfoExt.CanShowInEditorMethod;
                if (checkMethodInfo == null)
                {
                    methodInfoExt.GameObjectFullPath = UITestUtils.GetGameObjectFullPath(gameObjectToMethodInfo[methodInfoExt]);  
                    newActionAssertationMethods.Add(methodInfoExt);
                }
                else
                {
                    var targetGameobject = gameObjectToMethodInfo[methodInfoExt];
                    if (targetGameobject)
                    {
                        targetGameobject = FindAvailableInParents(targetGameobject, checkMethodInfo);
                        if (targetGameobject)
                        {
                            newActionAssertationMethods.Add(methodInfoExt);
                            methodInfoExt.GameObjectFullPath = UITestUtils.GetGameObjectFullPath(targetGameobject);
                            gameObjectToMethodInfo[methodInfoExt] = targetGameobject;
                        }
                        else
                        {
                            methodInfoExt.GameObjectFullPath = UITestUtils.GetGameObjectFullPath(gameObjectToMethodInfo[methodInfoExt]);   
                        }
                    }
                    else
                    {
                        Debug.LogWarning("gameObject is null for assertation method: " + methodInfoExt.AssertationMethodFullName);
                    }
                }
            }
            return newActionAssertationMethods;
        }

        private bool CheckMethodParamConsistance(
            MethodInfoExtended methodInfoExtended,
            List<object> paramsValues)
        {
            var assertationMethod = methodInfoExtended.AssertationMethod;
            var paramList = assertationMethod.GetParameters();
            int parameterValueIndex = 0;

            bool firstParamWasSkipped = false;
            foreach (var param in paramList)
            {
                if (!firstParamWasSkipped && param.ParameterType == typeof(string) && methodInfoExtended.PathIsPresent)
                {
                    firstParamWasSkipped = true;
                    continue;
                }

                if (parameterValueIndex >= paramsValues.Count ||
                    paramsValues[parameterValueIndex].GetType() != param.ParameterType)
                {
                    Debug.LogWarning("Params missmatch for asseratation method " + assertationMethod.Name);
                    return false;
                }
                parameterValueIndex++;
            }
            return true;
        }


        private List<MethodInfoExtended> ExtractParametersValuesForMethods(
            List<MethodInfoExtended> assertationMethods, 
            Dictionary<MethodInfoExtended, GameObject> gameObjectToMethodInfo)
        {    
            List<MethodInfoExtended> result = new List<MethodInfoExtended>();
            
            foreach (var methodInfoExt in  assertationMethods)
            {
                if (!gameObjectToMethodInfo.ContainsKey(methodInfoExt))
                {
                    continue;
                }
                var targetGameObject = gameObjectToMethodInfo[methodInfoExt];
                
                List<object> paramsValues;
                var methodRetunsParams = methodInfoExt.ParametersValuesMethod;
                if (methodRetunsParams == null)
                {
                    paramsValues = new List<object>();
                }
                else
                {
                    if (targetGameObject)
                    {
                        paramsValues = (List<object>) methodRetunsParams.Invoke(null, new object[] {targetGameObject});
                    }
                    else
                    {
                         Debug.LogWarning("gameObject is null for method " + methodInfoExt.AssertationMethodFullName);
                        continue;
                    }
                }

                var isConsistent = CheckMethodParamConsistance(methodInfoExt, paramsValues);
                
                if (!isConsistent)
                {
                    continue;
                }

                var newMethodInfoExt = methodInfoExt.Clone();
                newMethodInfoExt.ParametersValues = paramsValues;
                newMethodInfoExt.MapParameterNamesToParameterValues();
                result.Add(newMethodInfoExt);
            }
            
            return result;
        }



        public string GeneratedCode(bool isGenerateDebugStrings)
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

                if (isGenerateDebugStrings)
                {
                    cb.NewLine("Debug.Log(\"try to {0} on {1}\");",
                        action.SelectedAssertation.AssertationMethodDescription,
                        action.SelectedAssertation.GameObjectFullPath);   
                }
                if (!string.IsNullOrEmpty(action.Description))
                {
                    cb.NewLine("// " + action.Description);    
                }
                cb.NewLine(action.SelectedAssertation.GenerateMethodCode());
                cb.NewLine();
            }

            cb.CloseBrace();

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
            var currentAction = userAction.SelectedAssertation;
            if (currentAction.AssertationMethod == null)
            {
                var assertation = assertationMethods.FirstOrDefault(assertationMethod =>
                {
                    return assertationMethod.AssertationMethodFullName == currentAction.AssertationMethodFullName;
                });

                if (assertation != null)
                {
                    currentAction.AssertationMethod = assertation.AssertationMethod;
                }
                else
                {
                    Debug.LogWarning("can't find assertation method for method with name: " +
                                     currentAction.AssertationMethodFullName);
                    return;
                }   
            }
           
            var fullParams = new List<object>();
            if (currentAction.PathIsPresent)
            {
                fullParams.Add(currentAction.GameObjectFullPath);
            }
            if (currentAction.ParametersValuesToSave != null && currentAction.ParametersValuesToSave.Count>0)
            {
                fullParams.AddRange(currentAction.ParametersValuesToSave.Select(data=>data.Get()));
            }
            if (currentAction.AssertationMethodReturnType == typeof(IEnumerator).Name)
            {
                var result = (IEnumerator) currentAction.AssertationMethod.Invoke(null, fullParams.ToArray());
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
                currentAction.AssertationMethod.Invoke(null, fullParams.ToArray());
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

        public void NewLine(string text = null, params System.Object[] args)
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
                sb.Append(String.Format(text, args));
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
	public class UserActionInfo
	{
		public Vector2 DeviceResolution;
		public float DeviceDPI;
	    public string Description;
            
		public Vector2 PixelPos;
		public Vector2 PercentPos;
		public Vector2 InchPos;
		public List<MethodInfoExtended> MethodsInfoExtended;
	    public int SelectedAssertationIndex;
    
		public MethodInfoExtended SelectedAssertation
		{
			get { return MethodsInfoExtended[SelectedAssertationIndex]; }
		}
        
	    public UserActionInfo(){}
	    
	    public UserActionInfo(UserActionInfo copy)
	    {
	        DeviceResolution = new Vector2(copy.DeviceResolution.x, copy.DeviceResolution.y);
	        DeviceDPI = copy.DeviceDPI;
	        PixelPos = new Vector2(copy.PixelPos.x, copy.PixelPos.y);
	        PercentPos = new Vector2(copy.PercentPos.x, copy.PercentPos.y);
	        InchPos = new Vector2(copy.InchPos.x, copy.InchPos.y);
	        MethodsInfoExtended = copy.MethodsInfoExtended;
	        SelectedAssertationIndex = copy.SelectedAssertationIndex;
	    }

	    public UserActionInfo Clone()
	    {
	        var clone = (UserActionInfo)MemberwiseClone();
	        if (MethodsInfoExtended != null)
	        {
	            clone.MethodsInfoExtended = new List<MethodInfoExtended>();
	            for (int i = 0; i<MethodsInfoExtended.Count; i++)
	            {
	                clone.MethodsInfoExtended.Add(MethodsInfoExtended[i].Clone());
	            }    
	        }
	        
	        return clone;
	    }
	}
    
    [Serializable]
    public class MethodInfoExtended
    {
        public bool IsDefault;
        public string GameObjectFullPath;
        public bool PathIsPresent;
        
        private Type helperMethodsType;
        public Type HelperMethodsType
        {
            get
            {
                if (helperMethodsType == null)
                {
                    helperMethodsType = typeof(UITestBase).Assembly.GetTypes()
                        .FirstOrDefault(type => type.FullName == HelperMethodsTypeSerializable);
                }
                return helperMethodsType;
            }
            set
            {
                helperMethodsType = value;
                HelperMethodsTypeSerializable = helperMethodsType.FullName;
            }
        }
        public string HelperMethodsTypeSerializable;
        
        public string AssertationMethodName;
        public string AssertationMethodReturnType;
        public string AssertationMethodDeclaringType;


        private MethodInfo assertationMethod;
        public MethodInfo AssertationMethod
        {
            get { return assertationMethod; }
            set
            {
                assertationMethod = value;
                AssertationMethodName = assertationMethod.Name;
                AssertationMethodReturnType = TypeFullName(assertationMethod.ReturnType);
                AssertationMethodDeclaringType = TypeFullName(assertationMethod.DeclaringType);
                AssertationMethodFullName = assertationMethod.ToString();
            }
        }
        
        public MethodInfo ParametersValuesMethod;
        public MethodInfo CanShowInEditorMethod;

        public string AssertationMethodFullName;
        
        public string AssertationMethodDescription;
        public List<MethodInfoParametrsSave> ParametersValuesToSave;

        public void MapParameterNamesToParameterValues()
        {
            ParameterInfo[] paramInfo = assertationMethod.GetParameters();

            if (PathIsPresent)
            {
                paramInfo = paramInfo.Skip(1).ToArray();
            }
            
            if (ParametersValuesToSave != null)
            {
                
                for (int i=0; i<ParametersValuesToSave.Count; i++)
                {
                    
                    ParametersValuesToSave[i].Name = paramInfo[i].Name;
                }
            }
            else
            {
                Debug.LogWarning("you are trying to map param names, but ParametersValuesToSave is null");
            }
        }

        [Serializable]
        public class MethodInfoParametrsSave
        {
            public string Type;
            public string Value;
            public string Name;
            public bool isEnum;

            public MethodInfoParametrsSave(object parametr)
            {
                Type = parametr.GetType().ToString();
                Value = parametr.ToString();
                isEnum = parametr is Enum;
            }

            public MethodInfoParametrsSave Clone()
            {
                return (MethodInfoParametrsSave) MemberwiseClone();
            }
            
            
            public object Get()
            {
                if (isEnum)
                {
                    var enumType = typeof(UITestBase).Assembly.GetType(Type);
                    if (enumType != null)
                    {
                        return Enum.Parse(enumType, Value);
                    }
                    else
                    {
                        return null;
                    }
                }
                switch (Type)
                {
                    case "System.Int32":
                        return int.Parse(Value);
                    case "System.Single":
                        return float.Parse(Value);
                    case "System.Boolean":
                        return bool.Parse(Value);
                    case "System.String":
                        return Value;
                    default:
                        throw new ArgumentOutOfRangeException(Type);
                }
            }
        }

        public List<object> ParametersValues
        {
            set
            {
                ParametersValuesToSave = new List<MethodInfoParametrsSave>();
                foreach (var parametr in value)
                {
                    ParametersValuesToSave.Add(new MethodInfoParametrsSave(parametr));
                }
            }
        }
        
        public MethodInfoExtended Clone()
        {
            var clone = (MethodInfoExtended)MemberwiseClone();
            if (ParametersValuesToSave != null)
            {
                clone.ParametersValuesToSave = new List<MethodInfoParametrsSave>();
                for (int i = 0; i < ParametersValuesToSave.Count; i++)
                {
                    clone.ParametersValuesToSave.Add(ParametersValuesToSave[i].Clone());
                }    
            }
            return clone;
        }
        
        
        public string GenerateMethodCode()
        {
            var sb = new StringBuilder();

            if (AssertationMethodReturnType == typeof(System.Collections.IEnumerator).Name)
            {
                sb.Append("yield return ");
            }

            if (PathIsPresent)
            {
                sb.Append(AssertationMethodDeclaringType + "." + AssertationMethodName + "(\"" + GameObjectFullPath + "\"");
            }
            else
            {
                sb.Append(AssertationMethodDeclaringType + "." + AssertationMethodName + "(");
            }
            
            

            for (int i = 0; i < ParametersValuesToSave.Count; i++)
            {
                if (PathIsPresent || i > 0)
                {
                    sb.Append(", ");    
                }


                switch (ParametersValuesToSave[i].Type)
                {
                    case "System.Single":
                        sb.Append(ParametersValuesToSave[i].Value);
                        sb.Append('f');
                        break;
                        
                    case "System.Boolean":
                        sb.Append(ParametersValuesToSave[i].Value.ToLower());
                        break;
                        
                    case "System.String":
                        sb.Append("\"");
                        sb.Append(ParametersValuesToSave[i].Value);
                        sb.Append("\"");
                        break;
                        
                    default:
                        if (ParametersValuesToSave[i].isEnum)
                        {
                            var type = ParametersValuesToSave[i].Get().GetType(); 
                            sb.Append(TypeFullName(type)+"."+ParametersValuesToSave[i].Value);
                        }
                        else
                        {
                            sb.Append(ParametersValuesToSave[i].Value);    
                        }
                        break;
                }
                
               
            }
            sb.Append(");");
            return sb.ToString();
        }

        public static string TypeFullName(Type type)
        {
            var result = new StringBuilder();
            Type declaringType = type;
            while (declaringType != null)
            {
                result.Insert(0, declaringType.Name);
                declaringType = declaringType.DeclaringType;
                if (declaringType != null)
                {
                    result.Insert(0, '.');   
                }
            }
            return result.ToString();
        }
    }
}
