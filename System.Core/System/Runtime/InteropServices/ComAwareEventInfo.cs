namespace System.Runtime.InteropServices
{
    using System;
    using System.Reflection;
    using System.Security;
    using System.Security.Permissions;

    [SecuritySafeCritical]
    public class ComAwareEventInfo : EventInfo
    {
        private EventInfo _innerEventInfo;

        public ComAwareEventInfo(Type type, string eventName)
        {
            this._innerEventInfo = type.GetEvent(eventName);
        }

        public override void AddEventHandler(object target, Delegate handler)
        {
            if (Marshal.IsComObject(target))
            {
                Guid guid;
                int num;
                GetDataForComInvocation(this._innerEventInfo, out guid, out num);
                new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
                ComEventsHelper.Combine(target, guid, num, handler);
            }
            else
            {
                this._innerEventInfo.AddEventHandler(target, handler);
            }
        }

        public override MethodInfo GetAddMethod(bool nonPublic)
        {
            return this._innerEventInfo.GetAddMethod(nonPublic);
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            return this._innerEventInfo.GetCustomAttributes(inherit);
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return this._innerEventInfo.GetCustomAttributes(attributeType, inherit);
        }

        private static void GetDataForComInvocation(EventInfo eventInfo, out Guid sourceIid, out int dispid)
        {
            object[] customAttributes = eventInfo.DeclaringType.GetCustomAttributes(typeof(ComEventInterfaceAttribute), false);
            if ((customAttributes == null) || (customAttributes.Length == 0))
            {
                throw new InvalidOperationException("event invocation for COM objects requires interface to be attributed with ComSourceInterfaceGuidAttribute");
            }
            if (customAttributes.Length > 1)
            {
                throw new AmbiguousMatchException("more than one ComSourceInterfaceGuidAttribute found");
            }
            Type sourceInterface = ((ComEventInterfaceAttribute) customAttributes[0]).SourceInterface;
            Guid gUID = sourceInterface.GUID;
            Attribute customAttribute = Attribute.GetCustomAttribute(sourceInterface.GetMethod(eventInfo.Name), typeof(DispIdAttribute));
            if (customAttribute == null)
            {
                throw new InvalidOperationException("event invocation for COM objects requires event to be attributed with DispIdAttribute");
            }
            sourceIid = gUID;
            dispid = ((DispIdAttribute) customAttribute).Value;
        }

        public override MethodInfo GetRaiseMethod(bool nonPublic)
        {
            return this._innerEventInfo.GetRaiseMethod(nonPublic);
        }

        public override MethodInfo GetRemoveMethod(bool nonPublic)
        {
            return this._innerEventInfo.GetRemoveMethod(nonPublic);
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return this._innerEventInfo.IsDefined(attributeType, inherit);
        }

        public override void RemoveEventHandler(object target, Delegate handler)
        {
            if (Marshal.IsComObject(target))
            {
                Guid guid;
                int num;
                GetDataForComInvocation(this._innerEventInfo, out guid, out num);
                new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
                ComEventsHelper.Remove(target, guid, num, handler);
            }
            else
            {
                this._innerEventInfo.RemoveEventHandler(target, handler);
            }
        }

        public override EventAttributes Attributes
        {
            get
            {
                return this._innerEventInfo.Attributes;
            }
        }

        public override Type DeclaringType
        {
            get
            {
                return this._innerEventInfo.DeclaringType;
            }
        }

        public override string Name
        {
            get
            {
                return this._innerEventInfo.Name;
            }
        }

        public override Type ReflectedType
        {
            get
            {
                return this._innerEventInfo.ReflectedType;
            }
        }
    }
}

