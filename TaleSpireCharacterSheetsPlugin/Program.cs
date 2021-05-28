using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using BepInEx;
using Bounce.Unmanaged;
using System.Linq;
using TMPro;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using System.Drawing;
using Newtonsoft.Json;
using System.Windows.Forms;

namespace LordAshes
{
    [BepInPlugin(Guid, "Character Sheets Plug-In", Version)]
    [BepInDependency(LordAshes.ChatRollerPlugin.Guid)]
    public class CharacterSheetsPlugin : BaseUnityPlugin
    {
        // Plugin info
        public const string Guid = "org.lordashes.plugins.charactersheets";
        public const string Version = "1.0.0.0";

        // Configuration
        private ConfigEntry<KeyboardShortcut> triggerKeyEdition { get; set; }
        private ConfigEntry<KeyboardShortcut> triggerKeyShow { get; set; }

        // Replacements
        Dictionary<string, string> replacements = null;

        // Content directory
        private string dir = UnityEngine.Application.dataPath.Substring(0, UnityEngine.Application.dataPath.LastIndexOf("/")) + "/TaleSpire_CustomData/";

        // Character sheets style
        private string style = "Default";

        /// <summary>
        /// Function for initializing plugin
        /// This function is called once by TaleSpire
        /// </summary>
        void Awake()
        {
            UnityEngine.Debug.Log("Lord Ashes Character Sheets Plugin Active.");

            if (!System.IO.Directory.Exists(dir + "Images/"))
            {
                System.IO.Directory.CreateDirectory(dir + "Images/");
            }
            if (!System.IO.Directory.Exists(dir + "Misc/"))
            {
                System.IO.Directory.CreateDirectory(dir + "Misc/");
            }

            triggerKeyEdition = Config.Bind("Hotkeys", "Select Character Sheet Edition", new KeyboardShortcut(KeyCode.I, KeyCode.LeftControl));
            triggerKeyShow = Config.Bind("Hotkeys", "Open Character Sheet", new KeyboardShortcut(KeyCode.O, KeyCode.LeftControl));

            LordAshes.ChatRollerPlugin.SetEdition(style);
        }

        /// <summary>
        /// Function for determining if view mode has been toggled and, if so, activating or deactivating Character View mode.
        /// This function is called periodically by TaleSpire.
        /// </summary>
        void Update()
        {
            if(isBoardLoaded())
            {
                if (triggerKeyEdition.Value.IsUp())
                {
                    SystemMessage.AskForTextInput("Character Sheet Style", "Please Enter The Edition:", "OK", (s)=> { if (s != "") { style = s + ".";  } else { style = ""; }; LordAshes.ChatRollerPlugin.SetEdition(s); }, null, "Cancel", null, "Default");
                }
                else if (triggerKeyShow.Value.IsUp())
                {
                    Show();
                }
            }
        }

        /// <summary>
        /// Show character sheet
        /// </summary>
        void Show()
        {
            CreatureBoardAsset selected = null;
            foreach(CreatureBoardAsset asset in CreaturePresenter.AllCreatureAssets.ToArray())
            {
                if((NGuid)LocalClient.SelectedCreatureId.Value==asset.Creature.CreatureId.Value)
                {
                    selected = asset;
                    break;
                }
            }
            if(selected!=null)
            {
                System.Windows.Forms.Form sheet = new System.Windows.Forms.Form();
                sheet.Name = "Character Sheet_" + selected.Creature.Name;
                sheet.Text = "Character Sheet: " + selected.Creature.Name;
                UnityEngine.Debug.Log("Loading CharacterSheet Background from '" + dir + "Images/" + style + "CharacterSheet.png'");
                System.Drawing.Image sheetImage = new System.Drawing.Bitmap(dir+"Images/"+style+"CharacterSheet.png");
                sheet.BackgroundImage = sheetImage;
                sheet.Width = sheetImage.Width+15;
                sheet.Height = sheetImage.Height+30;
                sheet.Left = (UnityEngine.Screen.width - sheet.Width) / 2;
                sheet.Top = (UnityEngine.Screen.height - sheet.Height) / 2;
                replacements = new Dictionary<string, string>();
                UnityEngine.Debug.Log("Loading CharacterSheet Data from '" + dir + "Misc/" + style + selected.Creature.Name + ".chs'");
                string[] keyvals = System.IO.File.ReadAllLines(dir+"Misc/"+style+selected.Creature.Name+".chs");
                foreach (string keyval in keyvals)
                {
                    string[] parts = keyval.Split('=');
                    if (parts.Count() == 2)
                    {
                        replacements.Add(parts[0], parts[1]);
                    }
                }
                UnityEngine.Debug.Log("Loading CharacterSheet Data from '" + dir + "Misc/" + style + "CharacterSheetLayout.json'");
                string json = System.IO.File.ReadAllText(dir + "Misc/" + style + "CharacterSheetLayout.json");
                Element[] contents = JsonConvert.DeserializeObject<Element[]>(json);
                foreach (Element el in contents)
                {
                    Label item = new Label();
                    item.Left = el.position.X;
                    item.Top = el.position.Y;
                    item.Font = new Font("Courier New", el.size);
                    item.TextAlign = ContentAlignment.MiddleRight;
                    item.AutoSize = true;
                    item.ForeColor = System.Drawing.Color.FromArgb(255, 0, 0, 0);
                    item.BackColor = System.Drawing.Color.FromArgb(16, 30, 30, 30);
                    string content = el.name;
                    foreach (KeyValuePair<string, string> rep in replacements)
                    {
                        content = content.Replace(rep.Key, rep.Value);
                        if (el.text.Contains("{")) { el.text = el.text.Replace(rep.Key, rep.Value); }
                        if (rep.Key.StartsWith("{USERSLOT")) { el.roll = el.roll.Replace(rep.Key, rep.Value); }
                    }
                    item.Click += (s, ev) => { LinkClick(el, selected); };
                    item.Enabled = true;
                    item.Visible = true;
                    item.AutoSize = true;
                    if (el.width <= 0)
                    {
                        item.Text = el.text + content;
                    }
                    else
                    {
                        int maxWidth = el.width - content.Length;
                        item.Text = (el.text + new string(' ', el.width)).Substring(0, maxWidth) + content;
                    }
                    if (item.Text.StartsWith("{USERSLOT")) { item.Text = ""; }
                    sheet.Controls.Add(item);
                    sheet.Enabled = true;
                    sheet.Visible = true;
                    sheet.Show();
                    sheet.BringToFront();
                }
            }
        }

        private void LinkClick(Element el, CreatureBoardAsset selected)
        {
            ChatManager.SendChatMessage("@ " + el.roll, selected.Creature.CreatureId.Value);
        }

        /// <summary>
        /// Function to check if the board is loaded
        /// </summary>
        /// <returns></returns>
        public bool isBoardLoaded()
        {
            return CameraController.HasInstance && BoardSessionManager.HasInstance && !BoardSessionManager.IsLoading;
        }

        /// <summary>
        /// Class for holding layout information
        /// </summary>
        public class Element
        {
            public string name { get; set; }
            public Point position { get; set; }

            public string text { get; set; } = "";
            public float size { get; set; } = 12;
            public string roll { get; set; } = "";
            public int width { get; set; } = -1;
        }

    }
}
