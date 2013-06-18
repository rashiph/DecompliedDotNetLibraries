namespace System.Web.UI.WebControls.WebParts
{
    using System;
    using System.Reflection;
    using System.Web.UI;

    public class ProviderConnectionPoint : ConnectionPoint
    {
        internal static readonly Type[] ConstructorTypes;

        static ProviderConnectionPoint()
        {
            ConstructorInfo constructor = typeof(ProviderConnectionPoint).GetConstructors()[0];
            ConstructorTypes = WebPartUtil.GetTypesForConstructor(constructor);
        }

        public ProviderConnectionPoint(MethodInfo callbackMethod, Type interfaceType, Type controlType, string displayName, string id, bool allowsMultipleConnections) : base(callbackMethod, interfaceType, controlType, displayName, id, allowsMultipleConnections)
        {
        }

        public virtual object GetObject(Control control)
        {
            if (control == null)
            {
                throw new ArgumentNullException("control");
            }
            return base.CallbackMethod.Invoke(control, null);
        }

        public virtual ConnectionInterfaceCollection GetSecondaryInterfaces(Control control)
        {
            return ConnectionInterfaceCollection.Empty;
        }
    }
}

