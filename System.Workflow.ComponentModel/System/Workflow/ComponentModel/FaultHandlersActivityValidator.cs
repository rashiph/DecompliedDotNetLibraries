namespace System.Workflow.ComponentModel
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.ComponentModel.Design;

    internal sealed class FaultHandlersActivityValidator : CompositeActivityValidator
    {
        public override ValidationErrorCollection Validate(ValidationManager manager, object obj)
        {
            ValidationErrorCollection errors = base.Validate(manager, obj);
            FaultHandlersActivity activity = obj as FaultHandlersActivity;
            if (activity == null)
            {
                throw new ArgumentException(SR.GetString("Error_UnexpectedArgumentType", new object[] { typeof(FaultHandlersActivity).FullName }), "obj");
            }
            Hashtable hashtable = new Hashtable();
            ArrayList list = new ArrayList();
            bool flag = false;
            foreach (Activity activity2 in activity.EnabledActivities)
            {
                if (!(activity2 is FaultHandlerActivity))
                {
                    if (!flag)
                    {
                        errors.Add(new ValidationError(SR.GetString("Error_FaultHandlersActivityDeclNotAllFaultHandlerActivityDecl"), 0x51e));
                        flag = true;
                    }
                }
                else
                {
                    FaultHandlerActivity activity3 = (FaultHandlerActivity) activity2;
                    Type faultType = activity3.FaultType;
                    if (faultType != null)
                    {
                        if (hashtable[faultType] == null)
                        {
                            hashtable[faultType] = 1;
                            list.Add(faultType);
                        }
                        else if (((int) hashtable[faultType]) == 1)
                        {
                            errors.Add(new ValidationError(string.Format(CultureInfo.CurrentCulture, SR.GetString("Error_ScopeDuplicateFaultHandlerActivityFor"), new object[] { Helpers.GetEnclosingActivity(activity).GetType().Name, faultType.FullName }), 0x520));
                            hashtable[faultType] = 2;
                        }
                        foreach (Type type2 in list)
                        {
                            if ((type2 != faultType) && type2.IsAssignableFrom(faultType))
                            {
                                errors.Add(new ValidationError(SR.GetString("Error_FaultHandlerActivityWrongOrder", new object[] { faultType.Name, type2.Name }), 0x521));
                            }
                        }
                    }
                }
            }
            if (activity.AlternateFlowActivities.Count > 0)
            {
                errors.Add(new ValidationError(SR.GetString("Error_ModelingConstructsCanNotContainModelingConstructs"), 0x61f));
            }
            return errors;
        }
    }
}

