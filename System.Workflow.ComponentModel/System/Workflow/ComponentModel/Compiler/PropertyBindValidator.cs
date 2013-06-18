namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.Reflection;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;

    internal sealed class PropertyBindValidator : Validator
    {
        public override ValidationErrorCollection Validate(ValidationManager manager, object obj)
        {
            ValidationErrorCollection errors = base.Validate(manager, obj);
            PropertyBind bind = obj as PropertyBind;
            if (bind == null)
            {
                throw new ArgumentException(SR.GetString("Error_UnexpectedArgumentType", new object[] { typeof(PropertyBind).FullName }), "obj");
            }
            PropertyValidationContext validationContext = manager.Context[typeof(PropertyValidationContext)] as PropertyValidationContext;
            if (validationContext == null)
            {
                throw new InvalidOperationException(SR.GetString("Error_ContextStackItemMissing", new object[] { typeof(BindValidationContext).Name }));
            }
            Activity activity = manager.Context[typeof(Activity)] as Activity;
            if (activity == null)
            {
                throw new InvalidOperationException(SR.GetString("Error_ContextStackItemMissing", new object[] { typeof(Activity).Name }));
            }
            ValidationError item = null;
            if (string.IsNullOrEmpty(bind.Name))
            {
                item = new ValidationError(SR.GetString("Error_PropertyNotSet", new object[] { "Name" }), 0x116) {
                    PropertyName = base.GetFullPropertyName(manager) + ".Name"
                };
            }
            else
            {
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
                        errors.AddRange(this.ValidateBindProperty(manager, activity, bind, new BindValidationContext(targetType, context2.Access)));
                    }
                }
            }
            if (item != null)
            {
                errors.Add(item);
            }
            return errors;
        }

        private ValidationErrorCollection ValidateBindProperty(ValidationManager manager, Activity activity, PropertyBind bind, BindValidationContext validationContext)
        {
            ValidationErrorCollection errors = new ValidationErrorCollection();
            string name = bind.Name;
            Activity enclosingActivity = Helpers.GetEnclosingActivity(activity);
            Activity refActivity = enclosingActivity;
            if ((name.IndexOf('.') != -1) && (refActivity != null))
            {
                refActivity = Helpers.GetDataSourceActivity(activity, bind.Name, out name);
            }
            if (refActivity == null)
            {
                ValidationError error = new ValidationError(SR.GetString("Error_NoEnclosingContext", new object[] { activity.Name }), 0x130) {
                    PropertyName = base.GetFullPropertyName(manager) + ".Name"
                };
                errors.Add(error);
                return errors;
            }
            ValidationError item = null;
            PropertyInfo property = null;
            Type activityType = null;
            if (property == null)
            {
                activityType = BindValidatorHelper.GetActivityType(manager, refActivity);
                if (activityType != null)
                {
                    property = activityType.GetProperty(name, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                }
            }
            if (activityType == null)
            {
                item = new ValidationError(SR.GetString("Error_TypeNotResolvedInPropertyName", new object[] { "Name" }), 0x163) {
                    PropertyName = base.GetFullPropertyName(manager)
                };
            }
            else if (property == null)
            {
                item = new ValidationError(SR.GetString("Error_PropertyNotExists", new object[] { base.GetFullPropertyName(manager), name }), 0x164) {
                    PropertyName = base.GetFullPropertyName(manager)
                };
            }
            else if (!property.CanRead)
            {
                item = new ValidationError(SR.GetString("Error_PropertyReferenceNoGetter", new object[] { base.GetFullPropertyName(manager), name }), 360) {
                    PropertyName = base.GetFullPropertyName(manager)
                };
            }
            else if (property.GetGetMethod() == null)
            {
                item = new ValidationError(SR.GetString("Error_PropertyReferenceGetterNoAccess", new object[] { base.GetFullPropertyName(manager), name }), 0x60a) {
                    PropertyName = base.GetFullPropertyName(manager)
                };
            }
            else if (((refActivity != enclosingActivity) && !property.GetGetMethod().IsAssembly) && !property.GetGetMethod().IsPublic)
            {
                item = new ValidationError(SR.GetString("Error_PropertyNotAccessible", new object[] { base.GetFullPropertyName(manager), name }), 0x165) {
                    PropertyName = base.GetFullPropertyName(manager)
                };
            }
            else if (property.PropertyType == null)
            {
                item = new ValidationError(SR.GetString("Error_PropertyTypeNotResolved", new object[] { base.GetFullPropertyName(manager), name }), 0x166) {
                    PropertyName = base.GetFullPropertyName(manager)
                };
            }
            else
            {
                MemberInfo memberInfo = property;
                if (((bind.Path == null) || (bind.Path.Length == 0)) && ((validationContext.TargetType != null) && !ActivityBindValidator.DoesTargetTypeMatch(validationContext.TargetType, property.PropertyType, validationContext.Access)))
                {
                    item = new ValidationError(SR.GetString("Error_PropertyTypeMismatch", new object[] { base.GetFullPropertyName(manager), property.PropertyType.FullName, validationContext.TargetType.FullName }), 0x167) {
                        PropertyName = base.GetFullPropertyName(manager)
                    };
                }
                else if (!string.IsNullOrEmpty(bind.Path))
                {
                    memberInfo = MemberBind.GetMemberInfo(property.PropertyType, bind.Path);
                    if (memberInfo == null)
                    {
                        item = new ValidationError(SR.GetString("Error_InvalidMemberPath", new object[] { name, bind.Path }), 300) {
                            PropertyName = base.GetFullPropertyName(manager) + ".Path"
                        };
                    }
                    else
                    {
                        using ((WorkflowCompilationContext.Current == null) ? WorkflowCompilationContext.CreateScope(manager) : null)
                        {
                            if (WorkflowCompilationContext.Current.CheckTypes)
                            {
                                item = MemberBind.ValidateTypesInPath(property.PropertyType, bind.Path);
                                if (item != null)
                                {
                                    item.PropertyName = base.GetFullPropertyName(manager) + ".Path";
                                }
                            }
                        }
                        if (item == null)
                        {
                            Type memberType = (memberInfo is FieldInfo) ? (memberInfo as FieldInfo).FieldType : (memberInfo as PropertyInfo).PropertyType;
                            if (!ActivityBindValidator.DoesTargetTypeMatch(validationContext.TargetType, memberType, validationContext.Access))
                            {
                                item = new ValidationError(SR.GetString("Error_TargetTypeDataSourcePathMismatch", new object[] { validationContext.TargetType.FullName }), 0x141) {
                                    PropertyName = base.GetFullPropertyName(manager) + ".Path"
                                };
                            }
                        }
                    }
                }
                if (item == null)
                {
                    if (memberInfo is PropertyInfo)
                    {
                        PropertyInfo info3 = memberInfo as PropertyInfo;
                        if (!info3.CanRead && ((validationContext.Access & AccessTypes.Read) != 0))
                        {
                            item = new ValidationError(SR.GetString("Error_PropertyNoGetter", new object[] { info3.Name, bind.Path }), 0x142) {
                                PropertyName = base.GetFullPropertyName(manager) + ".Path"
                            };
                        }
                        else if (!info3.CanWrite && ((validationContext.Access & AccessTypes.Write) != 0))
                        {
                            item = new ValidationError(SR.GetString("Error_PropertyNoSetter", new object[] { info3.Name, bind.Path }), 0x143) {
                                PropertyName = base.GetFullPropertyName(manager) + ".Path"
                            };
                        }
                    }
                    else if (memberInfo is FieldInfo)
                    {
                        FieldInfo info4 = memberInfo as FieldInfo;
                        if (((info4.Attributes & (FieldAttributes.Literal | FieldAttributes.InitOnly)) != FieldAttributes.PrivateScope) && ((validationContext.Access & AccessTypes.Write) != 0))
                        {
                            item = new ValidationError(SR.GetString("Error_ReadOnlyField", new object[] { info4.Name }), 0x145) {
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
    }
}

