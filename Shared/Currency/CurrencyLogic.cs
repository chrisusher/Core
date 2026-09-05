namespace ChrisUsher.Core.Shared.Currency;

public static class CurrencyLogic
{
    /// <summary>
    /// Gets the currency symbol for the specified currency code.
    /// </summary>
    /// <param name="currencyCode">The currency code to get the symbol for.</param>
    /// <returns>The currency symbol string.</returns>
    public static string GetCurrencySymbol(CurrencyCode currencyCode)
    {
        return currencyCode switch
        {
            CurrencyCode.USD => "$",
            CurrencyCode.EUR => "€",
            CurrencyCode.GBP => "£",
            CurrencyCode.GBX => "p", // Pence
            CurrencyCode.CAD => "C$",
            CurrencyCode.CHF => "CHF",
            _ => "¤" // Generic currency symbol for unknown currencies
        };
    }

    /// <summary>
    /// Formats a price with the appropriate currency symbol.
    /// </summary>
    /// <param name="price">The price to format.</param>
    /// <param name="currencyCode">The currency code to determine the symbol.</param>
    /// <param name="decimals">Number of decimal places to show (default: 2).</param>
    /// <returns>Formatted price string with currency symbol.</returns>
    public static string FormatPrice(decimal? price, CurrencyCode currencyCode, int decimals = 2)
    {
        if (!price.HasValue)
        {
            return "-";
        }

        var symbol = GetCurrencySymbol(currencyCode);
        var roundedPrice = Math.Round(price.Value, 2);

        // Use "N" numeric format to include thousands separators when > 999.
        // Specify InvariantCulture so the grouping separator is ',' consistently.
        var formatString = $"N{decimals}";
        var formatted = roundedPrice.ToString(formatString, CultureInfo.InvariantCulture);

        return currencyCode switch
        {
            CurrencyCode.CHF => $"{formatted} {symbol.Trim()}",
            _ => $"{symbol}{formatted}"
        };
    }

    public static int CountDecimalPlaces(double number)
    {
        var currencyString = number.ToString();

        if (!currencyString.Contains('.'))
        {
            return 0;
        }

        var afterDecimalPlace = currencyString.Substring(currencyString.IndexOf('.') + 1);

        return afterDecimalPlace.Length;
    }
}