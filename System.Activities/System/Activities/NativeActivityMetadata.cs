namespace System.Activities
{
    using System;
    using System.Activities.Validation;
    using System.Collections.ObjectModel;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct NativeActivityMetadata
    {
        private Activity activity;
        private LocationReferenceEnvironment environment;
        private bool createEmptyBindings;
        internal NativeActivityMetadata(Activity activity, LocationReferenceEnvironment environment, bool createEmptyBindings)
        {
            this.activity = activity;
            this.environment = environment;
            this.createEmptyBindings = createEmptyBindings;
        }

        internal bool CreateEmptyBindings
        {
            get
            {
                return this.createEmptyBindings;
            }
        }
        public LocationReferenceEnvironment Environment
        {
            get
            {
                return this.environment;
            }
        }
        public bool HasViolations
        {
            get
            {
                if (this.activity == null)
                {
                    return false;
                }
                return this.activity.HasTempViolations;
            }
        }
        public static bool operator ==(NativeActivityMetadata left, NativeActivityMetadata right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(NativeActivityMetadata left, NativeActivityMetadata right)
        {
            return !left.Equals(right);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is NativeActivityMetadata))
            {
                return false;
            }
            NativeActivityMetadata metadata = (NativeActivityMetadata) obj;
            return (((metadata.activity == this.activity) && (metadata.Environment == this.Environment)) && (metadata.CreateEmptyBindings == this.CreateEmptyBindings));
        }

        public override int GetHashCode()
        {
            if (this.activity == null)
            {
                return 0;
            }
            return this.activity.GetHashCode();
        }

        public void Bind(Argument binding, RuntimeArgument argument)
        {
            this.ThrowIfDisposed();
            Argument.TryBind(binding, argument, this.activity);
        }

        public void SetValidationErrorsCollection(Collection<ValidationError> validationErrors)
        {
            this.ThrowIfDisposed();
            ActivityUtilities.RemoveNulls(validationErrors);
            this.activity.SetTempValidationErrorCollection(validationErrors);
        }

        public void AddValidationError(string validationErrorMessage)
        {
            this.AddValidationError(new ValidationError(validationErrorMessage));
        }

        public void AddValidationError(ValidationError validationError)
        {
            this.ThrowIfDisposed();
            if (validationError != null)
            {
                this.activity.AddTempValidationError(validationError);
            }
        }

        public void SetArgumentsCollection(Collection<RuntimeArgument> arguments)
        {
            this.ThrowIfDisposed();
            ActivityUtilities.RemoveNulls(arguments);
            this.activity.SetArgumentsCollection(arguments, this.createEmptyBindings);
        }

        public void AddArgument(RuntimeArgument argument)
        {
            this.ThrowIfDisposed();
            if (argument != null)
            {
                this.activity.AddArgument(argument, this.createEmptyBindings);
            }
        }

        public void SetChildrenCollection(Collection<Activity> children)
        {
            this.ThrowIfDisposed();
            ActivityUtilities.RemoveNulls(children);
            this.activity.SetChildrenCollection(children);
        }

        public void AddChild(Activity child)
        {
            this.ThrowIfDisposed();
            if (child != null)
            {
                this.activity.AddChild(child);
            }
        }

        public void SetImplementationChildrenCollection(Collection<Activity> implementationChildren)
        {
            this.ThrowIfDisposed();
            ActivityUtilities.RemoveNulls(implementationChildren);
            this.activity.SetImplementationChildrenCollection(implementationChildren);
        }

        public void AddImplementationChild(Activity implementationChild)
        {
            this.ThrowIfDisposed();
            if (implementationChild != null)
            {
                this.activity.AddImplementationChild(implementationChild);
            }
        }

        public void SetImportedChildrenCollection(Collection<Activity> importedChildren)
        {
            this.ThrowIfDisposed();
            ActivityUtilities.RemoveNulls(importedChildren);
            this.activity.SetImportedChildrenCollection(importedChildren);
        }

        public void AddImportedChild(Activity importedChild)
        {
            this.ThrowIfDisposed();
            if (importedChild != null)
            {
                this.activity.AddImportedChild(importedChild);
            }
        }

        public void SetDelegatesCollection(Collection<ActivityDelegate> delegates)
        {
            this.ThrowIfDisposed();
            ActivityUtilities.RemoveNulls(delegates);
            this.activity.SetDelegatesCollection(delegates);
        }

        public void AddDelegate(ActivityDelegate activityDelegate)
        {
            this.ThrowIfDisposed();
            if (activityDelegate != null)
            {
                this.activity.AddDelegate(activityDelegate);
            }
        }

        public void SetImplementationDelegatesCollection(Collection<ActivityDelegate> implementationDelegates)
        {
            this.ThrowIfDisposed();
            ActivityUtilities.RemoveNulls(implementationDelegates);
            this.activity.SetImplementationDelegatesCollection(implementationDelegates);
        }

        public void AddImplementationDelegate(ActivityDelegate implementationDelegate)
        {
            this.ThrowIfDisposed();
            if (implementationDelegate != null)
            {
                this.activity.AddImplementationDelegate(implementationDelegate);
            }
        }

        public void SetImportedDelegatesCollection(Collection<ActivityDelegate> importedDelegates)
        {
            this.ThrowIfDisposed();
            ActivityUtilities.RemoveNulls(importedDelegates);
            this.activity.SetImportedDelegatesCollection(importedDelegates);
        }

        public void AddImportedDelegate(ActivityDelegate importedDelegate)
        {
            this.ThrowIfDisposed();
            if (importedDelegate != null)
            {
                this.activity.AddImportedDelegate(importedDelegate);
            }
        }

        public void SetVariablesCollection(Collection<Variable> variables)
        {
            this.ThrowIfDisposed();
            ActivityUtilities.RemoveNulls(variables);
            this.activity.SetVariablesCollection(variables);
        }

        public void AddVariable(Variable variable)
        {
            this.ThrowIfDisposed();
            if (variable != null)
            {
                this.activity.AddVariable(variable);
            }
        }

        public void SetImplementationVariablesCollection(Collection<Variable> implementationVariables)
        {
            this.ThrowIfDisposed();
            ActivityUtilities.RemoveNulls(implementationVariables);
            this.activity.SetImplementationVariablesCollection(implementationVariables);
        }

        public void AddImplementationVariable(Variable implementationVariable)
        {
            this.ThrowIfDisposed();
            if (implementationVariable != null)
            {
                this.activity.AddImplementationVariable(implementationVariable);
            }
        }

        public Collection<RuntimeArgument> GetArgumentsWithReflection()
        {
            return Activity.ReflectedInformation.GetArguments(this.activity);
        }

        public Collection<Activity> GetChildrenWithReflection()
        {
            return Activity.ReflectedInformation.GetChildren(this.activity);
        }

        public Collection<Variable> GetVariablesWithReflection()
        {
            return Activity.ReflectedInformation.GetVariables(this.activity);
        }

        public Collection<ActivityDelegate> GetDelegatesWithReflection()
        {
            return Activity.ReflectedInformation.GetDelegates(this.activity);
        }

        public void AddDefaultExtensionProvider<T>(Func<T> extensionProvider) where T: class
        {
            if (extensionProvider == null)
            {
                throw FxTrace.Exception.ArgumentNull("extensionProvider");
            }
            this.activity.AddDefaultExtensionProvider<T>(extensionProvider);
        }

        public void RequireExtension<T>() where T: class
        {
            this.activity.RequireExtension(typeof(T));
        }

        public void RequireExtension(Type extensionType)
        {
            if (extensionType == null)
            {
                throw FxTrace.Exception.ArgumentNull("extensionType");
            }
            if (extensionType.IsValueType)
            {
                throw FxTrace.Exception.Argument("extensionType", System.Activities.SR.RequireExtensionOnlyAcceptsReferenceTypes(extensionType.FullName));
            }
            this.activity.RequireExtension(extensionType);
        }

        internal void Dispose()
        {
            this.activity = null;
        }

        private void ThrowIfDisposed()
        {
            if (this.activity == null)
            {
                throw FxTrace.Exception.AsError(new ObjectDisposedException(this.ToString()));
            }
        }
    }
}

