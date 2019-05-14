using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;

namespace Tests.Nodes
{
    public class ClassNode : Node
    {
        public Type Type { get; private set; }

        private List<MethodInfo> setUpMethods;
        
        [JsonIgnore]
        public IEnumerable<MethodInfo> SetUpMethods { get { return setUpMethods; } set { setUpMethods = (List<MethodInfo>)value; } }

        private List<MethodInfo> oneTimeSetUpMethods;
        [JsonIgnore]
        public IEnumerable<MethodInfo> OneTimeSetUpMethods
        { 
            get 
            {
                return oneTimeSetUpMethods;  
            }
            set
            { 
                oneTimeSetUpMethods = (List<MethodInfo>)value;

                oneTimeSetUpMethods.Sort((a, b) => 
                {
                    return NamespaceDepth(a).CompareTo(NamespaceDepth(b));
                });
            } 
        }

        [JsonIgnore]
        public readonly string FilePath;

        private List<MethodInfo> tearDownMethods;
        [JsonIgnore]
        public IEnumerable<MethodInfo> TearDownMethods { get { return tearDownMethods; } set { tearDownMethods = (List<MethodInfo>)value; } }

        private List<MethodInfo> oneTimeTearDownMethods;
        
        [JsonProperty]
        private string fullName;

        [JsonIgnore]
        public IEnumerable<MethodInfo> OneTimeTearDownMethods 
        {
            get 
            {
                return oneTimeTearDownMethods;
            }
            set
            {
                oneTimeTearDownMethods = (List<MethodInfo>)value;

                oneTimeTearDownMethods.Sort((a, b) =>
                {
                    return -(NamespaceDepth(a).CompareTo(NamespaceDepth(b)));
                });
            }
        }

        private int NamespaceDepth(MethodInfo info)
        {
            Type baseType = info.DeclaringType.BaseType;

            if (baseType == typeof(Object))
                return 0;

            return baseType.FullName.Split('.').Length;
        }


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