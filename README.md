# BuffKit
The main Guns of Icarus mod.

A collection of game modifications that feature:
- Moderation/referee tools
- Quality of Life enhancements and bug fixes
- Misc. features

Created by Trgk and Ightrril. Dr. Pit Lazarus is the current developer.

## Features
Popular features include:
- Loadout Viewer: see ship and player loadouts all at once.
- Auto accept loadout recommendations and sort by preference.
- Skipping the Launcher and Intro movie.
- Better gun info tooltips and ship stat panels.

A complete description of all features will be added soon. In the meantime, you can learn some of the features from reading the [release notes](https://github.com/DrPitLazarus/buffkit/releases). Work-in-Progress [BuffKit Wiki](https://github.com/DrPitLazarus/buffkit/wiki).

## Install Options
1. [BuffKit Mod Installer for Windows](BuffKitModInstaller/#readme)
2. Manual install from .zip file.
    1. Visit latest [release page](https://github.com/DrPitLazarus/buffkit/releases/latest) and download .zip file. Or [view all releases](https://github.com/DrPitLazarus/buffkit/releases).
    2. Open downloaded .zip file.
    3. Open your `Guns of Icarus Online` folder.
    4. Drag and drop .zip contents to your game folder, overwrite if prompted.
    5. **Additional steps for Linux**: Use Proton and add to your launch options:  
    `WINEDLLOVERRIDES="version=n,b" %command%`.

## Uninstall Options
1. Use either uninstall option in [BuffKit Mod Installer for Windows](BuffKitModInstaller/#readme).
2. Manual uninstall but keep mod settings:
   - Delete file `Guns of Icarus Online\BepInEx\plugins\BuffKit\BuffKit.dll`.
   - If you want to disable the mod, rename the file extension to not a .dll. For example: `BuffKit.dll.bk`
3. Manual clean uninstall:
   - Delete folder `Guns of Icarus Online\BepInEx\plugins\BuffKit`.
   - If you want to backup the folder, the backup cannot live in the `Guns of Icarus Online\BepInEx\plugins` folder. The mod loader BepInEx will load any .dll files in there. I would right-click the `BuffKit` folder and compress to a .zip file.
4. Manual complete mod loader uninstall:
    - Delete folder `Guns of Icarus Online\BepInEx`. Make a backup if there is any mod settings you want to keep.
    - Delete files `changelog.txt doorstop_config.ini version.dll` in `Guns of Icarus Online` folder.

# Developer Notes
## Building and installing
Dr. Pit Lazarus is using Visual Studio 2022 as his IDE. 
Any code editor should work if you can use build configs `Release to GameDir` and `Release to .zip`.

Clone the repo and open up the BuffKit solution.

Edit `.\BuffKit\GamePath.txt` to point to your game folder.  
Example: `C:\Program Files (x86)\Steam\steamapps\common\Guns of Icarus Online`.

This project does not use reflection, but publicizes game assemblies with [BepInEx.AssemblyPublicizer.MSBuild](https://github.com/BepInEx/BepInEx.AssemblyPublicizer). Simply add `Publicize="true"` to the `Reference` tag in `BuffKit.csproj`.

To build BuffKit, use these build configs: 

`Release to GameDir`: Reads `.\BuffKit\GamePath.txt` and copies it over.

`Release to .zip`: Outputs `.\BuffKit\bin\BuffKit_SCS_$(Version).zip`.

You may see reference errors after cloning. Ensure `.\BuffKit\GamePath.txt` is correct and build the project. The InitialTarget `CopyAssemblies` will copy assembiles before the build process starts.  
All build configs will stage the mod directory in `.\BuffKit\bin\temp\`. 
It will download BepInEx core files, and copy BuffKit.dll and assets. 
See `.\BuffKit\BuffKit.csproj` for the exact steps.

**The BuffKit must grow.**

## Spanner
> [!WARNING]
> Deprecated. Replaced with [BepInEx.AssemblyPublicizer.MSBuild](https://github.com/BepInEx/BepInEx.AssemblyPublicizer).

~~Edit `.\Binaries\spanner_config.toml` and `.\BuffKit\GamePath.txt` to your game folder. 
Example `C:\Program Files (x86)\Steam\steamapps\common\Guns of Icarus Online`.~~

~~`./Binaries/Spanner.exe` is the tool that will copy the game assemblies to `.\Assemblies\`. 
It will deprivatize classes and methods so you can use them without needing to use reflection. 
Run it with terminal or run it normally and check the `spanner_log.txt` for errors. 
You need .NET 8 runtime to run it. 
You should only need to run it once, unless you make changes to the spanner config.~~

~~Nothing much here. Open the Spanner solution and use publish to build the single file Spanner.exe. 
Put the new .exe in `.\Binaries\`.~~
