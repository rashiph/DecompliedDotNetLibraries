namespace System.Text
{
    using System;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable]
    internal class ISCIIEncoding : EncodingNLS, ISerializable
    {
        private const int CodeAssamese = 6;
        private const int CodeBengali = 3;
        private const int CodeDefault = 0;
        private const int CodeDevanagari = 2;
        private const int CodeGujarati = 10;
        private const int CodeKannada = 8;
        private const int CodeMalayalam = 9;
        private const int CodeOriya = 7;
        private const int CodePunjabi = 11;
        private const int CodeRoman = 1;
        private const int CodeTamil = 4;
        private const int CodeTelugu = 5;
        private const byte ControlATR = 0xef;
        private const byte ControlCodePageStart = 0x40;
        private int defaultCodePage;
        private const byte DevenagariExt = 240;
        private const int IndicBegin = 0x901;
        private const int IndicEnd = 0xd6f;
        private static char[,,] IndicMapping = new char[,,] { { { 
            '\0', 'ँ', 'ं', 'ः', 'अ', 'आ', 'इ', 'ई', 'उ', 'ऊ', 'ऋ', 'ऎ', 'ए', 'ऐ', 'ऍ', 'ऒ', 
            'ओ', 'औ', 'ऑ', 'क', 'ख', 'ग', 'घ', 'ङ', 'च', 'छ', 'ज', 'झ', 'ञ', 'ट', 'ठ', 'ड', 
            'ढ', 'ण', 'त', 'थ', 'द', 'ध', 'न', 'ऩ', 'प', 'फ', 'ब', 'भ', 'म', 'य', 'य़', 'र', 
            'ऱ', 'ल', 'ळ', 'ऴ', 'व', 'श', 'ष', 'स', 'ह', '\0', 'ा', 'ि', 'ी', 'ु', 'ू', 'ृ', 
            'ॆ', 'े', 'ै', 'ॅ', 'ॊ', 'ो', 'ौ', 'ॉ', '्', '़', '।', '\0', '\0', '\0', '\0', '\0', 
            '\0', '०', '१', '२', '३', '४', '५', '६', '७', '८', '९', '\0', '\0', '\0', '\0', '\0'
         }, { 
            '\0', 'ॐ', '\0', '\0', '\0', '\0', 'ऌ', 'ॡ', '\0', '\0', 'ॠ', '\0', '\0', '\0', '\0', '\0', 
            '\0', '\0', '\0', 'क़', 'ख़', 'ग़', '\0', '\0', '\0', '\0', 'ज़', '\0', '\0', '\0', '\0', 'ड़', 
            'ढ़', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', 'फ़', '\0', '\0', '\0', '\0', '\0', '\0', 
            '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', 'ॢ', 'ॣ', '\0', '\0', 'ॄ', 
            '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '‌', '‍', 'ऽ', '\0', '\0', '\0', '\0', '\0', 
            '뢿', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0'
         } }, { { 
            '\0', 'ঁ', 'ং', 'ঃ', 'অ', 'আ', 'ই', 'ঈ', 'উ', 'ঊ', 'ঋ', 'এ', 'এ', 'ঐ', 'ঐ', 'ও', 
            'ও', 'ঔ', 'ঔ', 'ক', 'খ', 'গ', 'ঘ', 'ঙ', 'চ', 'ছ', 'জ', 'ঝ', 'ঞ', 'ট', 'ঠ', 'ড', 
            'ঢ', 'ণ', 'ত', 'থ', 'দ', 'ধ', 'ন', 'ন', 'প', 'ফ', 'ব', 'ভ', 'ম', 'য', 'য়', 'র', 
            'র', 'ল', 'ল', 'ল', 'ব', 'শ', 'ষ', 'স', 'হ', '\0', 'া', 'ি', 'ী', 'ু', 'ূ', 'ৃ', 
            'ে', 'ে', 'ৈ', 'ৈ', 'ো', 'ো', 'ৌ', 'ৌ', '্', '়', '.', '\0', '\0', '\0', '\0', '\0', 
            '\0', '০', '১', '২', '৩', '৪', '৫', '৬', '৭', '৮', '৯', '\0', '\0', '\0', '\0', '\0'
         }, { 
            '\0', '\0', '\0', '\0', '\0', '\0', 'ঌ', 'ৡ', '\0', '\0', 'ৠ', '\0', '\0', '\0', '\0', '\0', 
            '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', 'ড়', 
            'ঢ়', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', 
            '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', 'ৢ', 'ৣ', '\0', '\0', 'ৄ', 
            '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '‌', '‍', '\0', '\0', '\0', '\0', '\0', '\0', 
            '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0'
         } }, { { 
            '\0', '\0', 'ஂ', 'ஃ', 'அ', 'ஆ', 'இ', 'ஈ', 'உ', 'ஊ', '\0', 'ஏ', 'ஏ', 'ஐ', 'ஐ', 'ஒ', 
            'ஓ', 'ஔ', 'ஔ', 'க', 'க', 'க', 'க', 'ங', 'ச', 'ச', 'ஜ', 'ஜ', 'ஞ', 'ட', 'ட', 'ட', 
            'ட', 'ண', 'த', 'த', 'த', 'த', 'ந', 'ன', 'ப', 'ப', 'ப', 'ப', 'ம', 'ய', 'ய', 'ர', 
            'ற', 'ல', 'ள', 'ழ', 'வ', 'ஷ', 'ஷ', 'ஸ', 'ஹ', '\0', 'ா', 'ி', 'ீ', 'ு', 'ூ', '\0', 
            'ெ', 'ே', 'ை', 'ை', 'ொ', 'ோ', 'ௌ', 'ௌ', '்', '\0', '.', '\0', '\0', '\0', '\0', '\0', 
            '\0', '0', '௧', '௨', '௩', '௪', '௫', '௬', '௭', '௮', '௯', '\0', '\0', '\0', '\0', '\0'
         }, { 
            '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', 
            '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', 
            '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', 
            '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', 
            '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '‌', '‍', '\0', '\0', '\0', '\0', '\0', '\0', 
            '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0'
         } }, { { 
            '\0', 'ఁ', 'ం', 'ః', 'అ', 'ఆ', 'ఇ', 'ఈ', 'ఉ', 'ఊ', 'ఋ', 'ఎ', 'ఏ', 'ఐ', 'ఐ', 'ఒ', 
            'ఓ', 'ఔ', 'ఔ', 'క', 'ఖ', 'గ', 'ఘ', 'ఙ', 'చ', 'ఛ', 'జ', 'ఝ', 'ఞ', 'ట', 'ఠ', 'డ', 
            'ఢ', 'ణ', 'త', 'థ', 'ద', 'ధ', 'న', 'న', 'ప', 'ఫ', 'బ', 'భ', 'మ', 'య', 'య', 'ర', 
            'ఱ', 'ల', 'ళ', 'ళ', 'వ', 'శ', 'ష', 'స', 'హ', '\0', 'ా', 'ి', 'ీ', 'ు', 'ూ', 'ృ', 
            'ె', 'ే', 'ై', 'ై', 'ొ', 'ో', 'ౌ', 'ౌ', '్', '\0', '.', '\0', '\0', '\0', '\0', '\0', 
            '\0', '౦', '౧', '౨', '౩', '౪', '౫', '౬', '౭', '౮', '౯', '\0', '\0', '\0', '\0', '\0'
         }, { 
            '\0', '\0', '\0', '\0', '\0', '\0', 'ఌ', 'ౡ', '\0', '\0', 'ౠ', '\0', '\0', '\0', '\0', '\0', 
            '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', 
            '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', 
            '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', 'ౄ', 
            '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '‌', '‍', '\0', '\0', '\0', '\0', '\0', '\0', 
            '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0'
         } }, { { 
            '\0', 'ଁ', 'ଂ', 'ଃ', 'ଅ', 'ଆ', 'ଇ', 'ଈ', 'ଉ', 'ଊ', 'ଋ', 'ଏ', 'ଏ', 'ଐ', 'ଐ', 'ଐ', 
            'ଓ', 'ଔ', 'ଔ', 'କ', 'ଖ', 'ଗ', 'ଘ', 'ଙ', 'ଚ', 'ଛ', 'ଜ', 'ଝ', 'ଞ', 'ଟ', 'ଠ', 'ଡ', 
            'ଢ', 'ଣ', 'ତ', 'ଥ', 'ଦ', 'ଧ', 'ନ', 'ନ', 'ପ', 'ଫ', 'ବ', 'ଭ', 'ମ', 'ଯ', 'ୟ', 'ର', 
            'ର', 'ଲ', 'ଳ', 'ଳ', 'ବ', 'ଶ', 'ଷ', 'ସ', 'ହ', '\0', 'ା', 'ି', 'ୀ', 'ୁ', 'ୂ', 'ୃ', 
            'େ', 'େ', 'ୈ', 'ୈ', 'ୋ', 'ୋ', 'ୌ', 'ୌ', '୍', '଼', '.', '\0', '\0', '\0', '\0', '\0', 
            '\0', '୦', '୧', '୨', '୩', '୪', '୫', '୬', '୭', '୮', '୯', '\0', '\0', '\0', '\0', '\0'
         }, { 
            '\0', '\0', '\0', '\0', '\0', '\0', 'ఌ', 'ౡ', '\0', '\0', 'ౠ', '\0', '\0', '\0', '\0', '\0', 
            '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', 'ଡ଼', 
            'ଢ଼', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', 
            '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', 'ౄ', 
            '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '‌', '‍', 'ଽ', '\0', '\0', '\0', '\0', '\0', 
            '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0'
         } }, { { 
            '\0', '\0', 'ಂ', 'ಃ', 'ಅ', 'ಆ', 'ಇ', 'ಈ', 'ಉ', 'ಊ', 'ಋ', 'ಎ', 'ಏ', 'ಐ', 'ಐ', 'ಒ', 
            'ಓ', 'ಔ', 'ಔ', 'ಕ', 'ಖ', 'ಗ', 'ಘ', 'ಙ', 'ಚ', 'ಛ', 'ಜ', 'ಝ', 'ಞ', 'ಟ', 'ಠ', 'ಡ', 
            'ಢ', 'ಣ', 'ತ', 'ಥ', 'ದ', 'ಧ', 'ನ', 'ನ', 'ಪ', 'ಫ', 'ಬ', 'ಭ', 'ಮ', 'ಯ', 'ಯ', 'ರ', 
            'ಱ', 'ಲ', 'ಳ', 'ಳ', 'ವ', 'ಶ', 'ಷ', 'ಸ', 'ಹ', '\0', 'ಾ', 'ಿ', 'ೀ', 'ು', 'ೂ', 'ೃ', 
            'ೆ', 'ೇ', 'ೈ', 'ೈ', 'ೊ', 'ೋ', 'ೌ', 'ೌ', '್', '\0', '.', '\0', '\0', '\0', '\0', '\0', 
            '\0', '೦', '೧', '೨', '೩', '೪', '೫', '೬', '೭', '೮', '೯', '\0', '\0', '\0', '\0', '\0'
         }, { 
            '\0', '\0', '\0', '\0', '\0', '\0', 'ಌ', 'ೡ', '\0', '\0', 'ೠ', '\0', '\0', '\0', '\0', '\0', 
            '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', 
            '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', 'ೞ', '\0', '\0', '\0', '\0', '\0', '\0', 
            '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', 'ೄ', 
            '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '‌', '‍', '\0', '\0', '\0', '\0', '\0', '\0', 
            '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0'
         } }, { { 
            '\0', '\0', 'ം', 'ഃ', 'അ', 'ആ', 'ഇ', 'ഈ', 'ഉ', 'ഊ', 'ഋ', 'എ', 'ഏ', 'ഐ', 'ഐ', 'ഒ', 
            'ഓ', 'ഔ', 'ഔ', 'ക', 'ഖ', 'ഗ', 'ഘ', 'ങ', 'ച', 'ഛ', 'ജ', 'ഝ', 'ഞ', 'ട', 'ഠ', 'ഡ', 
            'ഢ', 'ണ', 'ത', 'ഥ', 'ദ', 'ധ', 'ന', 'ന', 'പ', 'ഫ', 'ബ', 'ഭ', 'മ', 'യ', 'യ', 'ര', 
            'റ', 'ല', 'ള', 'ഴ', 'വ', 'ശ', 'ഷ', 'സ', 'ഹ', '\0', 'ാ', 'ി', 'ീ', 'ു', 'ൂ', 'ൃ', 
            'െ', 'േ', 'ൈ', 'ൈ', 'ൊ', 'ോ', 'ൌ', 'ൌ', '്', '\0', '.', '\0', '\0', '\0', '\0', '\0', 
            '\0', '൦', '൧', '൨', '൩', '൪', '൫', '൬', '൭', '൮', '൯', '\0', '\0', '\0', '\0', '\0'
         }, { 
            '\0', '\0', '\0', '\0', '\0', '\0', 'ഌ', 'ൡ', '\0', '\0', 'ൠ', '\0', '\0', '\0', '\0', '\0', 
            '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', 
            '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', 
            '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', 
            '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '‌', '‍', '\0', '\0', '\0', '\0', '\0', '\0', 
            '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0'
         } }, { { 
            '\0', 'ઁ', 'ં', 'ઃ', 'અ', 'આ', 'ઇ', 'ઈ', 'ઉ', 'ઊ', 'ઋ', 'એ', 'એ', 'ઐ', 'ઍ', 'ઍ', 
            'ઓ', 'ઔ', 'ઑ', 'ક', 'ખ', 'ગ', 'ઘ', 'ઙ', 'ચ', 'છ', 'જ', 'ઝ', 'ઞ', 'ટ', 'ઠ', 'ડ', 
            'ઢ', 'ણ', 'ત', 'થ', 'દ', 'ધ', 'ન', 'ન', 'પ', 'ફ', 'બ', 'ભ', 'મ', 'ય', 'ય', 'ર', 
            'ર', 'લ', 'ળ', 'ળ', 'વ', 'શ', 'ષ', 'સ', 'હ', '\0', 'ા', 'િ', 'ી', 'ુ', 'ૂ', 'ૃ', 
            'ે', 'ે', 'ૈ', 'ૅ', 'ો', 'ો', 'ૌ', 'ૉ', '્', '઼', '.', '\0', '\0', '\0', '\0', '\0', 
            '\0', '૦', '૧', '૨', '૩', '૪', '૫', '૬', '૭', '૮', '૯', '\0', '\0', '\0', '\0', '\0'
         }, { 
            '\0', 'ૐ', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', 'ૠ', '\0', '\0', '\0', '\0', '\0', 
            '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', 
            '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', 
            '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', 'ૄ', 
            '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '‌', '‍', 'ઽ', '\0', '\0', '\0', '\0', '\0', 
            '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0'
         } }, { { 
            '\0', '\0', 'ਂ', '\0', 'ਅ', 'ਆ', 'ਇ', 'ਈ', 'ਉ', 'ਊ', '\0', 'ਏ', 'ਏ', 'ਐ', 'ਐ', 'ਐ', 
            'ਓ', 'ਔ', 'ਔ', 'ਕ', 'ਖ', 'ਗ', 'ਘ', 'ਙ', 'ਚ', 'ਛ', 'ਜ', 'ਝ', 'ਞ', 'ਟ', 'ਠ', 'ਡ', 
            'ਢ', 'ਣ', 'ਤ', 'ਥ', 'ਦ', 'ਧ', 'ਨ', 'ਨ', 'ਪ', 'ਫ', 'ਬ', 'ਭ', 'ਮ', 'ਯ', 'ਯ', 'ਰ', 
            'ਰ', 'ਲ', 'ਲ਼', 'ਲ਼', 'ਵ', 'ਸ਼', 'ਸ਼', 'ਸ', 'ਹ', '\0', 'ਾ', 'ਿ', 'ੀ', 'ੁ', 'ੂ', '\0', 
            'ੇ', 'ੇ', 'ੈ', 'ੈ', 'ੋ', 'ੋ', 'ੌ', 'ੌ', '੍', '਼', '.', '\0', '\0', '\0', '\0', '\0', 
            '\0', '੦', '੧', '੨', '੩', '੪', '੫', '੬', '੭', '੮', '੯', '\0', '\0', '\0', '\0', '\0'
         }, { 
            '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', 
            '\0', '\0', '\0', '\0', 'ਖ਼', 'ਗ਼', '\0', '\0', '\0', '\0', 'ਜ਼', '\0', '\0', '\0', '\0', '\0', 
            'ੜ', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', 'ਫ਼', '\0', '\0', '\0', '\0', '\0', '\0', 
            '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', 
            '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '‌', '‍', '\0', '\0', '\0', '\0', '\0', '\0', 
            '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0'
         } } };
        private static int[] IndicMappingIndex = new int[] { -1, -1, 0, 1, 2, 3, 1, 4, 5, 6, 7, 8 };
        private const int MultiByteBegin = 160;
        private const byte Nukta = 0xe9;
        private static byte[] SecondIndicByte = new byte[] { 0, 0xe9, 0xb8, 0xbf };
        private static int[] UnicodeToIndicChar = new int[] { 
            0x2a1, 0x2a2, 0x2a3, 0, 0x2a4, 0x2a5, 0x2a6, 0x2a7, 680, 0x2a9, 0x2aa, 0x12a6, 0x2ae, 0x2ab, 0x2ac, 0x2ad, 
            690, 0x2af, 0x2b0, 0x2b1, 0x2b3, 0x2b4, 0x2b5, 0x2b6, 0x2b7, 0x2b8, 0x2b9, 0x2ba, 0x2bb, 700, 0x2bd, 0x2be, 
            0x2bf, 0x2c0, 0x2c1, 0x2c2, 0x2c3, 0x2c4, 0x2c5, 710, 0x2c7, 0x2c8, 0x2c9, 0x2ca, 0x2cb, 0x2cc, 0x2cd, 0x2cf, 
            720, 0x2d1, 0x2d2, 0x2d3, 0x2d4, 0x2d5, 0x2d6, 0x2d7, 0x2d8, 0, 0, 0x2e9, 0x12ea, 730, 0x2db, 0x2dc, 
            0x2dd, 0x2de, 0x2df, 0x12df, 0x2e3, 0x2e0, 0x2e1, 0x2e2, 0x2e7, 740, 0x2e5, 0x2e6, 0x2e8, 0, 0, 0x12a1, 
            0, 0x22f0, 0, 0, 0, 0, 0, 0x12b3, 0x12b4, 0x12b5, 0x12ba, 0x12bf, 0x12c0, 0x12c9, 0x2ce, 0x12aa, 
            0x12a7, 0x12db, 0x12dc, 0x2ea, 0, 0x2f1, 0x2f2, 0x2f3, 0x2f4, 0x2f5, 0x2f6, 0x2f7, 760, 0x2f9, 0x2fa, 0x32f0, 
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
            0x3a1, 930, 0x3a3, 0, 0x3a4, 0x3a5, 0x3a6, 0x3a7, 0x3a8, 0x3a9, 0x3aa, 0x13a6, 0, 0, 0x3ab, 0x3ad, 
            0, 0, 0x3af, 0x3b1, 0x3b3, 0x3b4, 0x3b5, 950, 0x3b7, 0x3b8, 0x3b9, 0x3ba, 0x3bb, 0x3bc, 0x3bd, 0x3be, 
            0x3bf, 960, 0x3c1, 0x3c2, 0x3c3, 0x3c4, 0x3c5, 0x3c6, 0, 0x3c8, 0x3c9, 970, 0x3cb, 0x3cc, 0x3cd, 0x3cf, 
            0, 0x3d1, 0, 0, 0, 0x3d5, 0x3d6, 0x3d7, 0x3d8, 0, 0, 0x3e9, 0, 0x3da, 0x3db, 0x3dc, 
            0x3dd, 990, 0x3df, 0x13df, 0, 0, 0x3e0, 0x3e2, 0, 0, 0x3e4, 0x3e6, 0x3e8, 0, 0, 0, 
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x13bf, 0x13c0, 0, 0x3ce, 0x13aa, 
            0x13a7, 0x13db, 0x13dc, 0, 0, 0x3f1, 0x3f2, 0x3f3, 0x3f4, 0x3f5, 0x3f6, 0x3f7, 0x3f8, 0x3f9, 0x3fa, 0, 
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
            0, 0xba2, 0, 0, 0xba4, 0xba5, 0xba6, 0xba7, 0xba8, 0xba9, 0, 0, 0, 0, 0xbab, 0xbad, 
            0, 0, 0xbb0, 0xbb1, 0xbb3, 0xbb4, 0xbb5, 0xbb6, 0xbb7, 0xbb8, 0xbb9, 0xbba, 0xbbb, 0xbbc, 0xbbd, 0xbbe, 
            0xbbf, 0xbc0, 0xbc1, 0xbc2, 0xbc3, 0xbc4, 0xbc5, 0xbc6, 0, 0xbc8, 0xbc9, 0xbca, 0xbcb, 0xbcc, 0xbcd, 0xbcf, 
            0, 0xbd1, 0xbd2, 0, 0xbd4, 0xbd5, 0, 0xbd7, 0xbd8, 0, 0, 0xbe9, 0, 0xbda, 0xbdb, 0xbdc, 
            0xbdd, 0xbde, 0, 0, 0, 0, 0xbe0, 0xbe2, 0, 0, 0xbe4, 0xbe6, 0xbe8, 0, 0, 0, 
            0, 0, 0, 0, 0, 0, 0, 0, 0x1bb4, 0x1bb5, 0x1bba, 0x1bc0, 0, 0x1bc9, 0, 0, 
            0, 0, 0, 0, 0, 0xbf1, 0xbf2, 0xbf3, 0xbf4, 0xbf5, 0xbf6, 0xbf7, 0xbf8, 0xbf9, 0xbfa, 0, 
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
            0xaa1, 0xaa2, 0xaa3, 0, 0xaa4, 0xaa5, 0xaa6, 0xaa7, 0xaa8, 0xaa9, 0xaaa, 0, 0xaae, 0, 0xaab, 0xaad, 
            0xab2, 0, 0xab0, 0xab1, 0xab3, 0xab4, 0xab5, 0xab6, 0xab7, 0xab8, 0xab9, 0xaba, 0xabb, 0xabc, 0xabd, 0xabe, 
            0xabf, 0xac0, 0xac1, 0xac2, 0xac3, 0xac4, 0xac5, 0xac6, 0, 0xac8, 0xac9, 0xaca, 0xacb, 0xacc, 0xacd, 0xacf, 
            0, 0xad1, 0xad2, 0, 0xad4, 0xad5, 0xad6, 0xad7, 0xad8, 0, 0, 0xae9, 0x1aea, 0xada, 0xadb, 0xadc, 
            0xadd, 0xade, 0xadf, 0x1adf, 0xae3, 0, 0xae0, 0xae2, 0xae7, 0, 0xae4, 0xae6, 0xae8, 0, 0, 0x1aa1, 
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x1aaa, 
            0, 0, 0, 0, 0, 0xaf1, 0xaf2, 0xaf3, 0xaf4, 0xaf5, 0xaf6, 0xaf7, 0xaf8, 0xaf9, 0xafa, 0, 
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
            0x7a1, 0x7a2, 0x7a3, 0, 0x7a4, 0x7a5, 0x7a6, 0x7a7, 0x7a8, 0x7a9, 0x7aa, 0x17a6, 0, 0, 0x7ab, 0x7ad, 
            0, 0, 0x7b0, 0x7b1, 0x7b3, 0x7b4, 0x7b5, 0x7b6, 0x7b7, 0x7b8, 0x7b9, 0x7ba, 0x7bb, 0x7bc, 0x7bd, 0x7be, 
            0x7bf, 0x7c0, 0x7c1, 0x7c2, 0x7c3, 0x7c4, 0x7c5, 0x7c6, 0, 0x7c8, 0x7c9, 0x7ca, 0x7cb, 0x7cc, 0x7cd, 0x7cf, 
            0, 0x7d1, 0x7d2, 0, 0, 0x7d5, 0x7d6, 0x7d7, 0x7d8, 0, 0, 0x7e9, 0x17ea, 0x7da, 0x7db, 0x7dc, 
            0x7dd, 0x7de, 0x7df, 0, 0, 0, 0x7e0, 0x7e2, 0, 0, 0x7e4, 0x7e6, 0x7e8, 0, 0, 0, 
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x17bf, 0x17c0, 0, 0x7ce, 0x17aa, 
            0x17a7, 0, 0, 0, 0, 0x7f1, 0x7f2, 0x7f3, 0x7f4, 0x7f5, 0x7f6, 0x7f7, 0x7f8, 0x7f9, 0x7fa, 0, 
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
            0, 0x4a2, 0x4a3, 0, 0x4a4, 0x4a5, 0x4a6, 0x4a7, 0x4a8, 0x4a9, 0, 0, 0, 0, 0x4ab, 0x4ad, 
            0, 0x4af, 0x4b0, 0x4b1, 0x4b3, 0, 0, 0, 0x4b7, 0x4b8, 0, 0x4ba, 0, 0x4bc, 0x4bd, 0, 
            0, 0, 0x4c1, 0x4c2, 0, 0, 0, 0x4c6, 0x4c7, 0x4c8, 0, 0, 0, 0x4cc, 0x4cd, 0x4cf, 
            0x4d0, 0x4d1, 0x4d2, 0x4d3, 0x4d4, 0, 0x4d5, 0x4d7, 0x4d8, 0, 0, 0, 0, 0x4da, 0x4db, 0x4dc, 
            0x4dd, 0x4de, 0, 0, 0, 0x4e0, 0x4e1, 0x4e2, 0, 0x4e4, 0x4e5, 0x4e6, 0x4e8, 0, 0, 0, 
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
            0, 0, 0, 0, 0, 0, 0x4f2, 0x4f3, 0x4f4, 0x4f5, 0x4f6, 0x4f7, 0x4f8, 0x4f9, 0x4fa, 0, 
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
            0x5a1, 0x5a2, 0x5a3, 0, 0x5a4, 0x5a5, 0x5a6, 0x5a7, 0x5a8, 0x5a9, 0x5aa, 0x15a6, 0, 0x5ab, 0x5ac, 0x5ad, 
            0, 0x5af, 0x5b0, 0x5b1, 0x5b3, 0x5b4, 0x5b5, 0x5b6, 0x5b7, 0x5b8, 0x5b9, 0x5ba, 0x5bb, 0x5bc, 0x5bd, 0x5be, 
            0x5bf, 0x5c0, 0x5c1, 0x5c2, 0x5c3, 0x5c4, 0x5c5, 0x5c6, 0, 0x5c8, 0x5c9, 0x5ca, 0x5cb, 0x5cc, 0x5cd, 0x5cf, 
            0x5d0, 0x5d1, 0x5d2, 0, 0x5d4, 0x5d5, 0x5d6, 0x5d7, 0x5d8, 0, 0, 0, 0, 0x5da, 0x5db, 0x5dc, 
            0x5dd, 0x5de, 0x5df, 0x15df, 0, 0x5e0, 0x5e1, 0x5e2, 0, 0x5e4, 0x5e5, 0x5e6, 0x5e8, 0, 0, 0, 
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x15aa, 
            0x15a7, 0, 0, 0, 0, 0x5f1, 0x5f2, 0x5f3, 0x5f4, 0x5f5, 0x5f6, 0x5f7, 0x5f8, 0x5f9, 0x5fa, 0, 
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
            0, 0x8a2, 0x8a3, 0, 0x8a4, 0x8a5, 0x8a6, 0x8a7, 0x8a8, 0x8a9, 0x8aa, 0x18a6, 0, 0x8ab, 0x8ac, 0x8ad, 
            0, 0x8af, 0x8b0, 0x8b1, 0x8b3, 0x8b4, 0x8b5, 0x8b6, 0x8b7, 0x8b8, 0x8b9, 0x8ba, 0x8bb, 0x8bc, 0x8bd, 0x8be, 
            0x8bf, 0x8c0, 0x8c1, 0x8c2, 0x8c3, 0x8c4, 0x8c5, 0x8c6, 0, 0x8c8, 0x8c9, 0x8ca, 0x8cb, 0x8cc, 0x8cd, 0x8cf, 
            0x8d0, 0x8d1, 0x8d2, 0, 0x8d4, 0x8d5, 0x8d6, 0x8d7, 0x8d8, 0, 0, 0, 0, 0x8da, 0x8db, 0x8dc, 
            0x8dd, 0x8de, 0x8df, 0x18df, 0, 0x8e0, 0x8e1, 0x8e2, 0, 0x8e4, 0x8e5, 0x8e6, 0x8e8, 0, 0, 0, 
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x18c9, 0, 0x18aa, 
            0x18a7, 0, 0, 0, 0, 0x8f1, 0x8f2, 0x8f3, 0x8f4, 0x8f5, 0x8f6, 0x8f7, 0x8f8, 0x8f9, 0x8fa, 0, 
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
            0, 0x9a2, 0x9a3, 0, 0x9a4, 0x9a5, 0x9a6, 0x9a7, 0x9a8, 0x9a9, 0x9aa, 0x19a6, 0, 0x9ab, 0x9ac, 0x9ad, 
            0, 0x9af, 0x9b0, 0x9b1, 0x9b3, 0x9b4, 0x9b5, 0x9b6, 0x9b7, 0x9b8, 0x9b9, 0x9ba, 0x9bb, 0x9bc, 0x9bd, 0x9be, 
            0x9bf, 0x9c0, 0x9c1, 0x9c2, 0x9c3, 0x9c4, 0x9c5, 0x9c6, 0, 0x9c8, 0x9c9, 0x9ca, 0x9cb, 0x9cc, 0x9cd, 0x9cf, 
            0x9d0, 0x9d1, 0x9d2, 0x9d3, 0x9d4, 0x9d5, 0x9d6, 0x9d7, 0x9d8, 0, 0, 0, 0, 0x9da, 0x9db, 0x9dc, 
            0x9dd, 0x9de, 0x9df, 0, 0, 0x9e0, 0x9e1, 0x9e2, 0, 0x9e4, 0x9e5, 0x9e6, 0x9e8, 0, 0, 0, 
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x19aa, 
            0x19a7, 0, 0, 0, 0, 0x9f1, 0x9f2, 0x9f3, 0x9f4, 0x9f5, 0x9f6, 0x9f7, 0x9f8, 0x9f9, 0x9fa
         };
        private const byte Virama = 0xe8;
        private const char ZWJ = '‍';
        private const char ZWNJ = '‌';

        public ISCIIEncoding(int codePage) : base(codePage)
        {
            this.defaultCodePage = codePage - 0xdea8;
            if ((this.defaultCodePage < 2) || (this.defaultCodePage > 11))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_CodepageNotSupported", new object[] { codePage }), "codePage");
            }
        }

        internal ISCIIEncoding(SerializationInfo info, StreamingContext context) : base(0)
        {
            throw new ArgumentException(Environment.GetResourceString("Arg_ExecutionEngineException"));
        }

        [SecurityCritical]
        internal override unsafe int GetByteCount(char* chars, int count, EncoderNLS baseEncoder)
        {
            return this.GetBytes(chars, count, null, 0, baseEncoder);
        }

        [SecurityCritical]
        internal override unsafe int GetBytes(char* chars, int charCount, byte* bytes, int byteCount, EncoderNLS baseEncoder)
        {
            ISCIIEncoder inEncoder = (ISCIIEncoder) baseEncoder;
            Encoding.EncodingByteBuffer buffer = new Encoding.EncodingByteBuffer(this, inEncoder, bytes, byteCount, chars, charCount);
            int defaultCodePage = this.defaultCodePage;
            bool bLastVirama = false;
            if (inEncoder != null)
            {
                defaultCodePage = inEncoder.currentCodePage;
                bLastVirama = inEncoder.bLastVirama;
                if (inEncoder.charLeftOver > '\0')
                {
                    buffer.Fallback(inEncoder.charLeftOver);
                    bLastVirama = false;
                }
            }
            while (buffer.MoreData)
            {
                char nextChar = buffer.GetNextChar();
                if (nextChar < '\x00a0')
                {
                    if (!buffer.AddByte((byte) nextChar))
                    {
                        break;
                    }
                    bLastVirama = false;
                    continue;
                }
                if ((nextChar < 'ँ') || (nextChar > '൯'))
                {
                    if (bLastVirama && ((nextChar == '‌') || (nextChar == '‍')))
                    {
                        if (nextChar == '‌')
                        {
                            if (!buffer.AddByte(0xe8))
                            {
                                break;
                            }
                        }
                        else if (!buffer.AddByte(0xe9))
                        {
                            break;
                        }
                        bLastVirama = false;
                        continue;
                    }
                    buffer.Fallback(nextChar);
                    bLastVirama = false;
                    continue;
                }
                int num2 = UnicodeToIndicChar[nextChar - 'ँ'];
                byte b = (byte) num2;
                int num4 = 15 & (num2 >> 8);
                int num5 = 0xf000 & num2;
                if (num2 == 0)
                {
                    buffer.Fallback(nextChar);
                    bLastVirama = false;
                }
                else
                {
                    if (num4 != defaultCodePage)
                    {
                        if (!buffer.AddByte(0xef, (byte) (num4 | 0x40)))
                        {
                            break;
                        }
                        defaultCodePage = num4;
                    }
                    if (!buffer.AddByte(b, (num5 != 0) ? 1 : 0))
                    {
                        break;
                    }
                    bLastVirama = b == 0xe8;
                    if ((num5 != 0) && !buffer.AddByte(SecondIndicByte[num5 >> 12]))
                    {
                        break;
                    }
                }
            }
            if ((defaultCodePage != this.defaultCodePage) && ((inEncoder == null) || inEncoder.MustFlush))
            {
                if (buffer.AddByte(0xef, (byte) (this.defaultCodePage | 0x40)))
                {
                    defaultCodePage = this.defaultCodePage;
                }
                else
                {
                    buffer.GetNextChar();
                }
                bLastVirama = false;
            }
            if ((inEncoder != null) && (bytes != null))
            {
                if (!buffer.fallbackBuffer.bUsedEncoder)
                {
                    inEncoder.charLeftOver = '\0';
                }
                inEncoder.currentCodePage = defaultCodePage;
                inEncoder.bLastVirama = bLastVirama;
                inEncoder.m_charsUsed = buffer.CharsUsed;
            }
            return buffer.Count;
        }

        [SecurityCritical]
        internal override unsafe int GetCharCount(byte* bytes, int count, DecoderNLS baseDecoder)
        {
            return this.GetChars(bytes, count, null, 0, baseDecoder);
        }

        [SecurityCritical]
        internal override unsafe int GetChars(byte* bytes, int byteCount, char* chars, int charCount, DecoderNLS baseDecoder)
        {
            ISCIIDecoder decoder = (ISCIIDecoder) baseDecoder;
            Encoding.EncodingCharBuffer buffer = new Encoding.EncodingCharBuffer(this, decoder, chars, charCount, bytes, byteCount);
            int defaultCodePage = this.defaultCodePage;
            bool bLastATR = false;
            bool bLastVirama = false;
            bool bLastDevenagariStressAbbr = false;
            char cLastCharForNextNukta = '\0';
            char cLastCharForNoNextNukta = '\0';
            if (decoder != null)
            {
                defaultCodePage = decoder.currentCodePage;
                bLastATR = decoder.bLastATR;
                bLastVirama = decoder.bLastVirama;
                bLastDevenagariStressAbbr = decoder.bLastDevenagariStressAbbr;
                cLastCharForNextNukta = decoder.cLastCharForNextNukta;
                cLastCharForNoNextNukta = decoder.cLastCharForNoNextNukta;
            }
            bool flag4 = ((bLastVirama | bLastATR) | bLastDevenagariStressAbbr) | (cLastCharForNextNukta != '\0');
            int num2 = -1;
            if ((defaultCodePage >= 2) && (defaultCodePage <= 11))
            {
                num2 = IndicMappingIndex[defaultCodePage];
            }
            while (buffer.MoreData)
            {
                byte nextByte = buffer.GetNextByte();
                if (flag4)
                {
                    flag4 = false;
                    if (bLastATR)
                    {
                        if ((nextByte >= 0x42) && (nextByte <= 0x4b))
                        {
                            defaultCodePage = nextByte & 15;
                            num2 = IndicMappingIndex[defaultCodePage];
                            bLastATR = false;
                            continue;
                        }
                        if (nextByte == 0x40)
                        {
                            defaultCodePage = this.defaultCodePage;
                            num2 = -1;
                            if ((defaultCodePage >= 2) && (defaultCodePage <= 11))
                            {
                                num2 = IndicMappingIndex[defaultCodePage];
                            }
                            bLastATR = false;
                            continue;
                        }
                        if (nextByte == 0x41)
                        {
                            defaultCodePage = this.defaultCodePage;
                            num2 = -1;
                            if ((defaultCodePage >= 2) && (defaultCodePage <= 11))
                            {
                                num2 = IndicMappingIndex[defaultCodePage];
                            }
                            bLastATR = false;
                            continue;
                        }
                        if (!buffer.Fallback((byte) 0xef))
                        {
                            break;
                        }
                        bLastATR = false;
                    }
                    else if (bLastVirama)
                    {
                        if (nextByte == 0xe8)
                        {
                            if (!buffer.AddChar('‌'))
                            {
                                break;
                            }
                            bLastVirama = false;
                            continue;
                        }
                        if (nextByte == 0xe9)
                        {
                            if (!buffer.AddChar('‍'))
                            {
                                break;
                            }
                            bLastVirama = false;
                            continue;
                        }
                        bLastVirama = false;
                    }
                    else if (bLastDevenagariStressAbbr)
                    {
                        if (nextByte == 0xb8)
                        {
                            if (!buffer.AddChar('॒'))
                            {
                                break;
                            }
                            bLastDevenagariStressAbbr = false;
                            continue;
                        }
                        if (nextByte == 0xbf)
                        {
                            if (!buffer.AddChar('॰'))
                            {
                                break;
                            }
                            bLastDevenagariStressAbbr = false;
                            continue;
                        }
                        if (!buffer.Fallback((byte) 240))
                        {
                            break;
                        }
                        bLastDevenagariStressAbbr = false;
                    }
                    else
                    {
                        if (nextByte == 0xe9)
                        {
                            if (!buffer.AddChar(cLastCharForNextNukta))
                            {
                                break;
                            }
                            cLastCharForNextNukta = cLastCharForNoNextNukta = '\0';
                            continue;
                        }
                        if (!buffer.AddChar(cLastCharForNoNextNukta))
                        {
                            break;
                        }
                        cLastCharForNextNukta = cLastCharForNoNextNukta = '\0';
                    }
                }
                if (nextByte < 160)
                {
                    if (buffer.AddChar((char) nextByte))
                    {
                        continue;
                    }
                    break;
                }
                if (nextByte == 0xef)
                {
                    bLastATR = flag4 = true;
                }
                else
                {
                    char ch = IndicMapping[num2, 0, nextByte - 160];
                    char ch4 = IndicMapping[num2, 1, nextByte - 160];
                    if ((ch4 == '\0') || (nextByte == 0xe9))
                    {
                        if (ch == '\0')
                        {
                            if (buffer.Fallback(nextByte))
                            {
                                continue;
                            }
                        }
                        else if (buffer.AddChar(ch))
                        {
                            continue;
                        }
                        break;
                    }
                    if (nextByte == 0xe8)
                    {
                        if (!buffer.AddChar(ch))
                        {
                            break;
                        }
                        bLastVirama = flag4 = true;
                    }
                    else
                    {
                        if ((ch4 & 0xf000) == 0)
                        {
                            flag4 = true;
                            cLastCharForNextNukta = ch4;
                            cLastCharForNoNextNukta = ch;
                            continue;
                        }
                        bLastDevenagariStressAbbr = flag4 = true;
                    }
                }
            }
            if ((decoder == null) || decoder.MustFlush)
            {
                if (bLastATR)
                {
                    if (buffer.Fallback((byte) 0xef))
                    {
                        bLastATR = false;
                    }
                    else
                    {
                        buffer.GetNextByte();
                    }
                }
                else if (bLastDevenagariStressAbbr)
                {
                    if (buffer.Fallback((byte) 240))
                    {
                        bLastDevenagariStressAbbr = false;
                    }
                    else
                    {
                        buffer.GetNextByte();
                    }
                }
                else if (cLastCharForNoNextNukta != '\0')
                {
                    if (buffer.AddChar(cLastCharForNoNextNukta))
                    {
                        cLastCharForNoNextNukta = cLastCharForNextNukta = '\0';
                    }
                    else
                    {
                        buffer.GetNextByte();
                    }
                }
            }
            if ((decoder != null) && (chars != null))
            {
                if ((!decoder.MustFlush || (cLastCharForNoNextNukta != '\0')) || (bLastATR || bLastDevenagariStressAbbr))
                {
                    decoder.currentCodePage = defaultCodePage;
                    decoder.bLastVirama = bLastVirama;
                    decoder.bLastATR = bLastATR;
                    decoder.bLastDevenagariStressAbbr = bLastDevenagariStressAbbr;
                    decoder.cLastCharForNextNukta = cLastCharForNextNukta;
                    decoder.cLastCharForNoNextNukta = cLastCharForNoNextNukta;
                }
                else
                {
                    decoder.currentCodePage = this.defaultCodePage;
                    decoder.bLastVirama = false;
                    decoder.bLastATR = false;
                    decoder.bLastDevenagariStressAbbr = false;
                    decoder.cLastCharForNextNukta = '\0';
                    decoder.cLastCharForNoNextNukta = '\0';
                }
                decoder.m_bytesUsed = buffer.BytesUsed;
            }
            return buffer.Count;
        }

        public override System.Text.Decoder GetDecoder()
        {
            return new ISCIIDecoder(this);
        }

        public override System.Text.Encoder GetEncoder()
        {
            return new ISCIIEncoder(this);
        }

        public override int GetHashCode()
        {
            return ((this.defaultCodePage + base.EncoderFallback.GetHashCode()) + base.DecoderFallback.GetHashCode());
        }

        public override int GetMaxByteCount(int charCount)
        {
            if (charCount < 0)
            {
                throw new ArgumentOutOfRangeException("charCount", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            long num = charCount + 1L;
            if (base.EncoderFallback.MaxCharCount > 1)
            {
                num *= base.EncoderFallback.MaxCharCount;
            }
            num *= 4L;
            if (num > 0x7fffffffL)
            {
                throw new ArgumentOutOfRangeException("charCount", Environment.GetResourceString("ArgumentOutOfRange_GetByteCountOverflow"));
            }
            return (int) num;
        }

        public override int GetMaxCharCount(int byteCount)
        {
            if (byteCount < 0)
            {
                throw new ArgumentOutOfRangeException("byteCount", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            long num = byteCount + 1L;
            if (base.DecoderFallback.MaxCharCount > 1)
            {
                num *= base.DecoderFallback.MaxCharCount;
            }
            if (num > 0x7fffffffL)
            {
                throw new ArgumentOutOfRangeException("byteCount", Environment.GetResourceString("ArgumentOutOfRange_GetCharCountOverflow"));
            }
            return (int) num;
        }

        [SecurityCritical]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.SerializeEncoding(info, context);
            info.AddValue("m_maxByteSize", 2);
            info.SetType(typeof(MLangCodePageEncoding));
        }

        [Serializable]
        internal class ISCIIDecoder : DecoderNLS
        {
            internal bool bLastATR;
            internal bool bLastDevenagariStressAbbr;
            internal bool bLastVirama;
            internal char cLastCharForNextNukta;
            internal char cLastCharForNoNextNukta;
            internal int currentCodePage;

            public ISCIIDecoder(Encoding encoding) : base(encoding)
            {
                this.currentCodePage = encoding.CodePage - 0xdea8;
            }

            public override void Reset()
            {
                this.bLastATR = false;
                this.bLastVirama = false;
                this.bLastDevenagariStressAbbr = false;
                this.cLastCharForNextNukta = '\0';
                this.cLastCharForNoNextNukta = '\0';
                if (base.m_fallbackBuffer != null)
                {
                    base.m_fallbackBuffer.Reset();
                }
            }

            internal override bool HasState
            {
                get
                {
                    if (((this.cLastCharForNextNukta == '\0') && (this.cLastCharForNoNextNukta == '\0')) && !this.bLastATR)
                    {
                        return this.bLastDevenagariStressAbbr;
                    }
                    return true;
                }
            }
        }

        [Serializable]
        internal class ISCIIEncoder : EncoderNLS
        {
            internal bool bLastVirama;
            internal int currentCodePage;
            internal int defaultCodePage;

            public ISCIIEncoder(Encoding encoding) : base(encoding)
            {
                this.currentCodePage = this.defaultCodePage = encoding.CodePage - 0xdea8;
            }

            public override void Reset()
            {
                this.bLastVirama = false;
                base.charLeftOver = '\0';
                if (base.m_fallbackBuffer != null)
                {
                    base.m_fallbackBuffer.Reset();
                }
            }

            internal override bool HasState
            {
                get
                {
                    if (base.charLeftOver == '\0')
                    {
                        return (this.currentCodePage != this.defaultCodePage);
                    }
                    return true;
                }
            }
        }
    }
}

