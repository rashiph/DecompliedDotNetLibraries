namespace Microsoft.VisualBasic.Activities
{
    using Microsoft.VisualBasic.Activities.XamlIntegration;
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Markup;
    using System.Xaml;

    [ValueSerializer(typeof(VisualBasicSettingsValueSerializer)), TypeConverter(typeof(VisualBasicSettingsConverter))]
    public class VisualBasicSettings
    {
        private static readonly System.Collections.Generic.HashSet<VisualBasicImportReference> defaultImportReferences;
        private static VisualBasicSettings defaultSettings;

        static VisualBasicSettings()
        {
            System.Collections.Generic.HashSet<VisualBasicImportReference> set = new System.Collections.Generic.HashSet<VisualBasicImportReference>();
            VisualBasicImportReference item = new VisualBasicImportReference {
                Import = "System",
                Assembly = "mscorlib"
            };
            set.Add(item);
            VisualBasicImportReference reference2 = new VisualBasicImportReference {
                Import = "System.Collections",
                Assembly = "mscorlib"
            };
            set.Add(reference2);
            VisualBasicImportReference reference3 = new VisualBasicImportReference {
                Import = "System.Collections.Generic",
                Assembly = "mscorlib"
            };
            set.Add(reference3);
            VisualBasicImportReference reference4 = new VisualBasicImportReference {
                Import = "System",
                Assembly = "system"
            };
            set.Add(reference4);
            VisualBasicImportReference reference5 = new VisualBasicImportReference {
                Import = "System.Collections.Generic",
                Assembly = "system"
            };
            set.Add(reference5);
            VisualBasicImportReference reference6 = new VisualBasicImportReference {
                Import = "System.Activities",
                Assembly = "System.Activities"
            };
            set.Add(reference6);
            VisualBasicImportReference reference7 = new VisualBasicImportReference {
                Import = "System.Activities.Statements",
                Assembly = "System.Activities"
            };
            set.Add(reference7);
            VisualBasicImportReference reference8 = new VisualBasicImportReference {
                Import = "System.Activities.Expressions",
                Assembly = "System.Activities"
            };
            set.Add(reference8);
            defaultImportReferences = set;
            defaultSettings = new VisualBasicSettings(defaultImportReferences);
        }

        public VisualBasicSettings()
        {
            this.ImportReferences = new System.Collections.Generic.HashSet<VisualBasicImportReference>();
        }

        private VisualBasicSettings(System.Collections.Generic.HashSet<VisualBasicImportReference> importReferences)
        {
            this.ImportReferences = new System.Collections.Generic.HashSet<VisualBasicImportReference>(importReferences);
        }

        internal void GenerateXamlReferences(IValueSerializerContext context)
        {
            INamespacePrefixLookup service = GetService<INamespacePrefixLookup>(context);
            foreach (VisualBasicImportReference reference in this.ImportReferences)
            {
                reference.GenerateXamlNamespace(service);
            }
        }

        internal static T GetService<T>(ITypeDescriptorContext context) where T: class
        {
            T service = (T) context.GetService(typeof(T));
            if (service == null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.InvalidTypeConverterUsage));
            }
            return service;
        }

        public static VisualBasicSettings Default
        {
            get
            {
                return defaultSettings;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ISet<VisualBasicImportReference> ImportReferences { get; private set; }

        internal bool SuppressXamlSerialization { get; set; }
    }
}

