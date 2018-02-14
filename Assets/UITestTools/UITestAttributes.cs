using System;

namespace PlayQ.UITestTools
{
    public class ShowInEditorAttribute : Attribute
    {
        public readonly string Descryption;
        public readonly Type ClassType;
        public bool PathIsPresent;
        public bool IsDefault;
        
        public ShowInEditorAttribute(Type classType, string description, bool pathIsPresent = true, bool isDefault = false)
        {
            Descryption = description;
            ClassType = classType;
            PathIsPresent = pathIsPresent;
            IsDefault = isDefault;
        }
        public ShowInEditorAttribute(string description, bool pathInPresent = true, bool isDefault = false)
        {
            Descryption = description;
            PathIsPresent = pathInPresent;
            IsDefault = isDefault;
        }
    } 
    
    public class RaycastChangerAttribute : Attribute
    {
    } 
}