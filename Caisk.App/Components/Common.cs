using Microsoft.AspNetCore.Components;

namespace Caisk.App.Components;

public static class Common
{
    public static RenderFragment Date(DateTime dateTime) => builder =>
    {
        var localTime = dateTime.ToLocalTime();
        builder.OpenElement(0, "time");
        builder.AddAttribute(1, "datetime", localTime.ToString("G"));
        builder.AddAttribute(1, "title", localTime.ToString("F"));
        builder.AddContent(2, When(DateOnly.FromDateTime(localTime)));
        builder.CloseElement();
    };
    
    private static string When(DateOnly date)
    {
        var now = DateOnly.FromDateTime(DateTime.Today);
        var diff = date.DayNumber - now.DayNumber;
        var unit = "days";

        if (Math.Abs(diff) >= 14)
        {
            diff /= 7;
            unit = "weeks";
        }

        return diff switch {
            0 => "today",
            < 0 => $"{-diff} {unit} ago",
            > 0 =>  $"in {diff} {unit}" };
    }

}

