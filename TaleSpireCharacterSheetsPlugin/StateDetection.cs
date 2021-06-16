using BepInEx;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LordAshes
{
    public static class StateDetection
    {
        public static void Initialize(System.Reflection.MemberInfo plugin)
        {
            SceneManager.sceneLoaded += (scene, mode) =>
            {
                try
                {
                    if (scene.name == "UI")
                    {
                        TextMeshProUGUI betaText = GetUITextByName("BETA");
                        if (betaText)
                        {
                            betaText.text = "INJECTED BUILD - unstable mods";
                        }
                    }
                    else
                    {
                        TextMeshProUGUI modListText = GetUITextByName("TextMeshPro Text");
                        if (modListText)
                        {
                            BepInPlugin bepInPlugin = (BepInPlugin)Attribute.GetCustomAttribute(plugin, typeof(BepInPlugin));
                            if (modListText.text.EndsWith("</size>"))
                            {
                                modListText.text += "\n\nMods Currently Installed:\n";
                            }
                            modListText.text += "\nLord Ashes' " + bepInPlugin.Name + " - " + bepInPlugin.Version;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.Log(ex);
                }
            };
        }

        public static TextMeshProUGUI GetUITextByName(string name)
        {
            TextMeshProUGUI[] texts = UnityEngine.Object.FindObjectsOfType<TextMeshProUGUI>();
            for (int i = 0; i < texts.Length; i++)
            {
                if (texts[i].name == name)
                {
                    return texts[i];
                }
            }
            return null;
        }
    }
}
