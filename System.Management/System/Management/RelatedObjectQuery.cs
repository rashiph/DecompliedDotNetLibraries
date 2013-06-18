namespace System.Management
{
    using System;
    using System.Runtime;

    public class RelatedObjectQuery : WqlObjectQuery
    {
        private bool classDefinitionsOnly;
        private bool isSchemaQuery;
        private string relatedClass;
        private string relatedQualifier;
        private string relatedRole;
        private string relationshipClass;
        private string relationshipQualifier;
        private string sourceObject;
        private string thisRole;
        private static readonly string tokenAssocClass = "assocclass";
        private static readonly string tokenAssociators = "associators";
        private static readonly string tokenClassDefsOnly = "classdefsonly";
        private static readonly string tokenOf = "of";
        private static readonly string tokenRequiredAssocQualifier = "requiredassocqualifier";
        private static readonly string tokenRequiredQualifier = "requiredqualifier";
        private static readonly string tokenResultClass = "resultclass";
        private static readonly string tokenResultRole = "resultrole";
        private static readonly string tokenRole = "role";
        private static readonly string tokenSchemaOnly = "schemaonly";
        private static readonly string tokenWhere = "where";

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public RelatedObjectQuery() : this(null)
        {
        }

        public RelatedObjectQuery(string queryOrSourceObject)
        {
            if (queryOrSourceObject != null)
            {
                if (queryOrSourceObject.TrimStart(new char[0]).StartsWith(tokenAssociators, StringComparison.OrdinalIgnoreCase))
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
        public RelatedObjectQuery(string sourceObject, string relatedClass) : this(sourceObject, relatedClass, null, null, null, null, null, false)
        {
        }

        public RelatedObjectQuery(bool isSchemaQuery, string sourceObject, string relatedClass, string relationshipClass, string relatedQualifier, string relationshipQualifier, string relatedRole, string thisRole)
        {
            if (!isSchemaQuery)
            {
                throw new ArgumentException(RC.GetString("INVALID_QUERY"), "isSchemaQuery");
            }
            this.isSchemaQuery = true;
            this.sourceObject = sourceObject;
            this.relatedClass = relatedClass;
            this.relationshipClass = relationshipClass;
            this.relatedQualifier = relatedQualifier;
            this.relationshipQualifier = relationshipQualifier;
            this.relatedRole = relatedRole;
            this.thisRole = thisRole;
            this.classDefinitionsOnly = false;
            this.BuildQuery();
        }

        public RelatedObjectQuery(string sourceObject, string relatedClass, string relationshipClass, string relatedQualifier, string relationshipQualifier, string relatedRole, string thisRole, bool classDefinitionsOnly)
        {
            this.isSchemaQuery = false;
            this.sourceObject = sourceObject;
            this.relatedClass = relatedClass;
            this.relationshipClass = relationshipClass;
            this.relatedQualifier = relatedQualifier;
            this.relationshipQualifier = relationshipQualifier;
            this.relatedRole = relatedRole;
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
                string qString = tokenAssociators + " " + tokenOf + " {" + this.sourceObject + "}";
                if ((((this.RelatedClass.Length != 0) || (this.RelationshipClass.Length != 0)) || ((this.RelatedQualifier.Length != 0) || (this.RelationshipQualifier.Length != 0))) || (((this.RelatedRole.Length != 0) || (this.ThisRole.Length != 0)) || (this.classDefinitionsOnly || this.isSchemaQuery)))
                {
                    qString = qString + " " + tokenWhere;
                    if (this.RelatedClass.Length != 0)
                    {
                        qString = qString + " " + tokenResultClass + " = " + this.relatedClass;
                    }
                    if (this.RelationshipClass.Length != 0)
                    {
                        qString = qString + " " + tokenAssocClass + " = " + this.relationshipClass;
                    }
                    if (this.RelatedRole.Length != 0)
                    {
                        qString = qString + " " + tokenResultRole + " = " + this.relatedRole;
                    }
                    if (this.ThisRole.Length != 0)
                    {
                        qString = qString + " " + tokenRole + " = " + this.thisRole;
                    }
                    if (this.RelatedQualifier.Length != 0)
                    {
                        qString = qString + " " + tokenRequiredQualifier + " = " + this.relatedQualifier;
                    }
                    if (this.RelationshipQualifier.Length != 0)
                    {
                        qString = qString + " " + tokenRequiredAssocQualifier + " = " + this.relationshipQualifier;
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
                return new RelatedObjectQuery(this.sourceObject, this.relatedClass, this.relationshipClass, this.relatedQualifier, this.relationshipQualifier, this.relatedRole, this.thisRole, this.classDefinitionsOnly);
            }
            return new RelatedObjectQuery(true, this.sourceObject, this.relatedClass, this.relationshipClass, this.relatedQualifier, this.relationshipQualifier, this.relatedRole, this.thisRole);
        }

        protected internal override void ParseQuery(string query)
        {
            string str = null;
            string tokenValue = null;
            string str3 = null;
            string str4 = null;
            string str5 = null;
            string str6 = null;
            string str7 = null;
            int num;
            bool flag = false;
            bool flag2 = false;
            string strA = query.Trim();
            if (string.Compare(strA, 0, tokenAssociators, 0, tokenAssociators.Length, StringComparison.OrdinalIgnoreCase) != 0)
            {
                throw new ArgumentException(RC.GetString("INVALID_QUERY"), "associators");
            }
            strA = strA.Remove(0, tokenAssociators.Length);
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
                goto Label_0450;
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
            bool flag8 = false;
            bool flag9 = false;
            bool flag10 = false;
        Label_01EA:
            while ((strA.Length >= tokenResultClass.Length) && (string.Compare(strA, 0, tokenResultClass, 0, tokenResultClass.Length, StringComparison.OrdinalIgnoreCase) == 0))
            {
                ManagementQuery.ParseToken(ref strA, tokenResultClass, "=", ref bTokenFound, ref tokenValue);
            }
            if ((strA.Length >= tokenAssocClass.Length) && (string.Compare(strA, 0, tokenAssocClass, 0, tokenAssocClass.Length, StringComparison.OrdinalIgnoreCase) == 0))
            {
                ManagementQuery.ParseToken(ref strA, tokenAssocClass, "=", ref flag4, ref str3);
                goto Label_01EA;
            }
            if ((strA.Length >= tokenResultRole.Length) && (string.Compare(strA, 0, tokenResultRole, 0, tokenResultRole.Length, StringComparison.OrdinalIgnoreCase) == 0))
            {
                ManagementQuery.ParseToken(ref strA, tokenResultRole, "=", ref flag5, ref str4);
                goto Label_01EA;
            }
            if ((strA.Length >= tokenRole.Length) && (string.Compare(strA, 0, tokenRole, 0, tokenRole.Length, StringComparison.OrdinalIgnoreCase) == 0))
            {
                ManagementQuery.ParseToken(ref strA, tokenRole, "=", ref flag6, ref str5);
                goto Label_01EA;
            }
            if ((strA.Length >= tokenRequiredQualifier.Length) && (string.Compare(strA, 0, tokenRequiredQualifier, 0, tokenRequiredQualifier.Length, StringComparison.OrdinalIgnoreCase) == 0))
            {
                ManagementQuery.ParseToken(ref strA, tokenRequiredQualifier, "=", ref flag7, ref str6);
                goto Label_01EA;
            }
            if ((strA.Length >= tokenRequiredAssocQualifier.Length) && (string.Compare(strA, 0, tokenRequiredAssocQualifier, 0, tokenRequiredAssocQualifier.Length, StringComparison.OrdinalIgnoreCase) == 0))
            {
                ManagementQuery.ParseToken(ref strA, tokenRequiredAssocQualifier, "=", ref flag8, ref str7);
                goto Label_01EA;
            }
            if ((strA.Length >= tokenSchemaOnly.Length) && (string.Compare(strA, 0, tokenSchemaOnly, 0, tokenSchemaOnly.Length, StringComparison.OrdinalIgnoreCase) == 0))
            {
                ManagementQuery.ParseToken(ref strA, tokenSchemaOnly, ref flag10);
                flag2 = true;
                goto Label_01EA;
            }
            if ((strA.Length >= tokenClassDefsOnly.Length) && (string.Compare(strA, 0, tokenClassDefsOnly, 0, tokenClassDefsOnly.Length, StringComparison.OrdinalIgnoreCase) == 0))
            {
                ManagementQuery.ParseToken(ref strA, tokenClassDefsOnly, ref flag9);
                flag = true;
                goto Label_01EA;
            }
            if (strA.Length != 0)
            {
                throw new ArgumentException(RC.GetString("INVALID_QUERY"));
            }
            if (flag10 && flag9)
            {
                throw new ArgumentException(RC.GetString("INVALID_QUERY"));
            }
        Label_0450:
            this.sourceObject = str;
            this.relatedClass = tokenValue;
            this.relationshipClass = str3;
            this.relatedRole = str4;
            this.thisRole = str5;
            this.relatedQualifier = str6;
            this.relationshipQualifier = str7;
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

        public string RelatedClass
        {
            get
            {
                if (this.relatedClass == null)
                {
                    return string.Empty;
                }
                return this.relatedClass;
            }
            set
            {
                this.relatedClass = value;
                this.BuildQuery();
                base.FireIdentifierChanged();
            }
        }

        public string RelatedQualifier
        {
            get
            {
                if (this.relatedQualifier == null)
                {
                    return string.Empty;
                }
                return this.relatedQualifier;
            }
            set
            {
                this.relatedQualifier = value;
                this.BuildQuery();
                base.FireIdentifierChanged();
            }
        }

        public string RelatedRole
        {
            get
            {
                if (this.relatedRole == null)
                {
                    return string.Empty;
                }
                return this.relatedRole;
            }
            set
            {
                this.relatedRole = value;
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

