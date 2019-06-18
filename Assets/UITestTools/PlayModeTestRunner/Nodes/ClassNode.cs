using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace Tests.Nodes
{
    public class ClassNode : Node
    {
        [JsonProperty] private string assemblySerialized;
        [JsonProperty] private string typeSerialized;
        
        private Type type;
        [JsonIgnore]
        public Type Type
        {
            get
            {
                if (type == null)
                {
                    var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                    var assembly = assemblies.FirstOrDefault(a => a.FullName == assemblySerialized);
                    if (assembly != null)
                    {
                        type = assembly.GetType(typeSerialized);
                    }
                }
                return type;
            }
            private set
            {
                type = value;
                typeSerialized = type.FullName;
                assemblySerialized = type.Assembly.FullName;
            }
        }
        
        private List<MethodInfo> Getter(Type type, 
            ref List<MethodInfo> infos, List<string> names)
        {
            if (infos == null)
            {
                infos = MethodNamesToInfos(type, names);
            }
            return infos;
        }
        private void Setter(IEnumerable<MethodInfo> value, 
            ref List<MethodInfo> infos, ref List<string> names)
        {
            infos = (List<MethodInfo>)value;
            names = MethodInfosToNames(infos);
        }

        [JsonProperty] private List<string> setUpMethodsNames = new List<string>();
        [JsonProperty] private List<string> oneTimeSetUpMethodsNames = new List<string>();
        [JsonProperty] private List<string> tearDownMethodsNames = new List<string>();
        [JsonProperty] private List<string> oneTimeTearDownMethodsNames = new List<string>();
        
        private List<MethodInfo> setUpMethods;
        private List<MethodInfo> oneTimeSetUpMethods;
        private List<MethodInfo> tearDownMethods;
        private List<MethodInfo> oneTimeTearDownMethods;

        [JsonIgnore]
        public IEnumerable<MethodInfo> SetUpMethods
        {
            get
            {
                return Getter(Type, ref setUpMethods, setUpMethodsNames);
            }
            set
            {
                Setter(value, ref setUpMethods, ref setUpMethodsNames);
            }
        }
        
        [JsonIgnore]
        public IEnumerable<MethodInfo> OneTimeSetUpMethods
        { 
            get 
            {
                return Getter(Type, ref oneTimeSetUpMethods, oneTimeSetUpMethodsNames);
            }
            set
            { 
                Setter(value, ref oneTimeSetUpMethods, ref oneTimeSetUpMethodsNames);
            } 
        }

        [JsonIgnore]
        public IEnumerable<MethodInfo> TearDownMethods
        {
            get
            {
                return Getter(Type, ref tearDownMethods, tearDownMethodsNames);
            }
            set
            {
                Setter(value, ref tearDownMethods, ref tearDownMethodsNames);
            }
        }

        [JsonIgnore]
        public IEnumerable<MethodInfo> OneTimeTearDownMethods 
        {
            get 
            {
                return Getter(Type, ref oneTimeTearDownMethods, oneTimeTearDownMethodsNames);
            }
            set
            {
                Setter(value, ref oneTimeTearDownMethods, ref oneTimeTearDownMethodsNames);
            }
        }


        [JsonProperty]
        private string fullName;
        
        [JsonIgnore]
        public readonly string FilePath;
        
        public void UpdateType(Type type)
        {
            Type = type;
            fullName = Type.FullName;   
        }
        
       
        public ClassNode(Type type, Node parentNode, string filePath = null) 
            : base (type.Name, parentNode)
        {
            FilePath = filePath;
            Type = type;
            fullName = Type.FullName;
        }
        
        [JsonConstructor]
        public ClassNode() 
        {
        }
        
        public ClassNode(string name, Node parentNode) 
            : base (name, parentNode)
        {
            if (parentNode != null)
            {
                fullName = parent.FullName + "." + name;    
            }
            else
            {
                fullName = name;
            }
            
        }

        [JsonIgnore]
        public override string FullName
        {
            get { return fullName; }
        }

        [JsonIgnore]
        public override TestState State
        {
            get
            {
                if (stateIsDirty)
                {
                    stateIsDirty = false;
                    bool isAnyIgnored = false;
                    bool isAnyUndefined = false;
                    bool isAnyFail = false;
                    
                    foreach (var child in children)
                    {
                        if (child.State == TestState.Failed)
                        {
                            isAnyFail = true;
                            break;
                        }
                        if (child.State == TestState.Undefined)
                        {
                            isAnyUndefined = true;
                        }
                        if (child.State == TestState.Ignored)
                        {
                            isAnyIgnored = true;
                        }
                    }

                    if (isAnyFail)
                    {
                        cachedState = TestState.Failed;
                    }
                    else if (isAnyUndefined)
                    {
                        cachedState = TestState.Undefined;
                    }
                    else if(isAnyIgnored)
                    {
                        cachedState = TestState.Ignored;
                    }
                    else
                    {
                        cachedState = TestState.Passed;
                    }
                    
                    return cachedState;
                }
                else
                {
                    return cachedState;
                }
            }
        }
        
        protected override void SetDataFromNode(Node other)
        {
            base.SetDataFromNode(other);
            if (other is ClassNode)
            {
                var otherClassNode = (ClassNode) other;
                cachedState = otherClassNode.cachedState;
            }
        }
    }
}