using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Utilities
{
    public static class ObdValueParser
    {
        public const int DELIVERY_MAX_LENGTH = 18;
        public const decimal NET_WEIGHT_MAX = 999999999999999m;

        private static readonly Regex DigitsOnly = new Regex(@"^\d+$", RegexOptions.Compiled);
        private static readonly Regex CommaThousands = new Regex(@"^\d{1,3}(,\d{3})+$", RegexOptions.Compiled);

        /// <summary>
        /// Delivery chỉ gồm chữ số, tối đa 18 ký tự để luôn nằm trong phạm vi BIGINT.
        /// Nhận cả ô Excel kiểu số lẫn kiểu text (number stored as text).
        /// </summary>
        public static bool TryParseDelivery(object value, out long delivery)
        {
            delivery = 0;
            if (value == null) return false;

            if (value is double d)
            {
                if (d < 0 || d != Math.Floor(d) || d >= 1e18) return false;
                delivery = (long)d;
                return delivery > 0;
            }

            var text = value.ToString().Trim().Replace(" ", "");
            if (text.Length == 0 || text.Length > DELIVERY_MAX_LENGTH || !DigitsOnly.IsMatch(text)) return false;

            return long.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out delivery) && delivery > 0;
        }

        /// <summary>
        /// Net Weight giữ phần thập phân (tối đa 3 số lẻ), không âm.
        /// Text dạng "7,000.000" (dấu phẩy ngăn nghìn, dấu chấm thập phân) hoặc "7.000,5" đều đọc được.
        /// </summary>
        public static bool TryParseNetWeight(object value, out decimal netWeight)
        {
            netWeight = 0;
            if (value == null) return false;

            if (value is double d)
            {
                if (double.IsNaN(d) || double.IsInfinity(d)) return false;
                netWeight = Math.Round((decimal)d, 3);
                return netWeight >= 0 && netWeight <= NET_WEIGHT_MAX;
            }

            var text = value.ToString().Trim().Replace(" ", "");
            if (text.Length == 0) return false;

            var lastComma = text.LastIndexOf(',');
            var lastDot = text.LastIndexOf('.');
            if (lastComma >= 0 && lastDot >= 0)
            {
                text = lastDot > lastComma
                    ? text.Replace(",", "")
                    : text.Replace(".", "").Replace(",", ".");
            }
            else if (lastComma >= 0)
            {
                text = CommaThousands.IsMatch(text) ? text.Replace(",", "") : text.Replace(",", ".");
            }

            if (!decimal.TryParse(text, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var parsed))
                return false;

            netWeight = Math.Round(parsed, 3);
            return netWeight >= 0 && netWeight <= NET_WEIGHT_MAX;
        }
    }
}
