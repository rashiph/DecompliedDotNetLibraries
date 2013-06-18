namespace System.Web.Services.Description
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Text;

    public class BasicProfileViolationCollection : CollectionBase, IEnumerable<BasicProfileViolation>, IEnumerable
    {
        private Hashtable violations = new Hashtable();

        internal int Add(string normativeStatement)
        {
            return this.Add(new BasicProfileViolation(normativeStatement));
        }

        internal int Add(BasicProfileViolation violation)
        {
            BasicProfileViolation violation2 = (BasicProfileViolation) this.violations[violation.NormativeStatement];
            if (violation2 == null)
            {
                this.violations[violation.NormativeStatement] = violation;
                return base.List.Add(violation);
            }
            foreach (string str in violation.Elements)
            {
                violation2.Elements.Add(str);
            }
            return this.IndexOf(violation2);
        }

        internal int Add(string normativeStatement, string element)
        {
            return this.Add(new BasicProfileViolation(normativeStatement, element));
        }

        public bool Contains(BasicProfileViolation violation)
        {
            return base.List.Contains(violation);
        }

        public void CopyTo(BasicProfileViolation[] array, int index)
        {
            base.List.CopyTo(array, index);
        }

        public int IndexOf(BasicProfileViolation violation)
        {
            return base.List.IndexOf(violation);
        }

        public void Insert(int index, BasicProfileViolation violation)
        {
            base.List.Insert(index, violation);
        }

        public void Remove(BasicProfileViolation violation)
        {
            base.List.Remove(violation);
        }

        IEnumerator<BasicProfileViolation> IEnumerable<BasicProfileViolation>.GetEnumerator()
        {
            return new BasicProfileViolationEnumerator(this);
        }

        public override string ToString()
        {
            if (base.List.Count <= 0)
            {
                return string.Empty;
            }
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < base.List.Count; i++)
            {
                BasicProfileViolation violation = this[i];
                if (i != 0)
                {
                    builder.Append(Environment.NewLine);
                }
                builder.Append(violation.NormativeStatement);
                builder.Append(": ");
                builder.Append(violation.Details);
                foreach (string str in violation.Elements)
                {
                    builder.Append(Environment.NewLine);
                    builder.Append("  -  ");
                    builder.Append(str);
                }
                if ((violation.Recommendation != null) && (violation.Recommendation.Length > 0))
                {
                    builder.Append(Environment.NewLine);
                    builder.Append(violation.Recommendation);
                }
            }
            return builder.ToString();
        }

        public BasicProfileViolation this[int index]
        {
            get
            {
                return (BasicProfileViolation) base.List[index];
            }
            set
            {
                base.List[index] = value;
            }
        }
    }
}

