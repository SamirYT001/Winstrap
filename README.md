# Winstrap


> [!IMPORTANT]
> Winstrap is still under a development. It can take a while!

Winstrap is **fork of Bloxstrap**. It is only supports a **Windows** PC's!

## How to get open source
You can get it using git commands. Link for [git](https://git-scm.com/).

```
C:\> git clone https://github.com/SamirYT001/Winstrap
```

Then wait until it downloads. Now open folder Winstrap and you have open source.
For open the code space, i using microsoft visual studio 2022. Just when you openned the folder it will be in the down. Just open file "Bloxstrap.sln".


If you need theres a list of useful directories for folders and others.

**Winstrap/Winstrap/Resources**: Basiclly its folder with all icons, cursors, shiftlocks and others. Also it have translations in it (Strings.resx and others).


**Winstrap/Images**: This is folder with logos of Bloxstrap, Winstrap.


**Winstrap/**: This is the main folder.


**Winstrap/Winstrap/UI**: This is folder which has UI in there. They are in xaml formats. Also they has viewmodels for checkboxes on options.


**Winstrap/Winstrap/Enums**: This is folder with bloxstraps ( or winstrap ) needed files.


**Winstrap/Winstrap/Enums/FlagPresets**: This is alternative folder for viewmodels but maded for fastflags Options.


**Winstrap/Winstrap/Intergrations/DiscordRichPresence.cs**: This is file that intergrates with discord, for a specific function. Discord RPC ( Rich Presence ) made for you to look for you and others people, that what are game they are playing in, and how much time. This file is made for roblox bootstrappers to give more statistics that what are you place playing, what you full name of account that you are playing in roblox ( for example bloxstrap code leaves comment on it: 
```
smallImageText = $"Playing on {userDetails.Data.DisplayName} (@{userDetails.Data.Name})"; // i.e. "axell (@Axelan_se)"          
```
)

### Compilating
**When you are finally do your code, make sure you selected in the top Release. Then press RMB onto your solution, and click build solution**. Now it gonna take a few seconds.**If compilation was successful then do this steps.

1. ** Go into your main folder( Winstrap/Winstrap ). **
2. ** Open "bin" folder. **
3. ** Then open "Release" folder. **
4. ** You will see a folder. It have a net, this is a windows library. On this folder you have your version (i have 6.0). And now just open folder nammed "net(your version)-windows" ( for me its net6.0-windows ). **
5. ** In it you seed like thousands of files. Just click random file one time. Now you need to select every. If you need just press Ctrl+A **
6. ** When you selected everything, now just zip it. For example i used WinRar, 7-Zip.**

When you done this steps you just have left to release it on github.

### Opening it
If you cant open it just download the file. Now if its normal archives like zip you can just open it with normal windows exctracter. For rar files you need [WinRAR](https://www.win-rar.com/). For 7zip files you need [7-Zip](https://www.7-zip.org/).

### Tools used for this project

**Microsoft Visual Studio 2022**: [Visual Studio](https://visualstudio.microsoft.com/)
**.Net Framework 6.0**: [.NET](https://dotnet.microsoft.com/)
**Git**: [Git](https://git-scm.com/)
**WinRar**: [WinRAR](https://www.win-rar.com/)
**7-Zip**: [7-Zip](https://www.7-zip.org/)
**SUBL**: [Sublime Text](https://www.sublimetext.com/)
**Windows 11**: Version 26H2
