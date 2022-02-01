using UnityEngine;
using BepInEx;
using System.Linq;
using System.Collections.Generic;
using BepInEx.Configuration;
using System.Drawing;
using Newtonsoft.Json;
using System.Windows.Forms;
using System;
using System.Collections;
using System.Text.RegularExpressions;

namespace LordAshes
{

    [BepInPlugin(Guid, "Character Sheets Plug-In", Version)]
    [BepInDependency(RadialUI.RadialUIPlugin.Guid)]
    [BepInDependency(LordAshes.FileAccessPlugin.Guid)]
    public class CharacterSheetsPlugin : BaseUnityPlugin
    {
        // Plugin info
        public const string Guid = "org.lordashes.plugins.charactersheets";
        public const string Version = "1.4.0.0";

        // Configuration
        public enum RollMode
        {
            ChatRollMode = 0,
            TalespireDice = 1
        }
        private ConfigEntry<KeyboardShortcut> triggerKeyShow { get; set; }

        // Replacements
        Dictionary<string, string> replacements = null;

        // Edition
        private string style = "Dnd5e".Replace(".","");

        // Cooldown
        private Tuple<CreatureGuid, int> showMenu = null;

        /// <summary>
        /// Function for initializing plugin
        /// This function is called once by TaleSpire
        /// </summary>
        void Awake()
        {
            UnityEngine.Debug.Log("Character Sheets Plugin: Active.");

            triggerKeyShow = Config.Bind("Hotkeys", "Open Character Sheet", new KeyboardShortcut(KeyCode.O, KeyCode.LeftControl));
            style = Config.Bind("Setting", "Edition", "Dnd5e").Value;

            // Add Info menu selection to main character menu
            RadialUI.RadialSubmenu.EnsureMainMenuItem(RadialUI.RadialUIPlugin.Guid + ".Info",
                                                        RadialUI.RadialSubmenu.MenuType.character,
                                                        "Info",
                                                        FileAccessPlugin.Image.LoadSprite("Images/Icons/Info.png")
                                                     );

            // Add Icons sub menu item
            RadialUI.RadialSubmenu.CreateSubMenuItem(RadialUI.RadialUIPlugin.Guid + ".Info",
                                                        "Character Sheet",
                                                        FileAccessPlugin.Image.LoadSprite("Images/Icons/CharacterSheet.png"),
                                                        Show,
                                                        true, () => { return LocalClient.HasControlOfCreature(new CreatureGuid(RadialUI.RadialUIPlugin.GetLastRadialTargetCreature())); }
                                                    );

            // Post plkugin on TaleSpire main page
            StateDetection.Initialize(this.GetType());
        }

        /// <summary>
        /// Function for determining if view mode has been toggled and, if so, activating or deactivating Character View mode.
        /// This function is called periodically by TaleSpire.
        /// </summary>
        void Update()
        {
            if(isBoardLoaded())
            {
                // Keyboard triggered character sheet
                if (triggerKeyShow.Value.IsUp())
                {
                    Show(LocalClient.SelectedCreatureId);
                }

                // Submenu triggered character sheet
                if (showMenu!=null)
                {
                    if(showMenu.Item2>0)
                    {
                        showMenu = new Tuple<CreatureGuid,int>(showMenu.Item1, showMenu.Item2 - 1);
                    }
                    else
                    {
                        Show(showMenu.Item1);
                        showMenu = null;
                    }
                }
            }
        }

        /// <summary>
        /// Method for radial menu selection which calls the Character Sheet show
        /// </summary>
        /// <param name="cid">Unused</param>
        /// <param name="menu">Unused</param>
        /// <param name="mmi">Unused</param>
        void Show(CreatureGuid cid, string menu, MapMenuItem mmi)
        {
            SystemMessage.DisplayInfoText("Character Sheets Plugin: Requesting Character Sheet...");
            showMenu = new Tuple<CreatureGuid, int>(cid,100);
        }

        /// <summary>
        /// Show character sheet
        /// </summary>
        void Show(CreatureGuid cid)
        {
            Debug.Log("Character Sheets Plugin: Creating Character Sheet...");
            CreatureBoardAsset selected = null;
            CreaturePresenter.TryGetAsset(cid, out selected);
            if(selected!=null)
            {
                Debug.Log("Creating Character Sheet For '"+selected.Creature.Name+"'");
                System.Windows.Forms.Form sheet = new System.Windows.Forms.Form();
                sheet.Name = "Character Sheet: " + GetCreatureName(selected);
                sheet.Text = "Character Sheet: " + GetCreatureName(selected);
                string location = FileAccessPlugin.File.Find("Images/" + style + ".CharacterSheet.png")[0];
                UnityEngine.Debug.Log("Character Sheets Plugin: Loading CharacterSheet Background from '" + location + "'");
                System.Drawing.Image sheetImage = new System.Drawing.Bitmap(location);
                sheet.BackgroundImage = sheetImage;
                sheet.Width = sheetImage.Width+15;
                sheet.Height = sheetImage.Height+30;
                sheet.Left = (UnityEngine.Screen.width - sheet.Width) / 2;
                sheet.Top = (UnityEngine.Screen.height - sheet.Height) / 2;
                replacements = new Dictionary<string, string>();
                location = FileAccessPlugin.File.Find("Misc/" + style + "." + GetCreatureName(selected) + ".chs")[0];
                UnityEngine.Debug.Log("Character Sheets Plugin: Loading CharacterSheet Data from '" + location +"'");
                string[] keyvals = System.IO.File.ReadAllLines(location);
                foreach (string keyval in keyvals)
                {
                    string[] parts = keyval.Split('=');
                    if (parts.Count() == 2)
                    {
                        replacements.Add(parts[0], parts[1]);
                    }
                }
                location = FileAccessPlugin.File.Find("Misc/" + style + ".CharacterSheetLayout.json")[0];
                UnityEngine.Debug.Log("Character Sheets Plugin: Loading CharacterSheet Layout from '" + location +"'");
                string json = System.IO.File.ReadAllText(location);
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
                    sheet.SendToBack();
                }
            }
        }

