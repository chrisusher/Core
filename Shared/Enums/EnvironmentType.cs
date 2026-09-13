using Newtonsoft.Json.Converters;

namespace ChrisUsher.Core.Shared.Enums;

[Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
public enum EnvironmentType
{
    Local = 0,

    Development = 1,

    Test = 2,

    Production = 3,

    Temp = 4,
}
