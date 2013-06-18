namespace System.Activities
{
    using System;
    using System.Activities.Validation;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Threading;

    public static class WorkflowInspectionServices
    {
        public static void CacheMetadata(Activity rootActivity)
        {
            CacheMetadata(rootActivity, null);
        }

        public static void CacheMetadata(Activity rootActivity, LocationReferenceEnvironment hostEnvironment)
        {
            if (rootActivity == null)
            {
                throw FxTrace.Exception.ArgumentNull("rootActivity");
            }
            if (rootActivity.HasBeenAssociatedWithAnInstance)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.RootActivityAlreadyAssociatedWithInstance(rootActivity.DisplayName)));
            }
            IList<ValidationError> validationErrors = null;
            if (hostEnvironment == null)
            {
                hostEnvironment = new ActivityLocationReferenceEnvironment();
            }
            ActivityUtilities.CacheRootMetadata(rootActivity, hostEnvironment, ProcessActivityTreeOptions.FullCachingOptions, null, ref validationErrors);
            ActivityValidationServices.ThrowIfViolationsExist(validationErrors);
        }

        public static IEnumerable<Activity> GetActivities(Activity activity)
        {
            int iteratorVariable0;
            if (activity == null)
            {
                throw FxTrace.Exception.ArgumentNull("activity");
            }
            if (!activity.IsMetadataCached)
            {
                IList<ValidationError> validationErrors = null;
                ActivityUtilities.CacheRootMetadata(activity, new ActivityLocationReferenceEnvironment(), ProcessActivityTreeOptions.FullCachingOptions, null, ref validationErrors);
                ActivityValidationServices.ThrowIfViolationsExist(validationErrors);
            }
            for (iteratorVariable0 = 0; iteratorVariable0 < activity.RuntimeArguments.Count; iteratorVariable0++)
            {
                RuntimeArgument iteratorVariable1 = activity.RuntimeArguments[iteratorVariable0];
                if ((iteratorVariable1.BoundArgument != null) && (iteratorVariable1.BoundArgument.Expression != null))
                {
                    yield return iteratorVariable1.BoundArgument.Expression;
                }
            }
            for (iteratorVariable0 = 0; iteratorVariable0 < activity.RuntimeVariables.Count; iteratorVariable0++)
            {
                Variable iteratorVariable2 = activity.RuntimeVariables[iteratorVariable0];
                if (iteratorVariable2.Default != null)
                {
                    yield return iteratorVariable2.Default;
                }
            }
            for (iteratorVariable0 = 0; iteratorVariable0 < activity.ImplementationVariables.Count; iteratorVariable0++)
            {
                Variable iteratorVariable3 = activity.ImplementationVariables[iteratorVariable0];
                if (iteratorVariable3.Default != null)
                {
                    yield return iteratorVariable3.Default;
                }
            }
            for (iteratorVariable0 = 0; iteratorVariable0 < activity.Children.Count; iteratorVariable0++)
            {
                yield return activity.Children[iteratorVariable0];
            }
            for (iteratorVariable0 = 0; iteratorVariable0 < activity.ImportedChildren.Count; iteratorVariable0++)
            {
                yield return activity.ImportedChildren[iteratorVariable0];
            }
            for (iteratorVariable0 = 0; iteratorVariable0 < activity.ImplementationChildren.Count; iteratorVariable0++)
            {
                yield return activity.ImplementationChildren[iteratorVariable0];
            }
            for (iteratorVariable0 = 0; iteratorVariable0 < activity.Delegates.Count; iteratorVariable0++)
            {
                ActivityDelegate iteratorVariable4 = activity.Delegates[iteratorVariable0];
                if (iteratorVariable4.Handler != null)
                {
                    yield return iteratorVariable4.Handler;
                }
            }
            for (iteratorVariable0 = 0; iteratorVariable0 < activity.ImportedDelegates.Count; iteratorVariable0++)
            {
                ActivityDelegate iteratorVariable5 = activity.ImportedDelegates[iteratorVariable0];
                if (iteratorVariable5.Handler != null)
                {
                    yield return iteratorVariable5.Handler;
                }
            }
            for (iteratorVariable0 = 0; iteratorVariable0 < activity.ImplementationDelegates.Count; iteratorVariable0++)
            {
                ActivityDelegate iteratorVariable6 = activity.ImplementationDelegates[iteratorVariable0];
                if (iteratorVariable6.Handler != null)
                {
                    yield return iteratorVariable6.Handler;
                }
            }
        }

        public static Activity Resolve(Activity root, string id)
        {
            Activity activity;
            if (root == null)
            {
                throw FxTrace.Exception.ArgumentNull("root");
            }
            if (string.IsNullOrEmpty(id))
            {
                throw FxTrace.Exception.ArgumentNullOrEmpty("id");
            }
            if (!root.IsMetadataCached)
            {
                IList<ValidationError> validationErrors = null;
                ActivityUtilities.CacheRootMetadata(root, new ActivityLocationReferenceEnvironment(), ProcessActivityTreeOptions.FullCachingOptions, null, ref validationErrors);
                ActivityValidationServices.ThrowIfViolationsExist(validationErrors);
            }
            QualifiedId id2 = QualifiedId.Parse(id);
            if (!QualifiedId.TryGetElementFromRoot(root, id2, out activity))
            {
                throw FxTrace.Exception.Argument("id", System.Activities.SR.IdNotFoundInWorkflow(id));
            }
            return activity;
        }

    }
}

