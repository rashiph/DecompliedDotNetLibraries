namespace System.Web.UI.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Globalization;
    using System.Web.UI;

    public class SkinIDTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return ((sourceType == typeof(string)) || base.CanConvertFrom(context, sourceType));
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destType)
        {
            return ((destType == typeof(string)) || base.CanConvertTo(context, destType));
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                return value;
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (value is string)
            {
                return value;
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            if (context == null)
            {
                return new TypeConverter.StandardValuesCollection(new ArrayList());
            }
            Control instance = context.Instance as Control;
            ArrayList values = new ArrayList();
            if ((instance != null) && (instance.Site != null))
            {
                IThemeResolutionService service = (IThemeResolutionService) instance.Site.GetService(typeof(IThemeResolutionService));
                ThemeProvider stylesheetThemeProvider = service.GetStylesheetThemeProvider();
                ThemeProvider themeProvider = service.GetThemeProvider();
                if (stylesheetThemeProvider != null)
                {
                    values.AddRange(stylesheetThemeProvider.GetSkinsForControl(instance.GetType()));
                    values.Remove(string.Empty);
                }
                if (themeProvider != null)
                {
                    foreach (string str in themeProvider.GetSkinsForControl(instance.GetType()))
                    {
                        if (!values.Contains(str))
                        {
                            values.Add(str);
                        }
                    }
                    values.Remove(string.Empty);
                }
                values.Sort();
            }
            return new TypeConverter.StandardValuesCollection(values);
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            ThemeProvider themeProvider = null;
            if (context != null)
            {
                Control instance = context.Instance as Control;
                if ((instance != null) && (instance.Site != null))
                {
                    IThemeResolutionService service = (IThemeResolutionService) instance.Site.GetService(typeof(IThemeResolutionService));
                    if (service != null)
                    {
                        themeProvider = service.GetThemeProvider();
                        if (themeProvider == null)
                        {
                            themeProvider = service.GetStylesheetThemeProvider();
                        }
                    }
                }
            }
            return (themeProvider != null);
        }
    }
}

