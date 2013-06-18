namespace System.Workflow.ComponentModel
{
    using System;
    using System.Globalization;
    using System.Reflection;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.ComponentModel.Design;

    internal static class BindHelpers
    {
        internal static AccessTypes GetAccessType(IServiceProvider serviceProvider, PropertyValidationContext validationContext)
        {
            AccessTypes read = AccessTypes.Read;
            if (validationContext.Property is PropertyInfo)
            {
                return Helpers.GetAccessType(validationContext.Property as PropertyInfo, validationContext.PropertyOwner, serviceProvider);
            }
            if (validationContext.Property is DependencyProperty)
            {
                IDynamicPropertyTypeProvider propertyOwner = validationContext.PropertyOwner as IDynamicPropertyTypeProvider;
                if (propertyOwner != null)
                {
                    read = propertyOwner.GetAccessType(serviceProvider, ((DependencyProperty) validationContext.Property).Name);
                }
            }
            return read;
        }

        internal static Type GetBaseType(IServiceProvider serviceProvider, PropertyValidationContext validationContext)
        {
            Type propertyType = null;
            if (validationContext.Property is PropertyInfo)
            {
                return Helpers.GetBaseType(validationContext.Property as PropertyInfo, validationContext.PropertyOwner, serviceProvider);
            }
            if (validationContext.Property is DependencyProperty)
            {
                DependencyProperty property = validationContext.Property as DependencyProperty;
                if (property == null)
                {
                    return propertyType;
                }
                if (propertyType == null)
                {
                    IDynamicPropertyTypeProvider propertyOwner = validationContext.PropertyOwner as IDynamicPropertyTypeProvider;
                    if (propertyOwner != null)
                    {
                        propertyType = propertyOwner.GetPropertyType(serviceProvider, property.Name);
                    }
                }
                if (propertyType == null)
                {
                    propertyType = property.PropertyType;
                }
            }
            return propertyType;
        }

        internal static PropertyInfo GetMatchedPropertyInfo(Type memberType, string[] aryArgName, object[] args)
        {
            if (memberType == null)
            {
                throw new ArgumentNullException("memberType");
            }
            if (aryArgName == null)
            {
                throw new ArgumentNullException("aryArgName");
            }
            if (args == null)
            {
                throw new ArgumentNullException("args");
            }
            MemberInfo[][] infoArray5 = new MemberInfo[2][];
            infoArray5[0] = memberType.GetDefaultMembers();
            MemberInfo[][] infoArray = infoArray5;
            if (memberType.IsArray)
            {
                MemberInfo[] member = memberType.GetMember("Get");
                MemberInfo[] infoArray3 = memberType.GetMember("Set");
                PropertyInfo info = new ActivityBindPropertyInfo(memberType, member[0] as MethodInfo, infoArray3[0] as MethodInfo, string.Empty, null);
                infoArray[1] = new MemberInfo[] { info };
            }
            for (int i = 0; i < infoArray.Length; i++)
            {
                if (infoArray[i] != null)
                {
                    MemberInfo[] infoArray4 = infoArray[i];
                    foreach (MemberInfo info2 in infoArray4)
                    {
                        PropertyInfo propertyInfo = info2 as PropertyInfo;
                        if ((propertyInfo != null) && MatchIndexerParameters(propertyInfo, aryArgName, args))
                        {
                            return propertyInfo;
                        }
                    }
                }
            }
            return null;
        }

        internal static Type GetMemberType(MemberInfo memberInfo)
        {
            FieldInfo info = memberInfo as FieldInfo;
            if (info != null)
            {
                return info.FieldType;
            }
            PropertyInfo info2 = memberInfo as PropertyInfo;
            if (info2 != null)
            {
                if (info2.PropertyType != null)
                {
                    return info2.PropertyType;
                }
                return info2.GetGetMethod().ReturnType;
            }
            EventInfo info4 = memberInfo as EventInfo;
            if (info4 != null)
            {
                return info4.EventHandlerType;
            }
            return null;
        }

        internal static bool MatchIndexerParameters(PropertyInfo propertyInfo, string[] argNames, object[] args)
        {
            if (propertyInfo == null)
            {
                throw new ArgumentNullException("propertyInfo");
            }
            if (argNames == null)
            {
                throw new ArgumentNullException("argNames");
            }
            if (args == null)
            {
                throw new ArgumentNullException("args");
            }
            ParameterInfo[] indexParameters = propertyInfo.GetIndexParameters();
            if (indexParameters.Length != argNames.Length)
            {
                return false;
            }
            for (int i = 0; i < args.Length; i++)
            {
                Type parameterType = indexParameters[i].ParameterType;
                if ((parameterType != typeof(string)) && (parameterType != typeof(int)))
                {
                    return false;
                }
                try
                {
                    object obj2 = null;
                    string str = argNames[i].Trim();
                    if (((parameterType == typeof(string)) && str.StartsWith("\"", StringComparison.Ordinal)) && str.EndsWith("\"", StringComparison.Ordinal))
                    {
                        obj2 = str.Substring(1, str.Length - 2).Trim();
                    }
                    else if (parameterType == typeof(int))
                    {
                        obj2 = Convert.ChangeType(str, typeof(int), CultureInfo.InvariantCulture);
                    }
                    if (obj2 != null)
                    {
                        args.SetValue(obj2, i);
                    }
                    else
                    {
                        return false;
                    }
                }
                catch
                {
                    return false;
                }
            }
            return true;
        }

        internal static object ResolveActivityPath(Activity refActivity, string path)
        {
            PathWalker walker;
            if (refActivity == null)
            {
                throw new ArgumentNullException("refActivity");
            }
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (path.Length == 0)
            {
                throw new ArgumentException(SR.GetString("Error_EmptyPathValue"), "path");
            }
            object value = refActivity;
            BindRecursionContext recursionContext = new BindRecursionContext();
            if (new PathWalker { MemberFound = (EventHandler<PathMemberInfoEventArgs>) Delegate.Combine(walker.MemberFound, delegate (object sender, PathMemberInfoEventArgs eventArgs) {
                if (value == null)
                {
                    eventArgs.Action = PathWalkAction.Cancel;
                }
                else
                {
                    switch (eventArgs.MemberKind)
                    {
                        case PathMemberKind.Field:
                            try
                            {
                                value = (eventArgs.MemberInfo as FieldInfo).GetValue(value);
                            }
                            catch (Exception exception)
                            {
                                value = null;
                                eventArgs.Action = PathWalkAction.Cancel;
                                if (!refActivity.DesignMode)
                                {
                                    TargetInvocationException exception2 = exception as TargetInvocationException;
                                    throw (exception2 != null) ? exception2.InnerException : exception;
                                }
                            }
                            break;

                        case PathMemberKind.Event:
                        {
                            EventInfo memberInfo = eventArgs.MemberInfo as EventInfo;
                            DependencyProperty dependencyProperty = DependencyProperty.FromName(memberInfo.Name, value.GetType());
                            if ((dependencyProperty != null) && (value is DependencyObject))
                            {
                                if ((value as DependencyObject).IsBindingSet(dependencyProperty))
                                {
                                    value = (value as DependencyObject).GetBinding(dependencyProperty);
                                }
                                else
                                {
                                    value = (value as DependencyObject).GetHandler(dependencyProperty);
                                }
                            }
                            break;
                        }
                        case PathMemberKind.Property:
                            if ((eventArgs.MemberInfo as PropertyInfo).CanRead)
                            {
                                DependencyProperty property2 = DependencyProperty.FromName(eventArgs.MemberInfo.Name, value.GetType());
                                if (((property2 != null) && (value is DependencyObject)) && (value as DependencyObject).IsBindingSet(property2))
                                {
                                    value = (value as DependencyObject).GetBinding(property2);
                                }
                                else
                                {
                                    try
                                    {
                                        value = (eventArgs.MemberInfo as PropertyInfo).GetValue(value, null);
                                    }
                                    catch (Exception exception3)
                                    {
                                        value = null;
                                        eventArgs.Action = PathWalkAction.Cancel;
                                        if (!refActivity.DesignMode)
                                        {
                                            TargetInvocationException exception4 = exception3 as TargetInvocationException;
                                            throw (exception4 != null) ? exception4.InnerException : exception3;
                                        }
                                    }
                                }
                                break;
                            }
                            eventArgs.Action = PathWalkAction.Cancel;
                            return;

                        case PathMemberKind.IndexedProperty:
                        case PathMemberKind.Index:
                            try
                            {
                                value = (eventArgs.MemberInfo as PropertyInfo).GetValue(value, BindingFlags.GetProperty, null, eventArgs.IndexParameters, CultureInfo.InvariantCulture);
                            }
                            catch (Exception exception5)
                            {
                                value = null;
                                eventArgs.Action = PathWalkAction.Cancel;
                                if (!refActivity.DesignMode)
                                {
                                    TargetInvocationException exception6 = exception5 as TargetInvocationException;
                                    throw (exception6 != null) ? exception6.InnerException : exception5;
                                }
                            }
                            break;
                    }
                    if (((value is ActivityBind) && !eventArgs.LastMemberInThePath) && (GetMemberType(eventArgs.MemberInfo) != typeof(ActivityBind)))
                    {
                        while (value is ActivityBind)
                        {
                            ActivityBind bind = value as ActivityBind;
                            if (recursionContext.Contains(refActivity, bind))
                            {
                                throw new InvalidOperationException(SR.GetString("Bind_ActivityDataSourceRecursionDetected"));
                            }
                            recursionContext.Add(refActivity, bind);
                            value = bind.GetRuntimeValue(refActivity);
                        }
                    }
                }
            }) }.TryWalkPropertyPath(refActivity.GetType(), path))
            {
                return value;
            }
            return null;
        }
    }
}

