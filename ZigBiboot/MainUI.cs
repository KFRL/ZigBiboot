#define DEBUG

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Linq.Expressions;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO;

namespace Programmer
{
    public partial class MainUI : Form
    {
        static SerialPort ser;
        static string comPath = "COM27";
        static string defaultFilePath = "Blink.hex";
        static bool xbeePackets = true;

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
        #region XBee
        const byte ZB_LOAD_ADDRESS = 0x80;

        const byte DELIMITER = 126;     // decimal for 0x7E
        const byte LENGTH_UPPER = 0;    // decimal for 0x00
        const byte LENGTH_LOWER = 15;   // decimal for 0x0F
        const byte FRAME_TYPE = 16;     // decimal for 0x00, TX request
        const byte FRAME_ID = 1;        // decimal for 0x01
        const byte OPTIONS = 0;         // decimal for 0x00, default options
        const byte BROADCAST_RADIUS = 0;// decimal for 0x00

        const byte STATUS_PACKETSIZE = 11;
        const byte SEND_PACKETSIZE = 19;
        const byte RECV_PACKETSIZE = 17;

        const byte STATUS_FTYPE = 0x8B;
        const byte RECV_FTYPE = 0x90;

        const byte DELIVERY_STATUS_POS = 8;
        const byte DATA_POS = 17;
        #endregion

        // XBee variables
        #region Xbee_Vars
        static byte[] frameHeader = new byte[] { DELIMITER, LENGTH_UPPER, LENGTH_LOWER, FRAME_TYPE, FRAME_ID };
        static byte[] hostXBeeAddress64 = new byte[] { 0x00, 0x13, 0xA2, 0x00, 0x40, 0xAD, 0xBE, 0x87 };
        static byte[] targetXBeeAddress64 = new byte[] { 0x00, 0x13, 0xA2, 0x00, 0x40, 0xAD, 0xBE, 0xC7 };
        static byte[] xBeeAddress16 = new byte[] { 0xFF, 0xFE };
        static byte frameSum = 123;  // Used to store the sum of all bytes before payload

        static List<byte[]> xbeeBuffer = new List<byte[]>();
        #endregion

        public MainUI()
        {
            InitializeComponent();

            // Initializations
            foreach (byte b in targetXBeeAddress64)
            {
                targetZigBeeAddressTextBox.Text += b.ToString("X2");
            }
            foreach (byte b in hostXBeeAddress64)
            {
                hostZigBeeAddressTextBox.Text += b.ToString("X2");
            }

            comPortComboBox.Text = comPath;
            fileNameBox.Text = defaultFilePath;

            recalculateFrameSum();
        }

        private void browseBtn_Click(object sender, EventArgs e)
        {
#if DEBUG
            Debug.WriteLine("Browse button pressed.");
#endif
            DialogResult result = openHexFileDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                try
                {
                    fileNameBox.Text = openHexFileDialog.FileName;
                }
                catch (InvalidOperationException)
                {
                    MessageBox.Show("Invalid operation with result code " + result.ToString(), 
                        "IOException", MessageBoxButtons.OK);
#if DEBUG
                    Debug.WriteLine("Invalid operation with result code " + result.ToString());
#endif
                }
            }

        }

        private void comPortComboBox_Click(object sender, EventArgs e)
        {
            // Clear all data in the combo box and fetch COM port names to choose from
            comPortComboBox.Items.Clear();
            comPortComboBox.Items.AddRange(SerialPort.GetPortNames());
        }

