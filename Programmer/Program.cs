using System;
using System.Collections.Generic;
using System.Linq;
using System.IO.Ports;          // For accessing the COM ports of the computer
using System.IO;                // For File I/O
using System.Text;
using System.Threading;         // For the Thread.Sleep function
using System.Globalization;     // For NumberStyles

namespace TestProgrammer
{
    class Program
    {
        static SerialPort ser;
        static string path = "COM14";
        
        // Declaration STK of constants
        #region stk500

        /* STK500 constants list, from AVRDUDE */
        const int STK_OK = 0x10;
        const int STK_FAILED = 0x11; // Not used
        const int STK_UNKNOWN = 0x12; // Not used
        const int STK_NODEVICE = 0x13; // Not used
        const int STK_INSYNC = 0x14; // ' '
        const int STK_NOSYNC = 0x15; // Not used
        const int ADC_CHANNEL_ERROR = 0x16; // Not used
        const int ADC_MEASURE_OK = 0x17; // Not used
        const int PWM_CHANNEL_ERROR = 0x18; // Not used
        const int PWM_ADJUST_OK = 0x19; // Not used
        const int CRC_EOP = 0x20; // 'SPACE'
        const int STK_GET_SYNC = 0x30; // '0'
        const int STK_GET_SIGN_ON = 0x31; // '1'
        const int STK_SET_PARAMETER = 0x40; // '@'
        const int STK_GET_PARAMETER = 0x41; // 'A'
        const int STK_SET_DEVICE = 0x42; // 'B'
        const int STK_SET_DEVICE_EXT = 0x45; // 'E'
        const int STK_ENTER_PROGMODE = 0x50; // 'P'
        const int STK_LEAVE_PROGMODE = 0x51; // 'Q'
        const int STK_CHIP_ERASE = 0x52; // 'R'
        const int STK_CHECK_AUTOINC = 0x53; // 'S'
        const int STK_LOAD_ADDRESS = 0x55; // 'U'
        const int STK_UNIVERSAL = 0x56; // 'V'
        const int STK_PROG_FLASH = 0x60; // '`'
        const int STK_PROG_DATA = 0x61; // 'a'
        const int STK_PROG_FUSE = 0x62; // 'b'
        const int STK_PROG_LOCK = 0x63; // 'c'
        const int STK_PROG_PAGE = 0x64; // 'd'
        const int STK_PROG_FUSE_EXT = 0x65; // 'e'
        const int STK_READ_FLASH = 0x70; // 'p'
        const int STK_READ_DATA = 0x71; // 'q'
        const int STK_READ_FUSE = 0x72; // 'r'
        const int STK_READ_LOCK = 0x73; // 's'
        const int STK_READ_PAGE = 0x74; // 't'
        const int STK_READ_SIGN = 0x75; // 'u'
        const int STK_READ_OSCCAL = 0x76; // 'v'
        const int STK_READ_FUSE_EXT = 0x77; // 'w'
        const int STK_READ_OSCCAL_EXT = 0x78; // 'x'

        #endregion
        
        // XBee Constants
        const byte DELIMITER = 126;     // decimal for 0x7E
        const byte LENGTH_UPPER = 0;    // decimal for 0x00
        const byte LENGTH_LOWER = 12;   // decimal for 0x0C
        const byte FRAME_TYPE = 00;     // decimal for 0x00, TX 64-bit request
        const byte FRAME_ID = 1;        // decimal for 0x01
        const byte OPTIONS = 0;         // decimal for 0x00, default options
        const byte FRAME_SUM = 155;     // decimal for 0x9B, sum of all the bytes before the payload

        static bool XBEE_PACKETS = true;
        
        static byte[] FRAME_HEADER = new byte[]{DELIMITER, LENGTH_UPPER, LENGTH_LOWER, FRAME_TYPE, FRAME_ID};
        static byte[] xBeeAddress = new byte[] {0x00, 0x13, 0xa2, 0x00, 0x40, 0x08, 0xd7, 0xc6 };

