/**********************************************************/
/* Zigbiboot bootloader for Arduino                       */
/*                                                        */
/* http://optiboot.googlecode.com                         */
/*                                                        */
/* Bootloader based off Optiboot that can be used         */
/* to program Arduino wirelessly through ZigBee           */
/* networks.                                              */
/*                                                        */
/* Fully supported:                                       */
/*   ATmega328P based devices (Duemilanove etc)           */
/*                                                        */
/* This program is free software; you can redistribute it */
/* and/or modify it under the terms of the GNU General    */
/* Public License as published by the Free Software       */
/* Foundation; either version 2 of the License, or        */
/* (at your option) any later version.                    */
/*                                                        */
/* This program is distributed in the hope that it will   */
/* be useful, but WITHOUT ANY WARRANTY; without even the  */
/* implied warranty of MERCHANTABILITY or FITNESS FOR A   */
/* PARTICULAR PURPOSE.  See the GNU General Public        */
/* License for more details.                              */
/*                                                        */
/* You should have received a copy of the GNU General     */
/* Public License along with this program; if not, write  */
/* to the Free Software Foundation, Inc.,                 */
/* 59 Temple Place, Suite 330, Boston, MA  02111-1307 USA */
/*                                                        */
/* Licence can be viewed at                               */
/* http://www.fsf.org/licenses/gpl.txt                    */
/*                                                        */
/**********************************************************/

#define OPTIBOOT_MAJVER 4
#define OPTIBOOT_MINVER 4

#define MAKESTR(a) #a
#define MAKEVER(a, b) MAKESTR(a*256+b)

asm("  .section .version\n"
    "optiboot_version:  .word " MAKEVER(OPTIBOOT_MAJVER, OPTIBOOT_MINVER) "\n"
    "  .section .text\n");

#include <inttypes.h>
#include <avr/io.h>
#include <avr/pgmspace.h>

// <avr/boot.h> uses sts instructions, but this version uses out instructions
// This saves cycles and program memory.
#include "boot.h"

// We don't use <avr/wdt.h> as those routines have interrupt overhead we don't need.

#include "pin_defs.h"
#include "stk500.h"

#ifndef LED_START_FLASHES
#define LED_START_FLASHES 0
#endif

/* set the UART baud rate defaults */
#ifndef BAUD_RATE
#if F_CPU >= 8000000L
#define BAUD_RATE   115200L // Highest rate Avrdude win32 will support
#elsif F_CPU >= 1000000L
#define BAUD_RATE   9600L   // 19200 also supported, but with significant error
#elsif F_CPU >= 128000L
#define BAUD_RATE   4800L   // Good for 128kHz internal RC
#else
#define BAUD_RATE 1200L     // Good even at 32768Hz
#endif
#endif

/* Switch in soft UART for hard baud rates */
#if (F_CPU/BAUD_RATE) > 280 // > 57600 for 16MHz
#ifndef SOFT_UART
#define SOFT_UART
#endif
#endif

/* Watchdog settings */
#define WATCHDOG_OFF    (0)
#define WATCHDOG_16MS   (_BV(WDE))
#define WATCHDOG_32MS   (_BV(WDP0) | _BV(WDE))
#define WATCHDOG_64MS   (_BV(WDP1) | _BV(WDE))
#define WATCHDOG_125MS  (_BV(WDP1) | _BV(WDP0) | _BV(WDE))
#define WATCHDOG_250MS  (_BV(WDP2) | _BV(WDE))
#define WATCHDOG_500MS  (_BV(WDP2) | _BV(WDP0) | _BV(WDE))
#define WATCHDOG_1S     (_BV(WDP2) | _BV(WDP1) | _BV(WDE))
#define WATCHDOG_2S     (_BV(WDP2) | _BV(WDP1) | _BV(WDP0) | _BV(WDE))
#ifndef __AVR_ATmega8__
#define WATCHDOG_4S     (_BV(WDP3) | _BV(WDE))
#define WATCHDOG_8S     (_BV(WDP3) | _BV(WDP0) | _BV(WDE))
#endif

/* Function Prototypes */
/* The main function is in init9, which removes the interrupt vector table */
/* we don't need. It is also 'naked', which means the compiler does not    */
/* generate any entry or exit code itself. */
int main(void) __attribute__ ((naked)) __attribute__ ((section (".init9")));
void putch(char);
void putpacket(char);
uint8_t sendFailure();
uint8_t getch(void);
uint8_t getCommand(void);
void ledhalt();
static inline void getNch(uint8_t); /* "static inline" is a compiler hint to reduce code size */
void verifySpace();
static inline void flash_led(uint8_t);
uint8_t getLen();
static inline void watchdogReset();
void watchdogConfig(uint8_t x);
#ifdef SOFT_UART
void uartDelay() __attribute__ ((naked));
#endif
void appStart() __attribute__ ((naked));

