namespace Microsoft.JScript.Vsa
{
    using System;
    using System.IO;

    [Obsolete("Use of this type is not recommended because it is being deprecated in Visual Studio 2005; there will be no replacement for this feature. Please see the ICodeCompiler documentation for additional help.")]
    public class ResInfo
    {
        public string filename;
        public string fullpath;
        public bool isLinked;
        public bool isPublic;
        public string name;

        public ResInfo(string resinfo, bool isLinked)
        {
            string[] strArray = resinfo.Split(new char[] { ',' });
            int length = strArray.Length;
            this.filename = strArray[0];
            this.name = Path.GetFileName(this.filename);
            this.isPublic = true;
            this.isLinked = isLinked;
            if (length == 2)
            {
                this.name = strArray[1];
            }
            else if (length > 2)
            {
                bool flag = false;
                if (string.Compare(strArray[length - 1], "public", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    flag = true;
                }
                else if (string.Compare(strArray[length - 1], "private", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    this.isPublic = false;
                    flag = true;
                }
                this.name = strArray[length - (flag ? 2 : 1)];
                this.filename = string.Join(",", strArray, 0, length - (flag ? 2 : 1));
            }
            this.fullpath = Path.GetFullPath(this.filename);
        }

        public ResInfo(string filename, string name, bool isPublic, bool isLinked)
        {
            this.filename = filename;
            this.fullpath = Path.GetFullPath(filename);
            this.name = name;
            this.isPublic = isPublic;
            this.isLinked = isLinked;
        }
    }
}

