namespace System.Web
{
    using System;

    internal class ModuleConfigurationInfo
    {
        private string _name;
        private string _precondition;
        private string _type;

        internal ModuleConfigurationInfo(string name, string type, string condition)
        {
            this._type = type;
            this._name = name;
            this._precondition = condition;
        }

        internal string Name
        {
            get
            {
                return this._name;
            }
        }

        internal string Precondition
        {
            get
            {
                return this._precondition;
            }
        }

        internal string Type
        {
            get
            {
                return this._type;
            }
        }
    }
}

