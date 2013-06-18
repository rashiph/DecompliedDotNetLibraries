namespace System.Web.UI
{
    using System;
    using System.Collections;
    using System.ComponentModel;

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ViewStateModeByIdAttribute : Attribute
    {
        private static Hashtable _viewStateIdTypes = Hashtable.Synchronized(new Hashtable());

        internal static bool IsEnabled(Type type)
        {
            if (!_viewStateIdTypes.ContainsKey(type))
            {
                ViewStateModeByIdAttribute attribute = (ViewStateModeByIdAttribute) TypeDescriptor.GetAttributes(type)[typeof(ViewStateModeByIdAttribute)];
                _viewStateIdTypes[type] = attribute != null;
            }
            return (bool) _viewStateIdTypes[type];
        }
    }
}

