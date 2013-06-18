namespace System.Management
{
    using System;
    using System.Runtime;

    public class RelationshipQuery : WqlObjectQuery
    {
        private bool classDefinitionsOnly;
        private bool isSchemaQuery;
        private string relationshipClass;
        private string relationshipQualifier;
        private string sourceObject;
        private string thisRole;
        private static readonly string tokenClassDefsOnly = "classdefsonly";
        private static readonly string tokenOf = "of";
        private static readonly string tokenReferences = "references";
        private static readonly string tokenRequiredQualifier = "requiredqualifier";
        private static readonly string tokenResultClass = "resultclass";
        private static readonly string tokenRole = "role";
        private static readonly string tokenSchemaOnly = "schemaonly";
        private static readonly string tokenWhere = "where";

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public RelationshipQuery() : this(null)
        {
        }

        public RelationshipQuery(string queryOrSourceObject)
        {
            if (queryOrSourceObject != null)
            {
                if (queryOrSourceObject.TrimStart(new char[0]).StartsWith(tokenReferences, StringComparison.OrdinalIgnoreCase))
                {
                    this.QueryString = queryOrSourceObject;
                }
                else
                {
                    ManagementPath path = new ManagementPath(queryOrSourceObject);
                    if ((!path.IsClass && !path.IsInstance) || (path.NamespacePath.Length != 0))
                    {
                        throw new ArgumentException(RC.GetString("INVALID_QUERY"), "queryOrSourceObject");
                    }
                    this.SourceObject = queryOrSourceObject;
                    this.isSchemaQuery = false;
                }
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public RelationshipQuery(string sourceObject, string relationshipClass) : this(sourceObject, relationshipClass, null, null, false)
        {
        }

        public RelationshipQuery(bool isSchemaQuery, string sourceObject, string relationshipClass, string relationshipQualifier, string thisRole)
        {
            if (!isSchemaQuery)
            {
                throw new ArgumentException(RC.GetString("INVALID_QUERY"), "isSchemaQuery");
            }
            this.isSchemaQuery = true;
            this.sourceObject = sourceObject;
            this.relationshipClass = relationshipClass;
            this.relationshipQualifier = relationshipQualifier;
            this.thisRole = thisRole;
            this.classDefinitionsOnly = false;
            this.BuildQuery();
        }

        public RelationshipQuery(string sourceObject, string relationshipClass, string relationshipQualifier, string thisRole, bool classDefinitionsOnly)
        {
            this.isSchemaQuery = false;
            this.sourceObject = sourceObject;
            this.relationshipClass = relationshipClass;
            this.relationshipQualifier = relationshipQualifier;
            this.thisRole = thisRole;
            this.classDefinitionsOnly = classDefinitionsOnly;
            this.BuildQuery();
        }

        protected internal void BuildQuery()
        {
            if (this.sourceObject == null)
            {
                base.SetQueryString(string.Empty);
            }
            if ((this.sourceObject != null) && (this.sourceObject.Length != 0))
            {
                string qString = tokenReferences + " " + tokenOf + " {" + this.sourceObject + "}";
                if (((this.RelationshipClass.Length != 0) || (this.RelationshipQualifier.Length != 0)) || (((this.ThisRole.Length != 0) || this.classDefinitionsOnly) || this.isSchemaQuery))
                {
                    qString = qString + " " + tokenWhere;
                    if (this.RelationshipClass.Length != 0)
                    {
                        qString = qString + " " + tokenResultClass + " = " + this.relationshipClass;
                    }
                    if (this.ThisRole.Length != 0)
                    {
                        qString = qString + " " + tokenRole + " = " + this.thisRole;
                    }
                    if (this.RelationshipQualifier.Length != 0)
                    {
                        qString = qString + " " + tokenRequiredQualifier + " = " + this.relationshipQualifier;
                    }
                    if (!this.isSchemaQuery)
                    {
                        if (this.classDefinitionsOnly)
                        {
                            qString = qString + " " + tokenClassDefsOnly;
                        }
                    }
                    else
                    {
                        qString = qString + " " + tokenSchemaOnly;
                    }
                }
                base.SetQueryString(qString);
            }
        }

        public override object Clone()
        {
            if (!this.isSchemaQuery)
            {
                return new RelationshipQuery(this.sourceObject, this.relationshipClass, this.relationshipQualifier, this.thisRole, this.classDefinitionsOnly);
            }
            return new RelationshipQuery(true, this.sourceObject, this.relationshipClass, this.relationshipQualifier, this.thisRole);
        }

        protected internal override void ParseQuery(string query)
        {
            string str = null;
            string tokenValue = null;
            string str3 = null;
            string str4 = null;
            int num;
            bool flag = false;
            bool flag2 = false;
            string strA = query.Trim();
            if (string.Compare(strA, 0, tokenReferences, 0, tokenReferences.Length, StringComparison.OrdinalIgnoreCase) != 0)
            {
                throw new ArgumentException(RC.GetString("INVALID_QUERY"), "references");
            }
            strA = strA.Remove(0, tokenReferences.Length);
            if ((strA.Length == 0) || !char.IsWhiteSpace(strA[0]))
            {
                throw new ArgumentException(RC.GetString("INVALID_QUERY"));
            }
            strA = strA.TrimStart(null);
            if (string.Compare(strA, 0, tokenOf, 0, tokenOf.Length, StringComparison.OrdinalIgnoreCase) != 0)
            {
                throw new ArgumentException(RC.GetString("INVALID_QUERY"), "of");
            }
            strA = strA.Remove(0, tokenOf.Length).TrimStart(null);
            if (strA.IndexOf('{') != 0)
            {
                throw new ArgumentException(RC.GetString("INVALID_QUERY"));
            }
            strA = strA.Remove(0, 1).TrimStart(null);
            if (-1 == (num = strA.IndexOf('}')))
            {
                throw new ArgumentException(RC.GetString("INVALID_QUERY"));
            }
            str = strA.Substring(0, num).TrimEnd(null);
            strA = strA.Remove(0, num + 1).TrimStart(null);
            if (0 >= strA.Length)
            {
                goto Label_0366;
            }
            if (string.Compare(strA, 0, tokenWhere, 0, tokenWhere.Length, StringComparison.OrdinalIgnoreCase) != 0)
            {
                throw new ArgumentException(RC.GetString("INVALID_QUERY"), "where");
            }
            strA = strA.Remove(0, tokenWhere.Length);
            if ((strA.Length == 0) || !char.IsWhiteSpace(strA[0]))
            {
                throw new ArgumentException(RC.GetString("INVALID_QUERY"));
            }
            strA = strA.TrimStart(null);
            bool bTokenFound = false;
            bool flag4 = false;
            bool flag5 = false;
            bool flag6 = false;
            bool flag7 = false;
        Label_01D8:
            while ((strA.Length >= tokenResultClass.Length) && (string.Compare(strA, 0, tokenResultClass, 0, tokenResultClass.Length, StringComparison.OrdinalIgnoreCase) == 0))
            {
                ManagementQuery.ParseToken(ref strA, tokenResultClass, "=", ref bTokenFound, ref tokenValue);
            }
            if ((strA.Length >= tokenRole.Length) && (string.Compare(strA, 0, tokenRole, 0, tokenRole.Length, StringComparison.OrdinalIgnoreCase) == 0))
            {
                ManagementQuery.ParseToken(ref strA, tokenRole, "=", ref flag4, ref str3);
                goto Label_01D8;
            }
            if ((strA.Length >= tokenRequiredQualifier.Length) && (string.Compare(strA, 0, tokenRequiredQualifier, 0, tokenRequiredQualifier.Length, StringComparison.OrdinalIgnoreCase) == 0))
            {
                ManagementQuery.ParseToken(ref strA, tokenRequiredQualifier, "=", ref flag5, ref str4);
                goto Label_01D8;
            }
            if ((strA.Length >= tokenClassDefsOnly.Length) && (string.Compare(strA, 0, tokenClassDefsOnly, 0, tokenClassDefsOnly.Length, StringComparison.OrdinalIgnoreCase) == 0))
            {
                ManagementQuery.ParseToken(ref strA, tokenClassDefsOnly, ref flag6);
                flag = true;
                goto Label_01D8;
            }
            if ((strA.Length >= tokenSchemaOnly.Length) && (string.Compare(strA, 0, tokenSchemaOnly, 0, tokenSchemaOnly.Length, StringComparison.OrdinalIgnoreCase) == 0))
            {
                ManagementQuery.ParseToken(ref strA, tokenSchemaOnly, ref flag7);
                flag2 = true;
                goto Label_01D8;
            }
            if (strA.Length != 0)
            {
                throw new ArgumentException(RC.GetString("INVALID_QUERY"));
            }
            if (flag && flag2)
            {
                throw new ArgumentException(RC.GetString("INVALID_QUERY"));
            }
        Label_0366:
            this.sourceObject = str;
            this.relationshipClass = tokenValue;
            this.thisRole = str3;
            this.relationshipQualifier = str4;
            this.classDefinitionsOnly = flag;
            this.isSchemaQuery = flag2;
        }

        public bool ClassDefinitionsOnly
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.classDefinitionsOnly;
            }
            set
            {
                this.classDefinitionsOnly = value;
                this.BuildQuery();
                base.FireIdentifierChanged();
            }
        }

        public bool IsSchemaQuery
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.isSchemaQuery;
            }
            set
            {
                this.isSchemaQuery = value;
                this.BuildQuery();
                base.FireIdentifierChanged();
            }
        }

        public string RelationshipClass
        {
            get
            {
                if (this.relationshipClass == null)
                {
                    return string.Empty;
                }
                return this.relationshipClass;
            }
            set
            {
                this.relationshipClass = value;
                this.BuildQuery();
                base.FireIdentifierChanged();
            }
        }

        public string RelationshipQualifier
        {
            get
            {
                if (this.relationshipQualifier == null)
                {
                    return string.Empty;
                }
                return this.relationshipQualifier;
            }
            set
            {
                this.relationshipQualifier = value;
                this.BuildQuery();
                base.FireIdentifierChanged();
            }
        }

        public string SourceObject
        {
            get
            {
                if (this.sourceObject == null)
                {
                    return string.Empty;
                }
                return this.sourceObject;
            }
            set
            {
                this.sourceObject = value;
                this.BuildQuery();
                base.FireIdentifierChanged();
            }
        }

        public string ThisRole
        {
            get
            {
                if (this.thisRole == null)
                {
                    return string.Empty;
                }
                return this.thisRole;
            }
            set
            {
                this.thisRole = value;
                this.BuildQuery();
                base.FireIdentifierChanged();
            }
        }
    }
}

