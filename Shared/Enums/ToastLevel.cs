using Newtonsoft.Json.Converters;

namespace ChrisUsher.Core.Shared.Enums;

[Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
public enum ToastLevel
{
    Info = 0,

    Success = 1,

    Warning = 2,

    Error = 3
}
