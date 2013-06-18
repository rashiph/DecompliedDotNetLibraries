namespace Microsoft.JScript
{
    using System;
    using System.Reflection;

    public interface IActivationObject
    {
        object GetDefaultThisObject();
        FieldInfo GetField(string name, int lexLevel);
        GlobalScope GetGlobalScope();
        FieldInfo GetLocalField(string name);
        object GetMemberValue(string name, int lexlevel);
    }
}