        static void Main(string[] args)
        {
            // Initialize serial port on which the Arduino is
            ser = new SerialPort(path, 115200, Parity.None, 8, StopBits.One);

            List<byte[]> programData = new List<byte[]>();
            List<byte[]> programAddresses = new List<byte[]>();

            readHexFile("Blank.hex", ref programData, ref programAddresses);
            programBoard(programAddresses, programData, false);

            //// A buffer to hold data being sent
            //byte[] buff;

            //// Open the port for transmission
            //ser.Open();

            //resetBoard();
            //waitForBootloader();
            //enterProgrammingMode();

            //// Code segment for GET_SYNC
            //buff = new byte[] { STK_GET_SYNC, CRC_EOP };

            //char ch = Console.ReadKey(true).KeyChar;

            //while (ch != 0x0d)
            //{
            //    writeToPort(new byte[]{ (byte) ch, CRC_EOP}, true);
            //    getResponse();
            //    ch = Console.ReadKey(true).KeyChar;
            //}

            //leaveProgrammingMode();

            Console.WriteLine("Done.");
            Console.ReadKey(true);
        }

        /// <summary>
        /// The bootloader sends a specific data packet when it starts up.
        /// This function waits until the packet has been received.
        /// </summary>
        /// <param name="print">Optionally prints data to console. Default is false.</param>
        /// <returns>Returns a bool which indicates if the datapacket was received.</returns>
        static bool waitForBootloader(bool print = true)
        {
            return (getResponse(print)[0] == 0x04);
        }

        /// <summary>
        /// Hard resets the board. Use this to enter the bootloader.
        /// </summary>
        /// <param name="print">Optionally prints data to console. Default is false.</param>
        static void resetBoard(bool print = true)
        {
            // Reset the board
            if (print)
                Console.WriteLine("Resetting the board...\n");
            ser.DtrEnable = !ser.DtrEnable;     // Toggle DTR
            ser.DtrEnable = !ser.DtrEnable;     // Toggle DTR
        }

        /// <summary>
        /// Stops the board from resetting automatically.
        /// </summary>
        /// <param name="print">Optionally prints data to console. Default is false.</param>
        static void enterProgrammingMode(bool print = true)
        {
            // Enter Programming Mode
            byte[] buff = new byte[] { STK_ENTER_PROGMODE, CRC_EOP };

            writeToPort(buff, XBEE_PACKETS);
            
            getResponse(print);
        }

        /// <summary>
        /// Soft resets the board. Use this to enter the application when the bootloader
        /// is active.
        /// </summary>
        /// <param name="print">Optionally prints data to console. Default is false.</param>
        static void leaveProgrammingMode(bool print = true)
        {
            // Enter Programming Mode
            byte[] buff = new byte[] { STK_LEAVE_PROGMODE, CRC_EOP };

            writeToPort(buff, XBEE_PACKETS);

            getResponse(print);
        }

        /// <summary>
        /// Use this function to send data to the board. It can be used to send data directly or
        /// by using XBee API frames.
        /// </summary>
        /// <param name="data">Byte array to write to the serial port.</param>
        /// <param name="xbee">Creates and sends XBee packets if set to true. Default is false.</param>
        /// <param name="print">Optionally prints data to console. Default is false.</param>
        static void writeToPort(byte[] data, bool xbee = false, bool print = true)
        {
            if (xbee)
            {
                int idx = 0;
                while (idx < data.Length)
                {
                    // Calculate the checksum for the packet
                    byte checksum = (byte)(0xFF - ((FRAME_SUM + data[idx]) & 0xFF));

                    // Write the entire frame header at once
                    ser.Write(FRAME_HEADER, 0, FRAME_HEADER.Length);
                    // Write the XBee address
                    ser.Write(xBeeAddress, 0, xBeeAddress.Length);
                    // Write the options byte
                    ser.Write(new byte[] { OPTIONS }, 0, 1);
                    // Write a single byte of data
                    ser.Write(data, idx, 1);
                    // Finish it off with the checksum
                    ser.Write(new byte[] { checksum }, 0, 1);

                    if (print)
                    {
                        Console.Write("Send:");
                        foreach (byte b in FRAME_HEADER)
                        {
                            Console.Write(" [{0:X2}] ", b);
                        }
                        foreach (byte b in xBeeAddress)
                        {
                            Console.Write(" [{0:X2}] ", b);
                        }

                        Console.Write(" [{0:X2}] ", OPTIONS);
                        Console.Write(" [{0:X2}] ", data[idx]);
                        Console.WriteLine(" [{0:X2}] ", checksum);
                    }

                    idx++;
                }
            }
            else
            {
                ser.Write(data, 0, data.Length);
                if (print)
                {
                    Console.Write("Send:");
                    for (int i = 0; i < data.Length; i++)
                        Console.Write(" [{0:X2}]", data[i]);
                    Console.WriteLine();
                }
            }
        }

