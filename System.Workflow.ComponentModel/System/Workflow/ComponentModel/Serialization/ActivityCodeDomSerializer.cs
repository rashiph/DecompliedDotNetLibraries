namespace System.Workflow.ComponentModel.Serialization
{
    using System;
    using System.CodeDom;
    using System.ComponentModel.Design.Serialization;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;

    public class ActivityCodeDomSerializer : DependencyObjectCodeDomSerializer
    {
        public static readonly DependencyProperty MarkupFileNameProperty = DependencyProperty.RegisterAttached("MarkupFileName", typeof(string), typeof(ActivityCodeDomSerializer), new PropertyMetadata(null, new Attribute[] { new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden) }));

        public override object Serialize(IDesignerSerializationManager manager, object obj)
        {
            if (manager == null)
            {
                throw new ArgumentNullException("manager");
            }
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            Activity activity = obj as Activity;
            if (activity == null)
            {
                throw new ArgumentException(SR.GetString("Error_UnexpectedArgumentType", new object[] { typeof(Activity).FullName }), "obj");
            }
            if (Helpers.IsActivityLocked(activity))
            {
                return null;
            }
            CodeStatementCollection statements = base.Serialize(manager, activity) as CodeStatementCollection;
            if (statements != null)
            {
                Activity rootActivity = Helpers.GetRootActivity(activity);
                if (((rootActivity == null) || (rootActivity.GetValue(MarkupFileNameProperty) == null)) || (((int) activity.GetValue(ActivityMarkupSerializer.StartLineProperty)) == -1))
                {
                    return statements;
                }
                foreach (CodeStatement statement in statements)
                {
                    if (!(statement is CodeCommentStatement))
                    {
                        statement.LinePragma = new CodeLinePragma((string) rootActivity.GetValue(MarkupFileNameProperty), Math.Max((int) activity.GetValue(ActivityMarkupSerializer.StartLineProperty), 1));
                    }
                }
            }
            return statements;
        }
    }
}

