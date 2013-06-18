namespace System.Workflow.ComponentModel
{
    using System;
    using System.Text;
    using System.Workflow.ComponentModel.Design;

    internal static class InternalHelpers
    {
        internal static string GenerateQualifiedNameForLockedActivity(Activity activity, string id)
        {
            StringBuilder builder = new StringBuilder();
            string str = string.IsNullOrEmpty(id) ? activity.Name : id;
            CompositeActivity declaringActivity = Helpers.GetDeclaringActivity(activity);
            if (declaringActivity != null)
            {
                builder.Append(declaringActivity.QualifiedName).Append(".").Append(str);
            }
            else
            {
                builder.Append(str);
            }
            return builder.ToString();
        }
    }
}

