namespace System.ComponentModel
{
    using System;

    public enum MaskedTextResultHint
    {
        AlphanumericCharacterExpected = -2,
        AsciiCharacterExpected = -1,
        CharacterEscaped = 1,
        DigitExpected = -3,
        InvalidInput = -51,
        LetterExpected = -4,
        NoEffect = 2,
        NonEditPosition = -54,
        PositionOutOfRange = -55,
        PromptCharNotAllowed = -52,
        SideEffect = 3,
        SignedDigitExpected = -5,
        Success = 4,
        UnavailableEditPosition = -53,
        Unknown = 0
    }
}

