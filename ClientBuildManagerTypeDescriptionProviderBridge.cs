using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

internal class ClientBuildManagerTypeDescriptionProviderBridge : MarshalByRefObject
{
    private TypeDescriptionProvider _targetFrameworkProvider;

    internal ClientBuildManagerTypeDescriptionProviderBridge(TypeDescriptionProvider typeDescriptionProvider)
    {
        this._targetFrameworkProvider = typeDescriptionProvider;
    }

    internal string[] GetFilteredEvents(Type type, BindingFlags bindingFlags)
    {
        EventInfo[] events = type.GetEvents(bindingFlags);
        if (this._targetFrameworkProvider == null)
        {
            return this.GetMemberNames(events);
        }
        EventInfo[] infoArray2 = this._targetFrameworkProvider.GetReflectionType(type).GetEvents(bindingFlags);
        IEnumerable<string> reflectionEventNames = from e in infoArray2 select e.Name;
        return (from e in events
            where reflectionEventNames.Contains<string>(e.Name)
            select e.Name).ToArray<string>();
    }

    internal string[] GetFilteredProperties(Type type, BindingFlags bindingFlags)
    {
        PropertyInfo[] properties = type.GetProperties(bindingFlags);
        if (this._targetFrameworkProvider == null)
        {
            return this.GetMemberNames(properties);
        }
        PropertyInfo[] infoArray2 = this._targetFrameworkProvider.GetReflectionType(type).GetProperties(bindingFlags);
        IEnumerable<string> reflectionPropertyNames = from p in infoArray2 select p.Name;
        return (from p in properties
            where reflectionPropertyNames.Contains<string>(p.Name)
            select p.Name).ToArray<string>();
    }

    private string[] GetMemberNames(MemberInfo[] members)
    {
        return (from m in members select m.Name).ToArray<string>();
    }

    private Type GetReflectionType(Type type)
    {
        if (type == null)
        {
            return null;
        }
        return this._targetFrameworkProvider.GetReflectionType(type);
    }

    internal bool HasEvent(Type type, string name)
    {
        if (this._targetFrameworkProvider == null)
        {
            return (type.GetEvent(name) != null);
        }
        return (this._targetFrameworkProvider.GetReflectionType(type).GetEvent(name) != null);
    }

    internal bool HasField(Type type, string name, BindingFlags bindingAttr)
    {
        if (this._targetFrameworkProvider == null)
        {
            return (type.GetField(name, bindingAttr) != null);
        }
        return (this._targetFrameworkProvider.GetReflectionType(type).GetField(name, bindingAttr) != null);
    }

    internal bool HasMethod(Type type, string name, BindingFlags bindingAttr)
    {
        Type reflectionType = type;
        if (this._targetFrameworkProvider != null)
        {
            reflectionType = this.GetReflectionType(type);
        }
        return (reflectionType.GetMethod(name, bindingAttr) != null);
    }

    internal bool HasProperty(Type type, string name, BindingFlags bindingAttr)
    {
        if (this._targetFrameworkProvider == null)
        {
            return (type.GetProperty(name, bindingAttr) != null);
        }
        return (this.GetReflectionType(type).GetProperty(name, bindingAttr) != null);
    }

    public override object InitializeLifetimeService()
    {
        return null;
    }
}

