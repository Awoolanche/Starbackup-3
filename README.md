![Starbackup_title](https://github.com/user-attachments/assets/9a104ce1-1130-4409-86fa-7cb774973162)
Starbackup 3 is a simple backup tool for the game Starbound. It provides basic backup automation and archiving.

# Features

### Backup your files easily
Every backup creates a ZIP archive (named with the current date) with your ```storage```, internal ```mods``` folder (for mods added outside Steam) and a ```modlist``` from the Steam Workshop, containing mod IDs and links to every mod included.  
### Automate your backups 
The Settings window contains options for creating a backup automatically when the tool is opened, closing the tool when the backup is successfully created and launching the game when it finishes, allowing for smoother user experience.
### Uh... what else?
Not much, really. It's a simple tool and it's not meant to be complex and feature-heavy. More features listed below are planned for future releases.


### Planned
* Reading Steam Workshop mod titles from the page instead of unreliable .modinfo,
* More Starbound-like interface,
* Timed backups; backups will only be performed once in a time period specified by the user,
* Backup limit; size or quantity, also speficied by the user,
* Backup manager; listing and restoring backups from the inside the tool,
* Hanging furry dice air freshener.


# Installation
Download the latest release [here](https://github.com/Awoolanche/Starbackup-3/releases), unpack and open ```Starbackup 3.exe``` to run the application.

# How to Use
### Set Starbound Root Folder
When the tool is launched for the first time, it will ask the user to provide a path to the Starbound folder.

![rootfolder](https://github.com/user-attachments/assets/19caf2d2-e913-42a8-aa60-2485347fa085)

If you skip that part, use the "Select Starbound Root Folder" option located in Settings to define the main directory of your Starbound installation. The application will check for the required subfolders and executable.

![folder](https://github.com/user-attachments/assets/f6e291ac-e182-4360-a4b1-e5b8b981309a)

### Configure Steam Workshop Folder (Optional)
If you are using Steam Workshop mods, specify the path to your Steam Workshop Starbound content folder (e.g., steamapps\workshop\content\211820). This step is done automatically, unless the Steam installation directory is different than default.

![steam](https://github.com/user-attachments/assets/012507b3-aeb6-4814-96d6-ccd6afee61cf)

### Adjust Automation Settings
In the settings window, enable or disable automatic backup on startup and control whether Starbound should launch or the application should exit automatically after a backup. Those settings can also be changed from ```settings.json``` located in the Starbackup 3 installation folder.

![auto](https://github.com/user-attachments/assets/493969ba-0b9a-484b-bd8c-e57cb586a07e)

### Initiate Manual Backup
Click the "Create Backup" button to start a backup process immediately. The backup will appear in the backup folder in the Starbound directory.

![backup](https://github.com/user-attachments/assets/d869da23-3df4-4669-a1da-343b0c511462)


# Caution
Starbackup 3 is a C# rewrite of my old personal project. It is not meant to be used by wider public and issues you may encounter may not be fixed. Here be dragons.

# MIT License

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
