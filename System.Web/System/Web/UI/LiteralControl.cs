namespace System.Web.UI
{
    using System;
    using System.ComponentModel;
    using System.Runtime;

    [ToolboxItem(false)]
    public class LiteralControl : Control, ITextControl
    {
        internal string _text;

        public LiteralControl()
        {
            base.PreventAutoID();
            base.SetEnableViewStateInternal(false);
        }

        public LiteralControl(string text) : this()
        {
            this._text = (text != null) ? text : string.Empty;
        }

        protected override ControlCollection CreateControlCollection()
        {
            return new EmptyControlCollection(this);
        }

        internal override void InitRecursive(Control namingContainer)
        {
            this.ResolveAdapter();
            if (base.AdapterInternal != null)
            {
                base.AdapterInternal.OnInit(EventArgs.Empty);
            }
            else
            {
                this.OnInit(EventArgs.Empty);
            }
        }

        internal override void LoadRecursive()
        {
            if (base.AdapterInternal != null)
            {
                base.AdapterInternal.OnLoad(EventArgs.Empty);
            }
            else
            {
                this.OnLoad(EventArgs.Empty);
            }
        }

        internal override void PreRenderRecursiveInternal()
        {
            if (base.AdapterInternal != null)
            {
                base.AdapterInternal.OnPreRender(EventArgs.Empty);
            }
            else
            {
                this.OnPreRender(EventArgs.Empty);
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        protected internal override void Render(HtmlTextWriter output)
        {
            output.Write(this._text);
        }

        internal override void UnloadRecursive(bool dispose)
        {
            if (base.AdapterInternal != null)
            {
                base.AdapterInternal.OnUnload(EventArgs.Empty);
            }
            else
            {
                this.OnUnload(EventArgs.Empty);
            }
            if (dispose)
            {
                this.Dispose();
            }
        }

        public virtual string Text
        {
            get
            {
                return this._text;
            }
            set
            {
                this._text = (value != null) ? value : string.Empty;
            }
        }
    }
}

