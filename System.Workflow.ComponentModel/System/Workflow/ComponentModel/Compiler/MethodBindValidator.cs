namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;

    internal sealed class MethodBindValidator : Validator
    {
        public override ValidationErrorCollection Validate(ValidationManager manager, object obj)
        {
            ValidationErrorCollection errors = base.Validate(manager, obj);
            MethodBind bind = obj as MethodBind;
            if (bind == null)
            {
                throw new ArgumentException(SR.GetString("Error_UnexpectedArgumentType", new object[] { typeof(MethodBind).FullName }), "obj");
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
                        errors.AddRange(this.ValidateMethod(manager, activity, bind, new BindValidationContext(targetType, context2.Access)));
                    }
                }
            }
            if (item != null)
            {
                errors.Add(item);
            }
            return errors;
        }

        private ValidationErrorCollection ValidateMethod(ValidationManager manager, Activity activity, MethodBind bind, BindValidationContext validationBindContext)
        {
            ValidationErrorCollection errors = new ValidationErrorCollection();
            if ((validationBindContext.Access & AccessTypes.Write) != 0)
            {
                ValidationError item = new ValidationError(SR.GetString("Error_HandlerReadOnly"), 0x133) {
                    PropertyName = base.GetFullPropertyName(manager)
                };
                errors.Add(item);
                return errors;
            }
            if (!TypeProvider.IsAssignable(typeof(Delegate), validationBindContext.TargetType))
            {
                ValidationError error2 = new ValidationError(SR.GetString("Error_TypeNotDelegate", new object[] { validationBindContext.TargetType.FullName }), 0x134) {
                    PropertyName = base.GetFullPropertyName(manager)
                };
                errors.Add(error2);
                return errors;
            }
            string name = bind.Name;
            Activity enclosingActivity = Helpers.GetEnclosingActivity(activity);
            if ((name.IndexOf('.') != -1) && (enclosingActivity != null))
            {
                enclosingActivity = Helpers.GetDataSourceActivity(activity, bind.Name, out name);
            }
            if (enclosingActivity == null)
            {
                ValidationError error3 = new ValidationError(SR.GetString("Error_NoEnclosingContext", new object[] { activity.Name }), 0x130) {
                    PropertyName = base.GetFullPropertyName(manager) + ".Name"
                };
                errors.Add(error3);
                return errors;
            }
            string errorText = string.Empty;
            int errorNumber = -1;
            Type dataSourceClass = Helpers.GetDataSourceClass(enclosingActivity, manager);
            if (dataSourceClass == null)
            {
                errorText = SR.GetString("Error_TypeNotResolvedInMethodName", new object[] { base.GetFullPropertyName(manager) + ".Name" });
                errorNumber = 0x135;
            }
            else
            {
                try
                {
                    ValidationHelpers.ValidateIdentifier(manager, name);
                }
                catch (Exception exception)
                {
                    errors.Add(new ValidationError(exception.Message, 0x119));
                }
                MethodInfo method = validationBindContext.TargetType.GetMethod("Invoke");
                if (method == null)
                {
                    throw new Exception(SR.GetString("Error_DelegateNoInvoke", new object[] { validationBindContext.TargetType.FullName }));
                }
                List<Type> list = new List<Type>();
                foreach (ParameterInfo info2 in method.GetParameters())
                {
                    list.Add(info2.ParameterType);
                }
                MethodInfo info3 = Helpers.GetMethodExactMatch(dataSourceClass, name, BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance, null, list.ToArray(), null);
                if (info3 == null)
                {
                    if (dataSourceClass.GetMethod(name, BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance) != null)
                    {
                        errorText = SR.GetString("Error_MethodSignatureMismatch", new object[] { base.GetFullPropertyName(manager) + ".Name" });
                        errorNumber = 310;
                    }
                    else
                    {
                        errorText = SR.GetString("Error_MethodNotExists", new object[] { base.GetFullPropertyName(manager) + ".Name", bind.Name });
                        errorNumber = 0x137;
                    }
                }
                else if (!method.ReturnType.Equals(info3.ReturnType))
                {
                    errorText = SR.GetString("Error_MethodReturnTypeMismatch", new object[] { base.GetFullPropertyName(manager), method.ReturnType.FullName });
                    errorNumber = 0x139;
                }
            }
            if (errorText.Length > 0)
            {
                ValidationError error4 = new ValidationError(errorText, errorNumber) {
                    PropertyName = base.GetFullPropertyName(manager) + ".Path"
                };
                errors.Add(error4);
            }
            return errors;
        }
    }
}

