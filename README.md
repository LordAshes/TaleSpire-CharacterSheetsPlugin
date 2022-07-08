# Character Sheets Plugin

This unofficial TaleSpire plugin is for adding character sheets to TaleSpire.
Character sheet layout is completely customizable and supports any edition.
Clicking on entries on the character sheets initiates a correspoding roll using the Chat Roller plugin.

![Preview](https://i.imgur.com/qbfyu65.png)

Note: Preview shows a D&D 5E character sheet but any plugin allows defining system/edition character sheets.

## Change Log

```
2.0.0: Fix for BR HF Integration update.
1.3.1: Fixed issue with negative modifiers.
1.3.1: Added format checks and display messahes if roll format is not compatible with selected roll method.
1.3.0: Added option for ChatRoll based rolls (like before) or Talespire Dice rolls.
1.3.0: ChatRoll Plugin is no longer a forced dependency since you may use Talespire Dice instead.
1.2.0: Improved stats lookup so that partial names will not get replaced (e.g. WIS won't replace WIS_Save)
1.2.0: Migrted plugin from ChatRoller to ChatRoll.
1.1.1: Added Radial UI dependency.
1.1.0: Access is now available from the mini's radial menu using the Info icon and then Character Sheets icon.
1.0.1: Posted plugin on the TaleSpire main page
1.0.0: Initial release
```

## Install

Download and install using R2ModMan. Also download ChatRoll Plugin if you want to use Chat Rolls.

Sample character background, layout and character sheet is found in CustomData folder of the plugin.

## Usage

CharacterSheets allow different layouts to support more than one RPG edition at once. To select the desired edition
press the Character Sheet Style hotkey (default CTRL+I). Type the edition into the input box. This entry is used
as the prefix to the Character Sheet background file, the Character Sheet layout file and the Character Sheet data
files. Normally this feature is used to switch between different RPG edition like DnD5e, DnD3.5e, PathFinder, etc.
However, this technique can also be ued to switch between different Character Sheet layouts which as DnD5ECompact,
DnD5EFull, etc.

To open a character sheet, press the Open Character Sheet kotkey (default CTRL+O). This opens the Character Sheet
for the currently selected mini if one if present. The plugin uses the Creature Name for finding the corresponding
character sheet, so ensure that the Name is set correctly on the mini.

Once the character sheet opens, any of the entries can be "rolled" by clicking on them. Static entries likes Name,
Class, Race, Level, and Stats will just display the value to the chat (if open) and display the value in the mini's
speech bubble. For actual roll entries like stat modifiers, stat saves, skills, attacks and so on, the corresponding
roll is made and the results are displayed in the mini's speech bubble.

If trying the sample character sheet (after correctly installing the TaleSpire_CustomData folder as per the installation
finstructions), use CTRL+I to set the stype to Dnd5e, ensure that the mini's name is Jon, and then use CTRL+O to open
the sample character sheet. 

## Customizing

There are three files associated with a character sheet. One is character specific and contains all the character data
while the other two are edition specific.

### Character Sheets

Character sheets are text files with one key/value pair per line separated by a equal sign (=). They are located in the
TaleSpire_CustomData/Misc folder. The name of the file is *edition.name*.chs. For example: Dnd5E.Jon.chs
The contents of file is just a set of replacement that will be applied. For example, the entry "stealth=1D20+5" would
mean that when the user rolls stealth (by entering "/r stealth" into the chat) it will be replaced with "1D20+5" instead
and then the dice will be rolled. Replacements are made in the order they are listed.

### Character Sheet Layouts

Layout files indicated the layout and content displayed on the character sheet. Layout files are edition specific
with one layout file per edition. The name of the file is *edition*.CharacterSheetLayout.json. For example:
Dnd5e.CharacterSheetLayout.json. The file is located in the TaleSpire_CustomData/Misc folder.
The contents of the file are an array of visual elements. Each element can have the following properties:

```
name: The name of the visual element. Determines what keyword to look up in the character sheet to use as the display value.
text: Text to be displayed before the character sheet lookup value (e.g. skill name before the skill modifier). Default blank.
roll: The entry types into the chat for rolling.
position: x,y coordinates of the element on the character sheet.
size: Font size to be used when displaying the text and value.
width: Number of characters that the text and value is set to. Text is left aligned, value is right aligned.
```

{USERSLOT*n*}, {USERSLOT*n*_NAME} and {USERSLOT*n*_ROLL} are special entries which allow the common character sheet layout to
be used be individual characters. A fighter may use the slots for various weapon attack rolls and damage rolls while a
magic user may use these slots for magic rolls. If a {USERSLOT*n*} is not defined, it is not displayed.

Use the sample Dnd5e.CharacterSheetLayout.json as template to customize the character sheet contents and layout.

### Character Sheet Backgrounds

Each edition is expected to have a corresponding PNG file which is used as the background of the character sheet.
Currently these files are edition (but not characters) specific. The file name is *edition*.CharacterSheet.PNG.
The file should be in the TaleSpire_CustomData/Images folder (not the TaleSpire_CustomData/Misc folder).
