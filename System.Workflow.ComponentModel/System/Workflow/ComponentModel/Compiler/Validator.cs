namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Workflow.ComponentModel;

    public class Validator
    {
        protected string GetFullPropertyName(ValidationManager manager)
        {
            if (manager == null)
            {
                throw new ArgumentNullException("manager");
            }
            string propertyName = string.Empty;
            for (int i = 0; manager.Context[i] != null; i++)
            {
                if (manager.Context[i] is PropertyValidationContext)
                {
                    PropertyValidationContext context = manager.Context[i] as PropertyValidationContext;
                    if (context.PropertyName == string.Empty)
                    {
                        propertyName = string.Empty;
                    }
                    else if (propertyName == string.Empty)
                    {
                        propertyName = context.PropertyName;
                    }
                    else
                    {
                        propertyName = context.PropertyName + "." + propertyName;
                    }
                }
            }
            return propertyName;
        }

        public virtual ValidationErrorCollection Validate(ValidationManager manager, object obj)
        {
            if (manager == null)
            {
                throw new ArgumentNullException("manager");
            }
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            return new ValidationErrorCollection();
        }

        public virtual ValidationError ValidateActivityChange(Activity activity, ActivityChangeAction action)
        {
            if (activity == null)
            {
                throw new ArgumentNullException("activity");
            }
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }
            return null;
        }

        public virtual ValidationErrorCollection ValidateProperties(ValidationManager manager, object obj)
        {
            if (manager == null)
            {
                throw new ArgumentNullException("manager");
            }
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            ValidationErrorCollection errors = new ValidationErrorCollection();
            Activity activity = manager.Context[typeof(Activity)] as Activity;
            Walker walker = new Walker(true);
            walker.FoundProperty += delegate (Walker w, WalkerEventArgs args) {
                if ((args.CurrentProperty != null) && (DependencyProperty.FromName(args.CurrentProperty.Name, args.CurrentProperty.DeclaringType) == null))
                {
                    object[] customAttributes = args.CurrentProperty.GetCustomAttributes(typeof(ValidationOptionAttribute), true);
                    if (((customAttributes.Length > 0) ? ((ValidationOptionAttribute) customAttributes[0]).ValidationOption : ValidationOption.Optional) != ValidationOption.None)
                    {
                        errors.AddRange(this.ValidateProperty(args.CurrentProperty, args.CurrentPropertyOwner, args.CurrentValue, manager));
                        args.Action = WalkerAction.Skip;
                    }
                }
            };
            walker.WalkProperties(activity, obj);
            return errors;
        }

        protected internal ValidationErrorCollection ValidateProperty(PropertyInfo propertyInfo, object propertyOwner, object propertyValue, ValidationManager manager)
        {
            ValidationErrorCollection errors = new ValidationErrorCollection();
            object[] customAttributes = propertyInfo.GetCustomAttributes(typeof(ValidationOptionAttribute), true);
            if (customAttributes.Length > 0)
            {
                ValidationOption validationOption = ((ValidationOptionAttribute) customAttributes[0]).ValidationOption;
            }
            PropertyValidationContext context = new PropertyValidationContext(propertyOwner, propertyInfo, propertyInfo.Name);
            manager.Context.Push(context);
            try
            {
                if (propertyValue == null)
                {
                    return errors;
                }
                errors.AddRange(ValidationHelpers.ValidateObject(manager, propertyValue));
                if (!(propertyValue is IList))
                {
                    return errors;
                }
                PropertyValidationContext context2 = new PropertyValidationContext(propertyValue, null, "");
                manager.Context.Push(context2);
                try
                {
                    foreach (object obj2 in (IList) propertyValue)
                    {
                        errors.AddRange(ValidationHelpers.ValidateObject(manager, obj2));
                    }
                    return errors;
                }
                finally
                {
                    manager.Context.Pop();
                }
            }
            finally
            {
                manager.Context.Pop();
            }
            return errors;
        }
    }
}

