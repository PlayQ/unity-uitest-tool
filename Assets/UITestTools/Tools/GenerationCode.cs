using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

namespace PlayQ.UITestTools
{
    public abstract class ShowHelperBase
    {
        protected AbstractGenerator VoidMethod
        {
            get { return new ReturnVoid() + new MethodName(); }
        }

        protected AbstractGenerator IEnumeratorMethod
        {
            get { return new ReturnIEnumerator() + new MethodName(); }
        }

        public abstract AbstractGenerator CreateGenerator(GameObject go);

        public virtual bool IsAvailable(GameObject go)
        {
            return true;
        }

        public virtual Camera GetCamera()
        {
            return null;
        }
    }

    public static class ExtentionForAbstractGenerator
    {
        public static AbstractGenerator Int(this AbstractGenerator abstractGenerator, int value)
        {
           return abstractGenerator.Append(new ParameterInt(value));
        }
        public static AbstractGenerator String(this AbstractGenerator abstractGenerator, string value, bool regex = false)
        {
           return abstractGenerator.Append(new ParameterString(value, regex));
        }
        public static AbstractGenerator Float(this AbstractGenerator abstractGenerator, float value)
        {
           return abstractGenerator.Append(new ParameterFloat(value));
        }
        public static AbstractGenerator Double(this AbstractGenerator abstractGenerator, double value)
        {
           return abstractGenerator.Append(new ParameterDouble(value));
        }
        public static AbstractGenerator Bool(this AbstractGenerator abstractGenerator, bool value)
        {
           return abstractGenerator.Append(new ParameterBool(value));
        }
        public static AbstractGenerator Enum(this AbstractGenerator abstractGenerator, Enum value)
        {
           return abstractGenerator.Append(new ParameterEnum(value));
        }
        public static AbstractGenerator Path(this AbstractGenerator abstractGenerator, GameObject go)
        {
           return abstractGenerator.Append(new ParameterPathToGameObject(go));
        }
    }

    public abstract class AbstractGenerator
    {
        private void ResetReferencesIfDeserializationFailedWithIt(List<AbstractGenerator> sequence)
        {
            if (sequence.Count == 2)
            {
                sequence[0].next = sequence[1];
                sequence[1].prev = sequence[0];
            }
            
            for (int i = 1; i < sequence.Count-1; i++)
            {
                sequence[i].next = sequence[i+1];
                sequence[i].prev = sequence[i-1];
                
                sequence[i - 1].next = sequence[i];
                sequence[i + 1].prev = sequence[i];
            }
        }
        
        public List<AbstractGenerator> CalculateGeneratorSequence()
        {
            List<AbstractGenerator> nextNodes = new List<AbstractGenerator>();
            AbstractGenerator node = this;

            int counter = 0;
            while (node != null)
            {
                counter++;
                if (counter > 1000)
                {
                    Debug.Log(GetType() + "too much interation during searching next elements. break");
                    break;
                }
                nextNodes.Add(node);
                node = node.next;
            }

            List<AbstractGenerator> prevNodes = new List<AbstractGenerator>();
            node = prev;
            counter = 0;
            while (node != null)
            {
                counter++;
                if (counter > 1000)
                {
                    Debug.Log(GetType() + "too much interation during searching previous elements. break");
                    break;
                }
                prevNodes.Add(node);
                node = node.prev;
            }

            List<AbstractGenerator> result = new List<AbstractGenerator>();
            for (int i = prevNodes.Count - 1; i >= 0; i--)
            {
                result.Add(prevNodes[i]);
            }

            result.AddRange(nextNodes);

            ResetReferencesIfDeserializationFailedWithIt(result);
            return result;
        }

        [JsonProperty] private AbstractGenerator next;
        [JsonProperty] private AbstractGenerator prev;
        
        public AbstractGenerator Next
        {
            get { return next; }
        }
        public AbstractGenerator Prev
        {
            get { return prev; }
        }
        
