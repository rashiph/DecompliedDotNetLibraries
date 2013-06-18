namespace System.Web.UI.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Configuration;
    using System.Design;
    using System.Globalization;

    public class AppSettingsExpressionEditor : ExpressionEditor
    {
        public override object EvaluateExpression(string expression, object parseTimeData, Type propertyType, IServiceProvider serviceProvider)
        {
            KeyValueConfigurationCollection appSettings = this.GetAppSettings(serviceProvider);
            if (appSettings != null)
            {
                KeyValueConfigurationElement element = appSettings[expression];
                if (element != null)
                {
                    return element.Value;
                }
            }
            return null;
        }

        private KeyValueConfigurationCollection GetAppSettings(IServiceProvider serviceProvider)
        {
            if (serviceProvider != null)
            {
                IWebApplication service = (IWebApplication) serviceProvider.GetService(typeof(IWebApplication));
                if (service != null)
                {
                    System.Configuration.Configuration configuration = service.OpenWebConfiguration(true);
                    if (configuration != null)
                    {
                        AppSettingsSection appSettings = configuration.AppSettings;
                        if (appSettings != null)
                        {
                            return appSettings.Settings;
                        }
                    }
                }
            }
            return null;
        }

        public override ExpressionEditorSheet GetExpressionEditorSheet(string expression, IServiceProvider serviceProvider)
        {
            return new AppSettingsExpressionEditorSheet(expression, this, serviceProvider);
        }

        private class AppSettingsExpressionEditorSheet : ExpressionEditorSheet
        {
            private string _appSetting;
            private AppSettingsExpressionEditor _owner;

            public AppSettingsExpressionEditorSheet(string expression, AppSettingsExpressionEditor owner, IServiceProvider serviceProvider) : base(serviceProvider)
            {
                this._owner = owner;
                this._appSetting = expression;
            }

            public override string GetExpression()
            {
                return this._appSetting;
            }

            [System.Design.SRDescription("AppSettingExpressionEditor_AppSetting"), DefaultValue(""), TypeConverter(typeof(AppSettingsTypeConverter))]
            public string AppSetting
            {
                get
                {
                    return this._appSetting;
                }
                set
                {
                    this._appSetting = value;
                }
            }

            public override bool IsValid
            {
                get
                {
                    return !string.IsNullOrEmpty(this.AppSetting);
                }
            }

            private class AppSettingsTypeConverter : TypeConverter
            {
                private static readonly string NoAppSetting = "(None)";

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
                    if (!(value is string))
                    {
                        return base.ConvertFrom(context, culture, value);
                    }
                    if (string.Equals((string) value, NoAppSetting, StringComparison.OrdinalIgnoreCase))
                    {
                        return string.Empty;
                    }
                    return value;
                }

                public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
                {
                    if (!(value is string))
                    {
                        return base.ConvertTo(context, culture, value, destinationType);
                    }
                    if (((string) value).Length == 0)
                    {
                        return NoAppSetting;
                    }
                    return value;
                }

                public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
                {
                    if (context != null)
                    {
                        AppSettingsExpressionEditor.AppSettingsExpressionEditorSheet instance = (AppSettingsExpressionEditor.AppSettingsExpressionEditorSheet) context.Instance;
                        KeyValueConfigurationCollection appSettings = instance._owner.GetAppSettings(instance.ServiceProvider);
                        if (appSettings != null)
                        {
                            ArrayList values = new ArrayList(appSettings.AllKeys);
                            values.Sort();
                            values.Add(string.Empty);
                            return new TypeConverter.StandardValuesCollection(values);
                        }
                    }
                    return base.GetStandardValues(context);
                }

                public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
                {
                    return false;
                }

                public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
                {
                    if (context != null)
                    {
                        AppSettingsExpressionEditor.AppSettingsExpressionEditorSheet instance = (AppSettingsExpressionEditor.AppSettingsExpressionEditorSheet) context.Instance;
                        KeyValueConfigurationCollection appSettings = instance._owner.GetAppSettings(instance.ServiceProvider);
                        if (appSettings != null)
                        {
                            return (appSettings.Count > 0);
                        }
                    }
                    return base.GetStandardValuesSupported(context);
                }
            }
        }
    }
}

