namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Runtime;

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ActivityDesignerThemeAttribute : Attribute
    {
        private Type designerThemeType;
        private string xml = string.Empty;

        public ActivityDesignerThemeAttribute(Type designerThemeType)
        {
            this.designerThemeType = designerThemeType;
        }

        public Type DesignerThemeType
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.designerThemeType;
            }
        }

        public string Xml
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.xml;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.xml = value;
            }
        }
    }
}