#if defined(__AVR_ATmega168__)
#define RAMSTART (0x100)
#define NRWWSTART (0x3800)
#elif defined(__AVR_ATmega328P__)
#define RAMSTART (0x100)
#define NRWWSTART (0x7000)
#elif defined (__AVR_ATmega644P__)
#define RAMSTART (0x100)
#define NRWWSTART (0xE000)
#elif defined(__AVR_ATtiny84__)
#define RAMSTART (0x100)
#define NRWWSTART (0x0000)
#elif defined(__AVR_ATmega1280__)
#define RAMSTART (0x200)
#define NRWWSTART (0xE000)
#elif defined(__AVR_ATmega8__) || defined(__AVR_ATmega88__)
#define RAMSTART (0x100)
#define NRWWSTART (0x1800)
#endif

#define BUFFERSTART (0x300)

/* C zero initialises all global variables. However, that requires */
/* These definitions are NOT zero initialised, but that doesn't matter */
/* This allows us to drop the zero init code, saving us memory */
#define buff    ((uint8_t*)(BUFFERSTART))
#ifdef VIRTUAL_BOOT_PARTITION
#define rstVect (*(uint16_t*)(RAMSTART+SPM_PAGESIZE*2+4))
#define wdtVect (*(uint16_t*)(RAMSTART+SPM_PAGESIZE*2+6))
#endif

#define XBEE
//#define XBEE_TEST

#ifdef XBEE_TEST
#ifndef XBEE
#define XBEE
#endif
#endif

#ifdef XBEE
#define RESET_MESSAGE 0x04

// Command to change address of host ZigBee
#define ZB_LOAD_ADDRESS 0x80
#define PAYLOAD_MAX 72
// Position of payload in RX packet after frame type
#define PAYLOAD_POS 11

// XBee information
#define DELIMITER 126     // decimal for 0x7E
#define LENGTH_UPPER 0    // decimal for 0x00
#define LENGTH_LOWER 15   // decimal for 0x0F
#define FRAME_TYPE 16     // decimal for 0x10, TX request
#define FRAME_ID 1        // decimal for 0x01
#define BROADCAST_RADIUS 0// decimal for 0x00
#define OPTIONS 0         // decimal for 0x00, default options
#define XBEE_ADDR_16H 255 // decimal for 0xFF
#define XBEE_ADDR_16L 254 // decimal for 0xFE

#define RECV_FRAMEID 144  // decimal for 0x90
#define STATUS_FRAMEID 139// decimal for 0x8B

uint8_t xbeeAddress[8];
uint8_t frameSum;		  // holds sum of all bytes before payload
#endif

