namespace System.Web.Compilation
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Xml;

    internal class PreservationFileWriter
    {
        private bool _precompilationMode;
        private XmlTextWriter _writer;
        internal const string buildResultDependenciesTagName = "builddeps";
        internal const string buildResultDependencyTagName = "builddep";
        internal const string fileDependenciesTagName = "filedeps";
        internal const string fileDependencyTagName = "filedep";

        internal PreservationFileWriter(bool precompilationMode)
        {
            this._precompilationMode = precompilationMode;
        }

        internal void SaveBuildResultToFile(string preservationFile, BuildResult result, long hashCode)
        {
            this._writer = new XmlTextWriter(preservationFile, Encoding.UTF8);
            try
            {
                this._writer.Formatting = Formatting.Indented;
                this._writer.Indentation = 4;
                this._writer.WriteStartDocument();
                this._writer.WriteStartElement("preserve");
                this.SetAttribute("resultType", ((int) result.GetCode()).ToString(CultureInfo.InvariantCulture));
                if (result.VirtualPath != null)
                {
                    this.SetAttribute("virtualPath", result.VirtualPath.VirtualPathString);
                }
                this.SetAttribute("hash", result.ComputeHashCode(hashCode).ToString("x", CultureInfo.InvariantCulture));
                string virtualPathDependenciesHash = result.VirtualPathDependenciesHash;
                if (virtualPathDependenciesHash != null)
                {
                    this.SetAttribute("filehash", virtualPathDependenciesHash);
                }
                result.SetPreservedAttributes(this);
                this.SaveDependencies(result.VirtualPathDependencies);
                this._writer.WriteEndElement();
                this._writer.WriteEndDocument();
                this._writer.Close();
            }
            catch
            {
                this._writer.Close();
                File.Delete(preservationFile);
                throw;
            }
        }

        private void SaveDependencies(ICollection dependencies)
        {
            if (dependencies != null)
            {
                this._writer.WriteStartElement("filedeps");
                foreach (string str in dependencies)
                {
                    this._writer.WriteStartElement("filedep");
                    this._writer.WriteAttributeString("name", str);
                    this._writer.WriteEndElement();
                }
                this._writer.WriteEndElement();
            }
        }

        internal void SetAttribute(string name, string value)
        {
            this._writer.WriteAttributeString(name, value);
        }
    }
}

