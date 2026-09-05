namespace ChrisUsher.Core.Shared.Converters;

public static class DecimalConverter
{
    public static decimal FromAbbreviatedString(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            throw new ArgumentException("Input cannot be null or whitespace.", nameof(input));
        }

        input = input.Trim().ToUpperInvariant();
        decimal multiplier = 1;

        if (input.EndsWith("T"))
        {
            multiplier = 1_000_000_000_000;
            input = input[..^1];
        }
        else if (input.EndsWith("B"))
        {
            multiplier = 1_000_000_000;
            input = input[..^1];
        }
        else if (input.EndsWith("M"))
        {
            multiplier = 1_000_000;
            input = input[..^1];
        }
        else if (input.EndsWith("K"))
        {
            multiplier = 1_000;
            input = input[..^1];
        }

        if (decimal.TryParse(input, out var number))
        {
            return number * multiplier;
        }

        throw new FormatException($"Input string '{input}' is not in a correct format.");
    }
}
