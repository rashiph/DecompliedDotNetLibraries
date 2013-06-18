namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Reflection.Emit;

    internal class IfState
    {
        private Label elseBegin;
        private Label endIf;

        internal Label ElseBegin
        {
            get
            {
                return this.elseBegin;
            }
            set
            {
                this.elseBegin = value;
            }
        }

        internal Label EndIf
        {
            get
            {
                return this.endIf;
            }
            set
            {
                this.endIf = value;
            }
        }
    }
}

