namespace System.Workflow.Activities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Workflow.ComponentModel;
    using System.Workflow.Runtime;

    internal sealed class CorrelationTokenTypeConverter : ExpandableObjectConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return (sourceType == typeof(string));
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return (destinationType == typeof(string));
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            object obj2 = null;
            string str = value as string;
            if (!string.IsNullOrEmpty(str))
            {
                foreach (object obj3 in this.GetStandardValues(context))
                {
                    CorrelationToken token = obj3 as CorrelationToken;
                    if ((token != null) && (token.Name == str))
                    {
                        obj2 = token;
                        break;
                    }
                }
                if (obj2 == null)
                {
                    obj2 = new CorrelationToken(str);
                }
            }
            return obj2;
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            object name = null;
            CorrelationToken token = value as CorrelationToken;
            if ((destinationType == typeof(string)) && (token != null))
            {
                name = token.Name;
            }
            return name;
        }

        private IEnumerable GetContainedActivities(CompositeActivity activity)
        {
            if (activity.Enabled)
            {
                foreach (Activity iteratorVariable0 in activity.Activities)
                {
                    if (iteratorVariable0 is CompositeActivity)
                    {
                        IEnumerator enumerator = this.GetContainedActivities((CompositeActivity) iteratorVariable0).GetEnumerator();
                        while (enumerator.MoveNext())
                        {
                            Activity current = (Activity) enumerator.Current;
                            if (current.Enabled)
                            {
                                yield return current;
                            }
                        }
                    }
                    else if (iteratorVariable0.Enabled)
                    {
                        yield return iteratorVariable0;
                    }
                }
            }
        }

        private IEnumerable GetPreceedingActivities(Activity startActivity)
        {
            Activity iteratorVariable0 = null;
            Stack<Activity> iteratorVariable1 = new Stack<Activity>();
            iteratorVariable1.Push(startActivity);
            while ((iteratorVariable0 = iteratorVariable1.Pop()) != null)
            {
                if (iteratorVariable0.Parent != null)
                {
                    foreach (Activity iteratorVariable2 in iteratorVariable0.Parent.Activities)
                    {
                        if (iteratorVariable2 == iteratorVariable0)
                        {
                            break;
                        }
                        if (iteratorVariable2.Enabled)
                        {
                            if (iteratorVariable2 is CompositeActivity)
                            {
                                IEnumerator enumerator = this.GetContainedActivities((CompositeActivity) iteratorVariable2).GetEnumerator();
                                while (enumerator.MoveNext())
                                {
                                    Activity current = (Activity) enumerator.Current;
                                    yield return current;
                                }
                                continue;
                            }
                            yield return iteratorVariable2;
                        }
                    }
                }
                iteratorVariable1.Push(iteratorVariable0.Parent);
            }
        }

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            ArrayList list = new ArrayList(base.GetProperties(context, value, attributes));
            return new PropertyDescriptorCollection((PropertyDescriptor[]) list.ToArray(typeof(PropertyDescriptor)));
        }

        public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            ArrayList values = new ArrayList();
            Activity instance = context.Instance as Activity;
            if (instance != null)
            {
                foreach (Activity activity2 in this.GetPreceedingActivities(instance))
                {
                    PropertyDescriptor descriptor = TypeDescriptor.GetProperties(activity2)["CorrelationToken"];
                    if (descriptor != null)
                    {
                        CorrelationToken item = descriptor.GetValue(activity2) as CorrelationToken;
                        if ((item != null) && !values.Contains(item))
                        {
                            values.Add(item);
                        }
                    }
                }
            }
            return new TypeConverter.StandardValuesCollection(values);
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return false;
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }


    }
}

