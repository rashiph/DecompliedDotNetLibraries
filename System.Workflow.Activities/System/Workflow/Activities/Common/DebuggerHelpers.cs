namespace System.Workflow.Activities.Common
{
    using System;
    using System.Collections;
    using System.Workflow.ComponentModel;

    internal static class DebuggerHelpers
    {
        private static Activity GetActivity(Activity containerActivity, string id)
        {
            if (containerActivity != null)
            {
                Queue queue = new Queue();
                queue.Enqueue(containerActivity);
                while (queue.Count > 0)
                {
                    Activity activity = (Activity) queue.Dequeue();
                    if (activity.Enabled)
                    {
                        if (activity.QualifiedName == id)
                        {
                            return activity;
                        }
                        if (activity is CompositeActivity)
                        {
                            foreach (Activity activity2 in ((CompositeActivity) activity).Activities)
                            {
                                queue.Enqueue(activity2);
                            }
                        }
                    }
                }
            }
            return null;
        }

        private static bool IsDeclaringActivityMatchesContext(Activity currentActivity, Activity context)
        {
            CompositeActivity compositeActivity = context as CompositeActivity;
            CompositeActivity declaringActivity = Helpers.GetDeclaringActivity(currentActivity);
            if (Helpers.IsActivityLocked(context) && ((compositeActivity == null) || !Helpers.IsCustomActivity(compositeActivity)))
            {
                compositeActivity = Helpers.GetDeclaringActivity(context);
            }
            return (compositeActivity == declaringActivity);
        }

        internal static Activity ParseActivity(Activity parsingContext, string activityName)
        {
            if (parsingContext == null)
            {
                throw new ArgumentNullException("parsingContext");
            }
            if (activityName == null)
            {
                throw new ArgumentNullException("activityName");
            }
            string id = activityName;
            string str2 = string.Empty;
            int index = activityName.IndexOf(".");
            if (index != -1)
            {
                id = activityName.Substring(0, index);
                str2 = activityName.Substring(index + 1);
                if (str2.Length == 0)
                {
                    return null;
                }
            }
            Activity containerActivity = null;
            containerActivity = GetActivity(parsingContext, id);
            if (((containerActivity == null) && (parsingContext is CompositeActivity)) && ((parsingContext.Parent != null) && Helpers.IsCustomActivity(parsingContext as CompositeActivity)))
            {
                containerActivity = GetActivity(parsingContext, parsingContext.QualifiedName + "." + id);
            }
            if (containerActivity == null)
            {
                return null;
            }
            if (str2.Length > 0)
            {
                if (!(containerActivity is CompositeActivity) || !Helpers.IsCustomActivity(containerActivity as CompositeActivity))
                {
                    return null;
                }
                string[] strArray = str2.Split(new char[] { '.' });
                for (int i = 0; i < strArray.Length; i++)
                {
                    Activity activity = GetActivity(containerActivity, containerActivity.QualifiedName + "." + strArray[i]);
                    if ((activity == null) || !Helpers.IsActivityLocked(activity))
                    {
                        return null;
                    }
                    CompositeActivity declaringActivity = Helpers.GetDeclaringActivity(activity);
                    if (containerActivity != declaringActivity)
                    {
                        return null;
                    }
                    containerActivity = activity;
                }
                return containerActivity;
            }
            if (Helpers.IsActivityLocked(containerActivity) && !IsDeclaringActivityMatchesContext(containerActivity, parsingContext))
            {
                return null;
            }
            return containerActivity;
        }
    }
}

