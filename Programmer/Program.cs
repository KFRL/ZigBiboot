﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO.Ports;
using System.Text;
using System.Threading;

namespace TestProgrammer
{
    class Program
    {
        // Declaration of constants
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
        
        static void Main(string[] args)
        {
            byte[] buff;    // Holds data to be transferred
            
            // Initialize serial port on which the Arduino is
            SerialPort ser = new SerialPort("COM13", 115200, Parity.None, 8, StopBits.One);

            // Open the port for transmission
            ser.Open();

            // Reset the board
            Console.WriteLine("Resetting the board...\n");
            ser.DtrEnable = !ser.DtrEnable;     // Toggle DTR
            ser.DtrEnable = !ser.DtrEnable;     // Toggle DTR
            //Thread.Sleep(10);                   // Adds a delay

            // Code segment for GETSYNC
            buff = new byte[2] { 0x30, 0x20 };

            for (int i = 0; i < 3; i++)
            {
                Console.WriteLine("Send: [{0:X2}] [{1:X2}]", buff[0], buff[1]);
                ser.Write(buff, 0, 2);
                Thread.Sleep(35);                  // Adds a delay
            }

            getResponse(ser);

            // Code segment for GET_PARAM 0x80
            buff = new byte[3] { 0x41, 0x80, 0x20 };
            
            ser.Write(buff, 0, 3);
            Console.WriteLine("Send: [{0:X2}] [{1:X2}] [{2:X2}]", buff[0], buff[1], buff[2]);

            getResponse(ser);

            // Code segment for GET_PARAM 0x81
            buff = new byte[3] { 0x41, 0x81, 0x20 };

            ser.Write(buff, 0, 3);
            Console.WriteLine("Send: [{0:X2}] [{1:X2}] [{2:X2}]", buff[0], buff[1], buff[2]);

            getResponse(ser);

            // Code segment for GET_PARAM 0x82
            buff = new byte[3] { 0x41, 0x82, 0x20 };

            ser.Write(buff, 0, 3);
            Console.WriteLine("Send: [{0:X2}] [{1:X2}] [{2:X2}]", buff[0], buff[1], buff[2]);

            getResponse(ser);

            // Code segment for GET_PARAM 0x98
            buff = new byte[3] { 0x41, 0x98, 0x20 };

            ser.Write(buff, 0, 3);
            Console.WriteLine("Send: [{0:X2}] [{1:X2}] [{2:X2}]", buff[0], buff[1], buff[2]);

            getResponse(ser);

            // TODO: Determine purpose of code Segment
            buff = new byte[] { 0x42, 0x86, 0x00, 0x00, 0x01, 0x01, 0x01, 0x01, 0x03, 0xff, 0xff, 0xff, 0xff, 0x00, 0x80, 0x04, 0x00, 0x00, 0x00, 0x80, 0x00, 0x20 };
            ser.Write(buff, 0, buff.Length);
            Console.Write("Send:");
            for (int i = 0; i < buff.Length; i++)
            {
                Console.Write(" [{0:X2}]", buff[i]);
            }
            Console.WriteLine();
            getResponse(ser);

            // TODO: Determine purpose of code Segment
            buff = new byte[] { 0x45, 0x05, 0x04, 0xd7, 0xc2, 0x00, 0x20 };
            ser.Write(buff, 0, buff.Length);
            Console.Write("Send:");
            for (int i = 0; i < buff.Length; i++)
            {
                Console.Write(" [{0:X2}]", buff[i]);
            }
            Console.WriteLine();
            getResponse(ser);

            // TODO: Determine purpose of code Segment
            buff = new byte[] { 0x50, 0x20 };
            ser.Write(buff, 0, buff.Length);
            Console.Write("Send:");
            for (int i = 0; i < buff.Length; i++)
            {
                Console.Write(" [{0:X2}]", buff[i]);
            }
            Console.WriteLine();
            getResponse(ser);

            // TODO: Determine purpose of code Segment
            buff = new byte[] { 0x75, 0x20 };
            ser.Write(buff, 0, buff.Length);
            Console.Write("Send:");
            for (int i = 0; i < buff.Length; i++)
            {
                Console.Write(" [{0:X2}]", buff[i]);
            }
            Console.WriteLine();
            getResponse(ser);

            // TODO: Determine purpose of code Segment
            buff = new byte[] { 0x56, 0xa0, 0x03, 0xfc, 0x00, 0x20 };
            ser.Write(buff, 0, buff.Length);
            Console.Write("Send:");
            for (int i = 0; i < buff.Length; i++)
            {
                Console.Write(" [{0:X2}]", buff[i]);
            }
            Console.WriteLine();
            getResponse(ser);

            // TODO: Determine purpose of code Segment
            buff = new byte[] { 0x56, 0xa0, 0x03, 0xfc, 0x00, 0x20 };
            ser.Write(buff, 0, buff.Length);
            Console.Write("Send:");
            for (int i = 0; i < buff.Length; i++)
            {
                Console.Write(" [{0:X2}]", buff[i]);
            }
            Console.WriteLine();
            getResponse(ser);

            // TODO: Determine purpose of code Segment
            buff = new byte[] { 0x56, 0xa0, 0x03, 0xfd, 0x00, 0x20 };
            ser.Write(buff, 0, buff.Length);
            Console.Write("Send:");
            for (int i = 0; i < buff.Length; i++)
            {
                Console.Write(" [{0:X2}]", buff[i]);
            }
            Console.WriteLine();
            getResponse(ser);
            
            // TODO: Determine purpose of code Segment
            buff = new byte[] { 0x56, 0xa0, 0x03, 0xfe, 0x00, 0x20 };
            ser.Write(buff, 0, buff.Length);
            Console.Write("Send:");
            for (int i = 0; i < buff.Length; i++)
            {
                Console.Write(" [{0:X2}]", buff[i]);
            }
            Console.WriteLine();
            getResponse(ser);

            // TODO: Determine purpose of code Segment
            buff = new byte[] { 0x56, 0xa0, 0x03, 0xff, 0x00, 0x20 };
            ser.Write(buff, 0, buff.Length);
            Console.Write("Send:");
            for (int i = 0; i < buff.Length; i++)
            {
                Console.Write(" [{0:X2}]", buff[i]);
            }
            Console.WriteLine();
            getResponse(ser);
            
            #region Blank Program

            // ==== Blank Program for testing basic communications ==== //

            //// TODO: Determine purpose of code Segment
            //buff = new byte[] { 0x55, 0x00, 0x00, 0x20 };
            //ser.Write(buff, 0, buff.Length);
            //Console.Write("Send:");
            //for (int i = 0; i < buff.Length; i++)
            //{
            //    Console.Write(" [{0:X2}]", buff[i]);
            //}
            //Console.WriteLine();
            //getResponse(ser);

            //// TODO: Determine purpose of code Segment
            //buff = new byte[] { 0x64, 0x00, 0x80, 0x46, 0x0c, 0x94, 0x34, 0x00, 0x0c, 0x94, 0x51, 0x00, 0x0c, 0x94, 0x51, 0x00, 0x0c, 0x94,  0x51,  0x00,  0x0c, 0x94, 0x51, 0x00, 0x0c, 0x94, 0x51, 0x00, 0x0c, 0x94, 0x51, 0x00, 0x0c, 0x94, 0x51, 0x00, 0x0c, 0x94, 0x51, 0x00, 0x0c, 0x94, 0x51, 0x00, 0x0c, 0x94, 0x51, 0x00, 0x0c, 0x94, 0x51, 0x00, 0x0c, 0x94, 0x51, 0x00, 0x0c, 0x94, 0x51, 0x00, 0x0c, 0x94, 0x51, 0x00, 0x0c, 0x94, 0x51, 0x00, 0x0c, 0x94, 0x67, 0x00, 0x0c, 0x94, 0x51, 0x00,0x0c,0x94,0x51,0x00,0x0c,0x94,0x51,0x00,0x0c,0x94,0x51,0x00,0x0c,0x94,0x51,0x00,0x0c,0x94,0x51,0x00,0x0c,0x94,0x51,0x00,0x0c,0x94,0x51,0x00,0x0c,0x94,0x51,0x00,0x11,0x24,0x1f,0xbe,0xcf,0xef,0xd8,0xe0,0xde,0xbf,0xcd,0xbf,0x11,0xe0,0xa0,0xe0,0xb1,0xe0,0xe8,0xed,0xf1, 0xe0, 0x02, 0xc0, 0x20 };
            //ser.Write(buff, 0, buff.Length);
            //Console.Write("Send:");
            //for (int i = 0; i < buff.Length; i++)
            //{
            //    Console.Write(" [{0:X2}]", buff[i]);
            //}
            //Console.WriteLine();
            //getResponse(ser);

            //// TODO: Determine purpose of code Segment
            //buff = new byte[] { 0x55, 0x40, 0x00, 0x20 };
            //ser.Write(buff, 0, buff.Length);
            //Console.Write("Send:");
            //for (int i = 0; i < buff.Length; i++)
            //{
            //    Console.Write(" [{0:X2}]", buff[i]);
            //}
            //Console.WriteLine();
            //getResponse(ser);

            //// TODO: Determine purpose of code Segment
            //buff = new byte[] { 0x64, 0x00, 0x80, 0x46, 0x05, 0x90, 0x0d, 0x92, 0xa0, 0x30, 0xb1, 0x07, 0xd9, 0xf7, 0x11, 0xe0, 0xa0, 0xe0, 0xb1, 0xe0, 0x01, 0xc0, 0x1d, 0x92, 0xa9, 0x30, 0xb1, 0x07, 0xe1, 0xf7, 0x0e, 0x94, 0x56, 0x00, 0x0c, 0x94, 0xea, 0x00, 0x0c, 0x94, 0x00, 0x00, 0x08, 0x95, 0x08, 0x95, 0x08, 0x95, 0xcf, 0x93, 0xdf, 0x93, 0x0e, 0x94, 0xaf, 0x00, 0x0e, 0x94, 0x55, 0x00, 0x0e, 0x94, 0x53, 0x00, 0xc0, 0xe0, 0xd0, 0xe0, 0x0e, 0x94, 0x54, 0x00, 0x20, 0x97, 0xe1, 0xf3, 0x0e, 0x94, 0x00, 0x00, 0xf9, 0xcf, 0x1f, 0x92, 0x0f, 0x92, 0x0f, 0xb6, 0x0f, 0x92, 0x11, 0x24, 0x2f, 0x93, 0x3f, 0x93, 0x8f, 0x93, 0x9f, 0x93, 0xaf, 0x93, 0xbf, 0x93, 0x80, 0x91, 0x04, 0x01, 0x90, 0x91, 0x05, 0x01, 0xa0, 0x91, 0x06, 0x01, 0xb0, 0x91, 0x07, 0x01, 0x30, 0x91, 0x08, 0x01, 0x01, 0x96, 0xa1, 0x1d, 0xb1, 0x1d, 0x23, 0x2f, 0x20 };
            //ser.Write(buff, 0, buff.Length);
            //Console.Write("Send:");
            //for (int i = 0; i < buff.Length; i++)
            //{
            //    Console.Write(" [{0:X2}]", buff[i]);
            //}
            //Console.WriteLine();
            //getResponse(ser);

            //// TODO: Determine purpose of code Segment
            //buff = new byte[] { 0x55, 0x80, 0x00, 0x20 };
            //ser.Write(buff, 0, buff.Length);
            //Console.Write("Send:");
            //for (int i = 0; i < buff.Length; i++)
            //{
            //    Console.Write(" [{0:X2}]", buff[i]);
            //}
            //Console.WriteLine();
            //getResponse(ser);

            //// TODO: Determine purpose of code Segment
            //buff = new byte[] { 0x64, 0x00, 0x80, 0x46, 0x2d, 0x5f, 0x2d, 0x37, 0x20, 0xf0, 0x2d, 0x57, 0x01, 0x96, 0xa1, 0x1d, 0xb1, 0x1d, 0x20, 0x93, 0x08, 0x01, 0x80, 0x93, 0x04, 0x01, 0x90, 0x93, 0x05, 0x01, 0xa0, 0x93, 0x06, 0x01, 0xb0, 0x93, 0x07, 0x01, 0x80, 0x91, 0x00, 0x01, 0x90, 0x91, 0x01, 0x01, 0xa0, 0x91, 0x02, 0x01, 0xb0, 0x91, 0x03, 0x01, 0x01, 0x96, 0xa1, 0x1d, 0xb1, 0x1d, 0x80, 0x93, 0x00, 0x01, 0x90, 0x93, 0x01, 0x01, 0xa0, 0x93, 0x02, 0x01, 0xb0, 0x93, 0x03, 0x01, 0xbf, 0x91, 0xaf, 0x91, 0x9f, 0x91, 0x8f, 0x91, 0x3f, 0x91, 0x2f, 0x91, 0x0f, 0x90, 0x0f, 0xbe, 0x0f, 0x90, 0x1f, 0x90, 0x18, 0x95, 0x78, 0x94, 0x84, 0xb5, 0x82, 0x60, 0x84, 0xbd, 0x84, 0xb5, 0x81, 0x60, 0x84, 0xbd, 0x85, 0xb5, 0x82, 0x60, 0x85, 0xbd, 0x85, 0xb5, 0x81, 0x60, 0x85, 0xbd, 0xee, 0xe6, 0xf0, 0xe0, 0x80, 0x81, 0x81, 0x60, 0x20 };
            //ser.Write(buff, 0, buff.Length);
            //Console.Write("Send:");
            //for (int i = 0; i < buff.Length; i++)
            //{
            //    Console.Write(" [{0:X2}]", buff[i]);
            //}
            //Console.WriteLine();
            //getResponse(ser);

            //// TODO: Determine purpose of code Segment
            //buff = new byte[] { 0x55, 0xc0, 0x00, 0x20 };
            //ser.Write(buff, 0, buff.Length);
            //Console.Write("Send:");
            //for (int i = 0; i < buff.Length; i++)
            //{
            //    Console.Write(" [{0:X2}]", buff[i]);
            //}
            //Console.WriteLine();
            //getResponse(ser);

            //// TODO: Determine purpose of code Segment
            //buff = new byte[] { 0x64, 0x00, 0x58, 0x46, 0x80, 0x83, 0xe1, 0xe8, 0xf0, 0xe0, 0x10, 0x82, 0x80, 0x81, 0x82, 0x60, 0x80, 0x83, 0x80, 0x81, 0x81, 0x60, 0x80, 0x83, 0xe0, 0xe8, 0xf0, 0xe0, 0x80, 0x81, 0x81, 0x60, 0x80, 0x83, 0xe1, 0xeb, 0xf0, 0xe0, 0x80, 0x81, 0x84, 0x60, 0x80, 0x83, 0xe0, 0xeb, 0xf0, 0xe0, 0x80, 0x81, 0x81, 0x60, 0x80, 0x83, 0xea, 0xe7, 0xf0, 0xe0, 0x80, 0x81, 0x84, 0x60, 0x80, 0x83, 0x80, 0x81, 0x82, 0x60, 0x80, 0x83, 0x80, 0x81, 0x81, 0x60, 0x80, 0x83, 0x80, 0x81, 0x80, 0x68, 0x80, 0x83, 0x10, 0x92, 0xc1, 0x00, 0x08, 0x95, 0xf8, 0x94, 0xff, 0xcf, 0x20 };
            //ser.Write(buff, 0, buff.Length);
            //Console.Write("Send:");
            //for (int i = 0; i < buff.Length; i++)
            //{
            //    Console.Write(" [{0:X2}]", buff[i]);
            //}
            //Console.WriteLine();
            //getResponse(ser);

            #endregion

            #region Blink Program

            // ==== Blink program for testing the uploading ==== //

            // TODO: Determine purpose of code Segment
            buff = new byte[] { 0x55, 0x00, 0x00, 0x20 };
            ser.Write(buff, 0, buff.Length);
            Console.Write("Send:");
            for (int i = 0; i < buff.Length; i++)
            {
                Console.Write(" [{0:X2}]", buff[i]);
            }
            Console.WriteLine();
            getResponse(ser);

            // TODO: Determine purpose of code Segment
            buff = new byte[] { 0x64, 0x00, 0x80, 0x46, 0x0c, 0x94, 0x61, 0x00, 0x0c, 0x94, 0x7e, 0x00, 0x0c, 0x94, 0x7e, 0x00, 0x0c, 0x94, 0x7e, 0x00, 0x0c, 0x94, 0x7e, 0x00, 0x0c, 0x94, 0x7e, 0x00, 0x0c, 0x94, 0x7e, 0x00, 0x0c, 0x94, 0x7e, 0x00, 0x0c, 0x94, 0x7e, 0x00, 0x0c, 0x94, 0x7e, 0x00, 0x0c, 0x94, 0x7e, 0x00, 0x0c, 0x94, 0x7e, 0x00, 0x0c, 0x94, 0x7e, 0x00, 0x0c, 0x94, 0x7e, 0x00, 0x0c, 0x94, 0x7e, 0x00, 0x0c, 0x94, 0x7e, 0x00, 0x0c, 0x94, 0x9a, 0x00, 0x0c, 0x94, 0x7e, 0x00, 0x0c, 0x94, 0x7e, 0x00, 0x0c, 0x94, 0x7e, 0x00, 0x0c, 0x94, 0x7e, 0x00, 0x0c, 0x94, 0x7e, 0x00, 0x0c, 0x94, 0x7e, 0x00, 0x0c, 0x94, 0x7e, 0x00, 0x0c, 0x94, 0x7e, 0x00, 0x0c, 0x94, 0x7e, 0x00, 0x00, 0x00, 0x00, 0x00, 0x24, 0x00, 0x27, 0x00, 0x2a, 0x00, 0x00, 0x00, 0x00, 0x00, 0x25, 0x00, 0x28, 0x00, 0x2b, 0x00, 0x00, 0x00, 0x00, 0x00, 0x20 };
            ser.Write(buff, 0, buff.Length);
            Console.Write("Send:");
            for (int i = 0; i < buff.Length; i++)
            {
                Console.Write(" [{0:X2}]", buff[i]);
            }
            Console.WriteLine();
            getResponse(ser);

            // TODO: Determine purpose of code Segment
            buff = new byte[] { 0x55, 0x40, 0x00, 0x20 };
            ser.Write(buff, 0, buff.Length);
            Console.Write("Send:");
            for (int i = 0; i < buff.Length; i++)
            {
                Console.Write(" [{0:X2}]", buff[i]);
            }
            Console.WriteLine();
            getResponse(ser);

            // TODO: Determine purpose of code Segment
            buff = new byte[] { 0x64, 0x00, 0x80, 0x46, 0x23, 0x00, 0x26, 0x00, 0x29, 0x00, 0x04, 0x04, 0x04, 0x04, 0x04, 0x04, 0x04, 0x04, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x03, 0x03, 0x03, 0x03, 0x03, 0x03, 0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80, 0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x00, 0x00, 0x00, 0x07, 0x00, 0x02, 0x01, 0x00, 0x00, 0x03, 0x04, 0x06, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x11, 0x24, 0x1f, 0xbe, 0xcf, 0xef, 0xd8, 0xe0, 0xde, 0xbf, 0xcd, 0xbf, 0x11, 0xe0, 0xa0, 0xe0, 0xb1, 0xe0, 0xea, 0xe3, 0xf4, 0xe0, 0x02, 0xc0, 0x05, 0x90, 0x0d, 0x92, 0xa0, 0x30, 0xb1, 0x07, 0xd9, 0xf7, 0x11, 0xe0, 0xa0, 0xe0, 0xb1, 0xe0, 0x01, 0xc0, 0x1d, 0x92, 0xa9, 0x30, 0xb1, 0x07, 0xe1, 0xf7, 0x0e, 0x94, 0x0a, 0x02, 0x0c, 0x94, 0x1b, 0x02, 0x0c, 0x94, 0x00, 0x00, 0x20 };
            ser.Write(buff, 0, buff.Length);
            Console.Write("Send:");
            for (int i = 0; i < buff.Length; i++)
            {
                Console.Write(" [{0:X2}]", buff[i]);
            }
            Console.WriteLine();
            getResponse(ser);

            // TODO: Determine purpose of code Segment
            buff = new byte[] { 0x55, 0x80, 0x00, 0x20 };
            ser.Write(buff, 0, buff.Length);
            Console.Write("Send:");
            for (int i = 0; i < buff.Length; i++)
            {
                Console.Write(" [{0:X2}]", buff[i]);
            }
            Console.WriteLine();
            getResponse(ser);

            // TODO: Determine purpose of code Segment
            buff = new byte[] { 0x64, 0x00, 0x80, 0x46, 0x8d, 0xe0, 0x61, 0xe0, 0x0e, 0x94, 0xb5, 0x01, 0x68, 0xee, 0x73, 0xe0, 0x80, 0xe0, 0x90, 0xe0, 0x0e, 0x94, 0xe2, 0x00, 0x8d, 0xe0, 0x60, 0xe0, 0x0e, 0x94, 0xb5, 0x01, 0x68, 0xee, 0x73, 0xe0, 0x80, 0xe0, 0x90, 0xe0, 0x0e, 0x94, 0xe2, 0x00, 0x08, 0x95, 0x8d, 0xe0, 0x61, 0xe0, 0x0e, 0x94, 0x76, 0x01, 0x08, 0x95, 0x1f, 0x92, 0x0f, 0x92, 0x0f, 0xb6, 0x0f, 0x92, 0x11, 0x24, 0x2f, 0x93, 0x3f, 0x93, 0x8f, 0x93, 0x9f, 0x93, 0xaf, 0x93, 0xbf, 0x93, 0x80, 0x91, 0x04, 0x01, 0x90, 0x91, 0x05, 0x01, 0xa0, 0x91, 0x06, 0x01, 0xb0, 0x91, 0x07, 0x01, 0x30, 0x91, 0x08, 0x01, 0x01, 0x96, 0xa1, 0x1d, 0xb1, 0x1d, 0x23, 0x2f, 0x2d, 0x5f, 0x2d, 0x37, 0x20, 0xf0, 0x2d, 0x57, 0x01, 0x96, 0xa1, 0x1d, 0xb1, 0x1d, 0x20, 0x93, 0x08, 0x01, 0x80, 0x93, 0x04, 0x01, 0x90, 0x93, 0x05, 0x01, 0x20 };
            ser.Write(buff, 0, buff.Length);
            Console.Write("Send:");
            for (int i = 0; i < buff.Length; i++)
            {
                Console.Write(" [{0:X2}]", buff[i]);
            }
            Console.WriteLine();
            getResponse(ser);

            // TODO: Determine purpose of code Segment
            buff = new byte[] { 0x55, 0xc0, 0x00, 0x20 };
            ser.Write(buff, 0, buff.Length);
            Console.Write("Send:");
            for (int i = 0; i < buff.Length; i++)
            {
                Console.Write(" [{0:X2}]", buff[i]);
            }
            Console.WriteLine();
            getResponse(ser);

            // TODO: Determine purpose of code Segment
            buff = new byte[] { 0x64, 0x00, 0x80, 0x46, 0xa0, 0x93, 0x06, 0x01, 0xb0, 0x93, 0x07, 0x01, 0x80, 0x91, 0x00, 0x01, 0x90, 0x91, 0x01, 0x01, 0xa0, 0x91, 0x02, 0x01, 0xb0, 0x91, 0x03, 0x01, 0x01, 0x96, 0xa1, 0x1d, 0xb1, 0x1d, 0x80, 0x93, 0x00, 0x01, 0x90, 0x93, 0x01, 0x01, 0xa0, 0x93, 0x02, 0x01, 0xb0, 0x93, 0x03, 0x01, 0xbf, 0x91, 0xaf, 0x91, 0x9f, 0x91, 0x8f, 0x91, 0x3f, 0x91, 0x2f, 0x91, 0x0f, 0x90, 0x0f, 0xbe, 0x0f, 0x90, 0x1f, 0x90, 0x18, 0x95, 0x9b, 0x01, 0xac, 0x01, 0x7f, 0xb7, 0xf8, 0x94, 0x80, 0x91, 0x00, 0x01, 0x90, 0x91, 0x01, 0x01, 0xa0, 0x91, 0x02, 0x01, 0xb0, 0x91, 0x03, 0x01, 0x66, 0xb5, 0xa8, 0x9b, 0x05, 0xc0, 0x6f, 0x3f, 0x19, 0xf0, 0x01, 0x96, 0xa1, 0x1d, 0xb1, 0x1d, 0x7f, 0xbf, 0xba, 0x2f, 0xa9, 0x2f, 0x98, 0x2f, 0x88, 0x27, 0x86, 0x0f, 0x91, 0x1d, 0xa1, 0x1d, 0xb1, 0x1d, 0x62, 0xe0, 0x20 };
            ser.Write(buff, 0, buff.Length);
            Console.Write("Send:");
            for (int i = 0; i < buff.Length; i++)
            {
                Console.Write(" [{0:X2}]", buff[i]);
            }
            Console.WriteLine();
            getResponse(ser);

            // TODO: Determine purpose of code Segment
            buff = new byte[] { 0x55, 0x00, 0x01, 0x20 };
            ser.Write(buff, 0, buff.Length);
            Console.Write("Send:");
            for (int i = 0; i < buff.Length; i++)
            {
                Console.Write(" [{0:X2}]", buff[i]);
            }
            Console.WriteLine();
            getResponse(ser);

            // TODO: Determine purpose of code Segment
            buff = new byte[] { 0x64, 0x00, 0x80, 0x46, 0x88, 0x0f, 0x99, 0x1f, 0xaa, 0x1f, 0xbb, 0x1f, 0x6a, 0x95, 0xd1, 0xf7, 0xbc, 0x01, 0x2d, 0xc0, 0xff, 0xb7, 0xf8, 0x94, 0x80, 0x91, 0x00, 0x01, 0x90, 0x91, 0x01, 0x01, 0xa0, 0x91, 0x02, 0x01, 0xb0, 0x91, 0x03, 0x01, 0xe6, 0xb5, 0xa8, 0x9b, 0x05, 0xc0, 0xef, 0x3f, 0x19, 0xf0, 0x01, 0x96, 0xa1, 0x1d, 0xb1, 0x1d, 0xff, 0xbf, 0xba, 0x2f, 0xa9, 0x2f, 0x98, 0x2f, 0x88, 0x27, 0x8e, 0x0f, 0x91, 0x1d, 0xa1, 0x1d, 0xb1, 0x1d, 0xe2, 0xe0, 0x88, 0x0f, 0x99, 0x1f, 0xaa, 0x1f, 0xbb, 0x1f, 0xea, 0x95, 0xd1, 0xf7, 0x86, 0x1b, 0x97, 0x0b, 0x88, 0x5e, 0x93, 0x40, 0xc8, 0xf2, 0x21, 0x50, 0x30, 0x40, 0x40, 0x40, 0x50, 0x40, 0x68, 0x51, 0x7c, 0x4f, 0x21, 0x15, 0x31, 0x05, 0x41, 0x05, 0x51, 0x05, 0x71, 0xf6, 0x08, 0x95, 0x78, 0x94, 0x84, 0xb5, 0x82, 0x60, 0x84, 0xbd, 0x84, 0xb5, 0x20 };
            ser.Write(buff, 0, buff.Length);
            Console.Write("Send:");
            for (int i = 0; i < buff.Length; i++)
            {
                Console.Write(" [{0:X2}]", buff[i]);
            }
            Console.WriteLine();
            getResponse(ser);

            // TODO: Determine purpose of code Segment
            buff = new byte[] { 0x55, 0x40, 0x01, 0x20 };
            ser.Write(buff, 0, buff.Length);
            Console.Write("Send:");
            for (int i = 0; i < buff.Length; i++)
            {
                Console.Write(" [{0:X2}]", buff[i]);
            }
            Console.WriteLine();
            getResponse(ser);

            // TODO: Determine purpose of code Segment
            buff = new byte[] { 0x64, 0x00, 0x80, 0x46, 0x81, 0x60, 0x84, 0xbd, 0x85, 0xb5, 0x82, 0x60, 0x85, 0xbd, 0x85, 0xb5, 0x81, 0x60, 0x85, 0xbd, 0xee, 0xe6, 0xf0, 0xe0, 0x80, 0x81, 0x81, 0x60, 0x80, 0x83, 0xe1, 0xe8, 0xf0, 0xe0, 0x10, 0x82, 0x80, 0x81, 0x82, 0x60, 0x80, 0x83, 0x80, 0x81, 0x81, 0x60, 0x80, 0x83, 0xe0, 0xe8, 0xf0, 0xe0, 0x80, 0x81, 0x81, 0x60, 0x80, 0x83, 0xe1, 0xeb, 0xf0, 0xe0, 0x80, 0x81, 0x84, 0x60, 0x80, 0x83, 0xe0, 0xeb, 0xf0, 0xe0, 0x80, 0x81, 0x81, 0x60, 0x80, 0x83, 0xea, 0xe7, 0xf0, 0xe0, 0x80, 0x81, 0x84, 0x60, 0x80, 0x83, 0x80, 0x81, 0x82, 0x60, 0x80, 0x83, 0x80, 0x81, 0x81, 0x60, 0x80, 0x83, 0x80, 0x81, 0x80, 0x68, 0x80, 0x83, 0x10, 0x92, 0xc1, 0x00, 0x08, 0x95, 0xcf, 0x93, 0xdf, 0x93, 0x48, 0x2f, 0x50, 0xe0, 0xca, 0x01, 0x86, 0x56, 0x9f, 0x4f, 0xfc, 0x01, 0x34, 0x91, 0x4a, 0x57, 0x20 };
            ser.Write(buff, 0, buff.Length);
            Console.Write("Send:");
            for (int i = 0; i < buff.Length; i++)
            {
                Console.Write(" [{0:X2}]", buff[i]);
            }
            Console.WriteLine();
            getResponse(ser);

            // TODO: Determine purpose of code Segment
            buff = new byte[] { 0x55, 0x80, 0x01, 0x20 };
            ser.Write(buff, 0, buff.Length);
            Console.Write("Send:");
            for (int i = 0; i < buff.Length; i++)
            {
                Console.Write(" [{0:X2}]", buff[i]);
            }
            Console.WriteLine();
            getResponse(ser);

            // TODO: Determine purpose of code Segment
            buff = new byte[] { 0x64, 0x00, 0x80, 0x46, 0x5f, 0x4f, 0xfa, 0x01, 0x84, 0x91, 0x88, 0x23, 0x69, 0xf1, 0x90, 0xe0, 0x88, 0x0f, 0x99, 0x1f, 0xfc, 0x01, 0xe8, 0x59, 0xff, 0x4f, 0xa5, 0x91, 0xb4, 0x91, 0xfc, 0x01, 0xee, 0x58, 0xff, 0x4f, 0xc5, 0x91, 0xd4, 0x91, 0x66, 0x23, 0x51, 0xf4, 0x2f, 0xb7, 0xf8, 0x94, 0x8c, 0x91, 0x93, 0x2f, 0x90, 0x95, 0x89, 0x23, 0x8c, 0x93, 0x88, 0x81, 0x89, 0x23, 0x0b, 0xc0, 0x62, 0x30, 0x61, 0xf4, 0x2f, 0xb7, 0xf8, 0x94, 0x8c, 0x91, 0x93, 0x2f, 0x90, 0x95, 0x89, 0x23, 0x8c, 0x93, 0x88, 0x81, 0x83, 0x2b, 0x88, 0x83, 0x2f, 0xbf, 0x06, 0xc0, 0x9f, 0xb7, 0xf8, 0x94, 0x8c, 0x91, 0x83, 0x2b, 0x8c, 0x93, 0x9f, 0xbf, 0xdf, 0x91, 0xcf, 0x91, 0x08, 0x95, 0x48, 0x2f, 0x50, 0xe0, 0xca, 0x01, 0x82, 0x55, 0x9f, 0x4f, 0xfc, 0x01, 0x24, 0x91, 0xca, 0x01, 0x86, 0x56, 0x9f, 0x4f, 0xfc, 0x01, 0x20 };
            ser.Write(buff, 0, buff.Length);
            Console.Write("Send:");
            for (int i = 0; i < buff.Length; i++)
            {
                Console.Write(" [{0:X2}]", buff[i]);
            }
            Console.WriteLine();
            getResponse(ser);

            // TODO: Determine purpose of code Segment
            buff = new byte[] { 0x55, 0xc0, 0x01, 0x20 };
            ser.Write(buff, 0, buff.Length);
            Console.Write("Send:");
            for (int i = 0; i < buff.Length; i++)
            {
                Console.Write(" [{0:X2}]", buff[i]);
            }
            Console.WriteLine();
            getResponse(ser);

            // TODO: Determine purpose of code Segment
            buff = new byte[] { 0x64, 0x00, 0x80, 0x46, 0x94, 0x91, 0x4a, 0x57, 0x5f, 0x4f, 0xfa, 0x01, 0x34, 0x91, 0x33, 0x23, 0x09, 0xf4, 0x40, 0xc0, 0x22, 0x23, 0x51, 0xf1, 0x23, 0x30, 0x71, 0xf0, 0x24, 0x30, 0x28, 0xf4, 0x21, 0x30, 0xa1, 0xf0, 0x22, 0x30, 0x11, 0xf5, 0x14, 0xc0, 0x26, 0x30, 0xb1, 0xf0, 0x27, 0x30, 0xc1, 0xf0, 0x24, 0x30, 0xd9, 0xf4, 0x04, 0xc0, 0x80, 0x91, 0x80, 0x00, 0x8f, 0x77, 0x03, 0xc0, 0x80, 0x91, 0x80, 0x00, 0x8f, 0x7d, 0x80, 0x93, 0x80, 0x00, 0x10, 0xc0, 0x84, 0xb5, 0x8f, 0x77, 0x02, 0xc0, 0x84, 0xb5, 0x8f, 0x7d, 0x84, 0xbd, 0x09, 0xc0, 0x80, 0x91, 0xb0, 0x00, 0x8f, 0x77, 0x03, 0xc0, 0x80, 0x91, 0xb0, 0x00, 0x8f, 0x7d, 0x80, 0x93, 0xb0, 0x00, 0xe3, 0x2f, 0xf0, 0xe0, 0xee, 0x0f, 0xff, 0x1f, 0xee, 0x58, 0xff, 0x4f, 0xa5, 0x91, 0xb4, 0x91, 0x2f, 0xb7, 0xf8, 0x94, 0x66, 0x23, 0x21, 0xf4, 0x20 };
            ser.Write(buff, 0, buff.Length);
            Console.Write("Send:");
            for (int i = 0; i < buff.Length; i++)
            {
                Console.Write(" [{0:X2}]", buff[i]);
            }
            Console.WriteLine();
            getResponse(ser);


            // TODO: Determine purpose of code Segment
            buff = new byte[] { 0x55, 0x00, 0x02, 0x20 };
            ser.Write(buff, 0, buff.Length);
            Console.Write("Send:");
            for (int i = 0; i < buff.Length; i++)
            {
                Console.Write(" [{0:X2}]", buff[i]);
            }
            Console.WriteLine();
            getResponse(ser);

            // TODO: Determine purpose of code Segment
            buff = new byte[] { 0x64, 0x00, 0x3a, 0x46, 0x8c, 0x91, 0x90, 0x95, 0x89, 0x23, 0x02, 0xc0, 0x8c, 0x91, 0x89, 0x2b, 0x8c, 0x93, 0x2f, 0xbf, 0x08, 0x95, 0x08, 0x95, 0xcf, 0x93, 0xdf, 0x93, 0x0e, 0x94, 0x3b, 0x01, 0x0e, 0x94, 0x09, 0x02, 0x0e, 0x94, 0x95, 0x00, 0xc0, 0xe0, 0xd0, 0xe0, 0x0e, 0x94, 0x80, 0x00, 0x20, 0x97, 0xe1, 0xf3, 0x0e, 0x94, 0x00, 0x00, 0xf9, 0xcf, 0xf8, 0x94, 0xff, 0xcf, 0x20 };
            ser.Write(buff, 0, buff.Length);
            Console.Write("Send:");
            for (int i = 0; i < buff.Length; i++)
            {
                Console.Write(" [{0:X2}]", buff[i]);
            }
            Console.WriteLine();
            getResponse(ser);

            #endregion

            // Code Segment to leave program mode
            buff = new byte[] { 0x51, 0x20 };
            ser.Write(buff, 0, buff.Length);
            Console.Write("Send:");
            for (int i = 0; i < buff.Length; i++)
            {
                Console.Write(" [{0:X2}]", buff[i]);
            }
            Console.WriteLine();
            getResponse(ser);

            // End communication
            ser.Close();
            Console.WriteLine("Done.");
            Console.ReadKey();
        }

        // Get response from the board and also displays it
        static List<byte> getResponse(SerialPort ser)
        {
            List<byte> response = new List<byte>(); // A list that holds all the received data
            byte rByte = 0xFF;          // Holds a byte of data recieved from the board
            long timer = 1000000;       // Timer to stop app if nothing is received

            while (ser.BytesToRead == 0 && timer > 0)   // Wait for data to be available
                timer--;    // Decrement timer
            
            if (timer == 0)
            {
                Console.WriteLine("Timeout error. Press any key to exit...");
                Console.ReadKey();
                Environment.Exit(1);
            }

            while (rByte != 0x10)
            {
                rByte = (byte)ser.ReadByte();                // Read the incoming byte
                response.Add(rByte);
                Console.WriteLine("Recv: [{0:X2}]", rByte);  // Print the byte to the screen
            }

            return response;
        }
    }
}
