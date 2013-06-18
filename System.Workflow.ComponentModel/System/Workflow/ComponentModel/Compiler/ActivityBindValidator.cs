namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.ComponentModel.Design;
    using System.Reflection;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;

    internal sealed class ActivityBindValidator : Validator
    {
        internal static bool DoesTargetTypeMatch(Type baseType, Type memberType, AccessTypes access)
        {
            if ((access & AccessTypes.ReadWrite) == AccessTypes.ReadWrite)
            {
                return TypeProvider.IsRepresentingTheSameType(memberType, baseType);
            }
            if ((access & AccessTypes.Read) == AccessTypes.Read)
            {
                return TypeProvider.IsAssignable(baseType, memberType, true);
            }
            return (((access & AccessTypes.Write) == AccessTypes.Write) && TypeProvider.IsAssignable(memberType, baseType, true));
        }

        public override ValidationErrorCollection Validate(ValidationManager manager, object obj)
        {
            ValidationErrorCollection errors = base.Validate(manager, obj);
            ActivityBind bind = obj as ActivityBind;
            if (bind == null)
            {
                throw new ArgumentException(SR.GetString("Error_UnexpectedArgumentType", new object[] { typeof(ActivityBind).FullName }), "obj");
            }
            Activity activity = manager.Context[typeof(Activity)] as Activity;
            if (activity == null)
            {
                throw new InvalidOperationException(SR.GetString("Error_ContextStackItemMissing", new object[] { typeof(Activity).Name }));
            }
            PropertyValidationContext validationContext = manager.Context[typeof(PropertyValidationContext)] as PropertyValidationContext;
            if (validationContext == null)
            {
                throw new InvalidOperationException(SR.GetString("Error_ContextStackItemMissing", new object[] { typeof(BindValidationContext).Name }));
            }
            ValidationError item = null;
            if (string.IsNullOrEmpty(bind.Name))
            {
                item = new ValidationError(SR.GetString("Error_IDNotSetForActivitySource"), 0x613) {
                    PropertyName = base.GetFullPropertyName(manager) + ".Name"
                };
                errors.Add(item);
                return errors;
            }
            Activity refActivity = Helpers.ParseActivityForBind(activity, bind.Name);
            if (refActivity == null)
            {
                if (bind.Name.StartsWith("/"))
                {
                    item = new ValidationError(SR.GetString("Error_CannotResolveRelativeActivity", new object[] { bind.Name }), 0x128);
                }
                else
                {
                    item = new ValidationError(SR.GetString("Error_CannotResolveActivity", new object[] { bind.Name }), 0x129);
                }
                item.PropertyName = base.GetFullPropertyName(manager) + ".Name";
                errors.Add(item);
            }
            if (string.IsNullOrEmpty(bind.Path))
            {
                item = new ValidationError(SR.GetString("Error_PathNotSetForActivitySource"), 0x12b) {
                    PropertyName = base.GetFullPropertyName(manager) + ".Path"
                };
                errors.Add(item);
            }
            if ((refActivity != null) && (errors.Count == 0))
            {
                string path = bind.Path;
                string str2 = string.Empty;
                int startIndex = path.IndexOfAny(new char[] { '.', '/', '[' });
                if (startIndex != -1)
                {
                    str2 = path.Substring(startIndex);
                    str2 = str2.StartsWith(".") ? str2.Substring(1) : str2;
                    path = path.Substring(0, startIndex);
                }
                Type baseType = BindHelpers.GetBaseType(manager, validationContext);
                MemberInfo memberInfo = null;
                Type srcType = null;
                if (!string.IsNullOrEmpty(path))
                {
                    srcType = BindValidatorHelper.GetActivityType(manager, refActivity);
                    if (srcType != null)
                    {
                        memberInfo = MemberBind.GetMemberInfo(srcType, path);
                        if ((memberInfo == null) && str2.StartsWith("[", StringComparison.Ordinal))
                        {
                            string str3 = bind.Path.Substring(startIndex);
                            int index = str3.IndexOf(']');
                            if (index != -1)
                            {
                                string str4 = str3.Substring(0, index + 1);
                                str2 = ((index + 1) < str3.Length) ? str3.Substring(index + 1) : string.Empty;
                                str2 = str2.StartsWith(".") ? str2.Substring(1) : str2;
                                str3 = str4;
                            }
                            path = path + str3;
                            memberInfo = MemberBind.GetMemberInfo(srcType, path);
                        }
                    }
                }
                Validator validator = null;
                object obj2 = null;
                if (memberInfo != null)
                {
                    string str5 = !string.IsNullOrEmpty(refActivity.QualifiedName) ? refActivity.QualifiedName : bind.Name;
                    if (memberInfo is FieldInfo)
                    {
                        obj2 = new FieldBind(str5 + "." + path, str2);
                        validator = new FieldBindValidator();
                    }
                    else if (memberInfo is MethodInfo)
                    {
                        if (typeof(Delegate).IsAssignableFrom(baseType))
                        {
                            obj2 = new MethodBind(str5 + "." + path);
                            validator = new MethodBindValidator();
                        }
                        else
                        {
                            item = new ValidationError(SR.GetString("Error_InvalidMemberType", new object[] { path, base.GetFullPropertyName(manager) }), 0x629) {
                                PropertyName = base.GetFullPropertyName(manager)
                            };
                            errors.Add(item);
                        }
                    }
                    else if (memberInfo is PropertyInfo)
                    {
                        if (refActivity == activity)
                        {
                            obj2 = new PropertyBind(str5 + "." + path, str2);
                            validator = new PropertyBindValidator();
                        }
                        else
                        {
                            obj2 = bind;
                            validator = this;
                        }
                    }
                    else if (memberInfo is EventInfo)
                    {
                        obj2 = bind;
                        validator = this;
                    }
                }
                else if (((memberInfo == null) && (baseType != null)) && typeof(Delegate).IsAssignableFrom(baseType))
                {
                    obj2 = bind;
                    validator = this;
                }
                if ((validator != null) && (obj2 != null))
                {
                    if ((validator == this) && (obj2 is ActivityBind))
                    {
                        errors.AddRange(this.ValidateActivityBind(manager, obj2));
                        return errors;
                    }
                    errors.AddRange(validator.Validate(manager, obj2));
                    return errors;
                }
                if (item == null)
                {
                    item = new ValidationError(SR.GetString("Error_PathCouldNotBeResolvedToMember", new object[] { bind.Path, !string.IsNullOrEmpty(refActivity.QualifiedName) ? refActivity.QualifiedName : refActivity.GetType().Name }), 0x60d) {
                        PropertyName = base.GetFullPropertyName(manager)
                    };
                    errors.Add(item);
                }
            }
            return errors;
        }

        private ValidationErrorCollection ValidateActivity(ValidationManager manager, ActivityBind bind, BindValidationContext validationContext)
        {
            ValidationError item = null;
            ValidationErrorCollection errors = new ValidationErrorCollection();
            Activity activity = manager.Context[typeof(Activity)] as Activity;
            if (activity == null)
            {
                throw new InvalidOperationException(SR.GetString("Error_ContextStackItemMissing", new object[] { typeof(Activity).Name }));
            }
            Activity request = Helpers.ParseActivityForBind(activity, bind.Name);
            if (request == null)
            {
                item = bind.Name.StartsWith("/", StringComparison.Ordinal) ? new ValidationError(SR.GetString("Error_CannotResolveRelativeActivity", new object[] { bind.Name }), 0x128) : new ValidationError(SR.GetString("Error_CannotResolveActivity", new object[] { bind.Name }), 0x129);
                item.PropertyName = base.GetFullPropertyName(manager) + ".Name";
            }
            else if ((bind.Path == null) || (bind.Path.Length == 0))
            {
                item = new ValidationError(SR.GetString("Error_PathNotSetForActivitySource"), 0x12b) {
                    PropertyName = base.GetFullPropertyName(manager) + ".Path"
                };
            }
            else
            {
                if (!bind.Name.StartsWith("/", StringComparison.Ordinal) && !ValidationHelpers.IsActivitySourceInOrder(request, activity))
                {
                    item = new ValidationError(SR.GetString("Error_BindActivityReference", new object[] { request.QualifiedName, activity.QualifiedName }), 0x12a, true) {
                        PropertyName = base.GetFullPropertyName(manager) + ".Name"
                    };
                }
                IDesignerHost service = manager.GetService(typeof(IDesignerHost)) as IDesignerHost;
                WorkflowDesignerLoader loader = manager.GetService(typeof(WorkflowDesignerLoader)) as WorkflowDesignerLoader;
                if ((service != null) && (loader != null))
                {
                    Type srcType = null;
                    if (service.RootComponent == request)
                    {
                        ITypeProvider provider = manager.GetService(typeof(ITypeProvider)) as ITypeProvider;
                        if (provider == null)
                        {
                            throw new InvalidOperationException(SR.GetString("General_MissingService", new object[] { typeof(ITypeProvider).FullName }));
                        }
                        srcType = provider.GetType(service.RootComponentClassName);
                    }
                    else
                    {
                        request.GetType();
                    }
                    if (srcType != null)
                    {
                        MemberInfo memberInfo = MemberBind.GetMemberInfo(srcType, bind.Path);
                        if ((memberInfo == null) || ((memberInfo is PropertyInfo) && !(memberInfo as PropertyInfo).CanRead))
                        {
                            item = new ValidationError(SR.GetString("Error_InvalidMemberPath", new object[] { request.QualifiedName, bind.Path }), 300) {
                                PropertyName = base.GetFullPropertyName(manager) + ".Path"
                            };
                        }
                        else
                        {
                            Type memberType = null;
                            if (memberInfo is FieldInfo)
                            {
                                memberType = ((FieldInfo) memberInfo).FieldType;
                            }
                            else if (memberInfo is PropertyInfo)
                            {
                                memberType = ((PropertyInfo) memberInfo).PropertyType;
                            }
                            else if (memberInfo is EventInfo)
                            {
                                memberType = ((EventInfo) memberInfo).EventHandlerType;
                            }
                            if (!DoesTargetTypeMatch(validationContext.TargetType, memberType, validationContext.Access))
                            {
                                if (typeof(WorkflowParameterBinding).IsAssignableFrom(memberInfo.DeclaringType))
                                {
                                    item = new ValidationError(SR.GetString("Warning_ParameterBinding", new object[] { bind.Path, request.QualifiedName, validationContext.TargetType.FullName }), 0x624, true) {
                                        PropertyName = base.GetFullPropertyName(manager) + ".Path"
                                    };
                                }
                                else
                                {
                                    item = new ValidationError(SR.GetString("Error_TargetTypeMismatch", new object[] { memberInfo.Name, memberType.FullName, validationContext.TargetType.FullName }), 0x12d) {
                                        PropertyName = base.GetFullPropertyName(manager) + ".Path"
                                    };
                                }
                            }
                        }
                    }
                }
                else
                {
                    MemberInfo info2 = MemberBind.GetMemberInfo(request.GetType(), bind.Path);
                    if ((info2 == null) || ((info2 is PropertyInfo) && !(info2 as PropertyInfo).CanRead))
                    {
                        item = new ValidationError(SR.GetString("Error_InvalidMemberPath", new object[] { request.QualifiedName, bind.Path }), 300) {
                            PropertyName = base.GetFullPropertyName(manager) + ".Path"
                        };
                    }
                    else
                    {
                        DependencyProperty dependencyProperty = DependencyProperty.FromName(info2.Name, info2.DeclaringType);
                        object obj2 = BindHelpers.ResolveActivityPath(request, bind.Path);
                        if (obj2 == null)
                        {
                            Type fromType = null;
                            if (info2 is FieldInfo)
                            {
                                fromType = ((FieldInfo) info2).FieldType;
                            }
                            else if (info2 is PropertyInfo)
                            {
                                fromType = ((PropertyInfo) info2).PropertyType;
                            }
                            else if (info2 is EventInfo)
                            {
                                fromType = ((EventInfo) info2).EventHandlerType;
                            }
                            if (!TypeProvider.IsAssignable(typeof(ActivityBind), fromType) && !DoesTargetTypeMatch(validationContext.TargetType, fromType, validationContext.Access))
                            {
                                if (typeof(WorkflowParameterBinding).IsAssignableFrom(info2.DeclaringType))
                                {
                                    item = new ValidationError(SR.GetString("Warning_ParameterBinding", new object[] { bind.Path, request.QualifiedName, validationContext.TargetType.FullName }), 0x624, true) {
                                        PropertyName = base.GetFullPropertyName(manager) + ".Path"
                                    };
                                }
                                else
                                {
                                    item = new ValidationError(SR.GetString("Error_TargetTypeMismatch", new object[] { info2.Name, fromType.FullName, validationContext.TargetType.FullName }), 0x12d) {
                                        PropertyName = base.GetFullPropertyName(manager) + ".Path"
                                    };
                                }
                            }
                        }
                        else if ((obj2 is ActivityBind) && (request.Parent != null))
                        {
                            ActivityBind bind2 = obj2 as ActivityBind;
                            bool flag = false;
                            BindRecursionContext context = manager.Context[typeof(BindRecursionContext)] as BindRecursionContext;
                            if (context == null)
                            {
                                context = new BindRecursionContext();
                                manager.Context.Push(context);
                                flag = true;
                            }
                            if (context.Contains(activity, bind))
                            {
                                item = new ValidationError(SR.GetString("Bind_ActivityDataSourceRecursionDetected"), 0x12f) {
                                    PropertyName = base.GetFullPropertyName(manager) + ".Path"
                                };
                            }
                            else
                            {
                                context.Add(activity, bind);
                                PropertyValidationContext propertyValidationContext = null;
                                if (dependencyProperty != null)
                                {
                                    propertyValidationContext = new PropertyValidationContext(request, dependencyProperty);
                                }
                                else
                                {
                                    propertyValidationContext = new PropertyValidationContext(request, info2 as PropertyInfo, info2.Name);
                                }
                                errors.AddRange(ValidationHelpers.ValidateProperty(manager, request, bind2, propertyValidationContext, validationContext));
                            }
                            if (flag)
                            {
                                manager.Context.Pop();
                            }
                        }
                        else if ((validationContext.TargetType != null) && !DoesTargetTypeMatch(validationContext.TargetType, obj2.GetType(), validationContext.Access))
                        {
                            item = new ValidationError(SR.GetString("Error_TargetTypeMismatch", new object[] { info2.Name, obj2.GetType().FullName, validationContext.TargetType.FullName }), 0x12d) {
                                PropertyName = base.GetFullPropertyName(manager) + ".Path"
                            };
                        }
                    }
                }
            }
            if (item != null)
            {
                errors.Add(item);
            }
            return errors;
        }

        private ValidationErrorCollection ValidateActivityBind(ValidationManager manager, object obj)
        {
            ValidationErrorCollection errors = base.Validate(manager, obj);
            ActivityBind bind = obj as ActivityBind;
            PropertyValidationContext validationContext = manager.Context[typeof(PropertyValidationContext)] as PropertyValidationContext;
            if (validationContext == null)
            {
                throw new InvalidOperationException(SR.GetString("Error_ContextStackItemMissing", new object[] { typeof(BindValidationContext).Name }));
            }
            if (!(manager.Context[typeof(Activity)] is Activity))
            {
                throw new InvalidOperationException(SR.GetString("Error_ContextStackItemMissing", new object[] { typeof(Activity).Name }));
            }
            ValidationError item = null;
            BindValidationContext context2 = manager.Context[typeof(BindValidationContext)] as BindValidationContext;
            if (context2 == null)
            {
                Type baseType = BindHelpers.GetBaseType(manager, validationContext);
                if (baseType != null)
                {
                    AccessTypes accessType = BindHelpers.GetAccessType(manager, validationContext);
                    context2 = new BindValidationContext(baseType, accessType);
                }
            }
            if (context2 != null)
            {
                Type targetType = context2.TargetType;
                if (item == null)
                {
                    errors.AddRange(this.ValidateActivity(manager, bind, new BindValidationContext(targetType, context2.Access)));
                }
            }
            if (item != null)
            {
                errors.Add(item);
            }
            return errors;
        }
    }
}

