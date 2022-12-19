using UnityEngine.Localization;
using UnityEngine.Localization.SmartFormat.Core.Extensions;

[DisplayName("Not NaN formatter")]
public class NotNaNFormatter : FormatterBase
{
    public override string[] DefaultNames => new string[] { "notnan", "notNaN" };

    public override bool TryEvaluateFormat(IFormattingInfo formattingInfo)
    {
        if (formattingInfo.CurrentValue is float value)
        {
            var format = formattingInfo.Format;
            var splits = format.Split('|', 1);
            if (splits.Count < 1)
                return false;

            if (!float.IsNaN(value))
            {
                formattingInfo.Write(splits[0], value);
            }
            else if (format.Items.Count >= 2)
            {
                formattingInfo.Write(splits[1], value);
            }
            return true;
        }
        return false;
    }
}