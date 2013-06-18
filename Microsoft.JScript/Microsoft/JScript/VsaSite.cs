namespace Microsoft.JScript
{
    using Microsoft.JScript.Vsa;
    using System;
    using System.Globalization;
    using System.IO;

    internal class VsaSite : BaseVsaSite
    {
        public TextWriter output = Console.Out;
        public bool treatWarningsAsErrors;
        public int warningLevel = 4;

        public VsaSite(TextWriter redirectedOutput)
        {
            this.output = redirectedOutput;
        }

        public override bool OnCompilerError(IJSVsaError error)
        {
            int severity = error.Severity;
            if (severity <= this.warningLevel)
            {
                bool fIsWarning = (severity != 0) && !this.treatWarningsAsErrors;
                this.PrintError(error.SourceMoniker, error.Line, error.StartColumn, fIsWarning, error.Number, error.Description);
            }
            return true;
        }

        private void PrintError(string sourceFile, int line, int column, bool fIsWarning, int number, string message)
        {
            int num = 0x2710 + (number & 0xffff);
            string str = num.ToString(CultureInfo.InvariantCulture).Substring(1);
            if (string.Compare(sourceFile, "no source", StringComparison.Ordinal) != 0)
            {
                this.output.Write(sourceFile + "(" + line.ToString(CultureInfo.InvariantCulture) + "," + column.ToString(CultureInfo.InvariantCulture) + ") : ");
            }
            this.output.WriteLine((fIsWarning ? "warning JS" : "error JS") + str + ": " + message);
        }
    }
}

