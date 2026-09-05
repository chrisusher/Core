using ChrisUsher.TradingStrat.Shared.DTOs.Plans;
using ChrisUsher.TradingStrat.Shared.Enums;

namespace ChrisUsher.TradingStrat.Shared.Data;

public static class StaticStrings
{
    private static readonly IEnumerable<string> _unlimitedFeatureTexts = new[]
    {
        "Unlimited Access to Strategies and Backtesting",
        "Unlimited Portfolios",
        "Unlimited AI Analysis",
        "Unlimited Screeners",
        "Unlimited Backtesting"
    };

    public static string LastSyncKeyName => "LAST_SYNC_DATE";

    public static List<string> GetFeatureText(Plan plan)
    {
        if (plan is null)
        {
            return new List<string>();
        }

        var features = new List<string>();

        if (plan.PlanType == PlanType.Unlimited)
        {
            features.AddRange(_unlimitedFeatureTexts);
        }
        else
        {
            foreach (var limit in plan.Limits)
            {
                var featureText = limit.FeatureType switch
                {
                    FeatureType.Portfolios => $"Up to {limit.Limit} Portfolios",
                    FeatureType.Backtests => $"{limit.Limit} Backtests/month",
                    FeatureType.AI_Analysis => $"{limit.Limit} AI Analysis/month",
                    FeatureType.Strategies => $"Up to {limit.Limit} Strategies",
                    FeatureType.Screeners => $"{limit.Limit} Screeners/month",
                    _ => $"{limit.Limit} {limit.FeatureType.GetDescription()}"
                };

                features.Add(featureText);
            }
        }

        if (plan.ExtraFeatures != null && plan.ExtraFeatures.Any())
        {
            features.AddRange(plan.ExtraFeatures);
        }

        return features;
    }
}
