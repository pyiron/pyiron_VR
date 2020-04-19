# Virtual Atoms

This is a project which goal is to present atom structures in 3D with the HTC Vive and can be debugged with the Oculus Quest.

The project uses Pyiron to create and manage the structures and Unity to show the structures in Virtual Reality and receive the input from the user.

This is the repository for the Unity program. The repository for the server is https://gitlab.mpcdf.mpg.de/wuseling/virtualatomsserver.

## Installation
### Installation of the server
*  Clone the server repository https://gitlab.mpcdf.mpg.de/wuseling/virtualatomsserver
*  Go to the project on the command line and type in 
```
git fetch
git checkout master
git pull`
```
*  Go to ```~\vrplugin``` on the command line

### Create the sample structures
*  Type in the command line
    ```python FolderInitializer.py```

### Run the server
*  Type in the command line
    ```python Manager.py```
    or
*  Execute the ServerStarter.bat application
*  This should print out the IP Adress on which the server is running

### Install the VR program on the Oculus Quest
*  Download SideQuest on the PC (e.g. https://sidequestvr.com/#/download)
*  Start SideQuest
*  Start the Quest and complete the room setup
*  Activate Developer Mode in the Quest, if not already done
*  On the PC, SideQuest should now show the Quest as connected
*  In the Rider in the Bottom-Left Corner click My Apps
*  On the top should be a big Banner "Installed Apps\nDrag and drop your APK/OBB files over this message to install!!!"
*  Download the [latest Oculus Quest Build](Builds/QuestBuild.apk)
*  Drag the file over the banner on the Top of SideQuest

### To start the Quest Program
*  Put on the Oculus Quest and go to the Home Menu
*  Go to Navigate->Library->Unknown Sources->aaa.MPIE.VirtualAtoms

## Tutorial
### Connect with a server
Type in the IP Address the server is running on (e.g. by using the virtual Keyboard)

### Explorer
On the Canvas, all Folders and Structures inside the Structures Folder (~\pyiron_mpie\pyiron_mpie\vrplugin\Structures) should be shown.
Choose the path you want to go to, and choose the structure you want to see. The structure can then be modified in the Calculate or Structure menu.
In the future it will be possible to delete jobs here.

### Calculator
In the Calculator Menu, settings for the animation can be set, e.g. the amount of steps that should be printed, the temperature or the potential that should be used. The animation can then be calculated.

### Structure
In structure mode the structure can be modified or a new one can be created. Atoms can be moved and in the future deleted.

### Changing the positions of atoms
In Structure Mode, atoms can be moved, using the left controller. In all modes, the structure can be moved with the right controller.
When pointing towards the structure or an atom, a laser will be shown. When pressing the Trigger, the Laser will get red and the structure/atom attached to it. The Touchpad/Joystick can then be used to move the structure/atom towards or away from the user (not working with the Quest). When holding the controller inside the structure or an atom, and pressing the trigger, it will be attached to the controller and can be moved until the Trigger gets released.

### Changing the temperature
The temperature can only be changed in Temperature Mode. There are two ways to change the temperature. 
1.  Using the Canvas
2.  Using the Thermometer, by pointing a controller in its direction and pressing the trigger 
    When changing the temperature on the thermometer, pressing the joystick or touchpad up or down increases or decreases the temperature intervals.

### Animation of the structure (not up to date anymore)
In Temperature and Minimize Mode, a calculation how the atoms will behave in the current situation can be started and stopped
1.  using the UI
2.  by pressing the middle of the Touchpad  in the middle (Vive Controller)
3.  by pressing the Joystick down in the middle (Oculus Touch Controller/Oculus Quest Controller)

While the animation is running, the playback speed can be changed using the UI or by pressing the Touchpad/Joystick on the left and right side.

While the animation is stopped, it is possible to go one frame forward/backward by pressing the Touchpad/Joystick on the left and right side.

To go to the first frame of the animation, click Reset Animation on the UI.

### Deleting atoms (can't be done in the current version)
In Temperature and Minimize Mode, a Trashcan will appear when grabbing an atom. If the atom gets put into the Trashcan, it will be deleted.

### Creating new Atoms (can't be done in the current version)
-- Currently in Alpha, bugs can and will occur --
In Temperature and Minimize Mode, Atoms and structures can be created using the UI.


## Troubleshooting
### Connection errors
*  Make sure the Server and the Client (e.g. the Oculus Quest) are connected to the same network
*  It can help to mark the network as private, else the server might get blocked from the firewall
*  Make sure VPN is disabled or both Server and Client are connected to the same VPN Network
*  If the network has a 2.4Ghz band and a 5Ghz band, make sure both devices are connected to the same band

### General Errors
*  If the VR application shows an error, look at the output of the server to see more details about the error
