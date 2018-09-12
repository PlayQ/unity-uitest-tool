using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PlayQ.UITestTools
{
    public class ParameterDrawersFactory
    {
        private Dictionary<Type, AbstractParameterDrawer> drawersByParameterType;

        public ParameterDrawersFactory()
        {
            drawersByParameterType = new Dictionary<Type, AbstractParameterDrawer>();
            var types = GetType().Assembly.GetTypes();
            var drawers = types.Where(type =>
            {
                if (type.IsGenericType || type.IsAbstract)
                {
                    return false;
                }
                return typeof(AbstractParameterDrawer).IsAssignableFrom(type);
            }).ToList();
            
            foreach (var drawerType in drawers)
            {
                var drawer = Activator.CreateInstance(drawerType) as AbstractParameterDrawer;
                drawersByParameterType.Add(drawer.ExpectedType, drawer);
            }
        }
        
        public AbstractParameterDrawer GetDrawer(AbstractParameter parameter)
        {
            AbstractParameterDrawer paramDrawer;
            var result = drawersByParameterType.TryGetValue(parameter.GetType(), out paramDrawer);
            if (!result)
            {
                Debug.LogError("Fail to get ParamDrawer for type: " + parameter.GetType() +
                               " probably drawer is not implemented or not added to factory.");
            }
            return paramDrawer;
        }
    }
}