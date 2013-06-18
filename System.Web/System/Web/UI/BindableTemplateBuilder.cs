namespace System.Web.UI
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel.Design;
    using System.Web.Util;

    public sealed class BindableTemplateBuilder : TemplateBuilder, IBindableTemplate, ITemplate
    {
        private System.Web.UI.ExtractTemplateValuesMethod _extractTemplateValuesMethod;

        private IOrderedDictionary ExtractTemplateValuesMethod(Control container)
        {
            BindableTemplateBuilder builder = this;
            OrderedDictionary table = new OrderedDictionary();
            if (builder != null)
            {
                this.ExtractTemplateValuesRecursive(builder.SubBuilders, table, container);
            }
            return table;
        }

        private void ExtractTemplateValuesRecursive(ArrayList subBuilders, OrderedDictionary table, Control container)
        {
            foreach (object obj2 in subBuilders)
            {
                ControlBuilder builder = obj2 as ControlBuilder;
                if (builder != null)
                {
                    ICollection boundPropertyEntries;
                    if (!builder.HasFilteredBoundEntries)
                    {
                        boundPropertyEntries = builder.BoundPropertyEntries;
                    }
                    else
                    {
                        ServiceContainer serviceProvider = new ServiceContainer();
                        serviceProvider.AddService(typeof(IFilterResolutionService), builder.TemplateControl);
                        try
                        {
                            builder.SetServiceProvider(serviceProvider);
                            boundPropertyEntries = builder.GetFilteredPropertyEntrySet(builder.BoundPropertyEntries);
                        }
                        finally
                        {
                            builder.SetServiceProvider(null);
                        }
                    }
                    string strA = null;
                    bool flag = true;
                    Control o = null;
                    foreach (BoundPropertyEntry entry in boundPropertyEntries)
                    {
                        if (entry.TwoWayBound)
                        {
                            string str2;
                            if (string.Compare(strA, entry.ControlID, StringComparison.Ordinal) != 0)
                            {
                                flag = true;
                            }
                            else
                            {
                                flag = false;
                            }
                            strA = entry.ControlID;
                            if (flag)
                            {
                                o = container.FindControl(entry.ControlID);
                                if ((o == null) || !entry.ControlType.IsInstanceOfType(o))
                                {
                                    continue;
                                }
                            }
                            object target = PropertyMapper.LocatePropertyObject(o, entry.Name, out str2, base.InDesigner);
                            table[entry.FieldName] = FastPropertyAccessor.GetProperty(target, str2, base.InDesigner);
                        }
                    }
                    this.ExtractTemplateValuesRecursive(builder.SubBuilders, table, container);
                }
            }
        }

        public IOrderedDictionary ExtractValues(Control container)
        {
            if ((this._extractTemplateValuesMethod != null) && !base.InDesigner)
            {
                return this._extractTemplateValuesMethod(container);
            }
            return new OrderedDictionary();
        }

        public override void OnAppendToParentBuilder(ControlBuilder parentBuilder)
        {
            base.OnAppendToParentBuilder(parentBuilder);
            if (base.HasTwoWayBoundProperties)
            {
                this._extractTemplateValuesMethod = new System.Web.UI.ExtractTemplateValuesMethod(this.ExtractTemplateValuesMethod);
            }
        }
    }
}

