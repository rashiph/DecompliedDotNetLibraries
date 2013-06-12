namespace System.Windows.Forms
{
    using System;

    public class QuestionEventArgs : EventArgs
    {
        private bool response;

        public QuestionEventArgs()
        {
            this.response = false;
        }

        public QuestionEventArgs(bool response)
        {
            this.response = response;
        }

        public bool Response
        {
            get
            {
                return this.response;
            }
            set
            {
                this.response = value;
            }
        }
    }
}

