namespace System.ServiceModel.Dispatcher
{
    using System;

    internal enum QueryProcessingError
    {
        None,
        Unexpected,
        TypeMismatch,
        UnsupportedXmlNodeType,
        NodeCountMaxExceeded,
        InvalidXmlAttributes,
        InvalidNavigatorPosition,
        NotAtomized,
        NotSupported,
        InvalidBodyAccess,
        InvalidNamespacePrefix
    }
}

