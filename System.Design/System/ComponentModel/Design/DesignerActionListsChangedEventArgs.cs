namespace System.ComponentModel.Design
{
    using System;

    public class DesignerActionListsChangedEventArgs : EventArgs
    {
        private DesignerActionListCollection actionLists;
        private DesignerActionListsChangedType changeType;
        private object relatedObject;

        public DesignerActionListsChangedEventArgs(object relatedObject, DesignerActionListsChangedType changeType, DesignerActionListCollection actionLists)
        {
            this.relatedObject = relatedObject;
            this.changeType = changeType;
            this.actionLists = actionLists;
        }

        public DesignerActionListCollection ActionLists
        {
            get
            {
                return this.actionLists;
            }
        }

        public DesignerActionListsChangedType ChangeType
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

