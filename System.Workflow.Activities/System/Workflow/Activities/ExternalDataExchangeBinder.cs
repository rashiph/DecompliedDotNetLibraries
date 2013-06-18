namespace System.Workflow.Activities
{
    using System;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal sealed class ExternalDataExchangeBinder : Binder
    {
        private Binder defltBinder = Type.DefaultBinder;

        internal ExternalDataExchangeBinder()
        {
        }

        public override FieldInfo BindToField(BindingFlags bindingAttr, FieldInfo[] match, object value, CultureInfo culture)
        {
            return this.defltBinder.BindToField(bindingAttr, match, value, culture);
        }

        public override MethodBase BindToMethod(BindingFlags bindingAttr, MethodBase[] match, ref object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] names, out object state)
        {
            object[] array = new object[args.Length];
            args.CopyTo(array, 0);
            state = null;
            try
            {
                return this.defltBinder.BindToMethod(bindingAttr, match, ref args, modifiers, culture, names, out state);
            }
            catch (MissingMethodException)
            {
                if ((match != null) && (match.Length != 0))
                {
                    for (int i = 0; i < match.Length; i++)
                    {
                        ParameterInfo[] parameters = match[i].GetParameters();
                        if (parameters.Length == array.Length)
                        {
                            for (int j = 0; j < parameters.Length; j++)
                            {
                                if (!parameters[j].ParameterType.IsInstanceOfType(array[j]) && (!parameters[j].ParameterType.IsArray || (array[j] != null)))
                                {
                                    break;
                                }
                                if ((j + 1) == parameters.Length)
                                {
                                    return match[i];
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }

        public override object ChangeType(object value, Type type, CultureInfo culture)
        {
            return this.defltBinder.ChangeType(value, type, culture);
        }

        public override void ReorderArgumentArray(ref object[] args, object state)
        {
            this.defltBinder.ReorderArgumentArray(ref args, state);
        }

        public override MethodBase SelectMethod(BindingFlags bindingAttr, MethodBase[] match, Type[] types, ParameterModifier[] modifiers)
        {
            return this.defltBinder.SelectMethod(bindingAttr, match, types, modifiers);
        }

        public override PropertyInfo SelectProperty(BindingFlags bindingAttr, PropertyInfo[] match, Type returnType, Type[] indexes, ParameterModifier[] modifiers)
        {
            return this.defltBinder.SelectProperty(bindingAttr, match, returnType, indexes, modifiers);
        }
    }
}

