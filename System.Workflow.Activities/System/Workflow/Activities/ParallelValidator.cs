namespace System.Workflow.Activities
{
    using System;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;

    internal sealed class ParallelValidator : CompositeActivityValidator
    {
        public override ValidationErrorCollection Validate(ValidationManager manager, object obj)
        {
            ValidationErrorCollection errors = base.Validate(manager, obj);
            ParallelActivity activity = obj as ParallelActivity;
            if (activity == null)
            {
                throw new ArgumentException(SR.GetString("Error_UnexpectedArgumentType", new object[] { typeof(ParallelActivity).FullName }), "obj");
            }
            if (activity.EnabledActivities.Count < 2)
            {
                errors.Add(new ValidationError(SR.GetString("Error_ParallelLessThanTwoChildren"), 0x517));
            }
            bool flag = false;
            foreach (Activity activity2 in activity.EnabledActivities)
            {
                if (activity2.GetType() != typeof(SequenceActivity))
                {
                    flag = true;
                }
            }
            if (flag)
            {
                errors.Add(new ValidationError(SR.GetString("Error_ParallelNotAllSequence"), 0x518));
            }
            return errors;
        }
    }
}

