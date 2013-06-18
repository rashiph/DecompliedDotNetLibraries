namespace System.Activities.Debugger
{
    using System;
    using System.Activities;
    using System.Activities.XamlIntegration;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Windows.Markup;
    using System.Xaml;

    internal class DebugInfo
    {
        private System.Activities.ActivityInstance activityInstance;
        private LocalInfo[] arguments;
        private Dictionary<string, LocalInfo> cachedLocalInfos;
        private LocalInfo[] locals;

        public DebugInfo(System.Activities.ActivityInstance activityInstance)
        {
            this.activityInstance = activityInstance;
        }

        private void CacheLocalInfos(LocalInfo[] localInfos)
        {
            if (this.cachedLocalInfos == null)
            {
                this.cachedLocalInfos = new Dictionary<string, LocalInfo>(StringComparer.OrdinalIgnoreCase);
            }
            foreach (LocalInfo info in localInfos)
            {
                this.cachedLocalInfos[info.Name] = info;
            }
        }

        private static bool ConvertToChar(string stringValue, int radix, out char ch)
        {
            bool flag = false;
            ch = '\0';
            try
            {
                int index;
                if ((stringValue[0] != '\'') && (stringValue[0] != '"'))
                {
                    goto Label_00B4;
                }
                if (stringValue[1] != '\\')
                {
                    goto Label_0097;
                }
                char ch2 = stringValue[2];
                if (ch2 <= 'b')
                {
                    switch (ch2)
                    {
                        case 'a':
                        case 'b':
                        case '\'':
                            goto Label_007A;
                    }
                    return flag;
                }
                switch (ch2)
                {
                    case 'r':
                    case 't':
                    case 'v':
                    case 'f':
                    case 'n':
                        break;

                    case 's':
                    case 'u':
                        return flag;

                    default:
                        return flag;
                }
            Label_007A:
                if (stringValue[3] == stringValue[0])
                {
                    ch = stringValue[2];
                }
                return true;
            Label_0097:
                if (stringValue[2] == stringValue[0])
                {
                    ch = stringValue[1];
                    flag = true;
                }
                return flag;
            Label_00B4:
                index = stringValue.IndexOf('\'');
                if (index < 0)
                {
                    index = stringValue.IndexOf('"');
                }
                if (index > 0)
                {
                    stringValue = stringValue.Substring(0, index);
                }
                ch = (char) Convert.ToUInt16(RemoveHexadecimalPrefix(stringValue), radix);
                return true;
            }
            catch (IndexOutOfRangeException)
            {
                return false;
            }
        }

        public object EvaluateExpression(string expressionString)
        {
            int num;
            object obj2;
            if (int.TryParse(expressionString, out num))
            {
                return num;
            }
            LocalInfo info = null;
            if ((this.cachedLocalInfos != null) && this.cachedLocalInfos.TryGetValue(expressionString, out info))
            {
                return info;
            }
            LocationReferenceEnvironment publicEnvironment = this.activityInstance.Activity.PublicEnvironment;
            ActivityContext context = new ActivityContext(this.activityInstance, null);
            try
            {
                if (!TryEvaluateExpression(expressionString, null, publicEnvironment, context, out obj2))
                {
                    return System.Activities.SR.DebugInfoCannotEvaluateExpression(expressionString);
                }
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                context.Dispose();
                return System.Activities.SR.DebugInfoCannotEvaluateExpression(expressionString);
            }
            try
            {
                object obj3;
                if (TryEvaluateExpression(expressionString, obj2.GetType(), publicEnvironment, context, out obj3))
                {
                    LocalInfo info2 = new LocalInfo {
                        Name = expressionString,
                        Location = obj3 as System.Activities.Location
                    };
                    this.cachedLocalInfos[expressionString] = info2;
                    return info2;
                }
            }
            catch (Exception exception2)
            {
                if (Fx.IsFatal(exception2))
                {
                    throw;
                }
            }
            finally
            {
                context.Dispose();
            }
            return obj2;
        }

        public LocalInfo[] GetArguments()
        {
            Func<RuntimeArgument, LocalInfo> selector = null;
            if ((this.arguments == null) || (this.arguments.Length == 0))
            {
                if (selector == null)
                {
                    selector = argument => new LocalInfo { Name = argument.Name, Location = argument.InternalGetLocation(this.activityInstance.Environment) };
                }
                this.arguments = this.activityInstance.Activity.RuntimeArguments.Select<RuntimeArgument, LocalInfo>(selector).ToArray<LocalInfo>();
                if (this.arguments.Length > 0)
                {
                    this.CacheLocalInfos(this.arguments);
                }
            }
            return this.arguments;
        }

        public LocalInfo[] GetLocals()
        {
            Func<Variable, LocalInfo> selector = null;
            Func<RuntimeArgument, LocalInfo> func2 = null;
            Func<DelegateArgument, LocalInfo> func3 = null;
            if ((this.locals == null) || (this.locals.Length == 0))
            {
                Activity parent = this.activityInstance.Activity;
                List<Variable> source = new List<Variable>();
                List<RuntimeArgument> list2 = new List<RuntimeArgument>();
                List<DelegateArgument> list3 = new List<DelegateArgument>();
                System.Collections.Generic.HashSet<string> existingNames = new System.Collections.Generic.HashSet<string>();
                while (parent != null)
                {
                    source.AddRange(RemoveHiddenVariables(existingNames, parent.RuntimeVariables));
                    source.AddRange(RemoveHiddenVariables(existingNames, parent.ImplementationVariables));
                    if (parent.HandlerOf != null)
                    {
                        list3.AddRange(RemoveHiddenDelegateArguments(existingNames, from delegateArgument in parent.HandlerOf.RuntimeDelegateArguments select delegateArgument.BoundArgument));
                    }
                    list2.AddRange(RemoveHiddenArguments(existingNames, parent.RuntimeArguments));
                    parent = parent.Parent;
                }
                LocalInfo[] first = new LocalInfo[1];
                LocalInfo info = new LocalInfo {
                    Name = "this",
                    Type = "System.Activities.ActivityInstance",
                    Value = this.activityInstance
                };
                first[0] = info;
                if (selector == null)
                {
                    selector = variable => new LocalInfo { Name = variable.Name, Location = variable.InternalGetLocation(this.activityInstance.Environment) };
                }
                if (func2 == null)
                {
                    func2 = argument => new LocalInfo { Name = argument.Name, Location = argument.InternalGetLocation(this.activityInstance.Environment) };
                }
                if (func3 == null)
                {
                    func3 = argument => new LocalInfo { Name = argument.Name, Location = argument.InternalGetLocation(this.activityInstance.Environment) };
                }
                this.locals = first.Concat<LocalInfo>((from info in source.Select<Variable, LocalInfo>(selector).Concat<LocalInfo>(list2.Select<RuntimeArgument, LocalInfo>(func2)).Concat<LocalInfo>(list3.Select<DelegateArgument, LocalInfo>(func3))
                    orderby info.Name
                    select info)).ToArray<LocalInfo>();
                if (this.locals.Length > 0)
                {
                    this.CacheLocalInfos(this.locals);
                }
            }
            return this.locals;
        }

        private static string RemoveHexadecimalPrefix(string stringValue)
        {
            stringValue = stringValue.Trim().ToUpperInvariant();
            if (stringValue.StartsWith("0X", StringComparison.Ordinal))
            {
                stringValue = stringValue.Substring(2);
            }
            return stringValue;
        }

        private static List<RuntimeArgument> RemoveHiddenArguments(System.Collections.Generic.HashSet<string> existingNames, IList<RuntimeArgument> ancestorArguments)
        {
            List<RuntimeArgument> list = new List<RuntimeArgument>(ancestorArguments.Count);
            foreach (RuntimeArgument argument in ancestorArguments)
            {
                if (!existingNames.Contains(argument.Name))
                {
                    list.Add(argument);
                    existingNames.Add(argument.Name);
                }
            }
            return list;
        }

        private static List<DelegateArgument> RemoveHiddenDelegateArguments(System.Collections.Generic.HashSet<string> existingNames, IEnumerable<DelegateArgument> ancestorDelegateArguments)
        {
            List<DelegateArgument> list = new List<DelegateArgument>();
            foreach (DelegateArgument argument in ancestorDelegateArguments)
            {
                if (((argument != null) && (argument.Name != null)) && !existingNames.Contains(argument.Name))
                {
                    list.Add(argument);
                    existingNames.Add(argument.Name);
                }
            }
            return list;
        }

        private static List<Variable> RemoveHiddenVariables(System.Collections.Generic.HashSet<string> existingNames, IEnumerable<Variable> ancestorVariables)
        {
            List<Variable> list = new List<Variable>();
            foreach (Variable variable in ancestorVariables)
            {
                if (((variable.Name != null) && !variable.Name.StartsWith("_", StringComparison.Ordinal)) && !existingNames.Contains(variable.Name))
                {
                    list.Add(variable);
                    existingNames.Add(variable.Name);
                }
            }
            return list;
        }

        private static string RemoveQuotes(string stringValue)
        {
            if (stringValue.StartsWith("\"", StringComparison.Ordinal))
            {
                stringValue = stringValue.Substring(1);
            }
            if (stringValue.EndsWith("\"", StringComparison.Ordinal))
            {
                stringValue = stringValue.Substring(0, stringValue.Length - 1);
            }
            return stringValue;
        }

        public bool SetValueAsString(System.Activities.Location location, string value, string stringRadix)
        {
            bool flag = true;
            try
            {
                value = value.Trim();
                Type locationType = location.LocationType;
                if (((locationType == typeof(string)) && value.StartsWith("\"", StringComparison.Ordinal)) && value.EndsWith("\"", StringComparison.Ordinal))
                {
                    location.Value = RemoveQuotes(value);
                    return flag;
                }
                if (locationType == typeof(bool))
                {
                    location.Value = Convert.ToBoolean(value, CultureInfo.InvariantCulture);
                    return flag;
                }
                if (locationType == typeof(sbyte))
                {
                    location.Value = Convert.ToSByte(RemoveHexadecimalPrefix(value), Convert.ToInt32(stringRadix, CultureInfo.InvariantCulture));
                    return flag;
                }
                if (locationType == typeof(char))
                {
                    char ch;
                    flag = ConvertToChar(value, Convert.ToInt32(stringRadix, CultureInfo.InvariantCulture), out ch);
                    if (flag)
                    {
                        location.Value = ch;
                    }
                    return flag;
                }
                if (locationType == typeof(short))
                {
                    location.Value = Convert.ToInt16(RemoveHexadecimalPrefix(value), Convert.ToInt32(stringRadix, CultureInfo.InvariantCulture));
                    return flag;
                }
                if (locationType == typeof(int))
                {
                    location.Value = Convert.ToInt32(RemoveHexadecimalPrefix(value), Convert.ToInt32(stringRadix, CultureInfo.InvariantCulture));
                    return flag;
                }
                if (locationType == typeof(long))
                {
                    location.Value = Convert.ToInt64(RemoveHexadecimalPrefix(value), Convert.ToInt32(stringRadix, CultureInfo.InvariantCulture));
                    return flag;
                }
                if (locationType == typeof(byte))
                {
                    location.Value = Convert.ToByte(RemoveHexadecimalPrefix(value), Convert.ToInt32(stringRadix, CultureInfo.InvariantCulture));
                    return flag;
                }
                if (locationType == typeof(ushort))
                {
                    location.Value = Convert.ToUInt16(RemoveHexadecimalPrefix(value), Convert.ToInt32(stringRadix, CultureInfo.InvariantCulture));
                    return flag;
                }
                if (locationType == typeof(uint))
                {
                    location.Value = Convert.ToUInt32(RemoveHexadecimalPrefix(value), Convert.ToInt32(stringRadix, CultureInfo.InvariantCulture));
                    return flag;
                }
                if (locationType == typeof(ulong))
                {
                    location.Value = Convert.ToUInt64(RemoveHexadecimalPrefix(value), Convert.ToInt32(stringRadix, CultureInfo.InvariantCulture));
                    return flag;
                }
                if (locationType == typeof(float))
                {
                    if (!value.Contains(","))
                    {
                        location.Value = Convert.ToSingle(value, CultureInfo.InvariantCulture);
                        return flag;
                    }
                    return false;
                }
                if (locationType == typeof(double))
                {
                    if (!value.Contains(","))
                    {
                        location.Value = Convert.ToDouble(value, CultureInfo.InvariantCulture);
                        return flag;
                    }
                    return false;
                }
                if (locationType == typeof(decimal))
                {
                    value = value.TrimEnd(new char[0]);
                    if (value.EndsWith("M", StringComparison.OrdinalIgnoreCase) || value.EndsWith("D", StringComparison.OrdinalIgnoreCase))
                    {
                        value = value.Substring(0, value.Length - 1);
                    }
                    if (value.Contains(","))
                    {
                        return false;
                    }
                    location.Value = Convert.ToDecimal(value, CultureInfo.InvariantCulture);
                    return flag;
                }
                if (locationType == typeof(DateTime))
                {
                    location.Value = Convert.ToDateTime(value, CultureInfo.CurrentCulture);
                    return flag;
                }
                if (locationType.IsEnum)
                {
                    location.Value = Enum.Parse(locationType, value, true);
                }
            }
            catch (InvalidCastException)
            {
                flag = false;
            }
            catch (OverflowException)
            {
                flag = false;
            }
            catch (FormatException)
            {
                flag = false;
            }
            catch (ArgumentOutOfRangeException)
            {
                flag = false;
            }
            return flag;
        }

        private static bool TryEvaluateExpression<T>(Activity<T> expression, LocationReferenceEnvironment locationReferenceEnvironment, ActivityContext context, out object result)
        {
            T local;
            if (!expression.TryGetValue(context, out local))
            {
                result = System.Activities.SR.DebugInfoTryGetValueFailed;
                return false;
            }
            Activity rootActivity = local as Activity;
            context.Activity = rootActivity;
            if ((rootActivity != null) && !rootActivity.IsRuntimeReady)
            {
                WorkflowInspectionServices.CacheMetadata(rootActivity, locationReferenceEnvironment);
            }
            IExpressionContainer container = local as IExpressionContainer;
            if (container == null)
            {
                result = System.Activities.SR.DebugInfoNotAnIExpressionContainer;
                return false;
            }
            Expression<Func<ActivityContext, object>> expression2 = container.Expression as Expression<Func<ActivityContext, object>>;
            if (expression2 == null)
            {
                result = System.Activities.SR.DebugInfoNoLambda;
                return false;
            }
            result = expression2.Compile()(context);
            return true;
        }

        private static bool TryEvaluateExpression(string expressionString, Type locationValueType, LocationReferenceEnvironment locationReferenceEnvironment, ActivityContext context, out object result)
        {
            Type type;
            expressionString = string.Format(CultureInfo.InvariantCulture, "[{0}]", new object[] { expressionString });
            if (locationValueType != null)
            {
                type = typeof(Activity<>).MakeGenericType(new Type[] { typeof(Location<>).MakeGenericType(new Type[] { locationValueType }) });
            }
            else
            {
                type = typeof(Activity<object>);
            }
            ActivityWithResultConverter converter = new ActivityWithResultConverter(type);
            TypeDescriptorContext context2 = new TypeDescriptorContext {
                LocationReferenceEnvironment = locationReferenceEnvironment
            };
            ActivityWithResult expression = converter.ConvertFromString(context2, expressionString) as ActivityWithResult;
            if (locationValueType != null)
            {
                LocationHelper helper = (LocationHelper) Activator.CreateInstance(typeof(LocationHelper).MakeGenericType(new Type[] { locationValueType }));
                return helper.TryGetValue(expression, locationReferenceEnvironment, context, out result);
            }
            return TryEvaluateExpression<object>(expression, locationReferenceEnvironment, context, out result);
        }

        public class LocalInfo
        {
            public System.Activities.Location Location;
            public string Name;
            private string type;
            private object valueField;

            public string Type
            {
                get
                {
                    if (this.Location != null)
                    {
                        return this.Location.LocationType.Name;
                    }
                    return this.type;
                }
                set
                {
                    this.type = value;
                }
            }

            public object Value
            {
                get
                {
                    if (this.Location != null)
                    {
                        return this.Location.Value;
                    }
                    return this.valueField;
                }
                set
                {
                    this.valueField = value;
                }
            }
        }

        private abstract class LocationHelper
        {
            protected LocationHelper()
            {
            }

            public abstract bool TryGetValue(Activity expression, LocationReferenceEnvironment locationReferenceEnvironment, ActivityContext context, out object result);
        }

        private class LocationHelper<TLocationValue> : DebugInfo.LocationHelper
        {
            public override bool TryGetValue(Activity expression, LocationReferenceEnvironment locationReferenceEnvironment, ActivityContext context, out object result)
            {
                Activity<Location<TLocationValue>> rootActivity = expression as Activity<Location<TLocationValue>>;
                result = null;
                if (rootActivity != null)
                {
                    Location<TLocationValue> location;
                    context.Activity = expression;
                    if ((rootActivity != null) && !rootActivity.IsRuntimeReady)
                    {
                        WorkflowInspectionServices.CacheMetadata(rootActivity, locationReferenceEnvironment);
                    }
                    if (rootActivity.TryGetValue(context, out location))
                    {
                        result = location;
                        return true;
                    }
                }
                return false;
            }
        }

        private class TypeDescriptorContext : ITypeDescriptorContext, IServiceProvider, IXamlNamespaceResolver, INameScope, INamespacePrefixLookup
        {
            public System.Activities.LocationReferenceEnvironment LocationReferenceEnvironment;

            public object FindName(string name)
            {
                LocationReference reference;
                if (!this.LocationReferenceEnvironment.TryGetLocationReference(name, out reference))
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.VariableOrArgumentDoesNotExist(name)));
                }
                return reference;
            }

            public string GetNamespace(string prefix)
            {
                if (!string.IsNullOrEmpty(prefix))
                {
                    throw FxTrace.Exception.AsError(new NotImplementedException());
                }
                return string.Empty;
            }

            public IEnumerable<NamespaceDeclaration> GetNamespacePrefixes()
            {
                return Enumerable.Empty<NamespaceDeclaration>();
            }

            public object GetService(Type serviceType)
            {
                if (serviceType.IsAssignableFrom(typeof(DebugInfo.TypeDescriptorContext)))
                {
                    return this;
                }
                return null;
            }

            public string LookupPrefix(string name)
            {
                throw FxTrace.Exception.AsError(new NotImplementedException());
            }

            public void OnComponentChanged()
            {
                throw FxTrace.Exception.AsError(new NotImplementedException());
            }

            public bool OnComponentChanging()
            {
                throw FxTrace.Exception.AsError(new NotImplementedException());
            }

            public void RegisterName(string name, object scopedElement)
            {
                throw FxTrace.Exception.AsError(new NotImplementedException());
            }

            public void UnregisterName(string name)
            {
                throw FxTrace.Exception.AsError(new NotImplementedException());
            }

            public IContainer Container
            {
                get
                {
                    throw FxTrace.Exception.AsError(new NotImplementedException());
                }
            }

            public object Instance
            {
                get
                {
                    throw FxTrace.Exception.AsError(new NotImplementedException());
                }
            }

            public System.ComponentModel.PropertyDescriptor PropertyDescriptor
            {
                get
                {
                    throw FxTrace.Exception.AsError(new NotImplementedException());
                }
            }
        }
    }
}

