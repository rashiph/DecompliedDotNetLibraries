namespace System.Deployment.Application.Manifest
{
    using System;
    using System.Deployment.Internal.Isolation.Manifest;
    using System.Text;

    internal class FileAssociation
    {
        private readonly string _defaultIcon;
        private readonly string _description;
        private readonly string _extension;
        private readonly string _parameter;
        private readonly string _progId;

        public FileAssociation(System.Deployment.Internal.Isolation.Manifest.FileAssociationEntry fileAssociationEntry)
        {
            this._extension = fileAssociationEntry.Extension;
            this._description = fileAssociationEntry.Description;
            this._progId = fileAssociationEntry.ProgID;
            this._defaultIcon = fileAssociationEntry.DefaultIcon;
            this._parameter = fileAssociationEntry.Parameter;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("(" + this._extension + ",");
            builder.Append(this._description + ",");
            builder.Append(this._progId + ",");
            builder.Append(this._defaultIcon + ",");
            builder.Append(this._parameter + ")");
            return builder.ToString();
        }

        public string DefaultIcon
        {
            get
            {
                return this._defaultIcon;
            }
        }

        public string Description
        {
            get
            {
                return this._description;
            }
        }

        public string Extension
        {
            get
            {
                return this._extension;
            }
        }

        public string Parameter
        {
            get
            {
                return this._parameter;
            }
        }

        public string ProgID
        {
            get
            {
                return this._progId;
            }
        }
    }
}

