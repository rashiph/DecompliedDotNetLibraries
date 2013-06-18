namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;
    using System.Workflow.ComponentModel.Serialization;

    public class DependencyObjectValidator : Validator
    {
        public override ValidationErrorCollection Validate(ValidationManager manager, object obj)
        {
            if (manager == null)
            {
                throw new ArgumentNullException("manager");
            }
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            ValidationErrorCollection errors = base.Validate(manager, obj);
            DependencyObject dependencyObject = obj as DependencyObject;
            if (dependencyObject == null)
            {
                throw new ArgumentException(SR.GetString("Error_UnexpectedArgumentType", new object[] { typeof(DependencyObject).FullName }), "obj");
            }
            ArrayList list = new ArrayList();
            foreach (DependencyProperty property in DependencyProperty.FromType(dependencyObject.GetType()))
            {
                if (!property.IsAttached)
                {
                    list.Add(property);
                }
            }
            foreach (DependencyProperty property2 in dependencyObject.MetaDependencyProperties)
            {
                if (property2.IsAttached && (obj.GetType().GetProperty(property2.Name, BindingFlags.Public | BindingFlags.Instance) == null))
                {
                    list.Add(property2);
                }
            }
            foreach (DependencyProperty property3 in list)
            {
                object[] attributes = property3.DefaultMetadata.GetAttributes(typeof(ValidationOptionAttribute));
                if (((attributes.Length > 0) ? ((ValidationOptionAttribute) attributes[0]).ValidationOption : ValidationOption.Optional) != ValidationOption.None)
                {
                    errors.AddRange(this.ValidateDependencyProperty(dependencyObject, property3, manager));
                }
            }
            return errors;
        }

        private ValidationErrorCollection ValidateDependencyProperty(DependencyObject dependencyObject, DependencyProperty dependencyProperty, ValidationManager manager)
        {
            ValidationErrorCollection errors = new ValidationErrorCollection();
            Attribute[] attributes = dependencyProperty.DefaultMetadata.GetAttributes(typeof(ValidationOptionAttribute));
            ValidationOption option = (attributes.Length > 0) ? ((ValidationOptionAttribute) attributes[0]).ValidationOption : ValidationOption.Optional;
            Activity propertyOwner = manager.Context[typeof(Activity)] as Activity;
            if (propertyOwner == null)
            {
                throw new InvalidOperationException(SR.GetString("Error_ContextStackItemMissing", new object[] { typeof(Activity).FullName }));
            }
            PropertyValidationContext context = new PropertyValidationContext(propertyOwner, dependencyProperty);
            manager.Context.Push(context);
            try
            {
                if (dependencyProperty.DefaultMetadata.DefaultValue != null)
                {
                    if (!dependencyProperty.PropertyType.IsValueType && (dependencyProperty.PropertyType != typeof(string)))
                    {
                        errors.Add(new ValidationError(SR.GetString("Error_PropertyDefaultIsReference", new object[] { dependencyProperty.Name }), 0x1a8));
                    }
                    else if (!dependencyProperty.PropertyType.IsAssignableFrom(dependencyProperty.DefaultMetadata.DefaultValue.GetType()))
                    {
                        errors.Add(new ValidationError(SR.GetString("Error_PropertyDefaultTypeMismatch", new object[] { dependencyProperty.Name, dependencyProperty.PropertyType.FullName, dependencyProperty.DefaultMetadata.DefaultValue.GetType().FullName }), 0x1a9));
                    }
                }
                object propValue = null;
                if (dependencyObject.IsBindingSet(dependencyProperty))
                {
                    propValue = dependencyObject.GetBinding(dependencyProperty);
                }
                else if (!dependencyProperty.IsEvent)
                {
                    propValue = dependencyObject.GetValue(dependencyProperty);
                }
                if ((propValue == null) || (propValue == dependencyProperty.DefaultMetadata.DefaultValue))
                {
                    if (dependencyProperty.IsEvent)
                    {
                        propValue = dependencyObject.GetHandler(dependencyProperty);
                        if (propValue == null)
                        {
                            propValue = WorkflowMarkupSerializationHelpers.GetEventHandlerName(dependencyObject, dependencyProperty.Name);
                        }
                        if ((propValue is string) && !string.IsNullOrEmpty((string) propValue))
                        {
                            errors.AddRange(this.ValidateEvent(propertyOwner, dependencyProperty, propValue, manager));
                        }
                    }
                    else
                    {
                        propValue = dependencyObject.GetValue(dependencyProperty);
                    }
                }
                bool flag = (propertyOwner.Parent != null) || ((propertyOwner is CompositeActivity) && (((CompositeActivity) propertyOwner).EnabledActivities.Count != 0));
                if (((option == ValidationOption.Required) && ((propValue == null) || ((propValue is string) && string.IsNullOrEmpty((string) propValue)))) && (dependencyProperty.DefaultMetadata.IsMetaProperty && flag))
                {
                    errors.Add(ValidationError.GetNotSetValidationError(base.GetFullPropertyName(manager)));
                    return errors;
                }
                if (propValue == null)
                {
                    return errors;
                }
                if (propValue is IList)
                {
                    PropertyValidationContext context2 = new PropertyValidationContext(propValue, null, string.Empty);
                    manager.Context.Push(context2);
                    try
                    {
                        foreach (object obj3 in (IList) propValue)
                        {
                            errors.AddRange(ValidationHelpers.ValidateObject(manager, obj3));
                        }
                        return errors;
                    }
                    finally
                    {
                        manager.Context.Pop();
                    }
                }
                if (dependencyProperty.ValidatorType != null)
                {
                    Validator validator = null;
                    try
                    {
                        validator = Activator.CreateInstance(dependencyProperty.ValidatorType) as Validator;
                        if (validator == null)
                        {
                            errors.Add(new ValidationError(SR.GetString("Error_CreateValidator", new object[] { dependencyProperty.ValidatorType.FullName }), 0x106));
                            return errors;
                        }
                        errors.AddRange(validator.Validate(manager, propValue));
                        return errors;
                    }
                    catch
                    {
                        errors.Add(new ValidationError(SR.GetString("Error_CreateValidator", new object[] { dependencyProperty.ValidatorType.FullName }), 0x106));
                        return errors;
                    }
                }
                errors.AddRange(ValidationHelpers.ValidateObject(manager, propValue));
            }
            finally
            {
                manager.Context.Pop();
            }
            return errors;
        }

        private ValidationErrorCollection ValidateEvent(Activity activity, DependencyProperty dependencyProperty, object propValue, ValidationManager manager)
        {
            ValidationErrorCollection errors = new ValidationErrorCollection();
            if ((propValue is string) && !string.IsNullOrEmpty((string) propValue))
            {
                bool flag = false;
                Type type = null;
                Activity rootActivity = Helpers.GetRootActivity(activity);
                Activity enclosingActivity = Helpers.GetEnclosingActivity(activity);
                string str = rootActivity.GetValue(WorkflowMarkupSerializer.XClassProperty) as string;
                if ((rootActivity == enclosingActivity) && !string.IsNullOrEmpty(str))
                {
                    ITypeProvider service = manager.GetService(typeof(ITypeProvider)) as ITypeProvider;
                    if (service == null)
                    {
                        throw new InvalidOperationException(SR.GetString("General_MissingService", new object[] { typeof(ITypeProvider).FullName }));
                    }
                    type = service.GetType(str);
                }
                else
                {
                    type = enclosingActivity.GetType();
                }
                if (type != null)
                {
                    MethodInfo method = dependencyProperty.PropertyType.GetMethod("Invoke");
                    if (method != null)
                    {
                        List<Type> list = new List<Type>();
                        foreach (ParameterInfo info2 in method.GetParameters())
                        {
                            list.Add(info2.ParameterType);
                        }
                        MethodInfo info3 = Helpers.GetMethodExactMatch(type, propValue as string, BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance, null, list.ToArray(), null);
                        if ((info3 != null) && TypeProvider.IsAssignable(method.ReturnType, info3.ReturnType))
                        {
                            flag = true;
                        }
                    }
                }
                if (!flag)
                {
                    ValidationError item = new ValidationError(SR.GetString("Error_CantResolveEventHandler", new object[] { dependencyProperty.Name, propValue as string }), 0x60f) {
                        PropertyName = base.GetFullPropertyName(manager)
                    };
                    errors.Add(item);
                }
            }
            return errors;
        }
    }
}

