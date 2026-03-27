using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using DuckLife4Archipelago.UI;

namespace DuckLife4Archipelago.Patches;

[HarmonyPatch(typeof(mainmenu), "Start")]
public class MainMenuPatches
{
    [HarmonyPostfix]
    static void Postfix()
    {
        GameObject settingsButtonObj = GameObject.Find("UI Main Menu/Main Menu Panel/Safe Area/b_setting/b_setting");
        if (settingsButtonObj == null)
        {
            Plugin.BepinLogger.LogWarning("MainMenuPatch: Could not find b_settings.");
            return;
        }

        // Clone settings button under the same parent
        GameObject apButtonObj = GameObject.Instantiate(settingsButtonObj, settingsButtonObj.transform.parent.parent);
        apButtonObj.name = "b_archipelago";

        // Position to the left of b_settings, matching the same y and spacing
        RectTransform apRect = apButtonObj.GetComponent<RectTransform>();
        apRect.anchoredPosition = new Vector2(280f, 45f);

        // Load the AP logo and apply it to the button image
        string imagePath = System.IO.Path.Combine(
            System.IO.Path.GetDirectoryName(typeof(Plugin).Assembly.Location),
            "color-icon.png"
        );

        if (System.IO.File.Exists(imagePath))
        {
            byte[] imageData = System.IO.File.ReadAllBytes(imagePath);
            Texture2D tex = new Texture2D(2, 2);
            ImageConversion.LoadImage(tex, imageData);
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            apButtonObj.GetComponent<Image>().sprite = sprite;
        }
        else
        {
            Plugin.BepinLogger.LogWarning("MainMenuPatch: archipelago.png not found next to DLL.");
        }
        // Clear the cloned onClick and wire up our placeholder
        Button apButton = apButtonObj.GetComponent<Button>();
        apButton.onClick.RemoveAllListeners();
        apButton.onClick = new Button.ButtonClickedEvent();
        apButton.onClick.AddListener(() =>
        {
            GameObject menuObj = new GameObject("AP_ConnectionMenu");
            ConnectionMenuManager menu = menuObj.AddComponent<ConnectionMenuManager>();
            menu.Open();
        });

        Plugin.BepinLogger.LogInfo("MainMenuPatch: Archipelago button added.");
    }
}