/* main program starts here */
int main(void) {
	
#ifdef XBEE
	xbeeAddress[0] = 0x00;
	xbeeAddress[1] = 0x00;
	xbeeAddress[2] = 0x00;
	xbeeAddress[3] = 0x00;
	xbeeAddress[4] = 0x00;
	xbeeAddress[5] = 0x00;
	xbeeAddress[6] = 0x00;
	xbeeAddress[7] = 0x00;
	
	frameSum = 0x0e;
#endif

  uint8_t ch;

  /*
   * Making these local and in registers prevents the need for initializing
   * them, and also saves space because code no longer stores to memory.
   * (initializing address keeps the compiler happy, but isn't really
   *  necessary, and uses 4 bytes of flash.)
   */
  register uint16_t address = 0;
  register uint8_t  length;

  // After the zero init loop, this is the first code to run.
  //
  // This code makes the following assumptions:
  //  No interrupts will execute
  //  SP points to RAMEND
  //  r1 contains zero
  //
  // If not, uncomment the following instructions:
  // cli();
  asm volatile ("clr __zero_reg__");
#ifdef __AVR_ATmega8__
  SP=RAMEND;  // This is done by hardware reset
#endif

  // Adaboot no-wait mod
  ch = MCUSR;
  MCUSR = 0;
  if (!(ch & _BV(EXTRF))) appStart();

#if LED_START_FLASHES > 0
  // Set up Timer 1 for timeout counter
  TCCR1B = _BV(CS12) | _BV(CS10); // div 1024
#endif
#ifndef SOFT_UART
#ifdef __AVR_ATmega8__
  UCSRA = _BV(U2X); //Double speed mode USART
  UCSRB = _BV(RXEN) | _BV(TXEN);  // enable Rx & Tx
  UCSRC = _BV(URSEL) | _BV(UCSZ1) | _BV(UCSZ0);  // config USART; 8N1
  UBRRL = (uint8_t)( (F_CPU + BAUD_RATE * 4L) / (BAUD_RATE * 8L) - 1 );
#else
  UCSR0A = _BV(U2X0); //Double speed mode USART0
  UCSR0B = _BV(RXEN0) | _BV(TXEN0);
  UCSR0C = _BV(UCSZ00) | _BV(UCSZ01);
  UBRR0L = (uint8_t)( (F_CPU + BAUD_RATE * 4L) / (BAUD_RATE * 8L) - 1 );
#endif
#endif

  // Set up watchdog to trigger after 500ms
#ifdef XBEE_TEST
  watchdogConfig(WATCHDOG_OFF);
#else
  watchdogConfig(WATCHDOG_2S);
#endif
  // For testing purposes
  // watchdogConfig(WATCHDOG_OFF);

  /* Set LED pin as output */
  LED_DDR |= _BV(LED);

#ifdef SOFT_UART
  /* Set TX pin as output */
  UART_DDR |= _BV(UART_TX_BIT);
#endif

#if LED_START_FLASHES > 0
  /* Flash onboard LED to signal entering of bootloader */
  flash_led(LED_START_FLASHES * 2);
#endif

  putpacket(RESET_MESSAGE);
  
  /* Forever loop */
  for (;;) {
    /* get character from UART */
    ch = getCommand();

#ifdef XBEE_TEST
	putpacket(ch);
	ledhalt();

#else
	if(ch == STK_LOAD_ADDRESS) {
      // LOAD ADDRESS
      uint16_t newAddress;
      newAddress = getch();
      newAddress = (newAddress & 0xff) | (getch() << 8);
#ifdef RAMPZ
      // Transfer top bit to RAMPZ
      RAMPZ = (newAddress & 0x8000) ? 1 : 0;
#endif
      newAddress += newAddress; // Convert from word address to byte address
      address = newAddress;
      verifySpace();
    }
    /* Write memory, length is big endian and is in bytes */
    else if(ch == STK_PROG_PAGE) {
      // PROGRAM PAGE - we support flash programming only, not EEPROM
      uint8_t *bufPtr;
      uint16_t addrPtr;

	  getch();			/* getlen() */
	  length = getch();
	  getch();

      // If we are in RWW section, immediately start page erase
      if (address < NRWWSTART) __boot_page_erase_short((uint16_t)(void*)address);

      // While that is going on, read in page contents
      bufPtr = buff;
	  do
	  {
		  ch = getch();
		  *bufPtr++ = ch;
	  }
      while (--length);

      // If we are in NRWW section, page erase has to be delayed until now.
      // Todo: Take RAMPZ into account
      if (address >= NRWWSTART) __boot_page_erase_short((uint16_t)(void*)address);

      // Read command terminator, start reply
      verifySpace();

      // If only a partial page is to be programmed, the erase might not be complete.
      // So check that here
      boot_spm_busy_wait();

#ifdef VIRTUAL_BOOT_PARTITION
      if ((uint16_t)(void*)address == 0) {
        // This is the reset vector page. We need to live-patch the code so the
        // bootloader runs.
        //
        // Move RESET vector to WDT vector
        uint16_t vect = buff[0] | (buff[1]<<8);
        rstVect = vect;
        wdtVect = buff[8] | (buff[9]<<8);
        vect -= 4; // Instruction is a relative jump (rjmp), so recalculate.
        buff[8] = vect & 0xff;
        buff[9] = vect >> 8;

        // Add jump to bootloader at RESET vector
        buff[0] = 0x7f;
        buff[1] = 0xce; // rjmp 0x1d00 instruction
      }
#endif

      // Copy buffer into programming buffer
      bufPtr = buff;
      addrPtr = (uint16_t)(void*)address;
      ch = SPM_PAGESIZE / 2;
      do {
        uint16_t a;
        a = *bufPtr++;
        a |= (*bufPtr++) << 8;
        __boot_page_fill_short((uint16_t)(void*)addrPtr,a);
        addrPtr += 2;
      } while (--ch);

      // Write from programming buffer
      __boot_page_write_short((uint16_t)(void*)address);
      boot_spm_busy_wait();

#if defined(RWWSRE)
      // Reenable read access to flash
      boot_rww_enable();
#endif

    }
    /* Read memory block mode, length is big endian.  */
//    else if(ch == STK_READ_PAGE) {
//      // READ PAGE - we only read flash
//      getpacket();			/* getlen() */
//      length = getpacket();
//      getpacket();
//
//      verifySpace();
//#ifdef VIRTUAL_BOOT_PARTITION
//      do {
//        // Undo vector patch in bottom page so verify passes
//        if (address == 0)       ch=rstVect & 0xff;
//        else if (address == 1)  ch=rstVect >> 8;
//        else if (address == 8)  ch=wdtVect & 0xff;
//        else if (address == 9) ch=wdtVect >> 8;
//        else ch = pgm_read_byte_near(address);
//        address++;
//        putpacket(ch);
//      } while (--length);
//#else
//#ifdef __AVR_ATmega1280__
////      do putch(pgm_read_byte_near(address++));
////      while (--length);
//      do {
//        uint8_t result;
//        __asm__ ("elpm %0,Z\n":"=r"(result):"z"(address));
//        putpacket(result);
//        address++;
//      }
//      while (--length);
//#else
//      do putpacket(pgm_read_byte_near(address++));
//      while (--length);
//#endif
//#endif
//    }
    /* Get device signature bytes  */
    else if (ch == STK_LEAVE_PROGMODE) {
      // Adaboot no-wait mod
	  watchdogConfig(WATCHDOG_64MS);
      verifySpace();
    }
    else if (ch == STK_ENTER_PROGMODE)
    {
    	// Switch off watchdog timer when programming
    	watchdogConfig(WATCHDOG_OFF);
    	verifySpace();
    }
    else {
      // This covers the response to other commands
      verifySpace();
    }
#endif
    putpacket(STK_OK);
  }
}

