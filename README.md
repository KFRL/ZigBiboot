# ZigBiboot (Alpha)

Wirelessly Programmming Arduino through ZigBee modules

- Currently only supports ATMega328 based boards (Arduino UNO)

#### How to use

1)  Burn the bootloader into the mirocontroller using any means of writing the flash 
    memory (I use a USBasp programmer. You can find the instructions in the repository).
2)  Compile your Arduino program using the Arduino IDE and get the hex file. Sample hex files
    have been provided.
3)  Open up the programmer application, select the port the ZigBee module is connected to, enter
    target address and select the hex file you wish to program and click "Upload".
4)  Wait for the programming to complete and the board to reset. Done!
    
#### Future Updates

- Bug fixes