        /// <summary>
        /// Gets response from the board and displays it
        /// </summary>
        /// <param name="print">Optionally prints data to console. Default is false.</param>
        /// <returns>Returns a list of bytes read from the port</returns>
        static List<byte> getResponse(bool print = true)
        {
            // TODO: Modify function to ignore 0x10 unless it is the end of the packet

            List<byte> response = new List<byte>(); // A list that holds all the received data
            byte rByte = 0xFF;          // Holds a byte of data recieved from the board
            long timer = (!XBEE_PACKETS) ? 2000000 : 5000000;       // Timer to stop app if nothing is received
 
            while (ser.BytesToRead == 0 && timer > 0)   // Wait for data to be available
                timer--;    // Decrement timer
            
            if (timer == 0)
            {
                Console.WriteLine("Timeout error. Press any key to exit...");
                Console.ReadKey();
                Environment.Exit(1);
            }

            // Makes sure there's no new data being written to the buffer
            int previousBytes = ser.BytesToRead;

            do 
            {
                previousBytes = ser.BytesToRead;
                Thread.Sleep(50);
            }
            while (previousBytes != ser.BytesToRead);

            Console.Write("Recv:");

            while (previousBytes-- > 0)
            {
                rByte = (byte)ser.ReadByte();                // Read the incoming byte
                response.Add(rByte);
                if (print)
                    Console.Write(" [{0:X2}]", rByte);  // Print the byte to the screen
            }

            Console.WriteLine();

            return response;
        }

        /// <summary>
        /// Programs the microcontoller
        /// </summary>
        /// <param name="path">The COM port at which the board is connected</param>
        /// <param name="addresses">List of new address packets</param>
        /// <param name="program">List of data to-be-written packets</param>
        /// <param name="print">Optionally prints data to console. Default is true.</param>
        static void programBoard(List<byte[]> addresses, List<byte[]> program, bool print = true)
        {
            // Open the port for transmission
            ser.Open();

            // Reset the board
            resetBoard(print);
            waitForBootloader(print);
            enterProgrammingMode(print);

            // Program the flash memory
            for (int idx = 0; idx < addresses.Count; idx++)
			{
			    // Change the current address to write data to
                writeToPort(addresses[idx], XBEE_PACKETS);

                if (print)
                {
                    Console.Write("Send:");
                    for (int i = 0; i < addresses[idx].Length; i++)
                        Console.Write(" [{0:X2}]", addresses[idx][i]);
                    Console.WriteLine();
                }

                getResponse();

                // Write the data to the flash memory
                writeToPort(program[idx], XBEE_PACKETS);
                if (print)
                {
                    Console.Write("Send:");
                    for (int i = 0; i < program[idx].Length; i++)
                        Console.Write(" [{0:X2}]", program[idx][i]);
                    Console.WriteLine();
                }
                getResponse();
			}

            // Read program from the flash memory 

            //Console.WriteLine("\n=========Verification============\n");

            //for (int idx = 0; idx < addresses.Count; idx++)
            //{
            //    // Change the current address to write data to
            //    writeToPort(addresses[idx], 0, addresses[idx].Length);
            //    Console.Write("Send:");
            //    for (int i = 0; i < addresses[idx].Length; i++)
            //    {
            //        Console.Write(" [{0:X2}]", addresses[idx][i]);
            //    }
            //    Console.WriteLine();
            //    getResponse();

            //    // Write the data to the flash memory
            //    buff = new byte[] { STK_READ_PAGE, 0x00, 0x80, 0x46, CRC_EOP};
            //    writeToPort(buff.Length);
            //    Console.Write("Send:");
            //    for (int i = 0; i < buff.Length; i++)
            //    {
            //        Console.Write(" [{0:X2}]", buff[i]);
            //    }
            //    Console.WriteLine();
            //    getResponse();
            //}

            leaveProgrammingMode(print);

            // End communication
            ser.Close();
        }

