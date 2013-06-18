namespace System.Web.UI
{
    using System;

    public class EventEntry
    {
        private string _handlerMethodName;
        private Type _handlerType;
        private string _name;

        public string HandlerMethodName
        {
            get
            {
                return this._handlerMethodName;
            }
            set
            {
                this._handlerMethodName = value;
            }
        }

        public Type HandlerType
        {
            get
            {
                return this._handlerType;
            }
            set
            {
                this._handlerType = value;
            }
        }

        public string Name
        {
            get
            {
                return this._name;
            }
            set
            {
                this._name = value;
            }
        }
    }
}