        public static string MethodFullName(MethodInfo methodInfo)
        {
            return TypeFullName(methodInfo.DeclaringType) + methodInfo.Name;
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

        public static AbstractGenerator operator +(AbstractGenerator current, AbstractGenerator next)
        {
            return current.Append(next);
        }

        public AbstractGenerator Append(AbstractGenerator nextNode)
        {
            if (nextNode == this)
            {
                return this;
            }

            next = nextNode;
            nextNode.prev = this;
            return next;
        }

        public string GenerateCode(MethodInfo methodInfo)
        {
            var sb = new StringBuilder();
            var genererators = CalculateGeneratorSequence();
            foreach (var generator in genererators)
            {
                sb.Append(generator.GenerateFromMethodInfo(methodInfo));
            }
            return sb.ToString();
        }

        protected abstract string GenerateFromMethodInfo(MethodInfo methodInfo);

        protected abstract AbstractGenerator MemberwiseClone();

        public AbstractGenerator Clone()
        {
            var sequence = CalculateGeneratorSequence();
            AbstractGenerator prev = null;
            for(var i=0; i < sequence.Count; i++)
            {
                var clone = sequence[i].MemberwiseClone();
                if (prev != null)
                {
                    clone.prev = prev;
                    prev.next = clone;
                }
                prev = clone;
            }
            return prev;
        }
    }

    public class ReturnIEnumerator : AbstractGenerator
    {
        protected override string GenerateFromMethodInfo(MethodInfo methodInfo)
        {
            return "yield return ";
        }

        protected override AbstractGenerator MemberwiseClone()
        {
            return new ReturnIEnumerator();
        }
    }

    public class ReturnVoid : AbstractGenerator
    {
        protected override string GenerateFromMethodInfo(MethodInfo methodInfo)
        {
            return "";
        }

        protected override AbstractGenerator MemberwiseClone()
        {
            return new ReturnVoid();
        }
    }

    public class MethodName : AbstractGenerator
    {
        [JsonProperty] private string methodFullName;

        protected override string GenerateFromMethodInfo(MethodInfo methodInfo)
        {
            methodFullName = TypeFullName(methodInfo.DeclaringType) + "." +  methodInfo.Name;
            var ending = "";
            if (Next == null)
            {
                ending = "();";
            }
            return methodFullName + ending;
        }

        protected override AbstractGenerator MemberwiseClone()
        {
            var clone = new MethodName();
            clone.methodFullName = methodFullName;
            return clone;
        }
    }


    public abstract class AbstractParameter : AbstractGenerator
    {
        public abstract System.Object ParameterValueToObject();
    }
    
    public abstract class AbstractParameter<T> : AbstractParameter
    {
        public T ParameterValue;

        public override object ParameterValueToObject()
        {
            return ParameterValue;
        }

        public int CurrentParameterIndex()
        {
            int previousParamsCount = 0;
            var prevNode = Prev;
            while (prevNode is AbstractParameter)
            {
                prevNode = prevNode.Prev;
                previousParamsCount++;
            }
            
            return previousParamsCount;
        }

        protected bool NextGeneratorIsParameter()
        {
            return Next is AbstractParameter;
        }

        protected bool CheckParameterConsistant(int index, MethodInfo methodInfo)
        {
            var parameters = methodInfo.GetParameters();
            var methodFullName = MethodFullName(methodInfo);
            if (parameters.Length == 0)
            {
                Debug.LogError("you are trying to get generate parametr for method " + methodFullName +
                               " but it has no params at all");
                return false;
            }

            if (parameters.Length <= index)
            {
                Debug.LogError("you are trying to get parameter at index " + index +
                               " but method " + methodFullName +
                               " has only " + parameters.Length + " params");
                return false;
            }
            

            var paramType = parameters[index];
            if (paramType.ParameterType is T)
            {
                Debug.LogError("params miss match: you trying to get at index " + index +
                               " param of type " + typeof(T) +
                               " but method " + methodFullName +
                               " has param of type " + paramType.ParameterType);
                return false;
            }

            return true;
        }

        protected override string GenerateFromMethodInfo(MethodInfo methodInfo)
        {
            var index = CurrentParameterIndex();
            var isConsistent = CheckParameterConsistant(index, methodInfo);
            if (!isConsistent)
            {
                return null;
            }

            var sb = new StringBuilder();
            if (index == 0)
            {
                sb.Append("(");
            }
            else
            {
                sb.Append(", ");
            }

            sb.Append(ParameterValueToCode(methodInfo));

            if (!NextGeneratorIsParameter())
            {
                sb.Append(");");
            }
            return sb.ToString();
        }

