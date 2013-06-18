namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Drawing;
    using System.Runtime;

    public sealed class DesignerAction
    {
        private int actionId;
        private ActivityDesigner activityDesigner;
        private System.Drawing.Image image;
        private string propertyName;
        private string text;
        private IDictionary userData;

        public DesignerAction(ActivityDesigner activityDesigner, int actionId, string text)
        {
            if (activityDesigner == null)
            {
                throw new ArgumentNullException("activityDesigner");
            }
            if ((text == null) || (text.Length == 0))
            {
                throw new ArgumentException(SR.GetString("Error_NullOrEmptyValue"), "text");
            }
            this.activityDesigner = activityDesigner;
            this.actionId = actionId;
            this.text = text;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public DesignerAction(ActivityDesigner activityDesigner, int actionId, string text, System.Drawing.Image image) : this(activityDesigner, actionId, text)
        {
            this.image = image;
        }

        public void Invoke()
        {
            this.activityDesigner.OnExecuteDesignerAction(this);
        }

        public int ActionId
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.actionId;
            }
        }

        public System.Drawing.Image Image
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.image;
            }
        }

        public string PropertyName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.propertyName;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.propertyName = value;
            }
        }

        public string Text
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.text;
            }
        }

        public IDictionary UserData
        {
            get
            {
                if (this.userData == null)
                {
                    this.userData = new HybridDictionary();
                }
                return this.userData;
            }
        }
    }
}

