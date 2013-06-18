namespace System.Data
{
    using System;

    internal sealed class Function
    {
        internal readonly int argumentCount;
        internal static string[] FunctionName = new string[] { 
            "Unknown", "Ascii", "Char", "CharIndex", "Difference", "Len", "Lower", "LTrim", "Patindex", "Replicate", "Reverse", "Right", "RTrim", "Soundex", "Space", "Str", 
            "Stuff", "Substring", "Upper", "IsNull", "Iif", "Convert", "cInt", "cBool", "cDate", "cDbl", "cStr", "Abs", "Acos", "In", "Trim", "Sum", 
            "Avg", "Min", "Max", "Count", "StDev", "Var", "DateTimeOffset"
         };
        internal readonly FunctionId id;
        internal readonly bool IsValidateArguments;
        internal readonly bool IsVariantArgumentList;
        internal readonly string name;
        internal readonly Type[] parameters;
        internal readonly Type result;

        internal Function()
        {
            this.parameters = new Type[3];
            this.name = null;
            this.id = FunctionId.none;
            this.result = null;
            this.IsValidateArguments = false;
            this.argumentCount = 0;
        }

        internal Function(string name, FunctionId id, Type result, bool IsValidateArguments, bool IsVariantArgumentList, int argumentCount, Type a1, Type a2, Type a3)
        {
            this.parameters = new Type[3];
            this.name = name;
            this.id = id;
            this.result = result;
            this.IsValidateArguments = IsValidateArguments;
            this.IsVariantArgumentList = IsVariantArgumentList;
            this.argumentCount = argumentCount;
            if (a1 != null)
            {
                this.parameters[0] = a1;
            }
            if (a2 != null)
            {
                this.parameters[1] = a2;
            }
            if (a3 != null)
            {
                this.parameters[2] = a3;
            }
        }
    }
}

