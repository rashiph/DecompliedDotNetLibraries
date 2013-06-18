namespace System.IdentityModel.Tokens
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IdentityModel;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;

    public class SecurityKeyIdentifier : IEnumerable<SecurityKeyIdentifierClause>, IEnumerable
    {
        private readonly List<SecurityKeyIdentifierClause> clauses;
        private const int InitialSize = 2;
        private bool isReadOnly;

        public SecurityKeyIdentifier()
        {
            this.clauses = new List<SecurityKeyIdentifierClause>(2);
        }

        public SecurityKeyIdentifier(params SecurityKeyIdentifierClause[] clauses)
        {
            if (clauses == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("clauses");
            }
            this.clauses = new List<SecurityKeyIdentifierClause>(clauses.Length);
            for (int i = 0; i < clauses.Length; i++)
            {
                this.Add(clauses[i]);
            }
        }

        public void Add(SecurityKeyIdentifierClause clause)
        {
            if (this.isReadOnly)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.IdentityModel.SR.GetString("ObjectIsReadOnly")));
            }
            if (clause == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("clause"));
            }
            this.clauses.Add(clause);
        }

        public SecurityKey CreateKey()
        {
            for (int i = 0; i < this.Count; i++)
            {
                if (this[i].CanCreateKey)
                {
                    return this[i].CreateKey();
                }
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.IdentityModel.SR.GetString("KeyIdentifierCannotCreateKey")));
        }

        public TClause Find<TClause>() where TClause: SecurityKeyIdentifierClause
        {
            TClause local;
            if (!this.TryFind<TClause>(out local))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.IdentityModel.SR.GetString("NoKeyIdentifierClauseFound", new object[] { typeof(TClause) }), "TClause"));
            }
            return local;
        }

        public IEnumerator<SecurityKeyIdentifierClause> GetEnumerator()
        {
            return this.clauses.GetEnumerator();
        }

        public void MakeReadOnly()
        {
            this.isReadOnly = true;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public override string ToString()
        {
            using (StringWriter writer = new StringWriter(CultureInfo.InvariantCulture))
            {
                writer.WriteLine("SecurityKeyIdentifier");
                writer.WriteLine("    (");
                writer.WriteLine("    IsReadOnly = {0},", this.IsReadOnly);
                writer.WriteLine("    Count = {0}{1}", this.Count, (this.Count > 0) ? "," : "");
                for (int i = 0; i < this.Count; i++)
                {
                    writer.WriteLine("    Clause[{0}] = {1}{2}", i, this[i], (i < (this.Count - 1)) ? "," : "");
                }
                writer.WriteLine("    )");
                return writer.ToString();
            }
        }

        public bool TryFind<TClause>(out TClause clause) where TClause: SecurityKeyIdentifierClause
        {
            for (int i = 0; i < this.clauses.Count; i++)
            {
                TClause local = this.clauses[i] as TClause;
                if (local != null)
                {
                    clause = local;
                    return true;
                }
            }
            clause = default(TClause);
            return false;
        }

        public bool CanCreateKey
        {
            get
            {
                for (int i = 0; i < this.Count; i++)
                {
                    if (this[i].CanCreateKey)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public int Count
        {
            get
            {
                return this.clauses.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return this.isReadOnly;
            }
        }

        public SecurityKeyIdentifierClause this[int index]
        {
            get
            {
                return this.clauses[index];
            }
        }
    }
}

