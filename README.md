# Remote Programmming Art
Change the default bootloader of Arduino to accommodate programming through ZigBees

#### How to use

1)  Change the ZigBee addresses in the Optiboot.c and Program.cs files.
2)  Burn the bootloader into the mirocontroller using any means of writing the flash 
    memory (I use a USBasp programmer. You can find the instructions in the repository).
3)  Compile your Arduino program using the Arduino IDE and get the hex file. Sample hex files
    have been provided.
4)  Currently there is no UI for the Programmer so you'll have to edit the Program.cs file
    and manually enter the path to the hex file like so "C:\\file.hex".
5)  Compile and run the program using Visual Studio. You'll have to manually reset the board
    when prompted. It will begin transferring the program.
    
#### Future Updates

- Make the ZigBee addressing dynamic so it can be changed at run time
- Create a UI
