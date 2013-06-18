namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.Reflection;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;

    internal sealed class FieldBindValidator : Validator
    {
        public override ValidationErrorCollection Validate(ValidationManager manager, object obj)
        {
            ValidationErrorCollection errors = base.Validate(manager, obj);
            FieldBind bind = obj as FieldBind;
            if (bind == null)
            {
                throw new ArgumentException(SR.GetString("Error_UnexpectedArgumentType", new object[] { typeof(FieldBind).FullName }), "obj");
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
                        errors.AddRange(this.ValidateField(manager, activity, bind, new BindValidationContext(targetType, context2.Access)));
                    }
                }
            }
            if (item != null)
            {
                errors.Add(item);
            }
            return errors;
        }

        private ValidationErrorCollection ValidateField(ValidationManager manager, Activity activity, FieldBind bind, BindValidationContext validationContext)
        {
            ValidationErrorCollection errors = new ValidationErrorCollection();
            string name = bind.Name;
            Activity enclosingActivity = Helpers.GetEnclosingActivity(activity);
            if ((name.IndexOf('.') != -1) && (enclosingActivity != null))
            {
                enclosingActivity = Helpers.GetDataSourceActivity(activity, bind.Name, out name);
            }
            if (enclosingActivity == null)
            {
                ValidationError error = new ValidationError(SR.GetString("Error_NoEnclosingContext", new object[] { activity.Name }), 0x130) {
                    PropertyName = base.GetFullPropertyName(manager) + ".Name"
                };
                errors.Add(error);
                return errors;
            }
            ValidationError item = null;
            Type dataSourceClass = Helpers.GetDataSourceClass(enclosingActivity, manager);
            if (dataSourceClass == null)
            {
                item = new ValidationError(SR.GetString("Error_TypeNotResolvedInFieldName", new object[] { "Name" }), 0x11f) {
                    PropertyName = base.GetFullPropertyName(manager)
                };
            }
            else
            {
                FieldInfo field = dataSourceClass.GetField(name, BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                if (field == null)
                {
                    item = new ValidationError(SR.GetString("Error_FieldNotExists", new object[] { base.GetFullPropertyName(manager), name }), 0x120) {
                        PropertyName = base.GetFullPropertyName(manager)
                    };
                }
                else if (field.FieldType == null)
                {
                    item = new ValidationError(SR.GetString("Error_FieldTypeNotResolved", new object[] { base.GetFullPropertyName(manager), name }), 290) {
                        PropertyName = base.GetFullPropertyName(manager)
                    };
                }
                else
                {
                    MemberInfo memberInfo = field;
                    if (((bind.Path == null) || (bind.Path.Length == 0)) && ((validationContext.TargetType != null) && !ActivityBindValidator.DoesTargetTypeMatch(validationContext.TargetType, field.FieldType, validationContext.Access)))
                    {
                        item = new ValidationError(SR.GetString("Error_FieldTypeMismatch", new object[] { base.GetFullPropertyName(manager), field.FieldType.FullName, validationContext.TargetType.FullName }), 0x13f) {
                            PropertyName = base.GetFullPropertyName(manager)
                        };
                    }
                    else if (!string.IsNullOrEmpty(bind.Path))
                    {
                        memberInfo = MemberBind.GetMemberInfo(field.FieldType, bind.Path);
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
                                    item = MemberBind.ValidateTypesInPath(field.FieldType, bind.Path);
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
            }
            if (item != null)
            {
                errors.Add(item);
            }
            return errors;
        }
    }
}

