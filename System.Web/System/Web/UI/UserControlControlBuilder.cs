namespace System.Web.UI
{
    using System;

    public class UserControlControlBuilder : ControlBuilder
    {
        private string _innerText;

        public override object BuildObject()
        {
            object obj2 = base.BuildObject();
            if (base.InDesigner)
            {
                IUserControlDesignerAccessor accessor = (IUserControlDesignerAccessor) obj2;
                accessor.TagName = base.TagName;
                if (this._innerText != null)
                {
                    accessor.InnerText = this._innerText;
                }
            }
            return obj2;
        }

        public override bool NeedsTagInnerText()
        {
            return base.InDesigner;
        }

        public override void SetTagInnerText(string text)
        {
            this._innerText = text;
        }
    }
}

