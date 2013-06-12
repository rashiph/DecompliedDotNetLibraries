namespace System.CodeDom
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true), ClassInterface(ClassInterfaceType.AutoDispatch)]
    public class CodeComment : CodeObject
    {
        private bool docComment;
        private string text;

        public CodeComment()
        {
        }

        public CodeComment(string text)
        {
            this.Text = text;
        }

        public CodeComment(string text, bool docComment)
        {
            this.Text = text;
            this.docComment = docComment;
        }

        public bool DocComment
        {
            get
            {
                return this.docComment;
            }
            set
            {
                this.docComment = value;
            }
        }

        public string Text
        {
            get
            {
                if (this.text != null)
                {
                    return this.text;
                }
                return string.Empty;
            }
            set
            {
                this.text = value;
            }
        }
    }
}

