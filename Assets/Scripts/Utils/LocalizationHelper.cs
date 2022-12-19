using System.Collections.Generic;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.SmartFormat.Extensions;
using UnityEngine.Localization.SmartFormat.PersistentVariables;

public class LocalizationHelper
{
    private const string GlobalVariablesGroupName = "global";

    public static VariablesGroupAsset GlobalVariables
    {
        get
        {
            if (_globalVariablesGroup == null)
            {
                var source = LocalizationSettings.StringDatabase.SmartFormatter.GetSourceExtension<PersistentVariablesSource>();
                _globalVariablesGroup = source[GlobalVariablesGroupName];
            }
            return _globalVariablesGroup;
        }
    }
    private static VariablesGroupAsset _globalVariablesGroup;

    public static string GetLocalizedString(string key)
    {
        return LocalizationSettings.StringDatabase.GetLocalizedString(key);
    }

    // Метод получения строки с данными локальными переменными. Использовать только один раз для каждой строки,
    // так как медленно выполняется
    public static string GetLocalizedString(string key, Dictionary<string, IVariable> variables)
    {
        var locale = new LocalizedString(LocalizationSettings.StringDatabase.DefaultTable, key);
        foreach (var kp in variables)
            locale.Add(kp);
        return locale.GetLocalizedString();
    }

    public static LocalizedString MakeStringClone(LocalizedString localizedString)
    {
        return new LocalizedString(localizedString.TableReference, localizedString.TableEntryReference);
    }

    // Метод получает глобальную переменную. Если таковой не существует, то создает новую
    public static T GetGlobalVariable<T>(string name) where T : class, IVariable, new()
    {
        if (GlobalVariables.TryGetValue(name, out var value))
            return value as T;

        var variable = new T();
        GlobalVariables.Add(name, variable);
        return variable;
    }
}
