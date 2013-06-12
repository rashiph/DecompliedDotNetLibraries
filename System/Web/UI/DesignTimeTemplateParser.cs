namespace System.Web.UI
{
    using System;
    using System.Collections;
    using System.ComponentModel.Design;
    using System.Security.Permissions;
    using System.Web;
    using System.Web.Util;

    public static class DesignTimeTemplateParser
    {
        public static Control ParseControl(DesignTimeParseData data)
        {
            Control[] controlArray = ParseControlsInternal(data, true);
            if (controlArray.Length > 0)
            {
                return controlArray[0];
            }
            return null;
        }

        public static Control[] ParseControls(DesignTimeParseData data)
        {
            return ParseControlsInternal(data, false);
        }

        [SecurityPermission(SecurityAction.Demand, Unrestricted=true)]
        internal static Control[] ParseControlsInternal(DesignTimeParseData data, bool returnFirst)
        {
            Control[] controlArray;
            try
            {
                if (data.DesignerHost != null)
                {
                    TargetFrameworkUtil.DesignerHost = data.DesignerHost;
                }
                controlArray = ParseControlsInternalHelper(data, returnFirst);
            }
            finally
            {
                TargetFrameworkUtil.DesignerHost = null;
            }
            return controlArray;
        }

        private static Control[] ParseControlsInternalHelper(DesignTimeParseData data, bool returnFirst)
        {
            TemplateParser parser = new PageParser {
                FInDesigner = true,
                DesignerHost = data.DesignerHost,
                DesignTimeDataBindHandler = data.DataBindingHandler,
                Text = data.ParseText
            };
            parser.Parse();
            ArrayList list = new ArrayList();
            ArrayList subBuilders = parser.RootBuilder.SubBuilders;
            if (subBuilders != null)
            {
                IEnumerator enumerator = subBuilders.GetEnumerator();
                for (int i = 0; enumerator.MoveNext(); i++)
                {
                    object current = enumerator.Current;
                    if ((current is ControlBuilder) && !(current is CodeBlockBuilder))
                    {
                        ControlBuilder builder = (ControlBuilder) current;
                        IServiceProvider serviceProvider = null;
                        if (data.DesignerHost != null)
                        {
                            serviceProvider = data.DesignerHost;
                        }
                        else
                        {
                            ServiceContainer container = new ServiceContainer();
                            container.AddService(typeof(IFilterResolutionService), new SimpleDesignTimeFilterResolutionService(data.Filter));
                            serviceProvider = container;
                        }
                        builder.SetServiceProvider(serviceProvider);
                        try
                        {
                            Control control = (Control) builder.BuildObject(data.ShouldApplyTheme);
                            list.Add(control);
                        }
                        finally
                        {
                            builder.SetServiceProvider(null);
                        }
                        if (!returnFirst)
                        {
                            continue;
                        }
                        break;
                    }
                    if (!returnFirst && (current is string))
                    {
                        LiteralControl control2 = new LiteralControl(current.ToString());
                        list.Add(control2);
                    }
                }
            }
            data.SetUserControlRegisterEntries(parser.UserControlRegisterEntries, parser.TagRegisterEntries);
            return (Control[]) list.ToArray(typeof(Control));
        }

        [SecurityPermission(SecurityAction.Demand, Unrestricted=true)]
        public static ITemplate ParseTemplate(DesignTimeParseData data)
        {
            TemplateParser parser = new PageParser {
                FInDesigner = true,
                DesignerHost = data.DesignerHost,
                DesignTimeDataBindHandler = data.DataBindingHandler,
                Text = data.ParseText
            };
            parser.Parse();
            parser.RootBuilder.Text = data.ParseText;
            parser.RootBuilder.SetDesignerHost(data.DesignerHost);
            return parser.RootBuilder;
        }

        public static ControlBuilder ParseTheme(IDesignerHost host, string theme, string themePath)
        {
            ControlBuilder rootBuilder;
            try
            {
                TemplateParser parser = new DesignTimePageThemeParser(themePath) {
                    FInDesigner = true,
                    DesignerHost = host,
                    ThrowOnFirstParseError = true,
                    Text = theme
                };
                parser.Parse();
                rootBuilder = parser.RootBuilder;
            }
            catch (Exception exception)
            {
                throw new Exception(System.Web.SR.GetString("DesignTimeTemplateParser_ErrorParsingTheme") + " " + exception.Message);
            }
            return rootBuilder;
        }

        private class SimpleDesignTimeFilterResolutionService : IFilterResolutionService
        {
            private string _currentFilter;

            public SimpleDesignTimeFilterResolutionService(string filter)
            {
                this._currentFilter = filter;
            }

            int IFilterResolutionService.CompareFilters(string filter1, string filter2)
            {
                if (string.IsNullOrEmpty(filter1))
                {
                    if (!string.IsNullOrEmpty(filter2))
                    {
                        return 1;
                    }
                    return 0;
                }
                if (string.IsNullOrEmpty(filter2))
                {
                    return -1;
                }
                return 0;
            }

            bool IFilterResolutionService.EvaluateFilter(string filterName)
            {
                return (string.IsNullOrEmpty(filterName) || StringUtil.EqualsIgnoreCase((this._currentFilter == null) ? string.Empty : this._currentFilter, filterName));
            }
        }
    }
}

