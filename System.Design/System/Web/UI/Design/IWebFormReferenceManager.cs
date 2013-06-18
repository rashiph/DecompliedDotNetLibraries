namespace System.Web.UI.Design
{
    using System;

    [Obsolete("The recommended alternative is System.Web.UI.Design.WebFormsReferenceManager. The WebFormsReferenceManager contains additional functionality and allows for more extensibility. To get the WebFormsReferenceManager use the RootDesigner.ReferenceManager property from your ControlDesigner. http://go.microsoft.com/fwlink/?linkid=14202")]
    public interface IWebFormReferenceManager
    {
        Type GetObjectType(string tagPrefix, string typeName);
        string GetRegisterDirectives();
        string GetTagPrefix(Type objectType);
    }
}

