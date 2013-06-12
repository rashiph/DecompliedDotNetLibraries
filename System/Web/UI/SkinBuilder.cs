namespace System.Web.UI
{
    using System;
    using System.ComponentModel;
    using System.Web;
    using System.Web.Util;

    public sealed class SkinBuilder : ControlBuilder
    {
        private Control _control;
        private ThemeProvider _provider;
        private ControlBuilder _skinBuilder;
        private string _themePath;
        internal static readonly object[] EmptyParams = new object[0];

        public SkinBuilder(ThemeProvider provider, Control control, ControlBuilder skinBuilder, string themePath)
        {
            this._provider = provider;
            this._control = control;
            this._skinBuilder = skinBuilder;
            this._themePath = themePath;
        }

        private void ApplyBoundProperties(Control control)
        {
            DataBindingCollection dataBindings = null;
            IAttributeAccessor attributeAccessor = null;
            foreach (BoundPropertyEntry entry in base.GetFilteredPropertyEntrySet(this._skinBuilder.BoundPropertyEntries))
            {
                this.InitBoundProperty(control, entry, ref dataBindings, ref attributeAccessor);
            }
        }

        private void ApplyComplexProperties(Control control)
        {
            foreach (ComplexPropertyEntry entry in base.GetFilteredPropertyEntrySet(this._skinBuilder.ComplexPropertyEntries))
            {
                if (entry.Builder != null)
                {
                    object obj3;
                    string str2;
                    string name = entry.Name;
                    if (entry.ReadOnly)
                    {
                        object obj2 = FastPropertyAccessor.GetProperty(control, name, base.InDesigner);
                        if (obj2 == null)
                        {
                            continue;
                        }
                        entry.Builder.SetServiceProvider(base.ServiceProvider);
                        try
                        {
                            entry.Builder.InitObject(obj2);
                            continue;
                        }
                        finally
                        {
                            entry.Builder.SetServiceProvider(null);
                        }
                    }
                    object val = entry.Builder.BuildObject(true);
                    PropertyDescriptor descriptor = PropertyMapper.GetMappedPropertyDescriptor(control, PropertyMapper.MapNameToPropertyName(name), out obj3, out str2, base.InDesigner);
                    if (descriptor != null)
                    {
                        string virtualPath = val as string;
                        if (((val != null) && (descriptor.Attributes[typeof(UrlPropertyAttribute)] != null)) && UrlPath.IsRelativeUrl(virtualPath))
                        {
                            val = this._themePath + virtualPath;
                        }
                    }
                    FastPropertyAccessor.SetProperty(obj3, name, val, base.InDesigner);
                }
            }
        }

        private void ApplySimpleProperties(Control control)
        {
            foreach (SimplePropertyEntry entry in base.GetFilteredPropertyEntrySet(this._skinBuilder.SimplePropertyEntries))
            {
                try
                {
                    if (entry.UseSetAttribute)
                    {
                        base.SetSimpleProperty(entry, control);
                    }
                    else
                    {
                        object obj2;
                        string str2;
                        string mappedName = PropertyMapper.MapNameToPropertyName(entry.Name);
                        PropertyDescriptor descriptor = PropertyMapper.GetMappedPropertyDescriptor(control, mappedName, out obj2, out str2, base.InDesigner);
                        if (descriptor != null)
                        {
                            DefaultValueAttribute attribute = (DefaultValueAttribute) descriptor.Attributes[typeof(DefaultValueAttribute)];
                            object objB = descriptor.GetValue(obj2);
                            if ((attribute == null) || object.Equals(attribute.Value, objB))
                            {
                                object obj4 = entry.Value;
                                string virtualPath = obj4 as string;
                                if (((obj4 != null) && (descriptor.Attributes[typeof(UrlPropertyAttribute)] != null)) && UrlPath.IsRelativeUrl(virtualPath))
                                {
                                    obj4 = this._themePath + virtualPath;
                                }
                                base.SetSimpleProperty(entry, control);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                }
                catch
                {
                }
            }
        }

        private void ApplyTemplateProperties(Control control)
        {
            object[] parameters = new object[1];
            foreach (TemplatePropertyEntry entry in base.GetFilteredPropertyEntrySet(this._skinBuilder.TemplatePropertyEntries))
            {
                try
                {
                    if (FastPropertyAccessor.GetProperty(control, entry.Name, base.InDesigner) == null)
                    {
                        ControlBuilder builder = entry.Builder;
                        builder.SetServiceProvider(base.ServiceProvider);
                        try
                        {
                            object obj3 = builder.BuildObject(true);
                            parameters[0] = obj3;
                        }
                        finally
                        {
                            builder.SetServiceProvider(null);
                        }
                        Util.InvokeMethod(entry.PropertyInfo.GetSetMethod(), control, parameters);
                    }
                }
                catch (Exception)
                {
                }
                catch
                {
                }
            }
        }

        public Control ApplyTheme()
        {
            if (this._skinBuilder != null)
            {
                this.ApplySimpleProperties(this._control);
                this.ApplyComplexProperties(this._control);
                this.ApplyBoundProperties(this._control);
                this.ApplyTemplateProperties(this._control);
            }
            return this._control;
        }

        private void InitBoundProperty(Control control, BoundPropertyEntry entry, ref DataBindingCollection dataBindings, ref IAttributeAccessor attributeAccessor)
        {
            if (entry.ExpressionPrefix.Length != 0)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("ControlBuilder_ExpressionsNotAllowedInThemes"));
            }
            if ((dataBindings == null) && (control != null))
            {
                dataBindings = ((IDataBindingsAccessor) control).DataBindings;
            }
            dataBindings.Add(new DataBinding(entry.Name, entry.Type, entry.Expression.Trim()));
        }
    }
}