        /// <summary>
        /// The following functions reads the provided hex files and writes
        /// </summary>
        /// <param name="path">The path to the hex file as string</param>
        /// <param name="programData">Reference to a list of byte arrays (List<byte[]>)
        /// to store retrieved data in.</param>
        /// <param name="programAddresses">eference to a list of byte arrays (List<byte[]>)
        /// to store addresses in.</param>
        /// <param name="print">Optionally prints data to console. Default is false.</param>
        static void readHexFile(string path, ref List<byte[]> programData, ref List<byte[]> programAddresses, bool print = false)
        {
            // The following list will contain the hex file in array
            // It will follow the same standard as that of the hex file CCAAAAXX...XX
            // As you can see above, it will not include the checksum or file type
            programData = new List<byte[]>();
            programAddresses = new List<byte[]>();

            StreamReader reader = new StreamReader(path);

            // Line read from the hex file
            string line = reader.ReadLine();

            while (line != null)
            {
                line = line.TrimStart(':');

                // Check the TT field to see if this is the last line of the file
                if (byte.Parse(line.Substring(6, 2), NumberStyles.HexNumber) != 0x00)
                    break;      // If it is the last line, don't read it and exit loop

                // If the program gets to this part, the then the current line has valid data
                // Declare a byte list for storing retrieved data
                List<byte> data = new List<byte>();
                
                // Declare a byte list for storing the retrieved address
                List<byte> address = new List<byte>();

                // Add commands to the front of the list
                data.Add(STK_PROG_PAGE);
                address.Add(STK_LOAD_ADDRESS);

                // Read AAAA field. We right shift it by one to get the word
                // address. It is the same as dividing by 2.
                int wordAddress = (int.Parse(line.Substring(2, 4), NumberStyles.HexNumber)) >> 1;

                address.Add((byte)(wordAddress & 0xFF));            // Lower byte of the address
                address.Add((byte)((wordAddress >> 8) & 0xFF));     // Upper byte of the address
                address.Add(CRC_EOP);                               // End of packet

                data.Add((byte)0x00);               // Add the second field here
                data.Add((byte)0x00);               // Initializes length field of the data packet
                data.Add((byte)0x46);

                byte accumulator = 0;               // Keeps track of total length of the data in the created packet

                // Read and create a packet of at most 128 bytes (page size)
                while (line != null && accumulator < 0x80)
                {
                    // Read CC field from the line
                    byte dataLength = byte.Parse(line.Substring(0, 2), NumberStyles.HexNumber);

                    accumulator += dataLength;      // Add new line's length to the length field

                    // Read XX...XX field
                    for (int i = 0; i < dataLength; i++)
                    {
                        data.Add(byte.Parse(line.Substring(i + i + 8, 2), NumberStyles.HexNumber));
                    }
                    
                    line = reader.ReadLine().TrimStart(':');

                    // Check the TT field to see if this is the last line of the file
                    if (byte.Parse(line.Substring(6, 2), NumberStyles.HexNumber) != 0x00)
                        break;      // If it is the last line, don't read it and exit loop
                }

                data[2] = accumulator;
                data.Add(CRC_EOP);                  // Add the CRC_EOP to end of the list

                // Add the data and address list to our programData and addresses lists
                programData.Add(data.ToArray());
                programAddresses.Add(address.ToArray());
            }

            //Print contents of the lists
            if (print)
            {
                Console.WriteLine("========== Program Data ==========\n");
                foreach (byte[] b in programData)
                {
                    for (int i = 0; i < b.Length; i++)
                    {
                        Console.Write("[{0:X2}] ", b[i]);
                    }
                    Console.WriteLine();
                }

                Console.WriteLine("\n========== Addresses ==========\n");
                foreach (byte[] b in programAddresses)
                {
                    for (int i = 0; i < b.Length; i++)
                    {
                        Console.Write("[{0:X2}] ", b[i]);
                    }
                    Console.WriteLine();
                }
            }
        }
    }
}