        protected abstract string ParameterValueToCode(MethodInfo methodInfo);
    }

    public class ParameterInt : AbstractParameter<int>
    {
        public ParameterInt(int defauleValue)
        {
            ParameterValue = defauleValue;
        }

        protected override string ParameterValueToCode(MethodInfo methodInfo)
        {
            return ParameterValue.ToString();
        }

        protected override AbstractGenerator MemberwiseClone()
        {
            var clone = new ParameterInt(ParameterValue);
            return clone;
        }
    }

    public class ParameterFloat : AbstractParameter<float>
    {
        public ParameterFloat(float defauleValue)
        {
            ParameterValue = defauleValue;
        }

        protected override string ParameterValueToCode(MethodInfo methodInfo)
        {
            return ParameterValue + "f";
        }

        protected override AbstractGenerator MemberwiseClone()
        {
            var clone = new ParameterFloat(ParameterValue);
            return clone;
        }
    }

    public class ParameterDouble : AbstractParameter<double>
    {
        public ParameterDouble(double defauleValue)
        {
            ParameterValue = defauleValue;
        }

        protected override string ParameterValueToCode(MethodInfo methodInfo)
        {
            return ParameterValue + "d";
        }

        protected override AbstractGenerator MemberwiseClone()
        {
            return new ParameterDouble(ParameterValue);
        }
    }

    public class ParameterBool : AbstractParameter<bool>
    {
        public ParameterBool(bool defauleValue)
        {
            ParameterValue = defauleValue;
        }

        protected override string ParameterValueToCode(MethodInfo methodInfo)
        {
            return ParameterValue ? "true" : "false";
        }

        protected override AbstractGenerator MemberwiseClone()
        {
            var clone = new ParameterBool(ParameterValue);
            return clone;
        }
    }

    public class ParameterString : AbstractParameter<string>
    {
        public bool IsRegExp;

        public ParameterString(string defauleValue, bool isRegExp = false)
        {
            IsRegExp = isRegExp;
            ParameterValue = defauleValue;
        }

        protected override string ParameterValueToCode(MethodInfo methodInfo)
        {
            if (IsRegExp)
            {
                var encoded = ParameterValue.Replace("\\", "\\\\");
                return "\"" + encoded + "\"";
            }
            else
            {
                return "\"" + ParameterValue + "\"";
            }
        }

        protected override AbstractGenerator MemberwiseClone()
        {
            var clone = new ParameterString(ParameterValue, IsRegExp);
            return clone;
        }
    }
   
    public class ParameterEnum : AbstractParameter<string>
    {
        [JsonProperty] public Type EnumType { get; private set; }
        [JsonIgnore]public Enum NonSerializedEnum;
        
        public ParameterEnum(Enum defauleValue)
        {
            if (defauleValue != null)
            {
                EnumType = defauleValue.GetType();
                ParameterValue = defauleValue.ToString();
            }
        }
        
        public override object ParameterValueToObject()
        {
            return (Enum)Enum.Parse(EnumType, ParameterValue);
        }

        protected override string ParameterValueToCode(MethodInfo methodInfo)
        {
            return TypeFullName(EnumType) + "." + ParameterValue;
        }

        protected override AbstractGenerator MemberwiseClone()
        {
            var clone = new ParameterEnum(null);
            clone.ParameterValue = ParameterValue;
            clone.EnumType = EnumType;
            return clone;
        }
    }

    public class ParameterPathToGameObject : AbstractParameter<string>
    {
        public ParameterPathToGameObject(GameObject go)
        {
            if (go)
            {
                ParameterValue = UITestUtils.GetGameObjectFullPath(go);   
            }
        }

        protected override string ParameterValueToCode(MethodInfo methodInfo)
        {
            return "\"" + ParameterValue + "\"";
        }

        protected override AbstractGenerator MemberwiseClone()
        {
            var clone = new ParameterPathToGameObject(null);
            clone.ParameterValue = ParameterValue;
            return clone;
        }
    }
}