namespace System.Web.Compilation
{
    using System;

    public sealed class ResourceExpressionFields
    {
        private string _classKey;
        private string _resourceKey;

        internal ResourceExpressionFields(string classKey, string resourceKey)
        {
            this._classKey = classKey;
            this._resourceKey = resourceKey;
        }

        public string ClassKey
        {
            get
            {
                if (this._classKey == null)
                {
                    return string.Empty;
                }
                return this._classKey;
            }
        }

        public string ResourceKey
        {
            get
            {
                if (this._resourceKey == null)
                {
                    return string.Empty;
                }
                return this._resourceKey;
            }
        }
    }
}

