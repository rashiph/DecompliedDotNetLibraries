namespace System.Web.UI
{
    using System;
    using System.Reflection;

    internal interface IScriptResourceMapping
    {
        IScriptResourceDefinition GetDefinition(string resourceName, Assembly resourceAssembly);
    }
}

