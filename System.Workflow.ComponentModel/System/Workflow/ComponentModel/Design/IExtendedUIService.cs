namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    public interface IExtendedUIService
    {
        void AddAssemblyReference(AssemblyName assemblyName);
        void AddDesignerActions(DesignerAction[] actions);
        DialogResult AddWebReference(out Uri url, out System.Type proxyClass);
        System.Type GetProxyClassForUrl(Uri url);
        ITypeDescriptorContext GetSelectedPropertyContext();
        Uri GetUrlForProxyClass(System.Type proxyClass);
        Dictionary<string, System.Type> GetXsdProjectItemsInfo();
        bool NavigateToProperty(string propName);
        void RemoveDesignerActions();
        void ShowToolsOptions();
    }
}

