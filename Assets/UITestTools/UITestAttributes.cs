using System;
using UnityEngine;

namespace PlayQ.UITestTools
{
    public class ShowInEditorAttribute : Attribute
    {
        public readonly string Description;
        public readonly Type ClassType;
        public bool IsDefault;
        
        public ShowInEditorAttribute(Type classType, string description, bool isDefault = false)
        {
            if (classType.IsAssignableFrom(typeof(ShowHelperBase)))
            {
                Debug.LogError("Class type for " + description + " is not ShowHelperBase");
                return;
            }
            Description = description;
            ClassType = classType;
            IsDefault = isDefault;
        }
    }
    
    public class RaycastChangerAttribute : Attribute
    {
    } 
}