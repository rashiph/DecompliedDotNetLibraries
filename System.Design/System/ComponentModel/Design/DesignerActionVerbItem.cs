namespace System.ComponentModel.Design
{
    using System;

    internal class DesignerActionVerbItem : DesignerActionMethodItem
    {
        private DesignerVerb _targetVerb;

        public DesignerActionVerbItem(DesignerVerb verb)
        {
            if (verb == null)
            {
                throw new ArgumentNullException();
            }
            this._targetVerb = verb;
        }

        public override void Invoke()
        {
            this._targetVerb.Invoke();
        }

        public override string Category
        {
            get
            {
                return "Verbs";
            }
        }

        public override string Description
        {
            get
            {
                return this._targetVerb.Description;
            }
        }

        public override string DisplayName
        {
            get
            {
                return this._targetVerb.Text;
            }
        }

        public override bool IncludeAsDesignerVerb
        {
            get
            {
                return false;
            }
        }

        public override string MemberName
        {
            get
            {
                return null;
            }
        }
    }
}

