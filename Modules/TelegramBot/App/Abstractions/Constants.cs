namespace TelegramBot.Abstractions;

public static class Constants
{
    public const string AntiCaptcha = "anticaptcha";

    public static class Kdmid
    {
        public const string Key = "kdmid";
        public static class Cities
        {
            public const string Belgrade = "blgrd";
            public const string Budapest = "bdpst";
            public const string Paris = "pris";
            public const string Bucharest = "bchrt";
            public const string Rome = "rm";
            public const string Vienna = "vn";
            public const string Warsaw = "wrs";
            public const string Ljubljana = "ljbln";
            public const string Sarajevo = "srjv";
            public const string Tirana = "trn";
            public const string Canberra = "cnbr";
            public const string Ottawa = "otw";
            public const string Washington = "wshngtn";
            public const string Berlin = "brln";
            public const string Bern = "brn";
            public const string Brussels = "brssls";
            public const string Helsinki = "hlsnk";
            public const string Madrid = "mdrd";
            public const string Oslo = "osl";
            public const string Stockholm = "stkhlm";
            public const string Hague = "hg";
            public const string Dublin = "dbrln";
            public const string Lisbon = "lps";
            public const string Luxembourg = "lxmbrg";
            public const string Riga = "rg";
            public const string Tallinn = "tlln";
            public const string Vilnius = "vltns";
            public const string Prague = "prg";
            public const string Valletta = "vlt";
        }
        public static class Commands
        {
            public const string Menu = "mnu";
            public const string Request = "req";
            public const string Seek = "sk";
            public const string Confirm = "cnf";
        }
    }

    public enum ProcessSteps
    {
        Process = 1
    }

    public enum ButtonStyle
    {
        VerticallyStrict,
        VerticallyFlex,
        Horizontally
    }
}
