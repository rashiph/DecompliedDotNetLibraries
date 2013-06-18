namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Drawing;
    using System.Runtime;

    public class DesignerView
    {
        private ActivityDesigner designer;
        private System.Drawing.Image image;
        private static int MaxViewName = 150;
        private string text;
        private IDictionary userData;
        private int viewId;

        public DesignerView(int viewId, string text, System.Drawing.Image image)
        {
            if (text == null)
            {
                throw new ArgumentNullException("text");
            }
            if (image == null)
            {
                throw new ArgumentNullException("image");
            }
            this.viewId = viewId;
            this.text = (text.Length > MaxViewName) ? (text.Substring(0, MaxViewName) + "...") : text;
            this.image = image;
        }

        public DesignerView(int viewId, string text, System.Drawing.Image image, ActivityDesigner associatedDesigner) : this(viewId, text, image)
        {
            if (associatedDesigner == null)
            {
                throw new ArgumentNullException("associatedDesigner");
            }
            this.designer = associatedDesigner;
        }

        public override bool Equals(object obj)
        {
            DesignerView view = obj as DesignerView;
            if (view == null)
            {
                return false;
            }
            return (this.viewId == view.viewId);
        }

        public override int GetHashCode()
        {
            return this.viewId;
        }

        public virtual void OnActivate()
        {
        }

        public virtual void OnDeactivate()
        {
        }

        public virtual ActivityDesigner AssociatedDesigner
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.designer;
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

        public int ViewId
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.viewId;
            }
        }
    }
}

