namespace System.CodeDom
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ClassInterface(ClassInterfaceType.AutoDispatch), ComVisible(true)]
    public class CodeCommentStatement : CodeStatement
    {
        private CodeComment comment;

        public CodeCommentStatement()
        {
        }

        public CodeCommentStatement(CodeComment comment)
        {
            this.comment = comment;
        }

        public CodeCommentStatement(string text)
        {
            this.comment = new CodeComment(text);
        }

        public CodeCommentStatement(string text, bool docComment)
        {
            this.comment = new CodeComment(text, docComment);
        }

        public CodeComment Comment
        {
            get
            {
                return this.comment;
            }
            set
            {
                this.comment = value;
            }
        }
    }
}

