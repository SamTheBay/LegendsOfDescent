using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LegendsOfDescent
{
    /// <summary>
    /// From www.kessels.com/WordGenerator/NameGenerator.html
    /// </summary>
    public class NameGenerator
    {
        public enum Style
        {
            MixedMaleFantasy,
            MixedFemaleFantasy,
            MaleDeverry,
            FemaleDeverry,
            ElvenDeverry,
            ElvenMaleTolkien,
            ElvenFemaleTolkien,
            MaleDwarvenTolkien,
            MaleHalflingTolkien,
            OrcTolkien,
            OrcWarhammer,
            MixedFelana,
            Babylon5Narn
        }

        private static Dictionary<Style, string[][]> syllables = new Dictionary<Style, string[][]>()
        {

            {
                Style.MixedMaleFantasy,                
                new string[][]
                {
                    new string[] { "A","Ab","Ac","Ad","Af","Agr","Ast","As","Al","Adw","Adr","Ar","B","Br","C","C",
                        "C","Cr","Ch","Cad","D","Dr","Dw","Ed","Eth","Et","Er","El","Eow","F","Fr","G",
                        "Gr","Gw","Gw","Gal","Gl","H","Ha","Ib","J","Jer","K","Ka","Ked","L","Loth","Lar",
                        "Leg","M","Mir","N","Nyd","Ol","Oc","On","P","Pr","Q","R","Rh","S","Sev","T",
                        "Tr","Th","Th","Ul","Um","Un","V","Y","Yb","Z","W","W","Wic" },
                    new string[] {"a","ae","ae","au","ao","are","ale","ali","ay","ardo","e","edri","ei","ea","ea",
                        "eri","era","ela","eli","enda","erra","i","ia","ie","ire","ira","ila","ili","ira",
                        "igo","o","oha","oma","oa","oi","oe","ore","u","y"},
                    new string[] {" "," "," "," "," "," "," ","a","and","b","bwyn","baen","bard","c","ch","can",
                        "d","dan","don","der","dric","dus","f","g","gord","gan","han","har","jar","jan",
                        "k","kin","kith","kath","koth","kor","kon","l","li","lin","lith","lath","loth",
                        "ld","ldan","m","mas","mos","mar","mond","n","nydd","nidd","nnon","nwan","nyth",
                        "nad","nn","nnor","nd","p","r","red","ric","rid","rin","ron","rd","s","sh","seth",
                        "sean","t","th","th","tha","tlan","trem","tram","v","vudd","w","wan","win","win",
                        "wyn","wyn","wyr","wyr","wyth" }
                }
            },  
                    
            {
                Style.MixedFemaleFantasy,
                new string[][] 
                {
                    new string[] {"A","Ab","Ac","Ad","Af","Agr","Ast","As","Al","Adw","Adr","Ar","B","Br","C","C",
                        "C","Cr","Ch","Cad","D","Dr","Dw","Ed","Eth","Et","Er","El","Eow","F","Fr","G",
                        "Gr","Gw","Gw","Gal","Gl","H","Ha","Ib","Jer","K","Ka","Ked","L","Loth","Lar",
                        "Leg","M","Mir","N","Nyd","Ol","Oc","On","P","Pr","Q","R","Rh","S","Sev","T",
                        "Tr","Th","Th","Ul","Um","Un","V","Y","Yb","Z","W","W","Wic"},
                    new string[] {"a","a","a","ae","ae","au","ao","are","ale","ali","ay","ardo","e","e","e","ei",
                        "ea","ea","eri","era","ela","eli","enda","erra","i","i","i","ia","ie","ire",
                        "ira","ila","ili","ira","igo","o","oa","oi","oe","ore","u","y"},
                    new string[] {"beth","cia","cien","clya","de","dia","dda","dien","dith","dia","lind","lith",
                        "lia","lian","lla","llan","lle","ma","mma","mwen","meth","n","n","n","nna",
                        "ndra","ng","ni","nia","niel","rith","rien","ria","ri","rwen","sa","sien","ssa",
                        "ssi","swen","thien","thiel","viel","via","ven","veth","wen","wen","wen","wen",
                        "wia","weth","wien","wiel" }
                }
            },

    {
        Style.MaleDeverry,     /* 2: Male Deverry-style */

        new string[][]
        {
            new string[] { "Aeth","Addr","Bl","C","Car","D","G","Gl","Gw","L","M","Ow","R","Rh","S","T",      
                "V","Yr"},    
            new string[] { "a","ae","e","eo","i","o","u","y"},    
            new string[] { "bryn","c","cyn","dd","ddry","ddyn","doc","dry","gwyn","llyn","myr","n","nnyn",      
                "nry","nvan","nyc","r","rcyn","rraent","ran","ryn" }
        }
    },

    { 
        Style.FemaleDeverry,      /* 3: Female Deverry-style */

        new string[][] {
        new string[] {"Al","Br","C","Cl","D","El","Gw","J","L","M","N","Mer","S","R","Ys"},    
        new string[] {"a","ae","e","ea","i","o","u","y","w"},    
        new string[] {"brylla","cla","dda","ll","lla","llyra","lonna","lyan","na","ngwen","niver",      "noic","ra","rka","ryan","ssa","vyan" }

        }},

    { 
        Style.ElvenDeverry,      /* 4: Elven Deverry-style */

        new string[][] {
        new string[] {"Ad","Adr","Al","Alb","Alod","Ann","B","Ban","Ber","Cal","Car","Carr","Cont",      "Dall","Dar","Dev","Eb","El","Elb","En","Far","Gann","Gav","Hal","Jav","Jenn",      "L","Land","Man","Mer","Nan","Ran","Tal","Tal","Val","Wyl"},    
        new string[] {"a","a","abe","abri","ae","ae","ala","alae","ale","ama","amae","ana","e","e",      "ede","ena","ere","o","oba","obre"},    
        new string[] {"beriel","clya","danten","dar","ddlaen","gyn","ladar","ldar","lden","lia",      "mario","na","ndar","ndario","nderiel","ndra","nnon","nna","ntar","ntariel",      "nteriel","ny","raen","rel","ria","riel","ryn","ssi","teriel","ver" }

        }},

    { 
        Style.ElvenMaleTolkien,      /* 5: Elven Male Tolkien-style */

        new string[][] {
        new string[] {"An","Am","Bel","Cel","C","Cal","Del","El","Elr","Elv","Eow","Eär","F","G",      "Gal","Gl","H","Is","Leg","Lóm","M","N","P","R","S","T","Thr","Tin","Ur","Un",      "V"},    
        new string[] {"a","á","adrie","ara","e","é","ebri","i","io","ithra","ilma","il-Ga","o","orfi",      "ó","u","y"},    
        new string[] {"l","las","lad","ldor","ldur","lith","mir","n","nd","ndel","ndil","ndir",      "nduil","ng","mbor","r","ril","riand","rion","wyn" }

        }},

    { 
        Style.ElvenFemaleTolkien,      /* 6: Elven Female Tolkien-style */

        new string[][] {
        new string[] {"An","Am","Bel","Cel","C","Cal","Del","El","Elr","Elv","Eow","Eär","F","G",      "Gal","Gl","H","Is","Leg","Lóm","M","N","P","R","S","T","Thr","Tin","Ur",      "Un","V"},    
        new string[] {"a","á","adrie","ara","e","é","ebri","i","io","ithra","ilma","il-Ga","o",      "orfi","ó","u","y"},    
        new string[] {"clya","lindë","dë","dien","dith","dia","lith","lia","ndra","ng","nia","niel",      "rith","thien","thiel","viel","wen","wien","wiel" }

        }},

    { 
        Style.MaleDwarvenTolkien,      /* 7: Male Dwarven Tolkien-style */

        new string[][] {
        new string[] {"B","D","F","G","Gl","H","K","L","M","N","R","S","T","V"},    
        new string[] {"a","e","i","o","oi","u"},    
        new string[] {"bur","fur","gan","gnus","gnar","li","lin","lir","mli","nar","nus","rin","ran",      "sin","sil","sur" }

    }},

    { 
        Style.MaleHalflingTolkien,      /* 8: Male Halfling Tolkien-style */

        new string[][] {
        new string[] {"B","Dr","Fr","Mer","Per","R","S"},    
        new string[] {"a","e","i","ia","o","oi","u"},    
        new string[] {"bo","do","doc","go","grin","m","ppi","rry" }

    }},

    { 
        Style.OrcTolkien,      /* 9: Orc Tolkien-style */

        new string[][] {
        new string[] {"B","Er","G","Gr","H","P","Pr","R","V","Vr"},    
        new string[] {"a","i","o","u"},    
        new string[] {"dash","dish","dush","gar","gor","gdush","lo","gdish","k","lg","nak","rag",      "rbag","rg","rk","ng","nk","rt","ol","urk","shnak" }

    }},

    { 
        Style.OrcWarhammer,      /* 10: Orc Warhammer-style */

        new string[][] {
        new string[] {"Head","Face","Eye","Arm","Foot","Toe","Ear","Nose","Hair","Blood","Nail",      "Snotling","Enemy","Public","Beast","Man","Finger","Goblin","Gretchin",      "Hobbit","Teeth","Elf","Rat","Ball","Ghoul","Knife","Axe","Wraith","Deamon",      "Dragon","Tooth","Death","Mother","Horse","Moon","Dwarf","Earth","Human",      "Grass"},    
        new string[] {" "},    
        new string[] {"killer","crucher","lover","thrower","throttler","eater","hammer","kicker",      "walker","punsher","dragger","stomper","torturer","ripper","mangler","hater",      "poker","chewer","cutter","slicer","juggler","raper","smasher","shooter",      "drinker","crawler" }

    }},

    { 
        Style.MixedFelana,      /* 11: Mixed Felana-style */

        new string[][]{
        new string[] {"Am","An","As","Ash","Ast","C","Chen","Chan","Char","Cher","Cer","Es",      "Esh","Is","Ish","Os","Osh","Us","Ush","Ys","Ysh","H","Ch","S","Shen",      "Sar","Sol","Shar","Shan","Sher","Shim","Sim","Sin","San","Sar","Ser",      "Sor","Shor","Sham","Sh"},    
        new string[] {" "," "," "," ","a","ar","as","e","es","i","is","o","os","u","us","y",      "ys","er","or","ur","yr","ir","eri","ari","osh","ash","esh","ish",      "ush","ysh","en","an","in","on","un","yn"},    
        new string[] {" "," "," "," "," "," "," "," ","dar","mir","nir","nor","nar","ish",      "ash","osh","esh","isha","asha","esha","osha","orsha","a","e","i","o",      "u","y","sar","ser","sor","sir","der","sham","shor","shen","as","es",      "ys","seth","san","sin","sil","sur","sen","sean","dor" }

    }},

    { 
        Style.Babylon5Narn,      /* 12: Babylon 5 Narn-style */

        new string[][] {
        new string[] {"Ch'","Do'","G'","Gre'","Mak'","Na'","Re'","Sh'","So'","T'","Ta'",      "Th'","Thu'","Tu'"},    
        new string[] {"Ba","Bo","Da","Do","Ga","Ge","Go","Ka","Ko","La","Le","Lo","Ma","Mo",      "Na","No","Oo","Pa","Po","Qua","Quo","Ra","Rala","Ro","Sha","Shali",      "Ska","Skali","Sta","Ste","Sto","Ta","Te","Tee","To","Tha","Tho","Va",      "Vo","Vy","Wa"},    
        new string[] {"ch","k","kk","l","n","r","th","s" }}}};

        public static string GetName(Style style)
        {
            var grammar = syllables[style];
            return string.Join("", grammar.Select(level => level.Random()).ToArray());
        }
    }
}
