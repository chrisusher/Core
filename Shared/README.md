# ChrisUsher.Core.Shared

Shared .NET utilities for JSON serialisation, date and time handling, currency formatting, and common data operations.

## Installation

```shell
dotnet add package ChrisUsher.Core.Shared
```

The package targets .NET 10.0.

## Included utilities

- `SharedCommon.JsonOptions` provides JSON serializer options with UTC `DateTime` and string-enum converters.
- `DateTimeHandler` and `UtcDateTimeConverter` standardize UTC date and time handling.
- `CurrencyLogic` and `CurrencyCode` format supported currency values.
- `DecimalConverter`, `EnumExtensions`, and `StaticData` provide common conversion and data helpers.

## Source and support

Source code and issue tracking are available at [github.com/chrisusher/Core](https://github.com/chrisusher/Core).
