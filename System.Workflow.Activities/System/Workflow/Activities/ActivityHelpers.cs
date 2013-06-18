namespace System.Workflow.Activities
{
    using System;
    using System.Workflow.Activities.Common;
    using System.Workflow.ComponentModel;
    using System.Workflow.Runtime;

    internal static class ActivityHelpers
    {
        internal static void InitializeCorrelationTokenCollection(Activity activity, CorrelationToken correlator)
        {
            if ((correlator != null) && !string.IsNullOrEmpty(correlator.OwnerActivityName))
            {
                string ownerActivityName = correlator.OwnerActivityName;
                Activity activityByName = activity.GetActivityByName(ownerActivityName);
                if (activityByName == null)
                {
                    activityByName = Helpers.ParseActivityForBind(activity, ownerActivityName);
                }
                if (activityByName == null)
                {
                    throw new ArgumentException("ownerActivity");
                }
                CorrelationTokenCollection tokens = activityByName.GetValue(CorrelationTokenCollection.CorrelationTokenCollectionProperty) as CorrelationTokenCollection;
                if (tokens == null)
                {
                    tokens = new CorrelationTokenCollection();
                    activityByName.SetValue(CorrelationTokenCollection.CorrelationTokenCollectionProperty, tokens);
                }
                if (!tokens.Contains(correlator.Name))
                {
                    tokens.Add(correlator);
                }
            }
        }
    }
}

