namespace System.Web.Compilation
{
    using System;
    using System.Reflection;

    internal class BuildResultCustomString : BuildResultCompiledAssembly
    {
        private string _customString;

        internal BuildResultCustomString()
        {
        }

        internal BuildResultCustomString(Assembly a, string customString) : base(a)
        {
            this._customString = customString;
        }

        internal override BuildResultTypeCode GetCode()
        {
            return BuildResultTypeCode.BuildResultCustomString;
        }

        internal override void GetPreservedAttributes(PreservationFileReader pfr)
        {
            base.GetPreservedAttributes(pfr);
            this._customString = pfr.GetAttribute("customString");
        }

        internal override void SetPreservedAttributes(PreservationFileWriter pfw)
        {
            base.SetPreservedAttributes(pfw);
            pfw.SetAttribute("customString", this._customString);
        }

        internal string CustomString
        {
            get
            {
                return this._customString;
            }
        }
    }
}

