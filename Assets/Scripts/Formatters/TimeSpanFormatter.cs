using System;
using UnityEngine.Localization;
using UnityEngine.Localization.SmartFormat.Core.Extensions;

[DisplayName("TimeSpan formatter")]
public class TimeSpanFormatter : FormatterBase
{
    public override string[] DefaultNames => new string[] { "time", "timeSpan" };

    public override bool TryEvaluateFormat(IFormattingInfo formattingInfo)
    {
        if (formattingInfo.CurrentValue is float value)
        {
            var timeSpan = TimeSpan.FromSeconds(value);

            formattingInfo.Write(timeSpan.ToString(formattingInfo.FormatterOptions));
            return true;
        }
        return false;
    }
}
