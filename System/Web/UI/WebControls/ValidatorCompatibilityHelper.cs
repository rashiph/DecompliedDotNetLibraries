namespace System.Web.UI.WebControls
{
    using System;
    using System.Reflection;
    using System.Web.UI;

    internal static class ValidatorCompatibilityHelper
    {
        public static void RegisterArrayDeclaration(Control control, string arrayName, string arrayValue)
        {
            control.Page.ScriptManagerType.InvokeMember("RegisterArrayDeclaration", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static, null, null, new object[] { control, arrayName, arrayValue });
        }

        public static void RegisterClientScriptResource(Control control, Type type, string resourceName)
        {
            control.Page.ScriptManagerType.InvokeMember("RegisterClientScriptResource", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static, null, null, new object[] { control, type, resourceName });
        }

        public static void RegisterExpandoAttribute(Control control, string controlId, string attributeName, string attributeValue, bool encode)
        {
            control.Page.ScriptManagerType.InvokeMember("RegisterExpandoAttribute", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static, null, null, new object[] { control, controlId, attributeName, attributeValue, encode });
        }

        public static void RegisterOnSubmitStatement(Control control, Type type, string key, string script)
        {
            control.Page.ScriptManagerType.InvokeMember("RegisterOnSubmitStatement", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static, null, null, new object[] { control, type, key, script });
        }

        public static void RegisterStartupScript(Control control, Type type, string key, string script, bool addScriptTags)
        {
            control.Page.ScriptManagerType.InvokeMember("RegisterStartupScript", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static, null, null, new object[] { control, type, key, script, addScriptTags });
        }
    }
}

