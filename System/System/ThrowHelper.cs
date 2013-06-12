namespace System
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    internal static class ThrowHelper
    {
        internal static string GetArgumentName(System.ExceptionArgument argument)
        {
            switch (argument)
            {
                case System.ExceptionArgument.obj:
                    return "obj";

                case System.ExceptionArgument.dictionary:
                    return "dictionary";

                case System.ExceptionArgument.array:
                    return "array";

                case System.ExceptionArgument.info:
                    return "info";

                case System.ExceptionArgument.key:
                    return "key";

                case System.ExceptionArgument.collection:
                    return "collection";

                case System.ExceptionArgument.match:
                    return "match";

                case System.ExceptionArgument.converter:
                    return "converter";

                case System.ExceptionArgument.queue:
                    return "queue";

                case System.ExceptionArgument.stack:
                    return "stack";

                case System.ExceptionArgument.capacity:
                    return "capacity";

                case System.ExceptionArgument.index:
                    return "index";

                case System.ExceptionArgument.startIndex:
                    return "startIndex";

                case System.ExceptionArgument.value:
                    return "value";

                case System.ExceptionArgument.count:
                    return "count";

                case System.ExceptionArgument.arrayIndex:
                    return "arrayIndex";

                case System.ExceptionArgument.item:
                    return "item";
            }
            return string.Empty;
        }

        internal static string GetResourceName(System.ExceptionResource resource)
        {
            switch (resource)
            {
                case System.ExceptionResource.Argument_ImplementIComparable:
                    return "Argument_ImplementIComparable";

                case System.ExceptionResource.ArgumentOutOfRange_NeedNonNegNum:
                    return "ArgumentOutOfRange_NeedNonNegNum";

                case System.ExceptionResource.ArgumentOutOfRange_NeedNonNegNumRequired:
                    return "ArgumentOutOfRange_NeedNonNegNumRequired";

                case System.ExceptionResource.Arg_ArrayPlusOffTooSmall:
                    return "Arg_ArrayPlusOffTooSmall";

                case System.ExceptionResource.Argument_AddingDuplicate:
                    return "Argument_AddingDuplicate";

                case System.ExceptionResource.Serialization_InvalidOnDeser:
                    return "Serialization_InvalidOnDeser";

                case System.ExceptionResource.Serialization_MismatchedCount:
                    return "Serialization_MismatchedCount";

                case System.ExceptionResource.Serialization_MissingValues:
                    return "Serialization_MissingValues";

                case System.ExceptionResource.Arg_RankMultiDimNotSupported:
                    return "Arg_MultiRank";

                case System.ExceptionResource.Arg_NonZeroLowerBound:
                    return "Arg_NonZeroLowerBound";

                case System.ExceptionResource.Argument_InvalidArrayType:
                    return "Invalid_Array_Type";

                case System.ExceptionResource.NotSupported_KeyCollectionSet:
                    return "NotSupported_KeyCollectionSet";

                case System.ExceptionResource.ArgumentOutOfRange_SmallCapacity:
                    return "ArgumentOutOfRange_SmallCapacity";

                case System.ExceptionResource.ArgumentOutOfRange_Index:
                    return "ArgumentOutOfRange_Index";

                case System.ExceptionResource.Argument_InvalidOffLen:
                    return "Argument_InvalidOffLen";

                case System.ExceptionResource.InvalidOperation_CannotRemoveFromStackOrQueue:
                    return "InvalidOperation_CannotRemoveFromStackOrQueue";

                case System.ExceptionResource.InvalidOperation_EmptyCollection:
                    return "InvalidOperation_EmptyCollection";

                case System.ExceptionResource.InvalidOperation_EmptyQueue:
                    return "InvalidOperation_EmptyQueue";

                case System.ExceptionResource.InvalidOperation_EnumOpCantHappen:
                    return "InvalidOperation_EnumOpCantHappen";

                case System.ExceptionResource.InvalidOperation_EnumFailedVersion:
                    return "InvalidOperation_EnumFailedVersion";

                case System.ExceptionResource.InvalidOperation_EmptyStack:
                    return "InvalidOperation_EmptyStack";

                case System.ExceptionResource.InvalidOperation_EnumNotStarted:
                    return "InvalidOperation_EnumNotStarted";

                case System.ExceptionResource.InvalidOperation_EnumEnded:
                    return "InvalidOperation_EnumEnded";

                case System.ExceptionResource.NotSupported_SortedListNestedWrite:
                    return "NotSupported_SortedListNestedWrite";

                case System.ExceptionResource.NotSupported_ValueCollectionSet:
                    return "NotSupported_ValueCollectionSet";
            }
            return string.Empty;
        }

        internal static void IfNullAndNullsAreIllegalThenThrow<T>(object value, System.ExceptionArgument argName)
        {
            if ((value == null) && (default(T) != null))
            {
                ThrowArgumentNullException(argName);
            }
        }

        internal static void ThrowArgumentException(System.ExceptionResource resource)
        {
            throw new ArgumentException(SR.GetString(GetResourceName(resource)));
        }

        internal static void ThrowArgumentNullException(System.ExceptionArgument argument)
        {
            throw new ArgumentNullException(GetArgumentName(argument));
        }

        internal static void ThrowArgumentOutOfRangeException(System.ExceptionArgument argument)
        {
            throw new ArgumentOutOfRangeException(GetArgumentName(argument));
        }

        internal static void ThrowArgumentOutOfRangeException(System.ExceptionArgument argument, System.ExceptionResource resource)
        {
            throw new ArgumentOutOfRangeException(GetArgumentName(argument), SR.GetString(GetResourceName(resource)));
        }

        internal static void ThrowInvalidOperationException(System.ExceptionResource resource)
        {
            throw new InvalidOperationException(SR.GetString(GetResourceName(resource)));
        }

        internal static void ThrowKeyNotFoundException()
        {
            throw new KeyNotFoundException();
        }

        internal static void ThrowNotSupportedException(System.ExceptionResource resource)
        {
            throw new NotSupportedException(SR.GetString(GetResourceName(resource)));
        }

        internal static void ThrowSerializationException(System.ExceptionResource resource)
        {
            throw new SerializationException(SR.GetString(GetResourceName(resource)));
        }

        internal static void ThrowWrongKeyTypeArgumentException(object key, Type targetType)
        {
            throw new ArgumentException(SR.GetString("Arg_WrongType", new object[] { key, targetType }), "key");
        }

        internal static void ThrowWrongValueTypeArgumentException(object value, Type targetType)
        {
            throw new ArgumentException(SR.GetString("Arg_WrongType", new object[] { value, targetType }), "value");
        }
    }
}

