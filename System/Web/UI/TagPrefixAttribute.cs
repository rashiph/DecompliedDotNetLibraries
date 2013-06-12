namespace System.Web.UI
{
    using System;
    using System.Web.Util;

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple=true)]
    public sealed class TagPrefixAttribute : Attribute
    {
        private string namespaceName;
        private string tagPrefix;

        public TagPrefixAttribute(string namespaceName, string tagPrefix)
        {
            if (string.IsNullOrEmpty(namespaceName))
            {
                throw ExceptionUtil.ParameterNullOrEmpty("namespaceName");
            }
            if (string.IsNullOrEmpty(tagPrefix))
            {
                throw ExceptionUtil.ParameterNullOrEmpty("tagPrefix");
            }
            this.namespaceName = namespaceName;
            this.tagPrefix = tagPrefix;
        }

        public string NamespaceName
        {
            get
            {
                return this.namespaceName;
            }
        }

        public string TagPrefix
        {
            get
            {
                return this.tagPrefix;
            }
        }
    }
}

