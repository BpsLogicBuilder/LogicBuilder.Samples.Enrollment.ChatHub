using System;
using System.Diagnostics.CodeAnalysis;

namespace Enrollment.ChatHub
{
    [ExcludeFromCodeCoverage]
    internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
    {
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
    }
}
