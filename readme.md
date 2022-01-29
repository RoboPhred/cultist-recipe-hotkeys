# Cultist Recipe Hotkeys

This is a mod for Cultist Simulator that adds the ability to create hotkeys for "recipes" inside situation windows.
This allows you to hotkey a combination of cards in a situation, and immediately restore or start the combination at the press of a button.

Download the latest release [here](https://github.com/RoboPhred/cultist-recipe-hotkeys/releases/).

## Supported Versions

This mod supports Cultist Simulator `2021.1.b.2`.

## Usage

To store a recipe, open a situation window and arrange the cards how you want them. Then, press Ctrl+F1 to Ctrl+F12 to save this combination.

When the situation is idle, you can press the corresponding function key to immediately start the situation operating with those cards.

If the game is unsuccessful at starting your recipe, the sitation window will open automatically.
Note that it may successfully start without all of the cards in place, if the recipe allows for it. This is useful in cases such as Painting,
where not all of the "Art or Bread" cards are required.

If you want to restore the cards without starting the situation, use Shift+F1 to Shift+F12. This will populate the window
with the cards you specified and attempt to start the situation.

At the moment, this mod does not support storing ongoing actions. If your recipe requires additional cards to be added after starting, you
must add those cards yourself.

You can change the hotkeys hotkeys if you prefer different keys. See [configuration](#configuration).

## Installation

This mod uses BepInEx 5.2.

- Install BepInEx x86 [version 5.2](https://github.com/BepInEx/BepInEx/releases/tag/v5.2) or later by extracting the zip file into your Cultist Simulator install location
- Run the game once, to let BepInEx create its folder structure.
- Place the cultist-recipe-hotkeys.dll file from the download into `Cultist Sumulator/BepInEx/Plugins`

## Configuration

The hotkeys used to learn, restore, and execute recipes can be modified. After running the mod once, look in `Cultist Simulator/BepInEx/Config` for the config file
`net.robophreddev.CultistSimulator.CultistRecipeHotkeys.cfg`. Inside you will find the bindings. A list of valid keys can be found [here](https://docs.unity3d.com/ScriptReference/KeyCode.html).

### Troubleshooting

If the mod isn't working, you can turn on the BepInEx logs to see what is going on.

Open your BepInEx config file at `Cultist Simulator/BepInEx/config/BepInEx.cfg` and enable the console by changing the `Enabled` key of `[Logging.Console]` to `true`.

```
[Logging.Console]
Enabled = true
```

Doing this will create a terminal window when you launch cultist simulator. If you do not see this new window open, then BepInEx is probably not installed correctly,
or the config file is misconfigured.

Once you get the window, check its output after you hit the play button on the Cultist Simulator launcher. You can either drag the terminal window to another
monitor, or tab out of Cultist Simulator to check it after launch.

If BepInEx is installed and configured properly, you should see messages similar to the following:

```
[Message:   BepInEx] BepInEx 5.0.0.0 RC1 - cultistsimulator
[Message:   BepInEx] Compiled in Unity v2018 mode
[Info   :   BepInEx] Running under Unity v2019.1.0.2698131
[Message:   BepInEx] Preloader started
[Info   :   BepInEx] 1 patcher plugin(s) loaded
[Info   :   BepInEx] Patching [UnityEngine.CoreModule] with [BepInEx.Chainloader]
[Message:   BepInEx] Preloader finished
```

Once you have confirmed BepInEx is installed properly, look for the mod loading message. Once you start the game from the launcher, the terminal window should contain:

```
[Info   :   BepInEx] Loading [CultistRecipeHotkeys 0.0.1]
```

and

```
[Info   :CultistRecipeHotkeys] CultistRecipeHotkeys initialized.
```

If you do not see these lines, then the mod isn't in the correct folder. Check the Installation instructions for details on where to put the mod.

If you have confirmed all of the above and still are having trouble, try looking at the terminal for lines starting with `[Error :CultistRecipeHotkeys]`. The mod will
try to log errors when it cannot do it's job properly. Create a github issue with any CultistRecipeHotkeys error messages you find, and I will try to help you further.

## Development

### Dependencies

Project dependencies should be placed in a folder called `externals` in the project's root directory.
This folder should include:

- BepInEx.dll - Copied from the BepInEx 5.0 installation under `BepInEx/core`
- Assembly-CSharp.dll - Copied from `Cultist Simulator/cultistsimulator_Data/Managed`
- UnityEngine.CoreModule.dll - Copied from `Cultist Simulator/cultistsimulator_Data/Managed`
- UnityEngine.UI.dll - Copied from `Cultist Simulator/cultistsimulator_Data/Managed`
- UnityEngine.dll - Copied from `Cultist Simulator/cultistsimulator_Data/Managed`
- UnityEngine.InputLegacyModule.dll - Copied from `Cultist Simulator/cultistsimulator_Data/Managed`

### Compiling

This project uses the dotnet cli, provided by the .Net SDK. To compile, simply use `dotnet build` on the project's root directory.