        private string GetCreatureName(CreatureBoardAsset asset)
        {
            string name = asset.Creature.Name;
            if (name.Contains("<size=0>")) { name = name.Substring(0, name.IndexOf("<size=0>")).Trim(); }
            return name;
        }

        private void LinkClick(Element el, CreatureBoardAsset selected)
        {
            try
            {
                if (el.roll.StartsWith("/"))
                {
                    ChatManager.SendChatMessage(el.roll, selected.Creature.CreatureId.Value);
                }
                else
                {
                    string expandedRoll = "{" + el.roll + "}";

                    Debug.Log("Character Sheet Plugin: Roll = " + expandedRoll);

                    expandedRoll = MakeReplacements("{" + el.roll + "}");
                    if (expandedRoll == "{" + el.roll + "}") { expandedRoll = MakeReplacements(el.roll); }

                    Debug.Log("Character Sheet Plugin: Rolling '" + el.text.Replace(" ", " ") + " " + expandedRoll + "'");
                    if (Config.Bind("Settings", "Roll Method", RollMode.ChatRollMode).Value == RollMode.ChatRollMode)
                    {
                        Debug.Log("Character Sheet Plugin: Processing Via Chat Roller");
                        ChatManager.SendChatMessage("/rn " + el.text.Replace(" ", " ") + " " + expandedRoll, selected.Creature.CreatureId.Value); // SPC => ALT255
                    }
                    else
                    {
                        Regex reg1 = new Regex(@"^[0-9]+D[0-9]+[\+\-][0-9]+[\+\-][0-9]+$");
                        Regex reg2 = new Regex(@"^[0-9]+D[0-9]+[\+\-][0-9]+$");
                        Regex reg3 = new Regex(@"^[0-9]+$");
                        if (reg1.IsMatch(expandedRoll))
                        {
                            SystemMessage.DisplayInfoText("Talespire Dice Protocol\r\nSupports Only One Modifier.");
                            SystemMessage.DisplayInfoText("Please Fix Character Sheet For '" + (selected.Creature.Name + "<").Substring(0, (selected.Creature.Name + "<").IndexOf("<")) + "'");
                        }
                        else if (reg2.IsMatch(expandedRoll))
                        {
                            Debug.Log("Character Sheet Plugin: Processing Via Talespire Protocol");
                            string cmd = "talespire://dice/" + el.text.Replace(" ", " ") + ":" + expandedRoll;
                            System.Diagnostics.Process process = new System.Diagnostics.Process()
                            {
                                StartInfo = new System.Diagnostics.ProcessStartInfo()
                                {
                                    FileName = cmd,
                                    Arguments = "",
                                    CreateNoWindow = true
                                }
                            };
                            process.Start();
                        }
                        else if (reg3.IsMatch(expandedRoll))
                        {
                            SystemMessage.DisplayInfoText("Selected Stat Is Static Not A Roll.");
                            SystemMessage.DisplayInfoText(((el.text.Trim() != "") ? el.text.Trim() : el.roll.Replace("{", "").Replace("}", "")) + " is " + expandedRoll);
                        }
                        else
                        {
                            SystemMessage.DisplayInfoText("Roll '" + expandedRoll + "' Not Supported.");
                        }
                    }
                }
            }
            catch (Exception) { ; }
        }

        public string MakeReplacements(string expandedRoll)
        {
            while (expandedRoll.Contains("{"))
            {
                string key = expandedRoll.Substring(expandedRoll.IndexOf("{") + 1);
                key = key.Substring(0, key.IndexOf("}"));
                Debug.Log("Character Sheet: Key = " + key);
                bool found = false;
                foreach (KeyValuePair<string, string> spec in replacements)
                {
                    Debug.Log("Character Sheet: Key = '" + key + "' vs Stat '" + spec.Key + "'");
                    if ((spec.Key == key) || (spec.Key == ("{" + key + "}")))
                    {
                        expandedRoll = expandedRoll.Replace("{" + key + "}", spec.Value);
                        found = true;
                        break;
                    }
                }
                expandedRoll = expandedRoll.Replace("+-", "-").Replace("-+", "-").Replace("++", "+").Replace("--", "-");
                Debug.Log("Character Sheet Plugin: Roll = " + expandedRoll);
                if (!found) { break; }
            }
            return expandedRoll;
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

