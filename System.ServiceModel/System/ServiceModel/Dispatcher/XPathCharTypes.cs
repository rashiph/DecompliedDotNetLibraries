namespace System.ServiceModel.Dispatcher
{
    using System;

    internal static class XPathCharTypes
    {
        private const string BaseChars = "AZaz\x00c0\x00d6\x00d8\x00f6\x00f8\x00ffĀıĴľŁňŊžƀǃǍǰǴǵǺȗɐʨʻˁΆΆΈΊΌΌΎΡΣώϐϖϚϚϜϜϞϞϠϠϢϳЁЌЎяёќўҁҐӄӇӈӋӌӐӫӮӵӸӹԱՖՙՙաֆאתװײءغفيٱڷںھۀێېۓەەۥۦअहऽऽक़ॡঅঌএঐওনপরললশহড়ঢ়য়ৡৰৱਅਊਏਐਓਨਪਰਲਲ਼ਵਸ਼ਸਹਖ਼ੜਫ਼ਫ਼ੲੴઅઋઍઍએઑઓનપરલળવહઽઽૠૠଅଌଏଐଓନପରଲଳଶହଽଽଡ଼ଢ଼ୟୡஅஊஎஐஒகஙசஜஜஞடணதநபமவஷஹఅఌఎఐఒనపళవహౠౡಅಌಎಐಒನಪಳವಹೞೞೠೡഅഌഎഐഒനപഹൠൡกฮะะาำเๅກຂຄຄງຈຊຊຍຍດທນຟມຣລລວວສຫອຮະະາຳຽຽເໄཀཇཉཀྵႠჅაჶᄀᄀᄂᄃᄅᄇᄉᄉᄋᄌᄎᄒᄼᄼᄾᄾᅀᅀᅌᅌᅎᅎᅐᅐᅔᅕᅙᅙᅟᅡᅣᅣᅥᅥᅧᅧᅩᅩᅭᅮᅲᅳᅵᅵᆞᆞᆨᆨᆫᆫᆮᆯᆷᆸᆺᆺᆼᇂᇫᇫᇰᇰᇹᇹḀẛẠỹἀἕἘἝἠὅὈὍὐὗὙὙὛὛὝὝὟώᾀᾴᾶᾼιιῂῄῆῌῐΐῖΊῠῬῲῴῶῼΩΩKÅ℮℮ↀↂぁゔァヺㄅㄬ가힣";
        private static byte[] charProperties;
        private const byte Combining = 2;
        private const string CombiningChars = "ֹֻֽֿֿׁׂًْٰٰ֑֣̀҃҆֡ׄׄۖۜ͠͡ͅ۝۪ۭ۟۠ۤۧۨँः़़ाौ््॑॔ॢॣঁঃ়়াািিীৄেৈো্ৗৗৢৣਂਂ਼਼ਾਾਿਿੀੂੇੈੋ੍ੰੱઁઃ઼઼ાૅેૉો્ଁଃ଼଼ାୃେୈୋ୍ୖୗஂஃாூெைொ்ௗௗఁఃాౄెైొ్ౕౖಂಃಾೄೆೈೊ್ೕೖംഃാൃെൈൊ്ൗൗััิฺ็๎ັັິູົຼ່ໍ༹༹༘༙༵༵༷༷༾༾༿༿྄ཱ྆ྋྐྕྗྗྙྭྱྷྐྵྐྵ゙゙゚゚〪〯⃐⃜⃡⃡";
        private const byte Digit = 4;
        private const string DigitChars = "09٠٩۰۹०९০৯੦੯૦૯୦୯௧௯౦౯೦೯൦൯๐๙໐໙༠༩";
        private const byte Extender = 8;
        private const string ExtenderChars = "\x00b7\x00b7ːːˑˑ··ــๆๆໆໆ々々〱〵ゝゞーヾ";
        private const string IdeogramicChars = "一龥〇〇〡〩";
        private const byte Letter = 1;
        private const byte NCName = 0x20;
        private const byte NCNameStart = 0x40;
        private const byte None = 0;
        private const string OtherNCNameChars = "..--__";
        private const string OtherNCNameStartChars = "__";
        private const byte Whitespace = 0x10;
        private const string WhitespaceChars = "  \t\t\r\r\n\n";

        static XPathCharTypes()
        {
            if (charProperties == null)
            {
                charProperties = new byte[0xffff];
                SetProperties("AZaz\x00c0\x00d6\x00d8\x00f6\x00f8\x00ffĀıĴľŁňŊžƀǃǍǰǴǵǺȗɐʨʻˁΆΆΈΊΌΌΎΡΣώϐϖϚϚϜϜϞϞϠϠϢϳЁЌЎяёќўҁҐӄӇӈӋӌӐӫӮӵӸӹԱՖՙՙաֆאתװײءغفيٱڷںھۀێېۓەەۥۦअहऽऽक़ॡঅঌএঐওনপরললশহড়ঢ়য়ৡৰৱਅਊਏਐਓਨਪਰਲਲ਼ਵਸ਼ਸਹਖ਼ੜਫ਼ਫ਼ੲੴઅઋઍઍએઑઓનપરલળવહઽઽૠૠଅଌଏଐଓନପରଲଳଶହଽଽଡ଼ଢ଼ୟୡஅஊஎஐஒகஙசஜஜஞடணதநபமவஷஹఅఌఎఐఒనపళవహౠౡಅಌಎಐಒನಪಳವಹೞೞೠೡഅഌഎഐഒനപഹൠൡกฮะะาำเๅກຂຄຄງຈຊຊຍຍດທນຟມຣລລວວສຫອຮະະາຳຽຽເໄཀཇཉཀྵႠჅაჶᄀᄀᄂᄃᄅᄇᄉᄉᄋᄌᄎᄒᄼᄼᄾᄾᅀᅀᅌᅌᅎᅎᅐᅐᅔᅕᅙᅙᅟᅡᅣᅣᅥᅥᅧᅧᅩᅩᅭᅮᅲᅳᅵᅵᆞᆞᆨᆨᆫᆫᆮᆯᆷᆸᆺᆺᆼᇂᇫᇫᇰᇰᇹᇹḀẛẠỹἀἕἘἝἠὅὈὍὐὗὙὙὛὛὝὝὟώᾀᾴᾶᾼιιῂῄῆῌῐΐῖΊῠῬῲῴῶῼΩΩKÅ℮℮ↀↂぁゔァヺㄅㄬ가힣", 1);
                SetProperties("一龥〇〇〡〩", 1);
                SetProperties("ֹֻֽֿֿׁׂًْٰٰ֑֣̀҃҆֡ׄׄۖۜ͠͡ͅ۝۪ۭ۟۠ۤۧۨँः़़ाौ््॑॔ॢॣঁঃ়়াািিীৄেৈো্ৗৗৢৣਂਂ਼਼ਾਾਿਿੀੂੇੈੋ੍ੰੱઁઃ઼઼ાૅેૉો્ଁଃ଼଼ାୃେୈୋ୍ୖୗஂஃாூெைொ்ௗௗఁఃాౄెైొ్ౕౖಂಃಾೄೆೈೊ್ೕೖംഃാൃെൈൊ്ൗൗััิฺ็๎ັັິູົຼ່ໍ༹༹༘༙༵༵༷༷༾༾༿༿྄ཱ྆ྋྐྕྗྗྙྭྱྷྐྵྐྵ゙゙゚゚〪〯⃐⃜⃡⃡", 2);
                SetProperties("09٠٩۰۹०९০৯੦੯૦૯୦୯௧௯౦౯೦೯൦൯๐๙໐໙༠༩", 4);
                SetProperties("\x00b7\x00b7ːːˑˑ··ــๆๆໆໆ々々〱〵ゝゞーヾ", 8);
                SetProperties("  \t\t\r\r\n\n", 0x10);
                SetProperties("AZaz\x00c0\x00d6\x00d8\x00f6\x00f8\x00ffĀıĴľŁňŊžƀǃǍǰǴǵǺȗɐʨʻˁΆΆΈΊΌΌΎΡΣώϐϖϚϚϜϜϞϞϠϠϢϳЁЌЎяёќўҁҐӄӇӈӋӌӐӫӮӵӸӹԱՖՙՙաֆאתװײءغفيٱڷںھۀێېۓەەۥۦअहऽऽक़ॡঅঌএঐওনপরললশহড়ঢ়য়ৡৰৱਅਊਏਐਓਨਪਰਲਲ਼ਵਸ਼ਸਹਖ਼ੜਫ਼ਫ਼ੲੴઅઋઍઍએઑઓનપરલળવહઽઽૠૠଅଌଏଐଓନପରଲଳଶହଽଽଡ଼ଢ଼ୟୡஅஊஎஐஒகஙசஜஜஞடணதநபமவஷஹఅఌఎఐఒనపళవహౠౡಅಌಎಐಒನಪಳವಹೞೞೠೡഅഌഎഐഒനപഹൠൡกฮะะาำเๅກຂຄຄງຈຊຊຍຍດທນຟມຣລລວວສຫອຮະະາຳຽຽເໄཀཇཉཀྵႠჅაჶᄀᄀᄂᄃᄅᄇᄉᄉᄋᄌᄎᄒᄼᄼᄾᄾᅀᅀᅌᅌᅎᅎᅐᅐᅔᅕᅙᅙᅟᅡᅣᅣᅥᅥᅧᅧᅩᅩᅭᅮᅲᅳᅵᅵᆞᆞᆨᆨᆫᆫᆮᆯᆷᆸᆺᆺᆼᇂᇫᇫᇰᇰᇹᇹḀẛẠỹἀἕἘἝἠὅὈὍὐὗὙὙὛὛὝὝὟώᾀᾴᾶᾼιιῂῄῆῌῐΐῖΊῠῬῲῴῶῼΩΩKÅ℮℮ↀↂぁゔァヺㄅㄬ가힣", 0x40);
                SetProperties("一龥〇〇〡〩", 0x40);
                SetProperties("__", 0x40);
                SetProperties("AZaz\x00c0\x00d6\x00d8\x00f6\x00f8\x00ffĀıĴľŁňŊžƀǃǍǰǴǵǺȗɐʨʻˁΆΆΈΊΌΌΎΡΣώϐϖϚϚϜϜϞϞϠϠϢϳЁЌЎяёќўҁҐӄӇӈӋӌӐӫӮӵӸӹԱՖՙՙաֆאתװײءغفيٱڷںھۀێېۓەەۥۦअहऽऽक़ॡঅঌএঐওনপরললশহড়ঢ়য়ৡৰৱਅਊਏਐਓਨਪਰਲਲ਼ਵਸ਼ਸਹਖ਼ੜਫ਼ਫ਼ੲੴઅઋઍઍએઑઓનપરલળવહઽઽૠૠଅଌଏଐଓନପରଲଳଶହଽଽଡ଼ଢ଼ୟୡஅஊஎஐஒகஙசஜஜஞடணதநபமவஷஹఅఌఎఐఒనపళవహౠౡಅಌಎಐಒನಪಳವಹೞೞೠೡഅഌഎഐഒനപഹൠൡกฮะะาำเๅກຂຄຄງຈຊຊຍຍດທນຟມຣລລວວສຫອຮະະາຳຽຽເໄཀཇཉཀྵႠჅაჶᄀᄀᄂᄃᄅᄇᄉᄉᄋᄌᄎᄒᄼᄼᄾᄾᅀᅀᅌᅌᅎᅎᅐᅐᅔᅕᅙᅙᅟᅡᅣᅣᅥᅥᅧᅧᅩᅩᅭᅮᅲᅳᅵᅵᆞᆞᆨᆨᆫᆫᆮᆯᆷᆸᆺᆺᆼᇂᇫᇫᇰᇰᇹᇹḀẛẠỹἀἕἘἝἠὅὈὍὐὗὙὙὛὛὝὝὟώᾀᾴᾶᾼιιῂῄῆῌῐΐῖΊῠῬῲῴῶῼΩΩKÅ℮℮ↀↂぁゔァヺㄅㄬ가힣", 0x20);
                SetProperties("一龥〇〇〡〩", 0x20);
                SetProperties("09٠٩۰۹०९০৯੦੯૦૯୦୯௧௯౦౯೦೯൦൯๐๙໐໙༠༩", 0x20);
                SetProperties("ֹֻֽֿֿׁׂًْٰٰ֑֣̀҃҆֡ׄׄۖۜ͠͡ͅ۝۪ۭ۟۠ۤۧۨँः़़ाौ््॑॔ॢॣঁঃ়়াািিীৄেৈো্ৗৗৢৣਂਂ਼਼ਾਾਿਿੀੂੇੈੋ੍ੰੱઁઃ઼઼ાૅેૉો્ଁଃ଼଼ାୃେୈୋ୍ୖୗஂஃாூெைொ்ௗௗఁఃాౄెైొ్ౕౖಂಃಾೄೆೈೊ್ೕೖംഃാൃെൈൊ്ൗൗััิฺ็๎ັັິູົຼ່ໍ༹༹༘༙༵༵༷༷༾༾༿༿྄ཱ྆ྋྐྕྗྗྙྭྱྷྐྵྐྵ゙゙゚゚〪〯⃐⃜⃡⃡", 0x20);
                SetProperties("\x00b7\x00b7ːːˑˑ··ــๆๆໆໆ々々〱〵ゝゞーヾ", 0x20);
                SetProperties("..--__", 0x20);
            }
        }

        private static byte GetCode(char c)
        {
            return charProperties[c];
        }

        internal static bool IsDigit(char c)
        {
            return ((GetCode(c) & 4) != 0);
        }

        internal static bool IsNCName(char c)
        {
            return ((GetCode(c) & 0x20) != 0);
        }

        internal static bool IsNCNameStart(char c)
        {
            return ((GetCode(c) & 0x40) != 0);
        }

        internal static bool IsWhitespace(char c)
        {
            return ((GetCode(c) & 0x10) != 0);
        }

        private static void SetProperties(string ranges, byte value)
        {
            for (int i = 0; i < ranges.Length; i += 2)
            {
                int index = ranges[i];
                int num3 = ranges[i + 1];
                while (index <= num3)
                {
                    charProperties[index] = (byte) (charProperties[index] | value);
                    index++;
                }
            }
        }
    }
}

