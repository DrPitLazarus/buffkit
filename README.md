# Buff Kit
Guns of Icarus modding toolkit/moderation mod

## Building and installing
Clone the repo  
Add the path to your game and the names of classes you want to patch to Binaries/spanner_config.toml  
Run Binaries/Spanner.exe and check the log to make sure it ran without issues 

Copy Assemblies/Assembly-CSharp.dll to [game folder]/GunsOfIcarusOnline_Data/Managed, overwriting the existing one

Download BepInEx at https://github.com/BepInEx/BepInEx/releases/download/v5.4.13/BepInEx_x86_5.4.13.0.zip  
Extract the BepInEx archive into Steam/steamapps/common/Guns of Icarus Online/  
Rename winhttp.dll to version.dll  
Run the game to make it generate all the BepInEx stuff, then close it

Open the BuffKit project  
Build BuffKit  
Copy BuffKit.dll to Guns of Icarus Online/BepInEx/plugins/

Start the game and enjoy the fruits of your labour
