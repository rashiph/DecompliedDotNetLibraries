namespace System.CodeDom
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Threading;

    [Serializable, ClassInterface(ClassInterfaceType.AutoDispatch), ComVisible(true)]
    public class CodeTypeDeclaration : CodeTypeMember
    {
        private System.Reflection.TypeAttributes attributes;
        private CodeTypeReferenceCollection baseTypes;
        private const int BaseTypesCollection = 1;
        private bool isEnum;
        [OptionalField]
        private bool isPartial;
        private bool isStruct;
        private CodeTypeMemberCollection members;
        private const int MembersCollection = 2;
        private int populated;
        [OptionalField]
        private CodeTypeParameterCollection typeParameters;

        public event EventHandler PopulateBaseTypes;

        public event EventHandler PopulateMembers;

        public CodeTypeDeclaration()
        {
            this.attributes = System.Reflection.TypeAttributes.Public;
            this.baseTypes = new CodeTypeReferenceCollection();
            this.members = new CodeTypeMemberCollection();
        }

        public CodeTypeDeclaration(string name)
        {
            this.attributes = System.Reflection.TypeAttributes.Public;
            this.baseTypes = new CodeTypeReferenceCollection();
            this.members = new CodeTypeMemberCollection();
            base.Name = name;
        }

        public CodeTypeReferenceCollection BaseTypes
        {
            get
            {
                if ((this.populated & 1) == 0)
                {
                    this.populated |= 1;
                    if (this.PopulateBaseTypes != null)
                    {
                        this.PopulateBaseTypes(this, EventArgs.Empty);
                    }
                }
                return this.baseTypes;
            }
        }

        public bool IsClass
        {
            get
            {
                return ((((this.attributes & System.Reflection.TypeAttributes.ClassSemanticsMask) == System.Reflection.TypeAttributes.AnsiClass) && !this.isEnum) && !this.isStruct);
            }
            set
            {
                if (value)
                {
                    this.attributes &= ~System.Reflection.TypeAttributes.ClassSemanticsMask;
                    this.attributes = this.attributes;
                    this.isStruct = false;
                    this.isEnum = false;
                }
            }
        }

        public bool IsEnum
        {
            get
            {
                return this.isEnum;
            }
            set
            {
                if (value)
                {
                    this.attributes &= ~System.Reflection.TypeAttributes.ClassSemanticsMask;
                    this.isStruct = false;
                    this.isEnum = true;
                }
                else
                {
                    this.isEnum = false;
                }
            }
        }

        public bool IsInterface
        {
            get
            {
                return ((this.attributes & System.Reflection.TypeAttributes.ClassSemanticsMask) == System.Reflection.TypeAttributes.ClassSemanticsMask);
            }
            set
            {
                if (value)
                {
                    this.attributes &= ~System.Reflection.TypeAttributes.ClassSemanticsMask;
                    this.attributes |= System.Reflection.TypeAttributes.ClassSemanticsMask;
                    this.isStruct = false;
                    this.isEnum = false;
                }
                else
                {
                    this.attributes &= ~System.Reflection.TypeAttributes.ClassSemanticsMask;
                }
            }
        }

        public bool IsPartial
        {
            get
            {
                return this.isPartial;
            }
            set
            {
                this.isPartial = value;
            }
        }

        public bool IsStruct
        {
            get
            {
                return this.isStruct;
            }
            set
            {
                if (value)
                {
                    this.attributes &= ~System.Reflection.TypeAttributes.ClassSemanticsMask;
                    this.isStruct = true;
                    this.isEnum = false;
                }
                else
                {
                    this.isStruct = false;
                }
            }
        }

        public CodeTypeMemberCollection Members
        {
            get
            {
                if ((this.populated & 2) == 0)
                {
                    this.populated |= 2;
                    if (this.PopulateMembers != null)
                    {
                        this.PopulateMembers(this, EventArgs.Empty);
                    }
                }
                return this.members;
            }
        }

        public System.Reflection.TypeAttributes TypeAttributes
        {
            get
            {
                return this.attributes;
            }
            set
            {
                this.attributes = value;
            }
        }

        [ComVisible(false)]
        public CodeTypeParameterCollection TypeParameters
        {
            get
            {
                if (this.typeParameters == null)
                {
                    this.typeParameters = new CodeTypeParameterCollection();
                }
                return this.typeParameters;
            }
        }
    }
}

