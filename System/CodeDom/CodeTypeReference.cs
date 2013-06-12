namespace System.CodeDom
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;

    [Serializable, ComVisible(true), ClassInterface(ClassInterfaceType.AutoDispatch)]
    public class CodeTypeReference : CodeObject
    {
        private CodeTypeReference arrayElementType;
        private int arrayRank;
        private string baseType;
        [OptionalField]
        private bool isInterface;
        [OptionalField]
        private bool needsFixup;
        [OptionalField]
        private CodeTypeReferenceOptions referenceOptions;
        [OptionalField]
        private CodeTypeReferenceCollection typeArguments;

        public CodeTypeReference()
        {
            this.baseType = string.Empty;
            this.arrayRank = 0;
            this.arrayElementType = null;
        }

        public CodeTypeReference(CodeTypeParameter typeParameter) : this((typeParameter == null) ? null : typeParameter.Name)
        {
            this.referenceOptions = CodeTypeReferenceOptions.GenericTypeParameter;
        }

        public CodeTypeReference(string typeName)
        {
            this.Initialize(typeName);
        }

        public CodeTypeReference(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (type.IsArray)
            {
                this.arrayRank = type.GetArrayRank();
                this.arrayElementType = new CodeTypeReference(type.GetElementType());
                this.baseType = null;
            }
            else
            {
                this.InitializeFromType(type);
                this.arrayRank = 0;
                this.arrayElementType = null;
            }
            this.isInterface = type.IsInterface;
        }

        public CodeTypeReference(CodeTypeReference arrayType, int rank)
        {
            this.baseType = null;
            this.arrayRank = rank;
            this.arrayElementType = arrayType;
        }

        public CodeTypeReference(string typeName, CodeTypeReferenceOptions codeTypeReferenceOption)
        {
            this.Initialize(typeName, codeTypeReferenceOption);
        }

        public CodeTypeReference(string typeName, params CodeTypeReference[] typeArguments) : this(typeName)
        {
            if ((typeArguments != null) && (typeArguments.Length > 0))
            {
                this.TypeArguments.AddRange(typeArguments);
            }
        }

        public CodeTypeReference(string baseType, int rank)
        {
            this.baseType = null;
            this.arrayRank = rank;
            this.arrayElementType = new CodeTypeReference(baseType);
        }

        public CodeTypeReference(Type type, CodeTypeReferenceOptions codeTypeReferenceOption) : this(type)
        {
            this.referenceOptions = codeTypeReferenceOption;
        }

        private void Initialize(string typeName)
        {
            this.Initialize(typeName, this.referenceOptions);
        }

        private void Initialize(string typeName, CodeTypeReferenceOptions options)
        {
            this.Options = options;
            if ((typeName == null) || (typeName.Length == 0))
            {
                typeName = typeof(void).FullName;
                this.baseType = typeName;
                this.arrayRank = 0;
                this.arrayElementType = null;
            }
            else
            {
                typeName = this.RipOffAssemblyInformationFromTypeName(typeName);
                int num = typeName.Length - 1;
                int num2 = num;
                this.needsFixup = true;
                Queue queue = new Queue();
                while (num2 >= 0)
                {
                    int num3 = 1;
                    if (typeName[num2--] != ']')
                    {
                        break;
                    }
                    while ((num2 >= 0) && (typeName[num2] == ','))
                    {
                        num3++;
                        num2--;
                    }
                    if ((num2 < 0) || (typeName[num2] != '['))
                    {
                        break;
                    }
                    queue.Enqueue(num3);
                    num2--;
                    num = num2;
                }
                num2 = num;
                ArrayList list = new ArrayList();
                Stack stack = new Stack();
                if ((num2 > 0) && (typeName[num2--] == ']'))
                {
                    this.needsFixup = false;
                    int num4 = 1;
                    int num5 = num;
                    while (num2 >= 0)
                    {
                        if (typeName[num2] == '[')
                        {
                            if (--num4 == 0)
                            {
                                break;
                            }
                        }
                        else if (typeName[num2] == ']')
                        {
                            num4++;
                        }
                        else if ((typeName[num2] == ',') && (num4 == 1))
                        {
                            if ((num2 + 1) < num5)
                            {
                                stack.Push(typeName.Substring(num2 + 1, (num5 - num2) - 1));
                            }
                            num5 = num2;
                        }
                        num2--;
                    }
                    if ((num2 > 0) && (((num - num2) - 1) > 0))
                    {
                        if ((num2 + 1) < num5)
                        {
                            stack.Push(typeName.Substring(num2 + 1, (num5 - num2) - 1));
                        }
                        while (stack.Count > 0)
                        {
                            string str = this.RipOffAssemblyInformationFromTypeName((string) stack.Pop());
                            list.Add(new CodeTypeReference(str));
                        }
                        num = num2 - 1;
                    }
                }
                if (num < 0)
                {
                    this.baseType = typeName;
                }
                else
                {
                    if (queue.Count > 0)
                    {
                        CodeTypeReference arrayType = new CodeTypeReference(typeName.Substring(0, num + 1), this.Options);
                        for (int i = 0; i < list.Count; i++)
                        {
                            arrayType.TypeArguments.Add((CodeTypeReference) list[i]);
                        }
                        while (queue.Count > 1)
                        {
                            arrayType = new CodeTypeReference(arrayType, (int) queue.Dequeue());
                        }
                        this.baseType = null;
                        this.arrayRank = (int) queue.Dequeue();
                        this.arrayElementType = arrayType;
                    }
                    else if (list.Count > 0)
                    {
                        for (int j = 0; j < list.Count; j++)
                        {
                            this.TypeArguments.Add((CodeTypeReference) list[j]);
                        }
                        this.baseType = typeName.Substring(0, num + 1);
                    }
                    else
                    {
                        this.baseType = typeName;
                    }
                    if ((this.baseType != null) && (this.baseType.IndexOf('`') != -1))
                    {
                        this.needsFixup = false;
                    }
                }
            }
        }

        private void InitializeFromType(Type type)
        {
            this.baseType = type.Name;
            if (!type.IsGenericParameter)
            {
                Type declaringType = type;
                while (declaringType.IsNested)
                {
                    declaringType = declaringType.DeclaringType;
                    this.baseType = declaringType.Name + "+" + this.baseType;
                }
                if (!string.IsNullOrEmpty(type.Namespace))
                {
                    this.baseType = type.Namespace + "." + this.baseType;
                }
            }
            if (type.IsGenericType && !type.ContainsGenericParameters)
            {
                Type[] genericArguments = type.GetGenericArguments();
                for (int i = 0; i < genericArguments.Length; i++)
                {
                    this.TypeArguments.Add(new CodeTypeReference(genericArguments[i]));
                }
            }
            else if (!type.IsGenericTypeDefinition)
            {
                this.needsFixup = true;
            }
        }

        private string RipOffAssemblyInformationFromTypeName(string typeName)
        {
            int startIndex = 0;
            int num2 = typeName.Length - 1;
            string str = typeName;
            while ((startIndex < typeName.Length) && char.IsWhiteSpace(typeName[startIndex]))
            {
                startIndex++;
            }
            while ((num2 >= 0) && char.IsWhiteSpace(typeName[num2]))
            {
                num2--;
            }
            if (startIndex < num2)
            {
                if ((typeName[startIndex] == '[') && (typeName[num2] == ']'))
                {
                    startIndex++;
                    num2--;
                }
                if (typeName[num2] == ']')
                {
                    return str;
                }
                int num3 = 0;
                for (int i = num2; i >= startIndex; i--)
                {
                    if (typeName[i] == ',')
                    {
                        num3++;
                        if (num3 == 4)
                        {
                            return typeName.Substring(startIndex, i - startIndex);
                        }
                    }
                }
            }
            return str;
        }

        public CodeTypeReference ArrayElementType
        {
            get
            {
                return this.arrayElementType;
            }
            set
            {
                this.arrayElementType = value;
            }
        }

        public int ArrayRank
        {
            get
            {
                return this.arrayRank;
            }
            set
            {
                this.arrayRank = value;
            }
        }

        public string BaseType
        {
            get
            {
                if ((this.arrayRank > 0) && (this.arrayElementType != null))
                {
                    return this.arrayElementType.BaseType;
                }
                if (string.IsNullOrEmpty(this.baseType))
                {
                    return string.Empty;
                }
                string baseType = this.baseType;
                if (this.needsFixup && (this.TypeArguments.Count > 0))
                {
                    baseType = baseType + '`' + this.TypeArguments.Count.ToString(CultureInfo.InvariantCulture);
                }
                return baseType;
            }
            set
            {
                this.baseType = value;
                this.Initialize(this.baseType);
            }
        }

        internal bool IsInterface
        {
            get
            {
                return this.isInterface;
            }
        }

        [ComVisible(false)]
        public CodeTypeReferenceOptions Options
        {
            get
            {
                return this.referenceOptions;
            }
            set
            {
                this.referenceOptions = value;
            }
        }

        [ComVisible(false)]
        public CodeTypeReferenceCollection TypeArguments
        {
            get
            {
                if ((this.arrayRank > 0) && (this.arrayElementType != null))
                {
                    return this.arrayElementType.TypeArguments;
                }
                if (this.typeArguments == null)
                {
                    this.typeArguments = new CodeTypeReferenceCollection();
                }
                return this.typeArguments;
            }
        }
    }
}

