# Virtual Atoms

This is a project which goal is to visualize atom structures in 3D using VR. Furthermore, interactions with the structure are possible as well, and further interactions will be implemented in the future. The application is designed for the HTC Vive and can be run and debugged with the Oculus Quest.

The project uses Pyiron to create and manage the structures and Unity to show the structures in Virtual Reality and receive the input from the user.

This is the repository for the Unity program. The repository for the server is https://github.com/pyiron/pyiron_VR_server.

Previous versions of the server and the Unity application can be found on Gitlab:
* Server: https://gitlab.mpcdf.mpg.de/wuseling/virtualatomsserver.
* Application: https://gitlab.mpcdf.mpg.de/wuseling/virtualatoms

## Requirements
### Server

The server runs on windows and linux, but the preferred OS for pyiron is linux. MacOS was not yet tested. It does not any special hardware specs. However, to execute the server, a working version of pyiron needs to be installed. For the installation, see https://pyiron.readthedocs.io/en/latest/source/installation.html. It is possible and suggested to execute the server on the High Performance Cluster in Garching (Account needed). For this, the computer running the Unity application needs to be connected to VPN (see https://www.mpcdf.mpg.de/services/campus/vpn for installation on non-VR headsets or the description below for the Oculus Quest).

## Installation
### (Optional) Accessing the HPC cluster

It is possible to run the server on the HPC cluster. In this section I present how I connect to the cluster. Note, that this can change, and look at the official documentation if any errors occur or to look for the download of AnyConnect. 
* For connect, a VPN conncection with AnyConnect is needed. Using AnyConnect, connect to the server vpn.mpcdf.mpg.de. Type in your Username and Password for your Account on the HPC cluster in Garching. 
* After connecting, open a command prompt. Connect to the cluster using ssh with the command `ssh -L 8000:localhost:30000 <username>@cmti001.bc.rzg.mpg.de` and enter your password.
* You should now have a connection to the mpg cluster. You can now install the server on it. I think the newest version of pyiron is already installed, but I am not entirely sure.

### Installation of the server
*  Clone the server repository https://github.com/pyiron/pyiron_VR_server
*  To get the newest version, go to the project on the command line and type in 
`
git fetch
git checkout master
git pull
`
*  Go to `~\vrplugin` on the command line

### Create the sample structures
*  Type in the command line
    `python FolderInitializer.py`

### Run the server
*  Type in the command line
    `python Manager.py`
    or
*  Execute the ServerStarter.bat application
*  This should print out the IP Adress on which the server is running

### Install the VR program on the Oculus Quest
*  Download SideQuest on the PC (e.g. https://sidequestvr.com/#/download)
*  Start SideQuest
*  Start the Quest and complete the room setup
*  Activate Developer Mode in the Quest, if not already done
*  On the PC, SideQuest should now show the Quest as connected
*  Download the [latest Oculus Quest Build](Builds/QuestBuild.apk)
*  To install the application on the Quest, drag the .apk file into SideQUest. A pop-up "Drag file(s) here" will appear. Release the file over this popup. Alternatively, SideQuest has an option to install apks in the top-right taskbar when connected to the Quest, which can be used instead.

### (optional) Install VPN on the Oculus Quest
*  Make sure SideQuest is installed on the PC (or install it e.g. via https://sidequestvr.com/#/download)
*  Start SideQuest on the computer
*  Start the Quest and complete the room setup
*  On the PC, SideQuest should now show the Quest as connected
*  Download the AnyConnect .apk, e.g. from https://play.google.com/store/apps/details?id=com.cisco.anyconnect.vpn.android.avf
*  To install the application on the Quest, drag the .apk file into SideQUest. A pop-up "Drag file(s) here" will appear. Release the file over this popup. Alternatively, SideQuest has an option to install apks in the top-right taskbar when connected to the Quest, which can be used instead.
*  Wait until the apk has been installed

### (optional) Start VPN on the Oculus Quest (required each time after the Quest has been shut down)
*  Put on the Oculus Quest and go to the Home Menu
*  Go to Navigate->Library->Unknown Sources->com.???.AnyConnect
*  In the application, enter the server name (vpn.mpcdf.mpg.de) and connect with your account for the mpg cluster

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

### Server Errors
* Depending on the network the server is in, sometimes the error "OSError: [Errno 98] Address already in use" occurs. This happens when starting the server quickly after closing it the last time. Usually waiting for a few seconds resolves this error. If not, check that the last process is not running anymore, and if it is, kill it (See point 3).
* If the server crashed and does not terminate, it can be usually stooped using ctrl + c. If not, it can be killed using ctrl + \.
* In some cases, this is not possible. If the server is running remote with ssh on a linux system (e.g. the HPC cluster), it can be stopped by opening a second ssh connection. Calling `ps -u <username>` shows all your processes. There should be one python process, which is the one executing the server. Read out the PID of this process and terminate it, using `kill -9 <PID>`.
