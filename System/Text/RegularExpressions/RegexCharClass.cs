namespace System.Text.RegularExpressions
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Text;

    internal sealed class RegexCharClass
    {
        private bool _canonical;
        private StringBuilder _categories;
        private static Dictionary<string, string> _definedCategories;
        private static readonly LowerCaseMapping[] _lcTable = new LowerCaseMapping[] { 
            new LowerCaseMapping('A', 'Z', 1, 0x20), new LowerCaseMapping('\x00c0', '\x00de', 1, 0x20), new LowerCaseMapping('Ā', 'Į', 2, 0), new LowerCaseMapping('İ', 'İ', 0, 0x69), new LowerCaseMapping('Ĳ', 'Ķ', 2, 0), new LowerCaseMapping('Ĺ', 'Ň', 3, 0), new LowerCaseMapping('Ŋ', 'Ŷ', 2, 0), new LowerCaseMapping('Ÿ', 'Ÿ', 0, 0xff), new LowerCaseMapping('Ź', 'Ž', 3, 0), new LowerCaseMapping('Ɓ', 'Ɓ', 0, 0x253), new LowerCaseMapping('Ƃ', 'Ƅ', 2, 0), new LowerCaseMapping('Ɔ', 'Ɔ', 0, 0x254), new LowerCaseMapping('Ƈ', 'Ƈ', 0, 0x188), new LowerCaseMapping('Ɖ', 'Ɗ', 1, 0xcd), new LowerCaseMapping('Ƌ', 'Ƌ', 0, 0x18c), new LowerCaseMapping('Ǝ', 'Ǝ', 0, 0x1dd), 
            new LowerCaseMapping('Ə', 'Ə', 0, 0x259), new LowerCaseMapping('Ɛ', 'Ɛ', 0, 0x25b), new LowerCaseMapping('Ƒ', 'Ƒ', 0, 0x192), new LowerCaseMapping('Ɠ', 'Ɠ', 0, 0x260), new LowerCaseMapping('Ɣ', 'Ɣ', 0, 0x263), new LowerCaseMapping('Ɩ', 'Ɩ', 0, 0x269), new LowerCaseMapping('Ɨ', 'Ɨ', 0, 0x268), new LowerCaseMapping('Ƙ', 'Ƙ', 0, 0x199), new LowerCaseMapping('Ɯ', 'Ɯ', 0, 0x26f), new LowerCaseMapping('Ɲ', 'Ɲ', 0, 0x272), new LowerCaseMapping('Ɵ', 'Ɵ', 0, 0x275), new LowerCaseMapping('Ơ', 'Ƥ', 2, 0), new LowerCaseMapping('Ƨ', 'Ƨ', 0, 0x1a8), new LowerCaseMapping('Ʃ', 'Ʃ', 0, 0x283), new LowerCaseMapping('Ƭ', 'Ƭ', 0, 0x1ad), new LowerCaseMapping('Ʈ', 'Ʈ', 0, 0x288), 
            new LowerCaseMapping('Ư', 'Ư', 0, 0x1b0), new LowerCaseMapping('Ʊ', 'Ʋ', 1, 0xd9), new LowerCaseMapping('Ƴ', 'Ƶ', 3, 0), new LowerCaseMapping('Ʒ', 'Ʒ', 0, 0x292), new LowerCaseMapping('Ƹ', 'Ƹ', 0, 0x1b9), new LowerCaseMapping('Ƽ', 'Ƽ', 0, 0x1bd), new LowerCaseMapping('Ǆ', 'ǅ', 0, 0x1c6), new LowerCaseMapping('Ǉ', 'ǈ', 0, 0x1c9), new LowerCaseMapping('Ǌ', 'ǋ', 0, 460), new LowerCaseMapping('Ǎ', 'Ǜ', 3, 0), new LowerCaseMapping('Ǟ', 'Ǯ', 2, 0), new LowerCaseMapping('Ǳ', 'ǲ', 0, 0x1f3), new LowerCaseMapping('Ǵ', 'Ǵ', 0, 0x1f5), new LowerCaseMapping('Ǻ', 'Ȗ', 2, 0), new LowerCaseMapping('Ά', 'Ά', 0, 940), new LowerCaseMapping('Έ', 'Ί', 1, 0x25), 
            new LowerCaseMapping('Ό', 'Ό', 0, 0x3cc), new LowerCaseMapping('Ύ', 'Ώ', 1, 0x3f), new LowerCaseMapping('Α', 'Ϋ', 1, 0x20), new LowerCaseMapping('Ϣ', 'Ϯ', 2, 0), new LowerCaseMapping('Ё', 'Џ', 1, 80), new LowerCaseMapping('А', 'Я', 1, 0x20), new LowerCaseMapping('Ѡ', 'Ҁ', 2, 0), new LowerCaseMapping('Ґ', 'Ҿ', 2, 0), new LowerCaseMapping('Ӂ', 'Ӄ', 3, 0), new LowerCaseMapping('Ӈ', 'Ӈ', 0, 0x4c8), new LowerCaseMapping('Ӌ', 'Ӌ', 0, 0x4cc), new LowerCaseMapping('Ӑ', 'Ӫ', 2, 0), new LowerCaseMapping('Ӯ', 'Ӵ', 2, 0), new LowerCaseMapping('Ӹ', 'Ӹ', 0, 0x4f9), new LowerCaseMapping('Ա', 'Ֆ', 1, 0x30), new LowerCaseMapping('Ⴀ', 'Ⴥ', 1, 0x30), 
            new LowerCaseMapping('Ḁ', 'Ỹ', 2, 0), new LowerCaseMapping('Ἀ', 'Ἇ', 1, -8), new LowerCaseMapping('Ἐ', '἟', 1, -8), new LowerCaseMapping('Ἠ', 'Ἧ', 1, -8), new LowerCaseMapping('Ἰ', 'Ἷ', 1, -8), new LowerCaseMapping('Ὀ', 'Ὅ', 1, -8), new LowerCaseMapping('Ὑ', 'Ὑ', 0, 0x1f51), new LowerCaseMapping('Ὓ', 'Ὓ', 0, 0x1f53), new LowerCaseMapping('Ὕ', 'Ὕ', 0, 0x1f55), new LowerCaseMapping('Ὗ', 'Ὗ', 0, 0x1f57), new LowerCaseMapping('Ὠ', 'Ὧ', 1, -8), new LowerCaseMapping('ᾈ', 'ᾏ', 1, -8), new LowerCaseMapping('ᾘ', 'ᾟ', 1, -8), new LowerCaseMapping('ᾨ', 'ᾯ', 1, -8), new LowerCaseMapping('Ᾰ', 'Ᾱ', 1, -8), new LowerCaseMapping('Ὰ', 'Ά', 1, -74), 
            new LowerCaseMapping('ᾼ', 'ᾼ', 0, 0x1fb3), new LowerCaseMapping('Ὲ', 'Ή', 1, -86), new LowerCaseMapping('ῌ', 'ῌ', 0, 0x1fc3), new LowerCaseMapping('Ῐ', 'Ῑ', 1, -8), new LowerCaseMapping('Ὶ', 'Ί', 1, -100), new LowerCaseMapping('Ῠ', 'Ῡ', 1, -8), new LowerCaseMapping('Ὺ', 'Ύ', 1, -112), new LowerCaseMapping('Ῥ', 'Ῥ', 0, 0x1fe5), new LowerCaseMapping('Ὸ', 'Ό', 1, -128), new LowerCaseMapping('Ὼ', 'Ώ', 1, -126), new LowerCaseMapping('ῼ', 'ῼ', 0, 0x1ff3), new LowerCaseMapping('Ⅰ', 'Ⅿ', 1, 0x10), new LowerCaseMapping('Ⓐ', 'ⓐ', 1, 0x1a), new LowerCaseMapping(0xff21, 0xff3a, 1, 0x20)
         };
        private bool _negate;
        private static readonly string[,] _propTable = new string[,] { 
            { "IsAlphabeticPresentationForms", "ﬀﭐ" }, { "IsArabic", "؀܀" }, { "IsArabicPresentationForms-A", "ﭐ︀" }, { "IsArabicPresentationForms-B", "ﹰ＀" }, { "IsArmenian", "԰֐" }, { "IsArrows", "←∀" }, { "IsBasicLatin", "\0\x0080" }, { "IsBengali", "ঀ਀" }, { "IsBlockElements", "▀■" }, { "IsBopomofo", "㄀㄰" }, { "IsBopomofoExtended", "ㆠ㇀" }, { "IsBoxDrawing", "─▀" }, { "IsBraillePatterns", "⠀⤀" }, { "IsBuhid", "ᝀᝠ" }, { "IsCJKCompatibility", "㌀㐀" }, { "IsCJKCompatibilityForms", "︰﹐" }, 
            { "IsCJKCompatibilityIdeographs", "豈ﬀ" }, { "IsCJKRadicalsSupplement", "⺀⼀" }, { "IsCJKSymbolsandPunctuation", "　぀" }, { "IsCJKUnifiedIdeographs", "一ꀀ" }, { "IsCJKUnifiedIdeographsExtensionA", "㐀䷀" }, { "IsCherokee", "Ꭰ᐀" }, { "IsCombiningDiacriticalMarks", "̀Ͱ" }, { "IsCombiningDiacriticalMarksforSymbols", "⃐℀" }, { "IsCombiningHalfMarks", "︠︰" }, { "IsCombiningMarksforSymbols", "⃐℀" }, { "IsControlPictures", "␀⑀" }, { "IsCurrencySymbols", "₠⃐" }, { "IsCyrillic", "ЀԀ" }, { "IsCyrillicSupplement", "Ԁ԰" }, { "IsDevanagari", "ऀঀ" }, { "IsDingbats", "✀⟀" }, 
            { "IsEnclosedAlphanumerics", "①─" }, { "IsEnclosedCJKLettersandMonths", "㈀㌀" }, { "IsEthiopic", "ሀᎀ" }, { "IsGeneralPunctuation", " ⁰" }, { "IsGeometricShapes", "■☀" }, { "IsGeorgian", "Ⴀᄀ" }, { "IsGreek", "ͰЀ" }, { "IsGreekExtended", "ἀ " }, { "IsGreekandCoptic", "ͰЀ" }, { "IsGujarati", "઀଀" }, { "IsGurmukhi", "਀઀" }, { "IsHalfwidthandFullwidthForms", "＀￰" }, { "IsHangulCompatibilityJamo", "㄰㆐" }, { "IsHangulJamo", "ᄀሀ" }, { "IsHangulSyllables", "가ힰ" }, { "IsHanunoo", "ᜠᝀ" }, 
            { "IsHebrew", "֐؀" }, { "IsHighPrivateUseSurrogates", "\udb80\udc00" }, { "IsHighSurrogates", "\ud800\udb80" }, { "IsHiragana", "぀゠" }, { "IsIPAExtensions", "ɐʰ" }, { "IsIdeographicDescriptionCharacters", "⿰　" }, { "IsKanbun", "㆐ㆠ" }, { "IsKangxiRadicals", "⼀⿠" }, { "IsKannada", "ಀഀ" }, { "IsKatakana", "゠㄀" }, { "IsKatakanaPhoneticExtensions", "ㇰ㈀" }, { "IsKhmer", "ក᠀" }, { "IsKhmerSymbols", "᧠ᨀ" }, { "IsLao", "຀ༀ" }, { "IsLatin-1Supplement", "\x0080Ā" }, { "IsLatinExtended-A", "Āƀ" }, 
            { "IsLatinExtended-B", "ƀɐ" }, { "IsLatinExtendedAdditional", "Ḁἀ" }, { "IsLetterlikeSymbols", "℀⅐" }, { "IsLimbu", "ᤀᥐ" }, { "IsLowSurrogates", "\udc00\ue000" }, { "IsMalayalam", "ഀ඀" }, { "IsMathematicalOperators", "∀⌀" }, { "IsMiscellaneousMathematicalSymbols-A", "⟀⟰" }, { "IsMiscellaneousMathematicalSymbols-B", "⦀⨀" }, { "IsMiscellaneousSymbols", "☀✀" }, { "IsMiscellaneousSymbolsandArrows", "⬀Ⰰ" }, { "IsMiscellaneousTechnical", "⌀␀" }, { "IsMongolian", "᠀ᢰ" }, { "IsMyanmar", "ကႠ" }, { "IsNumberForms", "⅐←" }, { "IsOgham", " ᚠ" }, 
            { "IsOpticalCharacterRecognition", "⑀①" }, { "IsOriya", "଀஀" }, { "IsPhoneticExtensions", "ᴀᶀ" }, { "IsPrivateUse", "豈" }, { "IsPrivateUseArea", "豈" }, { "IsRunic", "ᚠᜀ" }, { "IsSinhala", "඀฀" }, { "IsSmallFormVariants", "﹐ﹰ" }, { "IsSpacingModifierLetters", "ʰ̀" }, { "IsSpecials", "￰" }, { "IsSuperscriptsandSubscripts", "⁰₠" }, { "IsSupplementalArrows-A", "⟰⠀" }, { "IsSupplementalArrows-B", "⤀⦀" }, { "IsSupplementalMathematicalOperators", "⨀⬀" }, { "IsSyriac", "܀ݐ" }, { "IsTagalog", "ᜀᜠ" }, 
            { "IsTagbanwa", "ᝠក" }, { "IsTaiLe", "ᥐᦀ" }, { "IsTamil", "஀ఀ" }, { "IsTelugu", "ఀಀ" }, { "IsThaana", "ހ߀" }, { "IsThai", "฀຀" }, { "IsTibetan", "ༀက" }, { "IsUnifiedCanadianAboriginalSyllabics", "᐀ " }, { "IsVariationSelectors", "︀︐" }, { "IsYiRadicals", "꒐ꓐ" }, { "IsYiSyllables", "ꀀ꒐" }, { "IsYijingHexagramSymbols", "䷀一" }, { "_xmlC", "-/0;A[_`a{\x00b7\x00b8\x00c0\x00d7\x00d8\x00f7\x00f8ĲĴĿŁŉŊſƀǄǍǱǴǶǺȘɐʩʻ˂ː˒̀͆͢͠Ά΋Ό΍Ύ΢ΣϏϐϗϚϛϜϝϞϟϠϡϢϴЁЍЎѐёѝў҂҃҇ҐӅӇӉӋӍӐӬӮӶӸӺԱ՗ՙ՚աևֺֻ֑֢֣־ֿ׀ׁ׃ׅׄא׫װ׳ءػـٓ٠٪ٰڸںڿۀۏې۔ە۩۪ۮ۰ۺँऄअऺ़ॎ॑ॕक़।०॰ঁ঄অ঍এ঑ও঩প঱ল঳শ঺়ঽা৅ে৉োৎৗ৘ড়৞য়৤০৲ਂਃਅ਋ਏ਑ਓ਩ਪ਱ਲ਴ਵ਷ਸ਺਼਽ਾ੃ੇ੉ੋ੎ਖ਼੝ਫ਼੟੦ੵઁ઄અઌઍ઎એ઒ઓ઩પ઱લ઴વ઺઼૆ે૊ો૎ૠૡ૦૰ଁ଄ଅ଍ଏ଑ଓ଩ପ଱ଲ଴ଶ଺଼ୄେ୉ୋ୎ୖ୘ଡ଼୞ୟୢ୦୰ஂ஄அ஋எ஑ஒ஖ங஛ஜ஝ஞ஠ண஥ந஫மஶஷ஺ா௃ெ௉ொ௎ௗ௘௧௰ఁఄఅ఍ఎ఑ఒ఩పఴవ఺ా౅ె౉ొ౎ౕ౗ౠౢ౦౰ಂ಄ಅ಍ಎ಑ಒ಩ಪ಴ವ಺ಾ೅ೆ೉ೊ೎ೕ೗ೞ೟ೠೢ೦೰ംഄഅ഍എ഑ഒഩപഺാൄെ൉ൊൎൗ൘ൠൢ൦൰กฯะ฻เ๏๐๚ກ຃ຄ຅ງຉຊ຋ຍຎດຘນຠມ຤ລ຦ວຨສຬອຯະ຺ົ຾ເ໅ໆ໇່໎໐໚༘༚༠༪༵༶༷༸༹༺༾཈ཉཪཱ྅྆ྌྐྖྗ྘ྙྮྱྸྐྵྺႠ჆აჷᄀᄁᄂᄄᄅᄈᄉᄊᄋᄍᄎᄓᄼᄽᄾᄿᅀᅁᅌᅍᅎᅏᅐᅑᅔᅖᅙᅚᅟᅢᅣᅤᅥᅦᅧᅨᅩᅪᅭᅯᅲᅴᅵᅶᆞᆟᆨᆩᆫᆬᆮᆰᆷᆹᆺᆻᆼᇃᇫᇬᇰᇱᇹᇺḀẜẠỺἀ἖Ἐ἞ἠ὆Ὀ὎ὐ὘Ὑ὚Ὓ὜Ὕ὞Ὗ὾ᾀ᾵ᾶ᾽ι᾿ῂ῅ῆ῍ῐ῔ῖ῜ῠ῭ῲ῵ῶ´⃐⃝⃡⃢Ω℧Kℬ℮ℯↀↃ々〆〇〈〡〰〱〶ぁゕ゙゛ゝゟァ・ーヿㄅㄭ一龦가힤" }, { "_xmlD", "0:٠٪۰ۺ०॰০ৰ੦ੰ૦૰୦୰௧௰౦౰೦೰൦൰๐๚໐໚༠༪၀၊፩፲០៪᠐᠚０：" }, { "_xmlI", ":;A[_`a{\x00c0\x00d7\x00d8\x00f7\x00f8ĲĴĿŁŉŊſƀǄǍǱǴǶǺȘɐʩʻ˂Ά·Έ΋Ό΍Ύ΢ΣϏϐϗϚϛϜϝϞϟϠϡϢϴЁЍЎѐёѝў҂ҐӅӇӉӋӍӐӬӮӶӸӺԱ՗ՙ՚աևא׫װ׳ءػفًٱڸںڿۀۏې۔ەۖۥۧअऺऽाक़ॢঅ঍এ঑ও঩প঱ল঳শ঺ড়৞য়ৢৰ৲ਅ਋ਏ਑ਓ਩ਪ਱ਲ਴ਵ਷ਸ਺ਖ਼੝ਫ਼੟ੲੵઅઌઍ઎એ઒ઓ઩પ઱લ઴વ઺ઽાૠૡଅ଍ଏ଑ଓ଩ପ଱ଲ଴ଶ଺ଽାଡ଼୞ୟୢஅ஋எ஑ஒ஖ங஛ஜ஝ஞ஠ண஥ந஫மஶஷ஺అ఍ఎ఑ఒ఩పఴవ఺ౠౢಅ಍ಎ಑ಒ಩ಪ಴ವ಺ೞ೟ೠೢഅ഍എ഑ഒഩപഺൠൢกฯะัาิเๆກ຃ຄ຅ງຉຊ຋ຍຎດຘນຠມ຤ລ຦ວຨສຬອຯະັາິຽ຾ເ໅ཀ཈ཉཪႠ჆აჷᄀᄁᄂᄄᄅᄈᄉᄊᄋᄍᄎᄓᄼᄽᄾᄿᅀᅁᅌᅍᅎᅏᅐᅑᅔᅖᅙᅚᅟᅢᅣᅤᅥᅦᅧᅨᅩᅪᅭᅯᅲᅴᅵᅶᆞᆟᆨᆩᆫᆬᆮᆰᆷᆹᆺᆻᆼᇃᇫᇬᇰᇱᇹᇺḀẜẠỺἀ἖Ἐ἞ἠ὆Ὀ὎ὐ὘Ὑ὚Ὓ὜Ὕ὞Ὗ὾ᾀ᾵ᾶ᾽ι᾿ῂ῅ῆ῍ῐ῔ῖ῜ῠ῭ῲ῵ῶ´Ω℧Kℬ℮ℯↀↃ〇〈〡〪ぁゕァ・ㄅㄭ一龦가힤" }, { "_xmlW", "$%+,0:<?A[^_`{|}~\x007f\x00a2\x00ab\x00ac\x00ad\x00ae\x00b7\x00b8\x00bb\x00bc\x00bf\x00c0ȡȢȴɐʮʰ˯̀͐͠ͰʹͶͺͻ΄·Έ΋Ό΍Ύ΢ΣϏϐϷЀ҇҈ӏӐӶӸӺԀԐԱ՗ՙ՚աֈֺֻ֑֢֣־ֿ׀ׁ׃ׅׄא׫װ׳ءػـٖ٠٪ٮ۔ە۝۞ۮ۰ۿܐܭܰ݋ހ޲ँऄअऺ़ॎॐॕक़।०॰ঁ঄অ঍এ঑ও঩প঱ল঳শ঺়ঽা৅ে৉োৎৗ৘ড়৞য়৤০৻ਂਃਅ਋ਏ਑ਓ਩ਪ਱ਲ਴ਵ਷ਸ਺਼਽ਾ੃ੇ੉ੋ੎ਖ਼੝ਫ਼੟੦ੵઁ઄અઌઍ઎એ઒ઓ઩પ઱લ઴વ઺઼૆ે૊ો૎ૐ૑ૠૡ૦૰ଁ଄ଅ଍ଏ଑ଓ଩ପ଱ଲ଴ଶ଺଼ୄେ୉ୋ୎ୖ୘ଡ଼୞ୟୢ୦ୱஂ஄அ஋எ஑ஒ஖ங஛ஜ஝ஞ஠ண஥ந஫மஶஷ஺ா௃ெ௉ொ௎ௗ௘௧௳ఁఄఅ఍ఎ఑ఒ఩పఴవ఺ా౅ె౉ొ౎ౕ౗ౠౢ౦౰ಂ಄ಅ಍ಎ಑ಒ಩ಪ಴ವ಺ಾ೅ೆ೉ೊ೎ೕ೗ೞ೟ೠೢ೦೰ംഄഅ഍എ഑ഒഩപഺാൄെ൉ൊൎൗ൘ൠൢ൦൰ං඄අ඗ක඲ඳ඼ල඾ව෇්෋ා෕ූ෗ෘ෠ෲ෴ก฻฿๏๐๚ກ຃ຄ຅ງຉຊ຋ຍຎດຘນຠມ຤ລ຦ວຨສຬອ຺ົ຾ເ໅ໆ໇່໎໐໚ໜໞༀ༄༓༺༾཈ཉཫཱ྅྆ྌྐ྘ྙ྽྾࿍࿏࿐ကဢဣဨဩါာဳံ်၀၊ၐၚႠ჆აჹᄀᅚᅟᆣᆨᇺሀሇለቇቈ቉ቊ቎ቐ቗ቘ቙ቚ቞በኇኈ኉ኊ኎ነኯኰ኱ኲ኶ኸ኿ዀ዁ዂ዆ወዏዐ዗ዘዯደጏጐ጑ጒ጖ጘጟጠፇፈ፛፩፽ᎠᏵᐁ᙭ᙯᙷᚁ᚛ᚠ᛫ᛮᛱᜀᜍᜎ᜕ᜠ᜵ᝀ᝔ᝠ᝭ᝮ᝱ᝲ᝴ក។ៗ៘៛៝០៪᠋᠎᠐᠚ᠠᡸᢀᢪḀẜẠỺἀ἖Ἐ἞ἠ὆Ὀ὎ὐ὘Ὑ὚Ὓ὜Ὕ὞Ὗ὾ᾀ᾵ᾶ῅ῆ῔ῖ῜῝῰ῲ῵ῶ῿⁄⁅⁒⁓⁰⁲⁴⁽ⁿ₍₠₲⃫⃐℀℻ℽ⅌⅓ↄ←〈⌫⎴⎷⏏␀␧⑀⑋①⓿─☔☖☘☙♾⚀⚊✁✅✆✊✌✨✩❌❍❎❏❓❖❗❘❟❡❨❶➕➘➰➱➿⟐⟦⟰⦃⦙⧘⧜⧼⧾⬀⺀⺚⺛⻴⼀⿖⿰⿼〄〈〒〔〠〰〱〽〾぀ぁ゗゙゠ァ・ー㄀ㄅㄭㄱ㆏㆐ㆸㇰ㈝㈠㉄㉑㉼㉿㋌㋐㋿㌀㍷㍻㏞㏠㏿㐀䶶一龦ꀀ꒍꒐꓇가힤豈郞侮恵ﬀ﬇ﬓ﬘יִ﬷טּ﬽מּ﬿נּ﭂ףּ﭅צּ﮲ﯓ﴾ﵐ﶐ﶒ﷈ﷰ﷽︀︐︠︤﹢﹣﹤﹧﹩﹪ﹰ﹵ﹶ﻽＄％＋，０：＜？Ａ［＾＿｀｛｜｝～｟ｦ﾿ￂ￈ￊ￐ￒ￘ￚ￝￠￧￨￯￼￾" }
         };
        private List<SingleRange> _rangelist;
        private RegexCharClass _subtractor;
        internal const string AnyClass = "\0\x0001\0\0";
        private const int CATEGORYLENGTH = 2;
        internal static readonly string DigitClass;
        internal const string ECMADigitClass = "\0\x0002\00:";
        private const string ECMADigitSet = "0:";
        internal const string ECMASpaceClass = "\0\x0004\0\t\x000e !";
        private const string ECMASpaceSet = "\t\x000e !";
        internal const string ECMAWordClass = "\0\n\00:A[_`a{İı";
        private const string ECMAWordSet = "0:A[_`a{İı";
        internal const string EmptyClass = "\0\0\0";
        private const int FLAGS = 0;
        private const char GroupChar = '\0';
        private static readonly string InternalRegexIgnoreCase = "__InternalRegexIgnoreCase__";
        private const char Lastchar = '￿';
        private const int LowercaseAdd = 1;
        private const int LowercaseBad = 3;
        private const int LowercaseBor = 2;
        private const int LowercaseSet = 0;
        internal static readonly string NotDigitClass;
        internal const string NotECMADigitClass = "\x0001\x0002\00:";
        private const string NotECMADigitSet = "\00:";
        internal const string NotECMASpaceClass = "\x0001\x0004\0\t\x000e !";
        private const string NotECMASpaceSet = "\0\t\x000e !";
        internal const string NotECMAWordClass = "\x0001\n\00:A[_`a{İı";
        private const string NotECMAWordSet = "\00:A[_`a{İı";
        private static readonly string NotSpace = NegateCategory(Space);
        internal static readonly string NotSpaceClass;
        private const short NotSpaceConst = -100;
        private static readonly string NotWord;
        internal static readonly string NotWordClass;
        private const char Nullchar = '\0';
        private const int SETLENGTH = 1;
        private const int SETSTART = 3;
        private static readonly string Space = "d";
        internal static readonly string SpaceClass;
        private const short SpaceConst = 100;
        private static readonly string Word;
        internal static readonly string WordClass;
        private const char ZeroWidthJoiner = '‍';
        private const char ZeroWidthNonJoiner = '‌';

        static RegexCharClass()
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>(0x20);
            char[] chArray = new char[9];
            StringBuilder builder = new StringBuilder(11);
            builder.Append('\0');
            chArray[0] = '\0';
            chArray[1] = '\x000f';
            dictionary["Cc"] = chArray[1].ToString();
            chArray[2] = '\x0010';
            dictionary["Cf"] = chArray[2].ToString();
            chArray[3] = '\x001e';
            dictionary["Cn"] = chArray[3].ToString();
            chArray[4] = '\x0012';
            dictionary["Co"] = chArray[4].ToString();
            chArray[5] = '\x0011';
            dictionary["Cs"] = chArray[5].ToString();
            chArray[6] = '\0';
            dictionary["C"] = new string(chArray, 0, 7);
            chArray[1] = '\x0002';
            dictionary["Ll"] = chArray[1].ToString();
            chArray[2] = '\x0004';
            dictionary["Lm"] = chArray[2].ToString();
            chArray[3] = '\x0005';
            dictionary["Lo"] = chArray[3].ToString();
            chArray[4] = '\x0003';
            dictionary["Lt"] = chArray[4].ToString();
            chArray[5] = '\x0001';
            dictionary["Lu"] = chArray[5].ToString();
            dictionary["L"] = new string(chArray, 0, 7);
            builder.Append(new string(chArray, 1, 5));
            dictionary[InternalRegexIgnoreCase] = string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}{3}{4}", new object[] { '\0', chArray[1], chArray[4], chArray[5], chArray[6] });
            chArray[1] = '\a';
            dictionary["Mc"] = chArray[1].ToString();
            chArray[2] = '\b';
            dictionary["Me"] = chArray[2].ToString();
            chArray[3] = '\x0006';
            dictionary["Mn"] = chArray[3].ToString();
            chArray[4] = '\0';
            dictionary["M"] = new string(chArray, 0, 5);
            chArray[1] = '\t';
            dictionary["Nd"] = chArray[1].ToString();
            chArray[2] = '\n';
            dictionary["Nl"] = chArray[2].ToString();
            chArray[3] = '\v';
            dictionary["No"] = chArray[3].ToString();
            dictionary["N"] = new string(chArray, 0, 5);
            builder.Append(chArray[1]);
            chArray[1] = '\x0013';
            dictionary["Pc"] = chArray[1].ToString();
            chArray[2] = '\x0014';
            dictionary["Pd"] = chArray[2].ToString();
            chArray[3] = '\x0016';
            dictionary["Pe"] = chArray[3].ToString();
            chArray[4] = '\x0019';
            dictionary["Po"] = chArray[4].ToString();
            chArray[5] = '\x0015';
            dictionary["Ps"] = chArray[5].ToString();
            chArray[6] = '\x0018';
            dictionary["Pf"] = chArray[6].ToString();
            chArray[7] = '\x0017';
            dictionary["Pi"] = chArray[7].ToString();
            chArray[8] = '\0';
            dictionary["P"] = new string(chArray, 0, 9);
            builder.Append(chArray[1]);
            chArray[1] = '\x001b';
            dictionary["Sc"] = chArray[1].ToString();
            chArray[2] = '\x001c';
            dictionary["Sk"] = chArray[2].ToString();
            chArray[3] = '\x001a';
            dictionary["Sm"] = chArray[3].ToString();
            chArray[4] = '\x001d';
            dictionary["So"] = chArray[4].ToString();
            chArray[5] = '\0';
            dictionary["S"] = new string(chArray, 0, 6);
            chArray[1] = '\r';
            dictionary["Zl"] = chArray[1].ToString();
            chArray[2] = '\x000e';
            dictionary["Zp"] = chArray[2].ToString();
            chArray[3] = '\f';
            dictionary["Zs"] = chArray[3].ToString();
            chArray[4] = '\0';
            dictionary["Z"] = new string(chArray, 0, 5);
            builder.Append('\0');
            Word = builder.ToString();
            NotWord = NegateCategory(Word);
            SpaceClass = "\0\0\x0001" + Space;
            NotSpaceClass = "\x0001\0\x0001" + Space;
            WordClass = "\0\0" + ((char) Word.Length) + Word;
            NotWordClass = "\x0001\0" + ((char) Word.Length) + Word;
            DigitClass = "\0\0\x0001" + '\t';
            NotDigitClass = "\0\0\x0001" + ((char) 0xfff7);
            _definedCategories = dictionary;
        }

        internal RegexCharClass()
        {
            this._rangelist = new List<SingleRange>(6);
            this._canonical = true;
            this._categories = new StringBuilder();
        }

        private RegexCharClass(bool negate, List<SingleRange> ranges, StringBuilder categories, RegexCharClass subtraction)
        {
            this._rangelist = ranges;
            this._categories = categories;
            this._canonical = true;
            this._negate = negate;
            this._subtractor = subtraction;
        }

        private void AddCategory(string category)
        {
            this._categories.Append(category);
        }

        internal void AddCategoryFromName(string categoryName, bool invert, bool caseInsensitive, string pattern)
        {
            string str;
            _definedCategories.TryGetValue(categoryName, out str);
            if ((str != null) && !categoryName.Equals(InternalRegexIgnoreCase))
            {
                string category = str;
                if (caseInsensitive && ((categoryName.Equals("Ll") || categoryName.Equals("Lu")) || categoryName.Equals("Lt")))
                {
                    category = _definedCategories[InternalRegexIgnoreCase];
                }
                if (invert)
                {
                    category = NegateCategory(category);
                }
                this._categories.Append(category);
            }
            else
            {
                this.AddSet(SetFromProperty(categoryName, invert, pattern));
            }
        }

        internal void AddChar(char c)
        {
            this.AddRange(c, c);
        }

        internal void AddCharClass(RegexCharClass cc)
        {
            if (!cc._canonical)
            {
                this._canonical = false;
            }
            else if ((this._canonical && (this.RangeCount() > 0)) && ((cc.RangeCount() > 0) && (cc.GetRangeAt(0)._first <= this.GetRangeAt(this.RangeCount() - 1)._last)))
            {
                this._canonical = false;
            }
            for (int i = 0; i < cc.RangeCount(); i++)
            {
                this._rangelist.Add(cc.GetRangeAt(i));
            }
            this._categories.Append(cc._categories.ToString());
        }

        internal void AddDigit(bool ecma, bool negate, string pattern)
        {
            if (ecma)
            {
                if (negate)
                {
                    this.AddSet("\00:");
                }
                else
                {
                    this.AddSet("0:");
                }
            }
            else
            {
                this.AddCategoryFromName("Nd", negate, false, pattern);
            }
        }

        internal void AddLowercase(CultureInfo culture)
        {
            this._canonical = false;
            int num = 0;
            int count = this._rangelist.Count;
            while (num < count)
            {
                SingleRange range = this._rangelist[num];
                if (range._first == range._last)
                {
                    range._first = range._last = char.ToLower(range._first, culture);
                }
                else
                {
                    this.AddLowercaseRange(range._first, range._last, culture);
                }
                num++;
            }
        }

        private void AddLowercaseRange(char chMin, char chMax, CultureInfo culture)
        {
            int index = 0;
            int length = _lcTable.Length;
            while (index < length)
            {
                int num3 = (index + length) / 2;
                if (_lcTable[num3]._chMax < chMin)
                {
                    index = num3 + 1;
                }
                else
                {
                    length = num3;
                }
            }
            if (index < _lcTable.Length)
            {
                LowerCaseMapping mapping;
                while ((index < _lcTable.Length) && ((mapping = _lcTable[index])._chMin <= chMax))
                {
                    char ch;
                    char ch2;
                    if ((ch = mapping._chMin) < chMin)
                    {
                        ch = chMin;
                    }
                    if ((ch2 = mapping._chMax) > chMax)
                    {
                        ch2 = chMax;
                    }
                    switch (mapping._lcOp)
                    {
                        case 0:
                            ch = (char) mapping._data;
                            ch2 = (char) mapping._data;
                            break;

                        case 1:
                            ch = (char) (ch + ((char) mapping._data));
                            ch2 = (char) (ch2 + ((char) mapping._data));
                            break;

                        case 2:
                            ch = (char) (ch | '\x0001');
                            ch2 = (char) (ch2 | '\x0001');
                            break;

                        case 3:
                            ch = (char) (ch + ((char) (ch & '\x0001')));
                            ch2 = (char) (ch2 + ((char) (ch2 & '\x0001')));
                            break;
                    }
                    if ((ch < chMin) || (ch2 > chMax))
                    {
                        this.AddRange(ch, ch2);
                    }
                    index++;
                }
            }
        }

        internal void AddRange(char first, char last)
        {
            this._rangelist.Add(new SingleRange(first, last));
            if ((this._canonical && (this._rangelist.Count > 0)) && (first <= this._rangelist[this._rangelist.Count - 1]._last))
            {
                this._canonical = false;
            }
        }

        private void AddSet(string set)
        {
            if ((this._canonical && (this.RangeCount() > 0)) && ((set.Length > 0) && (set[0] <= this.GetRangeAt(this.RangeCount() - 1)._last)))
            {
                this._canonical = false;
            }
            int num = 0;
            while (num < (set.Length - 1))
            {
                this._rangelist.Add(new SingleRange(set[num], (char) (set[num + 1] - '\x0001')));
                num += 2;
            }
            if (num < set.Length)
            {
                this._rangelist.Add(new SingleRange(set[num], 0xffff));
            }
        }

        internal void AddSpace(bool ecma, bool negate)
        {
            if (negate)
            {
                if (ecma)
                {
                    this.AddSet("\0\t\x000e !");
                }
                else
                {
                    this.AddCategory(NotSpace);
                }
            }
            else if (ecma)
            {
                this.AddSet("\t\x000e !");
            }
            else
            {
                this.AddCategory(Space);
            }
        }

        internal void AddSubtraction(RegexCharClass sub)
        {
            this._subtractor = sub;
        }

        internal void AddWord(bool ecma, bool negate)
        {
            if (negate)
            {
                if (ecma)
                {
                    this.AddSet("\00:A[_`a{İı");
                }
                else
                {
                    this.AddCategory(NotWord);
                }
            }
            else if (ecma)
            {
                this.AddSet("0:A[_`a{İı");
            }
            else
            {
                this.AddCategory(Word);
            }
        }

        private void Canonicalize()
        {
            char ch;
            this._canonical = true;
            this._rangelist.Sort(0, this._rangelist.Count, new SingleRangeComparer());
            if (this._rangelist.Count <= 1)
            {
                return;
            }
            bool flag = false;
            int num = 1;
            int index = 0;
        Label_003B:
            ch = this._rangelist[index]._last;
        Label_004D:
            if ((num == this._rangelist.Count) || (ch == 0xffff))
            {
                flag = true;
            }
            else
            {
                SingleRange range;
                if ((range = this._rangelist[num])._first <= (ch + '\x0001'))
                {
                    if (ch < range._last)
                    {
                        ch = range._last;
                    }
                    num++;
                    goto Label_004D;
                }
            }
            this._rangelist[index]._last = ch;
            index++;
            if (!flag)
            {
                if (index < num)
                {
                    this._rangelist[index] = this._rangelist[num];
                }
                num++;
                goto Label_003B;
            }
            this._rangelist.RemoveRange(index, this._rangelist.Count - index);
        }

        private static bool CharInCategory(char ch, string set, int start, int mySetLength, int myCategoryLength)
        {
            UnicodeCategory unicodeCategory = char.GetUnicodeCategory(ch);
            int i = (start + 3) + mySetLength;
            int num2 = i + myCategoryLength;
            while (i < num2)
            {
                int num3 = (short) set[i];
                if (num3 == 0)
                {
                    if (CharInCategoryGroup(ch, unicodeCategory, set, ref i))
                    {
                        return true;
                    }
                }
                else
                {
                    if (num3 > 0)
                    {
                        if (num3 != 100)
                        {
                            num3--;
                            if (unicodeCategory == num3)
                            {
                                return true;
                            }
                            goto Label_0070;
                        }
                        if (char.IsWhiteSpace(ch))
                        {
                            return true;
                        }
                        i++;
                        continue;
                    }
                    if (num3 == -100)
                    {
                        if (!char.IsWhiteSpace(ch))
                        {
                            return true;
                        }
                        i++;
                        continue;
                    }
                    num3 = -1 - num3;
                    if (unicodeCategory != num3)
                    {
                        return true;
                    }
                }
            Label_0070:
                i++;
            }
            return false;
        }

        private static bool CharInCategoryGroup(char ch, UnicodeCategory chcategory, string category, ref int i)
        {
            i++;
            int num = (short) category[i];
            if (num > 0)
            {
                bool flag = false;
                while (num != 0)
                {
                    if (!flag)
                    {
                        num--;
                        if (chcategory == num)
                        {
                            flag = true;
                        }
                    }
                    i++;
                    num = (short) category[i];
                }
                return flag;
            }
            bool flag2 = true;
            while (num != 0)
            {
                if (flag2)
                {
                    num = -1 - num;
                    if (chcategory == num)
                    {
                        flag2 = false;
                    }
                }
                i++;
                num = (short) category[i];
            }
            return flag2;
        }

        internal static bool CharInClass(char ch, string set)
        {
            return CharInClassRecursive(ch, set, 0);
        }

        private static bool CharInClassInternal(char ch, string set, int start, int mySetLength, int myCategoryLength)
        {
            int num = start + 3;
            int num2 = num + mySetLength;
            while (num != num2)
            {
                int num3 = (num + num2) / 2;
                if (ch < set[num3])
                {
                    num2 = num3;
                }
                else
                {
                    num = num3 + 1;
                }
            }
            if ((num & 1) == (start & 1))
            {
                return true;
            }
            if (myCategoryLength == 0)
            {
                return false;
            }
            return CharInCategory(ch, set, start, mySetLength, myCategoryLength);
        }

        internal static bool CharInClassRecursive(char ch, string set, int start)
        {
            int mySetLength = set[start + 1];
            int myCategoryLength = set[start + 2];
            int num3 = ((start + 3) + mySetLength) + myCategoryLength;
            bool flag = false;
            if (set.Length > num3)
            {
                flag = CharInClassRecursive(ch, set, num3);
            }
            bool flag2 = CharInClassInternal(ch, set, start, mySetLength, myCategoryLength);
            if (set[start] == '\x0001')
            {
                flag2 = !flag2;
            }
            return (flag2 && !flag);
        }

        internal static string ConvertOldStringsToClass(string set, string category)
        {
            StringBuilder builder = new StringBuilder((set.Length + category.Length) + 3);
            if (((set.Length >= 2) && (set[0] == '\0')) && (set[1] == '\0'))
            {
                builder.Append('\x0001');
                builder.Append((char) (set.Length - 2));
                builder.Append((char) category.Length);
                builder.Append(set.Substring(2));
            }
            else
            {
                builder.Append('\0');
                builder.Append((char) set.Length);
                builder.Append((char) category.Length);
                builder.Append(set);
            }
            builder.Append(category);
            return builder.ToString();
        }

        private SingleRange GetRangeAt(int i)
        {
            return this._rangelist[i];
        }

        internal static bool IsECMAWordChar(char ch)
        {
            return CharInClass(ch, "\0\n\00:A[_`a{İı");
        }

        internal static bool IsEmpty(string charClass)
        {
            return (((charClass[2] == '\0') && (charClass[0] == '\0')) && ((charClass[1] == '\0') && !IsSubtraction(charClass)));
        }

        internal static bool IsMergeable(string charClass)
        {
            return (!IsNegated(charClass) && !IsSubtraction(charClass));
        }

        internal static bool IsNegated(string set)
        {
            return ((set != null) && (set[0] == '\x0001'));
        }

        internal static bool IsSingleton(string set)
        {
            if ((((set[0] != '\0') || (set[2] != '\0')) || ((set[1] != '\x0002') || IsSubtraction(set))) || ((set[3] != 0xffff) && ((set[3] + '\x0001') != set[4])))
            {
                return false;
            }
            return true;
        }

        internal static bool IsSingletonInverse(string set)
        {
            if ((((set[0] != '\x0001') || (set[2] != '\0')) || ((set[1] != '\x0002') || IsSubtraction(set))) || ((set[3] != 0xffff) && ((set[3] + '\x0001') != set[4])))
            {
                return false;
            }
            return true;
        }

        private static bool IsSubtraction(string charClass)
        {
            return (charClass.Length > (('\x0003' + charClass[1]) + charClass[2]));
        }

        internal static bool IsWordChar(char ch)
        {
            if (!CharInClass(ch, WordClass) && (ch != '‍'))
            {
                return (ch == '‌');
            }
            return true;
        }

        private static string NegateCategory(string category)
        {
            if (category == null)
            {
                return null;
            }
            StringBuilder builder = new StringBuilder(category.Length);
            for (int i = 0; i < category.Length; i++)
            {
                short num2 = (short) category[i];
                builder.Append((char) ((ushort) -num2));
            }
            return builder.ToString();
        }

        internal static RegexCharClass Parse(string charClass)
        {
            return ParseRecursive(charClass, 0);
        }

        private static RegexCharClass ParseRecursive(string charClass, int start)
        {
            int capacity = charClass[start + 1];
            int length = charClass[start + 2];
            int num3 = ((start + 3) + capacity) + length;
            List<SingleRange> ranges = new List<SingleRange>(capacity);
            int num4 = start + 3;
            int startIndex = num4 + capacity;
            while (num4 < startIndex)
            {
                char ch2;
                char first = charClass[num4];
                num4++;
                if (num4 < startIndex)
                {
                    ch2 = (char) (charClass[num4] - '\x0001');
                }
                else
                {
                    ch2 = 0xffff;
                }
                num4++;
                ranges.Add(new SingleRange(first, ch2));
            }
            RegexCharClass subtraction = null;
            if (charClass.Length > num3)
            {
                subtraction = ParseRecursive(charClass, num3);
            }
            return new RegexCharClass(charClass[start] == '\x0001', ranges, new StringBuilder(charClass.Substring(startIndex, length)), subtraction);
        }

        private int RangeCount()
        {
            return this._rangelist.Count;
        }

        private static string SetFromProperty(string capname, bool invert, string pattern)
        {
            int num = 0;
            int length = _propTable.GetLength(0);
            while (num != length)
            {
                int num3 = (num + length) / 2;
                int num4 = string.Compare(capname, _propTable[num3, 0], StringComparison.Ordinal);
                if (num4 < 0)
                {
                    length = num3;
                }
                else
                {
                    if (num4 > 0)
                    {
                        num = num3 + 1;
                        continue;
                    }
                    string str = _propTable[num3, 1];
                    if (!invert)
                    {
                        return str;
                    }
                    if (str[0] == '\0')
                    {
                        return str.Substring(1);
                    }
                    return ('\0' + str);
                }
            }
            throw new ArgumentException(SR.GetString("MakeException", new object[] { pattern, SR.GetString("UnknownProperty", new object[] { capname }) }));
        }

        internal static char SingletonChar(string set)
        {
            return set[3];
        }

        internal string ToStringClass()
        {
            int num2;
            if (!this._canonical)
            {
                this.Canonicalize();
            }
            int num = this._rangelist.Count * 2;
            StringBuilder builder = new StringBuilder((num + this._categories.Length) + 3);
            if (this._negate)
            {
                num2 = 1;
            }
            else
            {
                num2 = 0;
            }
            builder.Append((char) num2);
            builder.Append((char) num);
            builder.Append((char) this._categories.Length);
            for (int i = 0; i < this._rangelist.Count; i++)
            {
                SingleRange range = this._rangelist[i];
                builder.Append(range._first);
                if (range._last != 0xffff)
                {
                    builder.Append((char) (range._last + '\x0001'));
                }
            }
            builder[1] = (char) (builder.Length - 3);
            builder.Append(this._categories);
            if (this._subtractor != null)
            {
                builder.Append(this._subtractor.ToStringClass());
            }
            return builder.ToString();
        }

        internal bool CanMerge
        {
            get
            {
                return (!this._negate && (this._subtractor == null));
            }
        }

        internal bool Negate
        {
            set
            {
                this._negate = value;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct LowerCaseMapping
        {
            internal char _chMin;
            internal char _chMax;
            internal int _lcOp;
            internal int _data;
            internal LowerCaseMapping(char chMin, char chMax, int lcOp, int data)
            {
                this._chMin = chMin;
                this._chMax = chMax;
                this._lcOp = lcOp;
                this._data = data;
            }
        }

        private sealed class SingleRange
        {
            internal char _first;
            internal char _last;

            internal SingleRange(char first, char last)
            {
                this._first = first;
                this._last = last;
            }
        }

        private sealed class SingleRangeComparer : IComparer<RegexCharClass.SingleRange>
        {
            public int Compare(RegexCharClass.SingleRange x, RegexCharClass.SingleRange y)
            {
                if (x._first < y._first)
                {
                    return -1;
                }
                if (x._first <= y._first)
                {
                    return 0;
                }
                return 1;
            }
        }
    }
}

