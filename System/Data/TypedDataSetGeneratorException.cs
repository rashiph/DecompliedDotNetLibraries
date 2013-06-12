namespace System.Data
{
    using System;
    using System.Collections;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    public class TypedDataSetGeneratorException : DataException
    {
        private ArrayList errorList;
        private string KEY_ARRAYCOUNT;
        private string KEY_ARRAYVALUES;

        public TypedDataSetGeneratorException()
        {
            this.KEY_ARRAYCOUNT = "KEY_ARRAYCOUNT";
            this.KEY_ARRAYVALUES = "KEY_ARRAYVALUES";
            this.errorList = null;
            base.HResult = -2146232021;
        }

        public TypedDataSetGeneratorException(ArrayList list) : this()
        {
            this.errorList = list;
            base.HResult = -2146232021;
        }

        public TypedDataSetGeneratorException(string message) : base(message)
        {
            this.KEY_ARRAYCOUNT = "KEY_ARRAYCOUNT";
            this.KEY_ARRAYVALUES = "KEY_ARRAYVALUES";
            base.HResult = -2146232021;
        }

        protected TypedDataSetGeneratorException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.KEY_ARRAYCOUNT = "KEY_ARRAYCOUNT";
            this.KEY_ARRAYVALUES = "KEY_ARRAYVALUES";
            int num2 = (int) info.GetValue(this.KEY_ARRAYCOUNT, typeof(int));
            if (num2 > 0)
            {
                this.errorList = new ArrayList();
                for (int i = 0; i < num2; i++)
                {
                    this.errorList.Add(info.GetValue(this.KEY_ARRAYVALUES + i, typeof(string)));
                }
            }
            else
            {
                this.errorList = null;
            }
        }

        public TypedDataSetGeneratorException(string message, Exception innerException) : base(message, innerException)
        {
            this.KEY_ARRAYCOUNT = "KEY_ARRAYCOUNT";
            this.KEY_ARRAYVALUES = "KEY_ARRAYVALUES";
            base.HResult = -2146232021;
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.SerializationFormatter)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            if (this.errorList != null)
            {
                info.AddValue(this.KEY_ARRAYCOUNT, this.errorList.Count);
                for (int i = 0; i < this.errorList.Count; i++)
                {
                    info.AddValue(this.KEY_ARRAYVALUES + i, this.errorList[i].ToString());
                }
            }
            else
            {
                info.AddValue(this.KEY_ARRAYCOUNT, 0);
            }
        }

        public ArrayList ErrorList
        {
            get
            {
                return this.errorList;
            }
        }
    }
}

