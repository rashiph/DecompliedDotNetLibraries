namespace System.Data.Design
{
    using System;
    using System.CodeDom;
    using System.Data;

    internal sealed class RelationHandler
    {
        private TypedDataSourceCodeGenerator codeGenerator;
        private DesignRelationCollection relations;

        internal RelationHandler(TypedDataSourceCodeGenerator codeGenerator, DesignRelationCollection relations)
        {
            this.codeGenerator = codeGenerator;
            this.relations = relations;
        }

        internal void AddPrivateVars(CodeTypeDeclaration dataSourceClass)
        {
            if (dataSourceClass == null)
            {
                throw new InternalException("DataSource CodeTypeDeclaration should not be null.");
            }
            if (this.relations != null)
            {
                foreach (DesignRelation relation in this.relations)
                {
                    if (relation.DataRelation != null)
                    {
                        string generatorRelationVarName = relation.GeneratorRelationVarName;
                        dataSourceClass.Members.Add(CodeGenHelper.FieldDecl(CodeGenHelper.GlobalType(typeof(DataRelation)), generatorRelationVarName));
                    }
                }
            }
        }

        internal DesignRelationCollection Relations
        {
            get
            {
                return this.relations;
            }
        }
    }
}