        private void comPortComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Set COM port name
            comPath = comPortComboBox.SelectedItem.ToString();
#if DEBUG
            Debug.WriteLine(comPath + " selected");
#endif
        }

        private void updateAddressesBtn_Click(object sender, EventArgs e)
        {
            try
            {
                String targetAddress = targetZigBeeAddressTextBox.Text;
                String hostAddress = hostZigBeeAddressTextBox.Text;

                // TODO: add code to remove space characters from strings

                // Checks for invalid addresses
                if (targetAddress.Length != 16 ||
                   hostAddress.Length != 16)
                {
                    throw new Exception("Invalid address length.");
                }

                // Parse addresses from strings
                for (int idx = 0, j = 0; idx < 16; idx += 2, j++)
                {
                    // Target address
                    targetXBeeAddress64[j] = byte.Parse(targetAddress.Substring(idx, 2),
                        NumberStyles.HexNumber);

                    // Host address
                    hostXBeeAddress64[j] = byte.Parse(hostAddress.Substring(idx, 2),
                        NumberStyles.HexNumber);
                }

                recalculateFrameSum();
            }
            catch(Exception exception)
            {
                MessageBox.Show(exception.Message,
                        "Exception", MessageBoxButtons.OK);
#if DEBUG
                Debug.WriteLine(exception.Message);
#endif
            }

#if DEBUG
            Debug.Write("Host Address: ");
            foreach (byte b in hostXBeeAddress64)
            {
                Debug.Write(b.ToString("X2"));
            }
            Debug.Write("\nTarget Address: ");
            foreach (byte b in targetXBeeAddress64)
            {
                Debug.Write(b.ToString("X2"));
            }
            Debug.WriteLine("\nRecalculated frame sum: " + frameSum.ToString());
#endif

        }

        private void UploadBtn_Click(object sender, EventArgs e)
        {
            if (!backgroundWorker.IsBusy)
            {
                backgroundWorker.RunWorkerAsync();
                this.Enabled = false;
                // TODO: Show dialog box
            }
        }

        /// <summary>
        /// The bootloader sends a specific data packet when it starts up.
        /// This function waits until the packet has been received.
        /// </summary>
        /// <param name="print">Optionally prints data to Debug. Default is false.</param>
        /// <returns>Returns a bool which indicates if the datapacket was received.</returns>
        static bool waitForBootloader(bool print = true)
        {
            byte[] data = getResponse(print).ToArray();

            if (xbeePackets)
                return (data[data.Length - 2] == 0x04);
            else
                return (data[0] == 0x04);
        }

        /// <summary>
        /// Hard resets the board. Use this to enter the bootloader.
        /// </summary>
        /// <param name="print">Optionally prints data to Debug. Default is false.</param>
        static void resetBoard(bool print = true)
        {
            // Reset the board
            if (print)
            {
                Debug.WriteLine("Resetting the board...\n");
            }

            // Use remote AT commands to reset toggle DIO pin 1
            byte[] switchLow = new byte[] { 0x7E, 0x00, 0x10, 0x17, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFE, 0x02, 0x44, 0x31, 0x04, 0x48 };
            byte[] switchHigh = new byte[] { 0x7E, 0x00, 0x10, 0x17, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFE, 0x02, 0x44, 0x31, 0x05, 0x47 };

            // Load target address in to the command
            for (int addrIdx = 0; addrIdx < targetXBeeAddress64.Length; addrIdx++)
            {
                switchLow[addrIdx + 5] = targetXBeeAddress64[addrIdx];
                switchHigh[addrIdx + 5] = targetXBeeAddress64[addrIdx];
            }

            switchHigh[switchHigh.Length - 1] = calculateChecksum(switchHigh);
            switchLow[switchLow.Length - 1] = calculateChecksum(switchLow);

#if DEBUG
            Debug.Write("Switch Low: ");
            foreach (byte b in switchLow)
            {
                Debug.Write(b.ToString("X2") + " ");
            }
            Debug.Write("\nSwitch High: ");
            foreach (byte b in switchHigh)
            {
                Debug.Write(b.ToString("X2") + " ");
            }
            Debug.Write("\n");
#endif

            bool portClosed = !ser.IsOpen;
            if (portClosed) ser.Open();

            // TODO: Show reset messagebox

            ser.Write(switchLow, 0, switchLow.Length);
            getResponse(print);

            Thread.Sleep(2);

            ser.Write(switchHigh, 0, switchHigh.Length);
            getResponse(print);

            if (portClosed) ser.Close();
        }

        /// <summary>
        /// Stops the board from resetting automatically.
        /// </summary>
        /// <param name="print">Optionally prints data to Debug. Default is false.</param>
        static void enterProgrammingMode(bool print = true)
        {
            // Enter Programming Mode
            byte[] buff = new byte[] { STK_ENTER_PROGMODE, CRC_EOP };

            writeToPort(buff, xbeePackets);

            getResponse(print);
        }

        /// <summary>
        /// Soft resets the board. Use this to enter the application when the bootloader
        /// is active.
        /// </summary>
        /// <param name="print">Optionally prints data to Debug. Default is false.</param>
        static void leaveProgrammingMode(bool print = true)
        {
            // Enter Programming Mode
            byte[] buff = new byte[] { STK_LEAVE_PROGMODE, CRC_EOP };

            writeToPort(buff, xbeePackets);

            getResponse(print);
        }

        /// <summary>
        /// Use this function after every transaction to ensure the data reached its target.
        /// </summary>
        /// <param name="print">Optionally prints data to Debug. Default is false.</param>
        /// <returns>Returns whether the packet was dropped or corrupted.</returns>
        static bool sendFailure(bool print = true)
        {
            byte[] buff = new byte[RECV_PACKETSIZE];

            // Wait for data
            while (ser.BytesToRead < STATUS_PACKETSIZE) ;

            while (true)
            {
                for (int i = 0; i < 3; i++)
                {
                    buff[i] = (byte)ser.ReadByte();
                }

                ser.Read(buff, 3, buff[2] + 1);

                if (buff[3] == RECV_FTYPE)
                {
                    xbeeBuffer.Add(buff);
                    buff = new byte[RECV_PACKETSIZE];

                    // Wait for data
                    while (ser.BytesToRead < STATUS_PACKETSIZE) ;
                }
                else
                {
                    break;
                }
            }

            if (print)
            {
                Debug.Write("Recv:");
                foreach (byte b in buff)
                {
                    Debug.Write(" " + b.ToString("X2"));
                }
                Debug.WriteLine("");
            }

            switch (buff[DELIVERY_STATUS_POS])
            {
                case 0x24:
                    throw new Exception("Input XBee address not found.");
                case 0x00:
                    return false;
                default:
                    if (print) Debug.WriteLine("Transmission failed. Retrying");
                    return true;
            }
        }

        /// <summary>
        /// Use this function to send data to the board. It can be used to send data directly or
        /// by using XBee API frames.
        /// </summary>
        /// <param name="data">Byte array to write to the serial port.</param>
        /// <param name="xbee">Creates and sends XBee packets if set to true. Default is false.</param>
        /// <param name="print">Optionally prints data to Debug. Default is false.</param>
        static void writeToPort(byte[] data, bool xbee = false, bool print = true)
        {
            bool portClosed = !ser.IsOpen;

            if (portClosed) ser.Open();

            if (xbee)
            {
                int idx = 0;
                while (idx < data.Length)
                {
                    do
                    {
                        // Create packet for the a byte
                        byte[] packet = createPacket(data[idx]);

                        ser.Write(packet, 0, packet.Length);

                        if (print)
                        {
                            Debug.Write("Send:");

                            foreach (byte b in packet)
                            {
                                Debug.Write(" " + b.ToString("X2"));
                            }

                            //Debug.Write(" [{0:X2}]", packet[packet.Length - 2]);

                            Debug.WriteLine("");
                        }
                    }
                    // Resend data in case of failure
                    while (sendFailure(false));

                    idx++;
                }
            }
            else
            {
                ser.Write(data, 0, data.Length);
                if (print)
                {
                    Debug.Write("Send:");
                    for (int i = 0; i < data.Length; i++)
                        Debug.Write(" " + data[i].ToString("X2"));
                    Debug.WriteLine("");
                }
            }

            if (portClosed) ser.Close();
        }

        /// <summary>
        /// Create an XBee packet for the given byte.
        /// </summary>
        /// <param name="data">Byte of data to be transferred</param>
        /// <returns>XBee packet to be transferred.</returns>
        static byte[] createPacket(byte data)
        {
            byte[] packet = new byte[SEND_PACKETSIZE];
            int idx = 0;
            // Calculate the checksum for the packet
            byte checksum = (byte)(0xFF - ((frameSum + data) & 0xFF));

            // The frame header
            foreach (byte b in frameHeader)
            {
                packet[idx++] = b;
            }
            // The 64-bit XBee address
            foreach (byte b in targetXBeeAddress64)
            {
                packet[idx++] = b;
            }
            // The 16-bit XBee address
            foreach (byte b in xBeeAddress16)
            {
                packet[idx++] = b;
            }
            // The options byte
            packet[idx++] = BROADCAST_RADIUS;
            // The options byte
            packet[idx++] = OPTIONS;
            // Single byte of data
            packet[idx++] = data;
            // The checksum
            packet[idx++] = checksum;

            return packet;
        }

        /// <summary>
        /// Gets response from the board and displays it
        /// </summary>
        /// <param name="print">Optionally prints data to Debug. Default is false.</param>
        /// <returns>Returns a list of bytes read from the port</returns>
        static List<byte> getResponse(bool print = true)
        {
            List<byte> response = new List<byte>(); // A list that holds all the received data
            byte rByte = 0xFF;          // Holds a byte of data recieved from the board
            long timer = (!xbeePackets) ? 2000000 : 5000000;       // Timer to stop app if nothing is received
            bool dataInBuffer = false;

            // Check xbeeBuffer list for received responses
            if (xbeeBuffer.Count > 0)
            {
                dataInBuffer = true;

                foreach (byte[] buff in xbeeBuffer)
                {
                    foreach (byte b in buff)
                    {
                        response.Add(b);
                    }
                }

                xbeeBuffer.Clear();
            }

            // Open port if closed
            bool portClosed = !ser.IsOpen;
            if (portClosed) ser.Open();

            while (ser.BytesToRead == 0 && timer > 0)   // Wait for data to be available
                timer--;    // Decrement timer

            if (timer == 0 && !dataInBuffer)
            {
                throw new Exception("Timeout error. Device failed to respond");
            }

            // Makes sure there's no new data being written to the buffer
            int previousBytes = ser.BytesToRead;

            do
            {
                previousBytes = ser.BytesToRead;
                Thread.Sleep(50);
            }
            while (previousBytes != ser.BytesToRead);

            Debug.Write("Recv:");

            while (previousBytes-- > 0)
            {
                rByte = (byte)ser.ReadByte();                // Read the incoming byte
                response.Add(rByte);
                if (print)
                    Debug.Write(" " + rByte.ToString("X2"));  // Print the byte to the screen
            }

            Debug.WriteLine("");

            if (portClosed) ser.Close();

            return response;
        }

        /// <summary>
        /// Programs the microcontoller
        /// </summary>
        /// <param name="path">The COM port at which the board is connected</param>
        /// <param name="addresses">List of new address packets</param>
        /// <param name="program">List of data to-be-written packets</param>
        /// <param name="print">Optionally prints data to Debug. Default is true.</param>
        static void programBoard(List<byte[]> addresses, List<byte[]> program, bool print = true)
        {
            // Open the port for transmission
            bool portClosed = !ser.IsOpen;

            if (portClosed) ser.Open();

            // Reset the board
            resetBoard(print);
            waitForBootloader(print);
            enterProgrammingMode(print);

            //// Command for the zigbee address
            //byte[] addrCommand = new byte[10];
            //addrCommand[0] = ZB_LOAD_ADDRESS;
            //for (int i = 0; i < hostXBeeAddress64.Length; i++)
            //{
            //    addrCommand[i + 1] = hostXBeeAddress64[i];
            //}
            //addrCommand[9] = CRC_EOP;

            //// Change the host ZigBee address
            //writeToPort(addrCommand, xbeePackets, print);

            //getResponse(print);

            // Program the flash memory
            for (int idx = 0; idx < addresses.Count; idx++)
            {
                // Change the current address to write data to
                writeToPort(addresses[idx], xbeePackets, print);

                getResponse(print);

                // Write the data to the flash memory
                writeToPort(program[idx], xbeePackets, print);

                getResponse(print);
            }

            // Read program from the flash memory 

            //Debug.WriteLine("\n=========Verification============\n");

            //for (int idx = 0; idx < addresses.Count; idx++)
            //{
            //    // Change the current address to write data to
            //    writeToPort(addresses[idx], 0, addresses[idx].Length);
            //    Debug.Write("Send:");
            //    for (int i = 0; i < addresses[idx].Length; i++)
            //    {
            //        Debug.Write(" [{0:X2}]", addresses[idx][i]);
            //    }
            //    Debug.WriteLine();
            //    getResponse();

            //    // Write the data to the flash memory
            //    buff = new byte[] { STK_READ_PAGE, 0x00, 0x80, 0x46, CRC_EOP};
            //    writeToPort(buff.Length);
            //    Debug.Write("Send:");
            //    for (int i = 0; i < buff.Length; i++)
            //    {
            //        Debug.Write(" [{0:X2}]", buff[i]);
            //    }
            //    Debug.WriteLine();
            //    getResponse();
            //}

            leaveProgrammingMode(print);

            // End communication
            if (portClosed) ser.Close();
        }

        /// <summary>
        /// The following functions reads the provided hex files and writes
        /// </summary>
        /// <param name="path">The path to the hex file as string</param>
        /// <param name="programData">Reference to a list of byte arrays (List<byte[]>)
        /// to store retrieved data in.</param>
        /// <param name="programAddresses">eference to a list of byte arrays (List<byte[]>)
        /// to store addresses in.</param>
        /// <param name="print">Optionally prints data to Debug. Default is false.</param>
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
                Debug.WriteLine("========== Program Data ==========\n");
                foreach (byte[] b in programData)
                {
                    for (int i = 0; i < b.Length; i++)
                    {
                        Debug.Write(" " + b[i].ToString("X2"));
                    }
                    Debug.WriteLine("");
                }

                Debug.WriteLine("\n========== Addresses ==========\n");
                foreach (byte[] b in programAddresses)
                {
                    for (int i = 0; i < b.Length; i++)
                    {
                        Debug.Write(" " + b[i].ToString("X2"));
                    }
                    Debug.WriteLine("");
                }
            }
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            // Initialize serial port on which the Arduino is
            ser = new SerialPort(comPath, 115200, Parity.None, 8, StopBits.One);

            // Open the port for transmission
            ser.Open();

            List<byte[]> programData = new List<byte[]>();
            List<byte[]> programAddresses = new List<byte[]>();

            readHexFile(fileNameBox.Text, ref programData, ref programAddresses);
            programBoard(programAddresses, programData);

            //resetBoard();
            //waitForBootloader();
            //enterProgrammingMode();

            //// A buffer to hold data being sent
            ////byte[] buff = new byte[] { STK_GET_SYNC, CRC_EOP };
            //byte[] buff = new byte[] { STK_PROG_PAGE, 0x00, 0xFF, 0x46, CRC_EOP };

            //char ch = Debug.ReadKey(true).KeyChar;

            //while (ch != 0x0d)
            //{
            //    //writeToPort(new byte[] { (byte) ch }, true);
            //    writeToPort(buff, true);
            //    getResponse();
            //    ch = Debug.ReadKey(true).KeyChar;
            //}

            //leaveProgrammingMode();

            //while (Debug.ReadKey(true).KeyChar == 0x0d)
            //{
            //    resetBoard();
            //}

            if (ser.IsOpen) ser.Close();
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Enabled = true;
        }

        private static byte calculateChecksum(byte[] packet)
        {
            byte sum = 0;
            int length = packet[2];
            
            for (int i = 0; i < length; i++)
            {
                sum += packet[i + 3];
            }

            return (byte) (0xFF - sum);
        }

        private static void recalculateFrameSum()
        {
            // Recalculate frameSum
            frameSum = 0;
            frameSum += FRAME_TYPE;
            frameSum += FRAME_ID;

            // The 64-bit XBee address
            foreach (byte b in targetXBeeAddress64)
            {
                frameSum += b;
            }
            // The 16-bit XBee address
            foreach (byte b in xBeeAddress16)
            {
                frameSum += b;
            }
            // The options byte
            frameSum += BROADCAST_RADIUS;
            // The options byte
            frameSum += OPTIONS;
        }
    }
}
