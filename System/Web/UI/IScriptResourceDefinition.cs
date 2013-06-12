namespace System.Web.UI
{
    using System;
    using System.Reflection;

    internal interface IScriptResourceDefinition
    {
        string CdnDebugPath { get; }

        string CdnDebugPathSecureConnection { get; }

        string CdnPath { get; }

        string CdnPathSecureConnection { get; }

        string DebugPath { get; }

        string Path { get; }

        Assembly ResourceAssembly { get; }

        string ResourceName { get; }
    }
}