void putch(char ch) {
#ifndef SOFT_UART
  while (!(UCSR0A & _BV(UDRE0)));
  UDR0 = ch;
#else
  __asm__ __volatile__ (
    "   com %[ch]\n" // ones complement, carry set
    "   sec\n"
    "1: brcc 2f\n"
    "   cbi %[uartPort],%[uartBit]\n"
    "   rjmp 3f\n"
    "2: sbi %[uartPort],%[uartBit]\n"
    "   nop\n"
    "3: rcall uartDelay\n"
    "   rcall uartDelay\n"
    "   lsr %[ch]\n"
    "   dec %[bitcnt]\n"
    "   brne 1b\n"
    :
    :
      [bitcnt] "d" (10),
      [ch] "r" (ch),
      [uartPort] "I" (_SFR_IO_ADDR(UART_PORT)),
      [uartBit] "I" (UART_TX_BIT)
    :
      "r25"
  );
#endif
}

// Creates a data packet and sends it to Xbee
void putpacket(char cha)
{
#ifdef XBEE
	uint8_t checksum = frameSum;
	checksum += cha;
	checksum = 0xFF - (checksum & 0xFF);
  
	putch(DELIMITER);
	putch(LENGTH_UPPER);
	putch(LENGTH_LOWER);
	putch(FRAME_TYPE);
	putch(FRAME_ID);

	uint8_t i = 0;
	for(; i < 8; i++)
		putch(xbeeAddress[i]);
   
	putch(XBEE_ADDR_16H);
	putch(XBEE_ADDR_16L);
	putch(BROADCAST_RADIUS);
	putch(OPTIONS);
#endif

   putch(cha);

#ifdef XBEE
   putch(checksum);
#endif
}

#ifdef XBEE
// A function to ensure delivery status
uint8_t sendFailure()
{
	uint8_t ch = getch();
	getch();  // drop the higher byte of the length
	uint8_t length = getch();
	do
	{
		getch();
	} while (--length != 2);

	ch = getch();
	getch();
	getch();

	return ch;
}
#endif

// A function to handle receiving XBee packets
uint8_t getCommand(void)
{
	uint8_t ch = getch();

#ifdef XBEE
	while (ch != 0x7E)
		ch = getch();

	getch();					// drop upper byte of length
	uint8_t len = getch();	// store lower byte of length
	ch = getch();				// frame id

	while (ch != 0x90)
	{
		do getch();			// discard all bytes including checksum
		while (--len);

		getch();				// 0x7e
		getch();				// upper length
		len = getch();		// lower length
		ch = getch();			// frame id
	}

	uint8_t i = 0;

	while (i++ < PAYLOAD_POS)
		getch();				// discard all bytes until payload

	ch = getch();				// get first byte of payload
#endif

	return ch;
}

