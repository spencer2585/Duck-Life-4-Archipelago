using UnityEngine;
using UnityEngine.UI;
using DuckLife4Archipelago.Archipelago;

namespace DuckLife4Archipelago.UI;

public class ConnectionMenuManager : MonoBehaviour
{
    private GameObject _blocker;
    private GameObject _panel;

    private InputField _hostField;
    private InputField _slotField;
    private InputField _passwordField;

    public void Open()
    {
        if (_panel != null) return;
        BuildMenu();
    }

    private void BuildMenu()
    {
        Canvas canvas = FindObjectOfType<Canvas>();

        // Blocker — covers the whole screen, blocks clicks through to main menu
        _blocker = new GameObject("AP_Blocker");
        _blocker.transform.SetParent(canvas.transform, false);
        RectTransform blockerRect = _blocker.AddComponent<RectTransform>();
        blockerRect.anchorMin = Vector2.zero;
        blockerRect.anchorMax = Vector2.one;
        blockerRect.offsetMin = Vector2.zero;
        blockerRect.offsetMax = Vector2.zero;
        Image blockerImage = _blocker.AddComponent<Image>();
        blockerImage.color = new Color(0, 0, 0, 0.6f);

        // Panel
        _panel = new GameObject("AP_ConnectionPanel");
        _panel.transform.SetParent(_blocker.transform, false);
        RectTransform panelRect = _panel.AddComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(400f, 300f);
        panelRect.anchoredPosition = Vector2.zero;
        Image panelImage = _panel.AddComponent<Image>();
        panelImage.color = new Color(0.15f, 0.15f, 0.15f, 1f);

        // Title
        CreateLabel("Archipelago Connection", new Vector2(0f, 120f), new Vector2(380f, 30f), 18,
            TextAnchor.MiddleCenter);

        // Fields
        _hostField = CreateField("Host", new Vector2(0f, 60f), ArchipelagoClient.ServerData.Uri);
        _slotField = CreateField("Slot Name", new Vector2(0f, 5f), ArchipelagoClient.ServerData.SlotName);
        _passwordField = CreateField("Password", new Vector2(0f, -50f), ArchipelagoClient.ServerData.Password);

// Connect button
        CreateButton("Connect", new Vector2(-80f, -110f), () =>
        {
            ArchipelagoClient.ServerData.Uri = _hostField.text;
            ArchipelagoClient.ServerData.SlotName = _slotField.text;
            ArchipelagoClient.ServerData.Password = _passwordField.text;

            if (!string.IsNullOrWhiteSpace(_slotField.text))
                Plugin.ArchipelagoClient.Connect();
        });

// Close button
        CreateButton("Cancel", new Vector2(80f, -110f), Close);
    }

    private void CreateLabel(string text, Vector2 position, Vector2 size, int fontSize, TextAnchor alignment)
    {
        GameObject obj = new GameObject("Label_" + text);
        obj.transform.SetParent(_panel.transform, false);
        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
        Text label = obj.AddComponent<Text>();
        label.text = text;
        label.fontSize = fontSize;
        label.alignment = alignment;
        label.color = Color.white;
        label.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
    }

    private InputField CreateField(string labelText, Vector2 position, string defaultValue)
    {
        // Row container
        GameObject row = new GameObject("Row_" + labelText);
        row.transform.SetParent(_panel.transform, false);
        RectTransform rowRect = row.AddComponent<RectTransform>();
        rowRect.sizeDelta = new Vector2(370f, 30f);
        rowRect.anchoredPosition = position;

        // Label
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(row.transform, false);
        RectTransform labelRect = labelObj.AddComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0f, 0f);
        labelRect.anchorMax = new Vector2(0.35f, 1f);
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
        Text label = labelObj.AddComponent<Text>();
        label.text = labelText;
        label.fontSize = 14;
        label.alignment = TextAnchor.MiddleLeft;
        label.color = Color.white;
        label.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        // Input background
        GameObject inputObj = new GameObject("Input");
        inputObj.transform.SetParent(row.transform, false);
        RectTransform inputRect = inputObj.AddComponent<RectTransform>();
        inputRect.anchorMin = new Vector2(0.37f, 0f);
        inputRect.anchorMax = new Vector2(1f, 1f);
        inputRect.offsetMin = Vector2.zero;
        inputRect.offsetMax = Vector2.zero;
        Image inputImage = inputObj.AddComponent<Image>();
        inputImage.color = new Color(0.25f, 0.25f, 0.25f, 1f);

        // Text inside input
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(inputObj.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(5f, 0f);
        textRect.offsetMax = new Vector2(-5f, 0f);
        Text inputText = textObj.AddComponent<Text>();
        inputText.fontSize = 14;
        inputText.color = Color.white;
        inputText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        // InputField component
        InputField field = inputObj.AddComponent<InputField>();
        field.textComponent = inputText;
        field.text = defaultValue ?? "";

        return field;
    }

    private void CreateButton(string text, Vector2 position, UnityEngine.Events.UnityAction onClick)
    {
        GameObject obj = new GameObject("Button_" + text);
        obj.transform.SetParent(_panel.transform, false);
        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(120f, 35f);
        rect.anchoredPosition = position;
        Image image = obj.AddComponent<Image>();
        image.color = new Color(0.2f, 0.5f, 0.8f, 1f);
        Button button = obj.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(onClick);

        GameObject labelObj = new GameObject("Text");
        labelObj.transform.SetParent(obj.transform, false);
        RectTransform labelRect = labelObj.AddComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
        Text label = labelObj.AddComponent<Text>();
        label.text = text;
        label.fontSize = 14;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = Color.white;
        label.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
    }

    public void Close()
    {
        if (_blocker != null)
            Destroy(_blocker);
        _panel = null;
        _blocker = null;
    }
}