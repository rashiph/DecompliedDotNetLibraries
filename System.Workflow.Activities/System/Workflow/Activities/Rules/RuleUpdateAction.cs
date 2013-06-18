namespace System.Workflow.Activities.Rules
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime;
    using System.Workflow.ComponentModel.Compiler;

    [Serializable]
    public class RuleUpdateAction : RuleAction
    {
        private string path;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public RuleUpdateAction()
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public RuleUpdateAction(string path)
        {
            this.path = path;
        }

        public override RuleAction Clone()
        {
            return (RuleAction) base.MemberwiseClone();
        }

        public override bool Equals(object obj)
        {
            RuleUpdateAction action = obj as RuleUpdateAction;
            return ((action != null) && string.Equals(this.Path, action.Path, StringComparison.Ordinal));
        }

        public override void Execute(RuleExecution context)
        {
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override ICollection<string> GetSideEffects(RuleValidation validation)
        {
            return new string[] { this.path };
        }

        public override string ToString()
        {
            return ("Update(\"" + this.path + "\")");
        }

        public override bool Validate(RuleValidation validator)
        {
            if (validator == null)
            {
                throw new ArgumentNullException("validator");
            }
            bool flag = true;
            if (this.path == null)
            {
                ValidationError error = new ValidationError(Messages.NullUpdate, 0x53d);
                error.UserData["ErrorObject"] = this;
                validator.AddError(error);
                flag = false;
            }
            string[] strArray = this.path.Split(new char[] { '/' });
            if (strArray[0] == "this")
            {
                Type thisType = validator.ThisType;
                for (int i = 1; i < strArray.Length; i++)
                {
                    if (strArray[i] == "*")
                    {
                        if (i < (strArray.Length - 1))
                        {
                            ValidationError error2 = new ValidationError(Messages.InvalidWildCardInPathQualifier, 0x195);
                            error2.UserData["ErrorObject"] = this;
                            validator.AddError(error2);
                            flag = false;
                        }
                        return flag;
                    }
                    if (!string.IsNullOrEmpty(strArray[i]) || (i != (strArray.Length - 1)))
                    {
                        goto Label_00ED;
                    }
                    return flag;
                Label_00E6:
                    thisType = thisType.GetElementType();
                Label_00ED:
                    if (thisType.IsArray)
                    {
                        goto Label_00E6;
                    }
                    BindingFlags bindingAttr = BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance;
                    if (validator.AllowInternalMembers(thisType))
                    {
                        bindingAttr |= BindingFlags.NonPublic;
                    }
                    FieldInfo field = thisType.GetField(strArray[i], bindingAttr);
                    if (field != null)
                    {
                        thisType = field.FieldType;
                    }
                    else
                    {
                        PropertyInfo property = thisType.GetProperty(strArray[i], bindingAttr);
                        if (property != null)
                        {
                            thisType = property.PropertyType;
                        }
                        else
                        {
                            ValidationError error3 = new ValidationError(string.Format(CultureInfo.CurrentCulture, Messages.UpdateUnknownFieldOrProperty, new object[] { strArray[i] }), 0x57b);
                            error3.UserData["ErrorObject"] = this;
                            validator.AddError(error3);
                            return false;
                        }
                    }
                }
                return flag;
            }
            ValidationError error4 = new ValidationError(Messages.UpdateNotThis, 0x57b);
            error4.UserData["ErrorObject"] = this;
            validator.AddError(error4);
            return false;
        }

        public string Path
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.path;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.path = value;
            }
        }
    }
}