uint8_t getch(void) {
  uint8_t ch;

#ifdef LED_DATA_FLASH
#ifdef __AVR_ATmega8__
  LED_PORT ^= _BV(LED);
#else
  LED_PIN |= _BV(LED);
#endif
#endif

#ifdef SOFT_UART
  __asm__ __volatile__ (
    "1: sbic  %[uartPin],%[uartBit]\n"  // Wait for start edge
    "   rjmp  1b\n"
    "   rcall uartDelay\n"          // Get to middle of start bit
    "2: rcall uartDelay\n"              // Wait 1 bit period
    "   rcall uartDelay\n"              // Wait 1 bit period
    "   clc\n"
    "   sbic  %[uartPin],%[uartBit]\n"
    "   sec\n"
    "   dec   %[bitCnt]\n"
    "   breq  3f\n"
    "   ror   %[ch]\n"
    "   rjmp  2b\n"
    "3:\n"
    :
      [ch] "=r" (ch)
    :
      [bitCnt] "d" (9),
      [uartPin] "I" (_SFR_IO_ADDR(UART_PIN)),
      [uartBit] "I" (UART_RX_BIT)
    :
      "r25"
);
#else
  while(!(UCSR0A & _BV(RXC0)))
    ;
  if (!(UCSR0A & _BV(FE0))) {
      /*
       * A Framing Error indicates (probably) that something is talking
       * to us at the wrong bit rate.  Assume that this is because it
       * expects to be talking to the application, and DON'T reset the
       * watchdog.  This should cause the bootloader to abort and run
       * the application "soon", if it keeps happening.  (Note that we
       * don't care that an invalid char is returned...)
       */
    watchdogReset();
  }
  
  ch = UDR0;
#endif

#ifdef LED_DATA_FLASH
#ifdef __AVR_ATmega8__
  LED_PORT ^= _BV(LED);
#else
  LED_PIN |= _BV(LED);
#endif
#endif

  return ch;
}

#ifdef SOFT_UART
// AVR350 equation: #define UART_B_VALUE (((F_CPU/BAUD_RATE)-23)/6)
// Adding 3 to numerator simulates nearest rounding for more accurate baud rates
#define UART_B_VALUE (((F_CPU/BAUD_RATE)-20)/6)
#if UART_B_VALUE > 255
#error Baud rate too slow for soft UART
#endif

void uartDelay() {
  __asm__ __volatile__ (
    "ldi r25,%[count]\n"
    "1:dec r25\n"
    "brne 1b\n"
    "ret\n"
    ::[count] "M" (UART_B_VALUE)
  );
}
#endif

void getNch(uint8_t count) {
	do getch(); while (--count);
	verifySpace();
}

void verifySpace() {
	if (getch() != CRC_EOP) {
		watchdogConfig(WATCHDOG_64MS);    // shorten WD timeout
		while (1)			      // and busy-loop so that WD causes
			;				      //  a reset and app start.
	}
#ifdef XBEE
	getch();					  // Discard checksum
#endif
	putpacket(STK_INSYNC);
}

#if LED_START_FLASHES > 0
void flash_led(uint8_t count) {
  do {
    TCNT1 = -(F_CPU/(1024*16));
    TIFR1 = _BV(TOV1);
    while(!(TIFR1 & _BV(TOV1)));
#ifdef __AVR_ATmega8__
    LED_PORT ^= _BV(LED);
#else
    LED_PIN |= _BV(LED);
#endif
    watchdogReset();
  } while (--count);
}
#endif

void ledhalt()
{
	LED_PORT ^= _BV(LED);
	while (1);
}

// Watchdog functions. These are only safe with interrupts turned off.
void watchdogReset() {
  __asm__ __volatile__ (
    "wdr\n"
  );
}

void watchdogConfig(uint8_t x) {
  WDTCSR = _BV(WDCE) | _BV(WDE);
  WDTCSR = x;
}

void appStart() {
  watchdogConfig(WATCHDOG_OFF);
  __asm__ __volatile__ (
#ifdef VIRTUAL_BOOT_PARTITION
    // Jump to WDT vector
    "ldi r30,4\n"
    "clr r31\n"
#else
    // Jump to RST vector
    "clr r30\n"
    "clr r31\n"
#endif
    "ijmp\n"
  );
}
