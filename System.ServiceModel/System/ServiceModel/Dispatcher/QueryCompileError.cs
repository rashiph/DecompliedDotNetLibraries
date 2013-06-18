namespace System.ServiceModel.Dispatcher
{
    using System;

    internal enum QueryCompileError
    {
        None,
        General,
        CouldNotParseExpression,
        UnexpectedToken,
        UnsupportedOperator,
        UnsupportedAxis,
        UnsupportedFunction,
        UnsupportedNodeTest,
        UnsupportedExpression,
        AbsolutePathRequired,
        InvalidNCName,
        InvalidVariable,
        InvalidNumber,
        InvalidLiteral,
        InvalidOperatorName,
        InvalidNodeType,
        InvalidExpression,
        InvalidFunction,
        InvalidLocationPath,
        InvalidLocationStep,
        InvalidAxisSpecifier,
        InvalidNodeTest,
        InvalidPredicate,
        InvalidComparison,
        InvalidOrdinal,
        InvalidType,
        InvalidTypeConversion,
        NoNamespaceForPrefix,
        MismatchedParen,
        DuplicateOpcode,
        OpcodeExists,
        OpcodeNotFound,
        PredicateNestingTooDeep
    }
}

