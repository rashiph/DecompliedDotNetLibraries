namespace System.Data.Design
{
    using System;
    using System.Collections;
    using System.Data;
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

        public TypedDataSetGeneratorException(IList list) : this()
        {
            this.errorList = new ArrayList(list);
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
            int num = (int) info.GetValue(this.KEY_ARRAYCOUNT, typeof(int));
            if (num > 0)
            {
                this.errorList = new ArrayList();
                for (int i = 0; i < num; i++)
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

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
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

        public IList ErrorList
        {
            get
            {
                return this.errorList;
            }
        }
    }
}

