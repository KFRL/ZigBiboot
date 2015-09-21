.386
.model flat,stdcall 
option casemap:none
// the model of your program will usually be flat, stdcall
// There is stdcall syntax and C syntax of summoning functions
// the stdcall syntax means you call your program arguments from 
// RIGHT to LEFT rather than LEFT to RIGHT which is the C syntax.

.data
// This is your initialized variables that you will use.
// The format is: NameOfVariable db "Message",0
// the comma 0 is the null terminator indicating end of the message.

.data?
// This is your uninitialized variables that you will use.

.const
// Constant variables example: BUFFER_SIZE   equ 1024

.code

 main:
// Your code and everything goes in here.

mov eax, 8
mov ecx, eax

 end main
