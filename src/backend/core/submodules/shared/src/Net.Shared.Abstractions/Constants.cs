namespace Net.Shared.Abstractions
{
    public static class Constants
    {
        public static class Enums
        {
            public enum AssetTypes
            {
                Valuable = 1,
                CurrencyFiat = 2,
                Stock = 3,
                Bond = 4,
                Fund = 5,
                CurrencyToken = 6,
                NftToken = 7,
                RealEstate = 8,
                PersonalEstate = 9
            }
            public enum Countries
            {
                Rus = 1,
                Usa = 2,
                Chn = 3,
                Gbr = 4,
                Deu = 5,
                Che = 6,
                Jpn = 7
            }
            public enum Currencies
            {
                Rub = 1,
                Usd = 2,
                Eur = 3,
                Gbp = 4,
                Chy = 5,
            }
            public enum Exchanges
            {
                Spbex = 1,
                Moex = 2,
                Nyse = 3,
                Nasdaq = 4,
                Fwb = 5,
                Hkse = 6,
                Lse = 7,
                Sse = 8,
                Binance = 9,
                Ftx2 = 10,
                Coinbase = 11
            }
        }
    }
}