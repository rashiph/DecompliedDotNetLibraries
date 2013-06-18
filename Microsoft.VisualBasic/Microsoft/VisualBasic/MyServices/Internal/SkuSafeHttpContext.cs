namespace Microsoft.VisualBasic.MyServices.Internal
{
    using System;
    using System.ComponentModel;
    using System.Reflection;

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class SkuSafeHttpContext
    {
        private static PropertyInfo m_HttpContextCurrent = InitContext();

        private SkuSafeHttpContext()
        {
        }

        private static PropertyInfo InitContext()
        {
            Type type = Type.GetType("System.Web.HttpContext,System.Web,Version=4.0.0.0,Culture=neutral,PublicKeyToken=B03F5F7F11D50A3A");
            if (type != null)
            {
                return type.GetProperty("Current");
            }
            return null;
        }

        public static object Current
        {
            get
            {
                if (m_HttpContextCurrent != null)
                {
                    return m_HttpContextCurrent.GetValue(null, null);
                }
                return null;
            }
        }
    }
}

