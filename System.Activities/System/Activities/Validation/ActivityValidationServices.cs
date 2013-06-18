namespace System.Activities.Validation
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Text;

    public static class ActivityValidationServices
    {
        private static ValidationSettings defaultSettings = new ValidationSettings();
        internal static readonly ReadOnlyCollection<Activity> EmptyChildren = new ReadOnlyCollection<Activity>(new Activity[0]);
        internal static ReadOnlyCollection<ValidationError> EmptyValidationErrors = new ReadOnlyCollection<ValidationError>(new List<ValidationError>(0));

        private static Exception CreateExceptionFromValidationErrors(IList<ValidationError> validationErrors)
        {
            if ((validationErrors != null) && (validationErrors.Count > 0))
            {
                string message = GenerateExceptionString(validationErrors, ExceptionReason.InvalidTree);
                if (message != null)
                {
                    return new InvalidWorkflowException(message);
                }
            }
            return null;
        }

        private static string GenerateExceptionString(IList<ValidationError> validationErrors, ExceptionReason reason)
        {
            StringBuilder builder = null;
            for (int i = 0; i < validationErrors.Count; i++)
            {
                ValidationError error = validationErrors[i];
                if (!error.IsWarning)
                {
                    if (builder == null)
                    {
                        builder = new StringBuilder();
                        switch (reason)
                        {
                            case ExceptionReason.InvalidTree:
                                builder.Append(System.Activities.SR.ErrorsEncounteredWhileProcessingTree);
                                break;

                            case ExceptionReason.InvalidNullInputs:
                                builder.Append(System.Activities.SR.RootArgumentViolationsFoundNoInputs);
                                break;

                            case ExceptionReason.InvalidNonNullInputs:
                                builder.Append(System.Activities.SR.RootArgumentViolationsFound);
                                break;
                        }
                    }
                    string displayName = null;
                    if (error.Source != null)
                    {
                        displayName = error.Source.DisplayName;
                    }
                    else
                    {
                        displayName = "<UnknownActivity>";
                    }
                    builder.AppendLine();
                    builder.Append(string.Format(System.Activities.SR.Culture, "'{0}': {1}", new object[] { displayName, error.Message }));
                    if (builder.Length > 0x1000)
                    {
                        break;
                    }
                }
            }
            string str2 = null;
            if (builder != null)
            {
                str2 = builder.ToString();
                if (str2.Length > 0x1000)
                {
                    string tooManyViolationsForExceptionMessage = System.Activities.SR.TooManyViolationsForExceptionMessage;
                    str2 = str2.Substring(0, 0x1000 - tooManyViolationsForExceptionMessage.Length) + tooManyViolationsForExceptionMessage;
                }
            }
            return str2;
        }

        internal static string GenerateValidationErrorPrefix(Activity toValidate, ActivityUtilities.ActivityCallStack parentChain, out Activity source)
        {
            bool flag = true;
            string str = "";
            source = toValidate;
            for (int i = 0; i < parentChain.Count; i++)
            {
                ActivityUtilities.ChildActivity activity = parentChain[i];
                if (activity.Activity.MemberOf.Parent != null)
                {
                    flag = false;
                    break;
                }
            }
            while (source.MemberOf.Parent != null)
            {
                source = source.Parent;
            }
            if (toValidate.MemberOf.Parent != null)
            {
                return System.Activities.SR.ValidationErrorPrefixForHiddenActivity(source);
            }
            if (!flag)
            {
                str = System.Activities.SR.ValidationErrorPrefixForPublicActivityWithHiddenParent(source.Parent, source);
            }
            return str;
        }

        private static RuntimeArgument GetBoundRuntimeArgument(Activity expressionActivity)
        {
            Activity parent = expressionActivity.Parent;
            RuntimeArgument argument = null;
            for (int i = 0; i < parent.RuntimeArguments.Count; i++)
            {
                argument = parent.RuntimeArguments[i];
                if (object.ReferenceEquals(argument.BoundArgument.Expression, expressionActivity))
                {
                    return argument;
                }
            }
            return argument;
        }

        internal static List<Activity> GetChildren(ActivityUtilities.ChildActivity root, ActivityUtilities.ActivityCallStack parentChain, ProcessActivityTreeOptions options)
        {
            ActivityUtilities.FinishCachingSubtree(root, parentChain, options);
            List<Activity> list = new List<Activity>();
            foreach (Activity activity in WorkflowInspectionServices.GetActivities(root.Activity))
            {
                list.Add(activity);
            }
            for (int i = 0; i < list.Count; i++)
            {
                foreach (Activity activity2 in WorkflowInspectionServices.GetActivities(list[i]))
                {
                    list.Add(activity2);
                }
            }
            return list;
        }

        internal static bool HasErrors(IList<ValidationError> validationErrors)
        {
            if ((validationErrors != null) && (validationErrors.Count > 0))
            {
                for (int i = 0; i < validationErrors.Count; i++)
                {
                    if (!validationErrors[i].IsWarning)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static Activity Resolve(Activity root, string id)
        {
            return WorkflowInspectionServices.Resolve(root, id);
        }

        internal static void RunConstraints(ActivityUtilities.ChildActivity childActivity, ActivityUtilities.ActivityCallStack parentChain, IList<Constraint> constraints, ProcessActivityTreeOptions options, bool suppressGetChildrenViolations, ref IList<ValidationError> validationErrors)
        {
            if (constraints != null)
            {
                Activity activity = childActivity.Activity;
                LocationReferenceEnvironment parentEnvironment = activity.GetParentEnvironment();
                Dictionary<string, object> inputs = new Dictionary<string, object>(2);
                for (int i = 0; i < constraints.Count; i++)
                {
                    Constraint workflow = constraints[i];
                    if (workflow != null)
                    {
                        object obj2;
                        inputs["ToValidate"] = activity;
                        ValidationContext context = new ValidationContext(childActivity, parentChain, options, parentEnvironment);
                        inputs["ToValidateContext"] = context;
                        IDictionary<string, object> dictionary2 = null;
                        try
                        {
                            dictionary2 = WorkflowInvoker.Invoke(workflow, inputs);
                        }
                        catch (Exception exception)
                        {
                            if (Fx.IsFatal(exception))
                            {
                                throw;
                            }
                            ValidationError data = new ValidationError(System.Activities.SR.InternalConstraintException(workflow.DisplayName, activity.GetType().FullName, activity.DisplayName, exception.ToString()), false) {
                                Source = activity,
                                Id = activity.Id
                            };
                            ActivityUtilities.Add<ValidationError>(ref validationErrors, data);
                        }
                        if ((dictionary2 != null) && dictionary2.TryGetValue("ViolationList", out obj2))
                        {
                            IList<ValidationError> list = (IList<ValidationError>) obj2;
                            if (list.Count > 0)
                            {
                                Activity activity2;
                                if (validationErrors == null)
                                {
                                    validationErrors = new List<ValidationError>();
                                }
                                string str = GenerateValidationErrorPrefix(childActivity.Activity, parentChain, out activity2);
                                for (int j = 0; j < list.Count; j++)
                                {
                                    ValidationError item = list[j];
                                    item.Source = activity2;
                                    item.Id = activity2.Id;
                                    if (!string.IsNullOrEmpty(str))
                                    {
                                        item.Message = str + item.Message;
                                    }
                                    validationErrors.Add(item);
                                }
                            }
                        }
                        if (!suppressGetChildrenViolations)
                        {
                            context.AddGetChildrenErrors(ref validationErrors);
                        }
                    }
                }
            }
        }

        internal static void ThrowIfViolationsExist(IList<ValidationError> validationErrors)
        {
            Exception exception = CreateExceptionFromValidationErrors(validationErrors);
            if (exception != null)
            {
                throw FxTrace.Exception.AsError(exception);
            }
        }

        public static ValidationResults Validate(Activity toValidate)
        {
            return Validate(toValidate, defaultSettings);
        }

        public static ValidationResults Validate(Activity toValidate, ValidationSettings settings)
        {
            if (toValidate == null)
            {
                throw FxTrace.Exception.ArgumentNull("toValidate");
            }
            if (settings == null)
            {
                throw FxTrace.Exception.ArgumentNull("settings");
            }
            if (toValidate.HasBeenAssociatedWithAnInstance)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.RootActivityAlreadyAssociatedWithInstance(toValidate.DisplayName)));
            }
            InternalActivityValidationServices services = new InternalActivityValidationServices(settings, toValidate);
            return services.InternalValidate();
        }

        internal static void ValidateArguments(Activity activity, bool isRoot, ref IList<ValidationError> validationErrors)
        {
            Dictionary<string, List<RuntimeArgument>> dictionary;
            List<RuntimeArgument> list;
            ValidationHelper.OverloadGroupEquivalenceInfo info;
            if (ValidationHelper.GatherAndValidateOverloads(activity, out dictionary, out list, out info, ref validationErrors) && !isRoot)
            {
                ValidationHelper.ValidateArguments(activity, info, dictionary, list, null, ref validationErrors);
            }
            if (isRoot)
            {
                activity.OverloadGroups = dictionary;
                activity.RequiredArgumentsNotInOverloadGroups = list;
                activity.EquivalenceInfo = info;
            }
        }

        internal static void ValidateEvaluationOrder(IList<RuntimeArgument> runtimeArguments, Activity referenceActivity, ref IList<ValidationError> validationErrors)
        {
            for (int i = 0; i < (runtimeArguments.Count - 1); i++)
            {
                RuntimeArgument argument = runtimeArguments[i];
                RuntimeArgument argument2 = runtimeArguments[i + 1];
                if ((argument.IsEvaluationOrderSpecified && argument2.IsEvaluationOrderSpecified) && (argument.BoundArgument.EvaluationOrder == argument2.BoundArgument.EvaluationOrder))
                {
                    ActivityUtilities.Add<ValidationError>(ref validationErrors, new ValidationError(System.Activities.SR.DuplicateEvaluationOrderValues(referenceActivity.DisplayName, argument.BoundArgument.EvaluationOrder), false, argument.Name, referenceActivity));
                }
            }
        }

        internal static void ValidateRootInputs(Activity rootActivity, IDictionary<string, object> inputs)
        {
            IList<ValidationError> validationErrors = null;
            ValidationHelper.ValidateArguments(rootActivity, rootActivity.EquivalenceInfo, rootActivity.OverloadGroups, rootActivity.RequiredArgumentsNotInOverloadGroups, inputs, ref validationErrors);
            if (inputs != null)
            {
                List<string> c = null;
                IEnumerable<RuntimeArgument> enumerable = from a in rootActivity.RuntimeArguments
                    where ArgumentDirectionHelper.IsIn(a.Direction)
                    select a;
                foreach (string str in inputs.Keys)
                {
                    bool flag = false;
                    foreach (RuntimeArgument argument in enumerable)
                    {
                        if (argument.Name == str)
                        {
                            flag = true;
                            object obj2 = null;
                            if (inputs.TryGetValue(str, out obj2) && !TypeHelper.AreTypesCompatible(obj2, argument.Type))
                            {
                                ActivityUtilities.Add<ValidationError>(ref validationErrors, new ValidationError(System.Activities.SR.InputParametersTypeMismatch(argument.Type, argument.Name), rootActivity));
                            }
                            break;
                        }
                    }
                    if (!flag)
                    {
                        if (c == null)
                        {
                            c = new List<string>();
                        }
                        c.Add(str);
                    }
                }
                if (c != null)
                {
                    ActivityUtilities.Add<ValidationError>(ref validationErrors, new ValidationError(System.Activities.SR.UnusedInputArguments(c.AsCommaSeparatedValues()), rootActivity));
                }
            }
            if ((validationErrors != null) && (validationErrors.Count > 0))
            {
                string paramName = "rootArgumentValues";
                ExceptionReason invalidNonNullInputs = ExceptionReason.InvalidNonNullInputs;
                if (inputs == null)
                {
                    paramName = "program";
                    invalidNonNullInputs = ExceptionReason.InvalidNullInputs;
                }
                string message = GenerateExceptionString(validationErrors, invalidNonNullInputs);
                if (message != null)
                {
                    throw FxTrace.Exception.Argument(paramName, message);
                }
            }
        }

        private enum ExceptionReason
        {
            InvalidTree,
            InvalidNullInputs,
            InvalidNonNullInputs
        }

        private class InternalActivityValidationServices
        {
            private IList<ValidationError> errors;
            private Activity expressionRoot;
            private ProcessActivityTreeOptions options;
            private Activity rootToValidate;
            private ValidationSettings settings;

            internal InternalActivityValidationServices(ValidationSettings settings, Activity toValidate)
            {
                this.settings = settings;
                this.rootToValidate = toValidate;
            }

            internal ValidationResults InternalValidate()
            {
                this.options = ProcessActivityTreeOptions.GetValidationOptions(this.settings);
                if (this.settings.OnlyUseAdditionalConstraints)
                {
                    IList<ValidationError> validationErrors = null;
                    ActivityUtilities.CacheRootMetadata(this.rootToValidate, null, this.options, new ActivityUtilities.ProcessActivityCallback(this.ValidateElement), ref validationErrors);
                }
                else
                {
                    ActivityUtilities.CacheRootMetadata(this.rootToValidate, null, this.options, new ActivityUtilities.ProcessActivityCallback(this.ValidateElement), ref this.errors);
                }
                return new ValidationResults(this.errors);
            }

            private void ValidateElement(ActivityUtilities.ChildActivity childActivity, ActivityUtilities.ActivityCallStack parentChain)
            {
                Activity objA = childActivity.Activity;
                if (!this.settings.SingleLevel || object.ReferenceEquals(objA, this.rootToValidate))
                {
                    if (this.settings.HasAdditionalConstraints)
                    {
                        bool suppressGetChildrenViolations = this.settings.OnlyUseAdditionalConstraints || this.settings.SingleLevel;
                        for (Type type = objA.GetType(); type != null; type = type.BaseType)
                        {
                            IList<Constraint> list;
                            if (this.settings.AdditionalConstraints.TryGetValue(type, out list))
                            {
                                ActivityValidationServices.RunConstraints(childActivity, parentChain, list, this.options, suppressGetChildrenViolations, ref this.errors);
                            }
                            if (type.IsGenericType)
                            {
                                IList<Constraint> list2;
                                Type genericTypeDefinition = type.GetGenericTypeDefinition();
                                if ((genericTypeDefinition != null) && this.settings.AdditionalConstraints.TryGetValue(genericTypeDefinition, out list2))
                                {
                                    ActivityValidationServices.RunConstraints(childActivity, parentChain, list2, this.options, suppressGetChildrenViolations, ref this.errors);
                                }
                            }
                        }
                    }
                    if (childActivity.Activity.IsExpressionRoot)
                    {
                        if (childActivity.Activity.HasNonEmptySubtree)
                        {
                            this.expressionRoot = childActivity.Activity;
                            ActivityUtilities.FinishCachingSubtree(childActivity, parentChain, ProcessActivityTreeOptions.FullCachingOptions, new ActivityUtilities.ProcessActivityCallback(this.ValidateExpressionSubtree));
                            this.expressionRoot = null;
                        }
                        else if (childActivity.Activity.InternalCanInduceIdle)
                        {
                            Activity activity = childActivity.Activity;
                            RuntimeArgument boundRuntimeArgument = ActivityValidationServices.GetBoundRuntimeArgument(activity);
                            ValidationError data = new ValidationError(System.Activities.SR.CanInduceIdleActivityInArgumentExpression(boundRuntimeArgument.Name, activity.Parent.DisplayName, activity.DisplayName), true, boundRuntimeArgument.Name, activity.Parent);
                            ActivityUtilities.Add<ValidationError>(ref this.errors, data);
                        }
                    }
                }
            }

            private void ValidateExpressionSubtree(ActivityUtilities.ChildActivity childActivity, ActivityUtilities.ActivityCallStack parentChain)
            {
                if (childActivity.Activity.InternalCanInduceIdle)
                {
                    Activity activity = childActivity.Activity;
                    Activity expressionRoot = this.expressionRoot;
                    RuntimeArgument boundRuntimeArgument = ActivityValidationServices.GetBoundRuntimeArgument(expressionRoot);
                    ValidationError data = new ValidationError(System.Activities.SR.CanInduceIdleActivityInArgumentExpression(boundRuntimeArgument.Name, expressionRoot.Parent.DisplayName, activity.DisplayName), true, boundRuntimeArgument.Name, expressionRoot.Parent);
                    ActivityUtilities.Add<ValidationError>(ref this.errors, data);
                }
            }
        }
    }
}

