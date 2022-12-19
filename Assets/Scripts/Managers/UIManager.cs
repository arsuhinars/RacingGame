using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Managers/UI Manager")]
public class UIManager : MonoBehaviour
{
    // Ссылка на представителя класса одиночки
    public static UIManager Instance { get; private set; }

    [SerializeField] private UIMessage defaultUiMessage;
    [SerializeField] private UIDialog defaultUiDialog;
    [Space]
    [SerializeField] private SerializableDictionary<string, UIView> views;
    
    public UIMessage DefaultUIMessage => defaultUiMessage;
    public UIDialog DefaultUIDialog => defaultUiDialog;
    public string CurrentViewName { get; private set; }
    public UIView CurrentView { get; private set; }

    private readonly List<string> viewsHistory = new();

    private void Awake()
    {
        // Делаем класс одиночкой
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        else Instance = this;
    }

    // Изменить текущую видимость
    public void ChangeView(string name, object viewData)
    {
        if (name == CurrentViewName)
            return;

        if (CurrentView != null)
            CurrentView.Hide();

        CurrentViewName = name;
        CurrentView = views[name];
        CurrentView.viewData = viewData;
        CurrentView.Show();

        if (viewsHistory.Count > 0)
            viewsHistory[^1] = CurrentViewName;
        else
            viewsHistory.Add(CurrentViewName);
    }

    public void ChangeView(string name) => ChangeView(name, null);

    // Открыть видимость поверх текущей. Текущая добавляется в историю
    public void OpenView(string name, object viewData)
    {
        if (name == CurrentViewName)
            return;

        if (CurrentView != null)
            CurrentView.Hide();

        CurrentViewName = name;
        CurrentView = views[name];
        CurrentView.viewData = viewData;
        CurrentView.Show();
        
        var index = viewsHistory.FindIndex((val) => val == CurrentViewName);
        if (index >= 0)
            viewsHistory.RemoveAt(index);
        viewsHistory.Add(CurrentViewName);
    }

    public void OpenView(string name) => OpenView(name, null);

    // Вернуться назад по истории
    public void GoBack()
    {
        if (viewsHistory.Count < 2)
            return;

        CurrentView.Hide();

        CurrentViewName = viewsHistory[^2];
        CurrentView = views[CurrentViewName];
        CurrentView.Show();

        viewsHistory.RemoveAt(viewsHistory.Count - 1);
    }

    // Скрыть все видимости и очистить всю историю
    public void HideAllViews()
    {
        foreach (var kv in views)
        {
            kv.Value.Hide();
        }
        viewsHistory.Clear();
        CurrentView = null;
        CurrentViewName = "";
    }
}
