namespace System.Web.UI.WebControls
{
    using System;
    using System.Collections.Specialized;
    using System.Web;
    using System.Web.UI;

    internal static class DataBoundControlHelper
    {
        public static bool CompareStringArrays(string[] stringA, string[] stringB)
        {
            if ((stringA != null) || (stringB != null))
            {
                if ((stringA == null) || (stringB == null))
                {
                    return false;
                }
                if (stringA.Length != stringB.Length)
                {
                    return false;
                }
                for (int i = 0; i < stringA.Length; i++)
                {
                    if (!string.Equals(stringA[i], stringB[i], StringComparison.Ordinal))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        internal static void ExtractValuesFromBindableControls(IOrderedDictionary dictionary, Control container)
        {
            IBindableControl control = container as IBindableControl;
            if (control != null)
            {
                control.ExtractValues(dictionary);
            }
            foreach (Control control2 in container.Controls)
            {
                ExtractValuesFromBindableControls(dictionary, control2);
            }
        }

        public static Control FindControl(Control control, string controlID)
        {
            Control namingContainer = control;
            Control control3 = null;
            if (control != control.Page)
            {
                while ((control3 == null) && (namingContainer != control.Page))
                {
                    namingContainer = namingContainer.NamingContainer;
                    if (namingContainer == null)
                    {
                        throw new HttpException(System.Web.SR.GetString("DataBoundControlHelper_NoNamingContainer", new object[] { control.GetType().Name, control.ID }));
                    }
                    control3 = namingContainer.FindControl(controlID);
                }
                return control3;
            }
            return control.FindControl(controlID);
        }

        public static bool IsBindableType(Type type)
        {
            if (type == null)
            {
                return false;
            }
            Type underlyingType = Nullable.GetUnderlyingType(type);
            if (underlyingType != null)
            {
                type = underlyingType;
            }
            if (((!type.IsPrimitive && !(type == typeof(string))) && (!(type == typeof(DateTime)) && !(type == typeof(decimal)))) && (!(type == typeof(Guid)) && !(type == typeof(DateTimeOffset))))
            {
                return (type == typeof(TimeSpan));
            }
            return true;
        }
    }
}

