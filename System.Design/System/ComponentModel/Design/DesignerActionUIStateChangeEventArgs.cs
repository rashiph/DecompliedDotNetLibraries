namespace System.ComponentModel.Design
{
    using System;

    public class DesignerActionUIStateChangeEventArgs : EventArgs
    {
        private DesignerActionUIStateChangeType changeType;
        private object relatedObject;

        public DesignerActionUIStateChangeEventArgs(object relatedObject, DesignerActionUIStateChangeType changeType)
        {
            this.relatedObject = relatedObject;
            this.changeType = changeType;
        }

        public DesignerActionUIStateChangeType ChangeType
        {
            get
            {
                return this.changeType;
            }
        }

        public object RelatedObject
        {
            get
            {
                return this.relatedObject;
            }
        }
    }
}

