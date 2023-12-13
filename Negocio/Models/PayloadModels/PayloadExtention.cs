using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Negocio.Models.PayloadModels
{
    public static class PayloadExtention
    {
        public static string GetIndicator() => PayloadExtention.GetValue(PayloadId.PayloadFormatIndicator, "01");

        public static string GetMerchantAccountInformation(this Payload payload) => PayloadExtention.GetValue(PayloadId.MerchantAccountInfomation, PayloadExtention.GetValue(PayloadId.MerchantAccountInfomationGui, "br.gov.bcb.pix") + (!string.IsNullOrEmpty(payload?.PixKey) ? PayloadExtention.GetValue(PayloadId.MerchantAccountInfomationKey, payload.PixKey) : "") + (!string.IsNullOrEmpty(payload?.Description) ? PayloadExtention.GetValue(PayloadId.MerchantAccountInformationDescription, payload.Description) : "") + (!string.IsNullOrEmpty(payload?.Url) ? PayloadExtention.GetValue(PayloadId.MerchantAccountInformationUrl, payload.Url) : ""));

        public static string GetMerchantCategoryCode() => PayloadExtention.GetValue(PayloadId.MerchantCategoryCode, "0000");

        public static string GetTransationCurrency() => PayloadExtention.GetValue(PayloadId.TransactionCurrency, "986");

        public static string GetTransationAmount(this Payload payload) => string.IsNullOrEmpty(payload?.Amount) ? "" : PayloadExtention.GetValue(PayloadId.TransactionAmount, payload.Amount);

        public static string GetCountryCode() => PayloadExtention.GetValue(PayloadId.CountryCode, "BR");

        public static string GetMerchantName(this Payload payload) => PayloadExtention.GetValue(PayloadId.MerchantName, payload.Merchant.Name);

        public static string GetMerchantCity(this Payload payload) => PayloadExtention.GetValue(PayloadId.MerchantCity, payload.Merchant.City);

        public static string GetAdditionalDataFieldTemplate(this Payload payload) => PayloadExtention.GetValue(PayloadId.AdditionalFieldTemplate, PayloadExtention.GetValue(PayloadId.AdditionalFieldTemplateTxId, payload.TxId.Length > 25 ? payload.TxId.Substring(0, 25) : payload.TxId));

        public static string GetCrc16(string fullPaylod)
        {
            string crc = Crc16.ComputeCRC(fullPaylod + PayloadId.CRC16 + "04");
            return fullPaylod + PayloadExtention.GetValue(PayloadId.CRC16, crc);
        }

        public static string GetUniquePayment(this Payload payload) => !payload.UniquePayment ? "" : PayloadExtention.GetValue(PayloadId.PointOfInitiationMethod, "12");

        public static string GetValue(string id, string value)
        {
            string str = value.Length < 10 ? "0" + value.Length.ToString() : value.Length.ToString();
            return id + str + value;
        }

        public static string GenerateStringToQrCode(this Payload payload) => PayloadExtention.GetCrc16(PayloadExtention.GetIndicator() + payload.GetUniquePayment() + payload.GetMerchantAccountInformation() + PayloadExtention.GetMerchantCategoryCode() + PayloadExtention.GetTransationCurrency() + payload.GetTransationAmount() + PayloadExtention.GetCountryCode() + payload.GetMerchantName() + payload.GetMerchantCity() + payload.GetAdditionalDataFieldTemplate());
    }

    public abstract class PayloadId
    {
        public static string PayloadFormatIndicator => "00";

        public static string PointOfInitiationMethod => "01";

        public static string MerchantAccountInfomation => "26";

        public static string MerchantAccountInfomationGui => "00";

        public static string MerchantAccountInfomationKey => "01";

        public static string MerchantAccountInformationDescription => "02";

        public static string MerchantAccountInformationUrl => "25";

        public static string MerchantCategoryCode => "52";

        public static string TransactionCurrency => "53";

        public static string TransactionAmount => "54";

        public static string CountryCode => "58";

        public static string MerchantName => "59";

        public static string MerchantCity => "60";

        public static string AdditionalFieldTemplate => "62";

        public static string AdditionalFieldTemplateTxId => "05";

        public static string CRC16 => "63";
    }


    public static class Crc16
    {
        public static string ComputeCRC(string str)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(str);
            List<int> intList = new List<int>()
      {
        0,
        4129,
        8258,
        12387,
        16516,
        20645,
        24774,
        28903,
        33032,
        37161,
        41290,
        45419,
        49548,
        53677,
        57806,
        61935,
        4657,
        528,
        12915,
        8786,
        21173,
        17044,
        29431,
        25302,
        37689,
        33560,
        45947,
        41818,
        54205,
        50076,
        62463,
        58334,
        9314,
        13379,
        1056,
        5121,
        25830,
        29895,
        17572,
        21637,
        42346,
        46411,
        34088,
        38153,
        58862,
        62927,
        50604,
        54669,
        13907,
        9842,
        5649,
        1584,
        30423,
        26358,
        22165,
        18100,
        46939,
        42874,
        38681,
        34616,
        63455,
        59390,
        55197,
        51132,
        18628,
        22757,
        26758,
        30887,
        2112,
        6241,
        10242,
        14371,
        51660,
        55789,
        59790,
        63919,
        35144,
        39273,
        43274,
        47403,
        23285,
        19156,
        31415,
        27286,
        6769,
        2640,
        14899,
        10770,
        56317,
        52188,
        64447,
        60318,
        39801,
        35672,
        47931,
        43802,
        27814,
        31879,
        19684,
        23749,
        11298,
        15363,
        3168,
        7233,
        60846,
        64911,
        52716,
        56781,
        44330,
        48395,
        36200,
        40265,
        32407,
        28342,
        24277,
        20212,
        15891,
        11826,
        7761,
        3696,
        65439,
        61374,
        57309,
        53244,
        48923,
        44858,
        40793,
        36728,
        37256,
        33193,
        45514,
        41451,
        53516,
        49453,
        61774,
        57711,
        4224,
        161,
        12482,
        8419,
        20484,
        16421,
        28742,
        24679,
        33721,
        37784,
        41979,
        46042,
        49981,
        54044,
        58239,
        62302,
        689,
        4752,
        8947,
        13010,
        16949,
        21012,
        25207,
        29270,
        46570,
        42443,
        38312,
        34185,
        62830,
        58703,
        54572,
        50445,
        13538,
        9411,
        5280,
        1153,
        29798,
        25671,
        21540,
        17413,
        42971,
        47098,
        34713,
        38840,
        59231,
        63358,
        50973,
        55100,
        9939,
        14066,
        1681,
        5808,
        26199,
        30326,
        17941,
        22068,
        55628,
        51565,
        63758,
        59695,
        39368,
        35305,
        47498,
        43435,
        22596,
        18533,
        30726,
        26663,
        6336,
        2273,
        14466,
        10403,
        52093,
        56156,
        60223,
        64286,
        35833,
        39896,
        43963,
        48026,
        19061,
        23124,
        27191,
        31254,
        2801,
        6864,
        10931,
        14994,
        64814,
        60687,
        56684,
        52557,
        48554,
        44427,
        40424,
        36297,
        31782,
        27655,
        23652,
        19525,
        15522,
        11395,
        7392,
        3265,
        61215,
        65342,
        53085,
        57212,
        44955,
        49082,
        36825,
        40952,
        28183,
        32310,
        20053,
        24180,
        11923,
        16050,
        3793,
        7920
      };
            int num = (int)ushort.MaxValue;
            for (int index1 = 0; index1 < bytes.Length; ++index1)
            {
                int index2 = ((int)bytes[index1] ^ num >> 8) & (int)byte.MaxValue;
                num = intList[index2] ^ num << 8;
            }
            return Crc16.NumToHex((num ^ 0) & (int)ushort.MaxValue);
        }

        private static string NumToHex(int n) => n.ToString("X4").ToUpper();
    }
}
