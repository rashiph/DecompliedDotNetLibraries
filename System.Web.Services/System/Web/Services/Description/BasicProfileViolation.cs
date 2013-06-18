namespace System.Web.Services.Description
{
    using System;
    using System.Collections.Specialized;
    using System.Runtime;
    using System.Text;
    using System.Web.Services;

    public class BasicProfileViolation
    {
        private WsiProfiles claims;
        private string details;
        private StringCollection elements;
        private string normativeStatement;
        private string recommendation;

        internal BasicProfileViolation(string normativeStatement) : this(normativeStatement, null)
        {
        }

        internal BasicProfileViolation(string normativeStatement, string element)
        {
            this.claims = WsiProfiles.BasicProfile1_1;
            this.normativeStatement = normativeStatement;
            int index = normativeStatement.IndexOf(',');
            if (index >= 0)
            {
                normativeStatement = normativeStatement.Substring(0, index);
            }
            this.details = Res.GetString("HelpGeneratorServiceConformance" + normativeStatement);
            this.recommendation = Res.GetString("HelpGeneratorServiceConformance" + normativeStatement + "_r");
            if (element != null)
            {
                this.Elements.Add(element);
            }
            if (this.normativeStatement == "Rxxxx")
            {
                this.normativeStatement = Res.GetString("Rxxxx");
            }
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(this.normativeStatement);
            builder.Append(": ");
            builder.Append(this.Details);
            foreach (string str in this.Elements)
            {
                builder.Append(Environment.NewLine);
                builder.Append("  -  ");
                builder.Append(str);
            }
            return builder.ToString();
        }

        public WsiProfiles Claims
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.claims;
            }
        }

        public string Details
        {
            get
            {
                if (this.details == null)
                {
                    return string.Empty;
                }
                return this.details;
            }
        }

        public StringCollection Elements
        {
            get
            {
                if (this.elements == null)
                {
                    this.elements = new StringCollection();
                }
                return this.elements;
            }
        }

        public string NormativeStatement
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.normativeStatement;
            }
        }

        public string Recommendation
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.recommendation;
            }
        }
    }
}

