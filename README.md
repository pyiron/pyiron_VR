# Virtual Atoms

This is a project which goal is to present atom structures in 3D with the HTC Vive and can be debugged with the Oculus Quest.

The project uses Pyiron to create and manage the structures and Unity to show the structures in Virtual Reality and receive the input from the user.

## Installation
### Installation of the server
*  Clone the pyiron repository
*  Go to the project on the command line and type in 
```
git fetch
git checkout vrplugin_branch3
git pull`
```
*  Go to ```~\pyiron_mpie\pyiron_mpie\vrplugin``` on the command line

### Create the sample structure
*  Type in the command line
    ```python FolderInitializer.py```

### Run the server
*  Type in the command line
    ```python echo-server.py```
*  This should print out the IP Adress on which the server is running

### Install the VR program on the Oculus Quest
*  Download SideQuest on the PC (e.g. https://sidequestvr.com/#/download)
*  Start SideQuest
*  Start the Quest and complete the room setup
*  Activate Developer Mode in the Quest, if not already done
*  On the PC, SideQuest should now show the Quest as connected
*  In the Rider in the Bottom-Left Corner click My Apps
*  On the top should be a big Banner "Installed Apps\nDrag and drop your APK/OBB files over this message to install!!!"
*  Download the apk <insert here when a build got uploaded to Git>
*  Drag the file over the banner on the Top of SideQuest

### To start the Quest Program
*  Put on the Oculus Quest and go to the Home Menu
*  Go to Navigate->Library->Unknown Sources->com.MPIE.VirtualAtoms

## Tutorial
### Connect with a server
Type in the IP Address the server is running on (e.g. by using the virtual Keyboard)

### Loading a structure
On the Canvas, all Folders and Structures inside the Structures Folder (~\pyiron_mpie\pyiron_mpie\vrplugin\Structures) should be shown.
Choose the structure you want to run, for the sample structure click on save->ham_lammps.

### Modes
There are mulltiple modes, which allow for different actions. The mode can be chosen on the Canvas.

### Changing the positions of atoms
In Temperature and Minimize Mode, atoms and the structure can be moved, using the controllers. The right controller is for the atom structure, the left for single atoms. When pointing towards the structure or an atom, a laser will be shown. When pressing the Trigger, the Laser will get red and the structure/atom attached to it. The Touchpad/Joystick can then be used to move the structure/atom towards or away from the user. When holding the controller inside the structure or an atom, and pressing the trigger, it will be attached to the controller and can be moved until the Trigger gets released.

### Changing the temperature
The temperature can only be changed in Temperature Mode. There are two ways to change the temperature. 
1.  Using the Canvas
2.  Using the Thermometer, by pointing a controller in its direction and pressing the trigger 

### Animation of the structure
In Temperature and Minimize Mode, a calculation how the atoms will behave in the current situation can be started and stopped
1.  using the UI
2.  by pressing the middle of the Touchpad  in the middle (Vive Controller)
3.  by pressing the Joystick down in the middle (Oculus Touch Controller/Oculus Quest Controller)

While the animation is running, the playback speed can be changed using the UI or by pressing the Touchpad/Joystick on the left and right side.

While the animation is stopped, it is possible to go one frame forward/backward by pressing the Touchpad/Joystick on the left and right side.

To go to the first frame of the animation, click Reset Animation on the UI.

### Deleting atoms
In Temperature and Minimize Mode, a Trashcan will appear when grabbing an atom. If the atom gets put into the Trashcan, it will be deleted.

### Creating new Atoms
-- Currently in Alpha, bugs can and will occur --
In Temperature and Minimize Mode, Atoms and structures can be created using the UI.
