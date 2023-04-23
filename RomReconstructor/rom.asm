; 48K Spectrum ROM disassembly
;
; Annotations taken from 'The Complete Spectrum ROM Disassembly' by Dr Ian
; Logan and Dr Frank O'Hara, published by Melbourne House.
;
; Copyright 1982 Amstrad
; Copyright 1983 Dr Ian Logan & Dr Frank O'Hara
; Copyright 2013-2022 Richard Dymond

KSTATE EQU $5C00
LAST_K EQU $5C08
REPDEL EQU $5C09
REPPER EQU $5C0A
DEFADD EQU $5C0B
K_DATA EQU $5C0D
TVDATA EQU $5C0E
STRMS EQU $5C10
CHARS EQU $5C36
RASP EQU $5C38
PIP EQU $5C39
ERR_NR EQU $5C3A
FLAGS EQU $5C3B
TV_FLAG EQU $5C3C
ERR_SP EQU $5C3D
LIST_SP EQU $5C3F
MODE EQU $5C41
NEWPPC EQU $5C42
NSPPC EQU $5C44
PPC EQU $5C45
SUBPPC EQU $5C47
BORDCR EQU $5C48
E_PPC EQU $5C49
VARS EQU $5C4B
DEST EQU $5C4D
CHANS EQU $5C4F
CURCHL EQU $5C51
PROG EQU $5C53
NXTLIN EQU $5C55
DATADD EQU $5C57
E_LINE EQU $5C59
K_CUR EQU $5C5B
CH_ADD EQU $5C5D
X_PTR EQU $5C5F
WORKSP EQU $5C61
STKBOT EQU $5C63
STKEND EQU $5C65
BREG EQU $5C67
MEM EQU $5C68
FLAGS2 EQU $5C6A
DF_SZ EQU $5C6B
S_TOP EQU $5C6C
OLDPPC EQU $5C6E
OSPCC EQU $5C70
FLAGX EQU $5C71
STRLEN EQU $5C72
T_ADDR EQU $5C74
SEED EQU $5C76
FRAMES EQU $5C78
UDG EQU $5C7B
COORDS EQU $5C7D
P_POSN EQU $5C7F
PR_CC EQU $5C80
ECHO_E EQU $5C82
DF_CC EQU $5C84
DF_CCL EQU $5C86
S_POSN EQU $5C88
S_POSNL EQU $5C8A
SCR_CT EQU $5C8C
ATTR_P EQU $5C8D
MASK_P EQU $5C8E
ATTR_T EQU $5C8F
MASK_T EQU $5C90
P_FLAG EQU $5C91
MEMBOT EQU $5C92
NMIADD EQU $5CB0
RAMTOP EQU $5CB2
P_RAMT EQU $5CB4
CHINFO EQU $5CB6

  ORG $0000

; THE 'START'
;
; It all starts here when the Spectrum is powered on.
;
; The maskable interrupt is disabled and the DE register pair set to hold the 'top of possible RAM'.
START:
  DI                      ; Disable the 'keyboard interrupt'.
  XOR A                   ; +00 for start (but +FF for 'NEW').
  LD DE,$FFFF             ; Top of possible RAM.
  JP START_NEW            ; Jump forward.

; THE 'ERROR' RESTART
;
; Used by the routines at BEEP, SA_LD_RET, SAVE_ETC, LD_BLOCK, PO_SCR, COPY_LINE, REPORT_J, WAIT_KEY, CHAN_OPEN, STR_DATA, OPEN_2, STMT_RET, LINE_NEW, NEXT_LINE, VAR_A_1, NEXT_2NUM, STOP, FOR, NEXT, READ_3, FIND_INT1, CLEAR, RETURN, IN_ASSIGN, CO_TEMP_1, DRAW_LINE, S_FN_SBRN, STK_VAR, multiply, usr, chrs, exp and ln.
;
; The error pointer is made to point to the position of the error.
ERROR_1:
  LD HL,(CH_ADD)          ; The address reached by the interpreter (CH-ADD) is copied to the error pointer (X-PTR) before proceeding.
  LD (X_PTR),HL           ;
  JR ERROR_2              ;

; THE 'PRINT A CHARACTER' RESTART
;
; Used by the routines at SAVE_ETC, PO_SAVE, MAIN_EXEC, LIST, OUT_LINE, OUT_SP_2, PRINT_CR, PR_ITEM_1, PR_POSN_1, CO_TEMP_1 and PRINT_FP.
;
; A Code of the character to be printed
PRINT_A_1:
  JP PRINT_A_2            ; Jump forward immediately.

; Unused
  DEFS $05,$FF            ; Unused locations.

; THE 'COLLECT CHARACTER' RESTART
;
; Used by the routines at SAVE_ETC, MAIN_EXEC, LIST, STMT_LOOP, SEPARATOR, STMT_NEXT, CLASS_09, READ_3, PRINT_2, PR_ITEM_1, PR_POSN_1, INPUT, IN_ASSIGN, CO_TEMP_1, CIRCLE, DRAW, SCANNING, S_2_COORD, S_QUOTE, S_DECIMAL, S_LETTER, S_FN_SBRN, LOOK_VARS, STK_VAR, SLICING, DIM, DEC_TO_FP and val.
;
; The contents of the location currently addressed by CH-ADD are fetched. A return is made if the value represents a printable character, otherwise CH-ADD is incremented and the tests repeated.
;
; O:A Code of the character
GET_CHAR:
  LD HL,(CH_ADD)          ; Fetch the value that is addressed by CH-ADD.
  LD A,(HL)               ;
; This entry point is used by the routine at NEXT_CHAR.
TEST_CHAR:
  CALL SKIP_OVER          ; Find out if the character is printable.
  RET NC                  ; Return if it is so.
; This routine continues into NEXT_CHAR.

; THE 'COLLECT NEXT CHARACTER' RESTART
;
; Used by the routines at SAVE_ETC, LIST, EACH_STMT, E_LINE_NO, STMT_LOOP, SEPARATOR, NEXT_2NUM, FOR, READ_3, DATA, DEF_FN, PR_ITEM_1, PR_POSN_1, STR_ALTER, INPUT, CO_TEMP_1, CIRCLE, DRAW, S_2_COORD, S_U_PLUS, S_BRACKET, S_PI, S_INKEY, S_SCREEN, S_ATTR, S_POINT, S_LETTER, S_FN_SBRN, LOOK_VARS, STK_VAR, SLICING, DIM and DEC_TO_FP.
;
; The routine at GET_CHAR continues here.
;
; As a BASIC line is interpreted, this routine is called repeatedly to step along the line.
;
; O:A Code of the next character
NEXT_CHAR:
  CALL CH_ADD_1           ; CH-ADD needs to be incremented.
  JR TEST_CHAR            ; Jump back to test the new value.

; Unused
  DEFS $03,$FF            ; Unused locations.

; THE 'CALCULATOR' RESTART
;
; Used by the routines at BEEP, OPEN, FETCH_NUM, IF_CMD, FOR, NEXT, NEXT_LOOP, CIRCLE, DRAW, CD_PRMS1, S_RND, S_PI, S_LETTER, LET, DEC_TO_FP, STACK_BC, INT_TO_FP, e_to_fp, FP_TO_BC, LOG_2_A, PRINT_FP, compare, n_mod_m, int, exp, ln, get_argt, cos, sin, tan, atn, asn, acs, sqr and to_power.
;
; The floating point calculator is entered at CALCULATE.
FP_CALC:
  JP CALCULATE            ; Jump forward immediately.

; Unused
  DEFS $05,$FF            ; Unused locations.

; THE 'MAKE BC SPACES' RESTART
;
; Used by the routines at SAVE_ETC, ME_CONTRL, INPUT, S_SCRN_S, S_QUOTE, S_INKEY, LET, strs_add, chrs, val, str and read_in.
;
; This routine creates free locations in the work space.
;
;   BC Number of free locations to create
; O:DE Address of the first byte of new free space
; O:HL Address of the last byte of new free space
BC_SPACES:
  PUSH BC                 ; Save the 'number'.
  LD HL,(WORKSP)          ; Fetch the present address of the start of the work space (WORKSP) and save that also before proceeding.
  PUSH HL                 ;
  JP RESERVE              ;

; THE 'MASKABLE INTERRUPT' ROUTINE
;
; The real time clock is incremented and the keyboard scanned whenever a maskable interrupt occurs.
MASK_INT:
  PUSH AF                 ; Save the current values held in these registers.
  PUSH HL                 ;
  LD HL,(FRAMES)          ; The lower two bytes of the frame counter (FRAMES) are incremented every 20 ms. (U.K.) The highest byte of the frame counter is only incremented when the value of the lower two bytes is zero.
  INC HL                  ;
  LD (FRAMES),HL          ;
  LD A,H                  ;
  OR L                    ;
  JR NZ,KEY_INT           ;
  INC (IY+$40)            ;
KEY_INT:
  PUSH BC                 ; Save the current values held in these registers.
  PUSH DE                 ;
  CALL KEYBOARD           ; Now scan the keyboard.
  POP DE                  ; Restore the values.
  POP BC                  ;
  POP HL                  ;
  POP AF                  ;
  EI                      ; The maskable interrupt is enabled before returning.
  RET                     ;

; THE 'ERROR-2' ROUTINE
;
; Used by the routine at ERROR_1.
;
; The return address to the interpreter points to the 'DEFB' that signifies which error has occurred. This 'DEFB' is fetched and transferred to ERR-NR.
;
; The machine stack is cleared before jumping forward to clear the calculator stack.
ERROR_2:
  POP HL                  ; The address on the stack points to the error code.
  LD L,(HL)               ;
; This entry point is used by the routine at TEST_ROOM.
ERROR_3:
  LD (IY+$00),L           ; It is transferred to ERR-NR.
  LD SP,(ERR_SP)          ; The machine stack is cleared by setting the stack pointer to ERR-SP before exiting via SET_STK.
  JP SET_STK              ;

; Unused
  DEFS $07,$FF            ; Unused locations.

; THE 'NON-MASKABLE INTERRUPT' ROUTINE
;
; This routine is not used in the standard Spectrum but the code allows for a system reset to occur following activation of the NMI line. The system variable at 5CB0, named here NMIADD, has to have the value zero for the reset to occur.
RESET:
  PUSH AF                 ; Save the current values held in these registers.
  PUSH HL                 ;
  LD HL,(NMIADD)          ; The two bytes of NMIADD must both be zero for the reset to occur.
  LD A,H                  ;
  OR L                    ;
  JR NZ,NO_RESET          ; Note: this should have been 'JR Z'!
  JP (HL)                 ; Jump to START.
NO_RESET:
  POP HL                  ; Restore the current values to these registers and return.
  POP AF                  ;
  RETN                    ;

; THE 'CH-ADD+1' SUBROUTINE
;
; Used by the routines at NEXT_CHAR, S_QUOTE_S and INT_TO_FP.
;
; The address held in CH-ADD is fetched, incremented and restored. The contents of the location now addressed by CH-ADD are fetched. The entry points of TEMP_PTR1 and TEMP_PTR2 are used to set CH-ADD for a temporary period.
;
; O:A Code of the character at CH-ADD+1
CH_ADD_1:
  LD HL,(CH_ADD)          ; Fetch the address from CH-ADD.
; This entry point is used by the routines at READ_3 and S_DECIMAL.
TEMP_PTR1:
  INC HL                  ; Increment the pointer.
; This entry point is used by the routine at READ_3.
TEMP_PTR2:
  LD (CH_ADD),HL          ; Set CH-ADD.
  LD A,(HL)               ; Fetch the addressed value and then return.
  RET                     ;

; THE 'SKIP-OVER' SUBROUTINE
;
; Used by the routine at GET_CHAR.
;
; The value brought to the subroutine in the A register is tested to see if it is printable. Various special codes lead to HL being incremented once or twice, and CH-ADD amended accordingly.
;
; A Character code
SKIP_OVER:
  CP $21                  ; Return with the carry flag reset if ordinary character code.
  RET NC                  ;
  CP $0D                  ; Return if the end of the line has been reached.
  RET Z                   ;
  CP $10                  ; Return with codes +00 to +0F but with carry set.
  RET C                   ;
  CP $18                  ; Return with codes +18 to +20 again with carry set.
  CCF                     ;
  RET C                   ;
  INC HL                  ; Skip over once.
  CP $16                  ; Jump forward with codes +10 to +15 (INK to OVER).
  JR C,SKIPS              ;
  INC HL                  ; Skip over once more (AT and TAB).
SKIPS:
  SCF                     ; Return with the carry flag set and CH-ADD holding the appropriate address.
  LD (CH_ADD),HL          ;
  RET                     ;

; THE TOKEN TABLE
;
; Used by the routine at PO_MSG.
;
; All the tokens used by the Spectrum are expanded by reference to this table. The last code of each token is 'inverted' by having its bit 7 set.
TOKENS:
  DEFM "?"+$80            ; ?
  DEFM "RN","D"+$80       ; RND
  DEFM "INKEY","$"+$80    ; INKEY$
  DEFM "P","I"+$80        ; PI
  DEFM "F","N"+$80        ; FN
  DEFM "POIN","T"+$80     ; POINT
  DEFM "SCREEN","$"+$80   ; SCREEN$
  DEFM "ATT","R"+$80      ; ATTR
  DEFM "A","T"+$80        ; AT
  DEFM "TA","B"+$80       ; TAB
  DEFM "VAL","$"+$80      ; VAL$
  DEFM "COD","E"+$80      ; CODE
  DEFM "VA","L"+$80       ; VAL
  DEFM "LE","N"+$80       ; LEN
  DEFM "SI","N"+$80       ; SIN
  DEFM "CO","S"+$80       ; COS
  DEFM "TA","N"+$80       ; TAN
  DEFM "AS","N"+$80       ; ASN
  DEFM "AC","S"+$80       ; ACS
  DEFM "AT","N"+$80       ; ATN
  DEFM "L","N"+$80        ; LN
  DEFM "EX","P"+$80       ; EXP
  DEFM "IN","T"+$80       ; INT
  DEFM "SQ","R"+$80       ; SQR
  DEFM "SG","N"+$80       ; SGN
  DEFM "AB","S"+$80       ; ABS
  DEFM "PEE","K"+$80      ; PEEK
  DEFM "I","N"+$80        ; IN
  DEFM "US","R"+$80       ; USR
  DEFM "STR","$"+$80      ; STR$
  DEFM "CHR","$"+$80      ; CHR$
  DEFM "NO","T"+$80       ; NOT
  DEFM "BI","N"+$80       ; BIN
  DEFM "O","R"+$80        ; OR
  DEFM "AN","D"+$80       ; AND
  DEFM "<","="+$80        ; <=
  DEFM ">","="+$80        ; >=
  DEFM "<",">"+$80        ; <>
  DEFM "LIN","E"+$80      ; LINE
  DEFM "THE","N"+$80      ; THEN
  DEFM "T","O"+$80        ; TO
  DEFM "STE","P"+$80      ; STEP
  DEFM "DEF F","N"+$80    ; DEF FN
  DEFM "CA","T"+$80       ; CAT
  DEFM "FORMA","T"+$80    ; FORMAT
  DEFM "MOV","E"+$80      ; MOVE
  DEFM "ERAS","E"+$80     ; ERASE
  DEFM "OPEN ","#"+$80    ; OPEN #
  DEFM "CLOSE ","#"+$80   ; CLOSE #
  DEFM "MERG","E"+$80     ; MERGE
  DEFM "VERIF","Y"+$80    ; VERIFY
  DEFM "BEE","P"+$80      ; BEEP
  DEFM "CIRCL","E"+$80    ; CIRCLE
  DEFM "IN","K"+$80       ; INK
  DEFM "PAPE","R"+$80     ; PAPER
  DEFM "FLAS","H"+$80     ; FLASH
  DEFM "BRIGH","T"+$80    ; BRIGHT
  DEFM "INVERS","E"+$80   ; INVERSE
  DEFM "OVE","R"+$80      ; OVER
  DEFM "OU","T"+$80       ; OUT
  DEFM "LPRIN","T"+$80    ; LPRINT
  DEFM "LLIS","T"+$80     ; LLIST
  DEFM "STO","P"+$80      ; STOP
  DEFM "REA","D"+$80      ; READ
  DEFM "DAT","A"+$80      ; DATA
  DEFM "RESTOR","E"+$80   ; RESTORE
  DEFM "NE","W"+$80       ; NEW
  DEFM "BORDE","R"+$80    ; BORDER
  DEFM "CONTINU","E"+$80  ; CONTINUE
  DEFM "DI","M"+$80       ; DIM
  DEFM "RE","M"+$80       ; REM
  DEFM "FO","R"+$80       ; FOR
  DEFM "GO T","O"+$80     ; GO TO
  DEFM "GO SU","B"+$80    ; GO SUB
  DEFM "INPU","T"+$80     ; INPUT
  DEFM "LOA","D"+$80      ; LOAD
  DEFM "LIS","T"+$80      ; LIST
  DEFM "LE","T"+$80       ; LET
  DEFM "PAUS","E"+$80     ; PAUSE
  DEFM "NEX","T"+$80      ; NEXT
  DEFM "POK","E"+$80      ; POKE
  DEFM "PRIN","T"+$80     ; PRINT
  DEFM "PLO","T"+$80      ; PLOT
  DEFM "RU","N"+$80       ; RUN
  DEFM "SAV","E"+$80      ; SAVE
  DEFM "RANDOMIZ","E"+$80 ; RANDOMIZE
  DEFM "I","F"+$80        ; IF
  DEFM "CL","S"+$80       ; CLS
  DEFM "DRA","W"+$80      ; DRAW
  DEFM "CLEA","R"+$80     ; CLEAR
  DEFM "RETUR","N"+$80    ; RETURN
  DEFM "COP","Y"+$80      ; COPY

; THE KEY TABLES
;
; Used by the routines at K_TEST and K_DECODE.
;
; There are six separate key tables. The final character code obtained depends on the particular key pressed and the 'mode' being used.
;
; (a) The main key table - L mode and CAPS SHIFT.
KEYTABLE_A:
  DEFB $42                ; B
  DEFB $48                ; H
  DEFB $59                ; Y
  DEFB $36                ; 6
  DEFB $35                ; 5
  DEFB $54                ; T
  DEFB $47                ; G
  DEFB $56                ; V
  DEFB $4E                ; N
  DEFB $4A                ; J
  DEFB $55                ; U
  DEFB $37                ; 7
  DEFB $34                ; 4
  DEFB $52                ; R
  DEFB $46                ; F
  DEFB $43                ; C
  DEFB $4D                ; M
  DEFB $4B                ; K
  DEFB $49                ; I
  DEFB $38                ; 8
  DEFB $33                ; 3
  DEFB $45                ; E
  DEFB $44                ; D
  DEFB $58                ; X
  DEFB $0E                ; SYMBOL SHIFT
  DEFB $4C                ; L
  DEFB $4F                ; O
  DEFB $39                ; 9
  DEFB $32                ; 2
  DEFB $57                ; W
  DEFB $53                ; S
  DEFB $5A                ; Z
  DEFB $20                ; SPACE
  DEFB $0D                ; ENTER
  DEFB $50                ; P
  DEFB $30                ; 0
  DEFB $31                ; 1
  DEFB $51                ; Q
  DEFB $41                ; A
; (b) Extended mode. Letter keys and unshifted.
KEYTABLE_B:
  DEFB $E3                ; READ
  DEFB $C4                ; BIN
  DEFB $E0                ; LPRINT
  DEFB $E4                ; DATA
  DEFB $B4                ; TAN
  DEFB $BC                ; SGN
  DEFB $BD                ; ABS
  DEFB $BB                ; SQR
  DEFB $AF                ; CODE
  DEFB $B0                ; VAL
  DEFB $B1                ; LEN
  DEFB $C0                ; USR
  DEFB $A7                ; PI
  DEFB $A6                ; INKEY$
  DEFB $BE                ; PEEK
  DEFB $AD                ; TAB
  DEFB $B2                ; SIN
  DEFB $BA                ; INT
  DEFB $E5                ; RESTORE
  DEFB $A5                ; RND
  DEFB $C2                ; CHR$
  DEFB $E1                ; LLIST
  DEFB $B3                ; COS
  DEFB $B9                ; EXP
  DEFB $C1                ; STR$
  DEFB $B8                ; LN
; (c) Extended mode. Letter keys and either shift.
KEYTABLE_C:
  DEFB $7E                ; ~
  DEFB $DC                ; BRIGHT
  DEFB $DA                ; PAPER
  DEFB $5C                ; \
  DEFB $B7                ; ATN
  DEFB $7B                ; {
  DEFB $7D                ; }
  DEFB $D8                ; CIRCLE
  DEFB $BF                ; IN
  DEFB $AE                ; VAL$
  DEFB $AA                ; SCREEN$
  DEFB $AB                ; ATTR
  DEFB $DD                ; INVERSE
  DEFB $DE                ; OVER
  DEFB $DF                ; OUT
  DEFB $7F                ; ©
  DEFB $B5                ; ASN
  DEFB $D6                ; VERIFY
  DEFB $7C                ; |
  DEFB $D5                ; MERGE
  DEFB $5D                ; ]
  DEFB $DB                ; FLASH
  DEFB $B6                ; ACS
  DEFB $D9                ; INK
  DEFB $5B                ; [
  DEFB $D7                ; BEEP
; (d) Control codes. Digit keys and CAPS SHIFT.
KEYTABLE_D:
  DEFB $0C                ; DELETE
  DEFB $07                ; EDIT
  DEFB $06                ; CAPS LOCK
  DEFB $04                ; TRUE VIDEO
  DEFB $05                ; INV. VIDEO
  DEFB $08                ; Cursor left
  DEFB $0A                ; Cursor down
  DEFB $0B                ; Cursor up
  DEFB $09                ; Cursor right
  DEFB $0F                ; GRAPHICS
; (e) Symbol code. Letter keys and symbol shift.
KEYTABLE_E:
  DEFB $E2                ; STOP
  DEFB $2A                ; *
  DEFB $3F                ; ?
  DEFB $CD                ; STEP
  DEFB $C8                ; >=
  DEFB $CC                ; TO
  DEFB $CB                ; THEN
  DEFB $5E                ; power
  DEFB $AC                ; AT
  DEFB $2D                ; -
  DEFB $2B                ; +
  DEFB $3D                ; =
  DEFB $2E                ; .
  DEFB $2C                ; ,
  DEFB $3B                ; ;
  DEFB $22                ; "
  DEFB $C7                ; <=
  DEFB $3C                ; <
  DEFB $C3                ; NOT
  DEFB $3E                ; >
  DEFB $C5                ; OR
  DEFB $2F                ; /
  DEFB $C9                ; <>
  DEFB $60                ; CHR163
  DEFB $C6                ; AND
  DEFB $3A                ; :
; (f) Extended mode. Digit keys and symbol shift.
KEYTABLE_F:
  DEFB $D0                ; FORMAT
  DEFB $CE                ; DEF FN
  DEFB $A8                ; FN
  DEFB $CA                ; LINE
  DEFB $D3                ; OPEN
  DEFB $D4                ; CLOSE
  DEFB $D1                ; MOVE
  DEFB $D2                ; ERASE
  DEFB $A9                ; POINT
  DEFB $CF                ; CAT

; THE 'KEYBOARD SCANNING' SUBROUTINE
;
; This very important subroutine is called by both the main keyboard subroutine (KEYBOARD) and the INKEY$ routine (S_INKEY).
;
; In all instances the E register is returned with a value in the range of +00 to +27, the value being different for each of the forty keys of the keyboard, or the value +FF, for no-key.
;
; The D register is returned with a value that indicates which single shift key is being pressed. If both shift keys are being pressed then the D and E registers are returned with the values for the CAPS SHIFT and SYMBOL SHIFT keys respectively.
;
; If no key is being pressed then the DE register pair is returned holding +FFFF.
;
; The zero flag is returned reset if more than two keys are being pressed, or neither key of a pair of keys is a shift key.
;
; O:D Shift key pressed (+18 or +27), or +FF if no shift key pressed
; O:E Other key pressed (+00 to +27), or +FF if no other key pressed
; O:F Zero flag reset if an invalid combination of keys is pressed
KEY_SCAN:
  LD L,$2F                ; The initial key value for each line will be +2F, +2E,..., +28. (Eight lines.)
  LD DE,$FFFF             ; Initialise DE to 'no-key'.
  LD BC,$FEFE             ; C=port address, B=counter.
; Now enter a loop. Eight passes are made with each pass having a different initial key value and scanning a different line of five keys. (The first line is CAPS SHIFT, Z, X, C, V.)
KEY_LINE:
  IN A,(C)                ; Read from the port specified.
  CPL                     ; A pressed key in the line will set its respective bit, from bit 0 (outer key) to bit 4 (inner key).
  AND $1F                 ;
  JR Z,KEY_DONE           ; Jump forward if none of the five keys in the line are being pressed.
  LD H,A                  ; The key-bits go to the H register whilst the initial key value is fetched.
  LD A,L                  ;
KEY_3KEYS:
  INC D                   ; If three keys are being pressed on the keyboard then the D register will no longer hold +FF - so return if this happens.
  RET NZ                  ;
KEY_BITS:
  SUB $08                 ; Repeatedly subtract 8 from the present key value until a key-bit is found.
  SRL H                   ;
  JR NC,KEY_BITS          ;
  LD D,E                  ; Copy any earlier key value to the D register.
  LD E,A                  ; Pass the new key value to the E register.
  JR NZ,KEY_3KEYS         ; If there is a second, or possibly a third, pressed key in this line then jump back.
KEY_DONE:
  DEC L                   ; The line has been scanned so the initial key value is reduced for the next pass.
  RLC B                   ; The counter is shifted and the jump taken if there are still lines to be scanned.
  JR C,KEY_LINE           ;
; Four tests are now made.
  LD A,D                  ; Accept any key value which still has the D register holding +FF, i.e. a single key pressed or 'no-key'.
  INC A                   ;
  RET Z                   ;
  CP $28                  ; Accept the key value for a pair of keys if the D key is CAPS SHIFT.
  RET Z                   ;
  CP $19                  ; Accept the key value for a pair of keys if the D key is SYMBOL SHIFT.
  RET Z                   ;
  LD A,E                  ; It is however possible for the E key of a pair to be SYMBOL SHIFT - so this has to be considered.
  LD E,D                  ;
  LD D,A                  ;
  CP $18                  ;
  RET                     ; Return with the zero flag set if it was SYMBOL SHIFT and 'another key'; otherwise reset.

; THE 'KEYBOARD' SUBROUTINE
;
; Used by the routine at MASK_INT.
;
; This subroutine is called on every occasion that a maskable interrupt occurs. In normal operation this will happen once every 20 ms. The purpose of this subroutine is to scan the keyboard and decode the key value. The code produced will, if the 'repeat' status allows it, be passed to the system variable LAST-K. When a code is put into this system variable bit 5 of FLAGS is set to show that a 'new' key has been pressed.
KEYBOARD:
  CALL KEY_SCAN           ; Fetch a key value in the DE register pair but return immediately if the zero flag is reset.
  RET NZ                  ;
; A double system of 'KSTATE system variables' (KSTATE0-KSTATE3 and KSTATE4-KSTATE7) is used from now on.
;
; The two sets allow for the detection of a new key being pressed (using one set) whilst still within the 'repeat period' of the previous key to have been pressed (details in the other set).
;
; A set will only become free to handle a new key if the key is held down for about 1/10th. of a second, i.e. five calls to KEYBOARD.
  LD HL,KSTATE            ; Start with KSTATE0.
K_ST_LOOP:
  BIT 7,(HL)              ; Jump forward if a 'set is free', i.e. KSTATE0/4 holds +FF.
  JR NZ,K_CH_SET          ;
  INC HL                  ; However if the set is not free decrease its '5 call counter' and when it reaches zero signal the set as free.
  DEC (HL)                ;
  DEC HL                  ;
  JR NZ,K_CH_SET          ;
  LD (HL),$FF             ;
; After considering the first set change the pointer and consider the second set.
K_CH_SET:
  LD A,L                  ; Fetch the low byte of the address and jump back if the second set (KSTATE4) has still to be considered.
  LD HL,$5C04             ;
  CP L                    ;
  JR NZ,K_ST_LOOP         ;
; Return now if the key value indicates 'no-key' or a shift key only.
  CALL K_TEST             ; Make the necessary tests and return if needed. Also change the key value to a 'main code'.
  RET NC                  ;
; A key stroke that is being repeated (held down) is now separated from a new key stroke.
  LD HL,KSTATE            ; Look first at KSTATE0.
  CP (HL)                 ; Jump forward if the codes match - indicating a repeat.
  JR Z,K_REPEAT           ;
  EX DE,HL                ; Save the address of KSTATE0.
  LD HL,$5C04             ; Now look at KSTATE4.
  CP (HL)                 ; Jump forward if the codes match - indicating a repeat.
  JR Z,K_REPEAT           ;
; But a new key will not be accepted unless one of the sets of KSTATE system variables is 'free'.
  BIT 7,(HL)              ; Consider the second set.
  JR NZ,K_NEW             ; Jump forward if 'free'.
  EX DE,HL                ; Now consider the first set.
  BIT 7,(HL)              ; Continue if the set is 'free' but exit if not.
  RET Z                   ;
; The new key is to be accepted. But before the system variable LAST-K can be filled, the KSTATE system variables, of the set being used, have to be initialised to handle any repeats and the key's code has to be decoded.
K_NEW:
  LD E,A                  ; The code is passed to the E register and to KSTATE0/4.
  LD (HL),A               ;
  INC HL                  ; The '5 call counter' for this set is reset to '5'.
  LD (HL),$05             ;
  INC HL                  ; The third system variable of the set holds the REPDEL value (normally 0.7 secs.).
  LD A,(REPDEL)           ;
  LD (HL),A               ;
  INC HL                  ; Point to KSTATE3/7.
; The decoding of a 'main code' depends upon the present state of MODE, bit 3 of FLAGS and the 'shift byte'.
  LD C,(IY+$07)           ; Fetch MODE.
  LD D,(IY+$01)           ; Fetch FLAGS.
  PUSH HL                 ; Save the pointer whilst the 'main code' is decoded.
  CALL K_DECODE           ;
  POP HL                  ;
  LD (HL),A               ; The final code value is saved in KSTATE3/7, from where it is collected in case of a repeat.
; The next three instructions are common to the handling of both 'new keys' and 'repeat keys'.
K_END:
  LD (LAST_K),A           ; Enter the final code value into LAST-K and signal 'a new key' by setting bit 5 of FLAGS.
  SET 5,(IY+$01)          ;
  RET                     ; Finally return.
; A key will 'repeat' on the first occasion after the delay period (REPDEL - normally 0.7s) and on subsequent occasions after the delay period (REPPER - normally 0.1s).
K_REPEAT:
  INC HL                  ; Point to the '5 call counter' of the set being used and reset it to 5.
  LD (HL),$05             ;
  INC HL                  ; Point to the third system variable - the REPDEL/REPPER value - and decrement it.
  DEC (HL)                ;
  RET NZ                  ; Exit from the KEYBOARD subroutine if the delay period has not passed.
  LD A,(REPPER)           ; However once it has passed the delay period for the next repeat is to be REPPER.
  LD (HL),A               ;
  INC HL                  ; The repeat has been accepted so the final code value is fetched from KSTATE3/7 and passed to K_END.
  LD A,(HL)               ;
  JR K_END                ;

; THE 'K-TEST' SUBROUTINE
;
; Used by the routines at KEYBOARD and S_INKEY.
;
; The key value is tested and a return made if 'no-key' or 'shift-only'; otherwise the 'main code' for that key is found.
;
;   D Shift key pressed (+18 or +27), or +FF if no shift key pressed
;   E Other key pressed (+00 to +27), or +FF if no other key pressed
; O:A Main code (from the main key table)
; O:F Carry flag reset if an invalid combination of keys is pressed
K_TEST:
  LD B,D                  ; Copy the shift byte.
  LD D,$00                ; Clear the D register for later.
  LD A,E                  ; Move the key number.
  CP $27                  ; Return now if the key was 'CAPS SHIFT' only or 'no-key'.
  RET NC                  ;
  CP $18                  ; Jump forward unless the E key was SYMBOL SHIFT.
  JR NZ,K_MAIN            ;
  BIT 7,B                 ; However accept SYMBOL SHIFT and another key; return with SYMBOL SHIFT only.
  RET NZ                  ;
; The 'main code' is found by indexing into the main key table.
K_MAIN:
  LD HL,KEYTABLE_A        ; The base address of the main key table.
  ADD HL,DE               ; Index into the table and fetch the 'main code'.
  LD A,(HL)               ;
  SCF                     ; Signal 'valid keystroke' before returning.
  RET                     ;

; THE 'KEYBOARD DECODING' SUBROUTINE
;
; Used by the routines at KEYBOARD and S_INKEY.
;
; This subroutine is entered with the 'main code' in the E register, the value of FLAGS in the D register, the value of MODE in the C register and the 'shift byte' in the B register.
;
; By considering these four values and referring, as necessary, to the six key tables a 'final code' is produced. This is returned in the A register.
;
;   B Shift key pressed (+18 or +27), or +FF if no shift key pressed
;   C MODE
;   D FLAGS
;   E Main code (from the main key table)
; O:A Final code (from the key tables)
K_DECODE:
  LD A,E                  ; Copy the 'main code'.
  CP $3A                  ; Jump forward if a digit key is being considered; also SPACE, ENTER and both shifts.
  JR C,K_DIGIT            ;
  DEC C                   ; Decrement the MODE value.
  JP M,K_KLC_LET          ; Jump forward, as needed, for modes 'K', 'L', 'C' and 'E'.
  JR Z,K_E_LET            ;
; Only 'graphics' mode remains and the 'final code' for letter keys in graphics mode is computed from the 'main code'.
  ADD A,$4F               ; Add the offset.
  RET                     ; Return with the 'final code'.
; Letter keys in extended mode are considered next.
K_E_LET:
  LD HL,$01EB             ; The base address for table 'b'.
  INC B                   ; Jump forward to use this table if neither shift key is being pressed.
  JR Z,K_LOOK_UP          ;
  LD HL,KEYTABLE_A        ; Otherwise use the base address for table 'c'.
; Key tables 'b-f' are all served by the following look-up routine. In all cases a 'final code' is found and returned.
K_LOOK_UP:
  LD D,$00                ; Clear the D register.
  ADD HL,DE               ; Index the required table and fetch the 'final code'.
  LD A,(HL)               ;
  RET                     ; Then return.
; Letter keys in 'K', 'L' or 'C' modes are now considered. But first the special SYMBOL SHIFT codes have to be dealt with.
K_KLC_LET:
  LD HL,$0229             ; The base address for table 'e'.
  BIT 0,B                 ; Jump back if using the SYMBOL SHIFT key and a letter key.
  JR Z,K_LOOK_UP          ;
  BIT 3,D                 ; Jump forward if currently in 'K' mode.
  JR Z,K_TOKENS           ;
  BIT 3,(IY+$30)          ; If CAPS LOCK is set (bit 3 of FLAGS2 set) then return with the 'main code'.
  RET NZ                  ;
  INC B                   ; Also return in the same manner if CAPS SHIFT is being pressed.
  RET NZ                  ;
  ADD A,$20               ; However if lower case codes are required then +20 has to be added to the 'main code' to give the correct 'final code'.
  RET                     ;
; The 'final code' values for tokens are found by adding +A5 to the 'main code'.
K_TOKENS:
  ADD A,$A5               ; Add the required offset and return.
  RET                     ;
; Next the digit keys, SPACE, ENTER and both shifts are considered.
K_DIGIT:
  CP "0"                  ; Proceed only with the digit keys, i.e. return with SPACE (+20), ENTER (+0D) and both shifts (+0E).
  RET C                   ;
  DEC C                   ; Now separate the digit keys into three groups - according to the mode.
  JP M,K_KLC_DGT          ; Jump with 'K', 'L' and 'C' modes, and also with 'G' mode. Continue with 'E' mode.
  JR NZ,K_GRA_DGT         ;
  LD HL,$0254             ; The base address for table 'f'.
  BIT 5,B                 ; Use this table for SYMBOL SHIFT and a digit key in extended mode.
  JR Z,K_LOOK_UP          ;
  CP "8"                  ; Jump forward with digit keys '8' and '9'.
  JR NC,K_8_9             ;
; The digit keys '0' to '7' in extended mode are to give either a 'paper colour code' or an 'ink colour code' depending on the use of CAPS SHIFT.
  SUB $20                 ; Reduce the range +30 to +37 giving +10 to +17.
  INC B                   ; Return with this 'paper colour code' if CAPS SHIFT is not being used.
  RET Z                   ;
  ADD A,$08               ; But if it is then the range is to be +18 to +1F instead - indicating an 'ink colour code'.
  RET                     ;
; The digit keys '8' and '9' are to give 'BRIGHT' and 'FLASH' codes.
K_8_9:
  SUB $36                 ; +38 and +39 go to +02 and +03.
  INC B                   ; Return with these codes if CAPS SHIFT is not being used. (These are 'BRIGHT' codes.)
  RET Z                   ;
  ADD A,$FE               ; Subtract '2' if CAPS SHIFT is being used; giving +00 and +01 (as 'FLASH' codes).
  RET                     ;
; The digit keys in graphics mode are to give the block graphic characters (+80 to +8F), the GRAPHICS code (+0F) and the DELETE code (+0C).
K_GRA_DGT:
  LD HL,$0230             ; The base address of table 'd'.
  CP "9"                  ; Use this table directly for both digit key '9' that is to give GRAPHICS, and digit key '0' that is to give DELETE.
  JR Z,K_LOOK_UP          ;
  CP "0"                  ;
  JR Z,K_LOOK_UP          ;
  AND $07                 ; For keys '1' to '8' make the range +80 to +87.
  ADD A,$80               ;
  INC B                   ; Return with a value from this range if neither shift key is being pressed.
  RET Z                   ;
  XOR $0F                 ; But if 'shifted' make the range +88 to +8F.
  RET                     ;
; Finally consider the digit keys in 'K', 'L' and 'C' modes.
K_KLC_DGT:
  INC B                   ; Return directly if neither shift key is being used. (Final codes +30 to +39.)
  RET Z                   ;
  BIT 5,B                 ; Use table 'd' if the CAPS SHIFT key is also being pressed.
  LD HL,$0230             ;
  JR NZ,K_LOOK_UP         ;
; The codes for the various digit keys and SYMBOL SHIFT can now be found.
  SUB $10                 ; Reduce the range to give +20 to +29.
  CP $22                  ; Separate the '@' character from the others.
  JR Z,K_AT_CHAR          ;
  CP $20                  ; The '_' character has also to be separated.
  RET NZ                  ; Return now with the 'final codes' +21, +23 to +29.
  LD A,"_"                ; Give the '_' character a code of +5F.
  RET                     ;
K_AT_CHAR:
  LD A,"@"                ; Give the '@' character a code of +40.
  RET                     ;

; THE 'BEEPER' SUBROUTINE
;
; Used by the routines at BEEP, EDITOR, ED_ERROR and ED_COPY.
;
; The loudspeaker is activated by having D4 low during an 'OUT' instruction that is using port +FE. When D4 is high in a similar situation the loudspeaker is deactivated. A 'beep' can therefore be produced by regularly changing the level of D4.
;
; Consider now the note 'middle C' which has the frequency 261.63 Hz. In order to get this note the loudspeaker will have to be alternately activated and deactivated every 1/523.26th of a second. In the Spectrum the system clock is set to run at 3.5 MHz. and the note of 'middle C' will require that the requisite 'OUT' instruction be executed as close as possible to every 6689 T states. This last value, when reduced slightly for unavoidable overheads, represents the 'length of the timing loop' in
; this subroutine.
;
; This subroutine is entered with the DE register pair holding the value 'f*t', where a note of given frequency 'f' is to have a duration of 't' seconds, and the HL register pair holding a value equal to the number of T states in the 'timing loop' divided by 4, i.e. for the note 'middle C' to be produced for one second DE holds +0105 (INT(261.3*1)) and HL holds +066A (derived from 6689/4-30.125).
;
; DE Number of passes to make through the sound generation loop
; HL Loop delay parameter
BEEPER:
  DI                      ; Disable the interrupt for the duration of a 'beep'.
  LD A,L                  ; Save L temporarily.
  SRL L                   ; Each '1' in the L register is to count 4 T states, but take INT (L/4) and count 16 T states instead.
  SRL L                   ;
  CPL                     ; Go back to the original value in L and find how many were lost by taking 3-(A mod 4).
  AND $03                 ;
  LD C,A                  ;
  LD B,$00                ;
  LD IX,$03D1             ; The base address of the timing loop.
  ADD IX,BC               ; Alter the length of the timing loop. Use an earlier starting point for each '1' lost by taking INT (L/4).
  LD A,(BORDCR)           ; Fetch the present border colour from BORDCR and move it to bits 2, 1 and 0 of the A register.
  AND $38                 ;
  RRCA                    ;
  RRCA                    ;
  RRCA                    ;
  OR $08                  ; Ensure the MIC output is 'off'.
; Now enter the sound generation loop. DE complete passes are made, i.e. a pass for each cycle of the note.
;
; The HL register holds the 'length of the timing loop' with 16 T states being used for each '1' in the L register and 1024 T states for each '1' in the H register.
  NOP                     ; Add 4 T states for each earlier entry point that is used.
  NOP                     ;
  NOP                     ;
  INC B                   ; The values in the B and C registers will come from the H and L registers - see below.
  INC C                   ;
BE_H_L_LP:
  DEC C                   ; The 'timing loop', i.e. BC*4 T states. (But note that at the half-cycle point, C will be equal to L+1.)
  JR NZ,BE_H_L_LP         ;
  LD C,$3F                ;
  DEC B                   ;
  JP NZ,BE_H_L_LP         ;
; The loudspeaker is now alternately activated and deactivated.
  XOR $10                 ; Flip bit 4.
  OUT ($FE),A             ; Perform the 'OUT' operation, leaving the border unchanged.
  LD B,H                  ; Reset the B register.
  LD C,A                  ; Save the A register.
  BIT 4,A                 ; Jump if at the half-cycle point.
  JR NZ,BE_AGAIN          ;
; After a full cycle the DE register pair is tested.
  LD A,D                  ; Jump forward if the last complete pass has been made already.
  OR E                    ;
  JR Z,BE_END             ;
  LD A,C                  ; Fetch the saved value.
  LD C,L                  ; Reset the C register.
  DEC DE                  ; Decrease the pass counter.
  JP (IX)                 ; Jump back to the required starting location of the loop.
; The parameters for the second half-cycle are set up.
BE_AGAIN:
  LD C,L                  ; Reset the C register.
  INC C                   ; Add 16 T states as this path is shorter.
  JP (IX)                 ; Jump back.
; Upon completion of the 'beep' the maskable interrupt has to be enabled.
BE_END:
  EI                      ; Enable interrupt.
  RET                     ; Finally return.

; THE 'BEEP' COMMAND ROUTINE
;
; The address of this routine is found in the parameter table.
;
; The subroutine is entered with two numbers on the calculator stack. The topmost number (P) represents the 'pitch' of the note and the number underneath it (t) represents the 'duration'.
BEEP:
  RST $28                 ; The floating-point calculator is used to manipulate the two values: t, P.
  DEFB $31                ; duplicate: t, P, P
  DEFB $27                ; int: t, P, i (where i=INT P)
  DEFB $C0                ; st_mem_0: t, P, i (mem-0 holds i)
  DEFB $03                ; subtract: t, p (where p is the fractional part of P)
  DEFB $34                 ; stk_data: Stack the decimal value K=0.0577622606 (which is a little below 12*(2↑0.5)-1)
  DEFB $EC,$6C,$98,$1F,$F5 ;
  DEFB $04                ; multiply: t, pK
  DEFB $A1                ; stk_one: t, pK, 1
  DEFB $0F                ; addition: t, pK+1
  DEFB $38                ; end_calc
; Now perform several tests on i, the integer part of the 'pitch'.
  LD HL,MEMBOT            ; This is 'mem-0-1st' (MEMBOT).
  LD A,(HL)               ; Fetch the exponent of i.
  AND A                   ; Give an error if i is not in the integral (short) form.
  JR NZ,REPORT_B          ;
  INC HL                  ; Copy the sign byte to the C register.
  LD C,(HL)               ;
  INC HL                  ; Copy the low-byte to the B register, and to the A register.
  LD B,(HL)               ;
  LD A,B                  ;
  RLA                     ; Again give report B if i does not satisfy the test: -128<=i<=+127.
  SBC A,A                 ;
  CP C                    ;
  JR NZ,REPORT_B          ;
  INC HL                  ;
  CP (HL)                 ;
  JR NZ,REPORT_B          ;
  LD A,B                  ; Fetch the low-byte and test it further.
  ADD A,$3C               ;
  JP P,BE_i_OK            ; Accept -60<=i<=67.
  JP PO,REPORT_B          ; Reject -128 to -61.
; Note: the range 70 to 127 will be rejected later on.
;
; The correct frequency for the 'pitch' i can now be found.
BE_i_OK:
  LD B,$FA                ; Start '6' octaves below middle C.
BE_OCTAVE:
  INC B                   ; Repeatedly reduce i in order to find the correct octave.
  SUB $0C                 ;
  JR NC,BE_OCTAVE         ;
  ADD A,$0C               ; Add back the last subtraction.
  PUSH BC                 ; Save the octave number.
  LD HL,SEMITONES         ; The base address of the 'semitone table'.
  CALL LOC_MEM            ; Consider the table and pass the 'A th.' value to the calculator stack. (Call it C.)
  CALL STACK_NUM          ;
; Now the fractional part of the 'pitch' can be taken into consideration.
  RST $28                 ; t, pK+1, C
  DEFB $04                ; multiply: t, C(pK+1)
  DEFB $38                ; end_calc
; The final frequency f is found by modifying the 'last value' according to the octave number.
  POP AF                  ; Fetch the octave number.
  ADD A,(HL)              ; Multiply the 'last value' by 2 to the power of the octave number.
  LD (HL),A               ;
  RST $28                 ; t, f
  DEFB $C0                ; st_mem_0: Copy the frequency (f) to mem-0
  DEFB $02                ; delete: t
; Attention is now turned to the 'duration'.
  DEFB $31                ; duplicate: t, t
  DEFB $38                ; end_calc
  CALL FIND_INT1          ; The value 'INT t' must be in the range +00 to +0A.
  CP $0B                  ;
  JR NC,REPORT_B          ;
; The number of complete cycles in the 'beep' is given by f*t so this value is now found.
  RST $28                 ; t
  DEFB $E0                ; get_mem_0: t, f
  DEFB $04                ; multiply: f*t
; The result is left on the calculator stack whilst the length of the 'timing loop' required for the 'beep' is computed.
  DEFB $E0                ; get_mem_0: f*t, f
  DEFB $34                 ; stk_data: Stack the value (3.5*10↑6)/8=437500
  DEFB $80,$43,$55,$9F,$80 ;
  DEFB $01                ; exchange: f*t, 437500, f
  DEFB $05                ; division: f*t, 437500/f
  DEFB $34                ; stk_data: f*t, 437500/f, 30.125
  DEFB $35,$71            ;
  DEFB $03                ; subtract: f*t, 437500/f-30.125
  DEFB $38                ; end_calc
; Note: the value 437500/f gives the 'half-cycle' length of the note and reducing it by 30.125 allows for 120.5 T states in which to actually produce the note and adjust the counters etc.
;
; The values can now be transferred to the required registers.
  CALL FIND_INT2          ; The 'timing loop' value is compressed into the BC register pair and saved.
  PUSH BC                 ;
; Note: if the timing loop value is too large then an error will occur (returning via ERROR_1), thereby excluding 'pitch' values of 70 to 127.
  CALL FIND_INT2          ; The f*t value is compressed into the BC register pair.
  POP HL                  ; Move the 'timing loop' value to the HL register pair.
  LD D,B                  ; Move the f*t value to the DE register pair.
  LD E,C                  ;
; However before making the 'beep' test the value f*t.
  LD A,D                  ; Return if f*t has given the result of 'no cycles' required.
  OR E                    ;
  RET Z                   ;
  DEC DE                  ; Decrease the cycle number and jump to BEEPER (making at least one pass).
  JP BEEPER               ;
; Report B - integer out of range.
REPORT_B:
  RST $08                 ; Call the error handling routine.
  DEFB $0A                ;

; THE 'SEMI-TONE' TABLE
;
; Used by the routine at BEEP.
;
; This table holds the frequencies of the twelve semi-tones in an octave.
SEMITONES:
  DEFB $89,$02,$D0,$12,$86 ; 261.63 Hz - C
  DEFB $89,$0A,$97,$60,$75 ; 277.18 Hz - C#
  DEFB $89,$12,$D5,$17,$1F ; 293.66 Hz - D
  DEFB $89,$1B,$90,$41,$02 ; 311.13 Hz - D#
  DEFB $89,$24,$D0,$53,$CA ; 329.63 Hz - E
  DEFB $89,$2E,$9D,$36,$B1 ; 349.23 Hz - F
  DEFB $89,$38,$FF,$49,$3E ; 369.99 Hz - F#
  DEFB $89,$43,$FF,$6A,$73 ; 392.00 Hz - G
  DEFB $89,$4F,$A7,$00,$54 ; 415.30 Hz - G#
  DEFB $89,$5C,$00,$00,$00 ; 440.00 Hz - A
  DEFB $89,$69,$14,$F6,$24 ; 466.16 Hz - A#
  DEFB $89,$76,$F1,$10,$05 ; 493.88 Hz - B

; THE 'PROGRAM NAME' SUBROUTINE (ZX81)
;
; The following subroutine applies to the ZX81 and was not removed when the program was rewritten for the Spectrum.
PROGNAME:
  CALL SCANNING
  LD A,(FLAGS)
  ADD A,A
  JP M,REPORT_C
  POP HL
  RET NC
  PUSH HL
  CALL STK_FETCH
  LD H,D
  LD L,E
  DEC C
  RET M
  ADD HL,BC
  SET 7,(HL)
  RET

; THE 'SA-BYTES' SUBROUTINE
;
; Used by the routine at SA_CONTRL.
;
; This subroutine is called to save the header information and later the actual program/data block to tape.
;
; A +00 (header block) or +FF (data block)
; DE Block length
; IX Start address
SA_BYTES:
  LD HL,SA_LD_RET         ; Pre-load the machine stack with the address SA_LD_RET.
  PUSH HL                 ;
  LD HL,$1F80             ; This constant will give a leader of about 5 seconds for a 'header'.
  BIT 7,A                 ; Jump forward if saving a header.
  JR Z,SA_FLAG            ;
  LD HL,$0C98             ; This constant will give a leader of about 2 seconds for a program/data block.
SA_FLAG:
  EX AF,AF'               ; The flag is saved.
  INC DE                  ; The 'length' is incremented and the 'base address' reduced to allow for the flag.
  DEC IX                  ;
  DI                      ; The maskable interrupt is disabled during the save.
  LD A,$02                ; Signal 'MIC on' and border to be red.
  LD B,A                  ; Give a value to B.
; A loop is now entered to create the pulses of the leader. Both the 'MIC on' and the 'MIC off' pulses are 2,168 T states in length. The colour of the border changes from red to cyan with each 'edge'.
;
; Note: an 'edge' will be a transition either from 'on' to 'off', or from 'off' to 'on'.
SA_LEADER:
  DJNZ SA_LEADER          ; The main timing period.
  OUT ($FE),A             ; MIC on/off, border red/cyan, on each pass.
  XOR $0F                 ;
  LD B,$A4                ; The main timing constant.
  DEC L                   ; Decrease the low counter.
  JR NZ,SA_LEADER         ; Jump back for another pulse.
  DEC B                   ; Allow for the longer path (reduce by 13 T states).
  DEC H                   ; Decrease the high counter.
  JP P,SA_LEADER          ; Jump back for another pulse until completion of the leader.
; A sync pulse is now sent.
  LD B,$2F
SA_SYNC_1:
  DJNZ SA_SYNC_1          ; MIC off for 667 T states from 'OUT to OUT'.
  OUT ($FE),A             ; MIC on and red.
  LD A,$0D                ; Signal 'MIC off and cyan'.
  LD B,$37                ; MIC on for 735 T States from 'OUT to OUT'.
SA_SYNC_2:
  DJNZ SA_SYNC_2          ;
  OUT ($FE),A             ; Now MIC off and border cyan.
; The header v. program/data flag will be the first byte to be saved.
  LD BC,$3B0E             ; +3B is a timing constant; +0E signals 'MIC off and yellow'.
  EX AF,AF'               ; Fetch the flag and pass it to the L register for 'sending'.
  LD L,A                  ;
  JP SA_START             ; Jump forward into the saving loop.
; The byte saving loop is now entered. The first byte to be saved is the flag; this is followed by the actual data bytes and the final byte sent is the parity byte that is built up by considering the values of all the earlier bytes.
SA_LOOP:
  LD A,D                  ; The 'length' counter is tested and the jump taken when it has reached zero.
  OR E                    ;
  JR Z,SA_PARITY          ;
  LD L,(IX+$00)           ; Fetch the next byte that is to be saved.
SA_LOOP_P:
  LD A,H                  ; Fetch the current 'parity'.
  XOR L                   ; Include the present byte.
SA_START:
  LD H,A                  ; Restore the 'parity'. Note that on entry here the 'flag' value initialises 'parity'.
  LD A,$01                ; Signal 'MIC on and blue'.
  SCF                     ; Set the carry flag. This will act as a 'marker' for the 8 bits of a byte.
  JP SA_8_BITS            ; Jump forward.
; When it is time to send the 'parity' byte then it is transferred to the L register for saving.
SA_PARITY:
  LD L,H                  ; Get final 'parity' value.
  JR SA_LOOP_P            ; Jump back.
; The following inner loop produces the actual pulses. The loop is entered at SA_BIT_1 with the type of the bit to be saved indicated by the carry flag. Two passes of the loop are made for each bit thereby making an 'off pulse' and an 'on pulse'. The pulses for a reset bit are shorter by 855 T states.
SA_BIT_2:
  LD A,C                  ; Come here on the second pass and fetch 'MIC off and yellow'.
  BIT 7,B                 ; Set the zero flag to show 'second pass'.
SA_BIT_1:
  DJNZ SA_BIT_1           ; The main timing loop; always 801 T states on a second pass.
  JR NC,SA_OUT            ; Jump, taking the shorter path, if saving a '0'.
  LD B,$42                ; However if saving a '1' then add 855 T states.
SA_SET:
  DJNZ SA_SET             ;
SA_OUT:
  OUT ($FE),A             ; On the first pass 'MIC on and blue' and on the second pass 'MIC off and yellow'.
  LD B,$3E                ; Set the timing constant for the second pass.
  JR NZ,SA_BIT_2          ; Jump back at the end of the first pass; otherwise reclaim 13 T states.
  DEC B                   ;
  XOR A                   ; Clear the carry flag and set A to hold +01 (MIC on and blue) before continuing into the '8 bit loop'.
  INC A                   ;
; The '8 bit loop' is entered initially with the whole byte in the L register and the carry flag set. However it is re-entered after each bit has been saved until the point is reached when the 'marker' passes to the carry flag leaving the L register empty.
SA_8_BITS:
  RL L                    ; Move bit 7 to the carry and the 'marker' leftwards.
  JP NZ,SA_BIT_1          ; Save the bit unless finished with the byte.
  DEC DE                  ; Decrease the 'counter'.
  INC IX                  ; Advance the 'base address'.
  LD B,$31                ; Set the timing constant for the first bit of the next byte.
  LD A,$7F                ; Return (to SA_LD_RET) if the BREAK key is being pressed.
  IN A,($FE)              ;
  RRA                     ;
  RET NC                  ;
  LD A,D                  ; Otherwise test the 'counter' and jump back even if it has reached zero (so as to send the 'parity' byte).
  INC A                   ;
  JP NZ,SA_LOOP           ;
  LD B,$3B                ; Exit when the 'counter' reaches +FFFF. But first give a short delay.
SA_DELAY:
  DJNZ SA_DELAY           ;
  RET                     ;
; Note: a reset bit will give a 'MIC off' pulse of 855 T states followed by a 'MIC on' pulse of 855 T states, whereas a set bit will give pulses of exactly twice as long. Note also that there are no gaps either between the sync pulse and the first bit of the flag, or between bytes.

; THE 'SA/LD-RET' SUBROUTINE
;
; Used by the routines at SA_BYTES and LD_BYTES.
;
; This subroutine is common to both saving and loading.
;
; The border is set to its original colour and the BREAK key tested for a last time.
;
; F Carry flag reset if there was a loading error
SA_LD_RET:
  PUSH AF                 ; Save the carry flag. (It is reset after a loading error.)
  LD A,(BORDCR)           ; Fetch the original border colour from its system variable (BORDCR).
  AND $38                 ;
  RRCA                    ; Move the border colour to bits 2, 1 and 0.
  RRCA                    ;
  RRCA                    ;
  OUT ($FE),A             ; Set the border to its original colour.
  LD A,$7F                ; Read the BREAK key for a last time.
  IN A,($FE)              ;
  RRA                     ;
  EI                      ; Enable the maskable interrupt.
  JR C,SA_LD_END          ; Jump unless a break is to be made.
; Report D - BREAK-CONT repeats.
  RST $08                 ; Call the error handling routine.
  DEFB $0C                ;
; Continue here.
SA_LD_END:
  POP AF                  ; Retrieve the carry flag.
  RET                     ; Return to the calling routine.

; THE 'LD-BYTES' SUBROUTINE
;
; Used by the routines at SAVE_ETC and LD_BLOCK.
;
; This subroutine is called to load the header information and later load or verify an actual block of data from a tape.
;
;   A +00 (header block) or +FF (data block)
;   F Carry flag set if loading, reset if verifying
;   DE Block length
;   IX Start address
; O:F Carry flag reset if there was an error
LD_BYTES:
  INC D                   ; This resets the zero flag. (D cannot hold +FF.)
  EX AF,AF'               ; The A register holds +00 for a header and +FF for a block of data. The carry flag is reset for verifying and set for loading.
  DEC D                   ; Restore D to its original value.
  DI                      ; The maskable interrupt is now disabled.
  LD A,$0F                ; The border is made white.
  OUT ($FE),A             ;
  LD HL,SA_LD_RET         ; Preload the machine stack with the address SA_LD_RET.
  PUSH HL                 ;
  IN A,($FE)              ; Make an initial read of port '254'.
  RRA                     ; Rotate the byte obtained but keep only the EAR bit.
  AND $20                 ;
  OR $02                  ; Signal red border.
  LD C,A                  ; Store the value in the C register (+22 for 'off' and +02 for 'on' - the present EAR state).
  CP A                    ; Set the zero flag.
; The first stage of reading a tape involves showing that a pulsing signal actually exists (i.e. 'on/off' or 'off/on' edges).
LD_BREAK:
  RET NZ                  ; Return if the BREAK key is being pressed.
LD_START:
  CALL LD_EDGE_1          ; Return with the carry flag reset if there is no 'edge' within approx. 14,000 T states. But if an 'edge' is found the border will go cyan.
  JR NC,LD_BREAK          ;
; The next stage involves waiting a while and then showing that the signal is still pulsing.
  LD HL,$0415             ; The length of this waiting period will be almost one second in duration.
LD_WAIT:
  DJNZ LD_WAIT            ;
  DEC HL                  ;
  LD A,H                  ;
  OR L                    ;
  JR NZ,LD_WAIT           ;
  CALL LD_EDGE_2          ; Continue only if two edges are found within the allowed time period.
  JR NC,LD_BREAK          ;
; Now accept only a 'leader signal'.
LD_LEADER:
  LD B,$9C                ; The timing constant.
  CALL LD_EDGE_2          ; Continue only if two edges are found within the allowed time period.
  JR NC,LD_BREAK          ;
  LD A,$C6                ; However the edges must have been found within about 3,000 T states of each other.
  CP B                    ;
  JR NC,LD_START          ;
  INC H                   ; Count the pair of edges in the H register until '256' pairs have been found.
  JR NZ,LD_LEADER         ;
; After the leader come the 'off' and 'on' parts of the sync pulse.
LD_SYNC:
  LD B,$C9                ; The timing constant.
  CALL LD_EDGE_1          ; Every edge is considered until two edges are found close together - these will be the start and finishing edges of the 'off' sync pulse.
  JR NC,LD_BREAK          ;
  LD A,B                  ;
  CP $D4                  ;
  JR NC,LD_SYNC           ;
  CALL LD_EDGE_1          ; The finishing edge of the 'on' pulse must exist. (Return carry flag reset.)
  RET NC                  ;
; The bytes of the header or the program/data block can now be loaded or verified. But the first byte is the type flag.
  LD A,C                  ; The border colours from now on will be blue and yellow.
  XOR $03                 ;
  LD C,A                  ;
  LD H,$00                ; Initialise the 'parity matching' byte to zero.
  LD B,$B0                ; Set the timing constant for the flag byte.
  JR LD_MARKER            ; Jump forward into the byte loading loop.
; The byte loading loop is used to fetch the bytes one at a time. The flag byte is first. This is followed by the data bytes and the last byte is the 'parity' byte.
LD_LOOP:
  EX AF,AF'               ; Fetch the flags.
  JR NZ,LD_FLAG           ; Jump forward only when handling the first byte.
  JR NC,LD_VERIFY         ; Jump forward if verifying a tape.
  LD (IX+$00),L           ; Make the actual load when required.
  JR LD_NEXT              ; Jump forward to load the next byte.
LD_FLAG:
  RL C                    ; Keep the carry flag in a safe place temporarily.
  XOR L                   ; Return now if the type flag does not match the first byte on the tape. (Carry flag reset.)
  RET NZ                  ;
  LD A,C                  ; Restore the carry flag now.
  RRA                     ;
  LD C,A                  ;
  INC DE                  ; Increase the counter to compensate for its 'decrease' after the jump.
  JR LD_DEC               ;
; If a data block is being verified then the freshly loaded byte is tested against the original byte.
LD_VERIFY:
  LD A,(IX+$00)           ; Fetch the original byte.
  XOR L                   ; Match it against the new byte.
  RET NZ                  ; Return if 'no match'. (Carry flag reset.)
; A new byte can now be collected from the tape.
LD_NEXT:
  INC IX                  ; Increase the 'destination'.
LD_DEC:
  DEC DE                  ; Decrease the 'counter'.
  EX AF,AF'               ; Save the flags.
  LD B,$B2                ; Set the timing constant.
LD_MARKER:
  LD L,$01                ; Clear the 'object' register apart from a 'marker' bit.
; The following loop is used to build up a byte in the L register.
LD_8_BITS:
  CALL LD_EDGE_2          ; Find the length of the 'off' and 'on' pulses of the next bit.
  RET NC                  ; Return if the time period is exceeded. (Carry flag reset.)
  LD A,$CB                ; Compare the length against approx. 2,400 T states, resetting the carry flag for a '0' and setting it for a '1'.
  CP B                    ;
  RL L                    ; Include the new bit in the L register.
  LD B,$B0                ; Set the timing constant for the next bit.
  JP NC,LD_8_BITS         ; Jump back whilst there are still bits to be fetched.
; The 'parity matching' byte has to be updated with each new byte.
  LD A,H                  ; Fetch the 'parity matching' byte and include the new byte.
  XOR L                   ;
  LD H,A                  ; Save it once again.
; Passes round the loop are made until the 'counter' reaches zero. At that point the 'parity matching' byte should be holding zero.
  LD A,D                  ; Make a further pass if the DE register pair does not hold zero.
  OR E                    ;
  JR NZ,LD_LOOP           ;
  LD A,H                  ; Fetch the 'parity matching' byte.
  CP $01                  ; Return with the carry flag set if the value is zero. (Carry flag reset if in error.)
  RET                     ;

; THE 'LD-EDGE-2' AND 'LD-EDGE-1' SUBROUTINES
;
; Used by the routine at LD_BYTES.
;
; These two subroutines form the most important part of the LOAD/VERIFY operation.
;
; The subroutines are entered with a timing constant in the B register, and the previous border colour and 'edge-type' in the C register.
;
; The subroutines return with the carry flag set if the required number of 'edges' have been found in the time allowed, and the change to the value in the B register shows just how long it took to find the 'edge(s)'.
;
; The carry flag will be reset if there is an error. The zero flag then signals 'BREAK pressed' by being reset, or 'time-up' by being set.
;
; The entry point LD_EDGE_2 is used when the length of a complete pulse is required and LD_EDGE_1 is used to find the time before the next 'edge'.
;
; B Timing constant
; C Border colour (bits 0-2) and previous edge-type (bit 5)
LD_EDGE_2:
  CALL LD_EDGE_1          ; In effect call LD_EDGE_1 twice, returning in between if there is an error.
  RET NC                  ;
; This entry point is used by the routine at LD_BYTES.
LD_EDGE_1:
  LD A,$16                ; Wait 358 T states before entering the sampling loop.
LD_DELAY:
  DEC A                   ;
  JR NZ,LD_DELAY          ;
  AND A                   ;
; The sampling loop is now entered. The value in the B register is incremented for each pass; 'time-up' is given when B reaches zero.
LD_SAMPLE:
  INC B                   ; Count each pass.
  RET Z                   ; Return carry reset and zero set if 'time-up'.
  LD A,$7F                ; Read from port +7FFE, i.e. BREAK and EAR.
  IN A,($FE)              ;
  RRA                     ; Shift the byte.
  RET NC                  ; Return carry reset and zero reset if BREAK was pressed.
  XOR C                   ; Now test the byte against the 'last edge-type'; jump back unless it has changed.
  AND $20                 ;
  JR Z,LD_SAMPLE          ;
; A new 'edge' has been found within the time period allowed for the search. So change the border colour and set the carry flag.
  LD A,C                  ; Change the 'last edge-type' and border colour.
  CPL                     ;
  LD C,A                  ;
  AND $07                 ; Keep only the border colour.
  OR $08                  ; Signal 'MIC off'.
  OUT ($FE),A             ; Change the border colour (red/cyan or blue/yellow).
  SCF                     ; Signal the successful search before returning.
  RET                     ;
; Note: the LD_EDGE_1 subroutine takes 465 T states, plus an additional 58 T states for each unsuccessful pass around the sampling loop.
;
; For example, therefore, when awaiting the sync pulse (see LD_SYNC) allowance is made for ten additional passes through the sampling loop. The search is thereby for the next edge to be found within, roughly, 1100 T states (465+10*58+overhead). This will prove successful for the sync 'off' pulse that comes after the long 'leader pulses'.

; THE 'SAVE, LOAD, VERIFY and MERGE' COMMAND ROUTINES
;
; Used by the routine at CLASS_0B.
;
; This entry point is used for all four commands. The value held in T-ADDR, however, distinguishes between the four commands. The first part of the following routine is concerned with the construction of the 'header information' in the work space.
SAVE_ETC:
  POP AF                  ; Drop the address - SCAN_LOOP.
  LD A,(T_ADDR)           ; Reduce T-ADDR-lo by +E0, giving +00 for SAVE, +01 for LOAD, +02 for VERIFY and +03 for MERGE.
  SUB $E0                 ;
  LD (T_ADDR),A           ;
  CALL CLASS_0A           ; Pass the parameters of the 'name' to the calculator stack.
  CALL SYNTAX_Z           ; Jump forward if checking syntax.
  JR Z,SA_DATA            ;
  LD BC,$0011             ; Allow seventeen locations for the header of a SAVE (T-ADDR-lo=+00) but thirty four for the other commands.
  LD A,(T_ADDR)           ;
  AND A                   ;
  JR Z,SA_SPACE           ;
  LD C,$22                ;
SA_SPACE:
  RST $30                 ; The required amount of space is made in the work space.
  PUSH DE                 ; Copy the start address to the IX register pair.
  POP IX                  ;
  LD B,$0B                ; A program name can have up to ten characters but first enter eleven space characters into the prepared area.
  LD A," "                ;
SA_BLANK:
  LD (DE),A               ;
  INC DE                  ;
  DJNZ SA_BLANK           ;
  LD (IX+$01),$FF         ; A null name is +FF only.
  CALL STK_FETCH          ; The parameters of the name are fetched and its length is tested.
  LD HL,$FFF6             ; This is '-10'.
  DEC BC                  ; In effect jump forward if the length of the name is not too long (i.e. no more than ten characters).
  ADD HL,BC               ;
  INC BC                  ;
  JR NC,SA_NAME           ;
  LD A,(T_ADDR)           ; But allow for the LOADing, VERIFYing and MERGEing of programs (T-ADDR-lo>+00) with 'null' names or extra long names.
  AND A                   ;
  JR NZ,SA_NULL           ;
; Report F - Invalid file name.
  RST $08                 ; Call the error handling routine.
  DEFB $0E                ;
; Continue to handle the name of the program.
SA_NULL:
  LD A,B                  ; Jump forward if the name has a 'null' length.
  OR C                    ;
  JR Z,SA_DATA            ;
  LD BC,$000A             ; But truncate longer names.
; The name is now transferred to the work space (second location onwards).
SA_NAME:
  PUSH IX                 ; Copy the start address to the HL register pair.
  POP HL                  ;
  INC HL                  ; Step to the second location.
  EX DE,HL                ; Switch the pointers over and copy the name.
  LDIR                    ;
; The many different parameters, if any, that follow the command are now considered. Start by handling 'xxx "name" DATA'.
SA_DATA:
  RST $18                 ; Is the present code the token 'DATA'?
  CP $E4                  ;
  JR NZ,SA_SCR            ; Jump if not.
  LD A,(T_ADDR)           ; However it is not possible to have 'MERGE name DATA' (T-ADDR-lo=+03).
  CP $03                  ;
  JP Z,REPORT_C           ;
  RST $20                 ; Advance CH-ADD.
  CALL LOOK_VARS          ; Look in the variables area for the array.
  SET 7,C                 ; Set bit 7 of the array's name.
  JR NC,SA_V_OLD          ; Jump if handling an existing array.
  LD HL,$0000             ; Signal 'using a new array'.
  LD A,(T_ADDR)           ; Consider the value in T-ADDR-lo and give an error if trying to SAVE or VERIFY a new array.
  DEC A                   ;
  JR Z,SA_V_NEW           ;
; Report 2 - Variable not found.
  RST $08                 ; Call the error handling routine.
  DEFB $01                ;
; Continue with the handling of an existing array.
SA_V_OLD:
  JP NZ,REPORT_C          ; Note: this fails to exclude simple strings.
  CALL SYNTAX_Z           ; Jump forward if checking syntax.
  JR Z,SA_DATA_1          ;
  INC HL                  ; Point to the 'low length' of the variable.
  LD A,(HL)               ; The low length byte goes into the work space, followed by the high length byte.
  LD (IX+$0B),A           ;
  INC HL                  ;
  LD A,(HL)               ;
  LD (IX+$0C),A           ;
  INC HL                  ; Step past the length bytes.
; The next part is common to both 'old' and 'new' arrays. Note: syntax path error.
SA_V_NEW:
  LD (IX+$0E),C           ; Copy the array's name.
  LD A,$01                ; Assume an array of numbers.
  BIT 6,C                 ; Jump if it is so.
  JR Z,SA_V_TYPE          ;
  INC A                   ; It is an array of characters.
SA_V_TYPE:
  LD (IX+$00),A           ; Save the 'type' in the first location of the header area.
; The last part of the statement is examined before joining the other pathways.
SA_DATA_1:
  EX DE,HL                ; Save the pointer in DE.
  RST $20                 ; Is the next character a ')'?
  CP ")"                  ;
  JR NZ,SA_V_OLD          ; Give report C if it is not.
  RST $20                 ; Advance CH-ADD.
  CALL CHECK_END          ; Move on to the next statement if checking syntax.
  EX DE,HL                ; Return the pointer to the HL register pair before jumping forward. (The pointer indicates the start of an existing array's contents.)
  JP SA_ALL               ;
; Now consider 'SCREEN$'.
SA_SCR:
  CP $AA                  ; Is the present code the token SCREEN$?
  JR NZ,SA_CODE           ; Jump if not.
  LD A,(T_ADDR)           ; However it is not possible to have 'MERGE name SCREEN$' (T-ADDR-lo=+03).
  CP $03                  ;
  JP Z,REPORT_C           ;
  RST $20                 ; Advance CH-ADD.
  CALL CHECK_END          ; Move on to the next statement if checking syntax.
  LD (IX+$0B),$00         ; The display area and the attribute area occupy +1B00 locations and these locations start at +4000; these details are passed to the header area in the work space.
  LD (IX+$0C),$1B         ;
  LD HL,$4000             ;
  LD (IX+$0D),L           ;
  LD (IX+$0E),H           ;
  JR SA_TYPE_3            ; Jump forward.
; Now consider 'CODE'.
SA_CODE:
  CP $AF                  ; Is the present code the token 'CODE'?
  JR NZ,SA_LINE           ; Jump if not.
  LD A,(T_ADDR)           ; However it is not possible to have 'MERGE name CODE' (T-ADDR-lo=+03).
  CP $03                  ;
  JP Z,REPORT_C           ;
  RST $20                 ; Advance CH-ADD.
  CALL PR_ST_END          ; Jump forward if the statement has not finished.
  JR NZ,SA_CODE_1         ;
  LD A,(T_ADDR)           ; However it is not possible to have 'SAVE name CODE' (T-ADDR-lo=+00) by itself.
  AND A                   ;
  JP Z,REPORT_C           ;
  CALL USE_ZERO           ; Put a zero on the calculator stack - for the 'start'.
  JR SA_CODE_2            ; Jump forward.
; Look for a 'starting address'.
SA_CODE_1:
  CALL CLASS_06           ; Fetch the first number.
  RST $18                 ; Is the present character a comma?
  CP ","                  ;
  JR Z,SA_CODE_3          ; Jump if it is - the number was a 'starting address'.
  LD A,(T_ADDR)           ; However refuse 'SAVE name CODE' that does not have a 'start' and a 'length' (T-ADDR-lo=+00).
  AND A                   ;
  JP Z,REPORT_C           ;
SA_CODE_2:
  CALL USE_ZERO           ; Put a zero on the calculator stack - for the 'length'.
  JR SA_CODE_4            ; Jump forward.
; Fetch the 'length' as it was specified.
SA_CODE_3:
  RST $20                 ; Advance CH-ADD.
  CALL CLASS_06           ; Fetch the 'length'.
; The parameters are now stored in the header area of the work space.
SA_CODE_4:
  CALL CHECK_END          ; But move on to the next statement now if checking syntax.
  CALL FIND_INT2          ; Compress the 'length' into the BC register pair and store it.
  LD (IX+$0B),C           ;
  LD (IX+$0C),B           ;
  CALL FIND_INT2          ; Compress the 'starting address' into the BC register pair and store it.
  LD (IX+$0D),C           ;
  LD (IX+$0E),B           ;
  LD H,B                  ; Transfer the 'pointer' to the HL register pair as usual.
  LD L,C                  ;
; 'SCREEN$' and 'CODE' are both of type 3.
SA_TYPE_3:
  LD (IX+$00),$03         ; Enter the 'type' number.
  JR SA_ALL               ; Rejoin the other pathways.
; Now consider 'LINE' and 'no further parameters'.
SA_LINE:
  CP $CA                  ; Is the present code the token 'LINE'?
  JR Z,SA_LINE_1          ; Jump if it is.
  CALL CHECK_END          ; Move on to the next statement if checking syntax.
  LD (IX+$0E),$80         ; When there are no further parameters, +80 is entered.
  JR SA_TYPE_0            ; Jump forward.
; Fetch the 'line number' that must follow 'LINE'.
SA_LINE_1:
  LD A,(T_ADDR)           ; However only allow 'SAVE name LINE number' (T-ADDR-lo=+00).
  AND A                   ;
  JP NZ,REPORT_C          ;
  RST $20                 ; Advance CH-ADD.
  CALL CLASS_06           ; Pass the number to the calculator stack.
  CALL CHECK_END          ; Move on to the next statement if checking syntax.
  CALL FIND_INT2          ; Compress the 'line number' into the BC register pair and store it.
  LD (IX+$0D),C           ;
  LD (IX+$0E),B           ;
; 'LINE' and 'no further parameters' are both of type 0.
SA_TYPE_0:
  LD (IX+$00),$00         ; Enter the 'type' number.
; The parameters that describe the program, and its variables, are found and stored in the header area of the work space.
  LD HL,(E_LINE)          ; The pointer to the end of the variables area (E-LINE).
  LD DE,(PROG)            ; The pointer to the start of the BASIC program (PROG).
  SCF                     ; Now perform the subtraction to find the length of the 'program + variables'; store the result.
  SBC HL,DE               ;
  LD (IX+$0B),L           ;
  LD (IX+$0C),H           ;
  LD HL,(VARS)            ; Repeat the operation but this time storing the length of the 'program' only (VARS-PROG).
  SBC HL,DE               ;
  LD (IX+$0F),L           ;
  LD (IX+$10),H           ;
  EX DE,HL                ; Transfer the 'pointer' to the HL register pair as usual.
; In all cases the header information has now been prepared.
;
; * The location 'IX+$00' holds the type number.
; * Locations 'IX+$01 to IX+$0A' hold the name (+FF in 'IX+$01' if null).
; * Locations 'IX+$0B and IX+$0C' hold the number of bytes that are to be found in the 'data block'.
; * Locations 'IX+$0D to IX+$10' hold a variety of parameters whose exact interpretation depends on the 'type'.
;
; The routine continues with the first task being to separate SAVE from LOAD, VERIFY and MERGE.
SA_ALL:
  LD A,(T_ADDR)           ; Jump forward when handling a SAVE command (T-ADDR-lo=+00).
  AND A                   ;
  JP Z,SA_CONTRL          ;
; In the case of a LOAD, VERIFY or MERGE command the first seventeen bytes of the 'header area' in the work space hold the prepared information, as detailed above; and it is now time to fetch a 'header' from the tape.
  PUSH HL                 ; Save the 'destination' pointer.
  LD BC,$0011             ; Form in the IX register pair the base address of the 'second header area'.
  ADD IX,BC               ;
; Now enter a loop, leaving it only when a 'header' has been LOADed.
LD_LOOK_H:
  PUSH IX                 ; Make a copy of the base address.
  LD DE,$0011             ; LOAD seventeen bytes.
  XOR A                   ; Signal 'header'.
  SCF                     ; Signal 'LOAD'.
  CALL LD_BYTES           ; Now look for a header.
  POP IX                  ; Retrieve the base address.
  JR NC,LD_LOOK_H         ; Go round the loop until successful.
; The new 'header' is now displayed on the screen but the routine will only proceed if the 'new' header matches the 'old' header.
  LD A,$FE                ; Ensure that channel 'S' is open.
  CALL CHAN_OPEN          ;
  LD (IY+$52),$03         ; Set the scroll counter (SCR-CT).
  LD C,$80                ; Signal 'names do not match'.
  LD A,(IX+$00)           ; Compare the 'new' type against the 'old' type.
  CP (IX-$11)             ;
  JR NZ,LD_TYPE           ; Jump if the 'types' do not match.
  LD C,$F6                ; But if they do, signal 'ten characters are to match'.
LD_TYPE:
  CP $04                  ; Clearly the 'header' is nonsense if 'type 4 or more'.
  JR NC,LD_LOOK_H         ;
; The appropriate message - 'Program: ', 'Number array: ', 'Character array: ' or 'Bytes: ' is printed.
  LD DE,$09C0             ; The base address of the message block.
  PUSH BC                 ; Save the C register whilst the appropriate message is printed.
  CALL PO_MSG             ;
  POP BC                  ;
; The 'new name' is printed and as this is done the 'old' and the 'new' names are compared.
  PUSH IX                 ; Make the DE register pair point to the 'new name' and the HL register pair to the 'old name'.
  POP DE                  ;
  LD HL,$FFF0             ;
  ADD HL,DE               ;
  LD B,$0A                ; Ten characters are to be considered.
  LD A,(HL)               ; Jump forward if the match is to be against an actual name.
  INC A                   ;
  JR NZ,LD_NAME           ;
  LD A,C                  ; But if the 'old name' is 'null' then signal 'ten characters already match'.
  ADD A,B                 ;
  LD C,A                  ;
; A loop is entered to print the characters of the 'new name'. The name will be accepted if the 'counter' reaches zero, at least.
LD_NAME:
  INC DE                  ; Consider each character of the 'new name' in turn.
  LD A,(DE)               ;
  CP (HL)                 ; Match it against the appropriate character of the 'old name'.
  INC HL                  ;
  JR NZ,LD_CH_PR          ; Do not count it if it does not does not match.
  INC C                   ;
LD_CH_PR:
  RST $10                 ; Print the 'new' character.
  DJNZ LD_NAME            ; Loop for ten characters.
  BIT 7,C                 ; Accept the name only if the counter has reached zero.
  JR NZ,LD_LOOK_H         ;
  LD A,$0D                ; Follow the 'new name' with a 'carriage return'.
  RST $10                 ;
; The correct header has been found and the time has come to consider the three commands LOAD, VERIFY, and MERGE separately.
  POP HL                  ; Fetch the pointer.
  LD A,(IX+$00)           ; 'SCREEN$' and 'CODE' are handled with VERIFY.
  CP $03                  ;
  JR Z,VR_CONTRL          ;
  LD A,(T_ADDR)           ; Jump forward if using a LOAD command (T-ADDR-lo=+01).
  DEC A                   ;
  JP Z,LD_CONTRL          ;
  CP $02                  ; Jump forward if using a MERGE command; continue into VR_CONTRL with a VERIFY command.
  JP Z,ME_CONTRL          ;

; THE 'VERIFY' CONTROL ROUTINE
;
; Used by the routine at SAVE_ETC.
;
; The verification process involves the loading of a block of data, a byte at a time, but the bytes are not stored - only checked. This routine is also used to load blocks of data that have been described with 'SCREEN$' or 'CODE'.
;
; HL Block start address
VR_CONTRL:
  PUSH HL                 ; Save the 'pointer'.
  LD L,(IX-$06)           ; Fetch the 'number of bytes' as described in the 'old' header.
  LD H,(IX-$05)           ;
  LD E,(IX+$0B)           ; Fetch also the number from the 'new' header.
  LD D,(IX+$0C)           ;
  LD A,H                  ; Jump forward if the 'length' is unspecified, e.g. 'LOAD name CODE' only.
  OR L                    ;
  JR Z,VR_CONT_1          ;
  SBC HL,DE               ; Give report R if attempting to load a larger block than has been requested.
  JR C,REPORT_R           ;
  JR Z,VR_CONT_1          ; Accept equal 'lengths'.
  LD A,(IX+$00)           ; Also give report R if trying to verify blocks that are of unequal size. ('Old length' greater than 'new length'.)
  CP $03                  ;
  JR NZ,REPORT_R          ;
; The routine continues by considering the 'destination pointer'.
VR_CONT_1:
  POP HL                  ; Fetch the 'pointer', i.e. the 'start'.
  LD A,H                  ; This 'pointer' will be used unless it is zero, in which case the 'start' found in the 'new' header will be used instead.
  OR L                    ;
  JR NZ,VR_CONT_2         ;
  LD L,(IX+$0D)           ;
  LD H,(IX+$0E)           ;
; The verify/load flag is now considered and the actual load made.
VR_CONT_2:
  PUSH HL                 ; Move the 'pointer' to the IX register pair.
  POP IX                  ;
  LD A,(T_ADDR)           ; Jump forward unless using the VERIFY command (T-ADDR-lo=+02), with the carry flag signalling 'LOAD'.
  CP $02                  ;
  SCF                     ;
  JR NZ,VR_CONT_3         ;
  AND A                   ; Signal 'VERIFY'.
VR_CONT_3:
  LD A,$FF                ; Signal 'accept data block only' before loading the block.
; This routine continues into LD_BLOCK.

; THE 'LOAD A DATA BLOCK' SUBROUTINE
;
; Used by the routines at LD_CONTRL and ME_CONTRL.
;
; The routine at VR_CONTRL continues here.
;
; This subroutine is common to all the tape loading routines. In the case of LOAD and VERIFY it acts as a full return from the cassette handling routines but in the case of MERGE the data block has yet to be merged.
;
; A +FF
; F Carry flag set if loading, reset if verifying
; DE Block length
; IX Start address
LD_BLOCK:
  CALL LD_BYTES           ; Load/verify a data block.
  RET C                   ; Return unless an error.
; This entry point is used by the routine at VR_CONTRL.
;
; Report R - Tape loading error.
REPORT_R:
  RST $08                 ; Call the error handling routine.
  DEFB $1A                ;

; THE 'LOAD' CONTROL ROUTINE
;
; Used by the routine at SAVE_ETC.
;
; This routine controls the LOADing of a BASIC program, and its variables, or an array.
;
; HL Destination address (PROG, or the address of the array, or +0000 for a new array)
; IX Address of the header loaded from tape
LD_CONTRL:
  LD E,(IX+$0B)           ; Fetch the 'number of bytes' as given in the 'new header'.
  LD D,(IX+$0C)           ;
  PUSH HL                 ; Save the 'destination pointer'.
  LD A,H                  ; Jump forward unless trying to LOAD a previously undeclared array.
  OR L                    ;
  JR NZ,LD_CONT_1         ;
  INC DE                  ; Add three bytes to the length - for the name, the low length and the high length of a new variable.
  INC DE                  ;
  INC DE                  ;
  EX DE,HL                ;
  JR LD_CONT_2            ; Jump forward.
; Consider now if there is enough room in memory for the new data block.
LD_CONT_1:
  LD L,(IX-$06)           ; Fetch the size of the existing 'program+variables or array'.
  LD H,(IX-$05)           ;
  EX DE,HL                ;
  SCF                     ; Jump forward if no extra room will be required (taking into account the reclaiming of the presently used memory).
  SBC HL,DE               ;
  JR C,LD_DATA            ;
; Make the actual test for room.
LD_CONT_2:
  LD DE,$0005             ; Allow an overhead of five bytes.
  ADD HL,DE               ;
  LD B,H                  ; Move the result to the BC register pair and make the test.
  LD C,L                  ;
  CALL TEST_ROOM          ;
; Now deal with the LOADing of arrays.
LD_DATA:
  POP HL                  ; Fetch the 'pointer' anew.
  LD A,(IX+$00)           ; Jump forward if LOADing a BASIC program.
  AND A                   ;
  JR Z,LD_PROG            ;
  LD A,H                  ; Jump forward if LOADing a new array.
  OR L                    ;
  JR Z,LD_DATA_1          ;
  DEC HL                  ; Fetch the 'length' of the existing array by collecting the length bytes from the variables area.
  LD B,(HL)               ;
  DEC HL                  ;
  LD C,(HL)               ;
  DEC HL                  ; Point to its old name.
  INC BC                  ; Add three bytes to the length - one for the name and two for the 'length'.
  INC BC                  ;
  INC BC                  ;
  LD (X_PTR),IX           ; Save the IX register pair temporarily (in X-PTR) whilst the old array is reclaimed.
  CALL RECLAIM_2          ;
  LD IX,(X_PTR)           ;
; Space is now made available for the new array - at the end of the present variables area.
LD_DATA_1:
  LD HL,(E_LINE)          ; Find the pointer to the end-marker of the variables area - the '+80-byte' (E-LINE).
  DEC HL                  ;
  LD C,(IX+$0B)           ; Fetch the 'length' of the new array.
  LD B,(IX+$0C)           ;
  PUSH BC                 ; Save this 'length'.
  INC BC                  ; Add three bytes - one for the name and two for the 'length'.
  INC BC                  ;
  INC BC                  ;
  LD A,(IX-$03)           ; 'IX+0E' of the old header gives the name of the array.
  PUSH AF                 ; The name is saved whilst the appropriate amount of room is made available. In effect BC spaces before the 'new +80-byte'.
  CALL MAKE_ROOM          ;
  INC HL                  ;
  POP AF                  ;
  LD (HL),A               ; The name is entered.
  POP DE                  ; The 'length' is fetched and its two bytes are also entered.
  INC HL                  ;
  LD (HL),E               ;
  INC HL                  ;
  LD (HL),D               ;
  INC HL                  ; HL now points to the first location that is to be filled with data from the tape.
  PUSH HL                 ; This address is moved to the IX register pair; the carry flag set; 'data block' is signalled; and the block LOADed.
  POP IX                  ;
  SCF                     ;
  LD A,$FF                ;
  JP LD_BLOCK             ;
; Now deal with the LOADing of a BASIC program and its variables.
LD_PROG:
  EX DE,HL                ; Save the 'destination pointer'.
  LD HL,(E_LINE)          ; Find the address of the end marker of the current variables area - the '+80-byte' (E-LINE).
  DEC HL                  ;
  LD (X_PTR),IX           ; Save IX temporarily (in X-PTR).
  LD C,(IX+$0B)           ; Fetch the 'length' of the new data block.
  LD B,(IX+$0C)           ;
  PUSH BC                 ; Keep a copy of the 'length' whilst the present program and variables areas are reclaimed.
  CALL RECLAIM_1          ;
  POP BC                  ;
  PUSH HL                 ; Save the pointer to the program area and the length of the new data block.
  PUSH BC                 ;
  CALL MAKE_ROOM          ; Make sufficient room available for the new program and its variables.
  LD IX,(X_PTR)           ; Restore the IX register pair from X-PTR.
  INC HL                  ; The system variable VARS has also to be set for the new program.
  LD C,(IX+$0F)           ;
  LD B,(IX+$10)           ;
  ADD HL,BC               ;
  LD (VARS),HL            ;
  LD H,(IX+$0E)           ; If a line number was specified then it too has to be considered.
  LD A,H                  ;
  AND $C0                 ;
  JR NZ,LD_PROG_1         ; Jump if 'no number'; otherwise set NEWPPC and NSPPC.
  LD L,(IX+$0D)           ;
  LD (NEWPPC),HL          ;
  LD (IY+$0A),$00         ;
; The data block can now be LOADed.
LD_PROG_1:
  POP DE                  ; Fetch the 'length'.
  POP IX                  ; Fetch the 'start'.
  SCF                     ; Signal 'LOAD'.
  LD A,$FF                ; Signal 'data block' only.
  JP LD_BLOCK             ; Now LOAD it.

; THE 'MERGE' CONTROL ROUTINE
;
; Used by the routine at SAVE_ETC.
;
; There are three main parts to this routine.
;
; * Load the data block into the work space.
; * Merge the lines of the new program into the old program.
; * Merge the new variables into the old variables.
;
; Start therefore with the loading of the data block.
;
; IX Address of the header loaded from tape
ME_CONTRL:
  LD C,(IX+$0B)           ; Fetch the 'length' of the data block.
  LD B,(IX+$0C)           ;
  PUSH BC                 ; Save a copy of the 'length'.
  INC BC                  ; Now make 'length+1' locations available in the work space.
  RST $30                 ;
  LD (HL),$80             ; Place an end marker in the extra location.
  EX DE,HL                ; Move the 'start' pointer to the HL register pair.
  POP DE                  ; Fetch the original 'length'.
  PUSH HL                 ; Save a copy of the 'start'.
  PUSH HL                 ; Now set the IX register pair for the actual load.
  POP IX                  ;
  SCF                     ; Signal 'LOAD'.
  LD A,$FF                ; Signal 'data block only'.
  CALL LD_BLOCK           ; Load the data block.
; The lines of the new program are merged with the lines of the old program.
  POP HL                  ; Fetch the 'start' of the new program.
  LD DE,(PROG)            ; Initialise DE to the 'start' of the old program (PROG).
; Enter a loop to deal with the lines of the new program.
ME_NEW_LP:
  LD A,(HL)               ; Fetch a line number and test it.
  AND $C0                 ;
  JR NZ,ME_VAR_LP         ; Jump when finished with all the lines.
; Now enter an inner loop to deal with the lines of the old program.
ME_OLD_LP:
  LD A,(DE)               ; Fetch the high line number byte and compare it. Jump forward if it does not match but in any case advance both pointers.
  INC DE                  ;
  CP (HL)                 ;
  INC HL                  ;
  JR NZ,ME_OLD_L1         ;
  LD A,(DE)               ; Repeat the comparison for the low line number bytes.
  CP (HL)                 ;
ME_OLD_L1:
  DEC DE                  ; Now retreat the pointers.
  DEC HL                  ;
  JR NC,ME_NEW_L2         ; Jump forward if the correct place has been found for a line of the new program.
  PUSH HL                 ; Otherwise find the address of the start of the next old line.
  EX DE,HL                ;
  CALL NEXT_ONE           ;
  POP HL                  ;
  JR ME_OLD_LP            ; Go round the loop for each of the 'old lines'.
ME_NEW_L2:
  CALL ME_ENTER           ; Enter the 'new line' and go round the outer loop again.
  JR ME_NEW_LP            ;
; In a similar manner the variables of the new program are merged with the variables of the old program.
ME_VAR_LP:
  LD A,(HL)               ; Fetch each variable name in turn and test it.
  LD C,A                  ;
  CP $80                  ; Return when all the variables have been considered.
  RET Z                   ;
  PUSH HL                 ; Save the current new pointer.
  LD HL,(VARS)            ; Fetch VARS (for the old program).
; Now enter an inner loop to search the existing variables area.
ME_OLD_VP:
  LD A,(HL)               ; Fetch each variable name and test it.
  CP $80                  ;
  JR Z,ME_VAR_L2          ; Jump forward once the end marker is found. (Make an 'addition'.)
  CP C                    ; Compare the names (first bytes).
  JR Z,ME_OLD_V2          ; Jump forward to consider it further, returning here if it proves not to match fully.
ME_OLD_V1:
  PUSH BC                 ; Save the new variable's name whilst the next 'old variable' is located.
  CALL NEXT_ONE           ;
  POP BC                  ;
  EX DE,HL                ; Restore the pointer to the DE register pair and go round the loop again.
  JR ME_OLD_VP            ;
; The old and new variables match with respect to their first bytes but variables with long names will need to be matched fully.
ME_OLD_V2:
  AND $E0                 ; Consider bits 7, 6 and 5 only.
  CP $A0                  ; Accept all the variable types except 'long named variables'.
  JR NZ,ME_VAR_L1         ;
  POP DE                  ; Make DE point to the first character of the 'new name'.
  PUSH DE                 ;
  PUSH HL                 ; Save the pointer to the 'old name'.
; Enter a loop to compare the letters of the long names.
ME_OLD_V3:
  INC HL                  ; Update both the 'old' and the 'new' pointers.
  INC DE                  ;
  LD A,(DE)               ; Compare the two letters.
  CP (HL)                 ;
  JR NZ,ME_OLD_V4         ; Jump forward if the match fails.
  RLA                     ; Go round the loop until the 'last character' is found.
  JR NC,ME_OLD_V3         ;
  POP HL                  ; Fetch the pointer to the start of the 'old' name and jump forward - successful.
  JR ME_VAR_L1            ;
ME_OLD_V4:
  POP HL                  ; Fetch the pointer and jump back - unsuccessful.
  JR ME_OLD_V1            ;
; Come here if the match was found.
ME_VAR_L1:
  LD A,$FF                ; Signal 'replace' variable.
; And here if not. (A holds +80 - variable to be 'added'.)
ME_VAR_L2:
  POP DE                  ; Fetch pointer to 'new' name.
  EX DE,HL                ; Switch over the registers.
  INC A                   ; The zero flag is to be set if there is to be a 'replacement', reset for an 'addition'.
  SCF                     ; Signal 'handling variables'.
  CALL ME_ENTER           ; Now make the entry.
  JR ME_VAR_LP            ; Go round the loop to consider the next new variable.

; THE 'MERGE A LINE OR A VARIABLE' SUBROUTINE
;
; Used by the routine at ME_CONTRL.
;
;   DE Destination address of the new line/variable
;   HL Address of the new line/variable to MERGE
;   F Carry flag: MERGE a BASIC line (reset) or a variable (set)
;   F Zero flag: add (reset) or replace (set) the line/variable
; O:DE Address of the next line/variable in the existing program
; O:HL Address of the next new line/variable to MERGE
ME_ENTER:
  JR NZ,ME_ENT_1          ; Jump if handling an 'addition'.
  EX AF,AF'               ; Save the flags.
  LD (X_PTR),HL           ; Save the 'new' pointer (in X-PTR) whilst the 'old' line or variable is reclaimed.
  EX DE,HL                ;
  CALL NEXT_ONE           ;
  CALL RECLAIM_2          ;
  EX DE,HL                ;
  LD HL,(X_PTR)           ;
  EX AF,AF'               ; Restore the flags.
; The new entry can now be made.
ME_ENT_1:
  EX AF,AF'               ; Save the flags.
  PUSH DE                 ; Make a copy of the 'destination' pointer.
  CALL NEXT_ONE           ; Find the length of the 'new' variable/line.
  LD (X_PTR),HL           ; Save the pointer to the 'new' variable/line (in X-PTR).
  LD HL,(PROG)            ; Fetch PROG - to avoid corruption.
  EX (SP),HL              ; Save PROG on the stack and fetch the 'new' pointer.
  PUSH BC                 ; Save the length.
  EX AF,AF'               ; Retrieve the flags.
  JR C,ME_ENT_2           ; Jump forward if adding a new variable.
  DEC HL                  ; A new line is added before the 'destination' location.
  CALL MAKE_ROOM          ; Make the room for the new line.
  INC HL
  JR ME_ENT_3             ; Jump forward.
ME_ENT_2:
  CALL MAKE_ROOM          ; Make the room for the new variable.
ME_ENT_3:
  INC HL                  ; Point to the first new location.
  POP BC                  ; Retrieve the length.
  POP DE                  ; Retrieve PROG and store it in its correct place.
  LD (PROG),DE            ;
  LD DE,(X_PTR)           ; Also fetch the 'new' pointer (from X-PTR).
  PUSH BC                 ; Again save the length and the 'new' pointer.
  PUSH DE                 ;
  EX DE,HL                ; Switch the pointers and copy the 'new' variable/line into the room made for it.
  LDIR                    ;
; The 'new' variable/line has now to be removed from the work space.
  POP HL                  ; Fetch the 'new' pointer.
  POP BC                  ; Fetch the length.
  PUSH DE                 ; Save the 'old' pointer. (Points to the location after the 'added' variable/line.)
  CALL RECLAIM_2          ; Remove the variable/line from the work space.
  POP DE                  ; Return with the 'old' pointer in the DE register pair.
  RET                     ;

; THE 'SAVE' CONTROL ROUTINE
;
; Used by the routine at SAVE_ETC.
;
; The operation of saving a program or a block of data is very straightforward.
;
; HL Data block start address
; IX Header start address
SA_CONTRL:
  PUSH HL                 ; Save the 'pointer'.
  LD A,$FD                ; Ensure that channel 'K' is open.
  CALL CHAN_OPEN          ;
  XOR A                   ; Signal 'first message'.
  LD DE,CASSETTE          ; Print the message 'Start tape, then press any key.' (see CASSETTE).
  CALL PO_MSG             ;
  SET 5,(IY+$02)          ; Signal 'screen will require to be cleared' (set bit 5 of TV-FLAG).
  CALL WAIT_KEY           ; Wait for a key to be pressed.
; Upon receipt of a keystroke the 'header' is saved.
  PUSH IX                 ; Save the base address of the 'header' on the machine stack.
  LD DE,$0011             ; Seventeen bytes are to be saved.
  XOR A                   ; Signal 'it is a header'.
  CALL SA_BYTES           ; Send the 'header', with a leading 'type' byte and a trailing 'parity' byte.
; There follows a short delay before the program/data block is saved.
  POP IX                  ; Retrieve the pointer to the 'header'.
  LD B,$32                ; The delay is for fifty interrupts, i.e. one second.
SA_1_SEC:
  HALT                    ;
  DJNZ SA_1_SEC           ;
  LD E,(IX+$0B)           ; Fetch the length of the data block that is to be saved.
  LD D,(IX+$0C)           ;
  LD A,$FF                ; Signal 'data block'.
  POP IX                  ; Fetch the 'start of block pointer' and save the block.
  JP SA_BYTES             ;

; THE CASSETTE MESSAGES
;
; Used by the routines at SAVE_ETC and SA_CONTRL.
;
; Each message is given with the last character inverted (plus +80).
CASSETTE:
  DEFB $80                ; Initial byte is stepped over.
  DEFM "Start tape, then press any key","."+$80 ; 'Start tape, then press any key.'
BLOCK_HDR:
  DEFM $0D,"Program:"," "+$80 ; Carriage return + 'Program: '
  DEFM $0D,"Number array:"," "+$80 ; Carriage return + 'Number array: '
  DEFM $0D,"Character array:"," "+$80 ; Carriage return + 'Character array: '
  DEFM $0D,"Bytes:"," "+$80 ; Carriage return + 'Bytes: '

; THE 'PRINT-OUT' ROUTINES
;
; Used by the routines at ED_COPY and OUT_FLASH.
;
; The address of this routine is found in the initial channel information table.
;
; All of the printing to the main part of the screen, the lower part of the screen and the printer is handled by this set of routines.
;
; This routine is entered with the A register holding the code for a control character, a printable character or a token.
;
; A Character code
PRINT_OUT:
  CALL PO_FETCH           ; The current print position.
  CP " "                  ; If the code represents a printable character then jump.
  JP NC,PO_ABLE           ;
  CP $06                  ; Print a question mark for codes in the range +00 to +05.
  JR C,PO_QUEST           ;
  CP $18                  ; And also for codes +18 to +1F.
  JR NC,PO_QUEST          ;
  LD HL,$0A0B             ; Base of the control character table.
  LD E,A                  ; Move the code to the DE register pair.
  LD D,$00                ;
  ADD HL,DE               ; Index into the table and fetch the offset.
  LD E,(HL)               ;
  ADD HL,DE               ; Add the offset and make an indirect jump to the appropriate subroutine.
  PUSH HL                 ;
  JP PO_FETCH             ;

; THE 'CONTROL CHARACTER' TABLE
;
; Used by the routine at PRINT_OUT.
CTRL_CHARS:
  DEFB $4E                ; +06: PRINT comma (PO_COMMA)
  DEFB $57                ; +07: EDIT (PO_QUEST)
  DEFB $10                ; +08: Cursor left (PO_BACK_1)
  DEFB $29                ; +09: Cursor right (PO_RIGHT)
  DEFB $54                ; +0A: Cursor down (PO_QUEST)
  DEFB $53                ; +0B: Cursor up (PO_QUEST)
  DEFB $52                ; +0C: DELETE (PO_QUEST)
  DEFB $37                ; +0D: ENTER (PO_ENTER)
  DEFB $50                ; +0E: Not used (PO_QUEST)
  DEFB $4F                ; +0F: Not used (PO_QUEST)
  DEFB $5F                ; +10: INK control (PO_1_OPER)
  DEFB $5E                ; +11: PAPER control (PO_1_OPER)
  DEFB $5D                ; +12: FLASH control (PO_1_OPER)
  DEFB $5C                ; +13: BRIGHT control (PO_1_OPER)
  DEFB $5B                ; +14: INVERSE control (PO_1_OPER)
  DEFB $5A                ; +15: OVER control (PO_1_OPER)
  DEFB $54                ; +16: AT control (PO_2_OPER)
  DEFB $53                ; +17: TAB control (PO_2_OPER)

; THE 'CURSOR LEFT' SUBROUTINE
;
; The address of this routine is derived from an offset found in the control character table.
;
; B Current line number
; C Current column number
PO_BACK_1:
  INC C                   ; Move leftwards by one column.
  LD A,$22                ; Accept the change unless up against the lefthand side.
  CP C                    ;
  JR NZ,PO_BACK_3         ;
  BIT 1,(IY+$01)          ; If dealing with the printer (bit 1 of FLAGS set) jump forward.
  JR NZ,PO_BACK_2         ;
  INC B                   ; Go up one line.
  LD C,$02                ; Set column value.
  LD A,$18                ; Test against top line. Note: this ought to be +19.
  CP B                    ;
  JR NZ,PO_BACK_3         ; Accept the change unless at the top of the screen.
  DEC B                   ; Unacceptable so down a line.
PO_BACK_2:
  LD C,$21                ; Set to lefthand column.
PO_BACK_3:
  JP CL_SET               ; Make an indirect return via CL_SET and PO_STORE.

; THE 'CURSOR RIGHT' SUBROUTINE
;
; The address of this routine is derived from an offset found in the control character table.
;
; This subroutine performs an operation identical to the BASIC statement 'PRINT OVER 1;CHR$ 32;'.
;
; B Current line number
; C Current column number
; HL Display file address or printer buffer address
PO_RIGHT:
  LD A,(P_FLAG)           ; Fetch P-FLAG and save it on the machine stack.
  PUSH AF                 ;
  LD (IY+$57),$01         ; Set P-FLAG to OVER 1.
  LD A," "                ; A 'space'.
  CALL PO_CHAR            ; Print the character.
  POP AF                  ; Fetch the old value of P-FLAG.
  LD (P_FLAG),A           ;
  RET                     ; Finished. Note: the programmer has forgotten to exit via PO_STORE, which is a bug.

; THE 'CARRIAGE RETURN' SUBROUTINE
;
; The address of this routine is derived from an offset found in the control character table.
;
; If the printing being handled is going to the printer then a carriage return character leads to the printer buffer being emptied. If the printing is to the screen then a test for 'scroll?' is made before decreasing the line number.
;
; B Current line number
PO_ENTER:
  BIT 1,(IY+$01)          ; Jump if handling the printer (bit 1 of FLAGS set).
  JP NZ,COPY_BUFF         ;
  LD C,$21                ; Set to lefthand column.
  CALL PO_SCR             ; Scroll if necessary.
  DEC B                   ; Now down a line.
  JP CL_SET               ; Make an indirect return via CL_SET and PO_STORE.

; THE 'PRINT COMMA' SUBROUTINE
;
; The address of this routine is derived from an offset found in the control character table.
;
; The current column value is manipulated and the A register set to hold +00 (for TAB 0) or +10 (for TAB 16).
PO_COMMA:
  CALL PO_FETCH           ; Why again?
  LD A,C                  ; Current column number.
  DEC A                   ; Move rightwards by two columns and then test.
  DEC A                   ;
  AND $10                 ; The A register will be +00 or +10.
  JR PO_FILL              ; Exit via PO_FILL.

; THE 'PRINT A QUESTION MARK' SUBROUTINE
;
; Used by the routine at PRINT_OUT.
;
; The address of this routine is derived from an offset found in the control character table.
;
; A question mark is printed whenever an attempt is made to print an unprintable code.
PO_QUEST:
  LD A,"?"                ; The character '?'.
  JR PO_ABLE              ; Now print this character instead.

; THE 'CONTROL CHARACTERS WITH OPERANDS' ROUTINE
;
; The control characters from INK to OVER require a single operand whereas the control characters AT and TAB are required to be followed by two operands.
;
; The present routine leads to the control character code being saved in TVDATA-lo, the first operand in TVDATA-hi or the A register if there is only a single operand required, and the second operand in the A register.
;
; A Control character code (+10 to +17)
PO_TV_2:
  LD DE,PO_CONT           ; Save the first operand in TVDATA-hi and change the address of the 'output' routine to PO_CONT.
  LD ($5C0F),A            ;
  JR PO_CHANGE            ;
; The address of this entry point is derived from an offset found in the control character table.
;
; Enter here when handling the characters AT and TAB.
PO_2_OPER:
  LD DE,PO_TV_2           ; The character code will be saved in TVDATA-lo and the address of the 'output' routine changed to PO_TV_2.
  JR PO_TV_1              ;
; The address of this entry point is derived from an offset found in the control character table.
;
; Enter here when handling the colour items - INK to OVER.
PO_1_OPER:
  LD DE,PO_CONT           ; The 'output' routine is to be changed to PO_CONT.
PO_TV_1:
  LD (TVDATA),A           ; Save the control character code in TVDATA-hi.
; The current 'output' routine address is changed temporarily.
PO_CHANGE:
  LD HL,(CURCHL)          ; HL will point to the 'output' routine address (CURCHL).
  LD (HL),E               ; Enter the new 'output' routine address and thereby force the next character code to be considered as an operand.
  INC HL                  ;
  LD (HL),D               ;
  RET                     ;
; Once the operands have been collected the routine continues.
PO_CONT:
  LD DE,PRINT_OUT         ; Restore the original address for PRINT_OUT.
  CALL PO_CHANGE          ;
  LD HL,(TVDATA)          ; Fetch the control code and the first operand from TVDATA if there are indeed two operands.
  LD D,A                  ; The 'last' operand and the control code are moved.
  LD A,L                  ;
  CP $16                  ; Jump forward if handling INK to OVER.
  JP C,CO_TEMP_5          ;
  JR NZ,PO_TAB            ; Jump forward if handling TAB.
; Now deal with the AT control character.
  LD B,H                  ; The line number.
  LD C,D                  ; The column number.
  LD A,$1F                ; Reverse the column number, i.e. +00 to +1F becomes +1F to +00.
  SUB C                   ;
  JR C,PO_AT_ERR          ; Must be in range.
  ADD A,$02               ; Add in the offset to give C holding +21 to +02.
  LD C,A                  ;
  BIT 1,(IY+$01)          ; Jump forward if handling the printer (bit 1 of FLAGS set).
  JR NZ,PO_AT_SET         ;
  LD A,$16                ; Reverse the line number, i.e. +00 to +15 becomes +16 to +01.
  SUB B                   ;
PO_AT_ERR:
  JP C,REPORT_B_2         ; If appropriate jump forward.
  INC A                   ; The range +16 to +01 becomes +17 to +02.
  LD B,A                  ;
  INC B                   ; And now +18 to +03.
  BIT 0,(IY+$02)          ; If printing in the lower part of the screen (bit 0 of TV-FLAG set) then consider whether scrolling is needed.
  JP NZ,PO_SCR            ;
  CP (IY+$31)             ; Give report 5 - Out of screen, if required (DF-SZ>A).
  JP C,REPORT_5           ;
PO_AT_SET:
  JP CL_SET               ; Return via CL_SET and PO_STORE.
; And the TAB control character.
PO_TAB:
  LD A,H                  ; Fetch the first operand.
; This entry point is used by the routine at PO_COMMA.
PO_FILL:
  CALL PO_FETCH           ; The current print position.
  ADD A,C                 ; Add the current column value.
  DEC A                   ; Find how many spaces, modulo 32, are required and return if the result is zero.
  AND $1F                 ;
  RET Z                   ;
  LD D,A                  ; Use D as the counter.
  SET 0,(IY+$01)          ; Suppress 'leading space' (set bit 0 of FLAGS).
PO_SPACE:
  LD A," "                ; Print D number of spaces.
  CALL PO_SAVE            ;
  DEC D                   ;
  JR NZ,PO_SPACE          ;
  RET                     ; Now finished.

; PRINTABLE CHARACTER CODES
;
; Used by the routines at PRINT_OUT and PO_QUEST.
;
; The required character (or characters) is printed by calling PO_ANY followed by PO_STORE.
;
; A Character code
PO_ABLE:
  CALL PO_ANY             ; Print the character(s) and continue into PO_STORE.

; THE 'POSITION STORE' SUBROUTINE
;
; Used by the routine at CL_SET.
;
; The routine at PO_ABLE continues here.
;
; The new position's 'line and column' values and the 'pixel' address are stored in the appropriate system variables.
;
; B Line number
; C Column number
; HL Display file address or printer buffer address
PO_STORE:
  BIT 1,(IY+$01)          ; Jump forward if handling the printer (bit 1 of FLAGS set).
  JR NZ,PO_ST_PR          ;
  BIT 0,(IY+$02)          ; Jump forward if handling the lower part of the screen (bit 0 of TV-FLAG set).
  JR NZ,PO_ST_E           ;
  LD (S_POSN),BC          ; Save the values that relate to the main part of the screen at S-POSN and DF-CC.
  LD (DF_CC),HL           ;
  RET                     ; Then return.
PO_ST_E:
  LD (S_POSNL),BC         ; Save the values that relate to the lower part of the screen at S-POSNL, ECHO-E and DF-CCL.
  LD (ECHO_E),BC          ;
  LD (DF_CCL),HL          ;
  RET                     ; Then return.
PO_ST_PR:
  LD (IY+$45),C           ; Save the values that relate to the printer buffer at P-POSN and PR-CC.
  LD (PR_CC),HL           ;
  RET                     ; Then return.

; THE 'POSITION FETCH' SUBROUTINE
;
; Used by the routines at PRINT_OUT, PO_COMMA, PO_TV_2 and PO_ANY.
;
; The current position's parameters are fetched from the appropriate system variables.
;
; O:B Line number
; O:C Column number
; O:HL Display file address or printer buffer address
PO_FETCH:
  BIT 1,(IY+$01)          ; Jump forward if handling the printer (bit 1 of FLAGS set).
  JR NZ,PO_F_PR           ;
  LD BC,(S_POSN)          ; Fetch the values relating to the main part of the screen from S-POSN and DF-CC and return if this was the intention (bit 0 of TV-FLAG set).
  LD HL,(DF_CC)           ;
  BIT 0,(IY+$02)          ;
  RET Z                   ;
  LD BC,(S_POSNL)         ; Otherwise fetch the values relating to the lower part of the screen from S-POSNL and DF-CCL.
  LD HL,(DF_CCL)          ;
  RET
PO_F_PR:
  LD C,(IY+$45)           ; Fetch the values relating to the printer buffer from P-POSN and PR-CC.
  LD HL,(PR_CC)           ;
  RET

; THE 'PRINT ANY CHARACTER(S)' SUBROUTINE
;
; Used by the routine at PO_ABLE.
;
; Ordinary character codes, token codes and user-defined graphic codes, and graphic codes are dealt with separately.
;
; A Character code
; B Line number
; C Column number
; HL Display file address or printer buffer address
PO_ANY:
  CP $80                  ; Jump forward with ordinary character codes.
  JR C,PO_CHAR            ;
  CP $90                  ; Jump forward with token codes and UDG codes.
  JR NC,PO_T_UDG          ;
  LD B,A                  ; Move the graphic code.
  CALL PO_GR_1            ; Construct the graphic form.
  CALL PO_FETCH           ; HL has been disturbed so 'fetch' again.
  LD DE,MEMBOT            ; Make DE point to the start of the graphic form, i.e. MEMBOT.
  JR PR_ALL               ; Jump forward to print the graphic character.
; Graphic characters are constructed in an ad hoc manner in the calculator's memory area, i.e. mem-0 and mem-1.
PO_GR_1:
  LD HL,MEMBOT            ; This is MEMBOT.
  CALL PO_GR_2            ; In effect call the following subroutine twice.
PO_GR_2:
  RR B                    ; Determine bit 0 (and later bit 2) of the graphic code.
  SBC A,A                 ;
  AND $0F                 ; The A register will hold +00 or +0F depending on the value of the bit in the code.
  LD C,A                  ; Save the result in C.
  RR B                    ; Determine bit 1 (and later bit 3) of the graphic code.
  SBC A,A                 ;
  AND $F0                 ; The A register will hold +00 or +F0.
  OR C                    ; The two results are combined.
  LD C,$04                ; The A register holds half the character form and has to be used four times. This is done for the upper half of the character form and then the lower.
PO_GR_3:
  LD (HL),A               ;
  INC HL                  ;
  DEC C                   ;
  JR NZ,PO_GR_3           ;
  RET                     ;
; Token codes and user-defined graphic codes are now separated.
PO_T_UDG:
  SUB $A5                 ; Jump forward with token codes.
  JR NC,PO_T              ;
  ADD A,$15               ; UDG codes are now +00 to +0F.
  PUSH BC                 ; Save the current position values on the machine stack.
  LD BC,(UDG)             ; Fetch the base address of the UDG area (from UDG) and jump forward.
  JR PO_CHAR_2            ;
PO_T:
  CALL PO_TOKENS          ; Now print the token and return via PO_FETCH.
  JP PO_FETCH             ;
; This entry point is used by the routine at PO_RIGHT.
;
; The required character form is identified.
PO_CHAR:
  PUSH BC                 ; The current position is saved.
  LD BC,(CHARS)           ; The base address of the character area is fetched (CHARS).
PO_CHAR_2:
  EX DE,HL                ; The print address is saved.
  LD HL,FLAGS             ; This is FLAGS.
  RES 0,(HL)              ; Allow for a leading space.
  CP " "                  ; Jump forward if the character is not a 'space'.
  JR NZ,PO_CHAR_3         ;
  SET 0,(HL)              ; But 'suppress' if it is.
PO_CHAR_3:
  LD H,$00                ; Now pass the character code to the HL register pair.
  LD L,A                  ;
  ADD HL,HL               ; The character code is in effect multiplied by 8.
  ADD HL,HL               ;
  ADD HL,HL               ;
  ADD HL,BC               ; The base address of the character form is found.
  POP BC                  ; The current position is fetched and the base address passed to the DE register pair.
  EX DE,HL                ;
; The following subroutine is used to print all '8*8' bit characters. On entry the DE register pair holds the base address of the character form, the HL register the destination address and the BC register pair the current 'line and column' values.
PR_ALL:
  LD A,C                  ; Fetch the column number.
  DEC A                   ; Move one column rightwards.
  LD A,$21                ; Jump forward unless a new line is indicated.
  JR NZ,PR_ALL_1          ;
  DEC B                   ; Move down one line.
  LD C,A                  ; Column number is +21.
  BIT 1,(IY+$01)          ; Jump forward if handling the screen (bit 1 of FLAGS reset).
  JR Z,PR_ALL_1           ;
  PUSH DE                 ; Save the base address whilst the printer buffer is emptied.
  CALL COPY_BUFF          ;
  POP DE                  ;
  LD A,C                  ; Copy the new column number.
PR_ALL_1:
  CP C                    ; Test whether a new line is being used. If it is see if the display requires to be scrolled.
  PUSH DE                 ;
  CALL Z,PO_SCR           ;
  POP DE                  ;
; Now consider the present state of INVERSE and OVER.
  PUSH BC                 ; Save the position values and the destination address on the machine stack.
  PUSH HL                 ;
  LD A,(P_FLAG)           ; Fetch P-FLAG and read bit 0.
  LD B,$FF                ; Prepare the 'OVER mask' in the B register, i.e. OVER 0=+00 and OVER 1=+FF.
  RRA                     ;
  JR C,PR_ALL_2           ;
  INC B                   ;
PR_ALL_2:
  RRA                     ; Read bit 2 of P-FLAG and prepare the 'INVERSE mask' in the C register, i.e. INVERSE 0=+00 and INVERSE 1=+FF.
  RRA                     ;
  SBC A,A                 ;
  LD C,A                  ;
  LD A,$08                ; Set the A register to hold the 'pixel-line' counter and clear the carry flag.
  AND A                   ;
  BIT 1,(IY+$01)          ; Jump forward if handling the screen (bit 1 of FLAGS reset).
  JR Z,PR_ALL_3           ;
  SET 1,(IY+$30)          ; Signal 'printer buffer no longer empty' (set bit 1 of FLAGS2).
  SCF                     ; Set the carry flag to show that the printer is being used.
PR_ALL_3:
  EX DE,HL                ; Exchange the destination address with the base address before entering the loop.
; The character can now be printed. Eight passes of the loop are made - one for each 'pixel-line'.
PR_ALL_4:
  EX AF,AF'               ; The carry flag is set when using the printer. Save this flag in F'.
  LD A,(DE)               ; Fetch the existing 'pixel-line'.
  AND B                   ; Use the 'OVER mask' and then 'XOR' the result with the 'pixel-line' of the character form.
  XOR (HL)                ;
  XOR C                   ; Finally consider the 'INVERSE mask'.
  LD (DE),A               ; Enter the result.
  EX AF,AF'               ; Fetch the printer flag and jump forward if required.
  JR C,PR_ALL_6           ;
  INC D                   ; Update the destination address.
PR_ALL_5:
  INC HL                  ; Update the 'pixel-line' address of the character form.
  DEC A                   ; Decrease the counter and loop back unless it is zero.
  JR NZ,PR_ALL_4          ;
; Once the character has been printed the attribute byte is to be set as required.
  EX DE,HL                ; Make the H register hold a correct high-address for the character area.
  DEC H                   ;
  BIT 1,(IY+$01)          ; Set the attribute byte only if handling the screen (bit 1 of FLAGS reset).
  CALL Z,PO_ATTR          ;
  POP HL                  ; Restore the original destination address and the position values.
  POP BC                  ;
  DEC C                   ; Decrease the column number and increase the destination address before returning.
  INC HL                  ;
  RET                     ;
; When the printer is being used the destination address has to be updated in increments of +20.
PR_ALL_6:
  EX AF,AF'               ; Save the printer flag again.
  LD A,$20                ; The required increment value.
  ADD A,E                 ; Add the value and pass the result back to the E register.
  LD E,A                  ;
  EX AF,AF'               ; Fetch the flag.
  JR PR_ALL_5             ; Jump back into the loop.

; THE 'SET ATTRIBUTE BYTE' SUBROUTINE
;
; Used by the routines at PO_ANY and PLOT.
;
; The appropriate attribute byte is identified and fetched. The new value is formed by manipulating the old value, ATTR-T, MASK-T and P-FLAG. Finally this new value is copied to the attribute area.
;
; HL Display file address
PO_ATTR:
  LD A,H                  ; The high byte of the destination address is divided by eight and ANDed with +03 to determine which third of the screen is being addressed, i.e. +00, +01 or +02.
  RRCA                    ;
  RRCA                    ;
  RRCA                    ;
  AND $03                 ;
  OR $58                  ; The high byte for the attribute area is then formed.
  LD H,A                  ;
  LD DE,(ATTR_T)          ; E holds ATTR-T, and D holds MASK-T.
  LD A,(HL)               ; The old attribute value.
  XOR E                   ; The values of MASK-T and ATTR-T are taken into account.
  AND D                   ;
  XOR E                   ;
  BIT 6,(IY+$57)          ; Jump forward unless dealing with PAPER 9 (bit 6 of P-FLAG set).
  JR Z,PO_ATTR_1          ;
  AND $C7                 ; The old paper colour is ignored and depending on whether the ink colour is light or dark the new paper colour will be black (000) or white (111).
  BIT 2,A                 ;
  JR NZ,PO_ATTR_1         ;
  XOR $38                 ;
PO_ATTR_1:
  BIT 4,(IY+$57)          ; Jump forward unless dealing with INK 9 (bit 4 of P-FLAG set).
  JR Z,PO_ATTR_2          ;
  AND $F8                 ; The old ink colour is ignored and depending on whether the paper colour is light or dark the new ink colour will be black (000) or white (111).
  BIT 5,A                 ;
  JR NZ,PO_ATTR_2         ;
  XOR $07                 ;
PO_ATTR_2:
  LD (HL),A               ; Enter the new attribute value and return.
  RET                     ;

; THE 'MESSAGE PRINTING' SUBROUTINE
;
; Used by the routines at SAVE_ETC, SA_CONTRL, PO_SCR, NEW and MAIN_EXEC.
;
; This subroutine is used to print messages and tokens.
;
; A Message table entry number
; DE Message table address (CASSETTE, BLOCK_HDR-1, SCROLL, REPORTS, COMMA_SPC-1 or COPYRIGHT-1)
PO_MSG:
  PUSH HL                 ; The high byte of the last entry on the machine stack is made zero so as to suppress trailing spaces (see below).
  LD H,$00                ;
  EX (SP),HL              ;
  JR PO_TABLE             ; Jump forward.
; This entry point is used by the routine at PO_ANY.
;
; Enter here when expanding token codes.
PO_TOKENS:
  LD DE,TOKENS            ; The base address of the token table.
  PUSH AF                 ; Save the code on the stack. (Range +00 to +5A, RND to COPY).
; The table is searched and the correct entry printed.
PO_TABLE:
  CALL PO_SEARCH          ; Locate the required entry.
  JR C,PO_EACH            ; Print the message/token.
  LD A," "                ; A 'space' will be printed before the message/token if required (bit 0 of FLAGS reset).
  BIT 0,(IY+$01)          ;
  CALL Z,PO_SAVE          ;
; The characters of the message/token are printed in turn.
PO_EACH:
  LD A,(DE)               ; Collect a code.
  AND $7F                 ; Cancel any 'inverted bit'.
  CALL PO_SAVE            ; Print the character.
  LD A,(DE)               ; Collect the code again.
  INC DE                  ; Advance the pointer.
  ADD A,A                 ; The 'inverted bit' goes to the carry flag and signals the end of the message/token; otherwise jump back.
  JR NC,PO_EACH           ;
; Now consider whether a 'trailing space' is required.
  POP DE                  ; For messages, D holds +00; for tokens, D holds +00 to +5A.
  CP $48                  ; Jump forward if the last character was a '$'.
  JR Z,PO_TR_SP           ;
  CP $82                  ; Return if the last character was any other before 'A'.
  RET C                   ;
PO_TR_SP:
  LD A,D                  ; Examine the value in D and return if it indicates a message, RND, INKEY$ or PI.
  CP $03                  ;
  RET C                   ;
  LD A," "                ; All other cases will require a 'trailing space'.
; This routine continues into PO_SAVE.

; THE 'PO-SAVE' SUBROUTINE
;
; Used by the routines at PO_TV_2 and PO_MSG.
;
; The routine at PO_MSG continues here.
;
; This subroutine allows for characters to be printed 'recursively'. The appropriate registers are saved whilst PRINT_A_1 is called.
;
; A Character code
PO_SAVE:
  PUSH DE                 ; Save the DE register pair.
  EXX                     ; Save HL and BC.
  RST $10                 ; Print the single character.
  EXX                     ; Restore HL and BC.
  POP DE                  ; Restore DE.
  RET                     ; Finished.

; THE 'TABLE SEARCH' SUBROUTINE
;
; Used by the routine at PO_MSG.
;
;   A Message table entry number
;   DE Message table start address
; O:DE Address of the first character of message number A
; O:F Carry flag: suppress (set) or allow (reset) a leading space
PO_SEARCH:
  PUSH AF                 ; Save the 'entry number'.
  EX DE,HL                ; HL now holds the base address.
  INC A                   ; Compensate for the 'DEC A' below.
PO_STEP:
  BIT 7,(HL)              ; Wait for an 'inverted character'.
  INC HL                  ;
  JR Z,PO_STEP            ;
  DEC A                   ; Count through the entries until the correct one is found.
  JR NZ,PO_STEP           ;
  EX DE,HL                ; DE points to the initial character.
  POP AF                  ; Fetch the 'entry number' and return with carry set for the first thirty two entries.
  CP $20                  ;
  RET C                   ;
  LD A,(DE)               ; However if the initial character is a letter then a leading space may be needed.
  SUB "A"                 ;
  RET                     ;

; THE 'TEST FOR SCROLL' SUBROUTINE
;
; Used by the routines at PO_ENTER, PO_TV_2 and PO_ANY.
;
; This subroutine is called whenever there might be the need to scroll the display. This occurs on three occasions:
;
; * when handling a 'carriage return' character
; * when using AT in an INPUT line
; * when the current line is full and the next line has to be used
;
; B Current line number
PO_SCR:
  BIT 1,(IY+$01)          ; Return immediately if the printer is being used (bit 1 of FLAGS set).
  RET NZ                  ;
  LD DE,CL_SET            ; Pre-load the machine stack with the address of CL_SET.
  PUSH DE                 ;
  LD A,B                  ; Transfer the line number.
  BIT 0,(IY+$02)          ; Jump forward if considering 'INPUT ... AT ...' (bit 0 of TV-FLAG set).
  JP NZ,PO_SCR_4          ;
  CP (IY+$31)             ; Return, via CL_SET, if the line number is greater than the value of DF-SZ; give report 5 if it is less; otherwise continue.
  JR C,REPORT_5           ;
  RET NZ                  ;
  BIT 4,(IY+$02)          ; Jump forward unless dealing with an 'automatic listing' (bit 4 of TV-FLAG set).
  JR Z,PO_SCR_2           ;
  LD E,(IY+$2D)           ; Fetch the line counter from BREG.
  DEC E                   ; Decrease this counter.
  JR Z,PO_SCR_3           ; Jump forward if the listing is to be scrolled.
  LD A,$00                ; Otherwise open channel 'K', restore the stack pointer, flag that the automatic listing has finished (reset bit 4 of TV-FLAG) and return via CL_SET.
  CALL CHAN_OPEN          ;
  LD SP,(LIST_SP)         ;
  RES 4,(IY+$02)          ;
  RET                     ;
; This entry point is used by the routine at PO_TV_2.
;
; Report 5 - Out of screen.
REPORT_5:
  RST $08                 ; Call the error handling routine.
  DEFB $04                ;
; Now consider if the prompt 'scroll?' is required.
PO_SCR_2:
  DEC (IY+$52)            ; Decrease the scroll counter (SCR-CT) and proceed to give the prompt only if it becomes zero.
  JR NZ,PO_SCR_3          ;
; Proceed to give the prompt message.
  LD A,$18                ; The scroll counter (SCR-CT) is reset.
  SUB B                   ;
  LD (SCR_CT),A           ;
  LD HL,(ATTR_T)          ; The current values of ATTR-T and MASK-T are saved.
  PUSH HL                 ;
  LD A,(P_FLAG)           ; The current value of P-FLAG is saved.
  PUSH AF                 ;
  LD A,$FD                ; Channel 'K' is opened.
  CALL CHAN_OPEN          ;
  XOR A                   ; The message 'scroll?' is message '0'. This message is now printed.
  LD DE,SCROLL            ;
  CALL PO_MSG             ;
  SET 5,(IY+$02)          ; Signal 'clear the lower screen after a keystroke' (set bit 5 of TV-FLAG).
  LD HL,FLAGS             ; This is FLAGS.
  SET 3,(HL)              ; Signal 'L mode'.
  RES 5,(HL)              ; Signal 'no key yet'.
  EXX                     ; Note: DE should be pushed also.
  CALL WAIT_KEY           ; Fetch a single key code.
  EXX                     ; Restore the registers.
  CP " "                  ; There is a jump forward to REPORT_D - 'BREAK - CONT repeats' - if the keystroke was 'BREAK', 'STOP', 'N' or 'n'; otherwise accept the keystroke as indicating the need to scroll the display.
  JR Z,REPORT_D           ;
  CP $E2                  ;
  JR Z,REPORT_D           ;
  OR $20                  ;
  CP "n"                  ;
  JR Z,REPORT_D           ;
  LD A,$FE                ; Open channel 'S'.
  CALL CHAN_OPEN          ;
  POP AF                  ; Restore the value of P-FLAG.
  LD (P_FLAG),A           ;
  POP HL                  ; Restore the values of ATTR-T and MASK-T.
  LD (ATTR_T),HL          ;
; The display is now scrolled.
PO_SCR_3:
  CALL CL_SC_ALL          ; The whole display is scrolled.
  LD B,(IY+$31)           ; The line (DF-SZ) and column numbers for the start of the line above the lower part of the display are found and saved.
  INC B                   ;
  LD C,$21                ;
  PUSH BC                 ;
  CALL CL_ADDR            ; The corresponding attribute byte for this character area is then found. The HL register pair holds the address of the byte.
  LD A,H                  ;
  RRCA                    ;
  RRCA                    ;
  RRCA                    ;
  AND $03                 ;
  OR $58                  ;
  LD H,A                  ;
; The line in question will have 'lower part' attribute values and the new line at the bottom of the display may have 'ATTR-P' values so the attribute values are exchanged.
  LD DE,$5AE0             ; DE points to the first attribute byte of the bottom line.
  LD A,(DE)               ; The value is fetched.
  LD C,(HL)               ; The 'lower part' value.
  LD B,$20                ; There are thirty two bytes.
  EX DE,HL                ; Exchange the pointers.
PO_SCR_3A:
  LD (DE),A               ; Make the first exchange and then proceed to use the same values for the thirty two attribute bytes of the two lines being handled.
  LD (HL),C               ;
  INC DE                  ;
  INC HL                  ;
  DJNZ PO_SCR_3A          ;
  POP BC                  ; The line and column numbers of the bottom line of the 'upper part' are fetched before returning.
  RET                     ;
; The 'scroll?' message.
SCROLL:
  DEFB $80                ; Initial marker - stepped over.
  DEFM "scroll"
  DEFM "?"+$80            ; The '?' is inverted.
; Report D - BREAK - CONT repeats.
REPORT_D:
  RST $08                 ; Call the error handling routine.
  DEFB $0C                ;
; The lower part of the display is handled as follows:
PO_SCR_4:
  CP $02                  ; The 'out of screen' error is given if the lower part is going to be 'too large' (see DF-SZ) and a return made if scrolling is unnecessary.
  JR C,REPORT_5           ;
  ADD A,(IY+$31)          ;
  SUB $19                 ;
  RET NC                  ;
  NEG                     ; The A register will now hold 'the number of scrolls to be made'.
  PUSH BC                 ; The line and column numbers are now saved.
  LD B,A                  ; The 'scroll number', ATTR-T, MASK-T and P-FLAG are all saved.
  LD HL,(ATTR_T)          ;
  PUSH HL                 ;
  LD HL,(P_FLAG)          ;
  PUSH HL                 ;
  CALL TEMPS              ; The 'permanent' colour items are to be used.
  LD A,B                  ; The 'scroll number' is fetched.
; The lower part of the screen is now scrolled A number of times.
PO_SCR_4A:
  PUSH AF                 ; Save the 'number'.
  LD HL,DF_SZ             ; This is DF-SZ.
  LD B,(HL)               ; The value in DF-SZ is incremented; the B register set to hold the former value and the A register the new value.
  LD A,B                  ;
  INC A                   ;
  LD (HL),A               ;
  LD HL,$5C89             ; This is S-POSN-hi.
  CP (HL)                 ; The jump is taken if only the lower part of the display is to be scrolled (B=old DF-SZ).
  JR C,PO_SCR_4B          ;
  INC (HL)                ; Otherwise S-POSN-hi is incremented and the whole display scrolled (B=+18).
  LD B,$18                ;
PO_SCR_4B:
  CALL CL_SCROLL          ; Scroll B lines.
  POP AF                  ; Fetch and decrement the 'scroll number'.
  DEC A                   ;
  JR NZ,PO_SCR_4A         ; Jump back until finished.
  POP HL                  ; Restore the value of P-FLAG.
  LD (IY+$57),L           ;
  POP HL                  ; Restore the values of ATTR-T and MASK-T.
  LD (ATTR_T),HL          ;
  LD BC,(S_POSN)          ; In case S-POSN has been changed CL_SET is called to give a matching value to DF-CC (after resetting bit 0 of TV-FLAG).
  RES 0,(IY+$02)          ;
  CALL CL_SET             ;
  SET 0,(IY+$02)          ; Set bit 0 of TV-FLAG to indicate that the lower screen is being handled, fetch the line and column numbers, and then return.
  POP BC                  ;
  RET                     ;

; THE 'TEMPORARY COLOUR ITEMS' SUBROUTINE
;
; Used by the routines at PO_SCR, CLS, CL_ALL, ED_COPY, CHAN_S, CLASS_07, CLASS_09, LPRINT, PLOT and DRAW.
;
; This is a most important subroutine. It is used whenever the 'permanent' details are required to be copied to the 'temporary' system variables. First ATTR-T and MASK-T are considered.
TEMPS:
  XOR A                   ; A is set to hold +00.
  LD HL,(ATTR_P)          ; The current values of ATTR-P and MASK-P are fetched.
  BIT 0,(IY+$02)          ; Jump forward if handing the main part of the screen (bit 0 of TV-FLAG reset).
  JR Z,TEMPS_1            ;
  LD H,A                  ; Otherwise use +00 and the value in BORDCR instead.
  LD L,(IY+$0E)           ;
TEMPS_1:
  LD (ATTR_T),HL          ; Now set ATTR-T and MASK-T.
; Next P-FLAG is considered.
  LD HL,P_FLAG            ; This is P-FLAG.
  JR NZ,TEMPS_2           ; Jump forward if dealing with the lower part of the screen (A=+00).
  LD A,(HL)               ; Otherwise fetch the value of P-FLAG and move the odd bits to the even bits.
  RRCA                    ;
TEMPS_2:
  XOR (HL)                ; Proceed to copy the even bits of A to P-FLAG.
  AND %01010101           ;
  XOR (HL)                ;
  LD (HL),A               ;
  RET

; THE 'CLS' COMMAND ROUTINE
;
; Used by the routines at NEW and CLEAR.
;
; The address of this routine is found in the parameter table.
;
; In the first instance the whole of the display is 'cleared' - the 'pixels' are all reset and the attribute bytes are set to equal the value in ATTR-P - then the lower part of the display is reformed.
CLS:
  CALL CL_ALL             ; The whole of the display is 'cleared'.
; This entry point is used by the routines at KEY_INPUT, MAIN_EXEC and INPUT.
CLS_LOWER:
  LD HL,TV_FLAG           ; This is TV-FLAG.
  RES 5,(HL)              ; Signal 'do not clear the lower screen after keystroke'.
  SET 0,(HL)              ; Signal 'lower part'.
  CALL TEMPS              ; Use the permanent values, i.e. ATTR-T is copied from BORDCR.
  LD B,(IY+$31)           ; The lower part of the screen is now 'cleared' with these values (B=DF-SZ).
  CALL CL_LINE            ;
; With the exception of the attribute bytes for lines 22 and 23 the attribute bytes for the lines in the lower part of the display will need to be made equal to ATTR-P.
  LD HL,$5AC0             ; Attribute byte at start of line 22.
  LD A,(ATTR_P)           ; Fetch ATTR-P.
  DEC B                   ; The line counter.
  JR CLS_3                ; Jump forward into the loop.
CLS_1:
  LD C,$20                ; +20 characters per line.
CLS_2:
  DEC HL                  ; Go back along the line setting the attribute bytes.
  LD (HL),A               ;
  DEC C                   ;
  JR NZ,CLS_2             ;
CLS_3:
  DJNZ CLS_1              ; Loop back until finished.
; The size of the lower part of the display can now be fixed.
  LD (IY+$31),$02         ; It will be two lines in size (DF-SZ).
; This entry point is used by the routine at CL_ALL.
;
; It now remains for the following 'house keeping' tasks to be performed.
CL_CHAN:
  LD A,$FD                ; Open channel 'K'.
  CALL CHAN_OPEN          ;
  LD HL,(CURCHL)          ; Fetch the address of the current channel (CURCHL) and make the output address PRINT_OUT and the input address KEY_INPUT.
  LD DE,PRINT_OUT         ;
  AND A                   ;
CL_CHAN_A:
  LD (HL),E               ;
  INC HL                  ;
  LD (HL),D               ;
  INC HL                  ;
  LD DE,KEY_INPUT         ;
  CCF                     ; First the output address then the input address.
  JR C,CL_CHAN_A          ;
  LD BC,$1721             ; As the lower part of the display is being handled the 'lower print line' will be line 23.
  JR CL_SET               ; Return via CL_SET.

; THE 'CLEARING THE WHOLE DISPLAY AREA' SUBROUTINE
;
; Used by the routines at CLS, MAIN_EXEC and AUTO_LIST.
CL_ALL:
  LD HL,$0000             ; The system variable COORDS is reset to zero.
  LD (COORDS),HL          ;
  RES 0,(IY+$30)          ; Signal 'the screen is clear' (reset bit 0 of FLAGS2).
  CALL CL_CHAN            ; Perform the 'house keeping' tasks.
  LD A,$FE                ; Open channel 'S'.
  CALL CHAN_OPEN          ;
  CALL TEMPS              ; Use the 'permanent' values.
  LD B,$18                ; Now 'clear' the 24 lines of the display.
  CALL CL_LINE            ;
  LD HL,(CURCHL)          ; Ensure that the current output address (at (CURCHL)) is PRINT_OUT.
  LD DE,PRINT_OUT         ;
  LD (HL),E               ;
  INC HL                  ;
  LD (HL),D               ;
  LD (IY+$52),$01         ; Reset the scroll counter (SCR-CT).
  LD BC,$1821             ; As the upper part of the display is being handled the 'upper print line' will be line 0.
; This routine continues into CL_SET.

; THE 'CL-SET' SUBROUTINE
;
; Used by the routines at PO_BACK_1, PO_ENTER, PO_TV_2, PO_SCR, CLS, CLEAR_PRB, ED_COPY and INPUT.
;
; The routine at CL_ALL continues here.
;
; This subroutine is entered with the BC register pair holding the line and column numbers of a character area, or the C register holding the column number within the printer buffer. The appropriate address of the first character bit is then found. The subroutine returns via PO_STORE so as to store all the values in the required system variables.
;
; B Line number
; C Column number
CL_SET:
  LD HL,$5B00             ; The start of the printer buffer.
  BIT 1,(IY+$01)          ; Jump forward if handling the printer buffer (bit 1 of FLAGS set).
  JR NZ,CL_SET_2          ;
  LD A,B                  ; Transfer the line number.
  BIT 0,(IY+$02)          ; Jump forward if handling the main part of the display (bit 0 of TV-FLAG reset).
  JR Z,CL_SET_1           ;
  ADD A,(IY+$31)          ; The top line of the lower part of the display is called 'line +18' and this has to be converted (see DF-SZ).
  SUB $18                 ;
CL_SET_1:
  PUSH BC                 ; The line and column numbers are saved.
  LD B,A                  ; The line number is moved.
  CALL CL_ADDR            ; The address for the start of the line is formed in HL.
  POP BC                  ; The line and column numbers are fetched back.
CL_SET_2:
  LD A,$21                ; The column number is now reversed and transferred to the DE register pair.
  SUB C                   ;
  LD E,A                  ;
  LD D,$00                ;
  ADD HL,DE               ; The required address is now formed, and the address and the line and column numbers are stored by jumping to PO_STORE.
  JP PO_STORE             ;

; THE 'SCROLLING' SUBROUTINE
;
; Used by the routine at PO_SCR.
;
; The number of lines of the display that are to be scrolled has to be held on entry to the main subroutine in the B register.
CL_SC_ALL:
  LD B,$17                ; The entry point after 'scroll?'
; This entry point is used by the routine at PO_SCR.
;
; The main entry point - from above and when scrolling for INPUT...AT.
CL_SCROLL:
  CALL CL_ADDR            ; Find the starting address of the line.
  LD C,$08                ; There are eight pixel lines to a complete line.
; Now enter the main scrolling loop. The B register holds the number of the top line to be scrolled, the HL register pair the starting address in the display area of this line and the C register the pixel line counter.
CL_SCR_1:
  PUSH BC                 ; Save both counters.
  PUSH HL                 ; Save the starting address.
  LD A,B                  ; Jump forward unless dealing at the present moment with a 'third' of the display.
  AND $07                 ;
  LD A,B                  ;
  JR NZ,CL_SCR_3          ;
; The pixel lines of the top lines of the 'thirds' of the display have to be moved across the 2K boundaries. (Each 'third' is 2K.)
CL_SCR_2:
  EX DE,HL                ; The result of this manipulation is to leave HL unchanged and DE pointing to the required destination.
  LD HL,$F8E0             ;
  ADD HL,DE               ;
  EX DE,HL                ;
  LD BC,$0020             ; There are +20 characters.
  DEC A                   ; Decrease the counter as one line is being dealt with.
  LDIR                    ; Now move the thirty two bytes.
; The pixel lines within the 'thirds' can now be scrolled. The A register holds, on the first pass, +01 to +07, +09 to +0F, or +11 to +17.
CL_SCR_3:
  EX DE,HL                ; Again DE is made to point to the required destination, this time only thirty two locations away.
  LD HL,$FFE0             ;
  ADD HL,DE               ;
  EX DE,HL                ;
  LD B,A                  ; Save the line number in B.
  AND $07                 ; Now find how many characters there are remaining in the 'third'.
  RRCA                    ;
  RRCA                    ;
  RRCA                    ;
  LD C,A                  ; Pass the 'character total' to the C register.
  LD A,B                  ; Fetch the line number.
  LD B,$00                ; BC holds the 'character total' and a pixel line from each of the characters is 'scrolled'.
  LDIR                    ;
  LD B,$07                ; Now prepare to increment the address to jump across a 'third' boundary.
  ADD HL,BC               ; Increase HL by +0700.
  AND $F8                 ; Jump back if there are any 'thirds' left to consider.
  JR NZ,CL_SCR_2          ;
; Now find if the loop has been used eight times - once for each pixel line.
  POP HL                  ; Fetch the original address.
  INC H                   ; Address the next pixel line.
  POP BC                  ; Fetch the counters.
  DEC C                   ; Decrease the pixel line counter and jump back unless eight lines have been moved.
  JR NZ,CL_SCR_1          ;
; Next the attribute bytes are scrolled. Note that the B register still holds the number of lines to be scrolled and the C register holds zero.
  CALL CL_ATTR            ; The required address in the attribute area and the number of characters in B lines are found.
  LD HL,$FFE0             ; The displacement for all the attribute bytes is thirty two locations away.
  ADD HL,DE               ;
  EX DE,HL                ;
  LDIR                    ; The attribute bytes are 'scrolled'.
; It remains now to clear the bottom line of the display.
  LD B,$01                ; The B register is loaded with +01 and CL_LINE is entered.

; THE 'CLEAR LINES' SUBROUTINE
;
; Used by the routines at CLS, CL_ALL and AUTO_LIST.
;
; The routine at CL_SC_ALL continues here.
;
; This subroutine will clear the bottom B lines of the display.
;
; B Number of lines to clear
CL_LINE:
  PUSH BC                 ; The line number is saved for the duration of the subroutine.
  CALL CL_ADDR            ; The starting address for the line is formed in HL.
  LD C,$08                ; Again there are eight pixel lines to be considered.
; Now enter a loop to clear all the pixel lines.
CL_LINE_1:
  PUSH BC                 ; Save the line number and the pixel line counter.
  PUSH HL                 ; Save the address.
  LD A,B                  ; Save the line number in A.
CL_LINE_2:
  AND $07                 ; Find how many characters are involved in 'B mod 8' lines. Pass the result to the C register. (C will hold +00, i.e. 256, for a 'third'.)
  RRCA                    ;
  RRCA                    ;
  RRCA                    ;
  LD C,A                  ;
  LD A,B                  ; Fetch the line number.
  LD B,$00                ; Make the BC register pair hold one less than the number of characters.
  DEC C                   ;
  LD D,H                  ; Make DE point to the first character.
  LD E,L                  ;
  LD (HL),$00             ; Clear the pixel-byte of the first character.
  INC DE                  ; Make DE point to the second character and then clear the pixel-bytes of all the other characters.
  LDIR                    ;
  LD DE,$0701             ; For each 'third' of the display HL has to be increased by +0701.
  ADD HL,DE               ;
  DEC A                   ; Now decrease the line number.
  AND $F8                 ; Discard any extra lines and pass the 'third' count to B.
  LD B,A                  ;
  JR NZ,CL_LINE_2         ; Jump back if there are still 'thirds' to be dealt with.
; Now find if the loop has been used eight times.
  POP HL                  ; Update the address for each pixel line.
  INC H                   ;
  POP BC                  ; Fetch the counters.
  DEC C                   ; Decrease the pixel line counter and jump back unless finished.
  JR NZ,CL_LINE_1         ;
; Next the attribute bytes are set as required. The value in ATTR-P will be used when handling the main part of the display and the value in BORDCR when handling the lower part.
  CALL CL_ATTR            ; The address of the first attribute byte and the number of bytes are found.
  LD H,D                  ; HL will point to the first attribute byte and DE the second.
  LD L,E                  ;
  INC DE                  ;
  LD A,(ATTR_P)           ; Fetch the value in ATTR-P.
  BIT 0,(IY+$02)          ; Jump forward if handling the main part of the screen (bit 0 of TV-FLAG reset).
  JR Z,CL_LINE_3          ;
  LD A,(BORDCR)           ; Otherwise use BORDCR instead.
CL_LINE_3:
  LD (HL),A               ; Set the attribute byte.
  DEC BC                  ; One byte has been done.
  LDIR                    ; Now copy the value to all the attribute bytes.
  POP BC                  ; Restore the line number.
  LD C,$21                ; Set the column number to the lefthand column and return.
  RET                     ;

; THE 'CL-ATTR' SUBROUTINE
;
; Used by the routines at CL_SC_ALL and CL_LINE.
;
; This subroutine has two separate functions.
;
; * For a given display area address the appropriate attribute address is returned in the DE register pair. Note that the value on entry points to the 'ninth' line of a character.
; * For a given line number, in the B register, the number of character areas in the display from the start of that line onwards is returned in the BC register pair.
;
;   B Line number
;   C +00
;   HL Display file address
; O:BC Number of spaces from the given line number (B) downwards
; O:DE Corresponding attribute file address
CL_ATTR:
  LD A,H                  ; Fetch the high byte.
  RRCA                    ; Multiply this value by thirty two.
  RRCA                    ;
  RRCA                    ;
  DEC A                   ; Go back to the 'eight' line.
  OR $50                  ; Address the attribute area.
  LD H,A                  ; Restore to the high byte and transfer the address to DE.
  EX DE,HL                ;
  LD H,C                  ; This is always zero.
  LD L,B                  ; The line number.
  ADD HL,HL               ; Multiply by thirty two.
  ADD HL,HL               ;
  ADD HL,HL               ;
  ADD HL,HL               ;
  ADD HL,HL               ;
  LD B,H                  ; Move the result to the BC register pair before returning.
  LD C,L                  ;
  RET                     ;

; THE 'CL-ADDR' SUBROUTINE
;
; Used by the routines at PO_SCR, CL_SET, CL_SC_ALL and CL_LINE.
;
; For a given line number, in the B register, the appropriate display file address is formed in the HL register pair.
;
;   B Line number
; O:HL Display file address
CL_ADDR:
  LD A,$18                ; The line number has to be reversed.
  SUB B                   ;
  LD D,A                  ; The result is saved in D.
  RRCA                    ; In effect '(A mod 8)*32'. In a 'third' of the display the low byte for the first line is +00, for the second line +20, etc.
  RRCA                    ;
  RRCA                    ;
  AND $E0                 ;
  LD L,A                  ; The low byte goes into L.
  LD A,D                  ; The true line number is fetched.
  AND $18                 ; In effect '64+8*INT (A/8)'. For the upper 'third' of the display the high byte is +40, for the middle 'third' +48, and for the lower 'third' +50.
  OR $40                  ;
  LD H,A                  ; The high byte goes to H.
  RET                     ; Finished.

; THE 'COPY' COMMAND ROUTINE
;
; The address of this routine is found in the parameter table.
;
; The one hundred and seventy six pixel lines of the display are dealt with one by one.
COPY:
  DI                      ; The maskable interrupt is disabled during COPY.
  LD B,$B0                ; The 176 lines.
  LD HL,$4000             ; The base address of the display.
; The following loop is now entered.
COPY_1:
  PUSH HL                 ; Save the base address and the number of the line.
  PUSH BC                 ;
  CALL COPY_LINE          ; It is called 176 times.
  POP BC                  ; Fetch the line number and the base address.
  POP HL                  ;
  INC H                   ; The base address is updated by 256 locations for each line of pixels.
  LD A,H                  ; Jump forward and hence round the loop again directly for the eight pixel lines of a character line.
  AND $07                 ;
  JR NZ,COPY_2            ;
; For each new line of characters the base address has to be updated.
  LD A,L                  ; Fetch the low byte.
  ADD A,$20               ; Update it by +20 bytes.
  LD L,A                  ; The carry flag will be reset when 'within thirds' of the display.
  CCF                     ; Change the carry flag.
  SBC A,A                 ; The A register will hold +F8 when within a 'third' but +00 when a new 'third' is reached.
  AND $F8                 ;
  ADD A,H                 ; The high byte of the address is now updated.
  LD H,A                  ;
COPY_2:
  DJNZ COPY_1             ; Jump back until 176 lines have been printed.
  JR COPY_END             ; Jump forward to the end routine.

; THE 'COPY-BUFF' SUBROUTINE
;
; Used by the routines at PO_ENTER, PO_ANY and MAIN_EXEC.
;
; This subroutine is called whenever the printer buffer is to have its contents passed to the printer.
COPY_BUFF:
  DI                      ; Disable the maskable interrupt.
  LD HL,$5B00             ; The base address of the printer buffer.
  LD B,$08                ; There are eight pixel lines.
COPY_3:
  PUSH BC                 ; Save the line number.
  CALL COPY_LINE          ; Print the line.
  POP BC                  ; Fetch the line number.
  DJNZ COPY_3             ; Jump back until 8 lines have been printed.
; This entry point is used by the routine at COPY.
COPY_END:
  LD A,$04                ; Stop the printer motor.
  OUT ($FB),A             ;
  EI                      ; Enable the maskable interrupt and continue into CLEAR_PRB.

; THE 'CLEAR PRINTER BUFFER' SUBROUTINE
;
; Used by the routines at COPY_LINE and NEW.
;
; The printer buffer is cleared by calling this subroutine.
CLEAR_PRB:
  LD HL,$5B00             ; The base address of the printer buffer.
  LD (IY+$46),L           ; Reset the printer 'column' at PR-CC.
  XOR A                   ; Clear the A register.
  LD B,A                  ; Also clear the B register (in effect B holds 256).
PRB_BYTES:
  LD (HL),A               ; The 256 bytes of the printer buffer are all cleared in turn.
  INC HL                  ;
  DJNZ PRB_BYTES          ;
  RES 1,(IY+$30)          ; Signal 'the buffer is empty' (reset bit 1 of FLAGS2).
  LD C,$21                ; Set the printer position and return via CL_SET and PO_STORE.
  JP CL_SET               ;

; THE 'COPY-LINE' SUBROUTINE
;
; Used by the routines at COPY and COPY_BUFF.
;
; The subroutine is entered with the HL register pair holding the base address of the thirty two bytes that form the pixel-line and the B register holding the pixel-line number.
;
; B Pixel line number (+01 to +B0)
; HL Display file address or printer buffer address
COPY_LINE:
  LD A,B                  ; Copy the pixel-line number.
  CP $03                  ; The A register will hold +00 until the last two lines are being handled.
  SBC A,A                 ;
  AND $02                 ;
  OUT ($FB),A             ; Slow the motor for the last two pixel lines only.
  LD D,A                  ; The D register will hold either +00 or +02.
; There are three tests to be made before doing any 'printing'.
COPY_L_1:
  CALL BREAK_KEY          ; Jump forward unless the BREAK key is being pressed.
  JR C,COPY_L_2           ;
  LD A,$04                ; But if it is then stop the motor, enable the maskable interrupt, clear the printer buffer and exit via the error handling routine - 'BREAK-CONT repeats'.
  OUT ($FB),A             ;
  EI                      ;
  CALL CLEAR_PRB          ;
  RST $08                 ;
  DEFB $0C                ;
COPY_L_2:
  IN A,($FB)              ; Fetch the status of the printer.
  ADD A,A                 ;
  RET M                   ; Make an immediate return if the printer is not present.
  JR NC,COPY_L_1          ; Wait for the stylus.
  LD C,$20                ; There are thirty two bytes.
; Now enter a loop to handle these bytes.
COPY_L_3:
  LD E,(HL)               ; Fetch a byte.
  INC HL                  ; Update the pointer.
  LD B,$08                ; Eight bits per byte.
COPY_L_4:
  RL D                    ; Move D left.
  RL E                    ; Move each bit into the carry.
  RR D                    ; Move D back again, picking up the carry from E.
COPY_L_5:
  IN A,($FB)              ; Again fetch the status of the printer and wait for the signal from the encoder.
  RRA                     ;
  JR NC,COPY_L_5          ;
  LD A,D                  ; Now go ahead and pass the 'bit' to the printer. Note: bit 2 low starts the motor, bit 1 high slows the motor, and bit 7 is high for the actual 'printing'.
  OUT ($FB),A             ;
  DJNZ COPY_L_4           ; 'Print' each bit.
  DEC C                   ; Decrease the byte counter.
  JR NZ,COPY_L_3          ; Jump back whilst there are still bytes; otherwise return.
  RET                     ;

; THE 'EDITOR' ROUTINES
;
; Used by the routines at MAIN_EXEC and INPUT.
;
; The editor is called on two occasions:
;
; * From the main execution routine so that the user can enter a BASIC line into the system.
; * From the INPUT command routine.
;
; First the 'error stack pointer' is saved and an alternative address provided.
EDITOR:
  LD HL,(ERR_SP)          ; The current value of ERR-SP is saved on the machine stack.
  PUSH HL                 ;
; This entry point is used by the routine at ED_ERROR.
ED_AGAIN:
  LD HL,ED_ERROR          ; This is ED_ERROR.
  PUSH HL                 ; Any event that leads to the error handling routine (see ERR-SP) being used will come back to ED_ERROR.
  LD (ERR_SP),SP          ;
; A loop is now entered to handle each keystroke.
ED_LOOP:
  CALL WAIT_KEY           ; Return once a key has been pressed.
  PUSH AF                 ; Save the code temporarily.
  LD D,$00                ; Fetch the duration of the keyboard click (PIP).
  LD E,(IY-$01)           ;
  LD HL,$00C8             ; And the pitch.
  CALL BEEPER             ; Now make the 'pip'.
  POP AF                  ; Restore the code.
  LD HL,ED_LOOP           ; Pre-load the machine stack with the address of ED_LOOP.
  PUSH HL                 ;
; Now analyse the code obtained.
  CP $18                  ; Accept all character codes, graphic codes and tokens.
  JR NC,ADD_CHAR          ;
  CP $07                  ; Also accept ','.
  JR C,ADD_CHAR           ;
  CP $10                  ; Jump forward if the code represents an editing key.
  JR C,ED_KEYS            ;
; The control keys - INK to TAB - are now considered.
  LD BC,$0002             ; INK and PAPER will require two locations.
  LD D,A                  ; Copy the code to D.
  CP $16                  ; Jump forward with INK and PAPER.
  JR C,ED_CONTR           ;
; AT and TAB would be handled as follows:
  INC BC                  ; Three locations required.
  BIT 7,(IY+$37)          ; Jump forward unless dealing with 'INPUT LINE...' (bit 7 of FLAGX set).
  JP Z,ED_IGNORE          ;
  CALL WAIT_KEY           ; Get the second code and put it in E.
  LD E,A                  ;
; The other bytes for the control characters are now fetched.
ED_CONTR:
  CALL WAIT_KEY           ; Get another code.
  PUSH DE                 ; Save the previous codes.
  LD HL,(K_CUR)           ; Fetch K-CUR.
  RES 0,(IY+$07)          ; Signal 'K mode' (reset bit 0 of MODE).
  CALL MAKE_ROOM          ; Make two or three spaces.
  POP BC                  ; Restore the previous codes.
  INC HL                  ; Point to the first location.
  LD (HL),B               ; Enter first code.
  INC HL                  ; Then enter the second code which will be overwritten if there are only two codes - i.e. with INK and PAPER.
  LD (HL),C               ;
  JR ADD_CH_1             ; Jump forward.
; This entry point is used by the routine at ED_SYMBOL.
;
; The address of this entry point is found in the initial channel information table.
;
; The following subroutine actually adds a code to the current EDIT or INPUT line.
ADD_CHAR:
  RES 0,(IY+$07)          ; Signal 'K mode' (reset bit 0 of MODE).
  LD HL,(K_CUR)           ; Fetch the cursor position (K-CUR).
  CALL ONE_SPACE          ; Make a single space.
ADD_CH_1:
  LD (DE),A               ; Enter the code into the space and set K-CUR to signal that the cursor is to occur at the location after. Then return indirectly to ED_LOOP.
  INC DE                  ;
  LD (K_CUR),DE           ;
  RET                     ;
; The editing keys are dealt with as follows:
ED_KEYS:
  LD E,A                  ; The code is transferred to the DE register pair.
  LD D,$00                ;
  LD HL,$0F99             ; The base address of the editing keys table.
  ADD HL,DE               ; The entry is addressed and then fetched into E.
  LD E,(HL)               ;
  ADD HL,DE               ; The address of the handling routine is saved on the machine stack.
  PUSH HL                 ;
  LD HL,(K_CUR)           ; The HL register pair is set to K-CUR and an indirect jump made to the required routine.
  RET                     ;

; THE 'EDITING KEYS' TABLE
;
; Used by the routine at EDITOR.
EDITKEYS:
  DEFB $09                ; EDIT (ED_EDIT)
  DEFB $66                ; Cursor left (ED_LEFT)
  DEFB $6A                ; Cursor right (ED_RIGHT)
  DEFB $50                ; Cursor down (ED_DOWN)
  DEFB $B5                ; Cursor up (ED_UP)
  DEFB $70                ; DELETE (ED_DELETE)
  DEFB $7E                ; ENTER (ED_ENTER)
  DEFB $CF                ; SYMBOL SHIFT (ED_SYMBOL)
  DEFB $D4                ; GRAPHICS (ED_GRAPH)

; THE 'EDIT KEY' SUBROUTINE
;
; The address of this routine is derived from an offset found in the editing keys table.
;
; When in 'editing mode' pressing the EDIT key will bring down the 'current BASIC line'. However in 'INPUT mode' the action of the EDIT key is to clear the current reply and allow a fresh one.
ED_EDIT:
  LD HL,(E_PPC)           ; Fetch the current line number (E-PPC).
  BIT 5,(IY+$37)          ; But jump forward if in 'INPUT mode' (bit 5 of FLAGX set).
  JP NZ,CLEAR_SP          ;
  CALL LINE_ADDR          ; Find the address of the start of the current line and hence its number.
  CALL LINE_NO            ;
  LD A,D                  ; If the line number returned is zero then simply clear the editing area.
  OR E                    ;
  JP Z,CLEAR_SP           ;
  PUSH HL                 ; Save the address of the line.
  INC HL                  ; Move on to collect the length of the line.
  LD C,(HL)               ;
  INC HL                  ;
  LD B,(HL)               ;
  LD HL,$000A             ; Add +0A to the length and test that there is sufficient room for a copy of the line.
  ADD HL,BC               ;
  LD B,H                  ;
  LD C,L                  ;
  CALL TEST_ROOM          ;
  CALL CLEAR_SP           ; Now clear the editing area.
  LD HL,(CURCHL)          ; Fetch the current channel address (CURCHL) and exchange it for the address of the line.
  EX (SP),HL              ;
  PUSH HL                 ; Save it temporarily.
  LD A,$FF                ; Open channel 'R' so that the line will be copied to the editing area.
  CALL CHAN_OPEN          ;
  POP HL                  ; Fetch the address of the line.
  DEC HL                  ; Go to before the line.
  DEC (IY+$0F)            ; Decrement the current line number (E-PPC) so as to avoid printing the cursor.
  CALL OUT_LINE           ; Print the BASIC line.
  INC (IY+$0F)            ; Increment the current line number (E-PPC). Note: the decrementing of the line number does not always stop the cursor from being printed.
  LD HL,(E_LINE)          ; Fetch the start of the line in the editing area (E-LINE) and step past the line number and the length to find the address for K-CUR.
  INC HL                  ;
  INC HL                  ;
  INC HL                  ;
  INC HL                  ;
  LD (K_CUR),HL           ;
  POP HL                  ; Fetch the former channel address and set the appropriate flags before returning to ED_LOOP.
  CALL CHAN_FLAG          ;
  RET                     ;

; THE 'CURSOR DOWN EDITING' SUBROUTINE
;
; The address of this routine is derived from an offset found in the editing keys table.
ED_DOWN:
  BIT 5,(IY+$37)          ; Jump forward if in 'INPUT mode' (bit 5 of FLAGX set).
  JR NZ,ED_STOP           ;
  LD HL,E_PPC             ; This is E-PPC.
  CALL LN_FETCH           ; The next line number is found and a new automatic listing produced.
  JR ED_LIST              ;
ED_STOP:
  LD (IY+$00),$10         ; 'STOP in INPUT' report (ERR-NR).
  JR ED_ENTER             ; Jump forward.

; THE 'CURSOR LEFT EDITING' SUBROUTINE
;
; The address of this routine is derived from an offset found in the editing keys table.
;
; HL Address of the cursor (K-CUR)
ED_LEFT:
  CALL ED_EDGE            ; The cursor is moved.
  JR ED_CUR               ; Jump forward.

; THE 'CURSOR RIGHT EDITING' SUBROUTINE
;
; The address of this routine is derived from an offset found in the editing keys table.
;
; HL Address of the cursor (K-CUR)
ED_RIGHT:
  LD A,(HL)               ; The current character is tested and if it is 'carriage return' then return.
  CP $0D                  ;
  RET Z                   ;
  INC HL                  ; Otherwise make the cursor come after the character.
; This entry point is used by the routine at ED_LEFT.
ED_CUR:
  LD (K_CUR),HL           ; Set the system variable K-CUR.
  RET

; THE 'DELETE EDITING' SUBROUTINE
;
; The address of this routine is derived from an offset found in the editing keys table.
;
; HL Address of the cursor (K-CUR)
ED_DELETE:
  CALL ED_EDGE            ; Move the cursor leftwards.
  LD BC,$0001             ; Reclaim the current character.
  JP RECLAIM_2            ;

; THE 'ED-IGNORE' SUBROUTINE
;
; Used by the routine at EDITOR.
ED_IGNORE:
  CALL WAIT_KEY           ; The next two codes from the key-input routine are ignored.
  CALL WAIT_KEY           ;
; This routine continues into ED_ENTER.

; THE 'ENTER EDITING' SUBROUTINE
;
; Used by the routines at ED_DOWN and ED_SYMBOL.
;
; The routine at ED_IGNORE continues here.
;
; The address of this routine is derived from an offset found in the editing keys table.
ED_ENTER:
  POP HL                  ; The addresses of ED_LOOP and ED_ERROR are discarded.
  POP HL                  ;
; This entry point is used by the routine at ED_ERROR.
ED_END:
  POP HL                  ; The old value of ERR-SP is restored.
  LD (ERR_SP),HL          ;
  BIT 7,(IY+$00)          ; Now return if there were no errors (ERR-NR is +FF).
  RET NZ                  ;
  LD SP,HL                ; Otherwise make an indirect jump to the error routine.
  RET                     ;

; THE 'ED-EDGE' SUBROUTINE
;
; Used by the routines at ED_LEFT and ED_DELETE.
;
; The address of the cursor is in the HL register pair and will be decremented unless the cursor is already at the start of the line. Care is taken not to put the cursor between control characters and their parameters.
;
;   HL Address of the cursor (K-CUR)
; O:HL New address of the cursor
ED_EDGE:
  SCF                     ; DE will hold either E-LINE (for editing) or WORKSP (for INPUTing).
  CALL SET_DE             ;
  SBC HL,DE               ; The carry flag will become set if the cursor is already to be at the start of the line.
  ADD HL,DE               ;
  INC HL                  ; Correct for the subtraction.
  POP BC                  ; Drop the return address.
  RET C                   ; Return via ED_LOOP if the carry flag is set.
  PUSH BC                 ; Restore the return address.
  LD B,H                  ; Move the current address of the cursor to BC.
  LD C,L                  ;
; Now enter a loop to check that control characters are not split from their parameters.
ED_EDGE_1:
  LD H,D                  ; HL will point to the character in the line after that addressed by DE.
  LD L,E                  ;
  INC HL                  ;
  LD A,(DE)               ; Fetch a character code.
  AND $F0                 ; Jump forward if the code does not represent INK to TAB.
  CP $10                  ;
  JR NZ,ED_EDGE_2         ;
  INC HL                  ; Allow for one parameter.
  LD A,(DE)               ; Fetch the code anew.
  SUB $17                 ; Carry is reset for TAB.
  ADC A,$00               ; Note: this splits off AT and TAB but AT and TAB in this form are not implemented anyway so it makes no difference.
  JR NZ,ED_EDGE_2         ; Jump forward unless dealing with AT and TAB which would have two parameters, if used.
  INC HL                  ;
ED_EDGE_2:
  AND A                   ; Prepare for true subtraction.
  SBC HL,BC               ; The carry flag will be reset when the 'updated pointer' reaches K-CUR.
  ADD HL,BC               ;
  EX DE,HL                ; For the next loop use the 'updated pointer', but if exiting use the 'present pointer' for K-CUR. Note: it is the control character that is deleted when using DELETE.
  JR C,ED_EDGE_1          ;
  RET                     ;

; THE 'CURSOR UP EDITING' SUBROUTINE
;
; The address of this routine is derived from an offset found in the editing keys table.
ED_UP:
  BIT 5,(IY+$37)          ; Return if in 'INPUT mode' (bit 5 of FLAGX set).
  RET NZ                  ;
  LD HL,(E_PPC)           ; Fetch the current line number (E-PPC) and its start address.
  CALL LINE_ADDR          ;
  EX DE,HL                ; HL now points to the previous line.
  CALL LINE_NO            ; This line's number is fetched.
  LD HL,$5C4A             ; This is E-PPC-hi.
  CALL LN_STORE           ; The line number is stored.
; This entry point is used by the routine at ED_DOWN.
ED_LIST:
  CALL AUTO_LIST          ; A new automatic listing is now produced and channel 'K' re-opened before returning to ED_LOOP.
  LD A,$00                ;
  JP CHAN_OPEN            ;

; THE 'ED-SYMBOL' SUBROUTINE
;
; The address of this routine is derived from an offset found in the editing keys table.
;
; A Code of the last key pressed
;
; If SYMBOL and GRAPHICS codes were used they would be handled as follows:
ED_SYMBOL:
  BIT 7,(IY+$37)          ; Jump back unless dealing with INPUT LINE (bit 7 of FLAGX set).
  JR Z,ED_ENTER           ;
; The address of this entry point is derived from an offset found in the editing keys table.
ED_GRAPH:
  JP ADD_CHAR             ; Jump back.

; THE 'ED-ERROR' SUBROUTINE
;
; Used by the routine at EDITOR.
;
; Come here when there has been some kind of error.
ED_ERROR:
  BIT 4,(IY+$30)          ; Jump back if using other than channel 'K' (bit 4 of FLAGS2 reset).
  JR Z,ED_END             ;
  LD (IY+$00),$FF         ; Cancel the error number (ERR-NR) and give a 'rasp' (see RASP) before going around the editor again.
  LD D,$00                ;
  LD E,(IY-$02)           ;
  LD HL,P_FOR             ;
  CALL BEEPER             ;
  JP ED_AGAIN             ;

; THE 'CLEAR-SP' SUBROUTINE
;
; Used by the routines at ED_EDIT and MAIN_EXEC.
;
; The editing area or the work space is cleared as directed.
CLEAR_SP:
  PUSH HL                 ; Save the pointer to the space.
  CALL SET_HL             ; DE will point to the first character and HL the last.
  DEC HL                  ;
  CALL RECLAIM_1          ; The correct amount is now reclaimed.
  LD (K_CUR),HL           ; The system variables K-CUR and MODE ('K mode') are initialised before fetching the pointer and returning.
  LD (IY+$07),$00         ;
  POP HL                  ;
  RET                     ;

; THE 'KEYBOARD INPUT' SUBROUTINE
;
; The address of this routine is found in the initial channel information table.
;
; This important subroutine returns the code of the last key to have been pressed, but note that CAPS LOCK, the changing of the mode and the colour control parameters are handled within the subroutine.
;
; O:A Code of the last key pressed
; O:F Carry flag set if a key was pressed
KEY_INPUT:
  BIT 3,(IY+$02)          ; Copy the edit-line or the INPUT-line to the screen if the mode has changed (bit 3 of TV-FLAG set).
  CALL NZ,ED_COPY         ;
  AND A                   ; Return with both carry and zero flags reset if no new key has been pressed (bit 5 of FLAGS reset).
  BIT 5,(IY+$01)          ;
  RET Z                   ;
  LD A,(LAST_K)           ; Otherwise fetch the code (LAST-K) and signal that it has been taken (reset bit 5 of FLAGS).
  RES 5,(IY+$01)          ;
  PUSH AF                 ; Save the code temporarily.
  BIT 5,(IY+$02)          ; Clear the lower part of the display if necessary (bit 5 of TV-FLAG set), e.g. after 'scroll?'.
  CALL NZ,CLS_LOWER       ;
  POP AF                  ; Fetch the code.
  CP " "                  ; Accept all characters and token codes.
  JR NC,KEY_DONE_2        ;
  CP $10                  ; Jump forward with most of the control character codes.
  JR NC,KEY_CONTR         ;
  CP $06                  ; Jump forward with the 'mode' codes and the CAPS LOCK code.
  JR NC,KEY_M_CL          ;
; Now deal with the FLASH, BRIGHT and INVERSE codes.
  LD B,A                  ; Save the code.
  AND $01                 ; Keep only bit 0.
  LD C,A                  ; C holds +00 (=OFF) or +01 (=ON).
  LD A,B                  ; Fetch the code.
  RRA                     ; Rotate it once (losing bit 0).
  ADD A,$12               ; Increase it by +12 giving +12 for FLASH, +13 for BRIGHT, and +14 for INVERSE.
  JR KEY_DATA             ;
; The CAPS LOCK code and the mode codes are dealt with 'locally'.
KEY_M_CL:
  JR NZ,KEY_MODE          ; Jump forward with 'mode' codes.
  LD HL,FLAGS2            ; This is FLAGS2.
  LD A,$08                ; Flip bit 3 of FLAGS2. This is the CAPS LOCK flag.
  XOR (HL)                ;
  LD (HL),A               ;
  JR KEY_FLAG             ; Jump forward.
KEY_MODE:
  CP $0E                  ; Check the lower limit.
  RET C                   ;
  SUB $0D                 ; Reduce the range.
  LD HL,MODE              ; This is MODE.
  CP (HL)                 ; Has it been changed?
  LD (HL),A               ; Enter the new 'mode' code.
  JR NZ,KEY_FLAG          ; Jump if it has changed; otherwise make it 'L mode'.
  LD (HL),$00             ;
KEY_FLAG:
  SET 3,(IY+$02)          ; Signal 'the mode might have changed' (set bit 3 of TV-FLAG).
  CP A                    ; Reset the carry flag and return.
  RET                     ;
; The control key codes (apart from FLASH, BRIGHT and INVERSE) are manipulated.
KEY_CONTR:
  LD B,A                  ; Save the code.
  AND $07                 ; Make the C register hold the parameter (+00 to +07).
  LD C,A                  ;
  LD A,$10                ; A now holds the INK code.
  BIT 3,B                 ; But if the code was an 'unshifted' code then make A hold the PAPER code.
  JR NZ,KEY_DATA          ;
  INC A                   ;
; The parameter is saved in K-DATA and the channel address changed from KEY_INPUT to KEY_NEXT.
KEY_DATA:
  LD (IY-$2D),C           ; Save the parameter at K-DATA.
  LD DE,KEY_NEXT          ; This is KEY_NEXT.
  JR KEY_CHAN             ; Jump forward.
; Note: on the first pass entering at KEY_INPUT the A register is returned holding a 'control code' and then on the next pass, entering at KEY_NEXT, it is the parameter that is returned.
KEY_NEXT:
  LD A,(K_DATA)           ; Fetch the parameter (K-DATA).
  LD DE,KEY_INPUT         ; This is KEY_INPUT.
; Now set the input address in the first channel area.
KEY_CHAN:
  LD HL,(CHANS)           ; Fetch the channel address (CHANS).
  INC HL                  ;
  INC HL                  ;
  LD (HL),E               ; Now set the input address.
  INC HL                  ;
  LD (HL),D               ;
; Finally exit with the required code in the A register.
KEY_DONE_2:
  SCF                     ; Show a code has been found and return.
  RET                     ;

; THE 'LOWER SCREEN COPYING' SUBROUTINE
;
; Used by the routines at KEY_INPUT and INPUT.
;
; This subroutine is called whenever the line in the editing area or the INPUT area is to be printed in the lower part of the screen.
ED_COPY:
  CALL TEMPS              ; Use the permanent colours.
  RES 3,(IY+$02)          ; Signal that the 'mode is to be considered unchanged' (reset bit 3 of TV-FLAG) and the 'lower screen does not need clearing' (reset bit 5).
  RES 5,(IY+$02)          ;
  LD HL,(S_POSNL)         ; Save the current value of S-POSNL.
  PUSH HL                 ;
  LD HL,(ERR_SP)          ; Keep the current value of ERR-SP.
  PUSH HL                 ;
  LD HL,ED_FULL           ; This is ED_FULL.
  PUSH HL                 ; Push this address on to the machine stack to make ED_FULL the entry point following an error (see ERR-SP).
  LD (ERR_SP),SP          ;
  LD HL,(ECHO_E)          ; Push the value of ECHO-E on to the stack.
  PUSH HL                 ;
  SCF                     ; Make HL point to the start of the space and DE the end.
  CALL SET_DE             ;
  EX DE,HL                ;
  CALL OUT_LINE2          ; Now print the line.
  EX DE,HL                ; Exchange the pointers and print the cursor.
  CALL OUT_CURS           ;
  LD HL,(S_POSNL)         ; Next fetch the current value of S-POSNL and exchange it with ECHO-E.
  EX (SP),HL              ;
  EX DE,HL                ; Pass ECHO-E to DE.
  CALL TEMPS              ; Again fetch the permanent colours.
; The remainder of any line that has been started is now completed with spaces printed with the 'permanent' PAPER colour.
ED_BLANK:
  LD A,($5C8B)            ; Fetch the current line number from S-POSNL and subtract the old line number.
  SUB D                   ;
  JR C,ED_C_DONE          ; Jump forward if no 'blanking' of lines required.
  JR NZ,ED_SPACES         ; Jump forward if not on the same line.
  LD A,E                  ; Fetch the old column number and subtract the new column number (at S-POSNL).
  SUB (IY+$50)            ;
  JR NC,ED_C_DONE         ; Jump if no spaces required.
ED_SPACES:
  LD A," "                ; A 'space'.
  PUSH DE                 ; Save the old values.
  CALL PRINT_OUT          ; Print it.
  POP DE                  ; Fetch the old values.
  JR ED_BLANK             ; Back again.
; New deal with any errors.
ED_FULL:
  LD D,$00                ; Give out a 'rasp' (see RASP).
  LD E,(IY-$02)           ;
  LD HL,P_FOR             ;
  CALL BEEPER             ;
  LD (IY+$00),$FF         ; Cancel the error number (ERR-NR).
  LD DE,(S_POSNL)         ; Fetch the current value of S-POSNL and jump forward.
  JR ED_C_END             ;
; The normal exit upon completion of the copying over of the edit or the INPUT line.
ED_C_DONE:
  POP DE                  ; The new position value.
  POP HL                  ; The 'error address'.
; But come here after an error.
ED_C_END:
  POP HL                  ; The old value of ERR-SP is restored.
  LD (ERR_SP),HL          ;
  POP BC                  ; Fetch the old value of S-POSNL.
  PUSH DE                 ; Save the new position values.
  CALL CL_SET             ; Set the system variables.
  POP HL                  ; The old value of S-POSNL goes into ECHO-E.
  LD (ECHO_E),HL          ;
  LD (IY+$26),$00         ; X-PTR is cleared in a suitable manner and the return made.
  RET                     ;

; THE 'SET-HL' AND 'SET-DE' SUBROUTINES
;
; Used by the routine at CLEAR_SP.
;
; These subroutines return with DE pointing to the first location and HL to the last location of either the editing area or the work space.
;
; O:DE Address of the first byte of the editing area or work space
; O:HL Address of the last byte of the editing area or work space
SET_HL:
  LD HL,(WORKSP)          ; Point to the last location of the editing area (WORKSP-1).
  DEC HL                  ;
  AND A                   ; Clear the carry flag.
; This entry point is used by the routines at ED_EDGE and ED_COPY with the carry flag set.
SET_DE:
  LD DE,(E_LINE)          ; Point to the start of the editing area (E-LINE) and return if in 'editing mode' (bit 5 of FLAGX reset).
  BIT 5,(IY+$37)          ;
  RET Z                   ;
  LD DE,(WORKSP)          ; Otherwise point DE at the start of the work space (WORKSP).
  RET C                   ; Return if now intended.
  LD HL,(STKBOT)          ; Fetch STKBOT and then return.
  RET                     ;

; THE 'REMOVE-FP' SUBROUTINE
;
; Used by the routines at MAIN_EXEC and INPUT.
;
; This subroutine removes the hidden floating-point forms in a BASIC line.
;
; HL E-LINE or WORKSP
REMOVE_FP:
  LD A,(HL)               ; Each character in turn is examined.
  CP $0E                  ; Is it a number marker?
  LD BC,$0006             ; It will occupy six locations.
  CALL Z,RECLAIM_2        ; Reclaim the floating point number.
  LD A,(HL)               ; Fetch the code again.
  INC HL                  ; Update the pointer.
  CP $0D                  ; Is it a carriage return?
  JR NZ,REMOVE_FP         ; Back if not. But make a simple return if it is.
  RET                     ;

; THE 'NEW' COMMAND ROUTINE
;
; The address of this routine is found in the parameter table.
NEW:
  DI                      ; Disable the maskable interrupt.
  LD A,$FF                ; The NEW flag.
  LD DE,(RAMTOP)          ; The existing value of RAMTOP is preserved.
  EXX                     ; Load the alternate registers with the following system variables (P-RAMT, RASP, PIP, UDG). All of which will also be preserved.
  LD BC,(P_RAMT)          ;
  LD DE,(RASP)            ;
  LD HL,(UDG)             ;
  EXX                     ;
; This entry point is used by the routine at START.
;
; The main entry point.
START_NEW:
  LD B,A                  ; Save the flag for later.
  LD A,$07                ; Make the border white in colour.
  OUT ($FE),A             ;
  LD A,$3F                ; Set the I register to hold the value of +3F.
  LD I,A                  ;
  NOP                     ; Wait 24 T states.
  NOP                     ;
  NOP                     ;
  NOP                     ;
  NOP                     ;
  NOP                     ;
; Now the memory is checked.
  LD H,D                  ; Transfer the value in DE (START=+FFFF, NEW=RAMTOP).
  LD L,E                  ;
RAM_FILL:
  LD (HL),$02             ; Enter the value of +02 into every location above +3FFF.
  DEC HL                  ;
  CP H                    ;
  JR NZ,RAM_FILL          ;
RAM_READ:
  AND A                   ; Prepare for true subtraction.
  SBC HL,DE               ; The carry flag will become reset when the top is reached.
  ADD HL,DE               ;
  INC HL                  ; Update the pointer.
  JR NC,RAM_DONE          ; Jump when at top.
  DEC (HL)                ; +02 goes to +01.
  JR Z,RAM_DONE           ; But if zero then RAM is faulty. Use current HL as top.
  DEC (HL)                ; +01 goes to +00.
  JR Z,RAM_READ           ; Step to the next test unless it fails.
RAM_DONE:
  DEC HL                  ; HL points to the last actual location in working order.
; Next restore the 'preserved' system variables. (Meaningless when coming from START.)
  EXX                     ; Restore P-RAMT, RASP, PIP and UDG.
  LD (P_RAMT),BC          ;
  LD (RASP),DE            ;
  LD (UDG),HL             ;
  EXX                     ;
  INC B                   ; Test the START/NEW flag.
  JR Z,RAM_SET            ; Jump forward if coming from the NEW command routine.
; Overwrite the system variables when coming from START and initialise the user-defined graphics area.
  LD (P_RAMT),HL          ; Top of physical RAM (P-RAMT).
  LD DE,$3EAF             ; Last byte of 'U' in character set.
  LD BC,$00A8             ; There are this number of bytes in twenty one letters.
  EX DE,HL                ; Switch the pointers.
  LDDR                    ; Now copy the character forms of the letters 'A' to 'U'.
  EX DE,HL                ; Switch the pointers back.
  INC HL                  ; Point to the first byte.
  LD (UDG),HL             ; Now set UDG.
  DEC HL                  ; Down one location.
  LD BC,$0040             ; Set the system variables RASP and PIP.
  LD (RASP),BC            ;
; The remainder of the routine is common to both the START and the NEW operations.
RAM_SET:
  LD (RAMTOP),HL          ; Set RAMTOP.
  LD HL,$3C00             ; Initialise the system variable CHARS.
  LD (CHARS),HL           ;
; Next the machine stack is set up.
  LD HL,(RAMTOP)          ; The top location (RAMTOP) is made to hold +3E.
  LD (HL),$3E             ;
  DEC HL                  ; The next location is left holding zero.
  LD SP,HL                ; These two locations represent the 'last entry'.
  DEC HL                  ; Step down two locations to find the correct value for ERR-SP.
  DEC HL                  ;
  LD (ERR_SP),HL          ;
; The initialisation routine continues with:
  IM 1                    ; Interrupt mode 1 is used.
  LD IY,ERR_NR            ; IY holds +ERR-NR always.
  EI                      ; The maskable interrupt can now be enabled. The real-time clock will be updated and the keyboard scanned every 1/50th of a second.
  LD HL,CHINFO            ; The system variable CHANS is set to the base address of the channel information area.
  LD (CHANS),HL           ;
  LD DE,CHANINFO          ; The initial channel data is moved from the table (CHANINFO) to the channel information area.
  LD BC,$0015             ;
  EX DE,HL                ;
  LDIR                    ;
  EX DE,HL                ; The system variable DATADD is made to point to the last location of the channel data.
  DEC HL                  ;
  LD (DATADD),HL          ;
  INC HL                  ; And PROG and VARS to the the location after that.
  LD (PROG),HL            ;
  LD (VARS),HL            ;
  LD (HL),$80             ; The end-marker of the variables area.
  INC HL                  ; Move on one location to find the value for E-LINE.
  LD (E_LINE),HL          ;
  LD (HL),$0D             ; Make the edit-line be a single 'carriage return' character.
  INC HL                  ;
  LD (HL),$80             ; Now enter an end marker.
  INC HL                  ; Move on one location to find the value for WORKSP, STKBOT and STKEND.
  LD (WORKSP),HL          ;
  LD (STKBOT),HL          ;
  LD (STKEND),HL          ;
  LD A,$38                ; Initialise the colour system variables (ATTR-P, ATTR-T, BORDCR) to FLASH 0, BRIGHT 0, PAPER 7, INK 0, BORDER 7.
  LD (ATTR_P),A           ;
  LD (ATTR_T),A           ;
  LD (BORDCR),A           ;
  LD HL,$0523             ; Initialise the system variables REPDEL and REPPER.
  LD (REPDEL),HL          ;
  DEC (IY-$3A)            ; Make KSTATE0 hold +FF.
  DEC (IY-$36)            ; Make KSTATE4 hold +FF.
  LD HL,STRMDATA          ; Next move the initial stream data from its table to the streams area.
  LD DE,STRMS             ;
  LD BC,$000E             ;
  LDIR                    ;
  SET 1,(IY+$01)          ; Signal 'printer in use' (set bit 1 of FLAGS) and clear the printer buffer.
  CALL CLEAR_PRB          ;
  LD (IY+$31),$02         ; Set the size of the lower part of the display (DF-SZ) and clear the whole display.
  CALL CLS                ;
  XOR A                   ; Now print the message '© 1982 Sinclair Research Ltd' on the bottom line.
  LD DE,$1538             ;
  CALL PO_MSG             ;
  SET 5,(IY+$02)          ; Signal 'the lower part will required to be cleared' (set bit 5 of TV-FLAG).
  JR MAIN_1               ; Jump forward into the main execution loop.

; THE 'MAIN EXECUTION' LOOP
;
; Used by the routine at MAIN_ADD.
;
; The main loop controls the 'editing mode', the execution of direct commands and the production of reports.
MAIN_EXEC:
  LD (IY+$31),$02         ; The lower part of the screen is to be two lines in size (see DF-SZ).
  CALL AUTO_LIST          ; Produce an automatic listing.
; This entry point is used by the routine at NEW.
MAIN_1:
  CALL SET_MIN            ; All the areas from E-LINE onwards are given their minimum configurations.
MAIN_2:
  LD A,$00                ; Channel 'K' is opened before calling the EDITOR.
  CALL CHAN_OPEN          ;
  CALL EDITOR             ; The EDITOR is called to allow the user to build up a BASIC line.
  CALL LINE_SCAN          ; The current line is scanned for correct syntax.
  BIT 7,(IY+$00)          ; Jump forward if the syntax is correct (ERR-NR is +FF).
  JR NZ,MAIN_3            ;
  BIT 4,(IY+$30)          ; Jump forward if other than channel 'K' is being used (bit 4 of FLAGS2 is set).
  JR Z,MAIN_4             ;
  LD HL,(E_LINE)          ; Point to the start of the line with the error (E-LINE).
  CALL REMOVE_FP          ; Remove the floating-point forms from this line.
  LD (IY+$00),$FF         ; Reset ERR-NR and jump back to MAIN_2 leaving the listing unchanged.
  JR MAIN_2               ;
; The 'edit-line' has passed syntax and the three types of line that are possible have to be distinguished from each other.
MAIN_3:
  LD HL,(E_LINE)          ; Point to the start of the line (E-LINE).
  LD (CH_ADD),HL          ; Set CH-ADD to the start also.
  CALL E_LINE_NO          ; Fetch any line number into BC.
  LD A,B                  ; Is the line number a valid one?
  OR C                    ;
  JP NZ,MAIN_ADD          ; Jump if it is so, and add the new line to the existing program.
  RST $18                 ; Fetch the first character of the line and see if the line is 'carriage return only'.
  CP $0D                  ;
  JR Z,MAIN_EXEC          ; If it is then jump back.
; The 'edit-line' must start with a direct BASIC command so this line becomes the first line to be interpreted.
  BIT 0,(IY+$30)          ; Clear the whole display unless the flag (bit 0 of FLAGS2) says it is unnecessary.
  CALL NZ,CL_ALL          ;
  CALL CLS_LOWER          ; Clear the lower part anyway.
  LD A,$19                ; Set the appropriate value for the scroll counter (SCR-CT) by subtracting the second byte of S-POSN from +19).
  SUB (IY+$4F)            ;
  LD (SCR_CT),A           ;
  SET 7,(IY+$01)          ; Signal 'line execution' (set bit 7 of FLAGS).
  LD (IY+$00),$FF         ; Ensure ERR-NR is correct.
  LD (IY+$0A),$01         ; Deal with the first statement in the line (set NSPPC to +01).
  CALL LINE_RUN           ; Now the line is interpreted. Note: the address MAIN_4 goes on to the machine stack and is addressed by ERR-SP.
; After the line has been interpreted and all the actions consequential to it have been completed a return is made to MAIN_4, so that a report can be made.
MAIN_4:
  HALT                    ; The maskable interrupt must be enabled.
  RES 5,(IY+$01)          ; Signal 'ready for a new key' (reset bit 5 of FLAGS).
  BIT 1,(IY+$30)          ; Empty the printer buffer if it has been used (bit 1 of FLAGS2 set).
  CALL NZ,COPY_BUFF       ;
  LD A,(ERR_NR)           ; Fetch the error number (ERR-NR) and increment it.
  INC A                   ;
; This entry point is used by the routine at REPORT_G.
MAIN_G:
  PUSH AF                 ; Save the new value.
  LD HL,$0000             ; The system variables FLAGX, X-PTR-hi and DEFADD are all set to zero.
  LD (IY+$37),H           ;
  LD (IY+$26),H           ;
  LD (DEFADD),HL          ;
  LD HL,$0001             ; Ensure that stream +00 points to channel 'K' (see STRMS).
  LD ($5C16),HL           ;
  CALL SET_MIN            ; Clear all the work areas and the calculator stack.
  RES 5,(IY+$37)          ; Signal 'editing mode' (reset bit 5 of FLAGX).
  CALL CLS_LOWER          ; Clear the lower screen.
  SET 5,(IY+$02)          ; Signal 'the lower screen will require clearing' (set bit 5 of TV-FLAG).
  POP AF                  ; Fetch the report value.
  LD B,A                  ; Make a copy in B.
  CP $0A                  ; Jump forward with report numbers '0 to 9'.
  JR C,MAIN_5             ;
  ADD A,$07               ; Add the ASCII letter offset value.
MAIN_5:
  CALL OUT_CODE           ; Print the report code and follow it with a 'space'.
  LD A," "                ;
  RST $10                 ;
  LD A,B                  ; Fetch the report value (used to identify the required report message).
  LD DE,REPORTS           ; Print the message.
  CALL PO_MSG             ;
  XOR A                   ; Follow it by a 'comma' and a 'space'.
  LD DE,$1536             ;
  CALL PO_MSG             ;
  LD BC,(PPC)             ; Now fetch the current line number (PPC) and print it as well.
  CALL OUT_NUM_1          ;
  LD A,":"                ; Follow it by a ':'.
  RST $10                 ;
  LD C,(IY+$0D)           ; Fetch the current statement number (SUBPPC) into the BC register pair and print it.
  LD B,$00                ;
  CALL OUT_NUM_1          ;
  CALL CLEAR_SP           ; Clear the editing area.
  LD A,(ERR_NR)           ; Fetch the error number (ERR-NR) again.
  INC A                   ; Increment it as usual.
  JR Z,MAIN_9             ; If the program was completed successfully there cannot be any 'CONTinuing' so jump.
  CP $09                  ; If the program halted with 'STOP statement' or 'BREAK into program' CONTinuing will be from the next statement; otherwise SUBPPC is unchanged.
  JR Z,MAIN_6             ;
  CP $15                  ;
  JR NZ,MAIN_7            ;
MAIN_6:
  INC (IY+$0D)            ;
MAIN_7:
  LD BC,$0003             ; The system variables OLDPPC and OSPCC have now to be made to hold the CONTinuing line and statement numbers.
  LD DE,OSPCC             ;
  LD HL,NSPPC             ; The values used will be those in PPC and SUBPPC unless NSPPC indicates that the 'break' occurred before a 'jump' (i.e. after a GO TO statement etc.).
  BIT 7,(HL)              ;
  JR Z,MAIN_8             ;
  ADD HL,BC               ;
MAIN_8:
  LDDR                    ;
MAIN_9:
  LD (IY+$0A),$FF         ; NSPPC is reset to indicate 'no jump'.
  RES 3,(IY+$01)          ; 'K mode' is selected (reset bit 3 of FLAGS).
  JP MAIN_2               ; And finally the jump back is made but no program listing will appear until requested.

; THE REPORT MESSAGES
;
; Used by the routine at MAIN_EXEC.
;
; Each message is given with the last character inverted (plus +80).
REPORTS:
  DEFB $80                ; Initial byte is stepped over.
  DEFM "O","K"+$80        ; 0 OK
  DEFM "NEXT without FO","R"+$80 ; 1 NEXT without FOR
  DEFM "Variable not foun","d"+$80 ; 2 Variable not found
  DEFM "Subscript wron","g"+$80 ; 3 Subscript wrong
  DEFM "Out of memor","y"+$80 ; 4 Out of memory
  DEFM "Out of scree","n"+$80 ; 5 Out of screen
  DEFM "Number too bi","g"+$80 ; 6 Number too big
  DEFM "RETURN without GOSU","B"+$80 ; 7 RETURN without GOSUB
  DEFM "End of fil","e"+$80 ; 8 End of file
  DEFM "STOP statemen","t"+$80 ; 9 STOP statement
  DEFM "Invalid argumen","t"+$80 ; A Invalid argument
  DEFM "Integer out of rang","e"+$80 ; B Integer out of range
  DEFM "Nonsense in BASI","C"+$80 ; C Nonsense in BASIC
  DEFM "BREAK - CONT repeat","s"+$80 ; D BREAK - CONT repeats
  DEFM "Out of DAT","A"+$80 ; E Out of DATA
  DEFM "Invalid file nam","e"+$80 ; F Invalid file name
  DEFM "No room for lin","e"+$80 ; G No room for line
  DEFM "STOP in INPU","T"+$80 ; H STOP in INPUT
  DEFM "FOR without NEX","T"+$80 ; I FOR without NEXT
  DEFM "Invalid I/O devic","e"+$80 ; J Invalid I/O device
  DEFM "Invalid colou","r"+$80 ; K Invalid colour
  DEFM "BREAK into progra","m"+$80 ; L BREAK into program
  DEFM "RAMTOP no goo","d"+$80 ; M RAMTOP no good
  DEFM "Statement los","t"+$80 ; N Statement lost
  DEFM "Invalid strea","m"+$80 ; O Invalid stream
  DEFM "FN without DE","F"+$80 ; P FN without DEF
  DEFM "Parameter erro","r"+$80 ; Q Parameter error
  DEFM "Tape loading erro","r"+$80 ; R Tape loading error
COMMA_SPC:
  DEFM ","," "+$80        ; ', '

; THE COPYRIGHT MESSAGE
;
; Used by the routine at NEW.
COPYRIGHT:
  DEFM $7F," 1982 Sinclair Research Lt","d"+$80 ; © 1982 Sinclair Research Ltd

; Report G - No room for line
;
; Used by the routine at MAIN_ADD.
REPORT_G:
  LD A,$10                ; 'G' has the code +10 plus +37.
  LD BC,$0000             ; Clear BC.
  JP MAIN_G               ; Jump back to give the report.

; THE 'MAIN-ADD' SUBROUTINE
;
; Used by the routine at MAIN_EXEC.
;
; This subroutine allows for a new BASIC line to be added to the existing BASIC program in the program area. If a line has both an old and a new version then the old one is 'reclaimed'. A new line that consists of only a line number does not go into the program area.
;
; BC BASIC line number
MAIN_ADD:
  LD (E_PPC),BC           ; Make the new line number the 'current line' (E-PPC).
  LD HL,(CH_ADD)          ; Fetch CH-ADD and save the address in DE.
  EX DE,HL                ;
  LD HL,REPORT_G          ; Push the address of REPORT_G on to the machine stack. ERR-SP will now point to REPORT_G.
  PUSH HL                 ;
  LD HL,(WORKSP)          ; Fetch WORKSP.
  SCF                     ; Find the length of the line from after the line number to the 'carriage return' character inclusively.
  SBC HL,DE               ;
  PUSH HL                 ; Save the length.
  LD H,B                  ; Move the line number to the HL register pair.
  LD L,C                  ;
  CALL LINE_ADDR          ; Is there an existing line with this number?
  JR NZ,MAIN_ADD1         ; Jump if there was not.
  CALL NEXT_ONE           ; Find the length of the 'old' line and reclaim it.
  CALL RECLAIM_2          ;
MAIN_ADD1:
  POP BC                  ; Fetch the length of the 'new' line and jump forward if it is only a 'line number and a carriage return'.
  LD A,C                  ;
  DEC A                   ;
  OR B                    ;
  JR Z,MAIN_ADD2          ;
  PUSH BC                 ; Save the length.
  INC BC                  ; Four extra locations will be needed, i.e. two for the number and two for the length.
  INC BC                  ;
  INC BC                  ;
  INC BC                  ;
  DEC HL                  ; Make HL point to the location before the 'destination'.
  LD DE,(PROG)            ; Save the current value of PROG to avoid corruption when adding a first line.
  PUSH DE                 ;
  CALL MAKE_ROOM          ; Space for the new line is created.
  POP HL                  ; The old value of PROG is fetched and restored.
  LD (PROG),HL            ;
  POP BC                  ; A copy of the line length (without parameters) is taken.
  PUSH BC                 ;
  INC DE                  ; Make DE point to the end location of the new area and HL to the 'carriage return' character of the new line in the editing area (WORKSP-2).
  LD HL,(WORKSP)          ;
  DEC HL                  ;
  DEC HL                  ;
  LDDR                    ; Now copy over the line.
  LD HL,(E_PPC)           ; Fetch the line's number (E_PPC).
  EX DE,HL                ; Destination into HL and number into DE.
  POP BC                  ; Fetch the new line's length.
  LD (HL),B               ; The high length byte.
  DEC HL                  ;
  LD (HL),C               ; The low length byte.
  DEC HL                  ;
  LD (HL),E               ; The low line number byte.
  DEC HL                  ;
  LD (HL),D               ; The high line number byte.
MAIN_ADD2:
  POP AF                  ; Drop the address of REPORT_G.
  JP MAIN_EXEC            ; Jump back and this time do produce an automatic listing.

; THE 'INITIAL CHANNEL INFORMATION'
;
; Used by the routine at NEW, which copies the information from here to CHINFO.
;
; Initially there are four channels - 'K', 'S', 'R', and 'P' - for communicating with the 'keyboard', 'screen', 'work space' and 'printer'. For each channel the output routine address comes before the input routine address and the channel's code.
CHANINFO:
  DEFW PRINT_OUT          ; Keyboard.
  DEFW KEY_INPUT          ;
  DEFB "K"                ;
  DEFW PRINT_OUT          ; Screen.
  DEFW REPORT_J           ;
  DEFB "S"                ;
  DEFW ADD_CHAR           ; Work space.
  DEFW REPORT_J           ;
  DEFB "R"                ;
  DEFW PRINT_OUT          ; Printer.
  DEFW REPORT_J           ;
  DEFB "P"                ;
  DEFB $80                ; End marker.

; Report J - Invalid I/O device
;
; The address of this routine is found in the initial channel information table.
REPORT_J:
  RST $08                 ; Call the error handling routine.
  DEFB $12                ;

; THE 'INITIAL STREAM DATA'
;
; Used by the routines at NEW and CLOSE.
;
; Initially there are seven streams - +FD to +03.
STRMDATA:
  DEFW $0001              ; +FD: Leads to channel 'K' (keyboard)
  DEFW $0006              ; +FE: Leads to channel 'S' (screen)
  DEFW $000B              ; +FF: Leads to channel 'R' (work space)
  DEFW $0001              ; +00: Leads to channel 'K' (keyboard)
  DEFW $0001              ; +01: Leads to channel 'K' (keyboard)
  DEFW $0006              ; +02: Leads to channel 'S' (screen)
  DEFW $0010              ; +03: Leads to channel 'P' (printer)

; THE 'WAIT-KEY' SUBROUTINE
;
; Used by the routines at SA_CONTRL, PO_SCR, EDITOR and ED_IGNORE.
;
; This subroutine is the controlling subroutine for calling the current input subroutine.
WAIT_KEY:
  BIT 5,(IY+$02)          ; Jump forward if the flag (bit 5 of TV-FLAG) indicates the lower screen does not require clearing.
  JR NZ,WAIT_KEY1         ;
  SET 3,(IY+$02)          ; Otherwise signal 'consider the mode as having changed' (set bit 3 of TV-FLAG).
WAIT_KEY1:
  CALL INPUT_AD           ; Call the input subroutine indirectly via INPUT_AD.
  RET C                   ; Return with acceptable codes.
  JR Z,WAIT_KEY1          ; Both the carry flag and the zero flag are reset if 'no key is being pressed'; otherwise signal an error.
; Report 8 - End of file.
  RST $08                 ; Call the error handling routine.
  DEFB $07                ;

; THE 'INPUT-AD' SUBROUTINE
;
; Used by the routines at WAIT_KEY and read_in.
;
; The registers are saved and HL made to point to the input address.
INPUT_AD:
  EXX                     ; Save the registers.
  PUSH HL                 ;
  LD HL,(CURCHL)          ; Fetch the base address for the current channel information (CURCHL).
  INC HL                  ; Step past the output address.
  INC HL                  ;
  JR CALL_SUB             ; Jump forward.

; THE 'MAIN PRINTING' SUBROUTINE
;
; Used by the routines at MAIN_EXEC, OUT_SP_2, OUT_NUM_1 and PRINT_FP.
;
; A Value from +00 to +09 (for digits) or +11 to +22 (for the letters A-R)
OUT_CODE:
  LD E,$30                ; Increase the value in the A register by +30.
  ADD A,E                 ;
; This entry point is used by the routine at PRINT_A_1 with A holding the code of the character to be printed.
PRINT_A_2:
  EXX                     ; Save the registers.
  PUSH HL                 ;
  LD HL,(CURCHL)          ; Fetch the base address for the current channel (CURCHL). This will point to an output address.
; This entry point is used by the routine at INPUT_AD with A holding the code of the character to be printed.
;
; Now call the actual subroutine. HL points to the output or the input address as directed.
CALL_SUB:
  LD E,(HL)               ; Fetch the low byte.
  INC HL                  ; Fetch the high byte.
  LD D,(HL)               ;
  EX DE,HL                ; Move the address to the HL register pair.
  CALL CALL_JUMP          ; Call the actual subroutine.
  POP HL                  ; Restore the registers.
  EXX                     ;
  RET                     ; Return will be from here unless an error occurred.

; THE 'CHAN-OPEN' SUBROUTINE
;
; Used by the routines at SAVE_ETC, SA_CONTRL, PO_SCR, CLS, CL_ALL, ED_EDIT, ED_UP, MAIN_EXEC, LIST, LPRINT, STR_ALTER, INPUT, str and read_in.
;
; This subroutine is called with the A register holding a valid stream number - normally +FD to +03. Then depending on the stream data a particular channel will be made the current channel.
;
; A Stream number
CHAN_OPEN:
  ADD A,A                 ; The value in the A register is doubled and then increased by +16.
  ADD A,$16               ;
  LD L,A                  ; The result is moved to L.
  LD H,$5C                ; The address 5C16 is the base address for stream +00.
  LD E,(HL)               ; Fetch the first two bytes of the required stream's data.
  INC HL                  ;
  LD D,(HL)               ;
  LD A,D                  ; Give an error if both bytes are zero; otherwise jump forward.
  OR E                    ;
  JR NZ,CHAN_OP_1         ;
; This entry point is used by the routine at STR_ALTER.
;
; Report O - Invalid stream.
REPORT_O:
  RST $08                 ; Call the error handling routine.
  DEFB $17                ;
; Using the stream data now find the base address of the channel information associated with that stream.
CHAN_OP_1:
  DEC DE                  ; Reduce the stream data.
  LD HL,(CHANS)           ; The base address of the whole channel information area (CHANS).
  ADD HL,DE               ; Form the required address in this area.
; This routine continues into CHAN_FLAG.

; THE 'CHAN-FLAG' SUBROUTINE
;
; Used by the routines at ED_EDIT, str and read_in.
;
; The routine at CHAN_OPEN continues here.
;
; The appropriate flags for the different channels are set by this subroutine.
;
; HL Base address of the channel (see CHINFO)
CHAN_FLAG:
  LD (CURCHL),HL          ; The HL register pair holds the base address for a particular channel; set CURCHL accordingly.
  RES 4,(IY+$30)          ; Signal 'using other than channel 'K'' (reset bit 4 of FLAGS2).
  INC HL                  ; Step past the output and the input addresses and make HL point to the channel code.
  INC HL                  ;
  INC HL                  ;
  INC HL                  ;
  LD C,(HL)               ; Fetch the code.
  LD HL,CHANCODE          ; The base address of the channel code look-up table.
  CALL INDEXER            ; Index into this table and locate the required offset, but return if there is not a matching channel code.
  RET NC                  ;
  LD D,$00                ; Pass the offset to the DE register pair.
  LD E,(HL)               ;
  ADD HL,DE               ; Point HL at the appropriate flag setting routine.
; This entry point is used by the routine at OUT_CODE.
CALL_JUMP:
  JP (HL)                 ; Jump to the routine.

; THE 'CHANNEL CODE LOOK-UP' TABLE
;
; Used by the routine at CHAN_FLAG.
CHANCODE:
  DEFB "K",$06            ; Channel 'K', offset +06 (CHAN_K).
  DEFB "S",$12            ; Channel 'S', offset +12 (CHAN_S).
  DEFB "P",$1B            ; Channel 'P', offset +1B (CHAN_P).
  DEFB $00                ; End marker.

; THE 'CHANNEL 'K' FLAG' SUBROUTINE
;
; The address of this routine is derived from an offset found in the channel code lookup table.
CHAN_K:
  SET 0,(IY+$02)          ; Signal 'using lower screen' (set bit 0 of TV-FLAG).
  RES 5,(IY+$01)          ; Signal 'ready for a key' (reset bit 5 of FLAGS).
  SET 4,(IY+$30)          ; Signal 'using channel 'K'' (set bit 4 of FLAGS2).
  JR CHAN_S_1             ; Jump forward.

; THE 'CHANNEL 'S' FLAG' SUBROUTINE
;
; The address of this routine is derived from an offset found in the channel code lookup table.
CHAN_S:
  RES 0,(IY+$02)          ; Signal 'using main screen' (reset bit 0 of TV-FLAG).
; This entry point is used by the routine at CHAN_K.
CHAN_S_1:
  RES 1,(IY+$01)          ; Signal 'printer not being used' (reset bit 1 of FLAGS).
  JP TEMPS                ; Exit via TEMPS so as to set the colour system variables.

; THE 'CHANNEL 'P' FLAG' SUBROUTINE
;
; The address of this routine is derived from an offset found in the channel code lookup table.
CHAN_P:
  SET 1,(IY+$01)          ; Signal 'printer in use' (set bit 1 of FLAGS).
  RET

; THE 'MAKE-ROOM' SUBROUTINE
;
; Used by the routine at EDITOR.
;
; This is a very important subroutine. It is called on many occasions to 'open up' an area. In all cases the HL register pair points to the location after the place where 'room' is required and the BC register pair holds the length of the 'room' needed.
;
;   HL Address at which to create the new space
; O:DE Address of the last byte of the new space
; O:HL Address of the byte before the start of the new space
;
; When a single space only is required then the subroutine is entered here.
ONE_SPACE:
  LD BC,$0001             ; Just the single extra location is required.
; This entry point is used by the routines at LD_CONTRL, ME_ENTER, EDITOR, MAIN_ADD, RESERVE, FOR, DEF_FN, S_DECIMAL, LET and DIM with BC holding the size of the space to create.
MAKE_ROOM:
  PUSH HL                 ; Save the pointer.
  CALL TEST_ROOM          ; Make sure that there is sufficient memory available for the task being undertaken.
  POP HL                  ; Restore the pointer.
  CALL POINTERS           ; Alter all the pointers before making the 'room'.
  LD HL,(STKEND)          ; Make HL hold the new STKEND.
  EX DE,HL                ; Switch 'old' and 'new'.
  LDDR                    ; Now make the 'room' and return.
  RET                     ;
; Note: this subroutine returns with the HL register pair pointing to the location before the new 'room' and the DE register pair pointing to the last of the new locations. The new 'room' therefore has the description '(HL)+1' to '(DE)' inclusive.
;
; However as the 'new locations' still retain their 'old values' it is also possible to consider the new 'room' as having been made after the original location '(HL)' and it thereby has the description '(HL)+2' to '(DE)+1'.
;
; In fact the programmer appears to have a preference for the 'second description' and this can be confusing.

; THE 'POINTERS' SUBROUTINE
;
; Used by the routines at ONE_SPACE and RECLAIM_1.
;
; Whenever an area has to be 'made' or 'reclaimed' the system variables that address locations beyond the 'position' of the change have to be amended as required. On entry the BC register pair holds the number of bytes involved and the HL register pair addresses the location before the 'position'.
;
; BC Size of the area being created (positive) or reclaimed (negative)
; HL Base address of the area being created or reclaimed
POINTERS:
  PUSH AF                 ; These registers are saved.
  PUSH HL                 ; Copy the address of the 'position'.
  LD HL,VARS              ; This is VARS, the first of the fourteen system pointers.
  LD A,$0E                ;
; A loop is now entered to consider each pointer in turn. Only those pointers that point beyond the 'position' are changed.
PTR_NEXT:
  LD E,(HL)               ; Fetch the two bytes of the current pointer.
  INC HL                  ;
  LD D,(HL)               ;
  EX (SP),HL              ; Exchange the system variable with the address of the 'position'.
  AND A                   ; The carry flag will become set if the system variable's address is to be updated.
  SBC HL,DE               ;
  ADD HL,DE               ;
  EX (SP),HL              ; Restore the 'position'.
  JR NC,PTR_DONE          ; Jump forward if the pointer is to be left; otherwise change it.
  PUSH DE                 ; Save the old value.
  EX DE,HL                ; Now add the value in BC to the old value.
  ADD HL,BC               ;
  EX DE,HL                ;
  LD (HL),D               ; Enter the new value into the system variable - high byte before low byte.
  DEC HL                  ;
  LD (HL),E               ;
  INC HL                  ; Point again to the high byte.
  POP DE                  ; Fetch the old value.
PTR_DONE:
  INC HL                  ; Point to the next system variable and jump back until all fourteen have been considered.
  DEC A                   ;
  JR NZ,PTR_NEXT          ;
; Now find the size of the block to be moved.
  EX DE,HL                ; Put the old value of STKEND in HL and restore the other registers.
  POP DE                  ;
  POP AF                  ;
  AND A                   ; Now find the difference between the old value of STKEND and the 'position'.
  SBC HL,DE               ;
  LD B,H                  ; Transfer the result to BC and add 1 for the inclusive byte.
  LD C,L                  ;
  INC BC                  ;
  ADD HL,DE               ; Reform the old value of STKEND and pass it to DE before returning.
  EX DE,HL                ;
  RET                     ;

; THE 'COLLECT A LINE NUMBER' SUBROUTINE
;
; On entry the HL register pair points to the location under consideration. If the location holds a value that constitutes a suitable high byte for a line number then the line number is returned in DE. However if this is not so then the location addressed by DE is tried instead; and should this also be unsuccessful line number zero is returned.
;
;   HL Address of the first byte of the BASIC line number to test
;   DE Address of the first byte of the previous BASIC line number
; O:DE The line number, or zero if none was found
LINE_ZERO:
  DEFB $00,$00            ; Line number zero.
LINE_NO_A:
  EX DE,HL                ; Consider the other pointer.
  LD DE,LINE_ZERO         ; Use line number zero.
; The main entry point is here, and is used by the routines at ED_EDIT, ED_UP and LN_FETCH.
LINE_NO:
  LD A,(HL)               ; Fetch the high byte and test it.
  AND $C0                 ;
  JR NZ,LINE_NO_A         ; Jump if not suitable.
  LD D,(HL)               ; Fetch the high byte and low byte and return.
  INC HL                  ;
  LD E,(HL)               ;
  RET                     ;

; THE 'RESERVE' SUBROUTINE
;
; Used by the routine at BC_SPACES.
;
; On entry here the last value on the machine stack is WORKSP and the value above it is the number of spaces that are to be 'reserved'.
;
; This subroutine always makes 'room' between the existing work space and the calculator stack.
;
;   BC Number of spaces to reserve
; O:DE Address of the first byte of the new space
; O:HL Address of the last byte of the new space
RESERVE:
  LD HL,(STKBOT)          ; Fetch the current value of STKBOT and decrement it to get the last location of the work space.
  DEC HL                  ;
  CALL MAKE_ROOM          ; Now make 'BC spaces'.
  INC HL                  ; Point to the first new space and then the second.
  INC HL                  ;
  POP BC                  ; Fetch the old value of WORKSP and restore it.
  LD (WORKSP),BC          ;
  POP BC                  ; Restore BC - number of spaces.
  EX DE,HL                ; Switch the pointers.
  INC HL                  ; Make HL point to the first of the displaced bytes.
  RET                     ; Now return.
; Note: it can also be considered that the subroutine returns with the DE register pair pointing to a 'first extra byte' and the HL register pair pointing to a 'last extra byte', these extra bytes having been added after the original '(HL)+1' location.

; THE 'SET-MIN' SUBROUTINE
;
; Used by the routine at MAIN_EXEC.
;
; This subroutine resets the editing area and the areas after it to their minimum sizes. In effect it 'clears' the areas.
SET_MIN:
  LD HL,(E_LINE)          ; Fetch E-LINE.
  LD (HL),$0D             ; Make the editing area hold only the 'carriage return' character and the end marker, and set K-CUR accordingly
  LD (K_CUR),HL           ;
  INC HL                  ;
  LD (HL),$80             ;
  INC HL                  ; Reset WORKSP and move on to clear the work space.
  LD (WORKSP),HL          ;
; This entry point is used by the routines at STMT_LOOP and INPUT.
;
; Entering here will 'clear' the work space and the calculator stack.
SET_WORK:
  LD HL,(WORKSP)          ; Fetch the start address of the work space WORKSP.
  LD (STKBOT),HL          ; Clear the work space by setting STKBOT equal to WORKSP.
; This entry point is used by the routines at ERROR_2 and E_LINE_NO.
;
; Entering here will 'clear' only the calculator stack.
SET_STK:
  LD HL,(STKBOT)          ; Fetch STKBOT.
  LD (STKEND),HL          ; Clear the stack by setting STKEND equal to STKBOT.
; In all cases make MEM address the calculator's memory area.
  PUSH HL                 ; Save STKEND.
  LD HL,MEMBOT            ; The base of the memory area (MEMBOT).
  LD (MEM),HL             ; Set MEM to this address.
  POP HL                  ; Restore STKEND to the HL register pair before returning.
  RET                     ;

; THE 'RECLAIM THE EDIT-LINE' SUBROUTINE
;
; This routine is not used.
REC_EDIT:
  LD DE,(E_LINE)          ; Fetch E-LINE.
  JP RECLAIM_1            ; Reclaim the memory.

; THE 'INDEXER' SUBROUTINE
;
; This subroutine is used on several occasions to look through tables.
;
;   C Code to look for
;   HL Base address of the table
; O:HL Address of the second byte of the required table entry (if found)
; O:F Carry flag is set if the code is found
INDEXER_1:
  INC HL                  ; Move on to consider the next pair of entries.
; The main entry point is here and is used by the routines at CHAN_FLAG, CLOSE_2, OPEN_2, SCANNING and S_LETTER.
INDEXER:
  LD A,(HL)               ; Fetch the first of a pair of entries but return if it is zero - the end marker.
  AND A                   ;
  RET Z                   ;
  CP C                    ; Compare it to the supplied code.
  INC HL                  ; Point to the second entry.
  JR NZ,INDEXER_1         ; Jump back if the correct entry has not been found.
  SCF                     ; The carry flag is set upon a successful search.
  RET

; THE 'CLOSE #' COMMAND ROUTINE
;
; The address of this routine is found in the parameter table.
;
; This command allows the user to close streams. However for streams +00 to +03 the 'initial' stream data is restored and these streams cannot therefore be closed.
CLOSE:
  CALL STR_DATA           ; The existing data for the stream is fetched.
  CALL CLOSE_2            ; Check the code in that stream's channel.
  LD BC,$0000             ; Prepare to make the stream's data zero.
  LD DE,$A3E2             ; Prepare to identify the use of streams +00 to +03.
  EX DE,HL                ;
  ADD HL,DE               ; The carry flag will be set with streams +04 to +0F.
  JR C,CLOSE_1            ; Jump forward with these streams; otherwise find the correct entry in the initial stream data table.
  LD BC,$15D4             ;
  ADD HL,BC               ;
  LD C,(HL)               ; Fetch the initial data for streams +00 to +03.
  INC HL                  ;
  LD B,(HL)               ;
CLOSE_1:
  EX DE,HL                ; Now enter the data: either zero and zero, or the initial values.
  LD (HL),C               ;
  INC HL                  ;
  LD (HL),B               ;
  RET

; THE 'CLOSE-2' SUBROUTINE
;
; Used by the routine at CLOSE.
;
; The code of the channel associated with the stream being closed has to be 'K', 'S', or 'P'.
;
; BC Offset from the stream data table (STRMS)
; HL Address of the offset in the stream data table
CLOSE_2:
  PUSH HL                 ; Save the address of the stream's data.
  LD HL,(CHANS)           ; Fetch the base address of the channel information area (CHANS) and find the channel data for the stream being closed.
  ADD HL,BC               ;
  INC HL                  ; Step past the subroutine addresses and pick up the code for that channel.
  INC HL                  ;
  INC HL                  ;
  LD C,(HL)               ;
  EX DE,HL                ; Save the pointer.
  LD HL,CLOSESTRM         ; The base address of the CLOSE stream look-up table.
  CALL INDEXER            ; Index into this table and locate the required offset.
  LD C,(HL)               ; Pass the offset to the BC register pair.
  LD B,$00                ;
  ADD HL,BC               ; Jump to the appropriate routine.
  JP (HL)                 ;

; THE 'CLOSE STREAM LOOK-UP' TABLE
;
; Used by the routine at CLOSE_2.
CLOSESTRM:
  DEFB "K",$05            ; Channel 'K', offset +05 (CLOSE_STR)
  DEFB "S",$03            ; Channel 'S', offset +03 (CLOSE_STR)
  DEFB "P",$01            ; Channel 'P', offset +01 (CLOSE_STR)
; Note: there is no end marker at the end of this table, which is a bug.

; THE 'CLOSE STREAM' SUBROUTINE
;
; The address of this routine is derived from an offset found in the close stream lookup table.
;
; O:HL Address of the offset in the stream data table (see STRMS)
CLOSE_STR:
  POP HL                  ; Fetch the channel information pointer and return.
  RET                     ;

; THE 'STREAM DATA' SUBROUTINE
;
; Used by the routines at CLOSE and OPEN.
;
; This subroutine returns in the BC register pair the stream data for a given stream.
;
; O:BC Offset from the stream data table (STRMS)
; O:HL Address of the offset in the stream data table
STR_DATA:
  CALL FIND_INT1          ; The given stream number is taken off the calculator stack.
  CP $10                  ; Give an error if the stream number is greater than +0F.
  JR C,STR_DATA1          ;
; This entry point is used by the routines at OPEN and CAT_ETC.
;
; Report O - Invalid stream.
REPORT_O_2:
  RST $08                 ; Call the error handling routine.
  DEFB $17                ;
; Continue with valid stream numbers.
STR_DATA1:
  ADD A,$03               ; Range now +03 to +12.
  RLCA                    ; And now +06 to +24.
  LD HL,STRMS             ; The base address of the stream data area.
  LD C,A                  ; Move the stream code to the BC register pair.
  LD B,$00                ;
  ADD HL,BC               ; Index into the data area and fetch the the two data bytes into the BC register pair.
  LD C,(HL)               ;
  INC HL                  ;
  LD B,(HL)               ;
  DEC HL                  ; Make the pointer address the first of the data bytes before returning.
  RET                     ;

; THE 'OPEN #' COMMAND ROUTINE
;
; The address of this routine is found in the parameter table.
;
; This command allows the user to OPEN streams. A channel code must be supplied and it must be 'K', 'k', 'S', 's', 'P', or 'p'.
;
; Note that no attempt is made to give streams +00 to +03 their initial data.
OPEN:
  RST $28                 ; Use the calculator to exchange the stream number and the channel code.
  DEFB $01                ; exchange
  DEFB $38                ; end_calc
  CALL STR_DATA           ; Fetch the data for the stream.
  LD A,B                  ; Jump forward if both bytes of the data are zero, i.e. the stream was in a closed state.
  OR C                    ;
  JR Z,OPEN_1             ;
  EX DE,HL                ; Save HL.
  LD HL,(CHANS)           ; Fetch CHANS - the base address of the channel information and find the code of the channel associated with the stream being OPENed.
  ADD HL,BC               ;
  INC HL                  ;
  INC HL                  ;
  INC HL                  ;
  LD A,(HL)               ;
  EX DE,HL                ; Return HL.
  CP "K"                  ; The code fetched from the channel information area must be 'K', 'S' or 'P'; give an error if it is not.
  JR Z,OPEN_1             ;
  CP "S"                  ;
  JR Z,OPEN_1             ;
  CP "P"                  ;
  JR NZ,REPORT_O_2        ;
OPEN_1:
  CALL OPEN_2             ; Collect the appropriate data in DE.
  LD (HL),E               ; Enter the data into the two bytes in the stream information area.
  INC HL                  ;
  LD (HL),D               ;
  RET                     ; Finally return.

; THE 'OPEN-2' SUBROUTINE
;
; Used by the routine at OPEN.
;
; The appropriate stream data bytes for the channel that is associated with the stream being opened are found.
OPEN_2:
  PUSH HL                 ; Save HL.
  CALL STK_FETCH          ; Fetch the parameters of the channel code.
  LD A,B                  ; Give an error if the expression supplied is a null expression, e.g. OPEN #5,"".
  OR C                    ;
  JR NZ,OPEN_3            ;
; This entry point is used by the routine at OPEN_P.
;
; Report F - Invalid file name.
REPORT_F:
  RST $08                 ; Call the error handling routine.
  DEFB $0E                ;
; Continue if no error occurred.
OPEN_3:
  PUSH BC                 ; The length of the expression is saved.
  LD A,(DE)               ; Fetch the first character.
  AND $DF                 ; Convert lower case codes to upper case ones.
  LD C,A                  ; Move code to the C register.
  LD HL,OPENSTRM          ; The base address of the OPEN stream look-up table.
  CALL INDEXER            ; Index into this table and locate the required offset.
  JR NC,REPORT_F          ; Jump back if not found.
  LD C,(HL)               ; Pass the offset to the BC register pair.
  LD B,$00                ;
  ADD HL,BC               ; Make HL point to the start of the appropriate subroutine.
  POP BC                  ; Fetch the length of the expression before jumping to the subroutine.
  JP (HL)                 ;

; THE 'OPEN STREAM LOOK-UP' TABLE
;
; Used by the routine at OPEN_2.
OPENSTRM:
  DEFB "K",$06            ; Channel 'K', offset +06 (OPEN_K)
  DEFB "S",$08            ; Channel 'S', offset +08 (OPEN_S)
  DEFB "P",$0A            ; Channel 'P', offset +0A (OPEN_P)
  DEFB $00                ; End marker.

; THE 'OPEN-K' SUBROUTINE
;
; The address of this routine is derived from an offset found in the open stream lookup table.
;
; BC Length of the expression specifying the channel code
OPEN_K:
  LD E,$01                ; The data bytes will be +01 and +00.
  JR OPEN_END

; THE 'OPEN-S' SUBROUTINE
;
; The address of this routine is derived from an offset found in the open stream lookup table.
;
; BC Length of the expression specifying the channel code
OPEN_S:
  LD E,$06                ; The data bytes will be +06 and +00.
  JR OPEN_END

; THE 'OPEN-P' SUBROUTINE
;
; The address of this routine is derived from an offset found in the open stream lookup table.
;
; BC Length of the expression specifying the channel code
OPEN_P:
  LD E,$10                ; The data bytes will be +10 and +00.
; This entry point is used by the routines at OPEN_K and OPEN_S.
OPEN_END:
  DEC BC                  ; Decrease the length of the expression and give an error if it was not a single character.
  LD A,B                  ;
  OR C                    ;
  JR NZ,REPORT_F          ;
  LD D,A                  ; Otherwise clear the D register, fetch HL and return.
  POP HL                  ;
  RET                     ;

; THE 'CAT, ERASE, FORMAT and MOVE' COMMAND ROUTINES
;
; The address of this routine is found in the parameter table.
;
; In the standard Spectrum system the use of these commands leads to the production of report O - Invalid stream.
CAT_ETC:
  JR REPORT_O_2           ; Give this report.

; THE 'LIST and LLIST' COMMAND ROUTINES
;
; The routines in this part of the 16K program are used to produce listings of the current BASIC program. Each line has to have its line number evaluated, its tokens expanded and the appropriate cursors positioned.
;
; The entry point AUTO_LIST is used by both MAIN_EXEC and ED_UP to produce a single page of the listing.
AUTO_LIST:
  LD (LIST_SP),SP         ; The stack pointer is saved at LIST-SP allowing the machine stack to be reset when the listing is finished (see PO_SCR).
  LD (IY+$02),$10         ; Signal 'automatic listing in the main screen' (set bit 4 of TV-FLAG and reset all other bits).
  CALL CL_ALL             ; Clear this part of the screen.
  SET 0,(IY+$02)          ; Switch to the editing area (set bit 0 of TV-FLAG).
  LD B,(IY+$31)           ; Now clear the the lower part of the screen as well (see DF-SZ).
  CALL CL_LINE            ;
  RES 0,(IY+$02)          ; Then switch back (reset bit 0 of TV-FLAG).
  SET 0,(IY+$30)          ; Signal 'screen is clear' (set bit 0 of FLAGS2).
  LD HL,(E_PPC)           ; Now fetch the the 'current' line number (E-PPC) and the 'automatic' line number (S-TOP).
  LD DE,(S_TOP)           ;
  AND A                   ; If the 'current' number is less than the 'automatic' number then jump forward to update the 'automatic' number.
  SBC HL,DE               ;
  ADD HL,DE               ;
  JR C,AUTO_L_2           ;
; The 'automatic' number has now to be altered to give a listing with the 'current' line appearing near the bottom of the screen.
  PUSH DE                 ; Save the 'automatic' number.
  CALL LINE_ADDR          ; Find the address of the start of the 'current' line and produce an address roughly a 'screen before it' (negated).
  LD DE,$02C0             ;
  EX DE,HL                ;
  SBC HL,DE               ;
  EX (SP),HL              ; Save the 'result' on the machine stack whilst the 'automatic' line address is also found (in HL).
  CALL LINE_ADDR          ;
  POP BC                  ; The 'result' goes to the BC register pair.
; A loop is now entered. The 'automatic' line number is increased on each pass until it is likely that the 'current' line will show on a listing.
AUTO_L_1:
  PUSH BC                 ; Save the 'result'.
  CALL NEXT_ONE           ; Find the address of the start of the line after the present 'automatic' line (in DE).
  POP BC                  ; Restore the 'result'.
  ADD HL,BC               ; Perform the computation and jump forward if finished.
  JR C,AUTO_L_3           ;
  EX DE,HL                ; Move the next line's address to the HL register pair and collect its line number.
  LD D,(HL)               ;
  INC HL                  ;
  LD E,(HL)               ;
  DEC HL                  ;
  LD (S_TOP),DE           ; Now S-TOP can be updated and the test repeated with the new line.
  JR AUTO_L_1             ;
; Now the 'automatic' listing can be made.
AUTO_L_2:
  LD (S_TOP),HL           ; When E-PPC is less than S-TOP.
AUTO_L_3:
  LD HL,(S_TOP)           ; Fetch the top line's number (S-TOP) and hence its address.
  CALL LINE_ADDR          ;
  JR Z,AUTO_L_4           ; If the line cannot be found use DE instead.
  EX DE,HL                ;
AUTO_L_4:
  CALL LIST_ALL           ; The listing is produced.
  RES 4,(IY+$02)          ; The return will be to here unless scrolling was needed to show the current line; reset bit 4 of TV-FLAG before returning.
  RET                     ;

; THE 'LLIST' ENTRY POINT
;
; The address of this routine is found in the parameter table.
;
; The printer channel will need to be opened.
LLIST:
  LD A,$03                ; Use stream +03.
  JR LIST_1               ; Jump forward.

; THE 'LIST' ENTRY POINT
;
; The address of this routine is found in the parameter table.
;
; The 'main screen' channel will need to be opened.
LIST:
  LD A,$02                ; Use stream +02.
; This entry point is used by the routine at LLIST.
LIST_1:
  LD (IY+$02),$00         ; Signal 'an ordinary listing in the main part of the screen' (reset bit 4 of TV-FLAG and all other bits).
  CALL SYNTAX_Z           ; Open the channel unless checking syntax.
  CALL NZ,CHAN_OPEN       ;
  RST $18                 ; With the present character in the A register see if the stream is to be changed.
  CALL STR_ALTER          ;
  JR C,LIST_4             ; Jump forward if unchanged.
  RST $18                 ; Is the present character a ';'?
  CP ";"                  ;
  JR Z,LIST_2             ; Jump if it is.
  CP ","                  ; Is it a ','?
  JR NZ,LIST_3            ; Jump if it is not.
LIST_2:
  RST $20                 ; A numeric expression must follow, e.g. LIST #5,20.
  CALL CLASS_06           ;
  JR LIST_5               ; Jump forward with it.
LIST_3:
  CALL USE_ZERO           ; Otherwise use zero and also jump forward.
  JR LIST_5               ;
; Come here if the stream was unaltered.
LIST_4:
  CALL FETCH_NUM          ; Fetch any line or use zero if none supplied.
LIST_5:
  CALL CHECK_END          ; If checking the syntax of the edit-line move on to the next statement.
  CALL FIND_INT2          ; Line number to BC.
  LD A,B                  ; High byte to A.
  AND $3F                 ; Limit the high byte to the correct range and pass the whole line number to HL.
  LD H,A                  ;
  LD L,C                  ;
  LD (E_PPC),HL           ; Set E-PPC and find the address of the start of this line or the first line after it if the actual line does not exist.
  CALL LINE_ADDR          ;
; This entry point is used by the routine at AUTO_LIST.
LIST_ALL:
  LD E,$01                ; Flag 'before the current line'.
; Now the controlling loop for printing a series of lines is entered.
LIST_ALL_1:
  CALL OUT_LINE           ; Print the whole of a BASIC line.
  RST $10                 ; This will be a 'carriage return'.
  BIT 4,(IY+$02)          ; Jump back unless dealing with an automatic listing (bit 4 of TV-FLAG set).
  JR Z,LIST_ALL_1         ;
  LD A,(DF_SZ)            ; Also jump back if there is still part of the main screen that can be used (DF-SZ <> S-POSN-hi).
  SUB (IY+$4F)            ;
  JR NZ,LIST_ALL_1        ;
  XOR E                   ; A return can be made at this point if the screen is full and the current line has been printed (E=+00).
  RET Z                   ;
  PUSH HL                 ; However if the current line is missing from the listing then S-TOP has to be updated and a further line printed (using scrolling).
  PUSH DE                 ;
  LD HL,S_TOP             ;
  CALL LN_FETCH           ;
  POP DE                  ;
  POP HL                  ;
  JR LIST_ALL_1           ;

; THE 'PRINT A WHOLE BASIC LINE' SUBROUTINE
;
; Used by the routines at ED_EDIT and LIST.
;
; The HL register pair points to the start of the line - the location holding the high byte of the line number.
;
; Before the line number is printed it is tested to determine whether it comes before the 'current' line, is the 'current' line, or comes after.
;
; HL Address of the start of the BASIC line
OUT_LINE:
  LD BC,(E_PPC)           ; Fetch the 'current' line number from E-PPC and compare it.
  CALL CP_LINES           ;
  LD D,">"                ; Pre-load the D register with the current line cursor.
  JR Z,OUT_LINE1          ; Jump forward if printing the 'current' line.
  LD DE,$0000             ; Load the D register with zero (it is not the cursor) and set E to hold +01 if the line is before the 'current' line and +00 if after. (The carry flag comes from CP_LINES.)
  RL E                    ;
OUT_LINE1:
  LD (IY+$2D),E           ; Save the line marker in BREG.
  LD A,(HL)               ; Fetch the high byte of the line number and make a full return if the listing has been finished.
  CP $40                  ;
  POP BC                  ;
  RET NC                  ;
  PUSH BC
  CALL OUT_NUM_2          ; The line number can now be printed - with leading spaces.
  INC HL                  ; Move the pointer on to address the first command code in the line.
  INC HL                  ;
  INC HL                  ;
  RES 0,(IY+$01)          ; Signal 'leading space allowed' (reset bit 0 of FLAGS).
  LD A,D                  ; Fetch the cursor code and jump forward unless the cursor is to be printed.
  AND A                   ;
  JR Z,OUT_LINE3          ;
  RST $10                 ; So print the cursor now.
; This entry point is used by the routine at ED_COPY.
OUT_LINE2:
  SET 0,(IY+$01)          ; Signal 'no leading space now' (set bit 0 of FLAGS).
OUT_LINE3:
  PUSH DE                 ; Save the registers.
  EX DE,HL                ; Move the pointer to DE.
  RES 2,(IY+$30)          ; Signal 'not in quotes' (reset bit 2 of FLAGS2).
  LD HL,FLAGS             ; This is FLAGS.
  RES 2,(HL)              ; Signal 'print in K-mode'.
  BIT 5,(IY+$37)          ; Jump forward unless in INPUT mode (bit 5 of FLAGX set).
  JR Z,OUT_LINE4          ;
  SET 2,(HL)              ; Signal 'print in L-mode'.
; Now enter a loop to print all the codes in the rest of the BASIC line - jumping over floating-point forms as necessary.
OUT_LINE4:
  LD HL,(X_PTR)           ; Fetch the syntax error pointer (X-PTR) and jump forward unless it is time to print the error marker.
  AND A                   ;
  SBC HL,DE               ;
  JR NZ,OUT_LINE5         ;
  LD A,"?"                ; Print the error marker now. It is a flashing '?'.
  CALL OUT_FLASH          ;
OUT_LINE5:
  CALL OUT_CURS           ; Consider whether to print the cursor.
  EX DE,HL                ; Move the pointer to HL now.
  LD A,(HL)               ; Fetch each character in turn.
  CALL NUMBER             ; If the character is a 'number marker' then the hidden floating-point form is not to be printed.
  INC HL                  ; Update the pointer for the next pass.
  CP $0D                  ; Is the character a 'carriage return'?
  JR Z,OUT_LINE6          ; Jump if it is.
  EX DE,HL                ; Switch the pointer to DE.
  CALL OUT_CHAR           ; Print the character.
  JR OUT_LINE4            ; Go around the loop for at least one further pass.
; The line has now been printed.
OUT_LINE6:
  POP DE                  ; Restore the DE register pair and return.
  RET                     ;

; THE 'NUMBER' SUBROUTINE
;
; Used by the routines at OUT_LINE and EACH_STMT.
;
; If the A register holds the 'number marker' then the HL register pair is advanced past the floating-point form.
;
;   A Character code from a BASIC line
;   HL Address of that character
; O:A Next character code from the line (if the current one is a number marker)
; O:HL Address of that character
NUMBER:
  CP $0E                  ; Is the character a 'number marker'?
  RET NZ                  ; Return if not.
  INC HL                  ; Advance the pointer six times so as to step past the 'number marker' and the five locations holding the floating-point form.
  INC HL                  ;
  INC HL                  ;
  INC HL                  ;
  INC HL                  ;
  INC HL                  ;
  LD A,(HL)               ; Fetch the current code before returning.
  RET                     ;

; THE 'PRINT A FLASHING CHARACTER' SUBROUTINE
;
; Used by the routines at OUT_LINE and OUT_CURS.
;
; The 'error cursor' and the 'mode cursors' are printed using this subroutine.
;
; A Character code ('C', 'E', 'G', 'K', 'L' or '?')
OUT_FLASH:
  EXX                     ; Switch to the alternate registers.
  LD HL,(ATTR_T)          ; Save the values of ATTR-T and MASK-T on the machine stack.
  PUSH HL                 ;
  RES 7,H                 ; Ensure that FLASH is active.
  SET 7,L                 ;
  LD (ATTR_T),HL          ; Use these modified values for ATTR-T and MASK-T.
  LD HL,P_FLAG            ; This is P-FLAG.
  LD D,(HL)               ; Save P-FLAG also on the machine stack.
  PUSH DE                 ;
  LD (HL),$00             ; Ensure INVERSE 0, OVER 0, and not PAPER 9 nor INK 9.
  CALL PRINT_OUT          ; The character is printed.
  POP HL                  ; The former value of P-FLAG is restored.
  LD (IY+$57),H           ;
  POP HL                  ; The former values of ATTR-T and MASK-T are also restored before returning.
  LD (ATTR_T),HL          ;
  EXX                     ;
  RET                     ;

; THE 'PRINT THE CURSOR' SUBROUTINE
;
; Used by the routines at ED_COPY and OUT_LINE.
;
; A return is made if it is not the correct place to print the cursor but if it is then 'C', 'E', 'G', 'K' or 'L' will be printed.
;
; DE Address of the current position in the edit line or INPUT line
OUT_CURS:
  LD HL,(K_CUR)           ; Fetch the address of the cursor (K-CUR) but return if the correct place is not being considered.
  AND A                   ;
  SBC HL,DE               ;
  RET NZ                  ;
  LD A,(MODE)             ; The current value of MODE is fetched and doubled.
  RLC A                   ;
  JR Z,OUT_C_1            ; Jump forward unless dealing with Extended mode or Graphics.
  ADD A,$43               ; Add the appropriate offset to give 'E' or 'G'.
  JR OUT_C_2              ; Jump forward to print it.
OUT_C_1:
  LD HL,FLAGS             ; This is FLAGS.
  RES 3,(HL)              ; Signal 'K-mode'.
  LD A,"K"                ; The character 'K'.
  BIT 2,(HL)              ; Jump forward to print 'K' if 'the printing is to be in K-mode'.
  JR Z,OUT_C_2            ;
  SET 3,(HL)              ; The 'printing is to be in L-mode' so signal 'L-MODE'.
  INC A                   ; Form the character 'L'.
  BIT 3,(IY+$30)          ; Jump forward if not in 'C-mode' (bit 3 of FLAGS2 reset).
  JR Z,OUT_C_2            ;
  LD A,"C"                ; The character 'C'.
OUT_C_2:
  PUSH DE                 ; Save the DE register pair whilst the cursor is printed - FLASHing.
  CALL OUT_FLASH          ;
  POP DE                  ;
  RET                     ; Return once it has been done.
; Note: it is the action of considering which cursor-letter is to be printed that determines the mode - 'K', 'L' or 'C'.

; THE 'LN-FETCH' SUBROUTINE
;
; Used by the routines at ED_DOWN and LIST.
;
; This subroutine is entered with the HL register pair addressing a system variable - S-TOP or E-PPC.
;
; The subroutine returns with the system variable holding the line number of the following line.
;
; HL S-TOP or E-PPC
LN_FETCH:
  LD E,(HL)               ; The line number held by the system variable is collected.
  INC HL                  ;
  LD D,(HL)               ;
  PUSH HL                 ; The pointer is saved.
  EX DE,HL                ; The line number is moved to the HL register pair and incremented.
  INC HL                  ;
  CALL LINE_ADDR          ; The address of the start of this line is found, or the next line if the actual line number is not being used.
  CALL LINE_NO            ; The number of that line is fetched.
  POP HL                  ; The pointer to the system variable is restored.
; This entry point is used by the routine at ED_UP.
LN_STORE:
  BIT 5,(IY+$37)          ; Return if in 'INPUT mode' (bit 5 of FLAGX set).
  RET NZ                  ;
  LD (HL),D               ; Otherwise proceed to enter the line number into the two locations of the system variable.
  DEC HL                  ;
  LD (HL),E               ;
  RET                     ; Return when it has been done.

; THE 'PRINTING CHARACTERS IN A BASIC LINE' SUBROUTINE
;
; All of the character/token codes in a BASIC line are printed by repeatedly calling this subroutine.
OUT_SP_2:
  LD A,E                  ; The A register will hold +20 for a space or +FF for no-space.
  AND A                   ; Test the value and return if there is not to be a space.
  RET M                   ;
  JR OUT_CHAR             ; Jump forward to print a space.
; This entry point is used by the routine at OUT_NUM_1 to print a line number that may (E=+20) or may not (E=+FF) require leading spaces.
OUT_SP_NO:
  XOR A                   ; Clear the A register.
; The HL register pair holds the line number and the BC register the value for 'repeated subtraction' (-1000, -100 or -10).
OUT_SP_1:
  ADD HL,BC               ; The 'trial subtraction'.
  INC A                   ; Count each 'trial'.
  JR C,OUT_SP_1           ; Jump back until exhausted.
  SBC HL,BC               ; Restore last 'subtraction' and discount it.
  DEC A                   ;
  JR Z,OUT_SP_2           ; If no 'subtractions' were possible jump back to see if a space is to be printed.
  JP OUT_CODE             ; Otherwise print the digit.
; This entry point is used by the routine at OUT_LINE to print a character or token.
OUT_CHAR:
  CALL NUMERIC            ; Return carry reset if handling a digit code.
  JR NC,OUT_CH_3          ; Jump forward to print the digit.
  CP $21                  ; Also print the control characters and 'space'.
  JR C,OUT_CH_3           ;
  RES 2,(IY+$01)          ; Signal 'print in K-mode' (reset bit 2 of FLAGS).
  CP $CB                  ; Jump forward if dealing with the token 'THEN'.
  JR Z,OUT_CH_3           ;
  CP ":"                  ; Jump forward unless dealing with ':'.
  JR NZ,OUT_CH_1          ;
  BIT 5,(IY+$37)          ; Jump forward to print the ':' if in 'INPUT mode' (bit 5 of FLAGX set).
  JR NZ,OUT_CH_2          ;
  BIT 2,(IY+$30)          ; Jump forward if the ':' is 'not in quotes' (bit 2 of FLAGS2 reset), i.e. an inter-statement marker.
  JR Z,OUT_CH_3           ;
  JR OUT_CH_2             ; The ':' is inside quotes and can now be printed.
OUT_CH_1:
  CP "\""                 ; Accept for printing all characters except '"'.
  JR NZ,OUT_CH_2          ;
  PUSH AF                 ; Save the character code whilst changing the 'quote mode'.
  LD A,(FLAGS2)           ; Fetch FLAGS2 and flip bit 2.
  XOR $04                 ;
  LD (FLAGS2),A           ; Enter the amended value into FLAGS2 and restore the character code.
  POP AF                  ;
OUT_CH_2:
  SET 2,(IY+$01)          ; Signal 'the next character is to be printed in L-mode' (set bit 2 of FLAGS).
OUT_CH_3:
  RST $10                 ; The present character is printed before returning.
  RET                     ;
; Note: it is the consequence of the tests on the present character that determines whether the next character is to be printed in 'K' or 'L' mode.
;
; Also note how the program does not cater for ':' in REM statements.

; THE 'LINE-ADDR' SUBROUTINE
;
; Used by the routines at ED_EDIT, ED_UP, MAIN_ADD, AUTO_LIST, LIST, LN_FETCH, LINE_NEW and RESTORE.
;
; For a given line number, in the HL register pair, this subroutine returns the starting address of that line or the 'first line after', in the HL register pair, and the start of the previous line in the DE register pair.
;
; If the line number is being used the zero flag will be set. However if the 'first line after' is substituted then the zero flag is returned reset.
;
;   HL Target line number
; O:DE Start address of the line before the target
; O:HL Start address of the target line (if found) or the first line after
; O:F Zero flag set if the target line was found
LINE_ADDR:
  PUSH HL                 ; Save the given line number.
  LD HL,(PROG)            ; Fetch the system variable PROG and transfer the address to the DE register pair.
  LD D,H                  ;
  LD E,L                  ;
; Now enter a loop to test the line number of each line of the program against the given line number until the line number is matched or exceeded.
LINE_AD_1:
  POP BC                  ; The given line number.
  CALL CP_LINES           ; Compare the given line number against the addressed line number
  RET NC                  ; Return if carry reset; otherwise address the next line's number.
  PUSH BC                 ;
  CALL NEXT_ONE           ;
  EX DE,HL                ; Switch the pointers and jump back to consider the next line of the program.
  JR LINE_AD_1            ;

; THE 'COMPARE LINE NUMBERS' SUBROUTINE
;
; Used by the routines at OUT_LINE and LINE_ADDR.
;
; The given line number in the BC register pair is matched against the addressed line number.
;
;   BC First line number
;   HL Address of the second line number
; O:F Zero flag set if the line numbers match
; O:F Carry flag set if the first line number is greater than the second
CP_LINES:
  LD A,(HL)               ; Fetch the high byte of the addressed line number and compare it.
  CP B                    ;
  RET NZ                  ; Return if they do not match.
  INC HL                  ; Next compare the low bytes.
  LD A,(HL)               ;
  DEC HL                  ;
  CP C                    ;
  RET                     ; Return with the carry flag set if the addressed line number has yet to reach the given line number.

; Unused
  INC HL
  INC HL
  INC HL

; THE 'FIND EACH STATEMENT' SUBROUTINE
;
; Used by the routines at NEXT_LINE, LOOK_PROG, PASS_BY and S_FN_SBRN.
;
; This subroutine has two distinct functions.
;
; * It can be used to find the Dth statement in a BASIC line - returning with the HL register pair addressing the location before the start of the statement and the zero flag set.
; * Also the subroutine can be used to find a statement, if any, that starts with a given token code (in the E register).
;
;   D Statement number to look for (or +00 if looking for a token code)
;   E Token code to look for (or +00 if looking for a statement)
;   HL Address of the next character to consider
; O:HL Address of the token code or first character in the statement (if found)
; O:F Carry flag reset if the token code is found
; O:F Zero flag set if the statement is found
EACH_STMT:
  LD (CH_ADD),HL          ; Set CH-ADD to the current byte.
  LD C,$00                ; Set a 'quotes off' flag.
; Enter a loop to handle each statement in the BASIC line.
EACH_S_1:
  DEC D                   ; Decrease D and return if the required statement has been found.
  RET Z                   ;
  RST $20                 ; Fetch the next character code and jump if it does not match the given token code.
  CP E                    ;
  JR NZ,EACH_S_3          ;
  AND A                   ; But should it match then return with the carry and the zero flags both reset.
  RET                     ;
; Now enter another loop to consider the individual characters in the line to find where the statement ends.
EACH_S_2:
  INC HL                  ; Update the pointer and fetch the new code.
  LD A,(HL)               ;
EACH_S_3:
  CALL NUMBER             ; Step over any number.
  LD (CH_ADD),HL          ; Update CH-ADD.
  CP "\""                 ; Jump forward if the character is not a '"'.
  JR NZ,EACH_S_4          ;
  DEC C                   ; Otherwise set the 'quotes flag'.
EACH_S_4:
  CP ":"                  ; Jump forward if the character is a ':'.
  JR Z,EACH_S_5           ;
  CP $CB                  ; Jump forward unless the code is the token 'THEN'.
  JR NZ,EACH_S_6          ;
EACH_S_5:
  BIT 0,C                 ; Read the 'quotes flag' and jump back at the end of each statement (including after 'THEN').
  JR Z,EACH_S_1           ;
EACH_S_6:
  CP $0D                  ; Jump back unless at the end of a BASIC line.
  JR NZ,EACH_S_2          ;
  DEC D                   ; Decrease the statement counter and set the carry flag before returning.
  SCF                     ;
  RET                     ;

; THE 'NEXT-ONE' SUBROUTINE
;
; Used by the routines at ME_CONTRL, ME_ENTER, MAIN_ADD, AUTO_LIST, LINE_ADDR, LOOK_VARS and DIM.
;
; This subroutine can be used to find the 'next line' in the program area or the 'next variable' in the variables area. The subroutine caters for the six different types of variable that are used in the Spectrum system.
;
;   HL Start address of the current line or variable
; O:BC Length of the current line or variable
; O:DE Start address of the next line or variable
; O:HL Start address of the current line or variable (as on entry)
NEXT_ONE:
  PUSH HL                 ; Save the address of the current line or variable.
  LD A,(HL)               ; Fetch the first byte.
  CP $40                  ; Jump forward if searching for a 'next line'.
  JR C,NEXT_O_3           ;
  BIT 5,A                 ; Jump forward if searching for the next string or array variable.
  JR Z,NEXT_O_4           ;
  ADD A,A                 ; Jump forward with simple numeric and FOR-NEXT variables.
  JP M,NEXT_O_1           ;
  CCF                     ; Long name numeric variables only.
NEXT_O_1:
  LD BC,$0005             ; A numeric variable will occupy five locations but a FOR-NEXT control variable will need eighteen locations.
  JR NC,NEXT_O_2          ;
  LD C,$12                ;
NEXT_O_2:
  RLA                     ; The carry flag becomes reset for long named variables only, until the final character of the long name is reached.
  INC HL                  ; Increment the pointer and fetch the new code.
  LD A,(HL)               ;
  JR NC,NEXT_O_2          ; Jump back unless the previous code was the last code of the variable's name.
  JR NEXT_O_5             ; Now jump forward (BC=+0005 or +0012).
NEXT_O_3:
  INC HL                  ; Step past the low byte of the line number.
NEXT_O_4:
  INC HL                  ; Now point to the low byte of the length.
  LD C,(HL)               ; Fetch the length into the BC register pair.
  INC HL                  ;
  LD B,(HL)               ;
  INC HL                  ; Allow for the inclusive byte.
; In all cases the address of the 'next' line or variable is found.
NEXT_O_5:
  ADD HL,BC               ; Point to the first byte of the 'next' line or variable.
  POP DE                  ; Fetch the address of the previous one and continue into DIFFER.

; THE 'DIFFERENCE' SUBROUTINE
;
; Used by the routine at RECLAIM_1.
;
; The routine at NEXT_ONE continues here.
;
; The 'length' between two 'starts' is formed in the BC register pair. The pointers are reformed but returned exchanged.
;
;   DE First address
;   HL Second address
; O:BC HL-DE
; O:DE Second address (as in HL on entry)
; O:HL First address (as in DE on entry)
DIFFER:
  AND A                   ; Prepare for a true subtraction.
  SBC HL,DE               ; Find the length from one 'start' to the next and pass it to the BC register pair.
  LD B,H                  ;
  LD C,L                  ;
  ADD HL,DE               ; Reform the address and exchange them before returning.
  EX DE,HL                ;
  RET                     ;

; THE 'RECLAIMING' SUBROUTINE
;
; Used by the routines at LD_CONTRL, CLEAR_SP, REC_EDIT and CLEAR.
;
; The main entry point is used when the address of the first location to be reclaimed is in the DE register pair and the address of the first location to be left alone is in the HL register pair. The entry point RECLAIM_2 is used when the HL register pair points to the first location to be reclaimed and the BC register pair holds the number of bytes that are to be reclaimed.
;
;   DE Start address of the area to reclaim
;   HL One past the end address of the area to reclaim
; O:HL Start address of the area reclaimed (as DE on entry)
RECLAIM_1:
  CALL DIFFER             ; Use the 'difference' subroutine to develop the appropriate values.
; This entry point is used by the routines at LD_CONTRL, ME_ENTER, ED_DELETE, REMOVE_FP, MAIN_ADD, LET and DIM.
RECLAIM_2:
  PUSH BC                 ; Save the number of bytes to be reclaimed.
  LD A,B                  ; All the system variable pointers above the area have to be reduced by BC, so this number is 2's complemented before the pointers are altered.
  CPL                     ;
  LD B,A                  ;
  LD A,C                  ;
  CPL                     ;
  LD C,A                  ;
  INC BC                  ;
  CALL POINTERS           ;
  EX DE,HL                ; Return the 'first location' address to the DE register pair and form the address of the first location to the left.
  POP HL                  ;
  ADD HL,DE               ;
  PUSH DE                 ; Save the 'first location' whilst the actual reclamation occurs.
  LDIR                    ;
  POP HL                  ;
  RET                     ; Now return.

; THE 'E-LINE-NO' SUBROUTINE
;
; Used by the routines at MAIN_EXEC and LINE_SCAN.
;
; This subroutine is used to read the line number of the line in the editing area. If there is no line number, i.e. a direct BASIC line, then the line number is considered to be zero.
;
; O:BC Number of the line in the editing area (or +0000 if none)
E_LINE_NO:
  LD HL,(E_LINE)          ; Pick up the pointer to the edit-line (E-LINE).
  DEC HL                  ; Set CH-ADD to point to the location before any number.
  LD (CH_ADD),HL          ;
  RST $20                 ; Pass the first code to the A register.
  LD HL,MEMBOT            ; However before considering the code make the calculator's memory area a temporary calculator stack area (by setting STKEND equal to MEMBOT).
  LD (STKEND),HL          ;
  CALL INT_TO_FP          ; Now read the digits of the line number. Return zero if no number exists.
  CALL FP_TO_BC           ; Compress the line number into the BC register pair.
  JR C,E_L_1              ; Jump forward if the number exceeds 65,536.
  LD HL,$D8F0             ; Otherwise test it against 10,000.
  ADD HL,BC               ;
E_L_1:
  JP C,REPORT_C           ; Give report C if over 9,999.
  JP SET_STK              ; Return via SET_STK that restores the calculator stack to its rightful place.

; THE 'REPORT AND LINE NUMBER PRINTING' SUBROUTINE
;
; The entry point OUT_NUM_1, used by the routines at MAIN_EXEC and PRINT_FP, will lead to the number in the BC register pair being printed. Any value over 9,999 will not however be printed correctly.
;
; BC Number to print
OUT_NUM_1:
  PUSH DE                 ; Save the other registers throughout the subroutine.
  PUSH HL                 ;
  XOR A                   ; Clear the A register.
  BIT 7,B                 ; Jump forward to print a zero rather than '-2' when reporting on the edit-line.
  JR NZ,OUT_NUM_4         ;
  LD H,B                  ; Move the number to the HL register pair.
  LD L,C                  ;
  LD E,$FF                ; Flag 'no leading spaces'.
  JR OUT_NUM_3            ; Jump forward to print the number.
; The entry point OUT_NUM_2, used by the routine at OUT_LINE, will lead to the number indirectly addressed by the HL register pair being printed. This time any necessary leading spaces will appear. Again the limit of correctly printed numbers is 9,999.
OUT_NUM_2:
  PUSH DE                 ; Save the DE register pair.
  LD D,(HL)               ; Fetch the number into the DE register pair and save the pointer (updated).
  INC HL                  ;
  LD E,(HL)               ;
  PUSH HL                 ;
  EX DE,HL                ; Move the number to the HL register pair and flag 'leading spaces are to be printed'.
  LD E,$20                ;
; Now the integer form of the number in the HL register pair is printed.
OUT_NUM_3:
  LD BC,$FC18             ; This is '-1,000'.
  CALL OUT_SP_NO          ; Print a first digit.
  LD BC,$FF9C             ; This is '-100'.
  CALL OUT_SP_NO          ; Print the second digit.
  LD C,$F6                ; This is '-10'.
  CALL OUT_SP_NO          ; Print the third digit.
  LD A,L                  ; Move any remaining part of the number to the A register.
OUT_NUM_4:
  CALL OUT_CODE           ; Print the digit.
  POP HL                  ; Restore the registers before returning.
  POP DE                  ;
  RET                     ;

; THE SYNTAX TABLES
;
; Used by the routine at STMT_LOOP.
;
; i. The offset table.
;
; There is an offset value for each of the fifty BASIC commands.
SYNTAX:
  DEFB $B1                ; P_DEF_FN
  DEFB $CB                ; P_CAT
  DEFB $BC                ; P_FORMAT
  DEFB $BF                ; P_MOVE
  DEFB $C4                ; P_ERASE
  DEFB $AF                ; P_OPEN
  DEFB $B4                ; P_CLOSE
  DEFB $93                ; P_MERGE
  DEFB $91                ; P_VERIFY
  DEFB $92                ; P_BEEP
  DEFB $95                ; P_CIRCLE
  DEFB $98                ; P_INK
  DEFB $98                ; P_PAPER
  DEFB $98                ; P_FLASH
  DEFB $98                ; P_BRIGHT
  DEFB $98                ; P_INVERSE
  DEFB $98                ; P_OVER
  DEFB $98                ; P_OUT
  DEFB $7F                ; P_LPRINT
  DEFB $81                ; P_LLIST
  DEFB $2E                ; P_STOP
  DEFB $6C                ; P_READ
  DEFB $6E                ; P_DATA
  DEFB $70                ; P_RESTORE
  DEFB $48                ; P_NEW
  DEFB $94                ; P_BORDER
  DEFB $56                ; P_CONT
  DEFB $3F                ; P_DIM
  DEFB $41                ; P_REM
  DEFB $2B                ; P_FOR
  DEFB $17                ; P_GO_TO
  DEFB $1F                ; P_GO_SUB
  DEFB $37                ; P_INPUT
  DEFB $77                ; P_LOAD
  DEFB $44                ; P_LIST
  DEFB $0F                ; P_LET
  DEFB $59                ; P_PAUSE
  DEFB $2B                ; P_NEXT
  DEFB $43                ; P_POKE
  DEFB $2D                ; P_PRINT
  DEFB $51                ; P_PLOT
  DEFB $3A                ; P_RUN
  DEFB $6D                ; P_SAVE
  DEFB $42                ; P_RANDOM
  DEFB $0D                ; P_IF
  DEFB $49                ; P_CLS
  DEFB $5C                ; P_DRAW
  DEFB $44                ; P_CLEAR
  DEFB $15                ; P_RETURN
  DEFB $5D                ; P_COPY
; ii. The parameter table.
;
; For each of the fifty BASIC commands there are up to eight entries in the parameter table. These entries comprise command class details, required separators and, where appropriate, command routine addresses.
P_LET:
  DEFB $01                ; CLASS_01
  DEFB "="
  DEFB $02                ; CLASS_02
P_GO_TO:
  DEFB $06                ; CLASS_06
  DEFB $00                ; CLASS_00
  DEFW GO_TO
P_IF:
  DEFB $06                ; CLASS_06
  DEFB $CB                ; THEN
  DEFB $05                ; CLASS_05
  DEFW IF_CMD
P_GO_SUB:
  DEFB $06                ; CLASS_06
  DEFB $00                ; CLASS_00
  DEFW GO_SUB
P_STOP:
  DEFB $00                ; CLASS_00
  DEFW STOP
P_RETURN:
  DEFB $00                ; CLASS_00
  DEFW RETURN
P_FOR:
  DEFB $04                ; CLASS_04
  DEFB "="
  DEFB $06                ; CLASS_06
  DEFB $CC                ; TO
  DEFB $06                ; CLASS_06
  DEFB $05                ; CLASS_05
  DEFW FOR
P_NEXT:
  DEFB $04                ; CLASS_04
  DEFB $00                ; CLASS_00
  DEFW NEXT
P_PRINT:
  DEFB $05                ; CLASS_05
  DEFW PRINT
P_INPUT:
  DEFB $05                ; CLASS_05
  DEFW INPUT
P_DIM:
  DEFB $05                ; CLASS_05
  DEFW DIM
P_REM:
  DEFB $05                ; CLASS_05
  DEFW REM
P_NEW:
  DEFB $00                ; CLASS_00
  DEFW NEW
P_RUN:
  DEFB $03                ; CLASS_03
  DEFW RUN
P_LIST:
  DEFB $05                ; CLASS_05
  DEFW LIST
P_POKE:
  DEFB $08                ; CLASS_08
  DEFB $00                ; CLASS_00
  DEFW POKE
P_RANDOM:
  DEFB $03                ; CLASS_03
  DEFW RANDOMIZE
P_CONT:
  DEFB $00                ; CLASS_00
  DEFW CONTINUE
P_CLEAR:
  DEFB $03                ; CLASS_03
  DEFW CLEAR
P_CLS:
  DEFB $00                ; CLASS_00
  DEFW CLS
P_PLOT:
  DEFB $09                ; CLASS_09
  DEFB $00                ; CLASS_00
  DEFW PLOT
P_PAUSE:
  DEFB $06                ; CLASS_06
  DEFB $00                ; CLASS_00
  DEFW PAUSE
P_READ:
  DEFB $05                ; CLASS_05
  DEFW READ
P_DATA:
  DEFB $05                ; CLASS_05
  DEFW DATA
P_RESTORE:
  DEFB $03                ; CLASS_03
  DEFW RESTORE
P_DRAW:
  DEFB $09                ; CLASS_09
  DEFB $05                ; CLASS_05
  DEFW DRAW
P_COPY:
  DEFB $00                ; CLASS_00
  DEFW COPY
P_LPRINT:
  DEFB $05                ; CLASS_05
  DEFW LPRINT
P_LLIST:
  DEFB $05                ; CLASS_05
  DEFW LLIST
P_SAVE:
  DEFB $0B                ; CLASS_0B
P_LOAD:
  DEFB $0B                ; CLASS_0B
P_VERIFY:
  DEFB $0B                ; CLASS_0B
P_MERGE:
  DEFB $0B                ; CLASS_0B
P_BEEP:
  DEFB $08                ; CLASS_08
  DEFB $00                ; CLASS_00
  DEFW BEEP
P_CIRCLE:
  DEFB $09                ; CLASS_09
  DEFB $05                ; CLASS_05
  DEFW CIRCLE
P_INK:
  DEFB $07                ; CLASS_07
P_PAPER:
  DEFB $07                ; CLASS_07
P_FLASH:
  DEFB $07                ; CLASS_07
P_BRIGHT:
  DEFB $07                ; CLASS_07
P_INVERSE:
  DEFB $07                ; CLASS_07
P_OVER:
  DEFB $07                ; CLASS_07
P_OUT:
  DEFB $08                ; CLASS_08
  DEFB $00                ; CLASS_00
  DEFW OUT_CMD
P_BORDER:
  DEFB $06                ; CLASS_06
  DEFB $00                ; CLASS_00
  DEFW BORDER
P_DEF_FN:
  DEFB $05                ; CLASS_05
  DEFW DEF_FN
P_OPEN:
  DEFB $06                ; CLASS_06
  DEFB ","
  DEFB $0A                ; CLASS_0A
  DEFB $00                ; CLASS_00
  DEFW OPEN
P_CLOSE:
  DEFB $06                ; CLASS_06
  DEFB $00                ; CLASS_00
  DEFW CLOSE
P_FORMAT:
  DEFB $0A                ; CLASS_0A
  DEFB $00                ; CLASS_00
  DEFW CAT_ETC
P_MOVE:
  DEFB $0A                ; CLASS_0A
  DEFB ","
  DEFB $0A                ; CLASS_0A
  DEFB $00                ; CLASS_00
  DEFW CAT_ETC
P_ERASE:
  DEFB $0A                ; CLASS_0A
  DEFB $00                ; CLASS_00
  DEFW CAT_ETC
P_CAT:
  DEFB $00                ; CLASS_00
  DEFW CAT_ETC
; Note: the requirements for the different command classes are as follows:
;
; * CLASS_00 - No further operands.
; * CLASS_01 - Used in LET. A variable is required.
; * CLASS_02 - Used in LET. An expression, numeric or string, must follow.
; * CLASS_03 - A numeric expression may follow. Zero to be used in case of default.
; * CLASS_04 - A single character variable must follow.
; * CLASS_05 - A set of items may be given.
; * CLASS_06 - A numeric expression must follow.
; * CLASS_07 - Handles colour items.
; * CLASS_08 - Two numeric expressions, separated by a comma, must follow.
; * CLASS_09 - As for CLASS_08 but colour items may precede the expressions.
; * CLASS_0A - A string expression must follow.
; * CLASS_0B - Handles cassette routines.

; THE 'MAIN PARSER' OF THE BASIC INTERPRETER
;
; Used by the routine at MAIN_EXEC.
;
; The parsing routine of the BASIC interpreter is entered here when syntax is being checked, and at LINE_RUN when a BASIC program of one or more statements is to be executed.
;
; Each statement is considered in turn and the system variable CH-ADD is used to point to each code of the statement as it occurs in the program area or the editing area.
LINE_SCAN:
  RES 7,(IY+$01)          ; Signal 'syntax checking' (reset bit 7 of FLAGS).
  CALL E_LINE_NO          ; CH-ADD is made to point to the first code after any line number.
  XOR A                   ; The system variable SUBPPC is initialised to +00 and ERR-NR to +FF.
  LD (SUBPPC),A           ;
  DEC A                   ;
  LD (ERR_NR),A           ;
  JR STMT_L_1             ; Jump forward to consider the first statement of the line.

; THE STATEMENT LOOP
;
; Used by the routines at NEXT_LINE and STMT_NEXT.
;
; Each statement is considered in turn until the end of the line is reached.
STMT_LOOP:
  RST $20                 ; Advance CH-ADD along the line.
; This entry point is used by the routines at LINE_SCAN and IF_CMD.
STMT_L_1:
  CALL SET_WORK           ; The work space is cleared.
  INC (IY+$0D)            ; Increase SUBPPC on each passage around the loop.
  JP M,REPORT_C           ; But only '127' statements are allowed in a single line.
  RST $18                 ; Fetch a character.
  LD B,$00                ; Clear the B register for later.
  CP $0D                  ; Is the character a 'carriage return'?
  JR Z,LINE_END           ; Jump if it is.
  CP ":"                  ; Go around the loop again if it is a ':'.
  JR Z,STMT_LOOP          ;
; A statement has been identified so, first, its initial command is considered.
  LD HL,STMT_RET          ; Pre-load the machine stack with the return address STMT_RET.
  PUSH HL                 ;
  LD C,A                  ; Save the command temporarily in the C register whilst CH-ADD is advanced again.
  RST $20                 ;
  LD A,C                  ;
  SUB $CE                 ; Reduce the command's code by +CE, giving the range +00 to +31 for the fifty commands.
  JP C,REPORT_C           ; Give the appropriate error if not a command code.
  LD C,A                  ; Move the command code to the BC register pair (B holds +00).
  LD HL,SYNTAX            ; The base address of the syntax offset table.
  ADD HL,BC               ; The required offset is passed to the C register and used to compute the base address for the command's entries in the parameter table.
  LD C,(HL)               ;
  ADD HL,BC               ;
  JR GET_PARAM            ; Jump forward into the scanning loop with this address.
; Each of the command class routines applicable to the present command is executed in turn. Any required separators are also considered.
SCAN_LOOP:
  LD HL,(T_ADDR)          ; The temporary pointer to the entries in the parameter table (T-ADDR).
GET_PARAM:
  LD A,(HL)               ; Fetch each entry in turn.
  INC HL                  ; Update the pointer to the entries (T-ADDR) for the next pass.
  LD (T_ADDR),HL          ;
  LD BC,SCAN_LOOP         ; Pre-load the machine stack with the return address SCAN_LOOP.
  PUSH BC                 ;
  LD C,A                  ; Copy the entry to the C register for later.
  CP $20                  ; Jump forward if the entry is a 'separator'.
  JR NC,SEPARATOR         ;
  LD HL,CMDCLASS          ; The base address of the command class table.
  LD B,$00                ; Clear the B register and index into the table.
  ADD HL,BC               ;
  LD C,(HL)               ; Fetch the offset and compute the starting address of the required command class routine.
  ADD HL,BC               ;
  PUSH HL                 ; Push the address on to the machine stack.
  RST $18                 ; Before making an indirect jump to the command class routine pass the command code to the A register and set the B register to +FF.
  DEC B                   ;
  RET                     ;

; THE 'SEPARATOR' SUBROUTINE
;
; Used by the routine at STMT_LOOP.
;
; The report 'Nonsense in BASIC' is given if the required separator is not present. But note that when syntax is being checked the actual report does not appear on the screen - only the 'error marker'.
;
; C Entry from the parameter table
SEPARATOR:
  RST $18                 ; The current character is fetched and compared to the entry in the parameter table.
  CP C                    ;
  JP NZ,REPORT_C          ; Give the error report if there is not a match.
  RST $20                 ; Step past a correct character and return.
  RET                     ;

; THE 'STMT-RET' SUBROUTINE
;
; Used by the routine at STMT_LOOP.
;
; After the correct interpretation of a statement a return is made to this entry point.
STMT_RET:
  CALL BREAK_KEY          ; The BREAK key is tested after every statement.
  JR C,STMT_R_1           ; Jump forward unless it has been pressed.
; Report L - BREAK into program.
  RST $08                 ; Call the error handling routine.
  DEFB $14                ;
; Continue here as the BREAK key was not pressed.
STMT_R_1:
  BIT 7,(IY+$0A)          ; Jump forward if there is not a 'jump' to be made (NSPPC is +FF).
  JR NZ,STMT_NEXT         ;
  LD HL,(NEWPPC)          ; Fetch the 'new line' number (NEWPPC) and jump forward unless dealing with a further statement in the editing area.
  BIT 7,H                 ;
  JR Z,LINE_NEW           ;
; This routine continues into LINE_RUN.

; THE 'LINE-RUN' ENTRY POINT
;
; Used by the routine at MAIN_EXEC.
;
; The routine at STMT_RET continues here.
;
; This entry point is used wherever a line in the editing area is to be 'run'. In such a case the syntax/run flag (bit 7 of FLAGS) will be set.
;
; The entry point is also used in the syntax checking of a line in the editing area that has more than one statement (bit 7 of FLAGS will be reset).
LINE_RUN:
  LD HL,$FFFE             ; A line in the editing area is considered as line '-2'; set PPC accordingly
  LD (PPC),HL             ;
  LD HL,(WORKSP)          ; Make HL point to the end marker of the editing area (WORKSP-1) and DE to the location before the start of that area (E-LINE-1).
  DEC HL                  ;
  LD DE,(E_LINE)          ;
  DEC DE                  ;
  LD A,(NSPPC)            ; Fetch the number of the next statement to be handled (NSPPC) before jumping forward.
  JR NEXT_LINE            ;

; THE 'LINE-NEW' SUBROUTINE
;
; Used by the routine at STMT_RET.
;
; There has been a jump in the program and the starting address of the new line has to be found.
;
; HL Number of the new line
LINE_NEW:
  CALL LINE_ADDR          ; The starting address of the line, or the 'first line after' is found.
  LD A,(NSPPC)            ; Collect the statement number (NSPPC).
  JR Z,LINE_USE           ; Jump forward if the required line was found; otherwise check the validity of the statement number - must be zero.
  AND A                   ;
  JR NZ,REPORT_N          ;
  LD B,A                  ; Also check that the 'first line after' is not after the actual 'end of program'.
  LD A,(HL)               ;
  AND $C0                 ;
  LD A,B                  ;
  JR Z,LINE_USE           ; Jump forward with valid addresses; otherwise signal the error 'OK'.
; Report 0 - OK.
  RST $08                 ; Use the error handling routine.
  DEFB $FF                ;
; Note: obviously not an error in the normal sense - but rather a jump past the program.

; THE 'REM' COMMAND ROUTINE
;
; The address of this routine is found in the parameter table.
;
; The return address to STMT_RET is dropped which has the effect of forcing the rest of the line to be ignored.
REM:
  POP BC                  ; Drop the address - STMT_RET.
; This routine continues into LINE_END.

; THE 'LINE-END' ROUTINE
;
; Used by the routines at STMT_LOOP, STMT_NEXT and IF_CMD.
;
; The routine at REM continues here.
;
; If checking syntax a simple return is made but when 'running' the address held by NXTLIN has to be checked before it can be used.
LINE_END:
  CALL SYNTAX_Z           ; Return if syntax is being checked; otherwise fetch the address in NXTLIN.
  RET Z                   ;
  LD HL,(NXTLIN)          ;
  LD A,$C0                ; Return also if the address is after the end of the program - the 'run' is finished.
  AND (HL)                ;
  RET NZ                  ;
  XOR A                   ; Signal 'statement zero' before proceeding.
; This routine continues into LINE_USE.

; THE 'LINE-USE' ROUTINE
;
; Used by the routine at LINE_NEW.
;
; The routine at LINE_END continues here.
;
; This short routine has three functions:
;
; * Change statement zero to statement '1'.
; * Find the number of the new line and enter it into PPC.
; * Form the address of the start of the line after.
;
; A Statement number
; HL Start address of the line
LINE_USE:
  CP $01                  ; Statement zero becomes statement '1'.
  ADC A,$00               ;
  LD D,(HL)               ; The line number of the line to be used is collected and passed to PPC.
  INC HL                  ;
  LD E,(HL)               ;
  LD (PPC),DE             ;
  INC HL                  ; Now find the 'length' of the line.
  LD E,(HL)               ;
  INC HL                  ;
  LD D,(HL)               ;
  EX DE,HL                ; Switch over the values.
  ADD HL,DE               ; Form the address of the start of the line after in HL and the location before the 'next' line's first character in DE.
  INC HL                  ;
; This routine continues into NEXT_LINE.

; THE 'NEXT-LINE' ROUTINE
;
; Used by the routine at LINE_RUN.
;
; The routine at LINE_USE continues here.
;
; On entry the HL register pair points to the location after the end of the 'next' line to be handled and the DE register pair to the location before the first character of the line. This applies to lines in the program area and also to a line in the editing area - where the next line will be the same line again whilst there are still statements to be interpreted.
;
; DE One before the address of the first character in the line
; HL Start address of the following line
NEXT_LINE:
  LD (NXTLIN),HL          ; Set NXTLIN for use once the current line has been completed.
  EX DE,HL                ; As usual CH-ADD points to the location before the first character to be considered.
  LD (CH_ADD),HL          ;
  LD D,A                  ; The statement number is fetched.
  LD E,$00                ; The E register is cleared in case EACH_STMT is used.
  LD (IY+$0A),$FF         ; Signal 'no jump' by setting NSPPC to +FF.
  DEC D                   ; The statement number minus one goes into SUBPPC.
  LD (IY+$0D),D           ;
  JP Z,STMT_LOOP          ; A first statement can now be considered.
  INC D                   ; However for later statements the 'starting address' has to be found.
  CALL EACH_STMT          ;
  JR Z,STMT_NEXT          ; Jump forward unless the statement does not exist.
; This entry point is used by the routine at LINE_NEW.
;
; Report N - Statement lost.
REPORT_N:
  RST $08                 ; Call the error handling routine.
  DEFB $16                ;

; THE 'CHECK-END' SUBROUTINE
;
; Used by the routines at SAVE_ETC, LIST, CLASS_03, CLASS_02, CLASS_07, FOR, READ_3, DATA, DEF_FN, LPRINT, INPUT, CIRCLE, DRAW and DIM.
;
; This is an important routine and is called from many places in the monitor program when the syntax of the edit-line is being checked. The purpose of the routine is to give an error report if the end of a statement has not been reached and to move on to the next statement if the syntax is correct.
CHECK_END:
  CALL SYNTAX_Z           ; Do not proceed unless checking syntax.
  RET NZ                  ;
  POP BC                  ; Drop the addresses of SCAN_LOOP and STMT_RET before continuing into STMT_NEXT.
  POP BC                  ;

; THE 'STMT-NEXT' ROUTINE
;
; Used by the routines at STMT_RET and NEXT_LINE.
;
; The routine at CHECK_END continues here.
;
; If the present character is a 'carriage return' then the 'next statement' is on the 'next line'; if ':' it is on the same line; but if any other character is found then there is an error in syntax.
STMT_NEXT:
  RST $18                 ; Fetch the present character.
  CP $0D                  ; Consider the 'next line' if it is a 'carriage return'.
  JR Z,LINE_END           ;
  CP ":"                  ; Consider the 'next statement' if it is a ':'.
  JP Z,STMT_LOOP          ;
  JP REPORT_C             ; Otherwise there has been a syntax error.

; THE 'COMMAND CLASS' TABLE
;
; Used by the routine at STMT_LOOP.
CMDCLASS:
  DEFB $0F                ; CLASS_00
  DEFB $1D                ; CLASS_01
  DEFB $4B                ; CLASS_02
  DEFB $09                ; CLASS_03
  DEFB $67                ; CLASS_04
  DEFB $0B                ; CLASS_05
  DEFB $7B                ; CLASS_06
  DEFB $8E                ; CLASS_07
  DEFB $71                ; CLASS_08
  DEFB $B4                ; CLASS_09
  DEFB $81                ; CLASS_0A
  DEFB $CF                ; CLASS_0B

; THE 'COMMAND CLASSES - +00, +03 and +05'
;
; The address of this routine is derived from an offset found in the command class table.
;
; The commands of class +03 may, or may not, be followed by a number. e.g. RUN and RUN 200.
;
; A Code of the first character after the command
; HL Address of the first character after the command
CLASS_03:
  CALL FETCH_NUM          ; A number is fetched but zero is used in cases of default.
; The address of this entry point is derived from an offset found in the command class table.
;
; The commands of class +00 must not have any operands, e.g. COPY and CONTINUE.
CLASS_00:
  CP A                    ; Set the zero flag for later.
; The address of this entry point is derived from an offset found in the command class table.
;
; The commands of class +05 may be followed by a set of items, e.g. PRINT and PRINT "222".
CLASS_05:
  POP BC                  ; In all cases drop the address - SCAN_LOOP.
  CALL Z,CHECK_END        ; If handling commands of classes +00 and +03 and syntax is being checked move on now to consider the next statement.
  EX DE,HL                ; Save the line pointer in the DE register pair.
; After the command class entries and the separator entries in the parameter table have been considered the jump to the appropriate command routine is made.
  LD HL,(T_ADDR)          ; Fetch the pointer to the entries in the parameter table from T-ADDR and fetch the address of the required command routine.
  LD C,(HL)               ;
  INC HL                  ;
  LD B,(HL)               ;
  EX DE,HL                ; Exchange the pointers back and make an indirect jump to the command routine.
  PUSH BC                 ;
  RET                     ;

; THE 'COMMAND CLASS +01' ROUTINE
;
; Used by the routines at READ_3 and INPUT.
;
; The address of this routine is derived from an offset found in the command class table.
;
; Command class +01 is concerned with the identification of the variable in a LET, READ or INPUT statement.
CLASS_01:
  CALL LOOK_VARS          ; Look in the variables area to determine whether or not the variable has been used already.
; This routine continues into VAR_A_1.

; THE 'VARIABLE IN ASSIGNMENT' SUBROUTINE
;
; Used by the routine at CLASS_04.
;
; The routine at CLASS_01 continues here.
;
; This subroutine develops the appropriate values for the system variables DEST and STRLEN.
;
; C Bit 5: Set if the variable is numeric, reset if it's a string
; C Bit 6: Set if the variable is simple, reset if it's an array
; C Bit 7: Set if checking syntax, reset if executing
; HL Address of the last letter of the variable's name (in the variables area, if it exists)
; F Carry flag reset if the variable already exists
; F Zero flag reset if the variable is simple (not an array) and does not exist
VAR_A_1:
  LD (IY+$37),$00         ; Initialise FLAGX to +00.
  JR NC,VAR_A_2           ; Jump forward if the variable has been used before.
  SET 1,(IY+$37)          ; Signal 'a new variable' (set bit 1 of FLAGX).
  JR NZ,VAR_A_3           ; Give an error if trying to use an 'undimensioned array'.
; This entry point is used by the routines at NEXT and S_LETTER.
;
; Report 2 - Variable not found.
REPORT_2:
  RST $08                 ; Call the error handling routine.
  DEFB $01                ;
; Continue with the handling of existing variables.
VAR_A_2:
  CALL Z,STK_VAR          ; The parameters of simple string variables and all array variables are passed to the calculator stack. (STK_VAR will 'slice' a string if required.)
  BIT 6,(IY+$01)          ; Jump forward if handling a numeric variable (bit 6 of FLAGS set).
  JR NZ,VAR_A_3           ;
  XOR A                   ; Clear the A register.
  CALL SYNTAX_Z           ; The parameters of the string or string array variable are fetched unless syntax is being checked.
  CALL NZ,STK_FETCH       ;
  LD HL,FLAGX             ; This is FLAGX.
  OR (HL)                 ; Bit 0 is set only when handling complete 'simple strings' thereby signalling 'old copy to be deleted'.
  LD (HL),A               ;
  EX DE,HL                ; HL now points to the string or the element of the array.
; The pathways now come together to set STRLEN and DEST as required. For all numeric variables and 'new' string and string array variables STRLEN-lo holds the 'letter' of the variable's name. But for 'old' string and string array variables whether 'sliced' or complete it holds the 'length' in 'assignment'.
VAR_A_3:
  LD (STRLEN),BC          ; Set STRLEN as required.
; DEST holds the address for the 'destination' of an 'old' variable but in effect the 'source' for a 'new' variable.
  LD (DEST),HL            ; Set DEST as required and return.
  RET                     ;

; THE 'COMMAND CLASS +02' ROUTINE
;
; The address of this routine is derived from an offset found in the command class table.
;
; Command class +02 is concerned with the actual calculation of the value to be assigned in a LET statement.
CLASS_02:
  POP BC                  ; The address SCAN_LOOP is dropped.
  CALL VAL_FET_1          ; The assignment is made.
  CALL CHECK_END          ; Move on to the next statement either via CHECK_END if checking syntax, or STMT_RET if in 'run-time'.
  RET                     ;

; THE 'FETCH A VALUE' SUBROUTINE
;
; Used by the routines at CLASS_02 and READ_3.
;
; This subroutine is used by LET, READ and INPUT statements to first evaluate and then assign values to the previously designated variable.
;
; The main entry point is used by LET and READ and considers FLAGS, whereas the entry point VAL_FET_2 is used by INPUT and considers FLAGX.
VAL_FET_1:
  LD A,(FLAGS)            ; Use FLAGS.
; This entry point is used by the routine at IN_ASSIGN with A holding the contents of FLAGX.
VAL_FET_2:
  PUSH AF                 ; Save FLAGS or FLAGX.
  CALL SCANNING           ; Evaluate the next expression.
  POP AF                  ; Fetch the old FLAGS or FLAGX.
  LD D,(IY+$01)           ; Fetch the new FLAGS.
  XOR D                   ; The nature - numeric or string - of the variable and the expression must match.
  AND $40                 ;
  JR NZ,REPORT_C          ; Give report C if they do not.
  BIT 7,D                 ; Jump forward to make the actual assignment unless checking syntax (in which case simply return).
  JP NZ,LET               ;
  RET                     ;

; THE 'COMMAND CLASS +04' ROUTINE
;
; The address of this routine is derived from an offset found in the command class table.
;
; The command class +04 entry point is used by FOR and NEXT statements.
CLASS_04:
  CALL LOOK_VARS          ; Look in the variables area for the variable being used.
  PUSH AF                 ; Save the AF register pair whilst the discriminator byte is tested to ensure that the variable is a FOR-NEXT control variable.
  LD A,C                  ;
  OR $9F                  ;
  INC A                   ;
  JR NZ,REPORT_C          ;
  POP AF                  ; Restore the flags register and jump to make the variable that has been found the 'variable in assignment'.
  JR VAR_A_1              ;

; THE 'EXPECT NUMERIC/STRING EXPRESSIONS' SUBROUTINE
;
; Used by the routines at PR_ITEM_1 and S_2_COORD.
;
; There is a series of short subroutines that are used to fetch the result of evaluating the next expression. The result from a single expression is returned as a 'last value' on the calculator stack.
;
; This entry point is used when CH-ADD needs updating to point to the start of the first expression.
NEXT_2NUM:
  RST $20                 ; Advance CH-ADD.
; This entry point is used by the routine at CLASS_09.
;
; The address of this entry point is derived from an offset found in the command class table.
;
; This entry point allows for two numeric expressions, separated by a comma, to be evaluated.
CLASS_08:
  CALL CLASS_06           ; Evaluate each expression in turn - so evaluate the first.
  CP ","                  ; Give an error report if the separator is not a comma.
  JR NZ,REPORT_C          ;
  RST $20                 ; Advance CH-ADD.
; This entry point is used by the routines at SAVE_ETC, LIST, FETCH_NUM, FOR, PR_ITEM_1, STR_ALTER, CO_TEMP_1, CIRCLE, DRAW and INT_EXP1.
;
; The address of this entry point is derived from an offset found in the command class table.
;
; This entry point allows for a single numeric expression to be evaluated.
CLASS_06:
  CALL SCANNING           ; Evaluate the next expression.
  BIT 6,(IY+$01)          ; Return as long as the result was numeric (bit 6 of FLAGS set); otherwise it is an error.
  RET NZ                  ;
; This entry point is used by the routines at PROGNAME, SAVE_ETC, E_LINE_NO, STMT_LOOP, SEPARATOR, STMT_NEXT, VAL_FET_1, CLASS_04, DEF_FN, INPUT, CO_TEMP_1, CIRCLE, S_QUOTE_S, S_2_COORD, S_BRACKET, S_LETTER, S_FN_SBRN, LOOK_VARS, SLICING, DIM, DEC_TO_FP and val.
;
; Report C - Nonsense in BASIC.
REPORT_C:
  RST $08                 ; Call the error handling routine.
  DEFB $0B                ;
; This entry point is used by the routine at SAVE_ETC.
;
; The address of this entry point is derived from an offset found in the command class table.
;
; This entry point allows for a single string expression to be evaluated.
CLASS_0A:
  CALL SCANNING           ; Evaluate the next expression.
  BIT 6,(IY+$01)          ; This time return if the result indicates a string (bit 6 of FLAGS reset); otherwise give an error report.
  RET Z                   ;
  JR REPORT_C             ;

; THE 'SET PERMANENT COLOURS' SUBROUTINE
;
; The address of this routine is derived from an offset found in the command class table.
;
; This subroutine allows for the current temporary colours to be made permanent. As command class +07 it is in effect the command routine for the six colour item commands.
CLASS_07:
  BIT 7,(IY+$01)          ; The syntax/run flag (bit 7 of FLAGS) is read.
  RES 0,(IY+$02)          ; Signal 'main screen' (reset bit 0 of TV-FLAG).
  CALL NZ,TEMPS           ; Only during a 'run' call TEMPS to ensure the temporary colours are the main screen colours.
  POP AF                  ; Drop the return address SCAN_LOOP.
  LD A,(T_ADDR)           ; Fetch the low byte of T-ADDR and subtract +13 to give the range +D9 to +DE which are the token codes for INK to OVER.
  SUB $13                 ;
  CALL CO_TEMP_4          ; Change the temporary colours as directed by the BASIC statement.
  CALL CHECK_END          ; Move on to the next statement if checking syntax.
  LD HL,(ATTR_T)          ; Now the temporary colour values (ATTR-T and MASK-T) are made permanent (ATTR-P and MASK-P).
  LD (ATTR_P),HL          ;
  LD HL,P_FLAG            ; This is P-FLAG, and that too has to be considered.
  LD A,(HL)               ;
; The following instructions cleverly copy the even bits of the supplied byte to the odd bits, in effect making the permanent bits the same as the temporary ones.
  RLCA                    ; Move the mask leftwards.
  XOR (HL)                ; Impress onto the mask only the even bits of the other byte.
  AND %10101010           ;
  XOR (HL)                ;
  LD (HL),A               ; Restore the result.
  RET

; THE 'COMMAND CLASS +09' ROUTINE
;
; The address of this routine is derived from an offset found in the command class table.
;
; This routine is used by PLOT, DRAW and CIRCLE statements in order to specify the default conditions of 'FLASH 8; BRIGHT 8; PAPER 8;' that are set up before any embedded colour items are considered.
CLASS_09:
  CALL SYNTAX_Z           ; Jump forward if checking syntax.
  JR Z,CL_09_1            ;
  RES 0,(IY+$02)          ; Signal 'main screen' (reset bit 0 of TV-FLAG).
  CALL TEMPS              ; Set the temporary colours for the main screen.
  LD HL,MASK_T            ; This is MASK-T.
  LD A,(HL)               ; Fetch its present value but keep only its INK part 'unmasked'.
  OR $F8                  ;
  LD (HL),A               ; Restore the value which now indicates 'FLASH 8; BRIGHT 8; PAPER 8;'.
  RES 6,(IY+$57)          ; Also ensure NOT 'PAPER 9' (reset bit 6 of P-FLAG).
  RST $18                 ; Fetch the present character before continuing to deal with embedded colour items.
CL_09_1:
  CALL CO_TEMP_2          ; Deal with the locally dominant colour items.
  JR CLASS_08             ; Now get the first two operands for PLOT, DRAW or CIRCLE.

; THE 'COMMAND CLASS +0B' ROUTINE
;
; The address of this routine is derived from an offset found in the command class table.
;
; This routine is used by SAVE, LOAD, VERIFY and MERGE statements.
CLASS_0B:
  JP SAVE_ETC             ; Jump to the cassette handling routine.

; THE 'FETCH A NUMBER' SUBROUTINE
;
; Used by the routines at LIST and CLASS_03.
;
; This subroutine leads to a following numeric expression being evaluated but zero being used instead if there is no expression.
;
; A Code of the first character of the expression
FETCH_NUM:
  CP $0D                  ; Jump forward if at the end of a line.
  JR Z,USE_ZERO           ;
  CP ":"                  ; But jump to CLASS_06 unless at the end of a statement.
  JR NZ,CLASS_06          ;
; This entry point is used by the routines at SAVE_ETC and LIST.
;
; The calculator is now used to add the value zero to the calculator stack.
USE_ZERO:
  CALL SYNTAX_Z           ; Do not perform the operation if syntax is being checked.
  RET Z                   ;
  RST $28                 ; Use the calculator.
  DEFB $A0                ; stk_zero
  DEFB $38                ; end_calc
  RET                     ; Return with zero added to the stack.

; THE 'STOP' COMMAND ROUTINE
;
; The address of this routine is found in the parameter table.
;
; The command routine for STOP contains only a call to the error handling routine.
STOP:
  RST $08                 ; Call the error handling routine.
  DEFB $08                ;

; THE 'IF' COMMAND ROUTINE
;
; The address of this routine is found in the parameter table.
;
; On entry the value of the expression between the IF and the THEN is the 'last value' on the calculator stack. If this is logically true then the next statement is considered; otherwise the line is considered to have been finished.
IF_CMD:
  POP BC                  ; Drop the return address - STMT_RET.
  CALL SYNTAX_Z           ; Jump forward if checking syntax.
  JR Z,IF_1               ;
; Now use the calculator to 'delete' the last value on the calculator stack but leave the DE register pair addressing the first byte of the value.
  RST $28                 ; Use the calculator.
  DEFB $02                ; delete
  DEFB $38                ; end_calc
  EX DE,HL                ; Make HL point to the first byte and call TEST_ZERO.
  CALL TEST_ZERO          ;
  JP C,LINE_END           ; If the value was 'FALSE' jump to the next line.
IF_1:
  JP STMT_L_1             ; But if 'TRUE' jump to the next statement (after the THEN).

; THE 'FOR' COMMAND ROUTINE
;
; The address of this routine is found in the parameter table.
;
; This command routine is entered with the VALUE and the LIMIT of the FOR statement already on the top of the calculator stack.
;
; A Code of the next character in the statement
FOR:
  CP $CD                  ; Jump forward unless a 'STEP' is given.
  JR NZ,F_USE_1           ;
  RST $20                 ; Advance CH-ADD and fetch the value of the STEP.
  CALL CLASS_06           ;
  CALL CHECK_END          ; Move on to the next statement if checking syntax; otherwise jump forward.
  JR F_REORDER            ;
; There has not been a STEP supplied so the value '1' is to be used.
F_USE_1:
  CALL CHECK_END          ; Move on to the next statement if checking syntax.
  RST $28                 ; Otherwise use the calculator to place a '1' on the calculator stack.
  DEFB $A1                ; stk_one
  DEFB $38                ; end_calc
; The three values on the calculator stack are the VALUE (v), the LIMIT (l) and the STEP (s). These values now have to be manipulated.
F_REORDER:
  RST $28                 ; v, l, s
  DEFB $C0                ; st_mem_0: v, l, s (mem-0=s)
  DEFB $02                ; delete: v, l
  DEFB $01                ; exchange: l, v
  DEFB $E0                ; get_mem_0: l, v, s
  DEFB $01                ; exchange: l, s, v
  DEFB $38                ; end_calc
; A FOR control variable is now established and treated as a temporary calculator memory area.
  CALL LET                ; The variable is found, or created if needed (v is used).
  LD (MEM),HL             ; Make it a 'memory area' by setting MEM.
; The variable that has been found may be a simple numeric variable using only six locations in which case it will need extending.
  DEC HL                  ; Fetch the variable's single character name.
  LD A,(HL)               ;
  SET 7,(HL)              ; Ensure bit 7 of the name is set.
  LD BC,$0006             ; It will have six locations at least.
  ADD HL,BC               ; Make HL point after them.
  RLCA                    ; Rotate the name and jump if it was already a FOR variable.
  JR C,F_L_S              ;
  LD C,$0D                ; Otherwise create thirteen more locations.
  CALL MAKE_ROOM          ;
  INC HL                  ; Again make HL point to the LIMIT position.
; The initial values for the LIMIT and the STEP are now added.
F_L_S:
  PUSH HL                 ; The pointer is saved.
  RST $28                 ; l, s
  DEFB $02                ; delete: l
  DEFB $02                ; delete: -
  DEFB $38                ; end_calc: DE still points to 'l'
  POP HL                  ; The pointer is restored and both pointers exchanged.
  EX DE,HL                ;
  LD C,$0A                ; The ten bytes of the LIMIT and the STEP are moved.
  LDIR                    ;
; The looping line number and statement number are now entered.
  LD HL,(PPC)             ; The current line number (PPC).
  EX DE,HL                ; Exchange the registers before adding the line number to the FOR control variable.
  LD (HL),E               ;
  INC HL                  ;
  LD (HL),D               ;
  LD D,(IY+$0D)           ; The looping statement is always the next statement whether it exists or not (increment SUBPPC).
  INC D                   ;
  INC HL                  ;
  LD (HL),D               ;
; The NEXT_LOOP subroutine is called to test the possibility of a 'pass' and a return is made if one is possible; otherwise the statement after for FOR - NEXT loop has to be identified.
  CALL NEXT_LOOP          ; Is a 'pass' possible?
  RET NC                  ; Return now if it is.
  LD B,(IY+$38)           ; Fetch the variable's name from STRLEN.
  LD HL,(PPC)             ; Copy the present line number (PPC) to NEWPPC.
  LD (NEWPPC),HL          ;
  LD A,(SUBPPC)           ; Fetch the current statement number (SUBPPC) and two's complement it.
  NEG                     ;
  LD D,A                  ; Transfer the result to the D register.
  LD HL,(CH_ADD)          ; Fetch the current value of CH-ADD.
  LD E,$F3                ; The search will be for 'NEXT'.
; Now a search is made in the program area, from the present point onwards, for the first occurrence of NEXT followed by the correct variable.
F_LOOP:
  PUSH BC                 ; Save the variable's name.
  LD BC,(NXTLIN)          ; Fetch the current value of NXTLIN.
  CALL LOOK_PROG          ; The program area is now searched and BC will change with each new line examined.
  LD (NXTLIN),BC          ; Upon return save the pointer at NXTLIN.
  POP BC                  ; Restore the variable's name.
  JR C,REPORT_I           ; If there are no further NEXTs then give an error.
  RST $20                 ; Advance past the NEXT that was found.
  OR $20                  ; Allow for upper and lower case letters before the new variable name is tested.
  CP B                    ;
  JR Z,F_FOUND            ; Jump forward if it matches.
  RST $20                 ; Advance CH-ADD again and jump back if not the correct variable.
  JR F_LOOP               ;
; NEWPPC holds the line number of the line in which the correct NEXT was found. Now the statement number has to be found and stored in NSPPC.
F_FOUND:
  RST $20                 ; Advance CH-ADD.
  LD A,$01                ; The statement counter in the D register counted statements back from zero so it has to be subtracted from '1'.
  SUB D                   ;
  LD (NSPPC),A            ; The result is stored in NSPPC.
  RET                     ; Now return - to STMT_RET.
; Report I - FOR without NEXT.
REPORT_I:
  RST $08                 ; Call the error handling routine.
  DEFB $11                ;

; THE 'LOOK-PROG' SUBROUTINE
;
; Used by the routines at FOR, READ_3 and S_FN_SBRN.
;
; This subroutine is used to find occurrences of either DATA, DEF FN or NEXT.
;
;   E Token code to search for
;   HL Search start address
; O:F Carry flag reset if the token is found
LOOK_PROG:
  LD A,(HL)               ; Fetch the present character.
  CP ":"                  ; Jump forward if it is a ':', which will indicate there are more statements in the present line.
  JR Z,LOOK_P_2           ;
; Now a loop is entered to examine each further line in the program.
LOOK_P_1:
  INC HL                  ; Fetch the high byte of the line number and return with carry set if there are no further lines in the program.
  LD A,(HL)               ;
  AND $C0                 ;
  SCF                     ;
  RET NZ                  ;
  LD B,(HL)               ; The line number is fetched and passed to NEWPPC.
  INC HL                  ;
  LD C,(HL)               ;
  LD (NEWPPC),BC          ;
  INC HL                  ; Then the length is collected.
  LD C,(HL)               ;
  INC HL                  ;
  LD B,(HL)               ;
  PUSH HL                 ; The pointer is saved whilst the address of the end of the line is formed in the BC register pair.
  ADD HL,BC               ;
  LD B,H                  ;
  LD C,L                  ;
  POP HL                  ; The pointer is restored.
  LD D,$00                ; Set the statement counter to zero.
LOOK_P_2:
  PUSH BC                 ; The end-of-line pointer is saved whilst the statements of the line are examined.
  CALL EACH_STMT          ;
  POP BC                  ;
  RET NC                  ; Make a return if there was an 'occurrence'; otherwise consider the next line.
  JR LOOK_P_1             ;

; THE 'NEXT' COMMAND ROUTINE
;
; The address of this routine is found in the parameter table.
;
; The 'variable in assignment' has already been determined (see CLASS_04), and it remains to change the VALUE as required.
NEXT:
  BIT 1,(IY+$37)          ; Jump to give the error report if the variable was not found (bit 1 of FLAGX set).
  JP NZ,REPORT_2          ;
  LD HL,(DEST)            ; The address of the variable is fetched from DEST and the name tested further.
  BIT 7,(HL)              ;
  JR Z,REPORT_1           ;
; Next the variable's VALUE (v) and STEP (s) are manipulated by the calculator.
  INC HL                  ; Step past the name.
  LD (MEM),HL             ; Make the variable a temporary 'memory area' by setting MEM.
  RST $28                 ; -
  DEFB $E0                ; get_mem_0: v
  DEFB $E2                ; get_mem_2: v, s
  DEFB $0F                ; addition: v+s
  DEFB $C0                ; st_mem_0: v+s (v is replaced by v+s in mem-0)
  DEFB $02                ; delete: -
  DEFB $38                ; end_calc: -
; The result of adding the VALUE and the STEP is now tested against the LIMIT by calling NEXT_LOOP.
  CALL NEXT_LOOP          ; Test the new VALUE against the LIMIT.
  RET C                   ; Return now if the FOR-NEXT loop has been completed.
; Otherwise collect the 'looping' line number and statement.
  LD HL,(MEM)             ; Find the address of the low byte of the looping line number (MEM+0F).
  LD DE,$000F             ;
  ADD HL,DE               ;
  LD E,(HL)               ; Now fetch this line number.
  INC HL                  ;
  LD D,(HL)               ;
  INC HL                  ;
  LD H,(HL)               ; Followed by the statement number.
  EX DE,HL                ; Exchange the numbers before jumping forward to treat them as the destination line of a GO TO command.
  JP GO_TO_2              ;
; Report 1 - NEXT without FOR.
REPORT_1:
  RST $08                 ; Call the error handling routine.
  DEFB $00                ;

; THE 'NEXT-LOOP' SUBROUTINE
;
; Used by the routines at FOR and NEXT.
;
; This subroutine is used to determine whether the LIMIT (l) has been exceeded by the present VALUE (v). Note has to be taken of the sign of the STEP (s).
;
; The subroutine returns the carry flag set if the LIMIT is exceeded.
NEXT_LOOP:
  RST $28                 ; -
  DEFB $E1                ; get_mem_1: l
  DEFB $E0                ; get_mem_0: l, v
  DEFB $E2                ; get_mem_2: l, v, s
  DEFB $36                ; less_0: l, v,( 1/0)
  DEFB $00                ; jump_true to NEXT_1: l, v, (1/0)
  DEFB $02                ;
  DEFB $01                ; exchange: v, l
NEXT_1:
  DEFB $03                ; subtract: v-l or l-v
  DEFB $37                ; greater_0: (1/0)
  DEFB $00                ; jump_true to NEXT_2: (1/0)
  DEFB $04                ;
  DEFB $38                ; end_calc: -
  AND A                   ; Clear the carry flag and return - loop is possible.
  RET                     ;
; However if the loop is impossible the carry flag has to be set.
NEXT_2:
  DEFB $38                ; end_calc: -
  SCF                     ; Set the carry flag and return.
  RET                     ;

; THE 'READ' COMMAND ROUTINE
;
; The READ command allows for the reading of a DATA list and has an effect similar to a series of LET statements.
;
; Each assignment within a single READ statement is dealt with in turn. The system variable X-PTR is used as a storage location for the pointer to the READ statement whilst CH-ADD is used to step along the DATA list.
READ_3:
  RST $20                 ; Come here on each pass, after the first, to move along the READ statement.
; The address of this entry point is found in the parameter table.
READ:
  CALL CLASS_01           ; Consider whether the variable has been used before; find the existing entry if it has.
  CALL SYNTAX_Z           ; Jump forward if checking syntax.
  JR Z,READ_2             ;
  RST $18                 ; Save the current pointer CH-ADD in X-PTR.
  LD (X_PTR),HL           ;
  LD HL,(DATADD)          ; Fetch the current DATA list pointer (DATADD) and jump forward unless a new DATA statement has to be found.
  LD A,(HL)               ;
  CP ","                  ;
  JR Z,READ_1             ;
  LD E,$E4                ; The search is for 'DATA'.
  CALL LOOK_PROG          ; Jump forward if the search is successful.
  JR NC,READ_1            ;
; Report E - Out of DATA.
  RST $08                 ; Call the error handling routine.
  DEFB $0D                ;
; Continue - picking up a value from the DATA list.
READ_1:
  CALL TEMP_PTR1          ; Advance the pointer along the DATA list and set CH-ADD.
  CALL VAL_FET_1          ; Fetch the value and assign it to the variable.
  RST $18                 ; Fetch the current value of CH-ADD and store it in DATADD.
  LD (DATADD),HL          ;
  LD HL,(X_PTR)           ; Fetch the pointer to the READ statement from X-PTR and clear it.
  LD (IY+$26),$00         ;
  CALL TEMP_PTR2          ; Make CH-ADD once again point to the READ statement.
READ_2:
  RST $18                 ; Get the present character and see if it is a ','.
  CP ","                  ;
  JR Z,READ_3             ; If it is then jump back as there are further items; otherwise return via either CHECK_END (if checking syntax) or the 'RET' instruction (to STMT_RET).
  CALL CHECK_END          ;
  RET                     ;

; THE 'DATA' COMMAND ROUTINE
;
; The address of this routine is found in the parameter table.
;
; During syntax checking a DATA statement is checked to ensure that it contains a series of valid expressions, separated by commas. But in 'run-time' the statement is passed by.
DATA:
  CALL SYNTAX_Z           ; Jump forward unless checking syntax.
  JR NZ,DATA_2            ;
; A loop is now entered to deal with each expression in the DATA statement.
DATA_1:
  CALL SCANNING           ; Scan the next expression.
  CP ","                  ; Check for a comma separator.
  CALL NZ,CHECK_END       ; Move on to the next statement if not matched.
  RST $20                 ; Whilst there are still expressions to be checked go around the loop.
  JR DATA_1               ;
; The DATA statement has to be passed by in 'run-time'.
DATA_2:
  LD A,$E4                ; It is a 'DATA' statement that is to be passed by.
; This routine continues into PASS_BY.

; THE 'PASS-BY' SUBROUTINE
;
; Used by the routine at DEF_FN.
;
; The routine at DATA continues here.
;
; On entry the A register will hold either the token 'DATA' or the token 'DEF FN' depending on the type of statement that is being passed by.
;
; A +CE (DEF FN) or +E4 (DATA)
PASS_BY:
  LD B,A                  ; Make the BC register pair hold a very high number.
  CPDR                    ; Look back along the statement for the token.
  LD DE,$0200             ; Now look along the line for the statement after (the 'D-1'th statement from the current position).
  JP EACH_STMT            ;

; THE 'RESTORE' COMMAND ROUTINE
;
; The address of this routine is found in the parameter table.
;
; The operand for a RESTORE command is taken as a line number, zero being used if no operand is given.
RESTORE:
  CALL FIND_INT2          ; Compress the operand into the BC register pair.
; This entry point is used by the routine at RUN.
REST_RUN:
  LD H,B                  ; Transfer the result to the HL register pair.
  LD L,C                  ;
  CALL LINE_ADDR          ; Now find the address of that line or the 'first line after'.
  DEC HL                  ; Make DATADD point to the location before.
  LD (DATADD),HL          ;
  RET                     ; Return once it is done.

; THE 'RANDOMIZE' COMMAND ROUTINE
;
; The address of this routine is found in the parameter table.
;
; The operand is compressed into the BC register pair and transferred to the required system variable. However if the operand is zero the value in FRAMES1 and FRAMES2 is used instead.
RANDOMIZE:
  CALL FIND_INT2          ; Fetch the operand.
  LD A,B                  ; Jump forward unless the value of the operand is zero.
  OR C                    ;
  JR NZ,RAND_1            ;
  LD BC,(FRAMES)          ; Fetch the two low order bytes of FRAMES instead.
RAND_1:
  LD (SEED),BC            ; Now enter the result into the system variable SEED before returning.
  RET                     ;

; THE 'CONTINUE' COMMAND ROUTINE
;
; The address of this routine is found in the parameter table.
;
; The required line number and statement number within that line are made the object of a jump.
CONTINUE:
  LD HL,(OLDPPC)          ; The line number (OLDPPC).
  LD D,(IY+$36)           ; The statement number (OSPCC).
  JR GO_TO_2              ; Jump forward.

; THE 'GO TO' COMMAND ROUTINE
;
; Used by the routines at RUN and GO_SUB.
;
; The address of this routine is found in the parameter table.
;
; The operand of a GO TO ought to be a line number in the range 1-9999 but the actual test is against an upper value of 61439.
GO_TO:
  CALL FIND_INT2          ; Fetch the operand and transfer it to the HL register pair.
  LD H,B                  ;
  LD L,C                  ;
  LD D,$00                ; Set the statement number to zero.
  LD A,H                  ; Give the error message 'Integer out of range' with line numbers over 61439.
  CP $F0                  ;
  JR NC,REPORT_B_2        ;
; This entry point is used by the routines at NEXT, CONTINUE and RETURN to determine the line number of the next line to be handled.
GO_TO_2:
  LD (NEWPPC),HL          ; Enter the line number (NEWPPC) and then the statement number (NSPPC).
  LD (IY+$0A),D           ;
  RET                     ; Return - to STMT_RET.

; THE 'OUT' COMMAND ROUTINE
;
; The address of this routine is found in the parameter table.
;
; The two parameters for the 'OUT' instruction are fetched from the calculator stack and used as directed.
OUT_CMD:
  CALL TWO_PARAM          ; The operands are fetched.
  OUT (C),A               ; The actual 'OUT' instruction.
  RET                     ; Return - to STMT_RET.

; THE 'POKE' COMMAND ROUTINE
;
; The address of this routine is found in the parameter table.
;
; In a similar manner to OUT, the POKE operation is performed.
POKE:
  CALL TWO_PARAM          ; The operands are fetched.
  LD (BC),A               ; The actual POKE operation.
  RET                     ; Return - to STMT_RET.

; THE 'TWO-PARAM' SUBROUTINE
;
; Used by the routines at OUT_CMD and POKE.
;
; The topmost parameter on the calculator stack must be compressible into a single register. It is two's complemented if it is negative. The second parameter must be compressible into a register pair.
TWO_PARAM:
  CALL FP_TO_A            ; The parameter is fetched.
  JR C,REPORT_B_2         ; Give an error if it is too high a number.
  JR Z,TWO_P_1            ; Jump forward with positive numbers but two's complement negative numbers.
  NEG                     ;
TWO_P_1:
  PUSH AF                 ; Save the first parameter whilst the second is fetched.
  CALL FIND_INT2          ;
  POP AF                  ; The first parameter is restored before returning.
  RET                     ;

; THE 'FIND INTEGERS' SUBROUTINE
;
; Used by the routines at BEEP, STR_DATA, STR_ALTER, CO_TEMP_1, BORDER, CIRCLE and read_in.
;
; The 'last value' on the calculator stack is fetched and compressed into a single register or a register pair by entering at FIND_INT1 and FIND_INT2 respectively.
FIND_INT1:
  CALL FP_TO_A            ; Fetch the 'last value'.
  JR FIND_I_1             ; Jump forward.
; This entry point is used by the routines at BEEP, SAVE_ETC, LIST, RESTORE, RANDOMIZE, GO_TO, TWO_PARAM, CLEAR, PAUSE, PR_ITEM_1, INT_EXP1, f_in, peek and usr_no.
FIND_INT2:
  CALL FP_TO_BC           ; Fetch the 'last value'.
FIND_I_1:
  JR C,REPORT_B_2         ; In both cases overflow is indicated by a set carry flag.
  RET Z                   ; Return with all positive numbers that are in range.
; This entry point is used by the routines at PO_TV_2, GO_TO, TWO_PARAM and read_in.
;
; Report B - Integer out of range.
REPORT_B_2:
  RST $08                 ; Call the error handling routine.
  DEFB $0A                ;

; THE 'RUN' COMMAND ROUTINE
;
; The address of this routine is found in the parameter table.
;
; The parameter of the RUN command is passed to NEWPPC by calling GO_TO. The operations of 'RESTORE 0' and 'CLEAR 0' are then performed before a return is made.
RUN:
  CALL GO_TO              ; Set NEWPPC as required.
  LD BC,$0000             ; Now perform a 'RESTORE 0'.
  CALL REST_RUN           ;
  JR CLEAR_RUN            ; Exit via the CLEAR command routine.

; THE 'CLEAR' COMMAND ROUTINE
;
; The address of this routine is found in the parameter table.
;
; This routine allows for the variables area to be cleared, the display area cleared and RAMTOP moved. In consequence of the last operation the machine stack is rebuilt thereby having the effect of also clearing the GO SUB stack.
CLEAR:
  CALL FIND_INT2          ; Fetch the operand - using zero by default.
; This entry point is used by the routine at RUN.
CLEAR_RUN:
  LD A,B                  ; Jump forward if the operand is other than zero. When called from RUN there is no jump.
  OR C                    ;
  JR NZ,CLEAR_1           ;
  LD BC,(RAMTOP)          ; If zero use the existing value in RAMTOP.
CLEAR_1:
  PUSH BC                 ; Save the value.
  LD DE,(VARS)            ; Next reclaim all the bytes of the present variables area (VARS to E-LINE-1).
  LD HL,(E_LINE)          ;
  DEC HL                  ;
  CALL RECLAIM_1          ;
  CALL CLS                ; Clear the display area.
; The value in the BC register pair which will be used as RAMTOP is tested to ensure it is neither too low nor too high.
  LD HL,(STKEND)          ; The current value of STKEND is increased by 50 before being tested. This forms the lower limit.
  LD DE,$0032             ;
  ADD HL,DE               ;
  POP DE                  ;
  SBC HL,DE               ;
  JR NC,REPORT_M          ; RAMTOP will be too low.
  LD HL,(P_RAMT)          ; For the upper test the value for RAMTOP is tested against P-RAMT.
  AND A                   ;
  SBC HL,DE               ;
  JR NC,CLEAR_2           ; Jump forward if acceptable.
; Report M - RAMTOP no good.
REPORT_M:
  RST $08                 ; Call the error handling routine.
  DEFB $15                ;
; Continue with the CLEAR operation.
CLEAR_2:
  EX DE,HL                ; Now the value can actually be passed to RAMTOP.
  LD (RAMTOP),HL          ;
  POP DE                  ; Fetch the address of STMT_RET.
  POP BC                  ; Fetch the 'error address'.
  LD (HL),$3E             ; Enter a GO SUB stack end marker.
  DEC HL                  ; Leave one location.
  LD SP,HL                ; Make the stack pointer point to an empty GO SUB stack.
  PUSH BC                 ; Next pass the 'error address' to the stack and save its address in ERR-SP.
  LD (ERR_SP),SP          ;
  EX DE,HL                ; An indirect return is now made to STMT_RET.
  JP (HL)                 ;
; Note: when the routine is called from RUN the values of NEWPPC and NSPPC will have been affected and no statements coming after RUN can ever be found before the jump is taken.

; THE 'GO SUB' COMMAND ROUTINE
;
; The address of this routine is found in the parameter table.
;
; The present value of PPC and the incremented value of SUBPPC are stored on the GO SUB stack.
GO_SUB:
  POP DE                  ; Save the address - STMT_RET.
  LD H,(IY+$0D)           ; Fetch the statement number (SUBPPC) and increment it.
  INC H                   ;
  EX (SP),HL              ; Exchange the 'error address' with the statement number.
  INC SP                  ; Reclaim the use of a location.
  LD BC,(PPC)             ; Next save the present line number (PPC).
  PUSH BC                 ;
  PUSH HL                 ; Return the 'error address' to the machine stack and reset ERR-SP to point to it.
  LD (ERR_SP),SP          ;
  PUSH DE                 ; Return the address STMT_RET.
  CALL GO_TO              ; Now set NEWPPC and NSPPC to the required values.
  LD BC,$0014             ; But before making the jump make a test for room.
; This routine continues into TEST_ROOM.

; THE 'TEST-ROOM' SUBROUTINE
;
; Used by the routines at LD_CONTRL, ED_EDIT, ONE_SPACE, FREE_MEM and TEST_5_SP.
;
; The routine at GO_SUB continues here.
;
; A series of tests is performed to ensure that there is sufficient free memory available for the task being undertaken.
;
;   BC Size of the required space
; O:HL STKEND+BC+80-SP
TEST_ROOM:
  LD HL,(STKEND)          ; Increase the value taken from STKEND by the value carried into the routine by the BC register pair.
  ADD HL,BC               ;
  JR C,REPORT_4           ; Jump forward if the result is over +FFFF.
  EX DE,HL                ; Try it again allowing for a further eighty bytes.
  LD HL,$0050             ;
  ADD HL,DE               ;
  JR C,REPORT_4           ;
  SBC HL,SP               ; Finally test the value against the address of the machine stack.
  RET C                   ; Return if satisfactory.
; This entry point is used by the routines at GET_HLxDE and DIM.
;
; Report 4 - Out of memory.
REPORT_4:
  LD L,$03                ; This is a 'run-time' error and the error marker is not to be used.
  JP ERROR_3              ;

; THE 'FREE MEMORY' SUBROUTINE
;
; There is no BASIC command 'FRE' in the Spectrum but there is a subroutine for performing such a task.
;
; An estimate of the amount of free space can be found at any time by using 'PRINT 65536-USR 7962'.
;
; O:BC STKEND+80-SP (free space * -1)
FREE_MEM:
  LD BC,$0000             ; Do not allow any overhead.
  CALL TEST_ROOM          ; Make the test and pass the result to the BC register before returning.
  LD B,H                  ;
  LD C,L                  ;
  RET                     ;

; THE 'RETURN' COMMAND ROUTINE
;
; The address of this routine is found in the parameter table.
;
; The line number and the statement number that are to be made the object of a 'return' are fetched from the GO SUB stack.
RETURN:
  POP BC                  ; Fetch the address - STMT_RET.
  POP HL                  ; Fetch the 'error address'.
  POP DE                  ; Fetch the last entry on the GO SUB stack.
  LD A,D                  ; The entry is tested to see if it is the GO SUB stack end marker.
  CP $3E                  ;
  JR Z,REPORT_7           ; Jump if it is.
  DEC SP                  ; The full entry uses three locations only.
  EX (SP),HL              ; Exchange the statement number with the 'error address'.
  EX DE,HL                ; Move the statement number.
  LD (ERR_SP),SP          ; Reset the error pointer (ERR-SP).
  PUSH BC                 ; Replace the address STMT_RET.
  JP GO_TO_2              ; Jump back to change NEWPPC and NSPPC.
; Report 7 - RETURN without GOSUB.
REPORT_7:
  PUSH DE                 ; Replace the end marker and the 'error address'.
  PUSH HL                 ;
  RST $08                 ; Call the error handling routine.
  DEFB $06                ;

; THE 'PAUSE' COMMAND ROUTINE
;
; The address of this routine is found in the parameter table.
;
; The period of the pause is determined by counting the number of maskable interrupts as they occur every 1/50th of a second.
;
; A pause is finished either after the appropriate number of interrupts or by the system variable FLAGS indicating that a key has been pressed.
PAUSE:
  CALL FIND_INT2          ; Fetch the operand.
PAUSE_1:
  HALT                    ; Wait for a maskable interrupt.
  DEC BC                  ; Decrease the counter.
  LD A,B                  ; If the counter is thereby reduced to zero the pause has come to an end.
  OR C                    ;
  JR Z,PAUSE_END          ;
  LD A,B                  ; If the operand was zero BC will now hold +FFFF and this value will be returned to zero. Jump with all other operand values.
  AND C                   ;
  INC A                   ;
  JR NZ,PAUSE_2           ;
  INC BC                  ;
PAUSE_2:
  BIT 5,(IY+$01)          ; Jump back unless a key has been pressed (bit 5 of FLAGS set).
  JR Z,PAUSE_1            ;
; The period of the pause has now finished.
PAUSE_END:
  RES 5,(IY+$01)          ; Signal 'no key pressed' (reset bit 5 of FLAGS).
  RET                     ; Now return - to STMT_RET.

; THE 'BREAK-KEY' SUBROUTINE
;
; Used by the routines at COPY_LINE and STMT_RET.
;
; This subroutine is called in several instances to read the BREAK key. The carry flag is returned reset only if the SHIFT and the BREAK keys are both being pressed.
BREAK_KEY:
  LD A,$7F                ; Form the port address +7FFE and read in a byte.
  IN A,($FE)              ;
  RRA                     ; Examine only bit 0 by shifting it into the carry position.
  RET C                   ; Return if the BREAK key is not being pressed.
  LD A,$FE                ; Form the port address +FEFE and read in a byte.
  IN A,($FE)              ;
  RRA                     ; Again examine bit 0.
  RET                     ; Return with carry reset if both keys are being pressed.

; THE 'DEF FN' COMMAND ROUTINE
;
; The address of this routine is found in the parameter table.
;
; During syntax checking a DEF FN statement is checked to ensure that it has the correct form. Space is also made available for the result of evaluating the function.
;
; But in 'run-time' a DEF FN statement is passed by.
;
; A Code of the next character in the statement
DEF_FN:
  CALL SYNTAX_Z           ; Jump forward if checking syntax.
  JR Z,DEF_FN_1           ;
  LD A,$CE                ; Otherwise pass by the 'DEF FN' statement.
  JP PASS_BY              ;
; First consider the variable of the function.
DEF_FN_1:
  SET 6,(IY+$01)          ; Signal 'a numeric variable' (set bit 6 of FLAGS).
  CALL ALPHA              ; Check that the present code is a letter.
  JR NC,DEF_FN_4          ; Jump forward if not.
  RST $20                 ; Fetch the next character.
  CP "$"                  ; Jump forward unless it is a '$'.
  JR NZ,DEF_FN_2          ;
  RES 6,(IY+$01)          ; Reset bit 6 of FLAGS as it is a string variable.
  RST $20                 ; Fetch the next character.
DEF_FN_2:
  CP "("                  ; A '(' must follow the variable's name.
  JR NZ,DEF_FN_7          ;
  RST $20                 ; Fetch the next character.
  CP ")"                  ; Jump forward if it is a ')' as there are no parameters of the function.
  JR Z,DEF_FN_6           ;
; A loop is now entered to deal with each parameter in turn.
DEF_FN_3:
  CALL ALPHA              ; The present code must be a letter.
DEF_FN_4:
  JP NC,REPORT_C          ;
  EX DE,HL                ; Save the pointer in DE.
  RST $20                 ; Fetch the next character.
  CP "$"                  ; Jump forward unless it is a '$'.
  JR NZ,DEF_FN_5          ;
  EX DE,HL                ; Otherwise save the new pointer in DE instead.
  RST $20                 ; Fetch the next character.
DEF_FN_5:
  EX DE,HL                ; Move the pointer to the last character of the name to the HL register pair.
  LD BC,$0006             ; Now make six locations after that last character and enter a 'number marker' into the first of the new locations.
  CALL MAKE_ROOM          ;
  INC HL                  ;
  INC HL                  ;
  LD (HL),$0E             ;
  CP ","                  ; If the present character is a ',' then jump back as there should be a further parameter; otherwise jump out of the loop.
  JR NZ,DEF_FN_6          ;
  RST $20                 ;
  JR DEF_FN_3             ;
; Next the definition of the function is considered.
DEF_FN_6:
  CP ")"                  ; Check that the ')' does exist.
  JR NZ,DEF_FN_7          ;
  RST $20                 ; The next character is fetched.
  CP "="                  ; It must be an '='.
  JR NZ,DEF_FN_7          ;
  RST $20                 ; Fetch the next character.
  LD A,(FLAGS)            ; Save the nature - numeric or string - of the variable (bit 6 of FLAGS).
  PUSH AF                 ;
  CALL SCANNING           ; Now consider the definition as an expression.
  POP AF                  ; Fetch the nature of the variable and check that it is of the same type as found for the definition (specified by bit 6 of FLAGS).
  XOR (IY+$01)            ;
  AND $40                 ;
DEF_FN_7:
  JP NZ,REPORT_C          ; Give an error report if it is required.
  CALL CHECK_END          ; Exit via CHECK_END (thereby moving on to consider the next statement in the line).
; This routine continues into UNSTACK_Z.

; THE 'UNSTACK-Z' SUBROUTINE
;
; Used by the routines at PRINT_CR, PR_ITEM_1, STR_ALTER and CO_TEMP_1.
;
; The routine at DEF_FN continues here.
;
; This subroutine is called in several instances in order to 'return early' from a subroutine when checking syntax. The reason for this is to avoid actually printing characters or passing values to/from the calculator stack.
UNSTACK_Z:
  CALL SYNTAX_Z           ; Is syntax being checked?
  POP HL                  ; Fetch the return address but ignore it in 'syntax-time'.
  RET Z                   ;
  JP (HL)                 ; In 'run-time' make a simple return to the calling routine.

; THE 'LPRINT and PRINT' COMMAND ROUTINES
;
; The address of this routine is found in the parameter table.
;
; The appropriate channel is opened as necessary and the items to be printed are considered in turn.
LPRINT:
  LD A,$03                ; Prepare to open channel 'P'.
  JR PRINT_1              ; Jump forward.
; The address of this entry point is found in the parameter table.
PRINT:
  LD A,$02                ; Prepare to open channel 'S'.
PRINT_1:
  CALL SYNTAX_Z           ; Unless syntax is being checked open a channel.
  CALL NZ,CHAN_OPEN       ;
  CALL TEMPS              ; Set the temporary colour system variables.
  CALL PRINT_2            ; Call the print controlling subroutine.
  CALL CHECK_END          ; Move on to consider the next statement (via CHECK_END if checking syntax).
  RET                     ;

; THE 'PRINT CONTROLLING' SUBROUTINE
;
; This subroutine is called by the LPRINT and INPUT command routines.
PRINT_2:
  RST $18                 ; Get the first character.
  CALL PR_END_Z           ; Jump forward if already at the end of the item list.
  JR Z,PRINT_4            ;
; Now enter a loop to deal with the 'position controllers' and the print items.
PRINT_3:
  CALL PR_POSN_1          ; Deal with any consecutive position controllers.
  JR Z,PRINT_3            ;
  CALL PR_ITEM_1          ; Deal with a single print item.
  CALL PR_POSN_1          ; Check for further position controllers and print items until there are none left.
  JR Z,PRINT_3            ;
PRINT_4:
  CP ")"                  ; Return now if the present character is a ')'; otherwise consider performing a 'carriage return'.
  RET Z                   ;
; This routine continues into PRINT_CR.

; THE 'PRINT A CARRIAGE RETURN' SUBROUTINE
;
; Used by the routine at PR_POSN_1.
;
; The routine at PRINT_2 continues here.
PRINT_CR:
  CALL UNSTACK_Z          ; Return if checking syntax.
  LD A,$0D                ; Print a carriage return character and then return.
  RST $10                 ;
  RET                     ;

; THE 'PRINT ITEMS' SUBROUTINE
;
; This subroutine is called from the PRINT_2 and INPUT routines.
;
; The various types of print item are identified and printed.
PR_ITEM_1:
  RST $18                 ; The first character is fetched.
  CP $AC                  ; Jump forward unless it is an 'AT'.
  JR NZ,PR_ITEM_2         ;
; Now deal with an 'AT'.
  CALL NEXT_2NUM          ; The two parameters are transferred to the calculator stack.
  CALL UNSTACK_Z          ; Return now if checking syntax.
  CALL STK_TO_BC          ; The parameters are compressed into the BC register pair.
  LD A,$16                ; The A register is loaded with the AT control character before the jump is taken.
  JR PR_AT_TAB            ;
; Next look for a 'TAB'.
PR_ITEM_2:
  CP $AD                  ; Jump forward unless it is a 'TAB'.
  JR NZ,PR_ITEM_3         ;
; Now deal with a 'TAB'.
  RST $20                 ; Get the next character.
  CALL CLASS_06           ; Transfer one parameter to the calculator stack.
  CALL UNSTACK_Z          ; Return now if checking syntax.
  CALL FIND_INT2          ; The value is compressed into the BC register pair.
  LD A,$17                ; The A register is loaded with the TAB control character.
; The 'AT' and the 'TAB' print items are printed by making three calls to PRINT_A_1.
PR_AT_TAB:
  RST $10                 ; Print the control character.
  LD A,C                  ; Follow it with the first value.
  RST $10                 ;
  LD A,B                  ; Finally print the second value, then return.
  RST $10                 ;
  RET                     ;
; Next consider embedded colour items.
PR_ITEM_3:
  CALL CO_TEMP_3          ; Return with carry reset if colour items were found. Continue if none were found.
  RET NC                  ;
  CALL STR_ALTER          ; Next consider if the stream is to be changed.
  RET NC                  ; Continue unless it was altered.
; The print item must now be an expression, either numeric or string.
  CALL SCANNING           ; Evaluate the expression but return now if checking syntax.
  CALL UNSTACK_Z          ;
  BIT 6,(IY+$01)          ; Test for the nature of the expression (bit 6 of FLAGS).
  CALL Z,STK_FETCH        ; If it is a string then fetch the necessary parameters; but if it is numeric then exit via PRINT_FP.
  JP NZ,PRINT_FP          ;
; A loop is now set up to deal with each character in turn of the string.
PR_STRING:
  LD A,B                  ; Return now if there are no characters remaining in the string; otherwise decrease the counter.
  OR C                    ;
  DEC BC                  ;
  RET Z                   ;
  LD A,(DE)               ; Fetch the code and increment the pointer.
  INC DE                  ;
  RST $10                 ; The code is printed and a jump taken to consider any further characters.
  JR PR_STRING            ;

; THE 'END OF PRINTING' SUBROUTINE
;
; Used by the routines at PRINT_2 and PR_POSN_1.
;
; The zero flag will be set if no further printing is to be done.
;
;   A Code of the current character
; O:F Zero flag set if the character is ')', ':' or a carriage return
PR_END_Z:
  CP ")"                  ; Return now if the character is a ')'.
  RET Z                   ;
; This entry point is used by the routine at SAVE_ETC.
PR_ST_END:
  CP $0D                  ; Return now if the character is a 'carriage return'.
  RET Z                   ;
  CP ":"                  ; Make a final test against ':' before returning.
  RET                     ;

; THE 'PRINT POSITION' SUBROUTINE
;
; Used by the routines at PRINT_2 and INPUT.
;
; The various position controlling characters are considered by this subroutine.
;
; O:F Zero flag set if a position controlling character is found
PR_POSN_1:
  RST $18                 ; Get the present character.
  CP ";"                  ; Jump forward if it is a ';'.
  JR Z,PR_POSN_3          ;
  CP ","                  ; Also jump forward with a character other than a ',', but do not actually print the character if checking syntax.
  JR NZ,PR_POSN_2         ;
  CALL SYNTAX_Z           ;
  JR Z,PR_POSN_3          ;
  LD A,$06                ; Load the A register with the 'comma' control code and print it, then jump forward.
  RST $10                 ;
  JR PR_POSN_3            ;
PR_POSN_2:
  CP "'"                  ; Is it a '''?
  RET NZ                  ; Return now (with the zero flag reset) if not any of the position controllers.
  CALL PRINT_CR           ; Print 'carriage return' unless checking syntax.
PR_POSN_3:
  RST $20                 ; Fetch the next character.
  CALL PR_END_Z           ; If not at the end of a print statement then jump forward.
  JR NZ,PR_POSN_4         ;
  POP BC                  ; Otherwise drop the return address from the stack.
PR_POSN_4:
  CP A                    ; Set the zero flag and return.
  RET                     ;

; THE 'ALTER STREAM' SUBROUTINE
;
; Used by the routines at LIST and PR_ITEM_1.
;
; This subroutine is called whenever there is the need to consider whether the user wishes to use a different stream.
;
;   A Code of the current character
; O:F Carry flag set if the character is not '#'
STR_ALTER:
  CP "#"                  ; Unless the present character is a '#' return with the carry flag set.
  SCF                     ;
  RET NZ                  ;
  RST $20                 ; Advance CH-ADD.
  CALL CLASS_06           ; Pass the parameter to the calculator stack.
  AND A                   ; Clear the carry flag.
  CALL UNSTACK_Z          ; Return now if checking syntax.
  CALL FIND_INT1          ; The value is passed to the A register.
  CP $10                  ; Give report O if the value is over +0F.
  JP NC,REPORT_O          ;
  CALL CHAN_OPEN          ; Use the channel for the stream in question.
  AND A                   ; Clear the carry flag and return.
  RET                     ;

; THE 'INPUT' COMMAND ROUTINE
;
; The address of this routine is found in the parameter table.
;
; This routine allows for values entered from the keyboard to be assigned to variables. It is also possible to have print items embedded in the INPUT statement and these items are printed in the lower part of the display.
INPUT:
  CALL SYNTAX_Z           ; Jump forward if syntax is being checked.
  JR Z,INPUT_1            ;
  LD A,$01                ; Open channel 'K'.
  CALL CHAN_OPEN          ;
  CALL CLS_LOWER          ; The lower part of the display is cleared.
INPUT_1:
  LD (IY+$02),$01         ; Signal that the lower screen is being handled (set bit 0 of TV-FLAG and reset all other bits).
  CALL IN_ITEM_1          ; Call the subroutine to deal with the INPUT items.
  CALL CHECK_END          ; Move on to the next statement if checking syntax.
  LD BC,(S_POSN)          ; Fetch the current print position (S-POSN).
  LD A,(DF_SZ)            ; Jump forward if the current position is above the lower screen (see DF-SZ).
  CP B                    ;
  JR C,INPUT_2            ;
  LD C,$21                ; Otherwise set the print position to the top of the lower screen.
  LD B,A                  ;
INPUT_2:
  LD (S_POSN),BC          ; Reset S-POSN.
  LD A,$19                ; Now set the scroll counter (SCR-CT).
  SUB B                   ;
  LD (SCR_CT),A           ;
  RES 0,(IY+$02)          ; Signal 'main screen' (reset bit 0 of TV-FLAG).
  CALL CL_SET             ; Set the system variables and exit via CLS_LOWER.
  JP CLS_LOWER            ;
; The INPUT items and embedded PRINT items are dealt with in turn by the following loop.
IN_ITEM_1:
  CALL PR_POSN_1          ; Consider first any position control characters.
  JR Z,IN_ITEM_1          ;
  CP "("                  ; Jump forward if the present character is not a '('.
  JR NZ,IN_ITEM_2         ;
  RST $20                 ; Fetch the next character.
  CALL PRINT_2            ; Now call the PRINT command routine to handle the items inside the brackets.
  RST $18                 ; Fetch the present character.
  CP ")"                  ; Give report C unless the character is a ')'.
  JP NZ,REPORT_C          ;
  RST $20                 ; Fetch the next character and jump forward to see if there are any further INPUT items.
  JP IN_NEXT_2            ;
; Now consider whether INPUT LINE is being used.
IN_ITEM_2:
  CP $CA                  ; Jump forward if it is not 'LINE'.
  JR NZ,IN_ITEM_3         ;
  RST $20                 ; Advance CH-ADD.
  CALL CLASS_01           ; Determine the destination address for the variable.
  SET 7,(IY+$37)          ; Signal 'using INPUT LINE' (set bit 7 of FLAGX).
  BIT 6,(IY+$01)          ; Give report C unless using a string variable (bit 6 of FLAGS reset).
  JP NZ,REPORT_C          ;
  JR IN_PROMPT            ; Jump forward to issue the prompt message.
; Proceed to handle simple INPUT variables.
IN_ITEM_3:
  CALL ALPHA              ; Jump to consider going round the loop again if the present character is not a letter.
  JP NC,IN_NEXT_1         ;
  CALL CLASS_01           ; Determine the destination address for the variable.
  RES 7,(IY+$37)          ; Signal 'not INPUT LINE' (reset bit 7 of FLAGX).
; The prompt message is now built up in the work space.
IN_PROMPT:
  CALL SYNTAX_Z           ; Jump forward if only checking syntax.
  JP Z,IN_NEXT_2          ;
  CALL SET_WORK           ; The work space is set to null.
  LD HL,FLAGX             ; This is FLAGX.
  RES 6,(HL)              ; Signal 'string result'.
  SET 5,(HL)              ; Signal 'INPUT mode'.
  LD BC,$0001             ; Allow the prompt message only a single location.
  BIT 7,(HL)              ; Jump forward if using 'LINE'.
  JR NZ,IN_PR_2           ;
  LD A,(FLAGS)            ; Jump forward if awaiting a numeric entry (bit 6 of FLAGS set).
  AND $40                 ;
  JR NZ,IN_PR_1           ;
  LD C,$03                ; A string entry will need three locations.
IN_PR_1:
  OR (HL)                 ; Bit 6 of FLAGX will become set for a numeric entry.
  LD (HL),A               ;
IN_PR_2:
  RST $30                 ; The required number of locations is made available.
  LD (HL),$0D             ; A 'carriage return' goes into the last location.
  LD A,C                  ; Test bit 6 of the C register and jump forward if only one location was required.
  RRCA                    ;
  RRCA                    ;
  JR NC,IN_PR_3           ;
  LD A,"\""               ; A 'double quotes' character goes into the first and second locations.
  LD (DE),A               ;
  DEC HL                  ;
  LD (HL),A               ;
IN_PR_3:
  LD (K_CUR),HL           ; The position of the cursor (K-CUR) can now be saved.
; In the case of INPUT LINE the EDITOR can be called without further preparation but for other types of INPUT the error stack has to be changed so as to trap errors.
  BIT 7,(IY+$37)          ; Jump forward with 'INPUT LINE' (bit 7 of FLAGX set).
  JR NZ,IN_VAR_3          ;
  LD HL,(CH_ADD)          ; Save the current values of CH-ADD and ERR-SP on the machine stack.
  PUSH HL                 ;
  LD HL,(ERR_SP)          ;
  PUSH HL                 ;
IN_VAR_1:
  LD HL,IN_VAR_1          ; This will be the 'return point' in case of errors.
  PUSH HL                 ;
  BIT 4,(IY+$30)          ; Only change the error stack pointer (ERR-SP) if using channel 'K' (bit 4 of FLAGS2 set).
  JR Z,IN_VAR_2           ;
  LD (ERR_SP),SP          ;
IN_VAR_2:
  LD HL,(WORKSP)          ; Set HL to the start of the INPUT line (WORKSP) and remove any floating-point forms. (There will not be any except perhaps after an error.)
  CALL REMOVE_FP          ;
  LD (IY+$00),$FF         ; Signal 'no error yet' by resetting ERR-NR.
  CALL EDITOR             ; Now get the INPUT and with the syntax/run flag (bit 7 of FLAGS) indicating syntax, check the INPUT for errors; jump if in order; return to IN_VAR_1 if not.
  RES 7,(IY+$01)          ;
  CALL IN_ASSIGN          ;
  JR IN_VAR_4             ;
IN_VAR_3:
  CALL EDITOR             ; Get a 'LINE'.
; All the system variables have to be reset before the actual assignment of a value can be made.
IN_VAR_4:
  LD (IY+$22),$00         ; The cursor address (K-CUR) is reset.
  CALL IN_CHAN_K          ; The jump is taken if using other than channel 'K'.
  JR NZ,IN_VAR_5          ;
  CALL ED_COPY            ; The input-line is copied to the display and the position in ECHO-E made the current position in the lower screen.
  LD BC,(ECHO_E)          ;
  CALL CL_SET             ;
IN_VAR_5:
  LD HL,FLAGX             ; This is FLAGX.
  RES 5,(HL)              ; Signal 'edit mode'.
  BIT 7,(HL)              ; Jump forward if handling an INPUT LINE.
  RES 7,(HL)              ;
  JR NZ,IN_VAR_6          ;
  POP HL                  ; Drop the address IN-VAR-1.
  POP HL                  ; Reset the ERR-SP to its original address.
  LD (ERR_SP),HL          ;
  POP HL                  ; Save the original CH-ADD address in X-PTR.
  LD (X_PTR),HL           ;
  SET 7,(IY+$01)          ; Now with the syntax/run flag (bit 7 of FLAGS) indicating 'run' make the assignment.
  CALL IN_ASSIGN          ;
  LD HL,(X_PTR)           ; Restore the original address to CH-ADD and clear X-PTR.
  LD (IY+$26),$00         ;
  LD (CH_ADD),HL          ;
  JR IN_NEXT_2            ; Jump forward to see if there are further INPUT items.
IN_VAR_6:
  LD HL,(STKBOT)          ; The length of the 'LINE' in the work space is found (STKBOT-WORKSP-1).
  LD DE,(WORKSP)          ;
  SCF                     ;
  SBC HL,DE               ;
  LD B,H                  ; DE points to the start and BC holds the length.
  LD C,L                  ;
  CALL STK_STO            ; These parameters are stacked and the actual assignment made.
  CALL LET                ;
  JR IN_NEXT_2            ; Also jump forward to consider further items.
; Further items in the INPUT statement are considered.
IN_NEXT_1:
  CALL PR_ITEM_1          ; Handle any print items.
IN_NEXT_2:
  CALL PR_POSN_1          ; Handle any position controllers.
  JP Z,IN_ITEM_1          ; Go around the loop again if there are further items; otherwise return.
  RET                     ;

; THE 'IN-ASSIGN' SUBROUTINE
;
; Used by the routine at INPUT.
;
; This subroutine is called twice for each INPUT value: once with the syntax/run flag reset (syntax) and once with it set (run).
IN_ASSIGN:
  LD HL,(WORKSP)          ; Set CH-ADD to point to the first location of the work space (WORKSP) and fetch the character.
  LD (CH_ADD),HL          ;
  RST $18                 ;
  CP $E2                  ; Is it a 'STOP'?
  JR Z,IN_STOP            ; Jump if it is.
  LD A,(FLAGX)            ; Otherwise pick up FLAGX and make the assignment of the 'value' to the variable.
  CALL VAL_FET_2          ;
  RST $18                 ; Get the present character and check it is a 'carriage return'.
  CP $0D                  ;
  RET Z                   ; Return if it is.
; Report C - Nonsense in BASIC.
  RST $08                 ; Call the error handling routine.
  DEFB $0B                ;
; Come here if the INPUT line starts with 'STOP'.
IN_STOP:
  CALL SYNTAX_Z           ; But do not give the error report on the syntax-pass.
  RET Z                   ;
; Report H - STOP in INPUT.
  RST $08                 ; Call the error handling routine.
  DEFB $10                ;

; THE 'IN-CHAN-K' SUBROUTINE
;
; Used by the routine at INPUT.
;
; O:F Zero flag set if channel 'K' (keyboard) is being used
IN_CHAN_K:
  LD HL,(CURCHL)          ; The base address of the channel information for the current channel (CURCHL) is fetched and the channel code compared to the character 'K'.
  INC HL                  ;
  INC HL                  ;
  INC HL                  ;
  INC HL                  ;
  LD A,(HL)               ;
  CP "K"                  ;
  RET                     ; Return afterwards.

; THE 'COLOUR ITEM' ROUTINES
;
; This set of routines can be readily divided into two parts:
;
; * i. The embedded colour item' handler.
; * ii. The 'colour system variable' handler.
;
; i. Embedded colour items are handled by calling PRINT_A_1 as required.
;
; A loop is entered to handle each item in turn. The entry point is at CO_TEMP_2.
CO_TEMP_1:
  RST $20                 ; Consider the next character in the BASIC statement.
; This entry point is used by the routine at CLASS_09.
CO_TEMP_2:
  CALL CO_TEMP_3          ; Jump forward to see if the present code represents an embedded 'temporary' colour item. Return carry set if not a colour item.
  RET C                   ;
  RST $18                 ; Fetch the present character.
  CP ","                  ; Jump back if it is either a ',' or a ';'; otherwise there has been an error.
  JR Z,CO_TEMP_1          ;
  CP ";"                  ;
  JR Z,CO_TEMP_1          ;
  JP REPORT_C             ; Exit via 'report C'.
; This entry point is used by the routine at PR_ITEM_1.
CO_TEMP_3:
  CP $D9                  ; Return with the carry flag set if the code is not in the range +D9 to +DE (INK to OVER).
  RET C                   ;
  CP $DF                  ;
  CCF                     ;
  RET C                   ;
  PUSH AF                 ; The colour item code is preserved whilst CH-ADD is advanced to address the parameter that follows it.
  RST $20                 ;
  POP AF                  ;
; This entry point is used by the routine at CLASS_07.
;
; The colour item code and the parameter are now 'printed' by calling PRINT_A_1 on two occasions.
CO_TEMP_4:
  SUB $C9                 ; The token range (+D9 to +DE) is reduced to the control character range (+10 to +15).
  PUSH AF                 ; The control character code is preserved whilst the parameter is moved to the calculator stack.
  CALL CLASS_06           ;
  POP AF                  ;
  AND A                   ; A return is made at this point if syntax is being checked.
  CALL UNSTACK_Z          ;
  PUSH AF                 ; The control character code is preserved whilst the parameter is moved to the D register.
  CALL FIND_INT1          ;
  LD D,A                  ;
  POP AF                  ;
  RST $10                 ; The control character is sent out.
  LD A,D                  ; Then the parameter is fetched and sent out before returning.
  RST $10                 ;
  RET                     ;
; This entry point is used by the routine at PO_TV_2.
;
; ii. The colour system variables - ATTR-T, MASK-T and P-FLAG - are altered as required. On entry the control character code is in the A register and the parameter is in the D register.
;
; Note that all changes are to the 'temporary' system variables.
CO_TEMP_5:
  SUB $11                 ; Reduce the range and jump forward with INK and PAPER.
  ADC A,$00               ;
  JR Z,CO_TEMP_7          ;
  SUB $02                 ; Reduce the range once again and jump forward with FLASH and BRIGHT.
  ADC A,$00               ;
  JR Z,CO_TEMP_C          ;
; The colour control code will now be +01 for INVERSE and +02 for OVER and the system variable P-FLAG is altered accordingly.
  CP $01                  ; Prepare to jump with OVER.
  LD A,D                  ; Fetch the parameter.
  LD B,$01                ; Prepare the mask for OVER.
  JR NZ,CO_TEMP_6         ; Now jump.
  RLCA                    ; Bit 2 of the A register is to be reset for INVERSE 0 and set for INVERSE 1; the mask is to have bit 2 set.
  RLCA                    ;
  LD B,$04                ;
CO_TEMP_6:
  LD C,A                  ; Save the A register whilst the range is tested.
  LD A,D                  ; The correct range for INVERSE and OVER is only '0-1'.
  CP $02                  ;
  JR NC,REPORT_K          ;
  LD A,C                  ; Restore the A register.
  LD HL,P_FLAG            ; It is P-FLAG that is to be changed.
  JR CO_CHANGE            ; Exit via CO_CHANGE and alter P-FLAG using B as a mask, i.e. bit 0 for OVER and bit 2 for INVERSE.
; PAPER and INK are dealt with by the following routine. On entry the carry flag is set for INK.
CO_TEMP_7:
  LD A,D                  ; Fetch the parameter.
  LD B,$07                ; Prepare the mask for INK.
  JR C,CO_TEMP_8          ; Jump forward with INK.
  RLCA                    ; Multiply the parameter for PAPER by eight.
  RLCA                    ;
  RLCA                    ;
  LD B,$38                ; Prepare the mask for PAPER.
CO_TEMP_8:
  LD C,A                  ; Save the parameter in the C register whilst the range of the parameter is tested.
  LD A,D                  ; Fetch the original value.
  CP $0A                  ; Only allow PAPER/INK a range of '0' to '9'.
  JR C,CO_TEMP_9          ;
; This entry point is used by the routine at BORDER.
;
; Report K - Invalid colour.
REPORT_K:
  RST $08                 ; Call the error handling routine.
  DEFB $13                ;
; Continue to handle PAPER and INK.
CO_TEMP_9:
  LD HL,ATTR_T            ; Prepare to alter ATTR-T, MASK-T and P-FLAG.
  CP $08                  ; Jump forward with PAPER/INK '0' to '7'.
  JR C,CO_TEMP_B          ;
  LD A,(HL)               ; Fetch the current value of ATTR-T and use it unchanged, by jumping forward, with PAPER/INK '8'.
  JR Z,CO_TEMP_A          ;
  OR B                    ; But for PAPER/INK '9' the PAPER and INK colours have to be black and white.
  CPL                     ;
  AND $24                 ;
  JR Z,CO_TEMP_A          ; Jump for black INK/PAPER, but continue for white INK/PAPER.
  LD A,B                  ;
CO_TEMP_A:
  LD C,A                  ; Move the value to the C register.
; The mask (B) and the value (C) are now used to change ATTR-T.
CO_TEMP_B:
  LD A,C                  ; Move the value.
  CALL CO_CHANGE          ; Now change ATTR-T as needed.
; Next MASK-T is considered.
  LD A,$07                ; The bits of MASK-T are set only when using PAPER/INK '8' or '9'.
  CP D                    ;
  SBC A,A                 ;
  CALL CO_CHANGE          ; Now change MASK-T as needed.
; Next P-FLAG is considered.
  RLCA                    ; The appropriate mask is built up in the B register in order to change bits 4 and 6 as necessary.
  RLCA                    ;
  AND $50                 ;
  LD B,A                  ;
  LD A,$08                ; The bits of P-FLAG are set only when using PAPER/INK '9'. Continue into CO_CHANGE to manipulate P-FLAG.
  CP D                    ;
  SBC A,A                 ;
; The following subroutine is used to 'impress' upon a system variable the 'nature' of the bits in the A register. The B register holds a mask that shows which bits are to be 'copied over' from A to (HL).
CO_CHANGE:
  XOR (HL)                ; The bits, specified by the mask in the B register, are changed in the value and the result goes to form the system variable.
  AND B                   ;
  XOR (HL)                ;
  LD (HL),A               ;
  INC HL                  ; Move on to address the next system variable.
  LD A,B                  ; Return with the mask in the A register.
  RET                     ;
; FLASH and BRIGHT are handled by the following routine.
CO_TEMP_C:
  SBC A,A                 ; The zero flag will be set for BRIGHT.
  LD A,D                  ; The parameter is fetched and rotated.
  RRCA                    ;
  LD B,$80                ; Prepare the mask for FLASH.
  JR NZ,CO_TEMP_D         ; Jump forward with FLASH.
  RRCA                    ; Rotate an extra time and prepare the mask for BRIGHT.
  LD B,$40                ;
CO_TEMP_D:
  LD C,A                  ; Save the value in the C register.
  LD A,D                  ; Fetch the parameter and test its range; only '0', '1' and '8' are allowable.
  CP $08                  ;
  JR Z,CO_TEMP_E          ;
  CP $02                  ;
  JR NC,REPORT_K          ;
; The system variable ATTR-T can now be altered.
CO_TEMP_E:
  LD A,C                  ; Fetch the value.
  LD HL,ATTR_T            ; This is ATTR-T.
  CALL CO_CHANGE          ; Now change the system variable.
; The value in MASK-T is now considered.
  LD A,C                  ; The value is fetched anew.
  RRCA                    ; The set bit of FLASH/BRIGHT '8' (bit 3) is moved to bit 7 (for FLASH) or bit 6 (for BRIGHT).
  RRCA                    ;
  RRCA                    ;
  JR CO_CHANGE            ; Exit via CO_CHANGE.

; THE 'BORDER' COMMAND ROUTINE
;
; The address of this routine is found in the parameter table.
;
; The parameter of the BORDER command is used with an 'OUT' command to actually alter the colour of the border. The parameter is then saved in the system variable BORDCR.
BORDER:
  CALL FIND_INT1          ; The parameter is fetched and its range is tested.
  CP $08                  ;
  JR NC,REPORT_K          ;
  OUT ($FE),A             ; The 'OUT' instruction is then used to set the border colour.
  RLCA                    ; The parameter is then multiplied by eight.
  RLCA                    ;
  RLCA                    ;
  BIT 5,A                 ; Is the border colour a 'light' colour?
  JR NZ,BORDER_1          ; Jump if so (the INK colour will be black).
  XOR $07                 ; Change the INK colour to white.
BORDER_1:
  LD (BORDCR),A           ; Set the system variable (BORDCR) as required and return.
  RET                     ;

; THE 'PIXEL ADDRESS' SUBROUTINE
;
; This subroutine is called by POINT_SUB and by PLOT. Is is entered with the co-ordinates of a pixel in the BC register pair and returns with HL holding the address of the display file byte which contains that pixel and A pointing to the position of the pixel within the byte.
;
;   B Pixel y-coordinate
;   C Pixel x-coordinate
; O:A C mod 8
; O:HL Display file address
PIXEL_ADD:
  LD A,$AF                ; Test that the y co-ordinate (in B) is not greater than 175.
  SUB B                   ;
  JP C,REPORT_B_3         ;
  LD B,A                  ; B now contains 175 minus y.
  AND A                   ; A holds b7b6b5b4b3b2b1b0, the bits of B.
  RRA                     ; And now 0b7b6b5b4b3b2b1.
  SCF                     ; Now 10b7b6b5b4b3b2.
  RRA                     ;
  AND A                   ; Now 010b7b6b5b4b3.
  RRA                     ;
  XOR B                   ; Finally 010b7b6b2b1b0, so that H becomes 64+8*INT(B/64)+(B mod 8), the high byte of the pixel address.
  AND %11111000           ;
  XOR B                   ;
  LD H,A                  ;
  LD A,C                  ; C contains x.
  RLCA                    ; A starts as c7c6c5c4c3c2c1c0 and becomes c4c3c2c1c0c7c6c5.
  RLCA                    ;
  RLCA                    ;
  XOR B                   ; Now c4c3b5b4b3c7c6c5.
  AND %11000111           ;
  XOR B                   ;
  RLCA                    ; Finally b5b4b3c7c6c5c4c3, so that L becomes 32*INT((B mod 64)/8)+INT(x/8), the low byte.
  RLCA                    ;
  LD L,A                  ;
  LD A,C                  ; A holds x mod 8, so the pixel is bit (7-A) within the byte.
  AND $07                 ;
  RET

; THE 'POINT' SUBROUTINE
;
; This subroutine is called from S_POINT. It is entered with the coordinates of a pixel on the calculator stack, and returns a last value of 1 if that pixel is ink colour, and 0 if it is paper colour.
POINT_SUB:
  CALL STK_TO_BC          ; y-coordinate to B, x to C.
  CALL PIXEL_ADD          ; Pixel address to HL.
  LD B,A                  ; B will count A+1 loops to get the wanted bit of (HL) to location 0.
  INC B                   ;
  LD A,(HL)               ;
POINT_LP:
  RLCA                    ; The shifts.
  DJNZ POINT_LP           ;
  AND $01                 ; The bit is 1 for ink, 0 for paper.
  JP STACK_A              ; It is put on the calculator stack.

; THE 'PLOT' COMMAND ROUTINE
;
; The address of this routine is found in the parameter table.
;
; This routine consists of a main subroutine plus one line to call it and one line to exit from it. The main routine is used twice by CIRCLE and the subroutine is called by DRAW_LINE. The routine is entered with the coordinates of a pixel on the calculator stack. It finds the address of that pixel and plots it, taking account of the status of INVERSE and OVER held in the P-FLAG.
PLOT:
  CALL STK_TO_BC          ; y-coordinate to B, x to C.
  CALL PLOT_SUB           ; The subroutine is called.
  JP TEMPS                ; Exit, setting temporary colours.
; This entry point is used by the routine at DRAW_LINE.
PLOT_SUB:
  LD (COORDS),BC          ; The system variable COORDS is set.
  CALL PIXEL_ADD          ; Pixel address to HL.
  LD B,A                  ; B will count A+1 loops to get a zero to the correct place in A.
  INC B                   ;
  LD A,$FE                ; The zero is entered.
PLOT_LOOP:
  RRCA                    ; Then lined up with the pixel bit position in the byte.
  DJNZ PLOT_LOOP          ;
  LD B,A                  ; Then copied to B.
  LD A,(HL)               ; The pixel-byte is obtained in A.
  LD C,(IY+$57)           ; P-FLAG is obtained and first tested for OVER.
  BIT 0,C                 ;
  JR NZ,PL_TST_IN         ; Jump if OVER 1.
  AND B                   ; OVER 0 first makes the pixel zero.
PL_TST_IN:
  BIT 2,C                 ; Test for INVERSE.
  JR NZ,PLOT_END          ; INVERSE 1 just leaves the pixel as it was (OVER 1) or zero (OVER 0).
  XOR B                   ; INVERSE 0 leaves the pixel complemented (OVER 1) or 1 (OVER 0).
  CPL                     ;
PLOT_END:
  LD (HL),A               ; The byte is entered. Its other bits are unchanged in every case.
  JP PO_ATTR              ; Exit, setting attribute byte.

; THE 'STK-TO-BC' SUBROUTINE
;
; Used by the routines at PR_ITEM_1, POINT_SUB, PLOT, DRAW_LINE, S_SCRN_S and S_ATTR_S.
;
; This subroutine loads two floating point numbers into the BC register pair. It is thus used to pick up parameters in the range +00 to +FF. It also obtains in DE the 'diagonal move' values (+/-1,+/-1) which are used in DRAW_LINE.
;
; O:B First number from the calculator stack
; O:C Second number from the calculator stack
; O:D Sign of the first number (+01 or +FF)
; O:E Sign of the second number (+01 or +FF)
STK_TO_BC:
  CALL STK_TO_A           ; First number to A.
  LD B,A                  ; Hence to B.
  PUSH BC                 ; Save it briefly.
  CALL STK_TO_A           ; Second number to A.
  LD E,C                  ; Its sign indicator to E.
  POP BC                  ; Restore first number.
  LD D,C                  ; Its sign indicator to D.
  LD C,A                  ; Second number to C.
  RET                     ; BC, DE are now as required.

; THE 'STK-TO-A' SUBROUTINE
;
; Used by the routine at STK_TO_BC.
;
; This subroutine loads the A register with the floating point number held at the top of the calculator stack. The number must be in the range +00 to +FF.
;
; O:A Number from the calculator stack
; O:C Sign of the number (+01 or +FF)
STK_TO_A:
  CALL FP_TO_A            ; Modulus of rounded last value to A if possible; else, report error.
  JP C,REPORT_B_3         ;
  LD C,$01                ; One to C for positive last value.
  RET Z                   ; Return if value was positive.
  LD C,$FF                ; Else change C to +FF (i.e. minus one).
  RET                     ; Finished.

; THE 'CIRCLE' COMMAND ROUTINE
;
; The address of this routine is found in the parameter table.
;
; This routine draws an approximation to the circle with centre co-ordinates X and Y and radius Z. These numbers are rounded to the nearest integer before use. Thus Z must be less than 87.5, even when (X,Y) is in the centre of the screen. The method used is to draw a series of arcs approximated by straight lines.
;
; CIRCLE has four parts:
;
; * i. Tests the radius. If its modulus is less than 1, just plot X,Y.
; * ii. Calls CD_PRMS1, which is used to set the initial parameters for both CIRCLE and DRAW.
; * iii. Sets up the remaining parameters for CIRCLE, including the initial displacement for the first 'arc' (a straight line in fact).
; * iv. Jumps to DRW_STEPS to use the arc-drawing loop.
;
; Parts i. to iii. will now be explained in turn.
;
; i. The radius, say Z', is obtained from the calculator stack. Its modulus Z is formed and used from now on. If Z is less than 1, it is deleted from the stack and the point X,Y is plotted by a jump to PLOT.
CIRCLE:
  RST $18                 ; Get the present character.
  CP ","                  ; Test for comma.
  JP NZ,REPORT_C          ; If not so, report the error.
  RST $20                 ; Get next character (the radius).
  CALL CLASS_06           ; Radius to calculator stack.
  CALL CHECK_END          ; Move to consider next statement if checking syntax.
  RST $28                 ; Use calculator.
  DEFB $2A                ; abs: X, Y, Z
  DEFB $3D                ; re_stack: Z is re-stacked; its exponent is therefore available.
  DEFB $38                ; end_calc
  LD A,(HL)               ; Get exponent of radius.
  CP $81                  ; Test whether radius less than 1.
  JR NC,C_R_GRE_1         ; If not, jump.
  RST $28                 ; If less, delete it from the stack.
  DEFB $02                ; delete: X, Y
  DEFB $38                ; end_calc
  JR PLOT                 ; Just plot the point X, Y.
; ii. 2π is stored in mem-5 and CD_PRMS1 is called. This subroutine stores in the B register the number of arcs required for the circle, viz. A=4*INT (π*SQR Z/4)+4, hence 4, 8, 12, etc., up to a maximum of 32. It also stores in mem-0 to mem-4 the quantities 2π/A, SIN(π/A), 0, COS (2π/A) and SIN (2π/A).
C_R_GRE_1:
  RST $28
  DEFB $A3                ; stk_pi_2: X, Y, Z, π/2
  DEFB $38                ; end_calc
  LD (HL),$83             ; Now increase exponent to +83, changing π/2 into 2π.
  RST $28                 ; X, Y, Z, 2π.
  DEFB $C5                ; st_mem_5: (2π is copied to mem-5)
  DEFB $02                ; delete: X, Y, Z
  DEFB $38                ; end_calc
  CALL CD_PRMS1           ; Set the initial parameters.
; iii. A test is made to see whether the initial 'arc' length is less than 1. If it is, a jump is made simply to plot X, Y. Otherwise, the parameters are set: X+Z and X-Z*SIN (π/A) are stacked twice as start and end point, and copied to COORDS as well; zero and 2*Z*SIN (π/A) are stored in mem-1 and mem-2 as initial increments, giving as first 'arc' the vertical straight line joining X+Z, y-Z*SIN (π/A) and X+Z, Y+Z*SIN (π/A). The arc-drawing loop at DRW_STEPS will ensure that all subsequent
; points remain on the same circle as these two points, with incremental angle 2π/A. But it is clear that these 2 points in fact subtend this angle at the point X+Z*(1-COS (π/A)), Y not at X, Y. Hence the end points of each arc of the circle are displaced right by an amount 2*(1-COS (π/A)), which is less than half a pixel, and rounds to one pixel at most.
  PUSH BC                 ; Save the arc-count in B.
  RST $28                 ; X, Y, Z
  DEFB $31                ; duplicate: X, Y, Z, Z
  DEFB $E1                ; get_mem_1: X, Y, Z, Z, SIN (π/A)
  DEFB $04                ; multiply: X, Y, Z, Z*SIN (π/A)
  DEFB $38                ; end_calc
  LD A,(HL)               ; Z*SIN (π/A) is half the initial 'arc' length; it is tested to see whether it is less than 0.5.
  CP $80                  ;
  JR NC,C_ARC_GE1         ; If not, the jump is made.
  RST $28
  DEFB $02                ; delete: X, Y, Z
  DEFB $02                ; delete: X, Y
  DEFB $38                ; end_calc
  POP BC                  ; Clear the machine stack.
  JP PLOT                 ; Jump to plot X, Y.
C_ARC_GE1:
  RST $28                 ; X, Y, Z, Z*SIN (π/A)
  DEFB $C2                ; st_mem_2: (Z*SIN (π/A) to mem-2 for now)
  DEFB $01                ; exchange: X, Y, Z*SIN (π/A), Z
  DEFB $C0                ; st_mem_0: X, Y, Z*SIN (π/A), Z (Z is copied to mem-0)
  DEFB $02                ; delete: X, Y, Z*SIN (π/A)
  DEFB $03                ; subtract: X, Y-Z*SIN (π/A)
  DEFB $01                ; exchange: Y-Z*SIN (π/A), X
  DEFB $E0                ; get_mem_0: Y-Z*SIN (π/A), X, Z
  DEFB $0F                ; addition: Y-Z*SIN (π/A), X+Z
  DEFB $C0                ; st_mem_0: (X+Z is copied to mem-0)
  DEFB $01                ; exchange: X+Z, Y-Z*SIN (π/A)
  DEFB $31                ; duplicate: X+Z, Y-Z*SIN (π/A), Y-Z*SIN (π/A)
  DEFB $E0                ; get_mem_0: sa, sb, sb, sa
  DEFB $01                ; exchange: sa, sb, sa, sb
  DEFB $31                ; duplicate: sa, sb, sa, sb, sb
  DEFB $E0                ; get_mem_0: sa, sb, sa, sb, sb, sa
  DEFB $A0                ; stk_zero: sa, sb, sa, sb, sb, sa, 0
  DEFB $C1                ; st_mem_1: (mem-1 is set to zero)
  DEFB $02                ; delete: sa, sb, sa, sb, sb, sa
  DEFB $38                ; end_calc
; (Here sa denotes X+Z and sb denotes Y-Z*SIN (π/A).)
  INC (IY+$62)            ; Incrementing the exponent byte of mem-2 sets mem-2 to 2*Z*SIN(π/A).
  CALL FIND_INT1          ; The last value X+Z is moved from the stack to A and copied to L.
  LD L,A                  ;
  PUSH HL                 ; It is saved in HL.
  CALL FIND_INT1          ; Y-Z*SIN (π/A) goes from the stack to A and is copied to H. HL now holds the initial point.
  POP HL                  ;
  LD H,A                  ;
  LD (COORDS),HL          ; It is copied to COORDS.
  POP BC                  ; The arc-count is restored.
  JP DRW_STEPS            ; The jump is made to DRW_STEPS.
; (The stack now holds X+Z, Y-Z*SIN (π/A), Y-Z*SIN (π/A), X+Z.)

; THE 'DRAW' COMMAND ROUTINE
;
; The address of this routine is found in the parameter table.
;
; This routine is entered with the co-ordinates of a point X0, Y0, say, in COORDS. If only two parameters X, Y are given with the DRAW command, it draws an approximation to a straight line from the point X0, Y0 to X0+X, Y0+Y. If a third parameter G is given, it draws an approximation to a circular arc from X0, Y0 to X0+X, Y0+Y turning anti-clockwise through an angle G radians.
;
; The routine has four parts:
;
; * i. Just draws a line if only 2 parameters are given or if the diameter of the implied circle is less than 1.
; * ii. Calls CD_PRMS1 to set the first parameters.
; * iii. Sets up the remaining parameters, including the initial displacements for the first arc.
; * iv. Enters the arc-drawing loop and draws the arc as a series of smaller arcs approximated by straight lines, calling the line-drawing subroutine at DRAW_LINE as necessary.
;
; Two subroutines, CD_PRMS1 and DRAW_LINE, follow the main routine. The above 4 parts of the main routine will now be treated in turn.
;
; i. If there are only 2 parameters, a jump is made to LINE_DRAW. A line is also drawn if the quantity Z=(ABS X+ABS Y)/ABS SIN(G/2) is less than 1. Z lies between 1 and 1.5 times the diameter of the implied circle. In this section mem-0 is set to SIN (G/2), mem-1 to Y, and mem-5 to G.
DRAW:
  RST $18                 ; Get the current character.
  CP ","                  ; If it is a comma, then jump.
  JR Z,DR_3_PRMS          ;
  CALL CHECK_END          ; Move on to next statement if checking syntax.
  JP LINE_DRAW            ; Jump to just draw the line.
DR_3_PRMS:
  RST $20                 ; Get next character (the angle).
  CALL CLASS_06           ; Angle to calculator stack.
  CALL CHECK_END          ; Move on to next statement if checking syntax.
  RST $28                 ; X, Y, G are on the stack.
  DEFB $C5                ; st_mem_5: (G is copied to mem-5)
  DEFB $A2                ; stk_half: X, Y, G, 0.5
  DEFB $04                ; multiply: X, Y, G/2
  DEFB $1F                ; sin: X, Y, SIN (G/2)
  DEFB $31                ; duplicate: X, Y, SIN (G/2), SIN (G/2)
  DEFB $30                ; f_not: X, Y, SIN (G/2), (0/1)
  DEFB $30                ; f_not: X, Y, SIN (G/2), (1/0)
  DEFB $00                ; jump_true: X, Y, SIN (G/2)
  DEFB $06                ; to DR_SIN_NZ (if SIN (G/2)=0 i.e. G=2πN just draw a straight line).
  DEFB $02                ; delete: X, Y
  DEFB $38                ; end_calc
  JP LINE_DRAW            ; Line X0, Y0 to X0+X, Y0+Y.
DR_SIN_NZ:
  DEFB $C0                ; st_mem_0: (SIN (G/2) is copied to mem-0)
  DEFB $02                ; delete: X, Y are now on the stack.
  DEFB $C1                ; st_mem_1: (Y is copied to mem-1).
  DEFB $02                ; delete: X
  DEFB $31                ; duplicate: X, X
  DEFB $2A                ; abs: X, X' (X'=ABS X)
  DEFB $E1                ; get_mem_1: X, X', Y
  DEFB $01                ; exchange: X, Y, X'
  DEFB $E1                ; get_mem_1: X, Y, X', Y
  DEFB $2A                ; abs: X, Y, X', Y' (Y'=ABS Y)
  DEFB $0F                ; addition: X, Y, X'+Y'
  DEFB $E0                ; get_mem_0: X, Y, X'+Y', SIN (G/2)
  DEFB $05                ; division: X, Y, (X'+Y')/SIN (G/2)=Z', say
  DEFB $2A                ; abs: X, Y, Z (Z=ABS Z')
  DEFB $E0                ; get_mem_0: X, Y, Z, SIN (G/2)
  DEFB $01                ; exchange: X, Y, SIN (G/2), Z
  DEFB $3D                ; re_stack: (Z is re-stacked to make sure that its exponent is available).
  DEFB $38                ; end_calc
  LD A,(HL)               ; Get exponent of Z.
  CP $81                  ; If Z is greater than or equal to 1, jump.
  JR NC,DR_PRMS           ;
  RST $28                 ; X, Y, SIN (G/2), Z
  DEFB $02                ; delete: X, Y, SIN (G/2)
  DEFB $02                ; delete: X, Y
  DEFB $38                ; end_calc
  JP LINE_DRAW            ; Just draw the line from X0, Y0 to X0+X, Y0+Y.
; ii. Just calls CD_PRMS1. This subroutine saves in the B register the number of shorter arcs required for the complete arc, viz. A=4*INT (G'*SQR Z/8)+4 (where G'=ABS G), or 252 if this expression exceeds 252 (as can happen with a large chord and a small angle). So A is a multiple of 4 from 4 to 252. The subroutine also stores in mem-0 to mem-4 the quantities G/A, SIN (G/2*A), 0, COS (G/A), SIN (G/A).
DR_PRMS:
  CALL CD_PRMS1           ; The subroutine is called.
; iii. Sets up the rest of the parameters as follow. The stack will hold these 4 items, reading up to the top: X0+X and Y0+Y as end of last arc; then X0 and Y0 as beginning of first arc. Mem-0 will hold X0 and mem-5 Y0. Mem-1 and mem-2 will hold the initial displacements for the first arc, U and V; and mem-3 and mem-4 will hold COS (G/A) and SIN (G/A) for use in the arc-drawing loop.
;
; The formulae for U and V can be explained as follows. Instead of stepping along the final chord, of length L, say, with displacements X and Y, we want to step along an initial chord (which may be longer) of length L*W, where W=SIN (G/2*A)/SIN (G/2), with displacements X*W and Y*W, but turned through an angle (G/2-G/2*A), hence with true displacements:
;
; * U=Y*W*SIN (G/2-G/2*A)+X*W*COS (G/2-G/2*A)
; * Y=Y*W*COS (G/2-G/2*A)-X*W*SIN (G/2-G/2*A)
;
; These formulae can be checked from a diagram, using the normal expansion of COS (P-Q) and SIN (P-Q), where Q=G/2-G/2*A.
  PUSH BC                 ; Save the arc-counter in B.
  RST $28                 ; X, Y, SIN(G/2), Z
  DEFB $02                ; delete: X, Y, SIN(G/2)
  DEFB $E1                ; get_mem_1: X, Y, SIN(G/2), SIN(G/2*A)
  DEFB $01                ; exchange: X, Y, SIN(G/2*A), SIN(G/2)
  DEFB $05                ; division: X, Y, SIN(G/2*A)/SIN(G/2)=W
  DEFB $C1                ; st_mem_1: (W is copied to mem-1).
  DEFB $02                ; delete: X, Y
  DEFB $01                ; exchange: Y, X
  DEFB $31                ; duplicate: Y, X, X
  DEFB $E1                ; get_mem_1: Y, X, X, W
  DEFB $04                ; multiply: Y, X, X*W
  DEFB $C2                ; st_mem_2: (X*W is copied to mem-2).
  DEFB $02                ; delete: Y, X
  DEFB $01                ; exchange: X, Y
  DEFB $31                ; duplicate: X, Y, Y
  DEFB $E1                ; get_mem_1: X, Y, Y, W
  DEFB $04                ; multiply: X, Y, Y*W
  DEFB $E2                ; get_mem_2: X, Y, Y*W, X*W
  DEFB $E5                ; get_mem_5: X, Y, Y*W, X*W,G
  DEFB $E0                ; get_mem_0: X, Y, Y*W, X*W, G, G/A
  DEFB $03                ; subtract: X, Y, Y*W, X*W, G-G/A
  DEFB $A2                ; stk_half: X, Y, Y*W, X*W, G-G/A, 1/2
  DEFB $04                ; multiply: X, Y, Y*W, X*W, G/2-G/2*A=F
  DEFB $31                ; duplicate: X, Y, Y*W, X*W, F, F
  DEFB $1F                ; sin: X, Y, Y*W, X*W, F, SIN F
  DEFB $C5                ; st_mem_5: (SIN F is copied to mem-5).
  DEFB $02                ; delete: X, Y, Y*W, X*W,F
  DEFB $20                ; cos: X, Y, Y*W, X*W, COS F
  DEFB $C0                ; st_mem_0: (COS F is copied to mem-0).
  DEFB $02                ; delete: X, Y, Y*W, X*W
  DEFB $C2                ; st_mem_2: (X*W is copied to mem-2).
  DEFB $02                ; delete: X, Y, Y*W
  DEFB $C1                ; st_mem_1: (Y*W is copied to mem-1).
  DEFB $E5                ; get_mem_5: X, Y, Y*W, SIN F
  DEFB $04                ; multiply: X, Y, Y*W*SIN F
  DEFB $E0                ; get_mem_0: X, Y, Y*W*SIN F, X*W
  DEFB $E2                ; get_mem_2: X, Y, Y*W*SIN F, X*W, COS F
  DEFB $04                ; multiply: X, Y, Y*W*SIN F, X*W*COS F
  DEFB $0F                ; addition: X, Y, Y*W*SIN F+X*W*COS F=U
  DEFB $E1                ; get_mem_1: X, Y, U, Y*W
  DEFB $01                ; exchange: X, Y, Y*W, U
  DEFB $C1                ; st_mem_1: (U is copied to mem-1)
  DEFB $02                ; delete: X, Y, Y*W
  DEFB $E0                ; get_mem_0: X, Y, Y*W, COS F
  DEFB $04                ; multiply: X, Y, Y*W*COS F
  DEFB $E2                ; get_mem_2: X, Y, Y*W*COS F, X*W
  DEFB $E5                ; get_mem_5: X, Y, Y*W*COS F, X*W, SIN F
  DEFB $04                ; multiply: X, Y, Y*W*COS F, X*W*SIN F
  DEFB $03                ; subtract: X, Y, Y*W*COS F-X*W*SIN F=V
  DEFB $C2                ; st_mem_2: (V is copied to mem-2).
  DEFB $2A                ; abs: X, Y, V' (V'=ABS V)
  DEFB $E1                ; get_mem_1: X, Y, V', U
  DEFB $2A                ; abs: X, Y, V', U' (U'=ABS U)
  DEFB $0F                ; addition: X, Y, U'+V'
  DEFB $02                ; delete: X, Y
  DEFB $38                ; end_calc: (DE now points to U'+V').
  LD A,(DE)               ; Get exponent of U'+V'.
  CP $81                  ; If U'+V' is less than 1, just tidy the stack and draw the line from X0, Y0 to X0+X, Y0+Y.
  POP BC                  ;
  JP C,LINE_DRAW          ;
  PUSH BC                 ; Otherwise, continue with the parameters: X, Y, on the stack.
  RST $28                 ;
  DEFB $01                ; exchange: Y, X
  DEFB $38                ; end_calc
  LD A,(COORDS)           ; Get X0 from COORDS into A and so on to the stack.
  CALL STACK_A            ;
  RST $28                 ; Y, X, X0
  DEFB $C0                ; st_mem_0: (X0 is copied to mem-0).
  DEFB $0F                ; addition: Y, X0+X
  DEFB $01                ; exchange: X0+X, Y
  DEFB $38                ; end_calc
  LD A,($5C7E)            ; Get Y0 from COORDS into A and so on to the stack.
  CALL STACK_A            ;
  RST $28                 ; X0+X, Y, Y0
  DEFB $C5                ; st_mem_5: (Y0 is copied to mem-5).
  DEFB $0F                ; addition: X0+X, Y0+Y
  DEFB $E0                ; get_mem_0: X0+X, Y0+Y, X0
  DEFB $E5                ; get_mem_5: X0+X, Y0+Y, X0, Y0
  DEFB $38                ; end_calc
  POP BC                  ; Restore the arc-counter in B.
; This entry point is used by the routine at CIRCLE.
;
; iv. The arc-drawing loop. This is entered at ARC_START with the co-ordinates of the starting point on top of the stack, and the initial displacements for the first arc in mem-1 and mem-2. It uses simple trigonometry to ensure that all subsequent arcs will be drawn to points that lie on the same circle as the first two, subtending the same angle at the centre. It can be shown that if 2 points X1, Y1 and X2, Y2 lie on a circle and subtend an angle N at the centre, which is also the origin of
; co-ordinates, then X2=X1*COS N-Y1*SIN N, and Y2=X1*SIN N+Y1*COS N. But because the origin is here at the increments, say Un=Xn+1-Xn and Vn=Yn+1-Yn, thus achieving the desired result. The stack is shown below on the (n+1)th pass through the loop, as Xn and Yn are incremented by Un and Vn, after these are obtained from Un-1 and Vn-1. The 4 values on the top of the stack at ARC_LOOP are, in DRAW, reading upwards, X0+X, Y0+Y, Xn and Yn but to save space these are not shown until ARC_START. For the
; initial values in CIRCLE, see the end of CIRCLE, above. In CIRCLE too, the angle G must be taken to be 2π.
DRW_STEPS:
  DEC B                   ; B counts the passes through the loop.
  JR Z,ARC_END            ; Jump when B has reached zero.
  JR ARC_START            ; Jump into the loop to start.
ARC_LOOP:
  RST $28                 ; (See text above for the stack).
  DEFB $E1                ; get_mem_1: Un-1
  DEFB $31                ; duplicate: Un-1, Un-1
  DEFB $E3                ; get_mem_3: Un-1, Un-1, COS(G/A)
  DEFB $04                ; multiply: Un-1, Un-1*COS(G/A)
  DEFB $E2                ; get_mem_2: Un-1, Un-1*COS(G/A), Vn-1
  DEFB $E4                ; get_mem_4: Un-1, Un-1*COS(G/A), Vn-1, SIN(G/A)
  DEFB $04                ; multiply: Un-1, Un-1*COS(G/A), Vn-1*SIN(G/A)
  DEFB $03                ; subtract: Un-1, Un-1*COS(G/A)-Vn-1*SIN(G/A)=Un
  DEFB $C1                ; st_mem_1: (Un is copied to mem-1).
  DEFB $02                ; delete: Un-1
  DEFB $E4                ; get_mem_4: Un-1, SIN(G/A)
  DEFB $04                ; multiply: Un-1*SIN(G/A)
  DEFB $E2                ; get_mem_2: Un-1*SIN(G/A), Vn-1
  DEFB $E3                ; get_mem_3: Un-1*SIN(G/A), Vn-1, COS(G/A)
  DEFB $04                ; multiply: Un-1*SIN(G/A), Vn-1*COS(G/A)
  DEFB $0F                ; addition: Un-1*SIN(G/A)+Vn-1*COS(G/A)=Vn
  DEFB $C2                ; st_mem_2: (Vn is copied to mem-2).
  DEFB $02                ; delete: (As noted in the text, the stack in fact holds X0+X, Y0+Y, Xn and Yn).
  DEFB $38                ; end_calc
ARC_START:
  PUSH BC                 ; Save the arc-counter.
  RST $28                 ; X0+X, Y0+y, Xn, Yn
  DEFB $C0                ; st_mem_0: (Yn is copied to mem-0).
  DEFB $02                ; delete: X0+X, Y0+Y, Xn
  DEFB $E1                ; get_mem_1: X0+X, Y0+Y, Xn, Un
  DEFB $0F                ; addition: X0+X, Y0+Y, Xn+Un=Xn+1
  DEFB $31                ; duplicate: X0+X, Y0+Y, Xn+1, Xn+1
  DEFB $38                ; end_calc
  LD A,(COORDS)           ; Next Xn', the approximate value of Xn reached by the line-drawing subroutine is copied from COORDS to A and hence to the stack.
  CALL STACK_A            ;
  RST $28                 ; X0+X, Y0+Y, Xn+1, Xn'
  DEFB $03                ; subtract: X0+X, Y0+Y, Xn+1, Xn+1, Xn'-Xn'=Un'
  DEFB $E0                ; get_mem_0: X0+X, Y0+Y, Xn+1, Un', Yn
  DEFB $E2                ; get_mem_2: X0+X, Y0+Y, Xn+1, Un', Yn, Vn
  DEFB $0F                ; addition: X0+X, Y0+Y, Xn+1, Un', Yn+Vn=Yn+1
  DEFB $C0                ; st_mem_0: (Yn+1 is copied to mem-0).
  DEFB $01                ; exchange: X0+X, Y0+Y, Xn+1, Yn+1, Un'
  DEFB $E0                ; get_mem_0: X0+X, Y0+Y, Xn+1, Yn+1, Un', Yn+1
  DEFB $38                ; end_calc
  LD A,($5C7E)            ; Yn', approximate like Xn', is copied from COORDS to A and hence to the stack.
  CALL STACK_A            ;
  RST $28                 ; X0+X, Y0+Y, Xn+1, Yn+1, Un', Yn+1, Yn'
  DEFB $03                ; subtract: X0+X, Y0+Y, Xn+1, Yn+1, Un', Vn'
  DEFB $38                ; end_calc
  CALL DRAW_LINE          ; The next 'arc' is drawn.
  POP BC                  ; The arc-counter is restored.
  DJNZ ARC_LOOP           ; Jump if more arcs to draw.
ARC_END:
  RST $28                 ;
  DEFB $02                ; delete: The co-ordinates of the end of the last arc that was drawn are now deleted from the stack.
  DEFB $02                ;
  DEFB $01                ; exchange: Y0+Y, X0+X
  DEFB $38                ; end_calc
  LD A,(COORDS)           ; The X-co-ordinate of the end of the last arc that was drawn, say Xz', is copied from COORDS to the stack.
  CALL STACK_A            ;
  RST $28
  DEFB $03                ; subtract: Y0+Y, X0+X-Xz'
  DEFB $01                ; exchange: X0+X-Xz', Y0+Y
  DEFB $38                ; end_calc
  LD A,($5C7E)            ; The Y-co-ordinate is obtained from COORDS and stacked.
  CALL STACK_A            ;
  RST $28                 ; X0+X-Xz', Y0+Y, Yz'
  DEFB $03                ; subtract: X0+X-Xz', Y0+Y-Yz'
  DEFB $38                ; end_calc
LINE_DRAW:
  CALL DRAW_LINE          ; The final arc is drawn to reach X0+X, Y0+Y (or close the circle).
  JP TEMPS                ; Exit, setting temporary colours.

; THE 'INITIAL PARAMETERS' SUBROUTINE
;
; This subroutine is called by both CIRCLE and DRAW to set their initial parameters. It is called by CIRCLE with X, Y and the radius Z on the top of the stack, reading upwards. It is called by DRAW with its own X, Y, SIN (G/2) and Z, as defined in DRAW i., on the top of the stack. In what follows the stack is only shown from Z upwards.
;
; The subroutine returns in B the arc-count A as explained in both CIRCLE and DRAW, and in mem-0 to mem-5 the quantities G/A, SIN (G/2*A), 0, COS (G/A), SIN (G/A) and G. For a circle, G must be taken to be equal to 2π.
;
; O:B Arc count
CD_PRMS1:
  RST $28                 ; Z
  DEFB $31                ; duplicate: Z, Z
  DEFB $28                ; sqr: Z, SQR Z
  DEFB $34                ; stk_data: Z, SQR Z, 2
  DEFB $32,$00            ;
  DEFB $01                ; exchange: Z, 2, SQR Z
  DEFB $05                ; division: Z, 2/SQR Z
  DEFB $E5                ; get_mem_5: Z, 2/SQR Z, G
  DEFB $01                ; exchange: Z, G, 2/SQR Z
  DEFB $05                ; division: Z, G*SQR Z/2
  DEFB $2A                ; abs: Z, G'*SQR Z/2 (G'=ABS G)
  DEFB $38                ; end_calc: Z, G'*SQR Z/2=A1, say
  CALL FP_TO_A            ; A1 to A from the stack, if possible.
  JR C,USE_252            ; If A1 rounds to 256 or more, use 252.
  AND $FC                 ; 4*INT (A1/4) to A.
  ADD A,$04               ; Add 4, giving the arc-count A.
  JR NC,DRAW_SAVE         ; Jump if still under 256.
USE_252:
  LD A,$FC                ; Here, just use 252.
DRAW_SAVE:
  PUSH AF                 ; Now save the arc-count.
  CALL STACK_A            ; Copy it to calculator stack too.
  RST $28                 ; Z, A
  DEFB $E5                ; get_mem_5: Z, A, G
  DEFB $01                ; exchange: Z, G, A
  DEFB $05                ; division: Z, G/A
  DEFB $31                ; duplicate: Z, G/A, G/A
  DEFB $1F                ; sin: Z, G/A, SIN (G/A)
  DEFB $C4                ; st_mem_4: (SIN (G/A) is copied to mem-4)
  DEFB $02                ; delete: Z, G/A
  DEFB $31                ; duplicate: Z, G/A, G/A
  DEFB $A2                ; stk_half: Z, G/A, G/A, 0.5
  DEFB $04                ; multiply: Z, G/A, G/2*A
  DEFB $1F                ; sin: Z, G/A, SIN (G/2*A)
  DEFB $C1                ; st_mem_1: (SIN (G/2*A) is copied to mem-1)
  DEFB $01                ; exchange: Z, SIN (G/2*A), G/A
  DEFB $C0                ; st_mem_0: (G/A is copied to mem-0)
  DEFB $02                ; delete: Z, SIN (G/2*A)=S
  DEFB $31                ; duplicate: Z, S, S
  DEFB $04                ; multiply: Z, S*S
  DEFB $31                ; duplicate: Z, S*S, S*S
  DEFB $0F                ; addition: Z, 2*S*S
  DEFB $A1                ; stk_one: Z, 2*S*S, 1
  DEFB $03                ; subtract: Z, 2*S*S-1
  DEFB $1B                ; negate: Z, 1-2*S*S=COS (G/A)
  DEFB $C3                ; st_mem_3: (COS (G/A) is copied to mem-3)
  DEFB $02                ; delete: Z
  DEFB $38                ; end_calc
  POP BC                  ; Restore the arc-count to B.
  RET                     ; Finished.

; THE 'LINE-DRAWING' SUBROUTINE
;
; This subroutine is called by DRAW to draw an approximation to a straight line from the point X0, Y0 held in COORDS to the point X0+X, Y0+Y, where the increments X and Y are on the top of the calculator stack. The subroutine was originally intended for the ZX80 and ZX81 8K ROM, and it is described in a BASIC program on page 121 of the ZX81 manual.
;
; The method is to intersperse as many horizontal or vertical steps as are needed among a basic set of diagonal steps, using an algorithm that spaces the horizontal or vertical steps as evenly as possible.
DRAW_LINE:
  CALL STK_TO_BC          ; ABS Y to B; ABS X to C; SGN Y to D; SGN X to E.
  LD A,C                  ; Jump if ABS X is greater than or equal to ABS Y, so that the smaller goes to L, and the larger (later) goes to H.
  CP B                    ;
  JR NC,DL_X_GE_Y         ;
  LD L,C                  ;
  PUSH DE                 ; Save diagonal step (+/-1,+/-1) in DE.
  XOR A                   ; Insert a vertical step (+/-1,0) into DE (D holds SGN Y).
  LD E,A                  ;
  JR DL_LARGER            ; Now jump to set H.
DL_X_GE_Y:
  OR C                    ; Return if ABS X and ABS Y are both zero.
  RET Z                   ;
  LD L,B                  ; The smaller (ABS Y here) goes to L.
  LD B,C                  ; ABS X to B here, for H.
  PUSH DE                 ; Save the diagonal step here too.
  LD D,$00                ; Horizontal step (0,+/-1) to DE here.
DL_LARGER:
  LD H,B                  ; Larger of ABS X, ABS Y to H now.
; The algorithm starts here. The larger of ABS X and ABS Y, say H, is put into A and reduced to INT (H/2). The H-L horizontal or vertical steps and L diagonal steps are taken (where L is the smaller of ABS X and ABS Y) in this way: L is added to A; if A now equals or exceeds H, it is reduced by H and a diagonal step is taken; otherwise a horizontal or vertical step is taken. This is repeated H times (B also holds H). Note that meanwhile the exchange registers H' and L' are used to hold COORDS.
  LD A,B                  ; B to A as well as to H.
  RRA                     ; A starts at INT (H/2).
D_L_LOOP:
  ADD A,L                 ; L is added to A.
  JR C,D_L_DIAG           ; If 256 or more, jump - diagonal step.
  CP H                    ; If A is less than H, jump for horizontal or vertical step.
  JR C,D_L_HR_VT          ;
D_L_DIAG:
  SUB H                   ; Reduce A by H.
  LD C,A                  ; Restore it to C.
  EXX                     ; Now use the exchange resisters.
  POP BC                  ; Diagonal step to BC'.
  PUSH BC                 ; Save it too.
  JR D_L_STEP             ; Jump to take the step.
D_L_HR_VT:
  LD C,A                  ; Save A (unreduced) in C.
  PUSH DE                 ; Step to stack briefly.
  EXX                     ; Get exchange registers.
  POP BC                  ; Step to BC' now.
D_L_STEP:
  LD HL,(COORDS)          ; Now take the step: first, COORDS to HL' as the start point.
  LD A,B                  ; Y-step from B' to A.
  ADD A,H                 ; Add in H'.
  LD B,A                  ; Result to B'.
  LD A,C                  ; Now the X-step; it will be tested for range (Y will be tested in PLOT).
  INC A                   ;
  ADD A,L                 ; Add L' to C' in A, jump on carry for further test.
  JR C,D_L_RANGE          ;
  JR Z,REPORT_B_3         ; Zero after no carry denotes X-position -1, out of range.
D_L_PLOT:
  DEC A                   ; Restore true value to A.
  LD C,A                  ; Value to C' for plotting.
  CALL PLOT_SUB           ; Plot the step.
  EXX                     ; Restore main registers.
  LD A,C                  ; C back to A to continue algorithm.
  DJNZ D_L_LOOP           ; Loop back for B steps (i.e. H steps).
  POP DE                  ; Clear machine stack.
  RET                     ; Finished.
D_L_RANGE:
  JR Z,D_L_PLOT           ; Zero after carry denotes X-position 255, in range.
; This entry point is used by the routines at PIXEL_ADD and STK_TO_A.
;
; Report B - Integer out of range.
REPORT_B_3:
  RST $08                 ; Call the error handling routine.
  DEFB $0A                ;

; THE 'SCANNING' SUBROUTINE
;
; Used by the routines at PROGNAME, VAL_FET_1, NEXT_2NUM, DATA, DEF_FN, PR_ITEM_1, S_BRACKET, S_FN_SBRN and val.
;
; This subroutine is used to produce an evaluation result of the 'next expression'.
;
; The result is returned as the 'last value' on the calculator stack. For a numerical result, the last value will be the actual floating point number. However, for a string result the last value will consist of a set of parameters. The first of the five bytes is unspecified, the second and third bytes hold the address of the start of the string and the fourth and fifth bytes hold the length of the string.
;
; Bit 6 of FLAGS is set for a numeric result and reset for a string result.
;
; When a next expression consists of only a single operand (e.g. 'A', 'RND', 'A$(4,3 TO 7)'), then the last value is simply the value that is obtained from evaluating the operand.
;
; However when the next expression contains a function and an operand (e.g. 'CHR$ A', 'NOT A', 'SIN 1'), the operation code of the function is stored on the machine stack until the last value of the operand has been calculated. This last value is then subjected to the appropriate operation to give a new last value.
;
; In the case of there being an arithmetic or logical operation to be performed (e.g. 'A+B', 'A*B', 'A=B'), then both the last value of the first argument and the operation code have to be kept until the last value of the second argument has been found. Indeed the calculation of the last value of the second argument may also involve the storing of last values and operation codes whilst the calculation is being performed.
;
; It can therefore be shown that as a complex expression is evaluated (e.g. 'CHR$ (T+A-26*INT ((T+A)/26)+65)'), a hierarchy of operations yet to be performed is built up until the point is reached from which it must be dismantled to produce the final last value.
;
; Each operation code has associated with it an appropriate priority code and operations of higher priority are always performed before those of lower priority.
;
; The subroutine begins with the A register being set to hold the first character of the expression and a starting priority marker - zero - being put on the machine stack.
SCANNING:
  RST $18                 ; The first character is fetched.
  LD B,$00                ; The starting priority marker.
  PUSH BC                 ; It is stacked.
; This entry point is used by the routines at S_U_PLUS and S_LETTER.
S_LOOP_1:
  LD C,A                  ; The main re-entry point.
  LD HL,SCANFUNC          ; Index into the scanning function table with the code in C.
  CALL INDEXER            ;
  LD A,C                  ; Restore the code to A.
  JP NC,S_ALPHNUM         ; Jump if code not found in table.
  LD B,$00                ; Use the entry found in the table to build up the required address in HL, and jump to it.
  LD C,(HL)               ;
  ADD HL,BC               ;
  JP (HL)                 ;

; THE 'SCANNING QUOTES' SUBROUTINE
;
; This subroutine is used by S_QUOTE to check that every string quote is matched by another one.
;
;   BC Current string length counter
; O:BC Updated string length counter
; O:F Zero flag set if two consecutive '"' characters are found
S_QUOTE_S:
  CALL CH_ADD_1           ; Point to the next character.
  INC BC                  ; Increase the length count by one.
  CP $0D                  ; Is it a carriage return?
  JP Z,REPORT_C           ; Report the error if so.
  CP "\""                 ; Is it another '"'?
  JR NZ,S_QUOTE_S         ; Loop back if it is not.
  CALL CH_ADD_1           ; Point to next character.
  CP "\""                 ; Set zero flag if it is another '"'.
  RET                     ; Finished.

; THE 'SCANNING TWO CO-ORDINATES' SUBROUTINE
;
; This subroutine is called by S_SCREEN, S_ATTR and S_POINT to make sure the required two co-ordinates are given in their proper form.
;
; O:F Zero flag set if checking syntax
S_2_COORD:
  RST $20                 ; Fetch the next character.
  CP "("                  ; Is it a '('?
  JR NZ,S_RPORT_C         ; Report the error if it is not.
  CALL NEXT_2NUM          ; Co-ordinates to calculator stack.
  RST $18                 ; Fetch the current character.
  CP ")"                  ; Is it a ')'?
S_RPORT_C:
  JP NZ,REPORT_C          ; Report the error if it is not.
; This routine continues into SYNTAX_Z.

; THE 'SYNTAX-Z' SUBROUTINE
;
; Used by the routines at SAVE_ETC, LIST, LINE_END, CHECK_END, VAR_A_1, CLASS_09, FETCH_NUM, IF_CMD, READ_3, DATA, DEF_FN, UNSTACK_Z, LPRINT, PR_POSN_1, INPUT, IN_ASSIGN, S_QUOTE, S_RND, S_PI, S_DECIMAL, S_LETTER, S_FN_SBRN, LOOK_VARS, SLICING, INT_EXP1, GET_HLxDE and DIM.
;
; The routine at S_2_COORD continues here.
;
; This subroutine is called 31 times, with a saving of just one byte each call. A simple test of bit 7 of FLAGS will give the zero flag reset during execution and set during syntax checking.
;
; O:F Zero flag set if checking syntax
SYNTAX_Z:
  BIT 7,(IY+$01)          ; Test bit 7 of FLAGS.
  RET                     ; Finished.

; THE 'SCANNING SCREEN$' SUBROUTINE
;
; Used by the routine at S_SCREEN.
;
; This subroutine is used to find the character that appears at line x, column y of the screen. It only searches the character set 'pointed to' by CHARS.
;
; Note: this is normally the characters +20 (space) to +7F (©) although the user can alter CHARS to match for other characters, including user-defined graphics.
S_SCRN_S:
  CALL STK_TO_BC          ; x to C, y to B; 0<=x<=23; 0<=y<=31.
  LD HL,(CHARS)           ; CHARS plus +0100 gives HL pointing to the character set.
  LD DE,$0100             ;
  ADD HL,DE               ;
  LD A,C                  ; x is copied to A.
  RRCA                    ; The number 32*(x mod 8)+y is formed in A and copied to E. This is the low byte of the required screen address.
  RRCA                    ;
  RRCA                    ;
  AND $E0                 ;
  XOR B                   ;
  LD E,A                  ;
  LD A,C                  ; x is copied to A again.
  AND $18                 ; Now the number 64+8*INT (x/8) is inserted into D. DE now holds the screen address.
  XOR $40                 ;
  LD D,A                  ;
  LD B,$60                ; B counts the 96 characters.
S_SCRN_LP:
  PUSH BC                 ; Save the count.
  PUSH DE                 ; And the screen pointer.
  PUSH HL                 ; And the character set pointer.
  LD A,(DE)               ; Get first row of screen character.
  XOR (HL)                ; Match with row from character set.
  JR Z,S_SC_MTCH          ; Jump if direct match found.
  INC A                   ; Now test for match with inverse character (get +00 in A from +FF).
  JR NZ,S_SCR_NXT         ; Jump if neither match found.
  DEC A                   ; Restore +FF to A.
S_SC_MTCH:
  LD C,A                  ; Inverse status (+00 or +FF) to C.
  LD B,$07                ; B counts through the other 7 rows.
S_SC_ROWS:
  INC D                   ; Move DE to next row (add +0100).
  INC HL                  ; Move HL to next row (i.e. next byte).
  LD A,(DE)               ; Get the screen row.
  XOR (HL)                ; Match with row from the ROM.
  XOR C                   ; Include the inverse status.
  JR NZ,S_SCR_NXT         ; Jump if row fails to match.
  DJNZ S_SC_ROWS          ; Jump back till all rows done.
  POP BC                  ; Discard character set pointer.
  POP BC                  ; And screen pointer.
  POP BC                  ; Final count to BC.
  LD A,$80                ; Last character code in set plus one.
  SUB B                   ; A now holds required code.
  LD BC,$0001             ; One space is now needed in the work space.
  RST $30                 ; Make the space.
  LD (DE),A               ; Put the character into it.
  JR S_SCR_STO            ; Jump to stack the character.
S_SCR_NXT:
  POP HL                  ; Restore character set pointer.
  LD DE,$0008             ; Move it on 8 bytes, to the next character in the set.
  ADD HL,DE               ;
  POP DE                  ; Restore the screen pointer.
  POP BC                  ; And the counter.
  DJNZ S_SCRN_LP          ; Loop back for the 96 characters.
  LD C,B                  ; Stack the empty string (length zero).
S_SCR_STO:
  JP STK_STO              ; Jump to stack the matching character, or the null string if no match is found.
; Note: this exit, via STK_STO, is a mistake as it leads to 'double storing' of the string result (see S_STRING). The instruction line should be 'RET'.

; THE 'SCANNING ATTRIBUTES' SUBROUTINE
;
; Used by the routine at S_ATTR.
S_ATTR_S:
  CALL STK_TO_BC          ; x to C, y to B. Again, 0<=x<=23; 0<=y<=31.
  LD A,C                  ; x is copied to A and the number 32*(x mod 8)+y is formed in A. 32*(x mod 8)+INT (x/8) is also copied to C.
  RRCA                    ;
  RRCA                    ;
  RRCA                    ;
  LD C,A                  ;
  AND $E0                 ;
  XOR B                   ;
  LD L,A                  ; L holds low byte of attribute address.
  LD A,C                  ; 32*(x mod 8)+INT (x/8) is copied to A.
  AND $03                 ; 88+INT (x/8) is formed in A.
  XOR $58                 ;
  LD H,A                  ; H holds high byte of attribute address.
  LD A,(HL)               ; The attribute byte is copied to A.
  JP STACK_A              ; Exit, stacking the required byte.

; THE SCANNING FUNCTION TABLE
;
; Used by the routine at SCANNING.
;
; This table contains 8 functions and 4 operators. It thus incorporates 5 new Spectrum functions and provides a neat way of accessing some functions and operators which already existed on the ZX81.
SCANFUNC:
  DEFB "\"",$1C           ; S_QUOTE
  DEFB "(",$4F            ; S_BRACKET
  DEFB ".",$F2            ; S_DECIMAL
  DEFB "+",$12            ; S_U_PLUS
  DEFB $A8,$56            ; S_FN
  DEFB $A5,$57            ; S_RND
  DEFB $A7,$84            ; S_PI
  DEFB $A6,$8F            ; S_INKEY
  DEFB $C4,$E6            ; S_DECIMAL
  DEFB $AA,$BF            ; S_SCREEN
  DEFB $AB,$C7            ; S_ATTR
  DEFB $A9,$CE            ; S_POINT
  DEFB $00                ; End marker.

; THE 'SCANNING UNARY PLUS' ROUTINE
;
; The address of this routine is derived from an offset found in the scanning function table.
S_U_PLUS:
  RST $20                 ; For unary plus, simply move on to the next character and jump back to the main re-entry of SCANNING.
  JP S_LOOP_1             ;

; THE 'SCANNING QUOTE' ROUTINE
;
; The address of this routine is derived from an offset found in the scanning function table.
;
; This routine deals with string quotes, whether simple like "name" or more complex like "a ""white"" lie" or the seemingly redundant VAL$ """a""".
S_QUOTE:
  RST $18                 ; Fetch the current character.
  INC HL                  ; Point to the start of the string.
  PUSH HL                 ; Save the start address.
  LD BC,$0000             ; Set the length to zero.
  CALL S_QUOTE_S          ; Call the "matching" subroutine.
  JR NZ,S_Q_PRMS          ; Jump if zero reset - no more quotes.
S_Q_AGAIN:
  CALL S_QUOTE_S          ; Call it again for a third quote.
  JR Z,S_Q_AGAIN          ; And again for the fifth, seventh etc.
  CALL SYNTAX_Z           ; If testing syntax, jump to reset bit 6 of FLAGS and to continue scanning.
  JR Z,S_Q_PRMS           ;
  RST $30                 ; Make space in the work space for the string and the terminating quote.
  POP HL                  ; Get the pointer to the start.
  PUSH DE                 ; Save the pointer to the first space.
S_Q_COPY:
  LD A,(HL)               ; Get a character from the string.
  INC HL                  ; Point to the next one.
  LD (DE),A               ; Copy last one to work space.
  INC DE                  ; Point to the next space.
  CP "\""                 ; Is last character a '"'?
  JR NZ,S_Q_COPY          ; If not, jump to copy next one.
  LD A,(HL)               ; But if it was, do not copy next one; if next one is a '"', jump to copy the one after it; otherwise, finished with copying.
  INC HL                  ;
  CP "\""                 ;
  JR Z,S_Q_COPY           ;
S_Q_PRMS:
  DEC BC                  ; Get true length to BC.
; Note that the first quote was not counted into the length; the final quote was, and is discarded now. Inside the string, the first, third, fifth, etc., quotes were counted in but the second, fourth, etc., were not.
  POP DE                  ; Restore start of copied string.
; This entry point is used by the routine at S_SCREEN.
S_STRING:
  LD HL,FLAGS             ; This is FLAGS; this entry point is used whenever bit 6 is to be reset and a string stacked if executing a line. This is done now.
  RES 6,(HL)              ;
  BIT 7,(HL)              ;
  CALL NZ,STK_STO         ;
  JP S_CONT_2             ; Jump to continue scanning the line.
; Note that in copying the string to the work space, every two pairs of string quotes inside the string ("") have been reduced to one pair of string quotes(").

; THE 'SCANNING BRACKET' ROUTINE
;
; The address of this routine is derived from an offset found in the scanning function table.
S_BRACKET:
  RST $20                 ; This routine simply gets the character and calls SCANNING recursively.
  CALL SCANNING           ;
  CP ")"                  ; Report the error if no matching bracket.
  JP NZ,REPORT_C          ;
  RST $20                 ; Continue scanning.
  JP S_CONT_2             ;

; THE 'SCANNING FN' ROUTINE
;
; The address of this routine is derived from an offset found in the scanning function table.
;
; This routine, for user-defined functions, just jumps to the 'scanning FN' subroutine.
S_FN:
  JP S_FN_SBRN

; THE 'SCANNING RND' ROUTINE
;
; The address of this routine is derived from an offset found in the scanning function table.
S_RND:
  CALL SYNTAX_Z           ; Unless syntax is being checked, jump to calculate a random number.
  JR Z,S_RND_END          ;
  LD BC,(SEED)            ; Fetch the current value of SEED.
  CALL STACK_BC           ; Put it on the calculator stack.
  RST $28                 ; Now use the calculator.
  DEFB $A1                ; stk_one
  DEFB $0F                ; addition: The 'last value' is now SEED+1.
  DEFB $34                ; stk_data: Put the number 75 on the calculator stack.
  DEFB $37,$16            ;
  DEFB $04                ; multiply: 'last value' (SEED+1)*75.
  DEFB $34                 ; stk_data: Put the number 65537 on the calculator stack.
  DEFB $80,$41,$00,$00,$80 ;
  DEFB $32                ; n_mod_m: Divide (SEED+1)*75 by 65537 to give a 'remainder' and an 'answer'.
  DEFB $02                ; delete: Discard the 'answer'.
  DEFB $A1                ; stk_one
  DEFB $03                ; subtract: The 'last value' is now 'remainder' - 1.
  DEFB $31                ; duplicate: Make a copy of the 'last value'.
  DEFB $38                ; end_calc: The calculation is finished.
  CALL FP_TO_BC           ; Use the 'last value' to give the new value for SEED.
  LD (SEED),BC            ;
  LD A,(HL)               ; Fetch the exponent of 'last value'.
  AND A                   ; Jump forward if the exponent is zero.
  JR Z,S_RND_END          ;
  SUB $10                 ; Reduce the exponent, i.e. divide 'last value' by 65536 to give the required 'last value'.
  LD (HL),A               ;
S_RND_END:
  JR S_PI_END             ; Jump past the S_PI routine.

; THE 'SCANNING PI' ROUTINE
;
; The address of this routine is derived from an offset found in the scanning function table.
;
; Unless syntax is being checked the value of 'PI' is calculated and forms the 'last value' on the calculator stack.
S_PI:
  CALL SYNTAX_Z           ; Test for syntax checking.
  JR Z,S_PI_END           ; Jump if required.
  RST $28                 ; Now use the calculator.
  DEFB $A3                ; stk_pi_2: The value of π/2 is put on the calculator stack as the 'last value'.
  DEFB $38                ; end_calc
  INC (HL)                ; The exponent is incremented thereby doubling the 'last value' giving π.
; This entry point is used by the routine at S_RND.
S_PI_END:
  RST $20                 ; Move on to the next character.
  JP S_NUMERIC            ; Jump forward.

; THE' SCANNING INKEY$' ROUTINE
;
; The address of this routine is derived from an offset found in the scanning function table.
S_INKEY:
  LD BC,$105A             ; Priority +10, operation code +5A for the 'read-in' subroutine.
  RST $20                 ;
  CP "#"                  ; If next char. is '#', jump. There will be a numerical argument.
  JP Z,S_PUSH_PO          ;
  LD HL,FLAGS             ; This is FLAGS.
  RES 6,(HL)              ; Reset bit 6 for a string result.
  BIT 7,(HL)              ; Test for syntax checking.
  JR Z,S_INK_EN           ; Jump if required.
  CALL KEY_SCAN           ; Fetch a key-value in DE.
  LD C,$00                ; Prepare empty string; stack it if too many keys pressed.
  JR NZ,S_IK_STK          ;
  CALL K_TEST             ; Test the key value; stack empty string if unsatisfactory.
  JR NC,S_IK_STK          ;
  DEC D                   ; +FF to D for 'L' mode (bit 3 set).
  LD E,A                  ; Key-value to E for decoding.
  CALL K_DECODE           ; Decode the key-value.
  PUSH AF                 ; Save the ASCII value briefly.
  LD BC,$0001             ; One space is needed in the work space.
  RST $30                 ; Make it now.
  POP AF                  ; Restore the ASCII value.
  LD (DE),A               ; Prepare to stack it as a string.
  LD C,$01                ; Its length is one.
S_IK_STK:
  LD B,$00                ; Complete the length parameter.
  CALL STK_STO            ; Stack the required string.
S_INK_EN:
  JP S_CONT_2             ; Jump forward.

; THE 'SCANNING SCREEN$' ROUTINE
;
; The address of this routine is derived from an offset found in the scanning function table.
S_SCREEN:
  CALL S_2_COORD          ; Check that 2 co-ordinates are given.
  CALL NZ,S_SCRN_S        ; Call the subroutine unless checking syntax.
  RST $20                 ; Then get the next character and jump back.
  JP S_STRING             ;

; THE 'SCANNING ATTR' ROUTINE
;
; The address of this routine is derived from an offset found in the scanning function table.
S_ATTR:
  CALL S_2_COORD          ; Check that 2 co-ordinates are given.
  CALL NZ,S_ATTR_S        ; Call the subroutine unless checking syntax.
  RST $20                 ; Then get the next character and jump forward.
  JR S_NUMERIC            ;

; THE 'SCANNING POINT' ROUTINE
;
; The address of this routine is derived from an offset found in the scanning function table.
S_POINT:
  CALL S_2_COORD          ; Check that 2 co-ordinates are given.
  CALL NZ,POINT_SUB       ; Call the subroutine unless checking syntax.
  RST $20                 ; Then get the next character and jump forward.
  JR S_NUMERIC            ;

; THE 'SCANNING ALPHANUMERIC' ROUTINE
;
; Used by the routine at SCANNING.
;
; A Code of the current character
S_ALPHNUM:
  CALL ALPHANUM           ; Is the character alphanumeric?
  JR NC,S_NEGATE          ; Jump if not a letter or a digit.
  CP "A"                  ; Now jump if it is a letter; otherwise continue on into S_DECIMAL.
  JR NC,S_LETTER          ;

; THE 'SCANNING DECIMAL' ROUTINE
;
; The address of this routine is derived from an offset found in the scanning function table.
;
; The routine at S_ALPHNUM continues here.
;
; This routine deals with a decimal point or a number that starts with a digit. It also takes care of the expression 'BIN', which is dealt with in the 'decimal to floating-point' subroutine.
;
; A Code of the first character in the number
S_DECIMAL:
  CALL SYNTAX_Z           ; Jump forward if a line is being executed.
  JR NZ,S_STK_DEC         ;
; The action taken is now very different for syntax checking and line execution. If syntax is being checked then the floating-point form has to be calculated and copied into the actual BASIC line. However when a line is being executed the floating-point form will always be available so it is copied to the calculator stack to form a 'last value'.
;
; During syntax checking:
  CALL DEC_TO_FP          ; The floating-point form is found.
  RST $18                 ; Set HL to point one past the last digit.
  LD BC,$0006             ; Six locations are required.
  CALL MAKE_ROOM          ; Make the room in the BASIC line.
  INC HL                  ; Point to the first free space.
  LD (HL),$0E             ; Enter the number marker code.
  INC HL                  ; Point to the second location.
  EX DE,HL                ; This pointer is wanted in DE.
  LD HL,(STKEND)          ; Fetch the 'old' STKEND.
  LD C,$05                ; There are 5 bytes to move.
  AND A                   ; Clear the carry flag.
  SBC HL,BC               ; The 'new' STKEND='old' STKEND minus 5.
  LD (STKEND),HL          ;
  LDIR                    ; Move the floating-point number from the calculator stack to the line.
  EX DE,HL                ; Put the line pointer in HL.
  DEC HL                  ; Point to the last byte added.
  CALL TEMP_PTR1          ; This sets CH-ADD.
  JR S_NUMERIC            ; Jump forward.
; During line execution:
S_STK_DEC:
  RST $18                 ; Get the current character.
S_SD_SKIP:
  INC HL                  ; Now move on to the next character in turn until the number marker code (+0E) is found.
  LD A,(HL)               ;
  CP $0E                  ;
  JR NZ,S_SD_SKIP         ;
  INC HL                  ; Point to the first byte of the number.
  CALL STACK_NUM          ; Move the floating-point number.
  LD (CH_ADD),HL          ; Set CH-ADD.
; This entry point is used by the routines at S_PI, S_ATTR and S_POINT.
;
; A numeric result has now been identified, coming from RND, PI, ATTR, POINT or a decimal number, therefore bit 6 of FLAGS must be set.
S_NUMERIC:
  SET 6,(IY+$01)          ; Set the numeric marker flag (bit 6 of FLAGS).
  JR S_CONT_1             ; Jump forward.

; THE 'SCANNING VARIABLE' ROUTINE
;
; Used by the routine at S_ALPHNUM.
;
; When a variable name has been identified a call is made to LOOK_VARS which looks through those variables that already exist in the variables area (or in the program area at DEF FN statements for a user-defined function FN). If an appropriate numeric value is found then it is copied to the calculator stack using STACK_NUM. However a string or string array entry has to have the appropriate parameters passed to the calculator stack by STK_VAR (or in the case of a user-defined function, by
; STK_F_ARG as called from LOOK_VARS).
S_LETTER:
  CALL LOOK_VARS          ; Look in the existing variables for the matching entry.
  JP C,REPORT_2           ; An error is reported if there is no existing entry.
  CALL Z,STK_VAR          ; Stack the parameters of the string entry/return numeric element base address.
  LD A,(FLAGS)            ; Fetch FLAGS.
  CP $C0                  ; Test bits 6 and 7 together.
  JR C,S_CONT_1           ; Jump if one or both bits are reset.
  INC HL                  ; A numeric value is to be stacked.
  CALL STACK_NUM          ; Move the number.
; This entry point is used by the routine at S_DECIMAL.
S_CONT_1:
  JR S_CONT_2             ; Jump forward.
; This entry point is used by the routine at S_ALPHNUM.
;
; The character is tested against the code for '-', thus identifying the 'unary minus' operation.
;
; Before the actual test the B register is set to hold the priority +09 and the C register the operation code +DB that are required for this operation.
S_NEGATE:
  LD BC,$09DB             ; Priority +09, operation code +DB.
  CP "-"                  ; Is it a '-'?
  JR Z,S_PUSH_PO          ; Jump forward if it is 'unary minus'.
; Next the character is tested against the code for 'VAL$', with priority +10 and operation code +18.
  LD BC,$1018             ; Priority +10, operation code +18.
  CP $AE                  ; Is it 'VAL$'?
  JR Z,S_PUSH_PO          ; Jump forward if it is 'VAL$'.
; The present character must now represent one of the functions CODE to NOT, with codes +AF to +C3.
  SUB $AF                 ; The range of the functions is changed from +AF to +C3 to range +00 to +14.
  JP C,REPORT_C           ; Report an error if out of range.
; The function 'NOT' is identified and dealt with separately from the others.
  LD BC,$04F0             ; Priority +04, operation code +F0.
  CP $14                  ; Is it the function 'NOT'?
  JR Z,S_PUSH_PO          ; Jump if it is so.
  JP NC,REPORT_C          ; Check the range again.
; The remaining functions have priority +10. The operation codes for these functions are now calculated. Functions that operate on strings need bit 6 reset and functions that give string results need bit 7 reset in their operation codes.
  LD B,$10                ; Priority +10.
  ADD A,$DC               ; The function range is now +DC to +EF.
  LD C,A                  ; Transfer the operation code.
  CP $DF                  ; Separate CODE, VAL and LEN which operate on strings to give numerical results.
  JR NC,S_NO_TO_S         ;
  RES 6,C                 ;
S_NO_TO_S:
  CP $EE                  ; Separate STR$ and CHR$ which operate on numbers to give string results.
  JR C,S_PUSH_PO          ;
  RES 7,C                 ; Mark the operation codes. The other operation codes have bits 6 and 7 both set.
; This entry point is used by the routine at S_INKEY.
;
; The priority code and the operation code for the function being considered are now pushed on to the machine stack. A hierarchy of operations is thereby built up.
S_PUSH_PO:
  PUSH BC                 ; Stack the priority and operation codes before moving on to consider the next part of the expression.
  RST $20                 ;
  JP S_LOOP_1             ;
; This entry point is used by the routines at S_QUOTE, S_BRACKET, S_INKEY and S_FN_SBRN.
;
; The scanning of the line now continues. The present argument may be followed by a '(', a binary operator or, if the end of the expression has been reached, then e.g. a carriage return character or a colon, a separator or a 'THEN'.
S_CONT_2:
  RST $18                 ; Fetch the present character.
S_CONT_3:
  CP "("                  ; Jump forward if it is not a '(', which indicates a parenthesised expression.
  JR NZ,S_OPERTR          ;
; If the 'last value' is numeric then the parenthesised expression is a true sub-expression and must be evaluated by itself. However if the 'last value' is a string then the parenthesised expression represents an element of an array or a slice of a string. A call to SLICING modifies the parameters of the string as required.
  BIT 6,(IY+$01)          ; Jump forward if dealing with a numeric parenthesised expression (bit 6 of FLAGS set).
  JR NZ,S_LOOP            ;
  CALL SLICING            ; Modify the parameters of the 'last value'.
  RST $20                 ; Move on to consider the next character.
  JR S_CONT_3             ;
; If the present character is indeed a binary operator it will be given an operation code in the range +C3 to +CF, and the appropriate priority code.
S_OPERTR:
  LD B,$00                ; Original code to BC to index into the table of operators.
  LD C,A                  ;
  LD HL,OPERATORS         ; The pointer to the table.
  CALL INDEXER            ; Index into the table.
  JR NC,S_LOOP            ; Jump forward if no operation found.
  LD C,(HL)               ; Get required code from the table.
  LD HL,$26ED             ; The pointer to the priority table (26ED+C3 gives PRIORITIES as the first address).
  ADD HL,BC               ; Index into the table.
  LD B,(HL)               ; Fetch the appropriate priority.
; The main loop of this subroutine is now entered. At this stage there are:
;
; * i. A 'last value' on the calculator stack.
; * ii. The starting priority marker on the machine stack below a hierarchy, of unknown size, of function and binary operation codes. This hierarchy may be null.
; * iii. The BC register pair holding the 'present' operation and priority, which if the end of an expression has been reached will be priority zero.
;
; Initially the 'last' operation and priority are taken off the machine stack and compared against the 'present' operation and priority.
S_LOOP:
  POP DE                  ; Get the 'last' operation and priority.
  LD A,D                  ; The priority goes to the A register.
  CP B                    ; Compare 'last' against 'present'.
  JR C,S_TIGHTER          ; Exit to wait for the argument.
  AND A                   ; Are both priorities zero?
  JP Z,GET_CHAR           ; Exit via GET_CHAR thereby making 'last value' the required result.
; Before the 'last' operation is performed, the 'USR' function is separated into 'USR number' and 'USR string' according as bit 6 of FLAGS was set or reset when the argument of the function was stacked as the 'last value'.
  PUSH BC                 ; Stack the 'present' values.
  LD HL,FLAGS             ; This is FLAGS.
  LD A,E                  ; The 'last' operation is compared with the code for USR, which will give 'USR number' unless modified; jump if not 'USR'.
  CP $ED                  ;
  JR NZ,S_STK_LST         ;
  BIT 6,(HL)              ; Test bit 6 of FLAGS.
  JR NZ,S_STK_LST         ; Jump if it is set ('USR number').
  LD E,$99                ; Modify the 'last' operation code: 'offset' +19, plus +80 for string input and numerical result ('USR string').
S_STK_LST:
  PUSH DE                 ; Stack the 'last' values briefly.
  CALL SYNTAX_Z           ; Do not perform the actual operation if syntax is being checked.
  JR Z,S_SYNTEST          ;
  LD A,E                  ; The 'last' operation code.
  AND $3F                 ; Strip off bits 6 and 7 to convert the operation code to a calculator offset.
  LD B,A                  ;
  RST $28                 ; Now use the calculator.
  DEFB $3B                ; fp_calc_2: (perform the actual operation)
  DEFB $38                ; end_calc
  JR S_RUNTEST            ; Jump forward.
; An important part of syntax checking involves the testing of the operation to ensure that the nature of the 'last value' is of the correct type for the operation under consideration.
S_SYNTEST:
  LD A,E                  ; Get the 'last' operation code.
  XOR (IY+$01)            ; This tests the nature of the 'last value' (bit 6 of FLAGS) against the requirement of the operation. They are to be the same for correct syntax.
  AND $40                 ;
S_RPORT_C_2:
  JP NZ,REPORT_C          ; Jump if syntax fails.
; Before jumping back to go round the loop again the nature of the 'last value' must be recorded in FLAGS.
S_RUNTEST:
  POP DE                  ; Get the 'last' operation code.
  LD HL,FLAGS             ; This is FLAGS.
  SET 6,(HL)              ; Assume result to be numeric.
  BIT 7,E                 ; Jump forward if the nature of 'last value' is numeric.
  JR NZ,S_LOOPEND         ;
  RES 6,(HL)              ; It is a string.
S_LOOPEND:
  POP BC                  ; Get the 'present' values into BC.
  JR S_LOOP               ; Jump back.
; Whenever the 'present' operation binds tighter, the 'last' and the 'present' values go back on the machine stack. However if the 'present' operation requires a string as its operand then the operation code is modified to indicate this requirement.
S_TIGHTER:
  PUSH DE                 ; The 'last' values go on the stack.
  LD A,C                  ; Get the 'present' operation code.
  BIT 6,(IY+$01)          ; Do not modify the operation code if dealing with a numeric operand (bit 6 of FLAGS set).
  JR NZ,S_NEXT            ;
  AND $3F                 ; Clear bits 6 and 7.
  ADD A,$08               ; Increase the code by +08.
  LD C,A                  ; Return the code to the C register.
  CP $10                  ; Is the operation 'AND'?
  JR NZ,S_NOT_AND         ; Jump if it is not so.
  SET 6,C                 ; 'AND' requires a numeric operand.
  JR S_NEXT               ; Jump forward.
S_NOT_AND:
  JR C,S_RPORT_C_2        ; The operations -, *, /, ↑ and OR are not possible between strings.
  CP $17                  ; Is the operation a '+'?
  JR Z,S_NEXT             ; Jump if it is so.
  SET 7,C                 ; The other operations yield a numeric result.
S_NEXT:
  PUSH BC                 ; The 'present' values go on the machine stack.
  RST $20                 ; Consider the next character.
  JP S_LOOP_1             ; Go around the loop again.

; THE TABLE OF OPERATORS
;
; Used by the routine at S_LETTER. Each entry in this table points to an entry in the table of priorities.
OPERATORS:
  DEFB "+",$CF            ; +
  DEFB "-",$C3            ; -
  DEFB "*",$C4            ; *
  DEFB "/",$C5            ; /
  DEFB $5E,$C6            ; ↑
  DEFB "=",$CE            ; =
  DEFB ">",$CC            ; >
  DEFB "<",$CD            ; <
  DEFB $C7,$C9            ; <=
  DEFB $C8,$CA            ; >=
  DEFB $C9,$CB            ; <>
  DEFB $C5,$C7            ; OR
  DEFB $C6,$C8            ; AND
  DEFB $00                ; End marker.

; THE TABLE OF PRIORITIES
;
; Used by the routine at S_LETTER. Each entry in this table is pointed to by an entry in the table of operators.
PRIORITIES:
  DEFB $06                ; -
  DEFB $08                ; *
  DEFB $08                ; /
  DEFB $0A                ; ↑
  DEFB $02                ; OR
  DEFB $03                ; AND
  DEFB $05                ; <=
  DEFB $05                ; >=
  DEFB $05                ; <>
  DEFB $05                ; >
  DEFB $05                ; <
  DEFB $05                ; =
  DEFB $06                ; +

; THE 'SCANNING FUNCTION' SUBROUTINE
;
; Used by the routine at S_FN.
;
; This subroutine evaluates a user defined function which occurs in a BASIC line. The subroutine can be considered in four stages:
;
; * i. The syntax of the FN statement is checked during syntax checking.
; * ii. During line execution, a search is made of the program area for a DEF FN statement, and the names of the functions are compared, until a match is found - or an error is reported.
; * iii. The arguments of the FN are evaluated by calls to SCANNING.
; * iv. The function itself is evaluated by calling SCANNING, which in turn calls LOOK_VARS and so STK_F_ARG.
S_FN_SBRN:
  CALL SYNTAX_Z           ; Unless syntax is being checked, a jump is made to SF_RUN.
  JR NZ,SF_RUN            ;
  RST $20                 ; Get the first character of the name.
  CALL ALPHA              ; If it is not alphabetic, then report the error.
  JP NC,REPORT_C          ;
  RST $20                 ; Get the next character.
  CP "$"                  ; Is it a '$'?
  PUSH AF                 ; Save the zero flag on the stack.
  JR NZ,SF_BRKT_1         ; Jump if it was not a '$'.
  RST $20                 ; But get the next character if it was.
SF_BRKT_1:
  CP "("                  ; If the character is not a '(', then report the error.
  JR NZ,SF_RPRT_C         ;
  RST $20                 ; Get the next character.
  CP ")"                  ; Is it a ')'?
  JR Z,SF_FLAG_6          ; Jump if it is; there are no arguments.
SF_ARGMTS:
  CALL SCANNING           ; Within the loop, call SCANNING to check the syntax of each argument and to insert floating-point numbers.
  RST $18                 ; Get the character which follows the argument; if it is not a ',' then jump - no more arguments.
  CP ","                  ;
  JR NZ,SF_BRKT_2         ;
  RST $20                 ; Get the first character in the next argument.
  JR SF_ARGMTS            ; Loop back to consider this argument.
SF_BRKT_2:
  CP ")"                  ; Is the current character a ')'?
SF_RPRT_C:
  JP NZ,REPORT_C          ; Report the error if it is not.
SF_FLAG_6:
  RST $20                 ; Point to the next character in the BASIC line.
  LD HL,FLAGS             ; Assume a string-valued function and reset bit 6 of FLAGS.
  RES 6,(HL)              ;
  POP AF                  ; Restore the zero flag, jump if the FN is indeed string-valued.
  JR Z,SF_SYN_EN          ;
  SET 6,(HL)              ; Otherwise, set bit 6 of FLAGS.
SF_SYN_EN:
  JP S_CONT_2             ; Jump back to continue scanning the line.
; ii. During line execution, a search must first be made for a DEF FN statement.
SF_RUN:
  RST $20                 ; Get the first character of the name.
  AND $DF                 ; Reset bit 5 for upper case.
  LD B,A                  ; Copy the name to B.
  RST $20                 ; Get the next character.
  SUB "$"                 ; Subtract +24, the code for '$'.
  LD C,A                  ; Copy the result to C (zero for a string, non-zero for a numerical function).
  JR NZ,SF_ARGMT1         ; Jump if non-zero: numerical function.
  RST $20                 ; Get the next character, the '('.
SF_ARGMT1:
  RST $20                 ; Get 1st character of 1st argument.
  PUSH HL                 ; Save the pointer to it on the stack.
  LD HL,(PROG)            ; Point to the start of the program (PROG).
  DEC HL                  ; Go back one location.
SF_FND_DF:
  LD DE,$00CE             ; The search will be for 'DEF FN'.
  PUSH BC                 ; Save the name and 'string status'.
  CALL LOOK_PROG          ; Search the program now.
  POP BC                  ; Restore the name and status.
  JR NC,SF_CP_DEF         ; Jump if a DEF FN statement found.
; Report P - FN without DEF.
  RST $08                 ; Call the error handling routine.
  DEFB $18                ;
; When a DEF FN statement is found, the name and status of the two functions are compared; if they do not match, the search is resumed.
SF_CP_DEF:
  PUSH HL                 ; Save the pointer to the DEF FN character in case the search has to be resumed.
  CALL FN_SKPOVR          ; Get the name of the DEF FN function.
  AND $DF                 ; Reset bit 5 for upper case.
  CP B                    ; Does it match the FN name?
  JR NZ,SF_NOT_FD         ; Jump if it does not match.
  CALL FN_SKPOVR          ; Get the next character in the DEF FN.
  SUB "$"                 ; Subtract +24, the code for '$'.
  CP C                    ; Compare the status with that of FN.
  JR Z,SF_VALUES          ; Jump if complete match now found.
SF_NOT_FD:
  POP HL                  ; Restore the pointer to the 'DEF FN'.
  DEC HL                  ; Step back one location.
  LD DE,$0200             ; Use the search routine to find the end of the DEF FN statement, preparing for the next search; save the name and status meanwhile.
  PUSH BC                 ;
  CALL EACH_STMT          ;
  POP BC                  ;
  JR SF_FND_DF            ; Jump back for a further search.
; iii. The correct DEF FN statement has now been found. The arguments of the FN statement will be evaluated by repeated calls of SCANNING, and their 5 byte values (or parameters, for strings) will be inserted into the DEF FN statement in the spaces made there at syntax checking. HL will be used to point along the DEF FN statement (calling FN_SKPOVR as needed) while CH-ADD points along the FN statement (calling NEXT_CHAR as needed).
SF_VALUES:
  AND A                   ; If HL is now pointing to a '$', move on to the '('.
  CALL Z,FN_SKPOVR        ;
  POP DE                  ; Discard the pointer to 'DEF FN'.
  POP DE                  ; Get the pointer to the first argument of FN, and copy it to CH-ADD.
  LD (CH_ADD),DE          ;
  CALL FN_SKPOVR          ; Move past the '(' now.
  PUSH HL                 ; Save this pointer on the stack.
  CP ")"                  ; Is it pointing to a ')'?
  JR Z,SF_R_BR_2          ; If so, jump: FN has no arguments.
SF_ARG_LP:
  INC HL                  ; Point to the next code.
  LD A,(HL)               ; Put the code into A.
  CP $0E                  ; Is it the 'number marker' code, +0E?
  LD D,$40                ; Set bit 6 of D for a numerical argument.
  JR Z,SF_ARG_VL          ; Jump on zero: numerical argument.
  DEC HL                  ; Now ensure that HL is pointing to the '$' character (not e.g. to a control code).
  CALL FN_SKPOVR          ;
  INC HL                  ; HL now points to the 'number marker'.
  LD D,$00                ; Bit 6 of D is reset: string argument.
SF_ARG_VL:
  INC HL                  ; Point to the 1st of the 5 bytes in DEF FN.
  PUSH HL                 ; Save this pointer on the stack.
  PUSH DE                 ; Save the 'string status' of the argument.
  CALL SCANNING           ; Now evaluate the argument.
  POP AF                  ; Get the no./string flag into A.
  XOR (IY+$01)            ; Test bit 6 of it against the result of SCANNING (bit 6 of FLAGS).
  AND $40                 ;
  JR NZ,REPORT_Q          ; Give report Q if they did not match.
  POP HL                  ; Get the pointer to the first of the 5 spaces in DEF FN into DE.
  EX DE,HL                ;
  LD HL,(STKEND)          ; Point HL at STKEND.
  LD BC,$0005             ; BC will count 5 bytes to be moved.
  SBC HL,BC               ; First, decrease STKEND by 5, so deleting the 'last value' from the stack.
  LD (STKEND),HL          ;
  LDIR                    ; Copy the 5 bytes into the spaces in DEF FN.
  EX DE,HL                ; Point HL at the next code.
  DEC HL                  ; Ensure that HL points to the character after the 5 bytes.
  CALL FN_SKPOVR          ;
  CP ")"                  ; Is it a ')'?
  JR Z,SF_R_BR_2          ; Jump if it is: no more arguments in the DEF FN statement.
  PUSH HL                 ; It is a ',': save the pointer to it.
  RST $18                 ; Get the character after the last argument that was evaluated from FN.
  CP ","                  ; If it is not a ',' jump: mismatched arguments of FN and DEF FN.
  JR NZ,REPORT_Q          ;
  RST $20                 ; Point CH-ADD to the next argument of FN.
  POP HL                  ; Point HL to the ',' in DEF FN again.
  CALL FN_SKPOVR          ; Move HL on to the next argument in DEF FN.
  JR SF_ARG_LP            ; Jump back to consider this argument.
SF_R_BR_2:
  PUSH HL                 ; Save the pointer to the ')' in DEF FN.
  RST $18                 ; Get the character after the last argument in FN.
  CP ")"                  ; Is it a ')'?
  JR Z,SF_VALUE           ; If so, jump to evaluate the function; but if not, give report Q.
; Report Q - Parameter error.
REPORT_Q:
  RST $08                 ; Call the error handling routine.
  DEFB $19                ;
; iv. Finally, the function itself is evaluated by calling SCANNING, after first setting DEFADD to hold the address of the arguments as they occur in the DEF FN statement. This ensures that LOOK_VARS, when called by SCANNING, will first search these arguments for the required values, before making a search of the variables area.
SF_VALUE:
  POP DE                  ; Restore pointer to ')' in DEF FN.
  EX DE,HL                ; Get this pointer into HL.
  LD (CH_ADD),HL          ; Insert it into CH-ADD.
  LD HL,(DEFADD)          ; Get the old value of DEFADD.
  EX (SP),HL              ; Stack it, and get the start address of the arguments area of DEF FN into DEFADD.
  LD (DEFADD),HL          ;
  PUSH DE                 ; Save address of ')' in FN.
  RST $20                 ; Move CH-ADD on past ')' and '=' to the start of the expression for the function in DEF FN.
  RST $20                 ;
  CALL SCANNING           ; Now evaluate the function.
  POP HL                  ; Restore the address of ')' in FN.
  LD (CH_ADD),HL          ; Store it in CH-ADD.
  POP HL                  ; Restore original value of DEFADD.
  LD (DEFADD),HL          ; Put it back into DEFADD.
  RST $20                 ; Get the next character in the BASIC line.
  JP S_CONT_2             ; Jump back to continue scanning.

; THE 'FUNCTION SKIPOVER' SUBROUTINE
;
; This subroutine is used by S_FN_SBRN and by STK_F_ARG to move HL along the DEF FN statement while leaving CH-ADD undisturbed, as it points along the FN statement.
;
;   HL Address of the current character
; O:A Code of next non-control, non-space character
; O:HL Address of that character
FN_SKPOVR:
  INC HL                  ; Point to the next code in the statement.
  LD A,(HL)               ; Copy the code to A.
  CP $21                  ; Jump back to skip over it if it is a control code or a space.
  JR C,FN_SKPOVR          ;
  RET                     ; Finished.

; THE 'LOOK-VARS' SUBROUTINE
;
; Used by the routines at SAVE_ETC, CLASS_01, CLASS_04, S_LETTER and DIM.
;
; This subroutine is called whenever a search of the variables area or of the arguments of a DEF FN statement is required. The subroutine is entered with the system variable CH-ADD pointing to the first letter of the name of the variable whose location is being sought. The name will be in the program area or the work space. The subroutine initially builds up a discriminator byte, in the C register, that is based on the first letter of the variable's name. Bits 5 and 6 of this byte indicate the
; type of the variable that is being handled.
;
; The B register is used as a bit register to hold flags.
;
; O:C Bits 0-4: Code of the variable's name (if executing)
; O:C Bit 5: Set if the variable is numeric, reset if it's a string
; O:C Bit 6: Set if the variable is simple, reset if it's an array
; O:C Bit 7: Set if checking syntax, reset if executing
; O:HL Address of the last letter of the variable's name (in the variables area, if found)
; O:F Carry flag reset if the variable already exists or if checking syntax
; O:F Zero flag reset if the variable is simple (not an array) and does not exist
LOOK_VARS:
  SET 6,(IY+$01)          ; Presume a numeric variable (set bit 6 of FLAGS).
  RST $18                 ; Get the first character into A.
  CALL ALPHA              ; Is it alphabetic?
  JP NC,REPORT_C          ; Give an error report if it is not so.
  PUSH HL                 ; Save the pointer to the first letter.
  AND $1F                 ; Transfer bits 0 to 4 of the letter to the C register; bits 5 and 7 are always reset.
  LD C,A                  ;
  RST $20                 ; Get the second character into A.
  PUSH HL                 ; Save this pointer also.
  CP "("                  ; Is the second character a '('?
  JR Z,V_RUN_SYN          ; Separate arrays of numbers.
  SET 6,C                 ; Now set bit 6.
  CP "$"                  ; Is the second character a '$'?
  JR Z,V_STR_VAR          ; Separate all the strings.
  SET 5,C                 ; Now set bit 5.
  CALL ALPHANUM           ; If the variable's name has only one character then jump forward.
  JR NC,V_TEST_FN         ;
; Now find the end character of a name that has more than one character.
V_CHAR:
  CALL ALPHANUM           ; Is the character alphanumeric?
  JR NC,V_RUN_SYN         ; Jump out of the loop when the end of the name is found.
  RES 6,C                 ; Mark the discriminator byte.
  RST $20                 ; Get the next character.
  JR V_CHAR               ; Go back to test it.
; Simple strings and arrays of strings require that bit 6 of FLAGS is reset.
V_STR_VAR:
  RST $20                 ; Step CH-ADD past the '$'.
  RES 6,(IY+$01)          ; Reset bit 6 of FLAGS to indicate a string.
; If DEFADD-hi is non-zero, indicating that a 'function' (a 'FN') is being evaluated, and if in 'run-time', a search will be made of the arguments in the DEF FN statement.
V_TEST_FN:
  LD A,($5C0C)            ; Is DEFADD-hi zero?
  AND A                   ;
  JR Z,V_RUN_SYN          ; If so, jump forward.
  CALL SYNTAX_Z           ; In 'run-time'?
  JP NZ,STK_F_ARG         ; If so, jump forward to search the DEF FN statement.
; This entry point is used by the routine at STK_F_ARG.
;
; Otherwise (or if the variable was not found in the DEF FN statement) a search of variables area will be made, unless syntax is being checked.
V_RUN_SYN:
  LD B,C                  ; Copy the discriminator byte to the B register.
  CALL SYNTAX_Z           ; Jump forward if in 'run-time'.
  JR NZ,V_RUN             ;
  LD A,C                  ; Move the discriminator to A.
  AND $E0                 ; Drop the character code part.
  SET 7,A                 ; Indicate syntax by setting bit 7.
  LD C,A                  ; Restore the discriminator.
  JR V_SYNTAX             ; Jump forward to continue.
; A BASIC line is being executed so make a search of the variables area.
V_RUN:
  LD HL,(VARS)            ; Pick up the VARS pointer.
; Now enter a loop to consider the names of the existing variables.
V_EACH:
  LD A,(HL)               ; The first letter of each existing variable.
  AND $7F                 ; Match on bits 0 to 6.
  JR Z,V_80_BYTE          ; Jump when the '+80-byte' is reached.
  CP C                    ; The actual comparison.
  JR NZ,V_NEXT            ; Jump forward if the first characters do not match.
  RLA                     ; Rotate A leftwards and then double it to test bits 5 and 6.
  ADD A,A                 ;
  JP P,V_FOUND_2          ; Strings and array variables.
  JR C,V_FOUND_2          ; Simple numeric and FOR-NEXT variables.
; Long names are required to be matched fully.
  POP DE                  ; Take a copy of the pointer to the second character.
  PUSH DE                 ;
  PUSH HL                 ; Save the first letter pointer.
V_MATCHES:
  INC HL                  ; Consider the next character.
V_SPACES:
  LD A,(DE)               ; Fetch each character in turn.
  INC DE                  ; Point to the next character.
  CP " "                  ; Is the character a 'space'?
  JR Z,V_SPACES           ; Ignore the spaces.
  OR $20                  ; Set bit 5 so as to match lower and upper case letters.
  CP (HL)                 ; Make the comparison.
  JR Z,V_MATCHES          ; Back for another character if it does match.
  OR $80                  ; Will it match with bit 7 set?
  CP (HL)                 ; Try it.
  JR NZ,V_GET_PTR         ; Jump forward if the 'last characters' do not match.
  LD A,(DE)               ; Check that the end of the name has been reached before jumping forward.
  CALL ALPHANUM           ;
  JR NC,V_FOUND_1         ;
; In all cases where the names fail to match the HL register pair has to be made to point to the next variable in the variables area.
V_GET_PTR:
  POP HL                  ; Fetch the pointer.
V_NEXT:
  PUSH BC                 ; Save B and C briefly.
  CALL NEXT_ONE           ; DE is made to point to the next variable.
  EX DE,HL                ; Switch the two pointers.
  POP BC                  ; Get B and C back.
  JR V_EACH               ; Go around the loop again.
; Come here if no entry was found with the correct name.
V_80_BYTE:
  SET 7,B                 ; Signal 'variable not found'.
; Come here if checking syntax.
V_SYNTAX:
  POP DE                  ; Drop the pointer to the second character.
  RST $18                 ; Fetch the present character.
  CP "("                  ; Is it a '('?
  JR Z,V_PASS             ; Jump forward if so.
  SET 5,B                 ; Indicate not dealing with an array and jump forward.
  JR V_END                ;
; Come here when an entry with the correct name was found.
V_FOUND_1:
  POP DE                  ; Drop the saved variable pointer.
V_FOUND_2:
  POP DE                  ; Drop the second character pointer.
  POP DE                  ; Drop the first letter pointer.
  PUSH HL                 ; Save the 'last' letter pointer.
  RST $18                 ; Fetch the current character.
; If the matching variable name has more than a single letter then the other characters must be passed over.
;
; Note: this appears to have been done already at V_CHAR.
V_PASS:
  CALL ALPHANUM           ; Is it alphanumeric?
  JR NC,V_END             ; Jump when the end of the name has been found.
  RST $20                 ; Fetch the next character.
  JR V_PASS               ; Go back and test it.
; The exit-parameters are now set.
V_END:
  POP HL                  ; HL holds the pointer to the letter of a short name or the 'last' character of a long name.
  RL B                    ; Rotate the whole register.
  BIT 6,B                 ; Specify the state of bit 6.
  RET                     ; Finished.
; The exit-parameters for the subroutine can be summarised as follows.
;
; The system variable CH-ADD points to the first location after the name of the variable as it occurs in the BASIC line.
;
; When 'variable not found':
;
; * The carry flag is set.
; * The zero flag is set only when the search was for an array variable.
; * The HL register pair points to the first letter of the name of the variable as it occurs in the BASIC line.
;
; When 'variable found':
;
; * The carry flag is reset.
; * The zero flag is set for both simple string variables and all array variables.
; * The HL register pair points to the letter of a 'short' name, or the last character of a 'long' name, of the existing entry that was found in the variables area.
;
; In all cases bits 5 and 6 of the C register indicate the type of variable being handled. Bit 7 is the complement of the SYNTAX/RUN flag. But only when the subroutine is used in 'runtime' will bits 0 to 4 hold the code of the variable's letter.
;
; In syntax time the return is always made with the carry flag reset. The zero flag is set for arrays and reset for all other variables, except that a simple string name incorrectly followed by a '$' sets the zero flag and, in the case of SAVE "name" DATA a$(), passes syntax as well.

; THE 'STACK FUNCTION ARGUMENT' SUBROUTINE
;
; This subroutine is called by LOOK_VARS when DEFADD-hi is non-zero, to make a search of the arguments area of a DEF FN statement, before searching in the variables area. If the variable is found in the DEF FN statement, then the parameters of a string variable are stacked and a signal is given that there is no need to call STK_VAR. But it is left to S_LETTER to stack the value of a numerical variable at 26DA in the usual way.
STK_F_ARG:
  LD HL,(DEFADD)          ; Point to the first character in the arguments area (DEFADD) and put it into A.
  LD A,(HL)               ;
  CP ")"                  ; Is it a ')'?
  JP Z,V_RUN_SYN          ; Jump to search the variables area.
SFA_LOOP:
  LD A,(HL)               ; Get the next argument in the loop.
  OR $60                  ; Set bits 5 and 6, assuming a simple numeric variable; copy it to B.
  LD B,A                  ;
  INC HL                  ; Point to the next code.
  LD A,(HL)               ; Put it into the A register.
  CP $0E                  ; Is it the 'number marker' code, +0E?
  JR Z,SFA_CP_VR          ; Jump if so: numeric variable.
  DEC HL                  ; Ensure that HL points to the character, not to a space or control code.
  CALL FN_SKPOVR          ;
  INC HL                  ; HL now points to the 'number marker'.
  RES 5,B                 ; Reset bit 5 of B: string variable.
SFA_CP_VR:
  LD A,B                  ; Get the variable name into A.
  CP C                    ; Is it the one we are looking for?
  JR Z,SFA_MATCH          ; Jump if it matches.
  INC HL                  ; Now pass over the 5 bytes of the floating-point number or string parameters to get to the next argument.
  INC HL                  ;
  INC HL                  ;
  INC HL                  ;
  INC HL                  ;
  CALL FN_SKPOVR          ; Pass on to the next character.
  CP ")"                  ; Is it a ')'?
  JP Z,V_RUN_SYN          ; If so, jump to search the variables area.
  CALL FN_SKPOVR          ; Point to the next argument.
  JR SFA_LOOP             ; Jump back to consider it.
; A match has been found. The parameters of a string variable are stacked, avoiding the need to call STK_VAR.
SFA_MATCH:
  BIT 5,C                 ; Test for a numeric variable.
  JR NZ,SFA_END           ; Jump if the variable is numeric; SCANNING will stack it.
  INC HL                  ; Point to the first of the 5 bytes to be stacked.
  LD DE,(STKEND)          ; Point DE to STKEND.
  CALL duplicate          ; Stack the 5 bytes.
  EX DE,HL                ; Point HL to the new position of STKEND, and reset the system variable.
  LD (STKEND),HL          ;
SFA_END:
  POP DE                  ; Discard the LOOK_VARS pointers (second and first character pointers).
  POP DE                  ;
  XOR A                   ; Return from the search with both the carry and zero flags reset - signalling that a call STK_VAR is not required.
  INC A                   ;
  RET                     ; Finished.

; THE 'STK-VAR' SUBROUTINE
;
; Used by the routines at VAR_A_1, S_LETTER and DIM.
;
; This subroutine is used either to find the parameters that define an existing string entry in the variables area, or to return in the HL register pair the base address of a particular element or an array of numbers. When called from DIM the subroutine only checks the syntax of the BASIC statement.
;
; Note that the parameters that define a string may be altered by calling SLICING if this should be specified.
;
; Initially the A and the B registers are cleared and bit 7 of the C register is tested to determine whether syntax is being checked.
;
; C Bit 5: Set if the variable is numeric, reset if it's a string
; C Bit 6: Set if the variable is simple, reset if it's an array
; C Bit 7: Set if checking syntax, reset if executing
; HL Address of the last letter of the variable's name (in the variables area)
STK_VAR:
  XOR A                   ; Clear the array flag.
  LD B,A                  ; Clear the B register for later.
  BIT 7,C                 ; Jump forward if syntax is being checked.
  JR NZ,SV_COUNT          ;
; Next, simple strings are separated from array variables.
  BIT 7,(HL)              ; Jump forward if dealing with an array variable.
  JR NZ,SV_ARRAYS         ;
; The parameters for a simple string are readily found.
  INC A                   ; Signal 'a simple string'.
SV_SIMPLE:
  INC HL                  ; Move along the entry.
  LD C,(HL)               ; Pick up the low length counter.
  INC HL                  ; Advance the pointer.
  LD B,(HL)               ; Pick up the high length pointer.
  INC HL                  ; Advance the pointer.
  EX DE,HL                ; Transfer the pointer to the actual string.
  CALL STK_STO            ; Pass these parameters to the calculator stack.
  RST $18                 ; Fetch the present character and jump forward to see if a 'slice' is required.
  JP SV_SLICE2            ;
; The base address of an element in an array is now found. Initially the 'number of dimensions' is collected.
SV_ARRAYS:
  INC HL                  ; Step past the length bytes.
  INC HL                  ;
  INC HL                  ;
  LD B,(HL)               ; Collect the 'number of dimensions'.
  BIT 6,C                 ; Jump forward if handling an array of numbers.
  JR Z,SV_PTR             ;
; If an array of strings has its 'number of dimensions' equal to '1' then such an array can be handled as a simple string.
  DEC B                   ; Decrease the 'number of dimensions' and jump if the number is now zero.
  JR Z,SV_SIMPLE          ;
; Next a check is made to ensure that in the BASIC line the variable is followed by a subscript.
  EX DE,HL                ; Save the pointer in DE.
  RST $18                 ; Get the present character.
  CP "("                  ; Is it a '('?
  JR NZ,REPORT_3          ; Report the error if it is not so.
  EX DE,HL                ; Restore the pointer.
; For both numeric arrays and arrays of strings the variable pointer is transferred to the DE register pair before the subscript is evaluated.
SV_PTR:
  EX DE,HL                ; Pass the pointer to DE.
  JR SV_COUNT             ; Jump forward.
; The following loop is used to find the parameters of a specified element within an array. The loop is entered at the mid-point - SV_COUNT - where the element count is set to zero.
;
; The loop is accessed B times, this being, for a numeric array, equal to the number of dimensions that are being used, but for an array of strings B is one less than the number of dimensions in use as the last subscript is used to specify a 'slice' of the string.
SV_COMMA:
  PUSH HL                 ; Save the counter.
  RST $18                 ; Get the present character.
  POP HL                  ; Restore the counter.
  CP ","                  ; Is the present character a ','?
  JR Z,SV_LOOP            ; Jump forward to consider another subscript.
  BIT 7,C                 ; If a line is being executed then there is an error.
  JR Z,REPORT_3           ;
  BIT 6,C                 ; Jump forward if dealing with an array of strings.
  JR NZ,SV_CLOSE          ;
  CP ")"                  ; Is the present character a ')'?
  JR NZ,SV_RPT_C          ; Report an error if not so.
  RST $20                 ; Advance CH-ADD.
  RET                     ; Return as the syntax is correct.
; For an array of strings the present subscript may represent a 'slice', or the subscript for a 'slice' may yet be present in the BASIC line.
SV_CLOSE:
  CP ")"                  ; Is the present character a ')'?
  JR Z,SV_DIM             ; Jump forward and check whether there is another subscript.
  CP $CC                  ; Is the present character a 'TO'?
  JR NZ,SV_RPT_C          ; It must not be otherwise.
SV_CH_ADD:
  RST $18                 ; Get the present character.
  DEC HL                  ; Point to the preceding character and set CH-ADD.
  LD (CH_ADD),HL          ;
  JR SV_SLICE             ; Evaluate the 'slice'.
; Enter the loop here.
SV_COUNT:
  LD HL,$0000             ; Set the counter to zero.
SV_LOOP:
  PUSH HL                 ; Save the counter briefly.
  RST $20                 ; Advance CH-ADD.
  POP HL                  ; Restore the counter.
  LD A,C                  ; Fetch the discriminator byte.
  CP $C0                  ; Jump unless checking the syntax for an array of strings.
  JR NZ,SV_MULT           ;
  RST $18                 ; Get the present character.
  CP ")"                  ; Is it a ')'?
  JR Z,SV_DIM             ; Jump forward as finished counting elements.
  CP $CC                  ; Is to 'TO'?
  JR Z,SV_CH_ADD          ; Jump back if dealing with a 'slice'.
SV_MULT:
  PUSH BC                 ; Save the dimension-number counter and the discriminator byte.
  PUSH HL                 ; Save the element-counter.
  CALL DE_DE_1            ; Get a dimension-size into DE.
  EX (SP),HL              ; The counter moves to HL and the variable pointer is stacked.
  EX DE,HL                ; The counter moves to DE and the dimension-size to HL.
  CALL INT_EXP1           ; Evaluate the next subscript.
  JR C,REPORT_3           ; Give an error if out of range.
  DEC BC                  ; The result of the evaluation is decremented as the counter is to count the elements occurring before the specified element.
  CALL GET_HLxDE          ; Multiply the counter by the dimension-size.
  ADD HL,BC               ; Add the result of INT_EXP1 to the present counter.
  POP DE                  ; Fetch the variable pointer.
  POP BC                  ; Fetch the dimension-number and the discriminator byte.
  DJNZ SV_COMMA           ; Keep going round the loop until B equals zero.
; The SYNTAX/RUN flag is checked before arrays of strings are separated from arrays of numbers.
  BIT 7,C                 ; Report an error if checking syntax at this point.
SV_RPT_C:
  JR NZ,SL_RPT_C          ;
  PUSH HL                 ; Save the counter.
  BIT 6,C                 ; Jump forward if handling an array of strings.
  JR NZ,SV_ELEM           ;
; When dealing with an array of numbers the present character must be a ')'.
  LD B,D                  ; Transfer the variable pointer to the BC register pair.
  LD C,E                  ;
  RST $18                 ; Fetch the present character.
  CP ")"                  ; Is it a ')'?
  JR Z,SV_NUMBER          ; Jump past the error report unless it is needed.
; This entry point is used by the routines at SLICING and DIM.
;
; Report 3 - Subscript out of range.
REPORT_3:
  RST $08                 ; Call the error handling routine.
  DEFB $02                ;
; The address of the location before the actual floating-point form can now be calculated.
SV_NUMBER:
  RST $20                 ; Advance CH-ADD.
  POP HL                  ; Fetch the counter.
  LD DE,$0005             ; There are 5 bytes to each element in an array of numbers.
  CALL GET_HLxDE          ; Compute the total number of bytes before the required element.
  ADD HL,BC               ; Make HL point to the location before the required element.
  RET                     ; Return with this address.
; When dealing with an array of strings the length of an element is given by the last 'dimension-size'. The appropriate parameters are calculated and then passed to the calculator stack.
SV_ELEM:
  CALL DE_DE_1            ; Fetch the last dimension-size.
  EX (SP),HL              ; The variable pointer goes on the stack and the counter to HL.
  CALL GET_HLxDE          ; Multiply 'counter' by 'dimension-size'.
  POP BC                  ; Fetch the variable pointer.
  ADD HL,BC               ; This gives HL pointing to the location before the string.
  INC HL                  ; So point to the actual 'start'.
  LD B,D                  ; Transfer the last dimension-size to BC to form the 'length'.
  LD C,E                  ;
  EX DE,HL                ; Move the 'start' to DE.
  CALL STK_ST_0           ; Pass these parameters to the calculator stack. Note: the first parameter is zero indicating a string from an 'array of strings' and hence the existing entry is not to be reclaimed.
; There are three possible forms of the last subscript:
;
; * A$(2,4 TO 8)
; * A$(2)(4 TO 8)
; * A$(2)
;
; The last of these is the default form and indicates that the whole string is required.
  RST $18                 ; Get the present character.
  CP ")"                  ; Is it a ')'?
  JR Z,SV_DIM             ; Jump if it is so.
  CP ","                  ; Is it a ','?
  JR NZ,REPORT_3          ; Report the error if not so.
SV_SLICE:
  CALL SLICING            ; Use SLICING to modify the set of parameters.
SV_DIM:
  RST $20                 ; Fetch the next character.
SV_SLICE2:
  CP "("                  ; Is It a '('?
  JR Z,SV_SLICE           ; Jump back if there is a 'slice' to be considered.
; When finished considering the last subscript a return can be made.
  RES 6,(IY+$01)          ; Signal - string result (reset bit 6 of FLAGS).
  RET                     ; Return with the parameters of the required string forming a 'last value' on the calculator stack.

; THE 'SLICING' SUBROUTINE
;
; Used by the routines at S_LETTER and STK_VAR.
;
; The present string can be sliced using this subroutine. The subroutine is entered with the parameters of the string being present on the top of the calculator stack and in the registers A, B, C, D and E. Initially the SYNTAX/RUN flag is tested and the parameters of the string are fetched only if a line is being executed.
;
; BC Length of the string
; DE Address of the first character in the string
SLICING:
  CALL SYNTAX_Z           ; Check the flag.
  CALL NZ,STK_FETCH       ; Take the parameters off the stack in 'run-time'.
; The possibility of the 'slice' being '()' has to be considered.
  RST $20                 ; Get the next character.
  CP ")"                  ; Is it a ')'?
  JR Z,SL_STORE           ; Jump forward if it is so.
; Before proceeding the registers are manipulated as follows:
  PUSH DE                 ; The 'start' goes on the machine stack.
  XOR A                   ; The A register is cleared and saved.
  PUSH AF                 ;
  PUSH BC                 ; The 'length' is saved briefly.
  LD DE,$0001             ; Presume that the 'slice' is to begin with the first character.
  RST $18                 ; Get the first character.
  POP HL                  ; Pass the 'length' to HL.
; The first parameter of the 'slice' is now evaluated.
  CP $CC                  ; Is the present character a 'TO'?
  JR Z,SL_SECOND          ; The first parameter, by default, will be '1' if the jump is taken.
  POP AF                  ; At this stage A is zero.
  CALL INT_EXP2           ; BC is made to hold the first parameter. A will hold +FF if there has been an 'out of range' error.
  PUSH AF                 ; Save the value anyway.
  LD D,B                  ; Transfer the first parameter to DE.
  LD E,C                  ;
  PUSH HL                 ; Save the 'length' briefly.
  RST $18                 ; Get the present character.
  POP HL                  ; Restore the 'length'.
  CP $CC                  ; Is the present character a 'TO'?
  JR Z,SL_SECOND          ; Jump forward to consider the second parameter if it is so; otherwise show that there is a closing bracket.
  CP ")"                  ;
; This entry point is used by the routine at STK_VAR.
SL_RPT_C:
  JP NZ,REPORT_C
; At this point a 'slice' of a single character has been identified. e.g. A$(4).
  LD H,D                  ; The last character of the 'slice' is also the first character.
  LD L,E                  ;
  JR SL_DEFINE            ; Jump forward.
; The second parameter of a 'slice' is now evaluated.
SL_SECOND:
  PUSH HL                 ; Save the 'length' briefly.
  RST $20                 ; Get the next character.
  POP HL                  ; Restore the 'length'.
  CP ")"                  ; Is the present character a ')'?
  JR Z,SL_DEFINE          ; Jump if there is not a second parameter.
  POP AF                  ; If the first parameter was in range A will hold zero, otherwise +FF.
  CALL INT_EXP2           ; Make BC hold the second parameter.
  PUSH AF                 ; Save the 'error register'.
  RST $18                 ; Get the present character.
  LD H,B                  ; Pass the result obtained from INT_EXP2 to the HL register pair.
  LD L,C                  ;
  CP ")"                  ; Check that there is a closing bracket now.
  JR NZ,SL_RPT_C          ;
; The 'new' parameters are now defined.
SL_DEFINE:
  POP AF                  ; Fetch the 'error register'.
  EX (SP),HL              ; The second parameter goes on the stack and the 'start' goes to HL.
  ADD HL,DE               ; The first parameter is added to the 'start'.
  DEC HL                  ; Go back a location to get it correct.
  EX (SP),HL              ; The 'new start' goes on the stack and the second parameter goes to HL.
  AND A                   ; Subtract the first parameters from the second to find the length of the 'slice'.
  SBC HL,DE               ;
  LD BC,$0000             ; Initialise the 'new length'.
  JR C,SL_OVER            ; A negative 'slice' is a 'null string' rather than an error condition.
  INC HL                  ; Allow for the inclusive byte.
  AND A                   ; Only now test the 'error register'.
  JP M,REPORT_3           ; Jump if either parameter was out of range for the string.
  LD B,H                  ; Transfer the 'new length' to BC.
  LD C,L                  ;
SL_OVER:
  POP DE                  ; Get the 'new start'.
  RES 6,(IY+$01)          ; Ensure that a string is still indicated (reset bit 6 of FLAGS).
SL_STORE:
  CALL SYNTAX_Z           ; Return at this point if checking syntax; otherwise continue into STK_ST_0.
  RET Z                   ;

; THE 'STK-STORE' SUBROUTINE
;
; Used by the routine at STK_VAR.
;
; The routine at SLICING continues here.
;
; This subroutine passes the values held in the A, B, C, D and E registers to the calculator stack. The stack thereby grows in size by 5 bytes with every call to this subroutine.
;
; The subroutine is normally used to transfer the parameters of strings but it is also used by STACK_BC and LOG_2_A to transfer 'small integers' to the stack.
;
; Note that when storing the parameters of a string the first value stored (coming from the A register) will be a zero if the string comes from an array of strings or is a 'slice' of a string. The value will be '1' for a complete simple string. This 'flag' is used in the LET command routine when the '1' signals that the old copy of the string is to be 'reclaimed'.
;
;   A First byte (when entering at STK_STO or STK_STORE)
;   B Fifth byte
;   C Fourth byte
;   D Third byte
;   E Second byte
; O:HL The new value of STKEND
STK_ST_0:
  XOR A                   ; Signal - a string from an array of strings or a 'sliced' string.
; This entry point is used by the routines at INPUT, S_SCRN_S, S_QUOTE, S_INKEY, STK_VAR, strs_add, chrs, str and read_in.
STK_STO:
  RES 6,(IY+$01)          ; Ensure the flag indicates a string result (reset bit 6 of FLAGS).
; This entry point is used by the routines at STACK_BC and LOG_2_A.
STK_STORE:
  PUSH BC                 ; Save B and C briefly.
  CALL TEST_5_SP          ; Is there room for 5 bytes? Do not return here unless there is room available.
  POP BC                  ; Restore B and C.
  LD HL,(STKEND)          ; Fetch the address of the first location above the present stack (STKEND).
  LD (HL),A               ; Transfer the first byte.
  INC HL                  ; Step on.
  LD (HL),E               ; Transfer the second and third bytes; for a string these will be the 'start'.
  INC HL                  ;
  LD (HL),D               ;
  INC HL                  ; Step on.
  LD (HL),C               ; Transfer the fourth and fifth bytes; for a string these will be the 'length'.
  INC HL                  ;
  LD (HL),B               ;
  INC HL                  ; Step on so as to point to the location above the stack.
  LD (STKEND),HL          ; Save this address in STKEND and return.
  RET                     ;

; THE 'INT-EXP' SUBROUTINE
;
; Used by the routines at STK_VAR and DIM.
;
; This subroutine returns the result of evaluating the 'next expression' as an integer value held in the BC register pair. The subroutine also tests this result against a limit-value supplied in the HL register pair. The carry flag becomes set if there is an 'out of range' error.
;
; The A register is used as an 'error register' and holds +00 if there is no 'previous error' and +FF if there has been one.
;
;   HL Limit value
; O:BC Expression value
; O:F Carry flag set if this value is out of range
INT_EXP1:
  XOR A                   ; Clear the 'error register'.
; This entry point is used by the routine at SLICING.
INT_EXP2:
  PUSH DE                 ; Save both the DE and HL register pairs throughout.
  PUSH HL                 ;
  PUSH AF                 ; Save the 'error register' briefly.
  CALL CLASS_06           ; The 'next expression' is evaluated to give a 'last value' on the calculator stack.
  POP AF                  ; Restore the 'error register'.
  CALL SYNTAX_Z           ; Jump forward if checking syntax.
  JR Z,I_RESTORE          ;
  PUSH AF                 ; Save the error register again.
  CALL FIND_INT2          ; The 'last value' is compressed Into BC.
  POP DE                  ; Error register to D.
  LD A,B                  ; A 'next expression' that gives zero is always in error so jump forward if it is so.
  OR C                    ;
  SCF                     ;
  JR Z,I_CARRY            ;
  POP HL                  ; Take a copy of the limit-value. This will be a 'dimension-size', a 'DIM-limit' or a 'string length'.
  PUSH HL                 ;
  AND A                   ; Now compare the result of evaluating the expression against the limit.
  SBC HL,BC               ;
; The state of the carry flag and the value held in the D register are now manipulated so as to give the appropriate value for the 'error register'.
I_CARRY:
  LD A,D                  ; Fetch the 'old error value'.
  SBC A,$00               ; Form the 'new error value': +00 if no error at any time, +FF or less if an 'out of range' error on this pass or on previous ones.
; Restore the registers before returning.
I_RESTORE:
  POP HL                  ; Restore HL and DE.
  POP DE                  ;
  RET                     ; Return; 'error register' is the A register.

; THE 'DE,(DE+1)' SUBROUTINE
;
; Used by the routine at STK_VAR.
;
; This subroutine performs the construction 'LD DE,(DE+1)' and returns HL pointing to 'DE+2'.
;
;   DE Target address minus one
; O:DE Word at the target address
; O:HL Target address plus one
DE_DE_1:
  EX DE,HL                ; Use HL for the construction.
  INC HL                  ; Point to 'DE+1'.
  LD E,(HL)               ; In effect - LD E,(DE+1).
  INC HL                  ; Point to 'DE+2'.
  LD D,(HL)               ; In effect - LD D,(DE+2).
  RET                     ; Finished.

; THE 'GET-HL*DE' SUBROUTINE
;
; Used by the routines at STK_VAR and DIM.
;
; Unless syntax is being checked this subroutine calls HL_HLxDE which performs the implied construction.
;
; Overflow of the 16 bits available in the HL register pair gives the report 'out of memory'. This is not exactly the true situation but it implies that the memory is not large enough for the task envisaged by the programmer.
;
;   DE First number (M)
;   HL Second number (N)
; O:HL M*N (if not checking syntax)
GET_HLxDE:
  CALL SYNTAX_Z           ; Return directly if syntax is being checked.
  RET Z                   ;
  CALL HL_HLxDE           ; Perform the multiplication.
  JP C,REPORT_4           ; Report 'Out of memory'.
  RET                     ; Finished.

; THE 'LET' COMMAND ROUTINE
;
; Used by the routines at VAL_FET_1, FOR and INPUT.
;
; This is the actual assignment routine for the LET, READ and INPUT commands.
;
; When the destination variable is a 'newly declared variable' then DEST will point to the first letter of the variable's name as it occurs in the BASIC line. Bit 1 of FLAGX will be set.
;
; However if the destination variable 'exists already' then bit 1 of FLAGX will be reset and DEST will point for a numeric variable to the location before the five bytes of the 'old number', and for a string variable to the first location of the 'old string'. The use of DEST in this manner applies to simple variables and to elements of arrays.
;
; Bit 0 of FLAGX is set if the destination variable is a 'complete' simple string variable. (Signalling - delete the old copy.) Initially the current value of DEST is collected and bit 1 of FLAGS tested.
LET:
  LD HL,(DEST)            ; Fetch the present address in DEST.
  BIT 1,(IY+$37)          ; Jump if handling a variable that 'exists already' (bit 1 of FLAGX reset).
  JR Z,L_EXISTS           ;
; A 'newly declared variable' is being used. So first the length of its name is found.
  LD BC,$0005             ; Presume dealing with a numeric variable - 5 bytes.
; Enter a loop to deal with the characters of a long name. Any spaces or colour codes in the name are ignored.
L_EACH_CH:
  INC BC                  ; Add '1' to the counter for each character of a name.
L_NO_SP:
  INC HL                  ; Move along the variable's name.
  LD A,(HL)               ; Fetch the 'present code'.
  CP " "                  ; Jump back if it is a 'space', thereby ignoring spaces.
  JR Z,L_NO_SP            ;
  JR NC,L_TEST_CH         ; Jump forward if the code is +21 to +FF.
  CP $10                  ; Accept, as a final code, those in the range +00 to +0F.
  JR C,L_SPACES           ;
  CP $16                  ; Also accept the range +16 to +1F.
  JR NC,L_SPACES          ;
  INC HL                  ; Step past the control code after any of INK to OVER.
  JR L_NO_SP              ; Jump back as these control codes are treated as spaces.
; Separate 'numeric' and 'string' names.
L_TEST_CH:
  CALL ALPHANUM           ; Is the code alphanumeric?
  JR C,L_EACH_CH          ; If It is so then accept it as a character of a 'long' name.
  CP "$"                  ; Is the present code a '$'?
  JP Z,L_NEW              ; Jump forward as handling a 'newly declared' simple string.
; The 'newly declared numeric variable' presently being handled will require BC spaces in the variables area for its name and its value. The room is made available and the name of the variable is copied over with the characters being 'marked' as required.
L_SPACES:
  LD A,C                  ; Copy the 'length' to A.
  LD HL,(E_LINE)          ; Make HL point to the '+80-byte' at the end of the variables area (E-LINE-1).
  DEC HL                  ;
  CALL MAKE_ROOM          ; Now open up the variables area. Note: in effect BC spaces are made before the displaced '+80-byte'.
  INC HL                  ; Point to the first 'new' byte.
  INC HL                  ; Make DE point to the second 'new' byte.
  EX DE,HL                ;
  PUSH DE                 ; Save this pointer.
  LD HL,(DEST)            ; Fetch the pointer to the start of the name (DEST).
  DEC DE                  ; Make DE point to the first 'new' byte.
  SUB $06                 ; Make B hold the 'number of extra letters' that are found in a 'long name'.
  LD B,A                  ;
  JR Z,L_SINGLE           ; Jump forward if dealing with a variable with a 'short name'.
; The 'extra' codes of a long name are passed to the variables area.
L_CHAR:
  INC HL                  ; Point to each 'extra' code.
  LD A,(HL)               ; Fetch the code.
  CP $21                  ; Accept codes from +21 to +FF; ignore codes +00 to +20.
  JR C,L_CHAR             ;
  OR $20                  ; Set bit 5, as for lower case letters.
  INC DE                  ; Transfer the codes in turn to the 2nd 'new' byte onwards.
  LD (DE),A               ;
  DJNZ L_CHAR             ; Go round the loop for all the 'extra' codes.
; The last code of a 'long' name has to be ORed with +80.
  OR $80                  ; Mark the code as required and overwrite the last code.
  LD (DE),A               ;
; The first letter of the name of the variable being handled is now considered.
  LD A,$C0                ; Prepare to mark the letter of a 'long' name.
L_SINGLE:
  LD HL,(DEST)            ; Fetch the pointer to the letter (DEST).
  XOR (HL)                ; A holds +00 for a 'short' name and +C0 for a 'long' name.
  OR $20                  ; Set bit 5, as for lower case letters.
  POP HL                  ; Drop the pointer now.
; The subroutine L_FIRST is now called to enter the 'letter' into its appropriate location.
  CALL L_FIRST            ; Enter the letter and return with HL pointing to 'new +80-byte'.
; The 'last value' can now be transferred to the variables area. Note that at this point HL always points to the location after the five locations allotted to the number.
;
; A 'RST $28' instruction is used to call the calculator and the 'last value' is deleted. However this value is not overwritten.
L_NUMERIC:
  PUSH HL                 ; Save the 'destination' pointer.
  RST $28                 ; Use the calculator to move STKEND back five bytes.
  DEFB $02                ; delete
  DEFB $38                ; end_calc
  POP HL                  ; Restore the pointer.
  LD BC,$0005             ; Give the number a 'length' of five bytes.
  AND A                   ; Make HL point to the first of the five locations and jump forward to make the actual transfer.
  SBC HL,BC               ;
  JR L_ENTER              ;
; Come here if considering a variable that 'exists already'. First bit 6 of FLAGS is tested so as to separate numeric variables from string or array of string variables.
L_EXISTS:
  BIT 6,(IY+$01)          ; Jump forward if handling any kind of string variable (bit 6 of FLAGS reset).
  JR Z,L_DELETE           ;
; For numeric variables the 'new' number overwrites the 'old' number. So first HL has to be made to point to the location after the five bytes of the existing entry. At present HL points to the location before the five bytes.
  LD DE,$0006             ; The five bytes of a number + 1.
  ADD HL,DE               ; HL now points 'after'.
  JR L_NUMERIC            ; Jump back to make the actual transfer.
; The parameters of the string variable are fetched and complete simple strings separated from 'sliced' strings and array strings.
L_DELETE:
  LD HL,(DEST)            ; Fetch the 'start' (DEST). Note: this line is redundant.
  LD BC,(STRLEN)          ; Fetch the 'length' (STRLEN).
  BIT 0,(IY+$37)          ; Jump if dealing with a complete simple string (bit 0 of FLAGX set); the old string will need to be 'deleted' in this case only.
  JR NZ,L_ADD             ;
; When dealing with a 'slice' of an existing simple string, a 'slice' of a string from an array of strings or a complete string from an array of strings there are two distinct stages involved. The first is to build up the 'new' string in the work space, lengthening or shortening it as required. The second stage is then to copy the 'new' string to its allotted room in the variables area.
;
; However do nothing if the string has no 'length'.
  LD A,B                  ; Return if the string is a null string.
  OR C                    ;
  RET Z                   ;
; Then make the required number of spaces available in the work space.
  PUSH HL                 ; Save the 'start' (DEST).
  RST $30                 ; Make the necessary amount of room in the work space.
  PUSH DE                 ; Save the pointer to the first location.
  PUSH BC                 ; Save the 'length' for use later on.
  LD D,H                  ; Make DE point to the last location.
  LD E,L                  ;
  INC HL                  ; Make HL point 'one past' the new locations.
  LD (HL)," "             ; Enter a 'space' character.
  LDDR                    ; Copy this character into all the new locations. Finish with HL pointing to the first new location.
; The parameters of the string being handled are now fetched from the calculator stack.
  PUSH HL                 ; Save the pointer briefly.
  CALL STK_FETCH          ; Fetch the 'new' parameters.
  POP HL                  ; Restore the pointer.
; Note: at this point the required amount of room has been made available in the work space for the 'variable in assignment'; e.g. for the statement 'LET A$(4 TO 8)="abcdefg"' five locations have been made.
;
; The parameters fetched above as a 'last value' represent the string that is to be copied into the new locations with Procrustean lengthening or shortening as required.
;
; The length of the 'new' string is compared to the length of the room made available for it.
  EX (SP),HL              ; 'Length' of new area to HL. 'Pointer' to new area to stack.
  AND A                   ; Compare the two 'lengths' and jump forward if the 'new' string will fit into the room, i.e. no shortening required.
  SBC HL,BC               ;
  ADD HL,BC               ;
  JR NC,L_LENGTH          ;
  LD B,H                  ; However modify the 'new' length if it is too long.
  LD C,L                  ;
L_LENGTH:
  EX (SP),HL              ; 'Length' of new area to stack. 'Pointer' to new area to HL.
; As long as the new string is not a 'null string' it is copied into the work space. Procrustean lengthening is achieved automatically if the 'new' string is shorter than the room available for it.
  EX DE,HL                ; 'Start' of new string to HL. 'Pointer' to new area to DE.
  LD A,B                  ; Jump forward if the 'new' string is a 'null' string.
  OR C                    ;
  JR Z,L_IN_W_S           ;
  LDIR                    ; Otherwise move the 'new' string to the work space.
; The values that have been saved on the machine stack are restored.
L_IN_W_S:
  POP BC                  ; 'Length' of new area.
  POP DE                  ; 'Pointer' to new area.
  POP HL                  ; The start - the pointer to the 'variable in assignment' which was originally in DEST. L_ENTER is now used to pass the 'new' string to the variables area.
; The following short subroutine is used to pass either a numeric value from the calculator stack, or a string from the work space, to its appropriate position in the variables area.
;
; The subroutine is therefore used for all except 'newly declared' simple strings and 'complete and existing' simple strings.
L_ENTER:
  EX DE,HL                ; Change the pointers over.
  LD A,B                  ; Check once again that the length is not zero.
  OR C                    ;
  RET Z                   ;
  PUSH DE                 ; Save the destination pointer.
  LDIR                    ; Move the numeric value or the string.
  POP HL                  ; Return with the HL register pair pointing to the first byte of the numeric value or the string.
  RET                     ;
; When handling a 'complete and existing' simple string the new string is entered as if it were a 'newly declared' simple string before the existing version is 'reclaimed'.
L_ADD:
  DEC HL                  ; Make HL point to the letter of the variable's name, i.e. DEST-3.
  DEC HL                  ;
  DEC HL                  ;
  LD A,(HL)               ; Pick up the letter.
  PUSH HL                 ; Save the pointer to the 'existing version'.
  PUSH BC                 ; Save the 'length' of the 'existing string'.
  CALL L_STRING           ; Use L_STRING to add the new string to the variables area.
  POP BC                  ; Restore the 'length'.
  POP HL                  ; Restore the pointer.
  INC BC                  ; Allow one byte for the letter and two bytes for the length.
  INC BC                  ;
  INC BC                  ;
  JP RECLAIM_2            ; Exit by jumping to RECLAIM_2 which will reclaim the whole of the existing version.
; 'Newly declared' simple strings are handled as follows:
L_NEW:
  LD A,$DF                ; Prepare for the marking of the variable's letter.
  LD HL,(DEST)            ; Fetch the pointer to the letter (DEST).
  AND (HL)                ; Mark the letter as required. L_STRING is now used to add the new string to the variables area.
; The parameters of the 'new' string are fetched, sufficient room is made available for it and the string is then transferred.
L_STRING:
  PUSH AF                 ; Save the variable's letter.
  CALL STK_FETCH          ; Fetch the 'start' and the 'length' of the 'new' string.
  EX DE,HL                ; Move the 'start' to HL.
  ADD HL,BC               ; Make HL point one past the string.
  PUSH BC                 ; Save the 'length'.
  DEC HL                  ; Make HL point to the end of the string.
  LD (DEST),HL            ; Save the pointer briefly in DEST.
  INC BC                  ; Allow one byte for the letter and two bytes for the length.
  INC BC                  ;
  INC BC                  ;
  LD HL,(E_LINE)          ; Make HL point to the '+80-byte' at the end of the variables area (E-LINE-1).
  DEC HL                  ;
  CALL MAKE_ROOM          ; Now open up the variables area. Note: in effect BC spaces are made before the displaced '+80-byte'.
  LD HL,(DEST)            ; Restore the pointer to the end of the 'new' string from DEST.
  POP BC                  ; Make a copy of the length of the 'new' string.
  PUSH BC                 ;
  INC BC                  ; Add one to the length in case the 'new' string is a 'null' string.
  LDDR                    ; Now copy the 'new' string + one byte.
  EX DE,HL                ; Make HL point to the byte that is to hold the high-length.
  INC HL                  ;
  POP BC                  ; Fetch the 'length'.
  LD (HL),B               ; Enter the high-length.
  DEC HL                  ; Back one.
  LD (HL),C               ; Enter the low-length.
  POP AF                  ; Fetch the variable's letter.
; The following subroutine is entered with the letter of the variable, suitably marked, in the A register. The letter overwrites the 'old +80-byte' in the variables area. The subroutine returns with the HL register pair pointing to the 'new +80-byte'.
L_FIRST:
  DEC HL                  ; Make HL point to the 'old +80-byte'.
  LD (HL),A               ; It is overwritten with the letter of the variable.
  LD HL,(E_LINE)          ; Make HL point to the 'new +80-byte' (E-LINE-1).
  DEC HL                  ;
  RET                     ; Finished with all the 'newly declared variables'.

; THE 'STK-FETCH' SUBROUTINE
;
; Used by the routines at PROGNAME, SAVE_ETC, OPEN_2, VAR_A_1, PR_ITEM_1, SLICING, LET, usr, compare, strs_add, val, code and len.
;
; This important subroutine collects the 'last value' from the calculator stack. The five bytes can be either a floating-point number, in 'short' or 'long' form, or a set of parameters that define a string.
;
; O:A First byte
; O:B Fifth byte
; O:C Fourth byte
; O:D Third byte
; O:E Second byte
STK_FETCH:
  LD HL,(STKEND)          ; Get STKEND.
  DEC HL                  ; Back one.
  LD B,(HL)               ; The fifth value.
  DEC HL                  ; Back one.
  LD C,(HL)               ; The fourth value.
  DEC HL                  ; Back one.
  LD D,(HL)               ; The third value.
  DEC HL                  ; Back one.
  LD E,(HL)               ; The second value.
  DEC HL                  ; Back one.
  LD A,(HL)               ; The first value.
  LD (STKEND),HL          ; Reset STKEND to its new position.
  RET                     ; Finished.

; THE 'DIM' COMMAND ROUTINE
;
; The address of this routine is found in the parameter table.
;
; This routine establishes new arrays in the variables area. The routine starts by searching the existing variables area to determine whether there is an existing array with the same name. If such an array is found then it is 'reclaimed' before the new array is established.
;
; A new array will have all its elements set to zero if it is a numeric array, or to 'spaces' if it is an array of strings.
DIM:
  CALL LOOK_VARS          ; Search the variables area.
D_RPORT_C:
  JP NZ,REPORT_C          ; Give report C as there has been an error.
  CALL SYNTAX_Z           ; Jump forward if in 'run time'.
  JR NZ,D_RUN             ;
  RES 6,C                 ; Test the syntax for string arrays as if they were numeric.
  CALL STK_VAR            ; Check the syntax of the parenthesised expression.
  CALL CHECK_END          ; Move on to consider the next statement as the syntax was satisfactory.
; An 'existing array' is reclaimed.
D_RUN:
  JR C,D_LETTER           ; Jump forward if there is no 'existing array'.
  PUSH BC                 ; Save the discriminator byte.
  CALL NEXT_ONE           ; Find the start of the next variable.
  CALL RECLAIM_2          ; Reclaim the 'existing array'.
  POP BC                  ; Restore the discriminator byte.
; The initial parameters of the new array are found.
D_LETTER:
  SET 7,C                 ; Set bit 7 in the discriminator byte.
  LD B,$00                ; Make the dimension counter zero.
  PUSH BC                 ; Save the counter and the discriminator byte.
  LD HL,$0001             ; The HL register pair is to hold the size of the elements in the array: '1' for a string array, '5' for a numeric array.
  BIT 6,C                 ;
  JR NZ,D_SIZE            ;
  LD L,$05                ;
D_SIZE:
  EX DE,HL                ; Element size to DE.
; The following loop is accessed for each dimension that is specified in the parenthesised expression of the DIM statement. The total number of bytes required for the elements of the array is built up in the DE register pair.
D_NO_LOOP:
  RST $20                 ; Advance CH-ADD on each pass.
  LD H,$FF                ; Set a 'limit value'.
  CALL INT_EXP1           ; Evaluate a parameter.
  JP C,REPORT_3           ; Give an error if 'out of range'.
  POP HL                  ; Fetch the dimension counter and the discriminator byte.
  PUSH BC                 ; Save the parameter on each pass through the loop.
  INC H                   ; Increase the dimension counter on each pass also.
  PUSH HL                 ; Restack the dimension counter and the discriminator byte.
  LD H,B                  ; The parameter is moved to the HL register pair.
  LD L,C                  ;
  CALL GET_HLxDE          ; The byte total is built up in HL and then transferred to DE.
  EX DE,HL                ;
  RST $18                 ; Get the present character and go around the loop again if there is another dimension.
  CP ","                  ;
  JR Z,D_NO_LOOP          ;
; At this point the DE register pair indicates the number of bytes required for the elements of the new array and the size of each dimension is stacked, on the machine stack.
;
; Now check that there is indeed a closing bracket to the parenthesised expression.
  CP ")"                  ; Is it a ')'?
  JR NZ,D_RPORT_C         ; Jump back if not so.
  RST $20                 ; Advance CH-ADD past it.
; Allowance is now made for the dimension sizes.
  POP BC                  ; Fetch the dimension counter and the discriminator byte.
  LD A,C                  ; Pass the discriminator byte to the A register for later.
  LD L,B                  ; Move the counter to L.
  LD H,$00                ; Clear the H register.
  INC HL                  ; Increase the dimension counter by two and double the result and form the correct overall length for the variable by adding the element byte total.
  INC HL                  ;
  ADD HL,HL               ;
  ADD HL,DE               ;
  JP C,REPORT_4           ; Give the report 'Out of memory' if required.
  PUSH DE                 ; Save the element byte total.
  PUSH BC                 ; Save the dimension counter and the discriminator byte.
  PUSH HL                 ; Save the overall length also.
  LD B,H                  ; Move the overall length to BC.
  LD C,L                  ;
; The required amount of room is made available for the new array at the end of the variables area.
  LD HL,(E_LINE)          ; Make the HL register pair point to the '+80-byte' (E-LINE-1).
  DEC HL                  ;
  CALL MAKE_ROOM          ; The room is made available.
  INC HL                  ; HL is made to point to the first new location.
; The parameters are now entered.
  LD (HL),A               ; The letter, suitably marked, is entered first.
  POP BC                  ; The overall length is fetched and decreased by '3'.
  DEC BC                  ;
  DEC BC                  ;
  DEC BC                  ;
  INC HL                  ; Advance HL.
  LD (HL),C               ; Enter the low length.
  INC HL                  ; Advance HL.
  LD (HL),B               ; Enter the high length.
  POP BC                  ; Fetch the dimension counter.
  LD A,B                  ; Move it to the A register.
  INC HL                  ; Advance HL.
  LD (HL),A               ; Enter the dimension count.
; The elements of the new array are now 'cleared'.
  LD H,D                  ; HL is made to point to the last location of the array and DE to the location before that one.
  LD L,E                  ;
  DEC DE                  ;
  LD (HL),$00             ; Enter a zero into the last location but overwrite it with 'space' if dealing with an array of strings.
  BIT 6,C                 ;
  JR Z,DIM_CLEAR          ;
  LD (HL)," "             ;
DIM_CLEAR:
  POP BC                  ; Fetch the element byte total.
  LDDR                    ; Clear the array + one extra location.
; The 'dimension sizes' are now entered.
DIM_SIZES:
  POP BC                  ; Get a dimension size.
  LD (HL),B               ; Enter the high byte.
  DEC HL                  ; Back one.
  LD (HL),C               ; Enter the low byte.
  DEC HL                  ; Back one.
  DEC A                   ; Decrease the dimension counter.
  JR NZ,DIM_SIZES         ; Repeat the operation until all the dimensions have been considered; then return.
  RET                     ;

; THE 'ALPHANUM' SUBROUTINE
;
; Used by the routines at S_ALPHNUM, LOOK_VARS and LET.
;
; This subroutine returns with the carry flag set if the present value of the A register denotes a valid digit or letter.
;
;   A Character code
; O:F Carry flag set if the character is alphanumeric
ALPHANUM:
  CALL NUMERIC            ; Test for a digit; carry will be reset for a digit.
  CCF                     ; Complement the carry flag.
  RET C                   ; Return if a digit; otherwise continue on into ALPHA.

; THE 'ALPHA' SUBROUTINE
;
; Used by the routines at DEF_FN, INPUT, S_FN_SBRN, LOOK_VARS and usr.
;
; The routine at ALPHANUM continues here.
;
; This subroutine returns with the carry flag set if the present value of the A register denotes a valid letter of the alphabet.
;
;   A Character code
; O:F Carry flag set if the character is a letter (A-Z, a-z)
ALPHA:
  CP "A"                  ; Test against +41, the code for 'A'.
  CCF                     ; Complement the carry flag.
  RET NC                  ; Return if not a valid character code.
  CP $5B                  ; Test against +5B, 1 more than the code for 'Z'.
  RET C                   ; Return if an upper case letter.
  CP "a"                  ; Test against +61, the code for 'a'.
  CCF                     ; Complement the carry flag.
  RET NC                  ; Return if not a valid character code.
  CP $7B                  ; Test against +7B, 1 more than the code for 'z'.
  RET                     ; Finished.

; THE 'DECIMAL TO FLOATING POINT' SUBROUTINE
;
; Used by the routine at S_DECIMAL.
;
; As part of syntax checking decimal numbers that occur in a BASIC line are converted to their floating-point forms. This subroutine reads the decimal number digit by digit and gives its result as a 'last value' on the calculator stack. But first it deals with the alternative notation BIN, which introduces a sequence of 0's and 1's giving the binary representation of the required number.
;
; A Code of the first character in the number
DEC_TO_FP:
  CP $C4                  ; Is the character a 'BIN'?
  JR NZ,NOT_BIN           ; Jump if it is not 'BIN'.
  LD DE,$0000             ; Initialise result to zero in DE.
BIN_DIGIT:
  RST $20                 ; Get the next character.
  SUB "1"                 ; Subtract the character code for '1'.
  ADC A,$00               ; 0 now gives 0 with carry set; 1 gives 0 with carry reset.
  JR NZ,BIN_END           ; Any other character causes a jump to BIN_END and will be checked for syntax during or after scanning.
  EX DE,HL                ; Result so far to HL now.
  CCF                     ; Complement the carry flag.
  ADC HL,HL               ; Shift the result left, with the carry going to bit 0.
  JP C,REPORT_6           ; Report overflow if more than 65535.
  EX DE,HL                ; Return the result so far to DE.
  JR BIN_DIGIT            ; Jump back for next 0 or 1.
BIN_END:
  LD B,D                  ; Copy result to BC for stacking.
  LD C,E                  ;
  JP STACK_BC             ; Jump forward to stack the result.
; For other numbers, first any integer part is converted; if the next character is a decimal, then the decimal fraction is considered.
NOT_BIN:
  CP "."                  ; Is the first character a '.'?
  JR Z,DECIMAL            ; If so, jump forward.
  CALL INT_TO_FP          ; Otherwise, form a 'last value' of the integer.
  CP "."                  ; Is the next character a '.'?
  JR NZ,E_FORMAT          ; Jump forward to see if it is an 'E'.
  RST $20                 ; Get the next character.
  CALL NUMERIC            ; Is it a digit?
  JR C,E_FORMAT           ; Jump if not (e.g. 1.E4 is allowed).
  JR DEC_STO_1            ; Jump forward to deal with the digits after the decimal point.
DECIMAL:
  RST $20                 ; If the number started with a decimal, see if the next character is a digit.
  CALL NUMERIC            ;
DEC_RPT_C:
  JP C,REPORT_C           ; Report the error if it is not.
  RST $28                 ; Use the calculator to stack zero as the integer part of such numbers.
  DEFB $A0                ; stk_zero
  DEFB $38                ; end_calc
DEC_STO_1:
  RST $28                 ; Use the calculator to copy the number 1 to mem-0.
  DEFB $A1                ; stk_one
  DEFB $C0                ; st_mem_0
  DEFB $02                ; delete
  DEFB $38                ; end_calc
; For each passage of the following loop, the number (N) saved in the memory area mem-0 is fetched, divided by 10 and restored, i.e. N goes from 1 to .1 to .01 to .001 etc. The present digit (D) is multiplied by N/10 and added to the 'last value' (V), giving V+D*N/10.
NXT_DGT_1:
  RST $18                 ; Get the present character.
  CALL STK_DIGIT          ; If it is a digit (D) then stack it.
  JR C,E_FORMAT           ; If not jump forward.
  RST $28                 ; Now use the calculator.
  DEFB $E0                ; get_mem_0: V, D, N
  DEFB $A4                ; stk_ten: V, D, N, 10
  DEFB $05                ; division: V, D, N/10
  DEFB $C0                ; st_mem_0: V, D, N/10 (N/10 is copied to mem-0)
  DEFB $04                ; multiply: V, D*N/10
  DEFB $0F                ; addition: V+D*N/10
  DEFB $38                ; end_calc
  RST $20                 ; Get the next character.
  JR NXT_DGT_1            ; Jump back (one more byte than needed) to consider it.
; Next consider any 'E notation', i.e. the form xEm or xem where m is a positive or negative integer.
E_FORMAT:
  CP "E"                  ; Is the present character an 'E'?
  JR Z,SIGN_FLAG          ; Jump forward if it is.
  CP "e"                  ; Is it an 'e'?
  RET NZ                  ; Finished unless it is so.
SIGN_FLAG:
  LD B,$FF                ; Use B as a sign flag, +FF for '+'.
  RST $20                 ; Get the next character.
  CP "+"                  ; Is it a '+'?
  JR Z,SIGN_DONE          ; Jump forward.
  CP "-"                  ; Is it a '-'?
  JR NZ,ST_E_PART         ; Jump if neither '+' nor '-'.
  INC B                   ; Change the sign of the flag.
SIGN_DONE:
  RST $20                 ; Point to the first digit.
ST_E_PART:
  CALL NUMERIC            ; Is it indeed a digit?
  JR C,DEC_RPT_C          ; Report the error if not.
  PUSH BC                 ; Save the flag in B briefly.
  CALL INT_TO_FP          ; Stack ABS m, where m is the exponent.
  CALL FP_TO_A            ; Transfer ABS m to A.
  POP BC                  ; Restore the sign flag to B.
  JP C,REPORT_6           ; Report the overflow now if ABS m is greater than 255 or indeed greater than 127 (other values greater than about 39 will be detected later).
  AND A                   ;
  JP M,REPORT_6           ;
  INC B                   ; Test the sign flag in B; '+' (i.e. +FF) will now set the zero flag.
  JR Z,E_FP_JUMP          ; Jump if sign of m is '+'.
  NEG                     ; Negate m if sign is '-'.
E_FP_JUMP:
  JP e_to_fp              ; Jump to assign to the 'last value' the result of x*10↑m.

; THE 'NUMERIC' SUBROUTINE
;
; Used by the routines at OUT_SP_2, ALPHANUM, DEC_TO_FP and STK_DIGIT.
;
; This subroutine returns with the carry flag reset if the present value of the A register denotes a valid digit.
;
;   A Character code
; O:F Carry flag reset if the character is a digit (0-9)
NUMERIC:
  CP "0"                  ; Test against +30, the code for '0'.
  RET C                   ; Return if not a valid character code.
  CP $3A                  ; Test against the upper limit.
  CCF                     ; Complement the carry flag.
  RET                     ; Finished.

; THE 'STK-DIGIT' SUBROUTINE
;
; Used by the routines at DEC_TO_FP and INT_TO_FP.
;
; This subroutine simply returns if the current value held in the A register does not represent a digit but if it does then the floating-point form for the digit becomes the 'last value' on the calculator stack.
;
;   A Character code
; O:F Carry flag reset if the character is a digit (0-9)
STK_DIGIT:
  CALL NUMERIC            ; Is the character a digit?
  RET C                   ; Return if not.
  SUB "0"                 ; Replace the code by the actual digit.
; This routine continues into STACK_A.

; THE 'STACK-A' SUBROUTINE
;
; Used by the routines at POINT_SUB, DRAW, CD_PRMS1, S_ATTR_S, peek, code and ln.
;
; The routine at STK_DIGIT continues here.
;
; This subroutine gives the floating-point form for the absolute binary value currently held in the A register.
;
; A Value to stack
STACK_A:
  LD C,A                  ; Transfer the value to the C register.
  LD B,$00                ; Clear the B register.
; This routine continues into STACK_BC.

; THE 'STACK-BC' SUBROUTINE
;
; Used by the routines at S_RND, DEC_TO_FP, usr and len.
;
; The routine at STACK_A continues here.
;
; This subroutine gives the floating-point form for the absolute binary value currently held in the BC register pair.
;
; The form used in this and hence in the two previous subroutines as well is the one reserved in the Spectrum for small integers n, where -65535<=n<=65535. The first and fifth bytes are zero; the third and fourth bytes are the less significant and more significant bytes of the 16 bit integer n in two's complement form (if n is negative, these two bytes hold 65536+n); and the second byte is a sign byte, +00 for '+' and +FF for '-'.
;
; BC Value to stack
STACK_BC:
  LD IY,ERR_NR            ; Re-initialise IY to ERR-NR.
  XOR A                   ; Clear the A register.
  LD E,A                  ; And the E register, to indicate '+'.
  LD D,C                  ; Copy the less significant byte to D.
  LD C,B                  ; And the more significant byte to C.
  LD B,A                  ; Clear the B register.
  CALL STK_STORE          ; Now stack the number.
  RST $28                 ; Use the calculator to make HL point to STKEND-5.
  DEFB $38                ; end_calc
  AND A                   ; Clear the carry flag.
  RET                     ; Finished.

; THE 'INTEGER TO FLOATING-POINT' SUBROUTINE
;
; Used by the routines at E_LINE_NO and DEC_TO_FP.
;
; This subroutine returns a 'last value' on the calculator stack that is the result of converting an integer in a BASIC line, i.e. the integer part of the decimal number or the line number, to its floating-point form.
;
; Repeated calls to CH_ADD_1 fetch each digit of the integer in turn. An exit is made when a code that does not represent a digit has been fetched.
;
;   A Code of the current character
; O:A Code of the next non-digit character
INT_TO_FP:
  PUSH AF                 ; Save the first digit - in A.
  RST $28                 ; Use the calculator.
  DEFB $A0                ; stk_zero: (the 'last value' is now zero)
  DEFB $38                ; end_calc
  POP AF                  ; Restore the first digit.
; Now a loop is set up. As long as the code represents a digit then the floating-point form is found and stacked under the 'last value' (V, initially zero). V is then multiplied by 10 and added to the 'digit' to form a new 'last value' which is carried back to the start of the loop.
NXT_DGT_2:
  CALL STK_DIGIT          ; If the code represents a digit (D) then stack the floating-point form; otherwise return.
  RET C                   ;
  RST $28                 ; Use the calculator.
  DEFB $01                ; exchange: D, V
  DEFB $A4                ; stk_ten: D, V, 10
  DEFB $04                ; multiply: D, 10*V
  DEFB $0F                ; addition: D+10*V
  DEFB $38                ; end_calc: D+10*V (this is 'V' for the next pass through the loop)
  CALL CH_ADD_1           ; The next code goes into A.
  JR NXT_DGT_2            ; Loop back with this code.

; THE 'E-FORMAT TO FLOATING-POINT' SUBROUTINE (offset +3C)
;
; Used by the routines at DEC_TO_FP and PRINT_FP.
;
; The address of this routine is found in the table of addresses.
;
; This subroutine gives a 'last value' on the top of the calculator stack that is the result of converting a number given in the form xEm, where m is a positive or negative integer. The subroutine is entered with x at the top of the calculator stack and m in the A register.
;
; The method used is to find the absolute value of m, say p, and to multiply or divide x by 10↑p according to whether m is positive or negative.
;
; To achieve this, p is shifted right until it is zero, and x is multiplied or divided by 10↑(2↑n) for each set bit b(n) of p. Since p is never much more than 39, bits 6 and 7 of p will not normally be set.
;
; A Exponent (m)
e_to_fp:
  RLCA                    ; Test the sign of m by rotating bit 7 of A into the carry without changing A.
  RRCA                    ;
  JR NC,E_SAVE            ; Jump if m is positive.
  CPL                     ; Negate m in A without disturbing the carry flag.
  INC A                   ;
E_SAVE:
  PUSH AF                 ; Save m in A briefly.
  LD HL,MEMBOT            ; This is MEMBOT; a sign flag is now stored in the first byte of mem-0, i.e. 0 for '+' and 1 for '-'.
  CALL FP_0_1             ;
  RST $28                 ; The stack holds x.
  DEFB $A4                ; stk_ten: x, 10
  DEFB $38                ; end_calc: x, 10
  POP AF                  ; Restore m in A.
E_LOOP:
  SRL A                   ; In the loop, shift out the next bit of m, modifying the carry and zero flags appropriately; jump if carry reset.
  JR NC,E_TST_END         ;
  PUSH AF                 ; Save the rest of m and the flags.
  RST $28                 ; The stack holds x' and 10↑(2↑n), where x' is an interim stage in the multiplication of x by 10↑m, and n=0, 1, 2, 3, 4 or 5.
  DEFB $C1                ; st_mem_1: (10↑(2↑n) is copied to mem-1)
  DEFB $E0                ; get_mem_0: x', 10↑(2↑n), (1/0)
  DEFB $00                ; jump_true to E_DIVSN: x', 10↑(2↑n)
  DEFB $04                ;
  DEFB $04                ; multiply: x'*10↑(2↑n)=x"
  DEFB $33                ; jump to E_FETCH: x''
  DEFB $02                ;
E_DIVSN:
  DEFB $05                ; division: x/10↑(2↑n)=x'' (x'' is x'*10↑(2↑n) or x'/10↑(2↑n) according as m is '+' or '-')
E_FETCH:
  DEFB $E1                ; get_mem_1: x'', 10↑(2↑n)
  DEFB $38                ; end_calc: x'', 10↑(2↑n)
  POP AF                  ; Restore the rest of m in A, and the flags.
E_TST_END:
  JR Z,E_END              ; Jump if m has been reduced to zero.
  PUSH AF                 ; Save the rest of m in A.
  RST $28                 ; x'', 10↑(2↑n)
  DEFB $31                ; duplicate: x'', 10↑(2↑n), 10↑(2↑n)
  DEFB $04                ; multiply: x'', 10↑(2↑(n+1))
  DEFB $38                ; end_calc: x'', 10↑(2↑(n+1))
  POP AF                  ; Restore the rest of m in A.
  JR E_LOOP               ; Jump back for all bits of m.
E_END:
  RST $28                 ; Use the calculator to delete the final power of 10 reached leaving the 'last value' x*10↑m on the stack.
  DEFB $02                ; delete
  DEFB $38                ; end_calc
  RET

; THE 'INT-FETCH' SUBROUTINE
;
; Used by the routines at FP_TO_BC, PRINT_FP, multiply, re_stack and negate.
;
; This subroutine collects in DE a small integer n (-65535<=n<=65535) from the location addressed by HL, i.e. n is normally the first (or second) number at the top of the calculator stack; but HL can also access (by exchange with DE) a number which has been deleted from the stack.
;
; The subroutine does not itself delete the number from the stack or from memory; it returns HL pointing to the fourth byte of the number in its original position.
;
;   HL Address of the first byte of the value on the calculator stack
; O:C Sign byte
; O:DE The value
; O:HL Address of the fourth byte of the value
INT_FETCH:
  INC HL                  ; Point to the sign byte of the number.
  LD C,(HL)               ; Copy the sign byte to C.
; The following mechanism will two's complement the number if it is negative (C is +FF) but leave it unaltered if it is positive (C is +00).
  INC HL                  ; Point to the less significant byte.
  LD A,(HL)               ; Collect the byte in A.
  XOR C                   ; One's complement it if negative.
  SUB C                   ; This adds 1 for negative numbers; it sets the carry unless the byte was 0.
  LD E,A                  ; Less significant byte to E now.
  INC HL                  ; Point to the more significant byte.
  LD A,(HL)               ; Collect it in A.
  ADC A,C                 ; Finish two's complementing in the case of a negative number; note that the carry is always left reset.
  XOR C                   ;
  LD D,A                  ; More significant byte to D now.
  RET                     ; Finished.

; THE 'POSITIVE-INT-STORE' SUBROUTINE
P_INT_STO:
  LD C,$00                ; This (unused) entry point would store a number known to be positive.
; This routine continues into INT_STORE.

; THE 'INT-STORE' SUBROUTINE
;
; Used by the routines at multiply, truncate, negate and sgn.
;
; This subroutine stores a small integer n (-65535<=n<=65535) in the location addressed by HL and the four following locations, i.e. n replaces the first (or second) number at the top of the calculator stack. The subroutine returns HL pointing to the first byte of n on the stack.
;
; C Sign byte
; DE Value to store
; HL Address of the first byte of the slot on the calculator stack
INT_STORE:
  PUSH HL                 ; The pointer to the first location is saved.
  LD (HL),$00             ; The first byte is set to zero.
  INC HL                  ; Point to the second location.
  LD (HL),C               ; Enter the second byte.
; The same mechanism is now used as in INT_FETCH to two's complement negative numbers. This is needed e.g. before and after the multiplication of small integers. Addition is however performed without any further two's complementing before or afterwards.
  INC HL                  ; Point to the third location.
  LD A,E                  ; Collect the less significant byte.
  XOR C                   ; Two's complement it if the number is negative.
  SUB C                   ;
  LD (HL),A               ; Store the byte.
  INC HL                  ; Point to the fourth location.
  LD A,D                  ; Collect the more significant byte.
  ADC A,C                 ; Two's complement it if the number is negative.
  XOR C                   ;
  LD (HL),A               ; Store the byte.
  INC HL                  ; Point to the fifth location.
  LD (HL),$00             ; The fifth byte is set to zero.
  POP HL                  ; Return with HL pointing to the first byte of n on the stack.
  RET                     ;

; THE 'FLOATING-POINT TO BC' SUBROUTINE
;
; Used by the routines at E_LINE_NO, FIND_INT1, S_RND and FP_TO_A.
;
; This subroutine is used to compress the floating-point 'last value' on the calculator stack into the BC register pair. If the result is too large, i.e. greater than 65536, then the subroutine returns with the carry flag set. If the 'last value' is negative then the zero flag is reset. The low byte of the result is also copied to the A register.
;
; O:A LSB of the value (same as C)
; O:BC Last value from the calculator stack
; O:F Carry flag set on overflow
; O:F Zero flag set if the value is positive, reset if negative
FP_TO_BC:
  RST $28                 ; Use the calculator to make HL point to STKEND-5.
  DEFB $38                ; end_calc
  LD A,(HL)               ; Collect the exponent byte of the 'last value'; jump if it is zero, indicating a 'small integer'.
  AND A                   ;
  JR Z,FP_DELETE          ;
  RST $28                 ; Now use the calculator to round the 'last value' (V) to the nearest integer, which also changes it to 'small integer' form on the calculator stack if that is possible, i.e. if -65535.5<=V<65535.5.
  DEFB $A2                ; stk_half: V, 0.5
  DEFB $0F                ; addition: V+0.5
  DEFB $27                ; int: INT (V+0.5)
  DEFB $38                ; end_calc
FP_DELETE:
  RST $28                 ; Use the calculator to delete the integer from the stack; DE still points to it in memory (at STKEND).
  DEFB $02                ; delete
  DEFB $38                ; end_calc
  PUSH HL                 ; Save both stack pointers.
  PUSH DE                 ;
  EX DE,HL                ; HL now points to the number.
  LD B,(HL)               ; Copy the first byte to B.
  CALL INT_FETCH          ; Copy bytes 2, 3 and 4 to C, E and D.
  XOR A                   ; Clear the A register.
  SUB B                   ; This sets the carry unless B is zero.
  BIT 7,C                 ; This sets the zero flag if the number is positive (NZ denotes negative).
  LD B,D                  ; Copy the high byte to B.
  LD C,E                  ; And the low byte to C.
  LD A,E                  ; Copy the low byte to A too.
  POP DE                  ; Restore the stack pointers.
  POP HL                  ;
  RET                     ; Finished.

; THE 'LOG(2↑A)' SUBROUTINE
;
; Used by the routine at PRINT_FP.
;
; This subroutine calculates the approximate number of digits before the decimal in x, the number to be printed, or, if there are no digits before the decimal, then the approximate number of leading zeros after the decimal. It is entered with the A register containing e', the true exponent of x, or e'-2, and calculates z=log to the base 10 of (2↑A). It then sets A equal to ABS INT (z+0.5), as required, using FP_TO_A for this purpose.
;
;   A e' (true exponent) or e'-2
; O:A INT log (2↑A)
LOG_2_A:
  LD D,A                  ; The integer A is stacked, either as 00 00 A 00 00 (for positive A) or as 00 FF A FF 00 (for negative A).
  RLA                     ;
  SBC A,A                 ;
  LD E,A                  ; These bytes are first loaded into A, E, D, C, B and then STK_STORE is called to put the number on the calculator stack.
  LD C,A                  ;
  XOR A                   ;
  LD B,A                  ;
  CALL STK_STORE          ;
  RST $28                 ; The calculator is used.
  DEFB $34                 ; stk_data: log 2 to the base 10 is now stacked
  DEFB $EF,$1A,$20,$9A,$85 ;
  DEFB $04                ; multiply: A*log 2 i.e. log (2↑A)
  DEFB $27                ; int: INT log (2↑A)
  DEFB $38                ; end_calc
; The subroutine continues into FP_TO_A to complete the calculation.

; THE 'FLOATING-POINT TO A' SUBROUTINE
;
; Used by the routines at TWO_PARAM, FIND_INT1, STK_TO_A, CD_PRMS1, DEC_TO_FP, PRINT_FP, chrs and exp.
;
; The routine at LOG_2_A continues here.
;
; This short but vital subroutine is called at least 8 times for various purposes. It uses FP_TO_BC to get the 'last value' into the A register where this is possible. It therefore tests whether the modulus of the number rounds to more than 255 and if it does the subroutine returns with the carry flag set. Otherwise it returns with the modulus of the number, rounded to the nearest integer, in the A register, and the zero flag set to imply that the number was positive, or reset to imply that it
; was negative.
;
; O:A Last value from the calculator stack
; O:F Carry flag set on overflow
; O:F Zero flag set if the value is positive, reset if negative
FP_TO_A:
  CALL FP_TO_BC           ; Compress the 'last value' into BC.
  RET C                   ; Return if out of range already.
  PUSH AF                 ; Save the result and the flags.
  DEC B                   ; Again it will be out of range if the B register does not hold zero.
  INC B                   ;
  JR Z,FP_A_END           ; Jump if in range.
  POP AF                  ; Fetch the result and the flags.
  SCF                     ; Signal the result is out of range.
  RET                     ; Finished - unsuccessful.
FP_A_END:
  POP AF                  ; Fetch the result and the flags.
  RET                     ; Finished - successful.

; THE 'PRINT A FLOATING-POINT NUMBER' SUBROUTINE
;
; Used by the routines at PR_ITEM_1 and str.
;
; This subroutine prints x, the 'last value' on the calculator stack. The print format never occupies more than 14 spaces.
;
; The 8 most significant digits of x, correctly rounded, are stored in an ad hoc print buffer in mem-3 and mem-4. Small numbers, numerically less than 1, and large numbers, numerically greater than 2↑27, are dealt with separately. The former are multiplied by 10↑n, where n is the approximate number of leading zeros after the decimal, while the latter are divided by 10↑(n-7), where n is the approximate number of digits before the decimal. This brings all numbers into the middle range, and the
; number of digits required before the decimal is built up in the second byte of mem-5. Finally the printing is done, using E-format if there are more than 8 digits before the decimal or, for small numbers, more than 4 leading zeros after the decimal.
;
; The following program shows the range of print formats:
;
; 10 FOR a=-11 TO 12: PRINT SGN a*9↑a,: NEXT a
;
; i. First the sign of x is taken care of:
;
; * If x is negative, the subroutine jumps to PF_NEGTVE, takes ABS x and prints the minus sign.
; * If x is zero, x is deleted from the calculator stack, a '0' is printed and a return is made from the subroutine.
; * If x is positive, the subroutine just continues.
PRINT_FP:
  RST $28                 ; Use the calculator.
  DEFB $31                ; duplicate: x, x
  DEFB $36                ; less_0: x, (1/0) Logical value of x.
  DEFB $00,$0B            ; jump_true to PF_NEGTVE: x
  DEFB $31                ; duplicate: x, x
  DEFB $37                ; greater_0: x, (1/0) Logical value of x.
  DEFB $00,$0D            ; jump_true to PF_POSTVE: x Hereafter x'=ABS x.
  DEFB $02                ; delete: -
  DEFB $38                ; end_calc: -
  LD A,"0"                ; Enter the character code for '0'.
  RST $10                 ; Print the '0'.
  RET                     ; Finished as the 'last value' is zero.
PF_NEGTVE:
  DEFB $2A                ; abs: x' x'=ABS x.
  DEFB $38                ; end_calc: x'
  LD A,"-"                ; Enter the character code for '-'.
  RST $10                 ; Print the '-'.
  RST $28                 ; Use the calculator again.
PF_POSTVE:
  DEFB $A0                ; stk_zero: The 15 bytes of mem-3, mem-4 and mem-5 are now initialised to zero to be used for a print buffer and two counters.
  DEFB $C3,$C4,$C5        ;
  DEFB $02                ; delete: The stack is cleared, except for x'.
  DEFB $38                ; end_calc: x'
  EXX                     ; HL', which is used to hold calculator offsets (e.g. for 'STR$'), is saved on the machine stack.
  PUSH HL                 ;
  EXX                     ;
; ii. This is the start of a loop which deals with large numbers. Every number x is first split into its integer part i and the fractional part f. If i is a small integer, i.e. if -65535<=i<=65535, it is stored in DE' for insertion into the print buffer.
PF_LOOP:
  RST $28                 ; Use the calculator again.
  DEFB $31                ; duplicate: x', x'
  DEFB $27                ; int: x', INT (x')=i
  DEFB $C2                ; st_mem_2: (i is stored in mem-2).
  DEFB $03                ; subtract: x'-i=f
  DEFB $E2                ; get_mem_2: f, i
  DEFB $01                ; exchange: i, f
  DEFB $C2                ; st_mem_2: (f is stored in mem-2).
  DEFB $02                ; delete: i
  DEFB $38                ; end_calc: i
  LD A,(HL)               ; Is i a small integer (first byte zero) i.e. is ABS i<=65535?
  AND A                   ;
  JR NZ,PF_LARGE          ; Jump if it is not.
  CALL INT_FETCH          ; i is copied to DE (i, like x', >=0).
  LD B,$10                ; B is set to count 16 bits.
  LD A,D                  ; D is copied to A for testing: is it zero?
  AND A                   ;
  JR NZ,PF_SAVE           ; Jump if it is not zero.
  OR E                    ; Now test E.
  JR Z,PF_SMALL           ; Jump if DE is zero: x is a pure fraction.
  LD D,E                  ; Move E to D and set B for 8 bits: D was zero and E was not.
  LD B,$08                ;
PF_SAVE:
  PUSH DE                 ; Transfer DE to DE', via the machine stack, to be moved into the print buffer at PF_BITS.
  EXX                     ;
  POP DE                  ;
  EXX                     ;
  JR PF_BITS              ; Jump forward.
; iii. Pure fractions are multiplied by 10↑n, where n is the approximate number of leading zeros after the decimal; and -n is added to the second byte of mem-5, which holds the number of digits needed before the decimal; a negative number here indicates leading zeros after the decimal.
PF_SMALL:
  RST $28                 ; i (i=zero here)
  DEFB $E2                ; get_mem_2: i, f
  DEFB $38                ; end_calc: i, f
; Note that the stack is now unbalanced. An extra byte 'DEFB +02, delete' is needed immediately after the RST $28. Now an expression like "2"+STR$ 0.5 is evaluated incorrectly as 0.5; the zero left on the stack displaces the "2" and is treated as a null string. Similarly all the string comparisons can yield incorrect values if the second string takes the form STR$ x where x is numerically less than 1; e.g. the expression "50"<STR$ 0.1 yields the logical value "true"; once again "" is used
; instead of "50".
  LD A,(HL)               ; The exponent byte e of f is copied to A.
  SUB $7E                 ; A becomes e minus +7E, i.e. e'+2, where e' is the true exponent of f.
  CALL LOG_2_A            ; The construction A=ABS INT (LOG (2↑A)) is performed (LOG is to base 10); i.e. A=n, say: n is copied from A to D.
  LD D,A                  ;
  LD A,($5CAC)            ; The current count is collected from the second byte of mem-5 and n is subtracted from it.
  SUB D                   ;
  LD ($5CAC),A            ;
  LD A,D                  ; n is copied from D to A.
  CALL e_to_fp            ; y=f*10↑n is formed and stacked.
  RST $28                 ; i, y
  DEFB $31                ; duplicate: i, y, y
  DEFB $27                ; int: i, y, INT (y)=i2
  DEFB $C1                ; st_mem_1: (i2 is copied to mem-1).
  DEFB $03                ; subtract: i, y-i2
  DEFB $E1                ; get_mem_1: i, y-i2, i2
  DEFB $38                ; end_calc: i, f2, i2 (f2=y-i2)
  CALL FP_TO_A            ; i2 is transferred from the stack to A.
  PUSH HL                 ; The pointer to f2 is saved.
  LD ($5CA1),A            ; i2 is stored in the first byte of mem-3: a digit for printing.
  DEC A                   ; i2 will not count as a digit for printing if it is zero; A is manipulated so that zero will produce zero but a non-zero digit will produce 1.
  RLA                     ;
  SBC A,A                 ;
  INC A                   ;
  LD HL,$5CAB             ; The zero or one is inserted into the first byte of mem-5 (the number of digits for printing) and added to the second byte of mem-5 (the number of digits before the decimal).
  LD (HL),A               ;
  INC HL                  ;
  ADD A,(HL)              ;
  LD (HL),A               ;
  POP HL                  ; The pointer to f2 is restored.
  JP PF_FRACTN            ; Jump to store f2 in buffer (HL now points to f2, DE to i2).
; iv. Numbers greater than 2↑27 are similarly multiplied by 2↑(-n+7), reducing the number of digits before the decimal to 8, and the loop is re-entered at PF_LOOP.
PF_LARGE:
  SUB $80                 ; e minus +80 is e', the true exponent of i.
  CP $1C                  ; Is e' less than 28?
  JR C,PF_MEDIUM          ; Jump if it is less.
  CALL LOG_2_A            ; n is formed in A.
  SUB $07                 ; And reduced to n-7.
  LD B,A                  ; Then copied to B.
  LD HL,$5CAC             ; n-7 is added in to the second byte of mem-5, the number of digits required before the decimal in x.
  ADD A,(HL)              ;
  LD (HL),A               ;
  LD A,B                  ; Then i is multiplied by 10↑(-n+7). This will bring it into medium range for printing.
  NEG                     ;
  CALL e_to_fp            ;
  JR PF_LOOP              ; Round the loop again to deal with the now medium-sized number.
; v. The integer part of x is now stored in the print buffer in mem-3 and mem-4.
PF_MEDIUM:
  EX DE,HL                ; DE now points to i, HL to f.
  CALL FETCH_TWO          ; The mantissa of i is now in D', E', D, E.
  EXX                     ; Get the exchange registers.
  SET 7,D                 ; True numerical bit 7 to D'.
  LD A,L                  ; Exponent byte e of i to A.
  EXX                     ; Back to the main registers.
  SUB $80                 ; True exponent e'=e minus +80 to A.
  LD B,A                  ; This gives the required bit count.
; Note that the case where i is a small integer (less than 65536) re-enters here.
PF_BITS:
  SLA E                   ; The mantissa of i is now rotated left and all the bits of i are thus shifted into mem-4 and each byte of mem-4 is decimal adjusted at each shift.
  RL D                    ;
  EXX                     ;
  RL E                    ;
  RL D                    ;
  EXX                     ; Back to the main registers.
  LD HL,$5CAA             ; Address of fifth byte of mem-4 to HL; count of 5 bytes to C.
  LD C,$05                ;
PF_BYTES:
  LD A,(HL)               ; Get the byte of mem-4.
  ADC A,A                 ; Shift it left, taking in the new bit.
  DAA                     ; Decimal adjust the byte.
  LD (HL),A               ; Restore it to mem-4.
  DEC HL                  ; Point to next byte of mem-4.
  DEC C                   ; Decrease the byte count by one.
  JR NZ,PF_BYTES          ; Jump for each byte of mem-4.
  DJNZ PF_BITS            ; Jump for each bit of INT (x).
; Decimal adjusting each byte of mem-4 gave 2 decimal digits per byte, there being at most 9 digits. The digits will now be re-packed, one to a byte, in mem-3 and mem-4, using the instruction 'RLD'.
  XOR A                   ; A is cleared to receive the digits.
  LD HL,$5CA6             ; Source address: first byte of mem-4.
  LD DE,$5CA1             ; Destination: first byte of mem-3.
  LD B,$09                ; There are at most 9 digits.
  RLD                     ; The left nibble of mem-4 is discarded.
  LD C,$FF                ; +FF in C will signal a leading zero, +00 will signal a non-leading zero.
PF_DIGITS:
  RLD                     ; Left nibble of (HL) to A, right nibble of (HL) to left.
  JR NZ,PF_INSERT         ; Jump if digit in A is not zero.
  DEC C                   ; Test for a leading zero: it will now give zero reset.
  INC C                   ;
  JR NZ,PF_TEST_2         ; Jump if it was a leading zero.
PF_INSERT:
  LD (DE),A               ; Insert the digit now.
  INC DE                  ; Point to next destination.
  INC (IY+$71)            ; One more digit for printing, and one more before the decimal.
  INC (IY+$72)            ;
  LD C,$00                ; Change the flag from leading zero to other zero.
PF_TEST_2:
  BIT 0,B                 ; The source pointer needs to be incremented on every second passage through the loop, when B is odd.
  JR Z,PF_ALL_9           ;
  INC HL                  ;
PF_ALL_9:
  DJNZ PF_DIGITS          ; Jump back for all 9 digits.
  LD A,($5CAB)            ; Get counter from the first byte of mem-5: were there 9 digits excluding leading zeros?
  SUB $09                 ;
  JR C,PF_MORE            ; If not, jump to get more digits.
  DEC (IY+$71)            ; Prepare to round: reduce count to 8.
  LD A,$04                ; Compare 9th digit, byte 4 of mem-4, with 4 to set carry for rounding up.
  CP (IY+$6F)             ;
  JR PF_ROUND             ; Jump forward to round up.
PF_MORE:
  RST $28                 ; Use the calculator again.
  DEFB $02                ; delete: - (i is now deleted).
  DEFB $E2                ; get_mem_2: f
  DEFB $38                ; end_calc: f
; vi. The fractional part of x is now stored in the print buffer.
PF_FRACTN:
  EX DE,HL                ; DE now points to f.
  CALL FETCH_TWO          ; The mantissa of f is now in D', E', D, E.
  EXX                     ; Get the exchange registers.
  LD A,$80                ; The exponent of f is reduced to zero, by shifting the bits of f +80 minus e places right, where L' contained e.
  SUB L                   ;
  LD L,$00                ;
  SET 7,D                 ; True numerical bit to bit 7 of D'.
  EXX                     ; Restore the main registers.
  CALL SHIFT_FP           ; Now make the shift.
PF_FRN_LP:
  LD A,(IY+$71)           ; Get the digit count.
  CP $08                  ; Are there already 8 digits?
  JR C,PF_FR_DGT          ; If not, jump forward.
  EXX                     ; If 8 digits, just use f to round i up, rotating D' left to set the carry.
  RL D                    ;
  EXX                     ; Restore main registers and jump forward to round up.
  JR PF_ROUND             ;
PF_FR_DGT:
  LD BC,$0200             ; Initial zero to C, count of 2 to B.
PF_FR_EXX:
  LD A,E                  ; D'E'DE is multiplied by 10 in 2 stages, first DE then DE', each byte by byte in 2 steps, and the integer part of the result is obtained in C to be passed into the print buffer.
  CALL CA_10A_C           ;
  LD E,A                  ;
  LD A,D                  ;
  CALL CA_10A_C           ;
  LD D,A                  ;
  PUSH BC                 ; The count and the result alternate between BC and BC'.
  EXX                     ;
  POP BC                  ;
  DJNZ PF_FR_EXX          ; Loop back once through the exchange registers.
  LD HL,$5CA1             ; The start - 1st byte of mem-3.
  LD A,C                  ; Result to A for storing.
  LD C,(IY+$71)           ; Count of digits so far in number to C.
  ADD HL,BC               ; Address the first empty byte.
  LD (HL),A               ; Store the next digit.
  INC (IY+$71)            ; Step up the count of digits.
  JR PF_FRN_LP            ; Loop back until there are 8 digits.
; vii. The digits stored in the print buffer are rounded to a maximum of 8 digits for printing.
PF_ROUND:
  PUSH AF                 ; Save the carry flag for the rounding.
  LD HL,$5CA1             ; Base address of number: mem-3, byte 1.
  LD C,(IY+$71)           ; Offset (number of digits in number) to BC.
  LD B,$00                ;
  ADD HL,BC               ; Address the last byte of the number.
  LD B,C                  ; Copy C to B as the counter.
  POP AF                  ; Restore the carry flag.
PF_RND_LP:
  DEC HL                  ; This is the last byte of the number.
  LD A,(HL)               ; Get the byte into A.
  ADC A,$00               ; Add in the carry i.e. round up.
  LD (HL),A               ; Store the rounded byte in the buffer.
  AND A                   ; If the byte is 0 or 10, B will be decremented and the final zero (or the 10) will not be counted for printing.
  JR Z,PF_R_BACK          ;
  CP $0A                  ;
  CCF                     ; Reset the carry for a valid digit.
  JR NC,PF_COUNT          ; Jump if carry reset.
PF_R_BACK:
  DJNZ PF_RND_LP          ; Jump back for more rounding or more final zeros.
  LD (HL),$01             ; There is overflow to the left; an extra 1 is needed here.
  INC B                   ;
  INC (IY+$72)            ; It is also an extra digit before the decimal.
PF_COUNT:
  LD (IY+$71),B           ; B now sets the count of the digits to be printed (final zeros will not be printed).
  RST $28                 ; f is to be deleted.
  DEFB $02                ; delete: -
  DEFB $38                ; end_calc: -
  EXX                     ; The calculator offset saved on the stack is restored to HL'.
  POP HL                  ;
  EXX                     ;
; viii. The number can now be printed. First C will be set to hold the number of digits to be printed, not counting final zeros, while B will hold the number of digits required before the decimal.
  LD BC,($5CAB)           ; The counters are set (first two bytes of mem-5).
  LD HL,$5CA1             ; The start of the digits (first byte of mem-3).
  LD A,B                  ; If more than 9, or fewer than minus 4, digits are required before the decimal, then E-format will be needed.
  CP $09                  ;
  JR C,PF_NOT_E           ;
  CP $FC                  ; Fewer than 4 means more than 4 leading zeros after the decimal.
  JR C,PF_E_FRMT          ;
PF_NOT_E:
  AND A                   ; Are there no digits before the decimal? If so, print an initial zero.
  CALL Z,OUT_CODE         ;
; The next entry point is also used to print the digits needed for E-format printing.
PF_E_SBRN:
  XOR A                   ; Start by setting A to zero.
  SUB B                   ; Subtract B: minus will mean there are digits before the decimal; jump forward to print them.
  JP M,PF_OUT_LP          ;
  LD B,A                  ; A is now required as a counter.
  JR PF_DC_OUT            ; Jump forward to print the decimal part.
PF_OUT_LP:
  LD A,C                  ; Copy the number of digits to be printed to A. If A is 0, there are still final zeros to print (B is non-zero), so jump.
  AND A                   ;
  JR Z,PF_OUT_DT          ;
  LD A,(HL)               ; Get a digit from the print buffer.
  INC HL                  ; Point to the next digit.
  DEC C                   ; Decrease the count by one.
PF_OUT_DT:
  CALL OUT_CODE           ; Print the appropriate digit.
  DJNZ PF_OUT_LP          ; Loop back until B is zero.
PF_DC_OUT:
  LD A,C                  ; It is time to print the decimal, unless C is now zero; in that case, return - finished.
  AND A                   ;
  RET Z                   ;
  INC B                   ; Add 1 to B - include the decimal.
  LD A,"."                ; Put the code for '.' into A.
PF_DEC_0S:
  RST $10                 ; Print the '.'.
  LD A,"0"                ; Enter the character code for '0'.
  DJNZ PF_DEC_0S          ; Loop back to print all needed zeros.
  LD B,C                  ; Set the count for all remaining digits.
  JR PF_OUT_LP            ; Jump back to print them.
PF_E_FRMT:
  LD D,B                  ; The count of digits is copied to D.
  DEC D                   ; It is decremented to give the exponent.
  LD B,$01                ; One digit is required before the decimal in E-format.
  CALL PF_E_SBRN          ; All the part of the number before the 'E' is now printed.
  LD A,"E"                ; Enter the character code for 'E'.
  RST $10                 ; Print the 'E'.
  LD C,D                  ; Exponent to C now for printing.
  LD A,C                  ; And to A for testing.
  AND A                   ; Its sign is tested.
  JP P,PF_E_POS           ; Jump if it is positive.
  NEG                     ; Otherwise, negate it in A.
  LD C,A                  ; Then copy it back to C for printing.
  LD A,"-"                ; Enter the character code for '-'.
  JR PF_E_SIGN            ; Jump to print the sign.
PF_E_POS:
  LD A,"+"                ; Enter the character code for '+'.
PF_E_SIGN:
  RST $10                 ; Now print the sign: '+' or '-'.
  LD B,$00                ; BC holds the exponent for printing.
  JP OUT_NUM_1            ; Jump back to print it and finish.

; THE 'CA=10*A+C' SUBROUTINE
;
; This subroutine is called by PRINT_FP to multiply each byte of D'E'DE by 10 and return the integer part of the result in the C register. On entry, the A register contains the byte to be multiplied by 10 and the C register contains the carry over from the previous byte. On return, the A register contains the resulting byte and the C register the carry forward to the next byte.
;
;   A First number (M)
;   C Second number (N)
; O:A LSB of 10*M+N
; O:C MSB of 10*M+N
CA_10A_C:
  PUSH DE                 ; Save whichever DE pair is in use.
  LD L,A                  ; Copy the multiplicand from A to HL.
  LD H,$00                ;
  LD E,L                  ; Copy it to DE too.
  LD D,H                  ;
  ADD HL,HL               ; Double HL.
  ADD HL,HL               ; Double it again.
  ADD HL,DE               ; Add in DE to give HL=5*A.
  ADD HL,HL               ; Double again: now HL=10*A.
  LD E,C                  ; Copy C to DE (D is zero) for addition.
  ADD HL,DE               ; Now HL=10*A+C.
  LD C,H                  ; H is copied to C.
  LD A,L                  ; L is copied to A, completing the task.
  POP DE                  ; The DE register pair is restored.
  RET                     ; Finished.

; THE 'PREPARE TO ADD' SUBROUTINE
;
; Used by the routine at addition.
;
; This subroutine is the first of four subroutines that are used by the main arithmetic operation routines - subtract, addition, multiply and division.
;
; This particular subroutine prepares a floating-point number for addition, mainly by replacing the sign bit with a true numerical bit 1, and negating the number (two's complement) if it is negative. The exponent is returned in the A register and the first byte is set to +00 for a positive number and +FF for a negative number.
;
;   HL Address of the first byte of the number
; O:A Exponent byte
PREP_ADD:
  LD A,(HL)               ; Transfer the exponent to A.
  LD (HL),$00             ; Presume a positive number.
  AND A                   ; If the number is zero then the preparation is already finished.
  RET Z                   ;
  INC HL                  ; Now point to the sign byte.
  BIT 7,(HL)              ; Set the zero flag for positive number.
  SET 7,(HL)              ; Restore the true numeric bit.
  DEC HL                  ; Point to the first byte again.
  RET Z                   ; Positive numbers have been prepared, but negative numbers need to be two's complemented.
  PUSH BC                 ; Save any earlier exponent.
  LD BC,$0005             ; There are 5 bytes to be handled.
  ADD HL,BC               ; Point one past the last byte.
  LD B,C                  ; Transfer the 5 to B.
  LD C,A                  ; Save the exponent in C.
  SCF                     ; Set carry flag for negation.
NEG_BYTE:
  DEC HL                  ; Point to each byte in turn.
  LD A,(HL)               ; Get each byte.
  CPL                     ; One's complement the byte.
  ADC A,$00               ; Add in carry for negation.
  LD (HL),A               ; Restore the byte.
  DJNZ NEG_BYTE           ; Loop 5 times.
  LD A,C                  ; Restore the exponent to A.
  POP BC                  ; Restore any earlier exponent.
  RET                     ; Finished.

; THE 'FETCH TWO NUMBERS' SUBROUTINE
;
; Used by the routines at PRINT_FP, addition, multiply and division.
;
; This subroutine is called by addition, multiply and division to get two numbers from the calculator stack and put them into the registers, including the exchange registers.
;
; On entry to the subroutine the HL register pair points to the first byte of the first number (M) and the DE register pair points to the first byte of the second number (N).
;
; When the subroutine is called from multiply or division the sign of the result is saved in the second byte of the first number.
;
;   A Bit 7 holds the sign bit of the result (when called from multiply or division)
;   DE Address of the first byte of N
;   HL Address of the first byte of M
; O:H'B'C'CB The five bytes of M
; O:L'D'E'DE The five bytes of N
FETCH_TWO:
  PUSH HL                 ; HL is preserved.
  PUSH AF                 ; AF is preserved.
; Call the five bytes of the first number M1, M2, M3, M4 and M5, and the five bytes of the second number N1, N2, N3, N4 and N5.
  LD C,(HL)               ; M1 to C.
  INC HL                  ; Next.
  LD B,(HL)               ; M2 to B.
  LD (HL),A               ; Copy the sign of the result to bit 7 of (HL).
  INC HL                  ; Next.
  LD A,C                  ; M1 to A.
  LD C,(HL)               ; M3 to C.
  PUSH BC                 ; Save M2 and M3 on the machine stack.
  INC HL                  ; Next.
  LD C,(HL)               ; M4 to C.
  INC HL                  ; Next.
  LD B,(HL)               ; M5 to B.
  EX DE,HL                ; HL now points to N1.
  LD D,A                  ; M1 to D.
  LD E,(HL)               ; N1 to E.
  PUSH DE                 ; Save M1 and N1 on the machine stack.
  INC HL                  ; Next.
  LD D,(HL)               ; N2 to D.
  INC HL                  ; Next.
  LD E,(HL)               ; N3 to E.
  PUSH DE                 ; Save N2 and N3 on the machine stack.
  EXX                     ; Get the exchange registers.
  POP DE                  ; N2 to D' and N3 to E'.
  POP HL                  ; M1 to H' and N1 to L'.
  POP BC                  ; M2 to B' and M3 to C'.
  EXX                     ; Get the original set of registers.
  INC HL                  ; Next.
  LD D,(HL)               ; N4 to D.
  INC HL                  ; Next.
  LD E,(HL)               ; N5 to E.
  POP AF                  ; Restore the original AF.
  POP HL                  ; Restore the original HL.
  RET                     ; Finished.
; Summary:
;
; * M1 - M5 are in H', B', C', C, B.
; * N1 - N5 are in L', D', E', D, E.
; * HL points to the first byte of the first number.

; THE 'SHIFT ADDEND' SUBROUTINE
;
; Used by the routines at PRINT_FP and addition.
;
; This subroutine shifts a floating-point number up to 32 places right to line it up properly for addition. The number with the smaller exponent has been put in the addend position before this subroutine is called. Any overflow to the right, into the carry, is added back into the number. If the exponent difference is greater than 32, or the carry ripples right back to the beginning of the number then the number is set to zero so that the addition will not alter the other number (the augend).
;
; A Number of shifts to perform
; D'E'DE Mantissa of number to shift right
; L' Sign byte of number to shift right (+00 or +FF)
SHIFT_FP:
  AND A                   ; If the exponent difference is zero, the subroutine returns at once.
  RET Z                   ;
  CP $21                  ; If the difference is greater than +20, jump forward.
  JR NC,ADDEND_0          ;
  PUSH BC                 ; Save BC briefly.
  LD B,A                  ; Transfer the exponent difference to B to count the shifts right.
ONE_SHIFT:
  EXX                     ; Arithmetic shift right for L', preserving the sign marker bits.
  SRA L                   ;
  RR D                    ; Rotate right with carry D', E', D and E, thereby shifting the whole five bytes of the number to the right as many times as B counts.
  RR E                    ;
  EXX                     ;
  RR D                    ;
  RR E                    ;
  DJNZ ONE_SHIFT          ; Loop back until B reaches zero.
  POP BC                  ; Restore the original BC.
  RET NC                  ; Done if no carry to retrieve.
  CALL ADD_BACK           ; Retrieve carry.
  RET NZ                  ; Return unless the carry rippled right back. (In this case there is nothing to add.)
ADDEND_0:
  EXX                     ; Fetch L', D' and E'.
  XOR A                   ; Clear the A register.
; This entry point is used by the routine at multiply.
ZEROS_4_5:
  LD L,$00                ; Set the addend to zero in D', E', D and E, together with its marker byte (sign indicator) L', which was +00 for a positive number and +FF for a negative number. This produces only 4 zero bytes when called for near underflow by multiply.
  LD D,A                  ;
  LD E,L                  ;
  EXX                     ;
  LD DE,$0000             ;
  RET                     ; Finished.

; THE 'ADD-BACK' SUBROUTINE
;
; Used by the routines at SHIFT_FP and multiply.
;
; This subroutine adds back into the number any carry which has overflowed to the right. In the extreme case, the carry ripples right back to the left of the number.
;
; When this subroutine is called during addition, this ripple means that a mantissa of 0.5 was shifted a full 32 places right, and the addend will now be set to zero; when called from multiply, it means that the exponent must be incremented, and this may result in overflow.
;
;   D'E'DE Mantissa of number shifted right
; O:F Zero flag set on overflow (mantissa was FFFFFFFF)
ADD_BACK:
  INC E                   ; Add carry to rightmost byte.
  RET NZ                  ; Return if no overflow to left.
  INC D                   ; Continue to the next byte.
  RET NZ                  ; Return if no overflow to left.
  EXX                     ; Get the next byte.
  INC E                   ; Increment it too.
  JR NZ,ALL_ADDED         ; Jump if no overflow.
  INC D                   ; Increment the last byte.
ALL_ADDED:
  EXX                     ; Restore the original registers.
  RET                     ; Finished.

; THE 'SUBTRACTION' OPERATION (offset +03)
;
; Used by the routine at compare.
;
; The address of this routine is found in the table of addresses. It is called via the calculator literal +03 by the routines at BEEP, NEXT_LOOP, CIRCLE, DRAW, CD_PRMS1, S_RND, PRINT_FP, series, n_mod_m, int, exp, ln, get_argt, cos, sin, atn, asn and acs. It is also called indirectly via fp_calc_2.
;
; This subroutine simply changes the sign of the subtrahend and carries on into addition.
;
; Note that HL points to the minuend and DE points to the subtrahend. (See addition for more details.)
;
; DE Address of the first byte of the subtrahend
; HL Address of the first byte of the minuend
subtract:
  EX DE,HL                ; Exchange the pointers.
  CALL negate             ; Change the sign of the subtrahend.
  EX DE,HL                ; Exchange the pointers back and continue into addition.

; THE 'ADDITION' OPERATION (offset +0F)
;
; The address of this routine is found in the table of addresses. It is called via the calculator literal +0F by the routines at BEEP, NEXT, CIRCLE, DRAW, CD_PRMS1, S_RND, DEC_TO_FP, INT_TO_FP, FP_TO_BC, series, exp, ln, get_argt, sin, atn and asn. It is also called indirectly via fp_calc_2, and the routine at subtract continues here.
;
; The first of three major arithmetical subroutines, this subroutine carries out the floating-point addition of two numbers, each with a 4-byte mantissa and a 1-byte exponent. In these three subroutines, the two numbers at the top of the calculator stack are added/multiplied/divided to give one number at the top of the calculator stack, a 'last value'.
;
; HL points to the second number from the top, the augend/multiplier/dividend. DE points to the number at the top of the calculator stack, the addend/multiplicand/divisor. Afterwards HL points to the resultant 'last value' whose address can also be considered to be STKEND-5.
;
; But the addition subroutine first tests whether the 2 numbers to be added are 'small integers'. If they are, it adds them quite simply in HL and BC, and puts the result directly on the stack. No two's complementing is needed before or after the addition, since such numbers are held on the stack in two's complement form, ready for addition.
;
; DE Address of the first byte of the addend/multiplicand/divisor
; HL Address of the first byte of the augend/multiplier/dividend
addition:
  LD A,(DE)               ; Test whether the first bytes of both numbers are zero.
  OR (HL)                 ;
  JR NZ,FULL_ADDN         ; If not, jump for full addition.
  PUSH DE                 ; Save the pointer to the second number.
  INC HL                  ; Point to the second byte of the first number and save that pointer too.
  PUSH HL                 ;
  INC HL                  ; Point to the less significant byte.
  LD E,(HL)               ; Fetch it in E.
  INC HL                  ; Point to the more significant byte.
  LD D,(HL)               ; Fetch it in D.
  INC HL                  ; Move on to the second byte of the second number.
  INC HL                  ;
  INC HL                  ;
  LD A,(HL)               ; Fetch it in A (this is the sign byte).
  INC HL                  ; Point to the less significant byte.
  LD C,(HL)               ; Fetch it in C.
  INC HL                  ; Point to the more significant byte.
  LD B,(HL)               ; Fetch it in B.
  POP HL                  ; Fetch the pointer to the sign byte of the first number; put it in DE, and the number in HL.
  EX DE,HL                ;
  ADD HL,BC               ; Perform the addition: result in HL.
  EX DE,HL                ; Result to DE, sign byte to HL.
  ADC A,(HL)              ; Add the sign bytes and the carry into A; this will detect any overflow.
  RRCA                    ;
  ADC A,$00               ; A non-zero A now indicates overflow.
  JR NZ,ADDN_OFLW         ; Jump to reset the pointers and to do full addition.
  SBC A,A                 ; Define the correct sign byte for the result.
  LD (HL),A               ; Store it on the stack.
  INC HL                  ; Point to the next location.
  LD (HL),E               ; Store the low byte of the result.
  INC HL                  ; Point to the next location.
  LD (HL),D               ; Store the high byte of the result.
  DEC HL                  ; Move the pointer back to address the first byte of the result.
  DEC HL                  ;
  DEC HL                  ;
  POP DE                  ; Restore STKEND to DE.
  RET                     ; Finished.
; Note that the number -65536 can arise here in the form 00 FF 00 00 00 as the result of the addition of two smaller negative integers, e.g. -65000 and -536. It is simply stacked in this form. This is a mistake. The Spectrum system cannot handle this number.
;
; Most functions treat it as zero, and it is printed as -1E-38, obtained by treating is as 'minus zero' in an illegitimate format.
;
; One possible remedy would be to test for this number at about byte 3032 and, if it is present, to make the second byte +80 and the first byte +91, so producing the full five-byte floating-point form of the number, i.e. 91 80 00 00 00, which causes no problems. See also the remarks in 'truncate'.
ADDN_OFLW:
  DEC HL                  ; Restore the pointer to the first number.
  POP DE                  ; Restore the pointer to the second number.
FULL_ADDN:
  CALL RE_ST_TWO          ; Re-stack both numbers in full five-byte floating-point form.
; The full addition subroutine first calls PREP_ADD for each number, then gets the two numbers from the calculator stack and puts the one with the smaller exponent into the addend position. It then calls SHIFT_FP to shift the addend up to 32 decimal places right to line it up for addition. The actual addition is done in a few bytes, a single shift is made for carry (overflow to the left) if needed, the result is two's complemented if negative, and any arithmetic overflow is reported; otherwise
; the subroutine jumps to TEST_NORM to normalise the result and return it to the stack with the correct sign bit inserted into the second byte.
  EXX                     ; Exchange the registers.
  PUSH HL                 ; Save the next literal address.
  EXX                     ; Exchange the registers.
  PUSH DE                 ; Save pointer to the addend.
  PUSH HL                 ; Save pointer to the augend.
  CALL PREP_ADD           ; Prepare the augend.
  LD B,A                  ; Save its exponent in B.
  EX DE,HL                ; Exchange the pointers.
  CALL PREP_ADD           ; Prepare the addend.
  LD C,A                  ; Save its exponent in C.
  CP B                    ; If the first exponent is smaller, keep the first number in the addend position; otherwise change the exponents and the pointers back again.
  JR NC,SHIFT_LEN         ;
  LD A,B                  ;
  LD B,C                  ;
  EX DE,HL                ;
SHIFT_LEN:
  PUSH AF                 ; Save the larger exponent in A.
  SUB B                   ; The difference between the exponents is the length of the shift right.
  CALL FETCH_TWO          ; Get the two numbers from the stack.
  CALL SHIFT_FP           ; Shift the addend right.
  POP AF                  ; Restore the larger exponent.
  POP HL                  ; HL is to point to the result.
  LD (HL),A               ; Store the exponent of the result.
  PUSH HL                 ; Save the pointer again.
  LD L,B                  ; M4 to H and M5 to L (see FETCH_TWO).
  LD H,C                  ;
  ADD HL,DE               ; Add the two right bytes.
  EXX                     ; N2 to H' and N3 to L' (see FETCH_TWO).
  EX DE,HL                ;
  ADC HL,BC               ; Add left bytes with carry.
  EX DE,HL                ; Result back in D'E'.
  LD A,H                  ; Add H', L' and the carry; the resulting mechanisms will ensure that a single shift right is called if the sum of 2 positive numbers has overflowed left, or the sum of 2 negative numbers has not overflowed left.
  ADC A,L                 ;
  LD L,A                  ;
  RRA                     ;
  XOR L                   ;
  EXX                     ;
  EX DE,HL                ; The result is now in DED'E'.
  POP HL                  ; Get the pointer to the exponent.
  RRA                     ; The test for shift (H', L' were +00 for positive numbers and +FF for negative numbers).
  JR NC,TEST_NEG          ;
  LD A,$01                ; A counts a single shift right.
  CALL SHIFT_FP           ; The shift is called.
  INC (HL)                ; Add 1 to the exponent; this may lead to arithmetic overflow.
  JR Z,ADD_REP_6          ;
TEST_NEG:
  EXX                     ; Test for negative result: get sign bit of L' into A (this now correctly indicates the sign of the result).
  LD A,L                  ;
  AND $80                 ;
  EXX                     ;
  INC HL                  ; Store it in the second byte position of the result on the calculator stack.
  LD (HL),A               ;
  DEC HL                  ;
  JR Z,GO_NC_MLT          ; If it is zero, then do not two's complement the result.
  LD A,E                  ; Get the first byte.
  NEG                     ; Negate it.
  CCF                     ; Complement the carry for continued negation, and store byte.
  LD E,A                  ;
  LD A,D                  ; Get the next byte.
  CPL                     ; One's complement it.
  ADC A,$00               ; Add in the carry for negation.
  LD D,A                  ; Store the byte.
  EXX                     ; Proceed to get next byte into the A register.
  LD A,E                  ;
  CPL                     ; One's complement it.
  ADC A,$00               ; Add in the carry for negation.
  LD E,A                  ; Store the byte.
  LD A,D                  ; Get the last byte.
  CPL                     ; One's complement it.
  ADC A,$00               ; Add in the carry for negation.
  JR NC,END_COMPL         ; Done if no carry.
  RRA                     ; Else, get .5 into mantissa and add 1 to the exponent; this will be needed when two negative numbers add to give an exact power of 2, and it may lead to arithmetic overflow.
  EXX                     ;
  INC (HL)                ;
ADD_REP_6:
  JP Z,REPORT_6           ; Give the error if required.
  EXX                     ;
END_COMPL:
  LD D,A                  ; Store the last byte.
  EXX                     ;
GO_NC_MLT:
  XOR A                   ; Clear A and the carry flag.
  JP TEST_NORM            ; Exit via TEST_NORM.

; THE 'HL=HL*DE' SUBROUTINE
;
; This subroutine is called by GET_HLxDE and by multiply to perform the 16-bit multiplication as stated.
;
; Any overflow of the 16 bits available is dealt with on return from the subroutine.
;
;   DE First number (M)
;   HL Second number (N)
; O:HL M*N
HL_HLxDE:
  PUSH BC                 ; BC is saved.
  LD B,$10                ; It is to be a 16-bit multiplication.
  LD A,H                  ; A holds the high byte.
  LD C,L                  ; C holds the low byte.
  LD HL,$0000             ; Initialise the result to zero.
HL_LOOP:
  ADD HL,HL               ; Double the result.
  JR C,HL_END             ; Jump if overflow.
  RL C                    ; Rotate bit 7 of C into the carry.
  RLA                     ; Rotate the carry bit into bit 0 and bit 7 into the carry flag.
  JR NC,HL_AGAIN          ; Jump if the carry flag is reset.
  ADD HL,DE               ; Otherwise add DE in once.
  JR C,HL_END             ; Jump if overflow.
HL_AGAIN:
  DJNZ HL_LOOP            ; Repeat until 16 passes have been made.
HL_END:
  POP BC                  ; Restore BC.
  RET                     ; Finished.

; THE 'PREPARE TO MULTIPLY OR DIVIDE' SUBROUTINE
;
; Used by the routines at multiply and division.
;
; This subroutine prepares a floating-point number for multiplication or division, returning with carry set if the number is zero, getting the sign of the result into the A register, and replacing the sign bit in the number by the true numeric bit, 1.
;
;   A Bit 7 holds 0 on the first call, or the sign bit of the first number
;   HL Address of the first byte of the number
; O:A Bit 7 holds the sign bit of the first number, or the sign bit of the product/quotient
; O:F Carry flag set if the number is zero
PREP_M_D:
  CALL TEST_ZERO          ; If the number is zero, return with the carry flag set.
  RET C                   ;
  INC HL                  ; Point to the sign byte.
  XOR (HL)                ; Get sign for result into A (like signs give plus, unlike give minus); also reset the carry flag.
  SET 7,(HL)              ; Set the true numeric bit.
  DEC HL                  ; Point to the exponent again.
  RET                     ; Return with carry flag reset.

; THE 'MULTIPLICATION' OPERATION (offset +04)
;
; The address of this routine is found in the table of addresses. It is called via the calculator literal +04 by the routines at BEEP, CIRCLE, DRAW, CD_PRMS1, S_RND, DEC_TO_FP, INT_TO_FP, e_to_fp, LOG_2_A, series, n_mod_m, exp, ln, get_argt, sin, atn, asn and to_power. It is also called indirectly via fp_calc_2.
;
; This subroutine first tests whether the two numbers to be multiplied are 'small integers'. If they are, it uses INT_FETCH to get them from the stack, HL_HLxDE to multiply them and INT_STORE to return the result to the stack. Any overflow of this 'short multiplication' (i.e. if the result is not itself a 'small integer') causes a jump to multiplication in full five byte floating-point form (see below).
;
; DE Address of the first byte of the second number
; HL Address of the first byte of the first number
multiply:
  LD A,(DE)               ; Test whether the first bytes of both numbers are zero.
  OR (HL)                 ;
  JR NZ,MULT_LONG         ; If not, jump for 'long' multiplication.
  PUSH DE                 ; Save the pointers to the second number.
  PUSH HL                 ; And to the first number.
  PUSH DE                 ; And to the second number yet again.
  CALL INT_FETCH          ; Fetch sign in C, number in DE.
  EX DE,HL                ; Number to HL now.
  EX (SP),HL              ; Number to stack, second pointer to HL.
  LD B,C                  ; Save first sign in B.
  CALL INT_FETCH          ; Fetch second sign in C, number in DE.
  LD A,B                  ; Form sign of result in A: like signs give plus (+00), unlike give minus (+FF).
  XOR C                   ;
  LD C,A                  ; Store sign of result in C.
  POP HL                  ; Restore the first number to HL.
  CALL HL_HLxDE           ; Perform the actual multiplication.
  EX DE,HL                ; Store the result in DE.
  POP HL                  ; Restore the pointer to the first number.
  JR C,MULT_OFLW          ; Jump on overflow to 'full' multiplication.
  LD A,D                  ; These 5 bytes ensure that 00 FF 00 00 00 is replaced by zero; that they should not be needed if this number were excluded from the system is noted at ADDN_OFLW.
  OR E                    ;
  JR NZ,MULT_RSLT         ;
  LD C,A                  ;
MULT_RSLT:
  CALL INT_STORE          ; Now store the result on the stack.
  POP DE                  ; Restore STKEND to DE.
  RET                     ; Finished.
MULT_OFLW:
  POP DE                  ; Restore the pointer to the second number.
MULT_LONG:
  CALL RE_ST_TWO          ; Re-stack both numbers in full five byte floating-point form.
; The full multiplication subroutine prepares the first number for multiplication by calling PREP_M_D, returning if it is zero; otherwise the second number is prepared by again calling PREP_M_D, and if it is zero the subroutine goes to set the result to zero. Next it fetches the two numbers from the calculator stack and multiplies their mantissas in the usual way, rotating the first number (treated as the multiplier) right and adding in the second number (the multiplicand) to the result whenever
; the multiplier bit is set. The exponents are then added together and checks are made for overflow and for underflow (giving the result zero). Finally, the result is normalised and returned to the calculator stack with the correct sign bit in the second byte.
  XOR A                   ; A is set to zero so that the sign of the first number will go into A.
  CALL PREP_M_D           ; Prepare the first number, and return if zero. (Result already zero.)
  RET C                   ;
  EXX                     ; Exchange the registers.
  PUSH HL                 ; Save the next literal address.
  EXX                     ; Exchange the registers.
  PUSH DE                 ; Save the pointer to the multiplicand.
  EX DE,HL                ; Exchange the pointers.
  CALL PREP_M_D           ; Prepare the 2nd number.
  EX DE,HL                ; Exchange the pointers again.
  JR C,ZERO_RSLT          ; Jump forward if 2nd number is zero.
  PUSH HL                 ; Save the pointer to the result.
  CALL FETCH_TWO          ; Get the two numbers from the stack.
  LD A,B                  ; M5 to A (see FETCH_TWO).
  AND A                   ; Prepare for a subtraction.
  SBC HL,HL               ; Initialise HL to zero for the result.
  EXX                     ; Exchange the registers.
  PUSH HL                 ; Save M1 and N1 (see FETCH_TWO).
  SBC HL,HL               ; Also initialise HL' for the result.
  EXX                     ; Exchange the registers.
  LD B,$21                ; B counts thirty three shifts.
  JR STRT_MLT             ; Jump forward into the loop.
; Now enter the multiplier loop.
MLT_LOOP:
  JR NC,NO_ADD            ; Jump forward to NO_ADD if no carry, i.e. the multiplier bit was reset.
  ADD HL,DE               ; Else, add the multiplicand in D'E'DE (see FETCH_TWO) into the result being built up in H'L'HL.
  EXX                     ;
  ADC HL,DE               ;
  EXX                     ;
NO_ADD:
  EXX                     ; Whether multiplicand was added or not, shift result right in H'L'HL; the shift is done by rotating each byte with carry, so that any bit that drops into the carry is picked up by the next byte, and the shift continued into B'C'CA.
  RR H                    ;
  RR L                    ;
  EXX                     ;
  RR H                    ;
  RR L                    ;
STRT_MLT:
  EXX                     ; Shift right the multiplier in B'C'CA (see FETCH_TWO and above). A final bit dropping into the carry will trigger another add of the multiplicand to the result.
  RR B                    ;
  RR C                    ;
  EXX                     ;
  RR C                    ;
  RRA                     ;
  DJNZ MLT_LOOP           ; Loop 33 times to get all the bits.
  EX DE,HL                ; Move the result from H'L'HL to D'E'DE.
  EXX                     ;
  EX DE,HL                ;
  EXX                     ;
; Now add the exponents together.
  POP BC                  ; Restore the exponents - M1 and N1.
  POP HL                  ; Restore the pointer to the exponent byte.
  LD A,B                  ; Get the sum of the two exponent bytes in A, and the correct carry.
  ADD A,C                 ;
  JR NZ,MAKE_EXPT         ; If the sum equals zero then clear the carry; else leave it unchanged.
  AND A                   ;
MAKE_EXPT:
  DEC A                   ; Prepare to increase the exponent by +80.
  CCF                     ;
; This entry point is used by the routine at division.
DIVN_EXPT:
  RLA                     ; These few bytes very cleverly make the correct exponent byte. Rotating left then right gets the exponent byte (true exponent plus +80) into A.
  CCF                     ;
  RRA                     ;
  JP P,OFLW1_CLR          ; If the sign flag is reset, no report of arithmetic overflow needed.
  JR NC,REPORT_6          ; Report the overflow if carry reset.
  AND A                   ; Clear the carry now.
OFLW1_CLR:
  INC A                   ; The exponent byte is now complete; but if A is zero a further check for overflow is needed.
  JR NZ,OFLW2_CLR         ;
  JR C,OFLW2_CLR          ;
  EXX                     ; If there is no carry set and the result is already in normal form (bit 7 of D' set) then there is overflow to report; but if bit 7 of D' is reset, the result in just in range, i.e. just under 2**127.
  BIT 7,D                 ;
  EXX                     ;
  JR NZ,REPORT_6          ;
OFLW2_CLR:
  LD (HL),A               ; Store the exponent byte, at last.
  EXX                     ; Pass the fifth result byte to A for the normalisation sequence, i.e. the overflow from L into B'.
  LD A,B                  ;
  EXX                     ;
; This entry point is used by the routine at addition.
;
; The remainder of the subroutine deals with normalisation and is common to all the arithmetic routines.
TEST_NORM:
  JR NC,NORMALISE         ; If no carry then normalise now.
  LD A,(HL)               ; Else, deal with underflow (zero result) or near underflow (result 2**-128): return exponent to A, test if A is zero (case 2**-128) and if so produce 2**-128 if number is normal; otherwise produce zero. The exponent must then be set to zero (for zero) or 1 (for 2**-128).
  AND A                   ;
NEAR_ZERO:
  LD A,$80                ;
  JR Z,SKIP_ZERO          ;
ZERO_RSLT:
  XOR A                   ;
SKIP_ZERO:
  EXX                     ;
  AND D                   ;
  CALL ZEROS_4_5          ;
  RLCA                    ;
  LD (HL),A               ; Restore the exponent byte.
  JR C,OFLOW_CLR          ; Jump if case 2**-128.
  INC HL                  ; Otherwise, put zero into second byte of result on the calculator stack.
  LD (HL),A               ;
  DEC HL                  ;
  JR OFLOW_CLR            ; Jump forward to transfer the result.
; The actual normalisation operation.
NORMALISE:
  LD B,$20                ; Normalise the result by up to 32 shifts left of D'E'DE (with A adjoined) until bit 7 of D' is set. A holds zero after addition so no precision is gained or lost; A holds the fifth byte from B' after multiplication or division; but as only about 32 bits can be correct, no precision is lost. Note that A is rotated circularly, with branch at carry...eventually a random process.
SHIFT_ONE:
  EXX                     ;
  BIT 7,D                 ;
  EXX                     ;
  JR NZ,NORML_NOW         ;
  RLCA                    ;
  RL E                    ;
  RL D                    ;
  EXX                     ;
  RL E                    ;
  RL D                    ;
  EXX                     ;
  DEC (HL)                ; The exponent is decremented on each shift.
  JR Z,NEAR_ZERO          ; If the exponent becomes zero, then numbers from 2**-129 are rounded up to 2**-128.
  DJNZ SHIFT_ONE          ; Loop back, up to 32 times.
  JR ZERO_RSLT            ; If bit 7 never became 1 then the whole result is to be zero.
; Finish the normalisation by considering the 'carry'.
NORML_NOW:
  RLA                     ; After normalisation add back any final carry that went into A. Jump forward if the carry does not ripple right back.
  JR NC,OFLOW_CLR         ;
  CALL ADD_BACK           ;
  JR NZ,OFLOW_CLR         ;
  EXX                     ; If it should ripple right back then set mantissa to 0.5 and increment the exponent. This action may lead to arithmetic overflow (final case).
  LD D,$80                ;
  EXX                     ;
  INC (HL)                ;
  JR Z,REPORT_6           ;
; The final part of the subroutine involves passing the result to the bytes reserved for it on the calculator stack and resetting the pointers.
OFLOW_CLR:
  PUSH HL                 ; Save the result pointer.
  INC HL                  ; Point to the sign byte in the result.
  EXX                     ; The result is moved from D'E'DE to BCDE, and then to ACDE.
  PUSH DE                 ;
  EXX                     ;
  POP BC                  ;
  LD A,B                  ;
  RLA                     ; The sign bit is retrieved from its temporary store and transferred to its correct position of bit 7 of the first byte of the mantissa.
  RL (HL)                 ;
  RRA                     ;
  LD (HL),A               ; The first byte is stored.
  INC HL                  ; Next.
  LD (HL),C               ; The second byte is stored.
  INC HL                  ; Next.
  LD (HL),D               ; The third byte is stored.
  INC HL                  ; Next.
  LD (HL),E               ; The fourth byte is stored.
  POP HL                  ; Restore the pointer to the result.
  POP DE                  ; Restore the pointer to second number.
  EXX                     ; Exchange the register.
  POP HL                  ; Restore the next literal address.
  EXX                     ; Exchange the registers.
  RET                     ; Finished.
; This entry point is used by the routines at DEC_TO_FP, addition and division.
;
; Report 6 - Arithmetic overflow.
REPORT_6:
  RST $08                 ; Call the error handling routine.
  DEFB $05                ;

; THE 'DIVISION' OPERATION (offset +05)
;
; The address of this routine is found in the table of addresses. It is called via the calculator literal +05 by the routines at BEEP, DRAW, CD_PRMS1, DEC_TO_FP, e_to_fp, n_mod_m, tan, atn, asn and to_power. It is also called indirectly via fp_calc_2.
;
; This subroutine first prepares the divisor by calling PREP_M_D, reporting arithmetic overflow if it is zero; then it prepares the dividend again calling PREP_M_D, returning if it is zero. Next it fetches the two numbers from the calculator stack and divides their mantissa by means of the usual restoring division, trial subtracting the divisor from the dividend and restoring if there is carry, otherwise adding 1 to the quotient. The maximum precision is obtained for a 4-byte division, and after
; subtracting the exponents the subroutine exits by joining the later part of multiply.
;
; DE Address of the first byte of the second number (divisor)
; HL Address of the first byte of the first number (dividend)
division:
  CALL RE_ST_TWO          ; Use full floating-point forms.
  EX DE,HL                ; Exchange the pointers.
  XOR A                   ; A is set to 0, so that the sign of the first number will go into A.
  CALL PREP_M_D           ; Prepare the divisor and give the report for arithmetic overflow if it is zero.
  JR C,REPORT_6           ;
  EX DE,HL                ; Exchange the pointers.
  CALL PREP_M_D           ; Prepare the dividend and return if it is zero (result already zero).
  RET C                   ;
  EXX                     ; Exchange the pointers.
  PUSH HL                 ; Save the next literal address.
  EXX                     ; Exchange the registers.
  PUSH DE                 ; Save pointer to divisor.
  PUSH HL                 ; Save pointer to dividend.
  CALL FETCH_TWO          ; Get the two numbers from the stack.
  EXX                     ; Exchange the registers.
  PUSH HL                 ; Save M1 and N1 (the exponent bytes) on the machine stack.
  LD H,B                  ; Copy the four bytes of the dividend from registers B'C'CB (i.e. M2, M3, M4 and M5; see FETCH_TWO) to the registers H'L'HL.
  LD L,C                  ;
  EXX                     ;
  LD H,C                  ;
  LD L,B                  ;
  XOR A                   ; Clear A and reset the carry flag.
  LD B,$DF                ; B will count upwards from -33 to -1 (+DF to +FF), looping on minus and will jump again on zero for extra precision.
  JR DIV_START            ; Jump forward into the division loop for the first trial subtraction.
; Now enter the division loop.
DIV_LOOP:
  RLA                     ; Shift the result left into B'C'CA, shifting out the bits already there, picking up 1 from the carry whenever it is set, and rotating left each byte with carry to achieve the 32-bit shift.
  RL C                    ;
  EXX                     ;
  RL C                    ;
  RL B                    ;
  EXX                     ;
DIV_34TH:
  ADD HL,HL               ; Move what remains of the dividend left in H'L'HL before the next trial subtraction; if a bit drops into the carry, force no restore and a bit for the quotient, thus retrieving the lost bit and allowing a full 32-bit divisor.
  EXX                     ;
  ADC HL,HL               ;
  EXX                     ;
  JR C,SUBN_ONLY          ;
DIV_START:
  SBC HL,DE               ; Trial subtract divisor in D'E'DE from rest of dividend in H'L'HL; there is no initial carry (see previous step).
  EXX                     ;
  SBC HL,DE               ;
  EXX                     ;
  JR NC,NO_RSTORE         ; Jump forward if there is no carry.
  ADD HL,DE               ; Otherwise restore, i.e. add back the divisor. Then clear the carry so that there will be no bit for the quotient (the divisor 'did not go').
  EXX                     ;
  ADC HL,DE               ;
  EXX                     ;
  AND A                   ;
  JR COUNT_ONE            ; Jump forward to the counter.
SUBN_ONLY:
  AND A                   ; Just subtract with no restore and go on to set the carry flag because the lost bit of the dividend is to be retrieved and used for the quotient.
  SBC HL,DE               ;
  EXX                     ;
  SBC HL,DE               ;
  EXX                     ;
NO_RSTORE:
  SCF                     ; One for the quotient in B'C'CA.
COUNT_ONE:
  INC B                   ; Step the loop count up by one.
  JP M,DIV_LOOP           ; Loop 32 times for all bits.
  PUSH AF                 ; Save any 33rd bit for extra precision (the present carry).
  JR Z,DIV_START          ; Trial subtract yet again for any 34th bit; the 'PUSH AF' above saves this bit too.
; Note: this jump is made to the wrong place. No 34th bit will ever be obtained without first shifting the dividend. Hence important results like 1/10 and 1/1000 are not rounded up as they should be. Rounding up never occurs when it depends on the 34th bit. The jump should have been to DIV_34TH above, i.e. byte +3200 in the ROM should read +DA instead of +E1.
  LD E,A                  ; Now move the four bytes that form the mantissa of the result from B'C'CA to D'E'DE.
  LD D,C                  ;
  EXX                     ;
  LD E,C                  ;
  LD D,B                  ;
  POP AF                  ; Then put the 34th and 33rd bits into B' to be picked up on normalisation.
  RR B                    ;
  POP AF                  ;
  RR B                    ;
  EXX                     ;
  POP BC                  ; Restore the exponent bytes M1 and N1.
  POP HL                  ; Restore the pointer to the result.
  LD A,B                  ; Get the difference between the two exponent bytes into A and set the carry flag if required.
  SUB C                   ;
  JP DIVN_EXPT            ; Exit via DIVN_EXPT.

; THE 'INTEGER TRUNCATION TOWARDS ZERO' SUBROUTINE (offset +3A)
;
; The address of this routine is found in the table of addresses. It is called via the calculator literal +3A by the routine at int.
;
; This subroutine (say I(x)) returns the result of integer truncation of x, the 'last value', towards zero. Thus I(2.4) is 2 and I(-2.4) is -2. The subroutine returns at once if x is in the form of a 'short integer'. It returns zero if the exponent byte of x is less than +81 (ABS x is less than 1). If I(x) is a 'short integer' the subroutine returns it in that form. It returns x if the exponent byte is +A0 or greater (x has no significant non-integral part). Otherwise the correct number of bytes
; of x are set to zero and, if needed, one more byte is split with a mask.
;
; HL Address of the first byte of the number
truncate:
  LD A,(HL)               ; Get the exponent byte of x into A.
  AND A                   ; If A is zero, return since x is already a small integer.
  RET Z                   ;
  CP $81                  ; Compare e, the exponent, to +81.
  JR NC,T_GR_ZERO         ; Jump if e is greater than +80.
  LD (HL),$00             ; Else, set the exponent to zero; enter +20 into A and jump forward to NIL_BYTES to make all the bits of x be zero.
  LD A,$20                ;
  JR NIL_BYTES            ;
T_GR_ZERO:
  CP $91                  ; Compare e to +91.
  JR NZ,T_SMALL           ; Jump if e not +91.
; The next 26 bytes seem designed to test whether x is in fact -65536, i.e. 91 80 00 00 00, and if it is, to set it to 00 FF 00 00 00. This is a mistake. As already stated, the Spectrum system cannot handle this number. The result here is simply to make INT (-65536) return the value -1. This is a pity, since the number would have been perfectly all right if left alone. The remedy would seem to be simply to omit the 28 bytes from 3223 above to 323E inclusive from the program.
  INC HL                  ; HL is pointed at the fourth byte of x, where the 17 bits of the integer part of x end after the first bit.
  INC HL                  ;
  INC HL                  ;
  LD A,$80                ; The first bit is obtained in A, using +80 as a mask.
  AND (HL)                ;
  DEC HL                  ; That bit and the previous 8 bits are tested together for zero.
  OR (HL)                 ;
  DEC HL                  ; HL is pointed at the second byte of x.
  JR NZ,T_FIRST           ; If already non-zero, the test can end.
  LD A,$80                ; Otherwise, the test for -65536 is now completed: 91 80 00 00 00 will leave the zero flag set now.
  XOR (HL)                ;
T_FIRST:
  DEC HL                  ; HL is pointed at the first byte of x.
  JR NZ,T_EXPNENT         ; If zero reset, the jump is made.
  LD (HL),A               ; The first byte is set to zero.
  INC HL                  ; HL points to the second byte.
  LD (HL),$FF             ; The second byte is set to +FF.
  DEC HL                  ; HL again points to the first byte.
  LD A,$18                ; The last 24 bits are to be zero.
  JR NIL_BYTES            ; The jump to NIL_BYTES completes the number 00 FF 00 00 00.
; If the exponent byte of x is between +81 and +90 inclusive, I(x) is a 'small integer', and will be compressed into one or two bytes. But first a test is made to see whether x is, after all, large.
T_SMALL:
  JR NC,X_LARGE           ; Jump with exponent byte +92 or more (it would be better to jump with +91 too).
  PUSH DE                 ; Save STKEND in DE.
  CPL                     ; Range 129<=A<=144 becomes 126>=A>=111.
  ADD A,$91               ; Range is now 15>=A>=0.
  INC HL                  ; Point HL at second byte.
  LD D,(HL)               ; Second byte to D.
  INC HL                  ; Point HL at third byte.
  LD E,(HL)               ; Third byte to E.
  DEC HL                  ; Point HL at first byte again.
  DEC HL                  ;
  LD C,$00                ; Assume a positive number.
  BIT 7,D                 ; Now test for negative (bit 7 set).
  JR Z,T_NUMERIC          ; Jump if positive after all.
  DEC C                   ; Change the sign.
T_NUMERIC:
  SET 7,D                 ; Insert true numeric bit, 1, in D.
  LD B,$08                ; Now test whether A>=8 (one byte only) or two bytes needed.
  SUB B                   ;
  ADD A,B                 ; Leave A unchanged.
  JR C,T_TEST             ; Jump if two bytes needed.
  LD E,D                  ; Put the one byte into E.
  LD D,$00                ; And set D to zero.
  SUB B                   ; Now 1<=A<=7 to count the shifts needed.
T_TEST:
  JR Z,T_STORE            ; Jump if no shift needed.
  LD B,A                  ; B will count the shifts.
T_SHIFT:
  SRL D                   ; Shift D and E right B times to produce the correct number.
  RR E                    ;
  DJNZ T_SHIFT            ; Loop until B is zero.
T_STORE:
  CALL INT_STORE          ; Store the result on the stack.
  POP DE                  ; Restore STKEND to DE.
  RET                     ; Finished.
; Large values of x remain to be considered.
T_EXPNENT:
  LD A,(HL)               ; Get the exponent byte of x into A.
X_LARGE:
  SUB $A0                 ; Subtract +A0 from e.
  RET P                   ; Return on plus - x has no significant non-integral part. (If the true exponent were reduced to zero, the 'binary point' would come at or after the end of the four bytes of the mantissa.)
  NEG                     ; Else, negate the remainder; this gives the number of bits to become zero (the number of bits after the 'binary point').
; Now the bits of the mantissa can be cleared.
NIL_BYTES:
  PUSH DE                 ; Save the current value of DE (STKEND).
  EX DE,HL                ; Make HL point one past the fifth byte.
  DEC HL                  ; HL now points to the fifth byte of x.
  LD B,A                  ; Get the number of bits to be set to zero in B and divide it by 8 to give the number of whole bytes implied.
  SRL B                   ;
  SRL B                   ;
  SRL B                   ;
  JR Z,BITS_ZERO          ; Jump forward if the result is zero.
BYTE_ZERO:
  LD (HL),$00             ; Else, set the bytes to zero; B counts them.
  DEC HL                  ;
  DJNZ BYTE_ZERO          ;
BITS_ZERO:
  AND $07                 ; Get A (mod 8); this is the number of bits still to be set to zero.
  JR Z,IX_END             ; Jump to the end if nothing more to do.
  LD B,A                  ; B will count the bits now.
  LD A,$FF                ; Prepare the mask.
LESS_MASK:
  SLA A                   ; With each loop a zero enters the mask from the right and thereby a mask of the correct length is produced.
  DJNZ LESS_MASK          ;
  AND (HL)                ; The unwanted bits of (HL) are lost as the masking is performed.
  LD (HL),A               ;
IX_END:
  EX DE,HL                ; Return the pointer to HL.
  POP DE                  ; Return STKEND to DE.
  RET                     ; Finished.

; THE 'RE-STACK TWO' SUBROUTINE
;
; Used by the routines at addition, multiply and division.
;
; This subroutine is called to re-stack two 'small integers' in full five-byte floating-point form for the binary operations of addition, multiplication and division. It does so by calling the following subroutine twice.
;
; DE Address of the first byte of the second number
; HL Address of the first byte of the first number
RE_ST_TWO:
  CALL RESTK_SUB          ; Call the subroutine and then continue into it for the second call.
RESTK_SUB:
  EX DE,HL                ; Exchange the pointers at each call.
; This routine continues into re_stack.

; THE 'RE-STACK' SUBROUTINE (offset +3D)
;
; Used by the routine at atn.
;
; The routine at RE_ST_TWO continues here.
;
; The address of this routine is found in the table of addresses. It is called via the calculator literal +3D by the routines at CIRCLE, DRAW, exp, ln and get_argt. The routine at RE_ST_TWO also continues here.
;
; This subroutine is called to re-stack one number (which could be a 'small integer') in full five-byte floating-point form.
;
; HL Address of the first byte of the number
re_stack:
  LD A,(HL)               ; If the first byte is not zero, return - the number cannot be a 'small integer'.
  AND A                   ;
  RET NZ                  ;
  PUSH DE                 ; Save the 'other' pointer in DE.
  CALL INT_FETCH          ; Fetch the sign in C and the number in DE.
  XOR A                   ; Clear the A register.
  INC HL                  ; Point to the fifth location.
  LD (HL),A               ; Set the fifth byte to zero.
  DEC HL                  ; Point to the fourth location.
  LD (HL),A               ; Set the fourth byte to zero; bytes 2 and 3 will hold the mantissa.
  LD B,$91                ; Set B to +91 for the exponent, i.e. for up to 16 bits in the integer.
  LD A,D                  ; Test whether D is zero so that at most 8 bits would be needed.
  AND A                   ;
  JR NZ,RS_NRMLSE         ; Jump if more than 8 bits needed.
  OR E                    ; Now test E too.
  LD B,D                  ; Save the zero in B (it will give zero exponent if E too is zero).
  JR Z,RS_STORE           ; Jump if E is indeed zero.
  LD D,E                  ; Move E to D (D was zero, E not).
  LD E,B                  ; Set E to zero now.
  LD B,$89                ; Set B to +89 for the exponent - no more than 8 bits now.
RS_NRMLSE:
  EX DE,HL                ; Pointer to DE, number to HL.
RSTK_LOOP:
  DEC B                   ; Decrement the exponent on each shift.
  ADD HL,HL               ; Shift the number right one position.
  JR NC,RSTK_LOOP         ; Until the carry is set.
  RRC C                   ; Sign bit to carry flag now.
  RR H                    ; Insert it in place as the number is shifted back one place normal now.
  RR L                    ;
  EX DE,HL                ; Pointer to byte 4 back to HL.
RS_STORE:
  DEC HL                  ; Point to the third location.
  LD (HL),E               ; Store the third byte.
  DEC HL                  ; Point to the second location.
  LD (HL),D               ; Store the second byte.
  DEC HL                  ; Point to the first location.
  LD (HL),B               ; Store the exponent byte.
  POP DE                  ; Restore the 'other' pointer to DE.
  RET                     ; Finished.

; THE TABLE OF CONSTANTS
;
; Used by the routine at stk_con.
;
; This table holds five useful and frequently needed numbers: zero, one, a half, a half of pi, and ten. The numbers are held in a condensed form which is expanded by the routine at stk_data to give the required floating-point form.
CONSTANTS:
  DEFB $00,$B0,$00        ; zero (00 00 00 00 00)
stk_one:
  DEFB $40,$B0,$00,$01    ; one (00 00 01 00 00)
stk_half:
  DEFB $30,$00            ; a half (80 00 00 00 00)
stk_pi_2:
  DEFB $F1,$49,$0F,$DA,$A2 ; a half of pi (81 49 0F DA A2)
stk_ten:
  DEFB $40,$B0,$00,$0A    ; ten (00 00 0A 00 00)

; THE TABLE OF ADDRESSES
;
; Used by the routine at CALCULATE.
;
; This table is a look-up table of the addresses of the sixty-six operational subroutines of the calculator. The offsets used to index into the table are derived either from the operation codes used in the routine at SCANNING (see S_LOOP, etc.) or from the literals that follow a 'RST $28' instruction.
CALCADDR:
  DEFW jump_true          ; +00
  DEFW exchange           ; +01
  DEFW delete             ; +02
  DEFW subtract           ; +03
  DEFW multiply           ; +04
  DEFW division           ; +05
  DEFW to_power           ; +06
  DEFW no_or_no           ; +07
  DEFW no_and_no          ; +08
  DEFW compare            ; +09: <= (numbers)
  DEFW compare            ; +0A: >= (numbers)
  DEFW compare            ; +0B: <> (numbers)
  DEFW compare            ; +0C: > (numbers)
  DEFW compare            ; +0D: < (numbers)
  DEFW compare            ; +0E: = (numbers)
  DEFW addition           ; +0F
  DEFW str_no             ; +10
  DEFW compare            ; +11: <= (strings)
  DEFW compare            ; +12: >= (strings)
  DEFW compare            ; +13: <> (strings)
  DEFW compare            ; +14: > (strings)
  DEFW compare            ; +15: < (strings)
  DEFW compare            ; +16: = (strings)
  DEFW strs_add           ; +17
  DEFW val                ; +18 (VAL$)
  DEFW usr                ; +19
  DEFW read_in            ; +1A
  DEFW negate             ; +1B
  DEFW code               ; +1C
  DEFW val                ; +1D (VAL)
  DEFW len                ; +1E
  DEFW sin                ; +1F
  DEFW cos                ; +20
  DEFW tan                ; +21
  DEFW asn                ; +22
  DEFW acs                ; +23
  DEFW atn                ; +24
  DEFW ln                 ; +25
  DEFW exp                ; +26
  DEFW int                ; +27
  DEFW sqr                ; +28
  DEFW sgn                ; +29
  DEFW abs                ; +2A
  DEFW peek               ; +2B
  DEFW f_in               ; +2C
  DEFW usr_no             ; +2D
  DEFW str                ; +2E
  DEFW chrs               ; +2F
  DEFW f_not              ; +30
  DEFW duplicate          ; +31
  DEFW n_mod_m            ; +32
  DEFW jump               ; +33
  DEFW stk_data           ; +34
  DEFW dec_jr_nz          ; +35
  DEFW less_0             ; +36
  DEFW greater_0          ; +37
  DEFW end_calc           ; +38
  DEFW get_argt           ; +39
  DEFW truncate           ; +3A
  DEFW fp_calc_2          ; +3B
  DEFW e_to_fp            ; +3C
  DEFW re_stack           ; +3D
  DEFW series             ; +3E
  DEFW stk_con            ; +3F
  DEFW st_mem             ; +40
  DEFW get_mem            ; +41
; Note: the last four subroutines are multi-purpose subroutines and are entered with a parameter that is a copy of the right hand five bits of the original literal. The full set follows:
;
; * Offset +3E: series-06, series-08 and series-0C; literals +86, +88 and +8C.
; * Offset +3F: stk-zero, stk-one, stk-half, stk-pi/2 and stk-ten; literals +A0 to +A4.
; * Offset +40: st-mem-0, st-mem-1, st-mem-2, st-mem-3, st-mem-4 and st-mem-5; literals +C0 to +C5.
; * Offset +41: get-mem-0, get-mem-1, get-mem-2, get-mem-3, get-mem-4 and get-mem-5; literals +E0 to +E5.

; THE 'CALCULATE' SUBROUTINE
;
; Used by the routine at FP_CALC.
;
; This subroutine is used to perform floating-point calculations. These can be considered to be of three types:
;
; * Binary operations, e.g. addition, where two numbers in floating-point form are added together to give one 'last value'.
; * Unary operations, e.g. sin, where the 'last value' is changed to give the appropriate function result as a new 'last value'.
; * Manipulatory operations, e.g. st_mem, where the 'last value' is copied to the first five bytes of the calculator's memory area.
;
; The operations to be performed are specified as a series of data-bytes, the literals, that follow an RST $28 instruction that calls this subroutine. The last literal in the list is always '+38' which leads to an end to the whole operation.
;
; In the case of a single operation needing to be performed, the operation offset can be passed to the calculator in the B register, and operation '+3B', the single calculation operation, performed.
;
; It is also possible to call this subroutine recursively, i.e. from within itself, and in such a case it is possible to use the system variable BREG as a counter that controls how many operations are performed before returning.
;
; The first part of this subroutine is complicated but essentially it performs the two tasks of setting the registers to hold their required values, and to produce an offset, and possibly a parameter, from the literal that is currently being considered.
;
; The offset is used to index into the calculator's table of addresses to find the required subroutine address.
;
; The parameter is used when the multi-purpose subroutines are called.
;
; Note: a floating-point number may in reality be a set of string parameters.
;
; B Operation offset or counter
CALCULATE:
  CALL STK_PNTRS          ; Presume a unary operation and therefore set HL to point to the start of the 'last value' on the calculator stack and DE one past this floating-point number (STKEND).
; This entry point is used by the routine at series.
GEN_ENT_1:
  LD A,B                  ; Either transfer a single operation offset to BREG temporarily, or, when using the subroutine recursively, pass the parameter to BREG to be used as a counter.
  LD (BREG),A             ;
; This entry point is used by the routine at series.
GEN_ENT_2:
  EXX                     ; The return address of the subroutine is stored in HL'. This saves the pointer to the first literal. Entering the calculator here is done whenever BREG is in use as a counter and is not to be disturbed.
  EX (SP),HL              ;
  EXX                     ;
RE_ENTRY:
  LD (STKEND),DE          ; A loop is now entered to handle each literal in the list that follows the calling instruction; so first, always set STKEND.
  EXX                     ; Go to the alternate register set and fetch the literal for this loop.
  LD A,(HL)               ;
  INC HL                  ; Make HL' point to the next literal.
; This entry point is used by the routine at fp_calc_2.
SCAN_ENT:
  PUSH HL                 ; This pointer is saved briefly on the machine stack. SCAN_ENT is used by fp_calc_2 to find the subroutine that is required.
  AND A                   ; Test the A register.
  JP P,FIRST_3D           ; Separate the simple literals from the multi-purpose literals. Jump with literals +00 to +3D.
  LD D,A                  ; Save the literal in D.
  AND $60                 ; Continue only with bits 5 and 6.
  RRCA                    ; Four right shifts make them now bits 1 and 2.
  RRCA                    ;
  RRCA                    ;
  RRCA                    ;
  ADD A,$7C               ; The offsets required are +3E to +41, and L will now hold double the required offset.
  LD L,A                  ;
  LD A,D                  ; Now produce the parameter by taking bits 0, 1, 2, 3 and 4 of the literal; keep the parameter in A.
  AND $1F                 ;
  JR ENT_TABLE            ; Jump forward to find the address of the required subroutine.
FIRST_3D:
  CP $18                  ; Jump forward if performing a unary operation.
  JR NC,DOUBLE_A          ;
  EXX                     ; All of the subroutines that perform binary operations require that HL points to the first operand and DE points to the second operand (the 'last value') as they appear on the calculator stack.
  LD BC,$FFFB             ;
  LD D,H                  ;
  LD E,L                  ;
  ADD HL,BC               ;
  EXX                     ;
DOUBLE_A:
  RLCA                    ; As each entry in the table of addresses takes up two bytes the offset produced is doubled.
  LD L,A                  ;
ENT_TABLE:
  LD DE,CALCADDR          ; The base address of the table.
  LD H,$00                ; The address of the required table entry is formed in HL, and the required subroutine address is loaded into the DE register pair.
  ADD HL,DE               ;
  LD E,(HL)               ;
  INC HL                  ;
  LD D,(HL)               ;
  LD HL,RE_ENTRY          ; The address of RE_ENTRY is put on the machine stack underneath the subroutine address.
  EX (SP),HL              ;
  PUSH DE                 ;
  EXX                     ; Return to the main set of registers.
  LD BC,($5C66)           ; The current value of BREG is transferred to the B register thereby returning the single operation offset (see compare).
; The address of this entry point is found in the table of addresses. It is called via the calculator literal +02 by the routines at BEEP, IF_CMD, FOR, NEXT, CIRCLE, DRAW, CD_PRMS1, S_RND, LET, DEC_TO_FP, e_to_fp, FP_TO_BC, PRINT_FP, series, n_mod_m, exp, ln, get_argt and to_power.
delete:
  RET                     ; An indirect jump to the required subroutine.
; The delete subroutine contains only the single 'RET' instruction above. The literal '+02' results in this subroutine being considered as a binary operation that is to be entered with a first number addressed by the HL register pair and a second number addressed by the DE register pair, and the result produced again addressed by the HL register pair.
;
; The single 'RET' instruction thereby leads to the first number being considered as the resulting 'last value' and the second number considered as being deleted. Of course the number has not been deleted from the memory but remains inactive and will probably soon be overwritten.

; THE 'SINGLE OPERATION' SUBROUTINE (offset +3B)
;
; The address of this routine is found in the table of addresses. It is called via the calculator literal +3B by the routine at S_LETTER.
;
; This subroutine is only called from 2757 and is used to perform a single arithmetic operation. The offset that specifies which operation is to be performed is supplied to the calculator in the B register and subsequently transferred to the system variable BREG.
;
; The effect of calling this subroutine is essentially to make a jump to the appropriate subroutine for the single operation.
fp_calc_2:
  POP AF                  ; Discard the RE_ENTRY address.
  LD A,(BREG)             ; Transfer the offset from BREG to A.
  EXX                     ; Enter the alternate register set.
  JR SCAN_ENT             ; Jump back to find the required address; stack the RE_ENTRY address and jump to the subroutine for the operation.

; THE 'TEST 5-SPACES' SUBROUTINE
;
; Used by the routines at STK_ST_0, duplicate and stk_data.
;
; This subroutine tests whether there is sufficient room in memory for another 5-byte floating-point number to be added to the calculator stack.
;
; O:BC +0005
TEST_5_SP:
  PUSH DE                 ; Save DE briefly.
  PUSH HL                 ; Save HL briefly.
  LD BC,$0005             ; Specify the test is for 5 bytes.
  CALL TEST_ROOM          ; Make the test.
  POP HL                  ; Restore HL.
  POP DE                  ; Restore DE.
  RET                     ; Finished.

; THE 'STACK NUMBER' SUBROUTINE
;
; Used by the routines at BEEP, S_DECIMAL and S_LETTER.
;
; This subroutine is called by BEEP, S_U_PLUS and S_LETTER to copy STKEND to DE, move a floating-point number to the calculator stack, and reset STKEND from DE. It calls duplicate to do the actual move.
;
; HL Address of the first byte of the number to stack
STACK_NUM:
  LD DE,(STKEND)          ; Copy STKEND to DE as destination address.
  CALL duplicate          ; Move the number.
  LD (STKEND),DE          ; Reset STKEND from DE.
  RET                     ; Finished.

; THE 'MOVE A FLOATING-POINT NUMBER' SUBROUTINE (offset +31)
;
; Used by the routines at STK_F_ARG, STACK_NUM, get_mem and st_mem.
;
; The address of this routine is found in the table of addresses. It is called via the calculator literal +31 by the routines at BEEP, CIRCLE, DRAW, CD_PRMS1, S_RND, e_to_fp, PRINT_FP, series, n_mod_m, int, exp, ln, get_argt, sin, tan, atn, asn, sqr and to_power.
;
; This subroutine moves a floating-point number to the top of the calculator stack (3 cases) or from the top of the stack to the calculator's memory area (1 case). It is also called through the calculator when it simply duplicates the number at the top of the calculator stack, the 'last value', thereby extending the stack by five bytes.
;
; DE Destination address
; HL Address of the first byte of the number to move
duplicate:
  CALL TEST_5_SP          ; A test is made for room.
  LDIR                    ; Move the five bytes involved.
  RET                     ; Finished.

; THE 'STACK LITERALS' SUBROUTINE (offset +34)
;
; Used by the routine at series.
;
; The address of this routine is found in the table of addresses. It is called via the calculator literal +34 by the routines at BEEP, CD_PRMS1, S_RND, LOG_2_A, exp, ln and get_argt.
;
; This subroutine places on the calculator stack, as a 'last value', the floating-point number supplied to it as 2, 3, 4 or 5 literals.
;
; When called by using offset '+34' the literals follow the '+34' in the list of literals; when called by the series generator, the literals are supplied by the subroutine that called for a series to be generated; and when called by SKIP_CONS and stk_con the literals are obtained from the calculator's table of constants.
;
; In each case, the first literal supplied is divided by +40, and the integer quotient plus 1 determines whether 1, 2, 3 or 4 further literals will be taken from the source to form the mantissa of the number. Any unfilled bytes of the five bytes that go to form a 5-byte floating-point number are set to zero. The first literal is also used to determine the exponent, after reducing mod +40, unless the remainder is zero, in which case the second literal is used, as it stands, without reducing mod
; +40. In either case, +50 is added to the literal, giving the augmented exponent byte, e (the true exponent e' plus +80). The rest of the 5 bytes are stacked, including any zeros needed, and the subroutine returns.
;
; DE STKEND
; HL' Address of the next literal
stk_data:
  LD H,D                  ; This subroutine performs the manipulatory operation of adding a 'last value' to the calculator stack; hence HL is set to point one past the present 'last value' and hence point to the result.
  LD L,E                  ;
; This entry point is used by the routines at SKIP_CONS and stk_con.
STK_CONST:
  CALL TEST_5_SP          ; Now test that there is indeed room.
  EXX                     ; Go to the alternate register set and stack the pointer to the next literal.
  PUSH HL                 ;
  EXX                     ;
  EX (SP),HL              ; Switch over the result pointer and the next literal pointer.
  PUSH BC                 ; Save BC briefly.
  LD A,(HL)               ; The first literal is put into A and divided by +40 to give the integer values 0, 1, 2 or 3.
  AND $C0                 ;
  RLCA                    ;
  RLCA                    ;
  LD C,A                  ; The integer value is transferred to C and incremented, thereby giving the range 1, 2, 3 or 4 for the number of literals that will be needed.
  INC C                   ;
  LD A,(HL)               ; The literal is fetched anew, reduced mod +40 and discarded as inappropriate if the remainder if zero; in which case the next literal is fetched and used unreduced.
  AND $3F                 ;
  JR NZ,FORM_EXP          ;
  INC HL                  ;
  LD A,(HL)               ;
FORM_EXP:
  ADD A,$50               ; The exponent, e, is formed by the addition of +50 and passed to the calculator stack as the first of the five bytes of the result.
  LD (DE),A               ;
  LD A,$05                ; The number of literals specified in C are taken from the source and entered into the bytes of the result.
  SUB C                   ;
  INC HL                  ;
  INC DE                  ;
  LD B,$00                ;
  LDIR                    ;
  POP BC                  ; Restore BC.
  EX (SP),HL              ; Return the result pointer to HL and the next literal pointer to its usual position in HL'.
  EXX                     ;
  POP HL                  ;
  EXX                     ;
  LD B,A                  ; The number of zero bytes required at this stage is given by 5-C-1, and this number of zeros is added to the result to make up the required five bytes.
  XOR A                   ;
STK_ZEROS:
  DEC B                   ;
  RET Z                   ;
  LD (DE),A               ;
  INC DE                  ;
  JR STK_ZEROS            ;

; THE 'SKIP CONSTANTS' SUBROUTINE
;
; Used by the routine at stk_con.
;
; This subroutine is entered with the HL' register pair holding the base address of the calculator's table of constants and the A register holding a parameter that shows which of the five constants is being requested.
;
; The subroutine performs the null operations of loading the five bytes of each unwanted constant into the locations 0000, 0001, 0002, 0003 and 0004 at the beginning of the ROM until the requested constant is reached.
;
; The subroutine returns with the HL' register pair holding the base address of the requested constant within the table of constants.
;
;   A Index of the required constant (+00 to +04)
;   HL' CONSTANTS
; O:HL' Address of the required constant
SKIP_CONS:
  AND A                   ; The subroutine returns if the parameter is zero, or when the requested constant has been reached.
SKIP_NEXT:
  RET Z                   ;
  PUSH AF                 ; Save the parameter.
  PUSH DE                 ; Save the result pointer.
  LD DE,$0000             ; The dummy address.
  CALL STK_CONST          ; Perform imaginary stacking of an expanded constant.
  POP DE                  ; Restore the result pointer.
  POP AF                  ; Restore the parameter.
  DEC A                   ; Count the loops.
  JR SKIP_NEXT            ; Jump back to consider the value of the counter.

; THE 'MEMORY LOCATION' SUBROUTINE
;
; Used by the routines at BEEP, get_mem and st_mem.
;
; This subroutine finds the base address for each five-byte portion of the calculator's memory area to or from which a floating-point number is to be moved from or to the calculator stack. It does this operation by adding five times the parameter supplied to the base address for the area which is held in the HL register pair.
;
; Note that when a FOR-NEXT variable is being handled then the pointers are changed so that the variable is treated as if it were the calculator's memory area.
;
;   A Index of the required entry
;   HL Base address (SEMITONES or MEM)
; O:HL Base address + 5 * A
LOC_MEM:
  LD C,A                  ; Copy the parameter to C.
  RLCA                    ; Double the parameter.
  RLCA                    ; Double the result.
  ADD A,C                 ; Add the value of the parameter to give five times the original value.
  LD C,A                  ; This result is wanted in the BC register pair.
  LD B,$00                ;
  ADD HL,BC               ; Produce the new base address.
  RET                     ; Finished.

; THE 'GET FROM MEMORY AREA' SUBROUTINE (offset +41)
;
; The address of this routine is found in the table of addresses. It is called via a calculator literal (+E0 to +E5) by the routines at BEEP, FOR, NEXT, NEXT_LOOP, CIRCLE, DRAW, CD_PRMS1, DEC_TO_FP, e_to_fp, PRINT_FP, series, n_mod_m, int, exp and cos.
;
; This subroutine is called using the literals +E0 to +E5 and the parameter derived from these literals is held in the A register. The subroutine calls LOC_MEM to put the required source address into the HL register pair and duplicate to copy the five bytes involved from the calculator's memory area to the top of the calculator stack to form a new 'last value'.
;
;   A Index of the required memory area (+00 to +05)
;   DE Destination address
; O:HL Destination address (as DE on entry)
get_mem:
  PUSH DE                 ; Save the result pointer.
  LD HL,(MEM)             ; Fetch the pointer to the current memory area (MEM).
  CALL LOC_MEM            ; The base address is found.
  CALL duplicate          ; The five bytes are moved.
  POP HL                  ; Set the result pointer.
  RET                     ; Finished.

; THE 'STACK A CONSTANT' SUBROUTINE (offset +3F)
;
; The address of this routine is found in the table of addresses. It is called via a calculator literal (+A0 to +A4) by the routines at BEEP, FETCH_NUM, FOR, CIRCLE, DRAW, CD_PRMS1, S_RND, S_PI, DEC_TO_FP, INT_TO_FP, e_to_fp, FP_TO_BC, PRINT_FP, series, compare, int, exp, ln, get_argt, cos, sin, atn, asn, acs, sqr and to_power.
;
; This subroutine uses SKIP_CONS to find the base address of the requested constants from the calculator's table of constants and then calls STK_CONST to make the expanded form of the constant the 'last value' on the calculator stack.
;
; A Index of the required constant (+00 to +04)
; DE STKEND
stk_con:
  LD H,D                  ; Set HL to hold the result pointer.
  LD L,E                  ;
  EXX                     ; Go to the alternate register set and save the next literal pointer.
  PUSH HL                 ;
  LD HL,CONSTANTS         ; The base address of the calculator's table of constants.
  EXX                     ; Back to the main set of registers.
  CALL SKIP_CONS          ; Find the requested base address.
  CALL STK_CONST          ; Expand the constant.
  EXX                     ; Restore the next literal pointer.
  POP HL                  ;
  EXX                     ;
  RET                     ; Finished.

; THE 'STORE IN MEMORY AREA' SUBROUTINE (offset +40)
;
; The address of this routine is found in the table of addresses. It is called via a calculator literal (+C0 to +C5) by the routines at BEEP, FOR, NEXT, CIRCLE, DRAW, CD_PRMS1, DEC_TO_FP, e_to_fp, PRINT_FP, series, n_mod_m, int, exp and get_argt.
;
; This subroutine is called using the literals +C0 to +C5 and the parameter derived from these literals is held in the A register. This subroutine is very similar to get_mem but the source and destination pointers are exchanged.
;
; A Index of the required memory area (+00 to +05)
; HL Source address
st_mem:
  PUSH HL                 ; Save the result pointer.
  EX DE,HL                ; Source to DE briefly.
  LD HL,(MEM)             ; Fetch the pointer to the current memory area (MEM).
  CALL LOC_MEM            ; The base address is found.
  EX DE,HL                ; Exchange source and destination pointers.
  CALL duplicate          ; The five bytes are moved.
  EX DE,HL                ; 'Last value'+5, i.e. STKEND, to DE.
  POP HL                  ; Result pointer to HL.
  RET                     ; Finished.
; Note that the pointers HL and DE remain as they were, pointing to STKEND-5 and STKEND respectively, so that the 'last value' remains on the calculator stack. If required it can be removed by using delete.

; THE 'EXCHANGE' SUBROUTINE (offset +01)
;
; Used by the routine at compare.
;
; The address of this routine is found in the table of addresses. It is called via the calculator literal +01 by the routines at BEEP, OPEN, FOR, NEXT_LOOP, CIRCLE, DRAW, CD_PRMS1, INT_TO_FP, PRINT_FP, series, n_mod_m, int, ln, get_argt, tan, atn and to_power.
;
; This binary operation 'exchanges' the first number with the second number, i.e. the topmost two numbers on the calculator stack are exchanged.
;
;   DE Address of the first byte of the second number
;   HL Address of the first byte of the first number
; O:HL Address of the first byte of the second number
exchange:
  LD B,$05                ; There are five bytes involved.
SWAP_BYTE:
  LD A,(DE)               ; Each byte of the second number.
  LD C,(HL)               ; Each byte of the first number.
  EX DE,HL                ; Switch source and destination.
  LD (DE),A               ; Now to the first number.
  LD (HL),C               ; Now to the second number.
  INC HL                  ; Move to consider the next pair of bytes.
  INC DE                  ;
  DJNZ SWAP_BYTE          ; Exchange the five bytes.
  EX DE,HL                ; Get the pointers correct as 5 is an odd number.
  RET                     ; Finished.

; THE 'SERIES GENERATOR' SUBROUTINE (offset +3E)
;
; The address of this routine is found in the table of addresses. It is called via a calculator literal (+86, +88 or +8C) by the routines at exp, ln, sin and atn.
;
; This important subroutine generates the series of Chebyshev polynomials which are used to approximate to SIN, ATN, LN and EXP and hence to derive the other arithmetic functions which depend on these (COS, TAN, ASN, ACS, ** and SQR).
;
; In simple terms this subroutine is called with the 'last value' on the calculator stack, say Z, being a number that bears a simple relationship to the argument, say X, when the task is to evaluate, for instance, SIN X. The calling subroutine also supplies the list of constants that are to be required (six constants for SIN). The series generator then manipulates its data and returns to the calling routine a 'last value' that bears a simple relationship to the requested function, for instance,
; SIN X.
;
; A Series parameter (+06, +08 or +0C)
;
; This subroutine can be considered to have four major parts.
;
; i. The setting of the loop counter. The calling subroutine passes its parameters in the A register for use as a counter. The calculator is entered at GEN_ENT_1 so that the counter can be set.
series:
  LD B,A                  ; Move the parameter to B.
  CALL GEN_ENT_1          ; In effect a RST $28 instruction but sets the counter.
; ii. The handling of the 'last value', Z. The loop of the generator requires 2*Z to be placed in mem-0, zero to be placed in mem-2 and the 'last value' to be zero.
  DEFB $31                ; duplicate: Z, Z
  DEFB $0F                ; addition: 2*Z
  DEFB $C0                ; st_mem_0: 2*Z (mem-0 holds 2*Z)
  DEFB $02                ; delete: -
  DEFB $A0                ; stk_zero: 0
  DEFB $C2                ; st_mem_2: 0 (mem-2 holds 0)
; iii. The main loop.
;
; The series is generated by looping, using BREG as a counter; the constants in the calling subroutine are stacked in turn by calling stk_data; the calculator is re-entered at GEN_ENT_2 so as not to disturb the value of BREG; and the series is built up in the form:
;
; B(R)=2*Z*B(R-1)-B(R-2)+A(R), for R=1, 2, ..., N, where A(1), A(2)...A(N) are the constants supplied by the calling subroutine (SIN, ATN, LN and EXP) and B(0)=0=B(-1).
;
; The (R+1)th loop starts with B(R) on the stack and with 2*Z, B(R-2) and B(R-1) in mem-0, mem-1 and mem-2 respectively.
G_LOOP:
  DEFB $31                ; duplicate: B(R), B(R)
  DEFB $E0                ; get_mem_0: B(R), B(R), 2*Z
  DEFB $04                ; multiply: B(R), 2*B(R)*Z
  DEFB $E2                ; get_mem_2: B(R),2*B(R)*Z, B(R-1)
  DEFB $C1                ; st_mem_1: mem-1 holds B(R-1)
  DEFB $03                ; subtract: B(R), 2*B(R)*Z-B(R-1)
  DEFB $38                ; end_calc
; The next constant is placed on the calculator stack.
  CALL stk_data           ; B(R), 2*B(R)*Z-B(R-1), A(R+1)
; The calculator is re-entered without disturbing BREG.
  CALL GEN_ENT_2
  DEFB $0F                ; addition: B(R), 2*B(R)*Z-B(R-1)+A(R+1)
  DEFB $01                ; exchange: 2*B(R)*Z-B(R-1)+A(R+1), B(R)
  DEFB $C2                ; st_mem_2: mem-2 holds B(R)
  DEFB $02                ; delete: 2*B(R)*Z-B(R-1)+A(R+1)=B(R+1)
  DEFB $35                ; dec_jr_nz to G_LOOP: B(R+1)
  DEFB $EE                ;
; iv. The subtraction of B(N-2). The loop above leaves B(N) on the stack and the required result is given by B(N)-B(N-2).
  DEFB $E1                ; get_mem_1: B(N), B(N-2)
  DEFB $03                ; subtract: B(N)-B(N-2)
  DEFB $38                ; end_calc
  RET                     ; Finished.

; THE 'ABSOLUTE MAGNITUDE' FUNCTION (offset +2A)
;
; The address of this routine is found in the table of addresses. It is called via the calculator literal +2A by the routines at CIRCLE, DRAW, CD_PRMS1, PRINT_FP, get_argt and cos. It is also called indirectly via fp_calc_2.
;
; This subroutine performs its unary operation by ensuring that the sign bit of a floating-point number is reset.
;
; 'Small integers' have to be treated separately. Most of the work is shared with the 'unary minus' operation.
;
; HL Address of the first byte of the number
abs:
  LD B,$FF                ; B is set to +FF.
  JR NEG_TEST             ; The jump is made into 'unary minus'.

; THE 'UNARY MINUS' OPERATION (offset +1B)
;
; Used by the routine at subtract.
;
; The address of this routine is found in the table of addresses. It is called via the calculator literal +1B by the routines at CD_PRMS1, get_argt, cos, atn, asn and acs. It is also called indirectly via fp_calc_2.
;
; This subroutine performs its unary operation by changing the sign of the 'last value' on the calculator stack.
;
; Zero is simply returned unchanged. Full five byte floating-point numbers have their sign bit manipulated so that it ends up reset (for 'abs') or changed (for 'negate'). 'Small integers' have their sign byte set to zero (for 'abs') or changed (for 'negate').
;
; HL Address of the first byte of the number
negate:
  CALL TEST_ZERO          ; If the number is zero, the subroutine returns leaving 00 00 00 00 00 unchanged.
  RET C                   ;
  LD B,$00                ; B is set to +00 for 'negate'.
; This entry point is used by the routine at abs.
NEG_TEST:
  LD A,(HL)               ; If the first byte is zero, the jump is made to deal with a 'small integer'.
  AND A                   ;
  JR Z,INT_CASE           ;
  INC HL                  ; Point to the second byte.
  LD A,B                  ; Get +FF for 'abs', +00 for 'negate'.
  AND $80                 ; Now +80 for 'abs', +00 for 'negate'.
  OR (HL)                 ; This sets bit 7 for 'abs', but changes nothing for 'negate'.
  RLA                     ; Now bit 7 is changed, leading to bit 7 of byte 2 reset for 'abs', and simply changed for 'negate'.
  CCF                     ;
  RRA                     ;
  LD (HL),A               ; The new second byte is stored.
  DEC HL                  ; HL points to the first byte again.
  RET                     ; Finished.
; The 'integer case' does a similar operation with the sign byte.
INT_CASE:
  PUSH DE                 ; Save STKEND in DE.
  PUSH HL                 ; Save pointer to the number in HL.
  CALL INT_FETCH          ; Fetch the sign in C, the number in DE.
  POP HL                  ; Restore the pointer to the number in HL.
  LD A,B                  ; Get +FF for 'abs', +00 for 'negate'.
  OR C                    ; Now +FF for 'abs', no change for 'negate'.
  CPL                     ; Now +00 for 'abs', and a changed byte for 'negate'; store it in C.
  LD C,A                  ;
  CALL INT_STORE          ; Store result on the stack.
  POP DE                  ; Return STKEND to DE.
  RET

; THE 'SIGNUM' FUNCTION (offset +29)
;
; The address of this routine is found in the table of addresses. It is called indirectly via fp_calc_2.
;
; This subroutine handles the function SGN X and therefore returns a 'last value' of 1 if X is positive, zero if X is zero and -1 if X is negative.
;
; HL Address of the first byte of the number
sgn:
  CALL TEST_ZERO          ; If X is zero, just return with zero as the 'last value'.
  RET C                   ;
  PUSH DE                 ; Save the pointer to STKEND.
  LD DE,$0001             ; Store 1 in DE.
  INC HL                  ; Point to the second byte of X.
  RL (HL)                 ; Rotate bit 7 into the carry flag.
  DEC HL                  ; Point to the destination again.
  SBC A,A                 ; Set C to zero for positive X and to +FF for negative X.
  LD C,A                  ;
  CALL INT_STORE          ; Stack 1 or -1 as required.
  POP DE                  ; Restore the pointer to STKEND.
  RET                     ; Finished.

; THE 'IN' FUNCTION (offset +2C)
;
; The address of this routine is found in the table of addresses. It is called indirectly via fp_calc_2.
;
; This subroutine handles the function IN X. It inputs at processor level from port X, loading BC with X and performing the instruction 'IN A,(C)'.
f_in:
  CALL FIND_INT2          ; The 'last value', X, is compressed into BC.
  IN A,(C)                ; The signal is received.
  JR IN_PK_STK            ; Jump to stack the result.

; THE 'PEEK' FUNCTION (offset +2B)
;
; The address of this routine is found in the table of addresses. It is called indirectly via fp_calc_2.
;
; This subroutine handles the function PEEK X. The 'last value' is unstacked by calling FIND_INT2 and replaced by the value of the contents of the required location.
peek:
  CALL FIND_INT2          ; Evaluate the 'last value', rounded to the nearest integer; test that it is in range and return it in BC.
  LD A,(BC)               ; Fetch the required byte.
; This entry point is used by the routine at f_in.
IN_PK_STK:
  JP STACK_A              ; Exit by jumping to STACK_A.

; THE 'USR' FUNCTION (offset +2D)
;
; The address of this routine is found in the table of addresses. It is called indirectly via fp_calc_2.
;
; This subroutine ('USR number' as distinct from 'USR string') handles the function USR X, where X is a number. The value of X is obtained in BC, a return address is stacked and the machine code is executed from location X.
usr_no:
  CALL FIND_INT2          ; Evaluate the 'last value', rounded to the nearest integer; test that it is in range and return it in BC.
  LD HL,STACK_BC          ; Make the return address be that of the subroutine STACK_BC.
  PUSH HL                 ;
  PUSH BC                 ; Make an indirect jump to the required location.
  RET                     ;
; Note: it is interesting that the IY register pair is re-initialised when the return to STACK_BC has been made, but the important HL' that holds the next literal pointer is not restored should it have been disturbed. For a successful return to BASIC, HL' must on exit from the machine code contain the address of the 'end-calc' instruction at +2758.

; THE 'USR STRING' FUNCTION (offset +19)
;
; The address of this routine is found in the table of addresses. It is called indirectly via fp_calc_2.
;
; This subroutine handles the function USR X$, where X$ is a string. The subroutine returns in BC the address of the bit pattern for the user-defined graphic corresponding to X$. It reports error A if X$ is not a single letter between 'a' and 'u' or a user-defined graphic.
usr:
  CALL STK_FETCH          ; Fetch the parameters of the string X$.
  DEC BC                  ; Decrease the length by 1 to test it.
  LD A,B                  ; If the length was not 1, then jump to give error report A.
  OR C                    ;
  JR NZ,REPORT_A          ;
  LD A,(DE)               ; Fetch the single code of the string.
  CALL ALPHA              ; Does it denote a letter?
  JR C,USR_RANGE          ; If so, jump to gets its address.
  SUB $90                 ; Reduce range for actual user-defined graphics to 0-20.
  JR C,REPORT_A           ; Give report A if out of range.
  CP $15                  ; Test the range again.
  JR NC,REPORT_A          ; Give report A if out of range.
  INC A                   ; Make range of user-defined graphics 1 to 21, as for 'a' to 'u'.
USR_RANGE:
  DEC A                   ; Now make the range 0 to 20 in each case.
  ADD A,A                 ; Multiply by 8 to get an offset for the address.
  ADD A,A                 ;
  ADD A,A                 ;
  CP $A8                  ; Test the range of the offset.
  JR NC,REPORT_A          ; Give report A if out of range.
  LD BC,(UDG)             ; Fetch the address of the first user-defined graphic (UDG) in BC.
  ADD A,C                 ; Add C to the offset.
  LD C,A                  ; Store the result back in C.
  JR NC,USR_STACK         ; Jump if there is no carry.
  INC B                   ; Increment B to complete the address.
USR_STACK:
  JP STACK_BC             ; Jump to stack the address.
; Report A - Invalid argument.
REPORT_A:
  RST $08                 ; Call the error handling routine.
  DEFB $09                ;

; THE 'TEST-ZERO' SUBROUTINE
;
; Used by the routines at IF_CMD, PREP_M_D, negate, sgn, greater_0, f_not, no_or_no, no_and_no and str_no.
;
; This subroutine is called at least nine times to test whether a floating-point number is zero. This test requires that the first four bytes of the number should each be zero. The subroutine returns with the carry flag set if the number was in fact zero.
;
;   HL Address of the first byte of the number
; O:F Carry flag set if the number is zero
TEST_ZERO:
  PUSH HL                 ; Save HL on the stack.
  PUSH BC                 ; Save BC on the stack.
  LD B,A                  ; Save the value of A in B.
  LD A,(HL)               ; Get the first byte.
  INC HL                  ; Point to the second byte.
  OR (HL)                 ; 'OR' the first byte with the second.
  INC HL                  ; Point to the third byte.
  OR (HL)                 ; 'OR' the result with the third byte.
  INC HL                  ; Point to the fourth byte.
  OR (HL)                 ; 'OR' the result with the fourth byte.
  LD A,B                  ; Restore the original value of A.
  POP BC                  ; And of BC.
  POP HL                  ; Restore the pointer to the number to HL.
  RET NZ                  ; Return with carry reset if any of the four bytes was non-zero.
  SCF                     ; Set the carry flag to indicate that the number was zero, and return.
  RET                     ;

; THE 'GREATER THAN ZERO' OPERATION (offset +37)
;
; Used by the routine at compare.
;
; The address of this routine is found in the table of addresses. It is called via the calculator literal +37 by the routines at NEXT_LOOP, PRINT_FP, ln, get_argt and to_power.
;
; This subroutine returns a 'last value' of one if the present 'last value' is greater than zero and zero otherwise. It is also used by other subroutines to 'jump on plus'.
;
; HL Address of the first byte of the number
greater_0:
  CALL TEST_ZERO          ; Is the 'last-value' zero?
  RET C                   ; If so, return.
  LD A,$FF                ; Jump forward to less_0 but signal the opposite action is needed.
  JR SIGN_TO_C            ;

; THE 'NOT' FUNCTION (offset +30)
;
; Used by the routine at compare.
;
; The address of this routine is found in the table of addresses. It is called via the calculator literal +30 by the routines at DRAW, int, sqr and to_power. It is also called indirectly via fp_calc_2.
;
; This subroutine returns a 'last value' of one if the present 'last value' is zero and zero otherwise. It is also used by other subroutines to 'jump on zero'.
;
; HL Address of the first byte of the number
f_not:
  CALL TEST_ZERO          ; The carry flag will be set only if the 'last value' is zero; this gives the correct result.
  JR FP_0_1               ; Jump forward.

; THE 'LESS THAN ZERO' OPERATION (offset +36)
;
; The address of this routine is found in the table of addresses. It is called via the calculator literal +36 by the routines at NEXT_LOOP, PRINT_FP, int, get_argt and atn.
;
; This subroutine returns a 'last value' of one if the present 'last value' is less than zero and zero otherwise. It is also used by other subroutines to 'jump on minus'.
;
; HL Address of the first byte of the number
less_0:
  XOR A                   ; Clear the A register.
; This entry point is used by the routine at greater_0 with A=+FF.
SIGN_TO_C:
  INC HL                  ; Point to the sign byte.
  XOR (HL)                ; The carry is reset for a positive number and set for a negative number; when entered from greater_0 the opposite sign goes to the carry.
  DEC HL                  ;
  RLCA                    ;
; This routine continues into FP_0_1.

; THE 'ZERO OR ONE' SUBROUTINE
;
; Used by the routines at e_to_fp, f_not, no_or_no and no_and_no.
;
; The routine at less_0 continues here.
;
; This subroutine sets the 'last value' to zero if the carry flag is reset and to one if it is set. When called from e_to_fp however it creates the zero or one not on the stack but in mem-0.
;
; HL Address of the first byte of the number
; F Carry set for 1, reset for 0
FP_0_1:
  PUSH HL                 ; Save the result pointer.
  LD A,$00                ; Clear A without disturbing the carry.
  LD (HL),A               ; Set the first byte to zero.
  INC HL                  ; Point to the second byte.
  LD (HL),A               ; Set the second byte to zero.
  INC HL                  ; Point to the third byte.
  RLA                     ; Rotate the carry into A, making A one if the carry was set, but zero if the carry was reset.
  LD (HL),A               ; Set the third byte to one or zero.
  RRA                     ; Ensure that A is zero again.
  INC HL                  ; Point to the fourth byte.
  LD (HL),A               ; Set the fourth byte to zero.
  INC HL                  ; Point to the fifth byte.
  LD (HL),A               ; Set the fifth byte to zero.
  POP HL                  ; Restore the result pointer.
  RET

; THE 'OR' OPERATION (offset +07)
;
; The address of this routine is found in the table of addresses. It is called indirectly via fp_calc_2.
;
; This subroutine performs the binary operation 'X OR Y' and returns X if Y is zero and the value 1 otherwise.
;
; DE Address of the first byte of the second number (Y)
; HL Address of the first byte of the first number (X)
no_or_no:
  EX DE,HL                ; Point HL at Y, the second number.
  CALL TEST_ZERO          ; Test whether Y is zero.
  EX DE,HL                ; Restore the pointers.
  RET C                   ; Return if Y was zero; X is now the 'last value'.
  SCF                     ; Set the carry flag and jump back to set the 'last value' to 1.
  JR FP_0_1               ;

; THE 'NUMBER AND NUMBER' OPERATION (offset +08)
;
; The address of this routine is found in the table of addresses. It is called indirectly via fp_calc_2.
;
; This subroutine performs the binary operation 'X AND Y' and returns X if Y is non-zero and the value zero otherwise.
;
; DE Address of the first byte of the second number (Y)
; HL Address of the first byte of the first number (X)
no_and_no:
  EX DE,HL                ; Point HL at Y, DE at X.
  CALL TEST_ZERO          ; Test whether Y is zero.
  EX DE,HL                ; Swap the pointers back.
  RET NC                  ; Return with X as the 'last value' if Y was non-zero.
  AND A                   ; Reset the carry flag and jump back to set the 'last value' to zero.
  JR FP_0_1               ;

; THE 'STRING AND NUMBER' OPERATION (offset +10)
;
; The address of this routine is found in the table of addresses. It is called indirectly via fp_calc_2.
;
; This subroutine performs the binary operation 'X$ AND Y' and returns X$ if Y is non-zero and a null string otherwise.
;
; DE Address of the first byte of the number (Y)
str_no:
  EX DE,HL                ; Point HL at Y, DE at X$.
  CALL TEST_ZERO          ; Test whether Y is zero.
  EX DE,HL                ; Swap the pointers back.
  RET NC                  ; Return with X$ as the 'last value' if Y was non-zero.
  PUSH DE                 ; Save the pointer to the number.
  DEC DE                  ; Point to the fifth byte of the string parameters, i.e. length-high.
  XOR A                   ; Clear the A register.
  LD (DE),A               ; Length-high is now set to zero.
  DEC DE                  ; Point to length-low.
  LD (DE),A               ; Length-low is now set to zero.
  POP DE                  ; Restore the pointer.
  RET                     ; Return with the string parameters being the 'last value'.

; THE 'COMPARISON' OPERATIONS (offsets +09 to +0E, +11 to +16)
;
; The address of this routine is found in the table of addresses. It is called indirectly via fp_calc_2.
;
; This subroutine is used to perform the twelve possible comparison operations (offsets +09 to +0E and +11 to +16: '<=', '>=', '<>', '>', '<' and '=' for numbers and strings respectively). The single operation offset is present in the B register at the start of the subroutine.
;
; B Operation offset (+09 to +0E, +11 to +16)
; DE Address of the first byte of the second argument
; HL Address of the first byte of the first argument
compare:
  LD A,B                  ; The single offset goes to the A register.
  SUB $08                 ; The range is now +01 to +06 and +09 to +0E.
  BIT 2,A                 ; This range is changed to +00, +01, +02, +04, +05, +06, +08, +09, +0A, +0C, +0D, +0E.
  JR NZ,EX_OR_NOT         ;
  DEC A                   ;
EX_OR_NOT:
  RRCA                    ; Then reduced to +00 to +07 with carry set for 'greater than or equal to' and 'less than'; the operations with carry set are then treated as their complementary operation once their values have been exchanged.
  JR NC,NU_OR_STR         ;
  PUSH AF                 ;
  PUSH HL                 ;
  CALL exchange           ;
  POP DE                  ;
  EX DE,HL                ;
  POP AF                  ;
NU_OR_STR:
  BIT 2,A                 ; The numerical comparisons are now separated from the string comparisons by testing bit 2.
  JR NZ,STRINGS           ;
  RRCA                    ; The numerical operations now have the range +00 to +01 with carry set for 'equal' and 'not equal'.
  PUSH AF                 ; Save the offset.
  CALL subtract           ; The numbers are subtracted for the final tests.
  JR END_TESTS            ;
STRINGS:
  RRCA                    ; The string comparisons now have the range +02 to +03 with carry set for 'equal' and 'not equal'.
  PUSH AF                 ; Save the offset.
  CALL STK_FETCH          ; The lengths and starting addresses of the strings are fetched from the calculator stack.
  PUSH DE                 ;
  PUSH BC                 ;
  CALL STK_FETCH          ;
  POP HL                  ; The length of the second string.
BYTE_COMP:
  LD A,H
  OR L
  EX (SP),HL
  LD A,B
  JR NZ,SEC_PLUS          ; Jump unless the second string is null.
  OR C
SECND_LOW:
  POP BC                  ; Here the second string is either null or less than the first.
  JR Z,BOTH_NULL
  POP AF
  CCF                     ; The carry is complemented to give the correct test results.
  JR STR_TEST             ;
BOTH_NULL:
  POP AF                  ; Here the carry is used as it stands.
  JR STR_TEST             ;
SEC_PLUS:
  OR C
  JR Z,FRST_LESS          ; The first string is now null, the second not.
  LD A,(DE)               ; Neither string is null, so their next bytes are compared.
  SUB (HL)                ;
  JR C,FRST_LESS          ; Jump if the first byte is less.
  JR NZ,SECND_LOW         ; Jump if the second byte is less.
  DEC BC                  ; The bytes are equal; so the lengths are decremented and a jump is made to BYTE_COMP to compare the next bytes of the reduced strings.
  INC DE                  ;
  INC HL                  ;
  EX (SP),HL              ;
  DEC HL                  ;
  JR BYTE_COMP            ;
FRST_LESS:
  POP BC
  POP AF
  AND A                   ; The carry is cleared here for the correct test results.
STR_TEST:
  PUSH AF                 ; For the string tests, a zero is put on to the calculator stack.
  RST $28                 ;
  DEFB $A0                ; stk_zero
  DEFB $38                ; end_calc
END_TESTS:
  POP AF                  ; These three tests, called as needed, give the correct results for all twelve comparisons. The initial carry is set for 'not equal' and 'equal', and the final carry is set for 'greater than', 'less than' and 'equal'.
  PUSH AF                 ;
  CALL C,f_not            ;
  POP AF                  ;
  PUSH AF                 ;
  CALL NC,greater_0       ;
  POP AF                  ;
  RRCA                    ;
  CALL NC,f_not           ;
  RET                     ; Finished.

; THE 'STRING CONCATENATION' OPERATION (offset +17)
;
; The address of this routine is found in the table of addresses. It is called indirectly via fp_calc_2.
;
; This subroutine performs the binary operation 'A$+B$'. The parameters for these strings are fetched and the total length found. Sufficient room to hold both the strings is made available in the work space and the strings are copied over. The result of this subroutine is therefore to produce a temporary variable A$+B$ that resides in the work space.
strs_add:
  CALL STK_FETCH          ; The parameters of the second string are fetched and saved.
  PUSH DE                 ;
  PUSH BC                 ;
  CALL STK_FETCH          ; The parameters of the first string are fetched.
  POP HL                  ; The lengths are now in HL and BC.
  PUSH HL                 ;
  PUSH DE                 ; The parameters of the first string are saved.
  PUSH BC                 ;
  ADD HL,BC               ; The total length of the two strings is calculated and passed to BC.
  LD B,H                  ;
  LD C,L                  ;
  RST $30                 ; Sufficient room is made available.
  CALL STK_STO            ; The parameters of the new string are passed to the calculator stack.
  POP BC                  ; The parameters of the first string are retrieved and the string copied to the work space as long as it is not a null string.
  POP HL                  ;
  LD A,B                  ;
  OR C                    ;
  JR Z,OTHER_STR          ;
  LDIR                    ;
OTHER_STR:
  POP BC                  ; Exactly the same procedure is followed for the second string thereby giving 'A$+B$'.
  POP HL                  ;
  LD A,B                  ;
  OR C                    ;
  JR Z,STK_PNTRS          ;
  LDIR                    ;
; This routine continues into STK_PNTRS.

; THE 'STK-PNTRS' SUBROUTINE
;
; Used by the routines at CALCULATE, strs_add, val and read_in.
;
; This subroutine resets the HL register pair to point to the first byte of the 'last value', i.e. STKEND-5, and the DE register pair to point one past the 'last value', i.e. STKEND.
;
; O:DE STKEND
; O:HL STKEND-5
STK_PNTRS:
  LD HL,(STKEND)          ; Fetch the current value of STKEND.
  LD DE,$FFFB             ; Set DE to -5, two's complement.
  PUSH HL                 ; Stack the value for STKEND.
  ADD HL,DE               ; Calculate STKEND-5.
  POP DE                  ; DE now holds STKEND and HL holds STKEND-5.
  RET

; THE 'CHR$' FUNCTION (offset +2F)
;
; The address of this routine is found in the table of addresses. It is called indirectly via fp_calc_2.
;
; This subroutine handles the function CHR$ X and creates a single character string in the work space.
chrs:
  CALL FP_TO_A            ; The 'last value' is compressed into the A register.
  JR C,REPORT_B_4         ; Give the error report if X is greater than 255, or X is a negative number.
  JR NZ,REPORT_B_4        ;
  PUSH AF                 ; Save the compressed value of X.
  LD BC,$0001             ; Make one space available in the work space.
  RST $30                 ;
  POP AF                  ; Fetch the value.
  LD (DE),A               ; Copy the value to the work space.
  CALL STK_STO            ; Pass the parameters of the new string to the calculator stack.
  EX DE,HL                ; Reset the pointers.
  RET                     ; Finished.
; Report B - Integer out of range.
REPORT_B_4:
  RST $08                 ; Call the error handling routine.
  DEFB $0A                ;

; THE 'VAL' AND 'VAL$' FUNCTIONS (offsets +18, +1D)
;
; The address of this routine is found in the table of addresses. It is called indirectly via fp_calc_2.
;
; This subroutine handles the functions VAL X$ and VAL$ X$. When handling VAL X$, it returns a 'last value' that is the result of evaluating the string (without its bounding quotes) as a numerical expression. when handling VAL$ X$, it evaluates X$ (without its bounding quotes) as a string expression, and returns the parameters of that string expression as a 'last value' on the calculator stack.
;
; B Offset (+18 or +1D)
val:
  LD HL,(CH_ADD)          ; The current value of CH-ADD is preserved on the machine stack.
  PUSH HL                 ;
  LD A,B                  ; The 'offset' for 'val' or 'val$' must be in the B register; it is now copied to A.
  ADD A,$E3               ; Produce +00 and carry set for 'val', +FB and carry reset for 'val$'.
  SBC A,A                 ; Produce +FF (bit 6 therefore set) for 'val', but +00 (bit 6 reset) for 'val$'.
  PUSH AF                 ; Save this 'flag' on the machine stack.
  CALL STK_FETCH          ; The parameters of the string are fetched; the starting address is saved; one byte is added to the length and room made available for the string (+1) in the work space.
  PUSH DE                 ;
  INC BC                  ;
  RST $30                 ;
  POP HL                  ; The starting address of the string goes to HL as a source address.
  LD (CH_ADD),DE          ; The pointer to the first new space goes to CH-ADD and to the machine stack.
  PUSH DE                 ;
  LDIR                    ; The string is copied to the work space, together with an extra byte.
  EX DE,HL                ; Switch the pointers.
  DEC HL                  ; The extra byte is replaced by a 'carriage return' character.
  LD (HL),$0D             ;
  RES 7,(IY+$01)          ; The syntax flag (bit 7 of FLAGS) is reset and the string is scanned for correct syntax.
  CALL SCANNING           ;
  RST $18                 ; The character after the string is fetched.
  CP $0D                  ; A check is made that the end of the expression has been reached.
  JR NZ,V_RPORT_C         ; If not, the error is reported.
  POP HL                  ; The starting address of the string is fetched.
  POP AF                  ; The 'flag' for 'val/val$' is fetched and bit 6 is compared with bit 6 of the result (FLAGS) of the syntax scan.
  XOR (IY+$01)            ;
  AND $40                 ;
V_RPORT_C:
  JP NZ,REPORT_C          ; Report the error if they do not match.
  LD (CH_ADD),HL          ; Start address to CH-ADD again.
  SET 7,(IY+$01)          ; The flag (bit 7 of FLAGS) is set for line execution.
  CALL SCANNING           ; The string is treated as a 'next expression' and a 'last value' produced.
  POP HL                  ; The original value of CH-ADD is restored.
  LD (CH_ADD),HL          ;
  JR STK_PNTRS            ; The subroutine exits via STK_PNTRS which resets the pointers.

; THE 'STR$' FUNCTION (offset +2E)
;
; The address of this routine is found in the table of addresses. It is called indirectly via fp_calc_2.
;
; This subroutine handles the function STR$ X and returns a 'last value' which is a set of parameters that define a string containing what would appear on the screen if X were displayed by a PRINT command.
str:
  LD BC,$0001             ; One space is made in the work space and its address is copied to K-CUR, the address of the cursor.
  RST $30                 ;
  LD (K_CUR),HL           ;
  PUSH HL                 ; This address is saved on the stack too.
  LD HL,(CURCHL)          ; The current channel address (CURCHL) is saved on the machine stack.
  PUSH HL                 ;
  LD A,$FF                ; Channel 'R' is opened, allowing the string to be 'printed' out into the work space.
  CALL CHAN_OPEN          ;
  CALL PRINT_FP           ; The 'last value', X, is now printed out in the work space and the work space is expanded with each character.
  POP HL                  ; Restore CURCHL to HL and restore the flags that are appropriate to it.
  CALL CHAN_FLAG          ;
  POP DE                  ; Restore the start address of the string.
  LD HL,(K_CUR)           ; Now the cursor address is one past the end of the string and hence the difference is the length.
  AND A                   ;
  SBC HL,DE               ;
  LD B,H                  ; Transfer the length to BC.
  LD C,L                  ;
  CALL STK_STO            ; Pass the parameters of the new string to the calculator stack.
  EX DE,HL                ; Reset the pointers.
  RET                     ; Finished.
; Note: see PRINT_FP for an explanation of the 'PRINT "A"+STR$ 0.1' error.

; THE 'READ-IN' SUBROUTINE (offset +1A)
;
; The address of this routine is found in the table of addresses. It is called indirectly via fp_calc_2.
;
; This subroutine is called via the calculator offset (+5A) through the first line of S_INKEY. It appears to provide for the reading in of data through different streams from those available on the standard Spectrum. Like S_INKEY the subroutine returns a string.
read_in:
  CALL FIND_INT1          ; The numerical parameter is compressed into the A register.
  CP $10                  ; Is it smaller than 16?
  JP NC,REPORT_B_2        ; If not, report the error.
  LD HL,(CURCHL)          ; The current channel address (CURCHL) is saved on the machine stack.
  PUSH HL                 ;
  CALL CHAN_OPEN          ; The channel specified by the parameter is opened.
  CALL INPUT_AD           ; The signal is now accepted, like a 'key-value'.
  LD BC,$0000             ; The default length of the resulting string is zero.
  JR NC,R_I_STORE         ; Jump if there was no signal.
  INC C                   ; Set the length to 1 now.
  RST $30                 ; Make a space in the work space.
  LD (DE),A               ; Put the string into it.
R_I_STORE:
  CALL STK_STO            ; Pass the parameters of the string to the calculator stack.
  POP HL                  ; Restore CURCHL and the appropriate flags.
  CALL CHAN_FLAG          ;
  JP STK_PNTRS            ; Exit, setting the pointers.

; THE 'CODE' FUNCTION (offset +1C)
;
; The address of this routine is found in the table of addresses. It is called indirectly via fp_calc_2.
;
; This subroutine handles the function CODE A$ and returns the Spectrum code of the first character in A$, or zero if A$ is null.
code:
  CALL STK_FETCH          ; The parameters of the string are fetched.
  LD A,B                  ; The length is tested and the A register holding zero is carried forward if A$ is a null string.
  OR C                    ;
  JR Z,STK_CODE           ;
  LD A,(DE)               ; The code of the first character is put into A otherwise.
STK_CODE:
  JP STACK_A              ; The subroutine exits via STACK_A which gives the correct 'last value'.

; THE 'LEN' FUNCTION (offset +1E)
;
; The address of this routine is found in the table of addresses. It is called indirectly via fp_calc_2.
;
; This subroutine handles the function LEN A$ and returns a 'last value' that is equal to the length of the string.
len:
  CALL STK_FETCH          ; The parameters of the string are fetched.
  JP STACK_BC             ; The subroutine exits via STACK_BC which gives the correct 'last value'.

; THE 'DECREASE THE COUNTER' SUBROUTINE (offset +35)
;
; The address of this routine is found in the table of addresses. It is called via the calculator literal +35 by the routine at series.
;
; This subroutine is only called by the series generator and in effect is a 'DJNZ' operation but the counter is the system variable, BREG, rather than the B register.
;
;   HL' Address of the jump offset
; O:HL' Address of the next calculator literal to execute
dec_jr_nz:
  EXX                     ; Go to the alternative register set and save the next literal pointer on the machine stack.
  PUSH HL                 ;
  LD HL,BREG              ; Make HL point to BREG.
  DEC (HL)                ; Decrease BREG.
  POP HL                  ; Restore the next literal pointer.
  JR NZ,JUMP_2            ; The jump is made on non-zero.
  INC HL                  ; The next literal is passed over.
  EXX                     ; Return to the main register set.
  RET                     ; Finished.

; THE 'JUMP' SUBROUTINE (offset +33)
;
; Used by the routine at jump_true.
;
; The address of this routine is found in the table of addresses. It is called via the calculator literal +33 by the routines at e_to_fp, cos and atn.
;
; This subroutine executes an unconditional jump when called by the literal '+33'.
;
;   HL' Address of the jump offset
; O:HL' Address of the next calculator literal to execute
jump:
  EXX                     ; Go to the next alternate register set.
; This entry point is used by the routine at dec_jr_nz.
JUMP_2:
  LD E,(HL)               ; The next literal (jump length) is put in the E' register.
  LD A,E                  ; The number +00 or +FF is formed in A according as E' is positive or negative, and is then copied to D'.
  RLA                     ;
  SBC A,A                 ;
  LD D,A                  ;
  ADD HL,DE               ; HL' now holds the next literal pointer.
  EXX                     ;
  RET                     ; Finished.

; THE 'JUMP ON TRUE' SUBROUTINE (offset +00)
;
; The address of this routine is found in the table of addresses. It is called via the calculator literal +00 by the routines at NEXT_LOOP, DRAW, e_to_fp, PRINT_FP, int, ln, get_argt, cos, atn, sqr and to_power.
;
; This subroutine executes a conditional jump if the 'last value' on the calculator stack, or more precisely the number addressed currently by the DE register pair, is true.
;
;   DE Address of the first byte of the last value on the calculator stack
;   HL' Address of the jump offset
; O:HL' Address of the next calculator literal to execute
jump_true:
  INC DE                  ; Point to the third byte, which is zero or one.
  INC DE                  ;
  LD A,(DE)               ; Collect this byte in the A register.
  DEC DE                  ; Point to the first byte once again.
  DEC DE                  ;
  AND A                   ; Test the third byte: is it zero?
  JR NZ,jump              ; Make the jump if the byte is non-zero, i.e. if the number is not-false.
  EXX                     ; Go to the alternate register set.
  INC HL                  ; Pass over the jump length.
  EXX                     ; Back to the main set of registers.
  RET                     ; Finished.

; THE 'END-CALC' SUBROUTINE (offset +38)
;
; The address of this routine is found in the table of addresses. It is called via the calculator literal +38 by the routines at BEEP, OPEN, FETCH_NUM, IF_CMD, FOR, NEXT, NEXT_LOOP, CIRCLE, DRAW, CD_PRMS1, S_RND, S_PI, S_LETTER, LET, DEC_TO_FP, STACK_BC, INT_TO_FP, e_to_fp, FP_TO_BC, LOG_2_A, PRINT_FP, series, compare, n_mod_m, int, exp, ln, get_argt, sin, tan, atn, asn, acs, sqr and to_power.
;
; This subroutine ends a RST $28 operation.
end_calc:
  POP AF                  ; The return address to the calculator (RE_ENTRY) is discarded.
  EXX                     ; Instead, the address in HL' is put on the machine stack and an indirect jump is made to it. HL' will now hold any earlier address in the calculator chain of addresses.
  EX (SP),HL              ;
  EXX                     ;
  RET                     ; Finished.

; THE 'MODULUS' SUBROUTINE (offset +32)
;
; The address of this routine is found in the table of addresses. It is called via the calculator literal +32 by the routine at S_RND.
;
; This subroutine calculates N (mod M), where M is a positive integer held at the top of the calculator stack (the 'last value'), and N is the integer held on the stack beneath M.
;
; The subroutine returns the integer quotient INT (N/M) at the top of the calculator stack (the 'last value'), and the remainder N-INT (N/M) in the second place on the stack.
;
; This subroutine is called during the calculation of a random number to reduce N mod 65537.
n_mod_m:
  RST $28                 ; N, M
  DEFB $C0                ; st_mem_0: N, M (mem-0 holds M)
  DEFB $02                ; delete: N
  DEFB $31                ; duplicate: N, N
  DEFB $E0                ; get_mem_0: N, N, M
  DEFB $05                ; division: N, N/M
  DEFB $27                ; int: N, INT (N/M)
  DEFB $E0                ; get_mem_0: N, INT (N/M), M
  DEFB $01                ; exchange: N, M, INT (N/M)
  DEFB $C0                ; st_mem_0: N, M, INT (N/M) (mem-0 holds INT (N/M))
  DEFB $04                ; multiply: N, M*INT (N/M)
  DEFB $03                ; subtract: N-M*INT (N/M)
  DEFB $E0                ; get_mem_0: N-M*INT (N/M), INT (N/M)
  DEFB $38                ; end_calc
  RET                     ; Finished.

; THE 'INT' FUNCTION (offset +27)
;
; The address of this routine is found in the table of addresses. It is called via the calculator literal +27 by the routines at BEEP, FP_TO_BC, LOG_2_A, PRINT_FP, n_mod_m, exp and get_argt. It is also called indirectly via fp_calc_2.
;
; This subroutine handles the function INT X and returns a 'last value' that is the 'integer part' of the value supplied. Thus INT 2.4 gives 2 but as the subroutine always rounds the result down INT -2.4 gives -3.
;
; The subroutine uses truncate to produce I(X) such that I(2.4)=2 and I(-2.4)=-2. Thus, INT X is given by I(X) when X>=0, and by I(X)-1 for negative values of X that are not already integers, when the result is, of course, I(X).
int:
  RST $28                 ; X
  DEFB $31                ; duplicate: X, X
  DEFB $36                ; less_0: X, (1/0)
  DEFB $00                ; jump_true to X_NEG: X
  DEFB $04                ;
; For values of X that have been shown to be greater than or equal to zero there is no jump and I(X) is readily found.
  DEFB $3A                ; truncate: I(X)
  DEFB $38                ; end_calc
  RET                     ; Finished.
; When X is a negative integer I(X) is returned, otherwise I(X)-1 is returned.
X_NEG:
  DEFB $31                ; duplicate: X, X
  DEFB $3A                ; truncate: X, I(X)
  DEFB $C0                ; st_mem_0: X, I(X) (mem-0 holds I(X))
  DEFB $03                ; subtract: X-I(X)
  DEFB $E0                ; get_mem_0: X-I(X), I(X)
  DEFB $01                ; exchange: I(X), X-I(X)
  DEFB $30                ; f_not: I(X), (1/0)
  DEFB $00                ; jump_true to EXIT: I(X)
  DEFB $03                ;
; The jump is made for values of X that are negative integers, otherwise there is no jump and I(X)-1 is calculated.
  DEFB $A1                ; stk_one: I(X), 1
  DEFB $03                ; subtract: I(X)-1
; In either case the subroutine finishes with:
EXIT:
  DEFB $38                ; end_calc: I(X) or I(X)-1
  RET

; THE 'EXPONENTIAL' FUNCTION (offset +26)
;
; Used by the routine at to_power.
;
; The address of this routine is found in the table of addresses. It is called indirectly via fp_calc_2.
;
; This subroutine handles the function EXP X and is the first of four routines that use the series generator to produce Chebyshev polynomials.
;
; The approximation to EXP X is found as follows:
;
; * i. X is divided by LN 2 to give Y, so that 2**Y is now the required result.
; * ii. The value N is found, such that N=INT Y.
; * iii. The value W=Y-N is found; 0<=W<=1, as required for the series to converge.
; * iv. The argument Z=2*W-1 is formed.
; * v. The series generator is used to return 2**W.
; * vi. Finally N is added to the exponent, giving 2**(N+W), which is 2**Y and therefore the required answer.
exp:
  RST $28                 ; X
; Perform step i.
  DEFB $3D                ; re_stack: X (in full floating-point form)
  DEFB $34                 ; stk_data: X, 1/LN 2
  DEFB $F1,$38,$AA,$3B,$29 ;
  DEFB $04                ; multiply: X/LN 2=Y
; Perform step ii.
  DEFB $31                ; duplicate: Y, Y
  DEFB $27                ; int: Y, INT Y=N
  DEFB $C3                ; st_mem_3: Y, N (mem-3 holds N)
; Perform step iii.
  DEFB $03                ; subtract: Y-N=W
; Perform step iv.
  DEFB $31                ; duplicate: W, W
  DEFB $0F                ; addition: 2*W
  DEFB $A1                ; stk_one: 2*W, 1
  DEFB $03                ; subtract: 2*W-1=Z
; Perform step v, passing to the series generator the parameter '8' and the eight constants required.
  DEFB $88                ; series_08: Z
  DEFB $13,$36
  DEFB $58,$65,$66
  DEFB $9D,$78,$65,$40
  DEFB $A2,$60,$32,$C9
  DEFB $E7,$21,$F7,$AF,$24
  DEFB $EB,$2F,$B0,$B0,$14
  DEFB $EE,$7E,$BB,$94,$58
  DEFB $F1,$3A,$7E,$F8,$CF
; At the end of the last loop the 'last value' is 2**W.
;
; Perform step vi.
  DEFB $E3                ; get_mem_3: 2**W, N
  DEFB $38                ; end_calc
  CALL FP_TO_A            ; The absolute value of N mod 256 is put into the A register.
  JR NZ,N_NEGTV           ; Jump forward if N was negative.
  JR C,REPORT_6_2         ; Error if ABS N>+FF.
  ADD A,(HL)              ; Now add ABS N to the exponent.
  JR NC,RESULT_OK         ; Jump unless e>+FF.
; Report 6 - Number too big.
REPORT_6_2:
  RST $08                 ; Call the error handling routine.
  DEFB $05                ;
N_NEGTV:
  JR C,RSLT_ZERO          ; The result is to be zero if N<-255.
  SUB (HL)                ; Subtract ABS N from the exponent as N was negative.
  JR NC,RSLT_ZERO         ; Zero result if e less than zero.
  NEG                     ; Minus e is changed to e.
RESULT_OK:
  LD (HL),A               ; The exponent, e, is entered.
  RET                     ; Finished: 'last value' is EXP X.
RSLT_ZERO:
  RST $28                 ; Use the calculator to make the 'last value' zero.
  DEFB $02                ; delete (the stack is now empty)
  DEFB $A0                ; stk_zero: 0
  DEFB $38                ; end_calc
  RET                     ; Finished, with EXP X=0.

; THE 'NATURAL LOGARITHM' FUNCTION (offset +25)
;
; The address of this routine is found in the table of addresses. It is called via the calculator literal +25 by the routine at to_power. It is also called indirectly via fp_calc_2.
;
; This subroutine handles the function LN X and is the second of the four routines that use the series generator to produce Chebyshev polynomials.
;
; The approximation to LN X is found as follows:
;
; * i. X is tested and report A is given if X is not positive.
; * ii. X is then split into its true exponent, e', and its mantissa X'=X/(2**e'), where 0.5<=X'<1.
; * iii. The required value Y1 or Y2 is formed: if X'>0.8 then Y1=e'*LN 2, otherwise Y2=(e'-1)*LN 2.
; * iv. If X'>0.8 then the quantity X'-1 is stacked; otherwise 2*X'-1 is stacked.
; * v. Now the argument Z is formed, being 2.5*X'-3 if X'>0.8, otherwise 5*X'-3. In each case, -1<=Z<=1, as required for the series to converge.
; * vi. The series generator is used to produce the required function.
; * vii. Finally a simple multiplication and addition leads to LN X being returned as the 'last value'.
ln:
  RST $28                 ; X
; Perform step i.
  DEFB $3D                ; re_stack: X (in full floating-point form)
  DEFB $31                ; duplicate: X, X
  DEFB $37                ; greater_0: X, (1/0)
  DEFB $00                ; jump_true to VALID: X
  DEFB $04                ; multiply: X
  DEFB $38                ; end_calc: X
; Report A - Invalid argument.
  RST $08                 ; Call the error handling routine.
  DEFB $09                ;
; Perform step ii.
VALID:
  DEFB $A0                ; stk_zero: X, 0 (the deleted 1 is overwritten with zero)
  DEFB $02                ; delete: X
  DEFB $38                ; end_calc: X
  LD A,(HL)               ; The exponent, e, goes into A.
  LD (HL),$80             ; X is reduced to X'.
  CALL STACK_A            ; The stack holds: X', e.
  RST $28                 ; X', e
  DEFB $34                ; stk_data: X', e, 128
  DEFB $38,$00            ;
  DEFB $03                ; subtract: X', e'
; Perform step iii.
  DEFB $01                ; exchange: e', X'
  DEFB $31                ; duplicate: e', X', X'
  DEFB $34                 ; stk_data: e', X', X', 0.8
  DEFB $F0,$4C,$CC,$CC,$CD ;
  DEFB $03                ; subtract: e', X', X'-0.8
  DEFB $37                ; greater_0: e', X', (1/0)
  DEFB $00                ; jump_true to GRE_8: e', X'
  DEFB $08                ;
  DEFB $01                ; exchange: X', e'
  DEFB $A1                ; stk_one: X', e', 1
  DEFB $03                ; subtract: X', e'-1
  DEFB $01                ; exchange: e'-1, X'
  DEFB $38                ; end_calc
  INC (HL)                ; Double X' to give 2*X'.
  RST $28                 ; e'-1, 2*X'
GRE_8:
  DEFB $01                ; exchange: X', e' (X'>0.8) or 2*X', e'-1 (X'<=0.8)
  DEFB $34                 ; stk_data: X', e', LN 2 or 2*X', e'-1, LN 2
  DEFB $F0,$31,$72,$17,$F8 ;
  DEFB $04                ; multiply: X', e'*LN 2=Y1 or 2*X', (e'-1)*LN 2=Y2
; Perform step iv.
  DEFB $01                ; exchange: Y1, X' (X'>0.8) or Y2, 2*X' (X'<=0.8)
  DEFB $A2                ; stk_half: Y1, X', .5 or Y2, 2*X', .5
  DEFB $03                ; subtract: Y1, X'-.5 or Y2, 2*X'-.5
  DEFB $A2                ; stk_half: Y1, X'-.5, .5 or Y2, 2*X'-.5, .5
  DEFB $03                ; subtract: Y1, X'-1 or Y2, 2*X'-1
; Perform step v.
  DEFB $31                ; duplicate: Y, X'-1, X'-1 or Y2, 2*X'-1, 2*X'-1
  DEFB $34                ; stk_data: Y1, X'-1, X'-1, 2.5 or Y2, 2*X'-1, 2*X'-1, 2.5
  DEFB $32,$20            ;
  DEFB $04                ; multiply: Y1, X'-1, 2.5*X'-2.5 or Y2, 2*X'-1, 5*X'-2.5
  DEFB $A2                ; stk_half: Y1, X'-1, 2.5*X'-2.5, .5 or Y2, 2*X'-1, 5*X'-2.5, .5
  DEFB $03                ; subtract: Y1, X'-1, 2.5*X'-3=Z or Y2, 2*X'-1, 5*X'-3=Z
; Perform step vi, passing to the series generator the parameter '12', and the twelve constants required.
  DEFB $8C                ; series_0C: Y1, X'-1, Z or Y2, 2*X'-1, Z
  DEFB $11,$AC
  DEFB $14,$09
  DEFB $56,$DA,$A5
  DEFB $59,$30,$C5
  DEFB $5C,$90,$AA
  DEFB $9E,$70,$6F,$61
  DEFB $A1,$CB,$DA,$96
  DEFB $A4,$31,$9F,$B4
  DEFB $E7,$A0,$FE,$5C,$FC
  DEFB $EA,$1B,$43,$CA,$36
  DEFB $ED,$A7,$9C,$7E,$5E
  DEFB $F0,$6E,$23,$80,$93
; At the end of the last loop the 'last value' is:
;
; * LN X'/(X'-1) if X'>0.8
; * LN (2*X')/(2*X'-1) if X'<=0.8
;
; Perform step vii.
  DEFB $04                ; multiply: Y1=LN (2**e'), LN X' or Y2=LN (2**(e'-1)), LN (2*X')
  DEFB $0F                ; addition: LN (2**e')*X')=LN X or LN (2**(e'-1)*2*X')=LN X
  DEFB $38                ; end_calc: LN X
  RET                     ; Finished: 'last value' is LN X.

; THE 'REDUCE ARGUMENT' SUBROUTINE (offset +39)
;
; The address of this routine is found in the table of addresses. It is called via the calculator literal +39 by the routines at cos and sin.
;
; This subroutine transforms the argument X of SIN X or COS X into a value V.
;
; The subroutine first finds the value Y=X/2π-INT(X/2π+0.5), where -0.5<=Y<0.5.
;
; The subroutine returns with:
;
; * V=4*Y if -1<=4*Y<=1 (case i)
; * or V=2-4*Y if 1<4*Y<2 (case ii)
; * or V=-4*Y-2 if -2<=4*Y<-1 (case iii)
;
; In each case, -1<=V<=1 and SIN (πV/2)=SIN X.
get_argt:
  RST $28                 ; X
  DEFB $3D                ; re_stack: X (in full floating-point form)
  DEFB $34                 ; stk_data: X, 1/2π
  DEFB $EE,$22,$F9,$83,$6E ;
  DEFB $04                ; multiply: X/2π
  DEFB $31                ; duplicate: X/2π, X/2π
  DEFB $A2                ; stk_half: X/2π, X/2π, 0.5
  DEFB $0F                ; addition: X/2π, X/2π+0.5
  DEFB $27                ; int: X/2π, INT (X/2π+0.5)
  DEFB $03                ; subtract: X/2π-INT (X/2π+0.5)=Y
; Note: adding 0.5 and taking INT rounds the result to the nearest integer.
  DEFB $31                ; duplicate: Y, Y
  DEFB $0F                ; addition: 2*Y
  DEFB $31                ; duplicate: 2*Y, 2*Y
  DEFB $0F                ; addition: 4*Y
  DEFB $31                ; duplicate: 4*Y, 4*Y
  DEFB $2A                ; abs: 4*Y, ABS (4*Y)
  DEFB $A1                ; stk_one: 4*Y, ABS (4*Y), 1
  DEFB $03                ; subtract: 4*Y, ABS (4*Y)-1=Z
  DEFB $31                ; duplicate: 4*Y, Z, Z
  DEFB $37                ; greater_0: 4*Y, Z, (1/0)
  DEFB $C0                ; st_mem_0: (mem-0 holds the result of the test)
  DEFB $00                ; jump_true to ZPLUS: 4*Y, Z
  DEFB $04                ;
  DEFB $02                ; delete: 4*Y
  DEFB $38                ; end_calc: 4*Y=V (case i)
  RET                     ; Finished.
; If the jump was made then continue.
ZPLUS:
  DEFB $A1                ; stk_one: 4*Y, Z, 1
  DEFB $03                ; subtract: 4*Y, Z-1
  DEFB $01                ; exchange: Z-1, 4*Y
  DEFB $36                ; less_0: Z-1, (1/0)
  DEFB $00                ; jump_true to YNEG: Z-1
  DEFB $02                ;
  DEFB $1B                ; negate: 1-Z
YNEG:
  DEFB $38                ; end_calc: 1-Z=V (case ii) or Z-1=V (case iii)
  RET                     ; Finished.

; THE 'COSINE' FUNCTION (offset +20)
;
; The address of this routine is found in the table of addresses. It is called via the calculator literal +20 by the routines at DRAW and tan. It is also called indirectly via fp_calc_2.
;
; This subroutine handles the function COS X and returns a 'last value' 'that is an approximation to COS X.
;
; The subroutine uses the expression COS X=SIN (πW/2), where -1<=W<=1.
;
; In deriving W for X the subroutine uses the test result obtained in the previous subroutine and stored for this purpose in mem-0. It then jumps to the sin subroutine, entering at C_ENT, to produce a 'last value' of COS X.
cos:
  RST $28                 ; X
  DEFB $39                ; get_argt: V
  DEFB $2A                ; abs: ABS V
  DEFB $A1                ; stk_one: ABS V, 1
  DEFB $03                ; subtract: ABS V-1
  DEFB $E0                ; get_mem_0: ABS V-1, (1/0)
  DEFB $00                ; jump_true to C_ENT: ABS V-1=W
  DEFB $06                ;
; If the jump was not made then continue.
  DEFB $1B                ; negate: 1-ABS V
  DEFB $33                ; jump to C_ENT: 1-ABS V=W
  DEFB $03                ;

; THE 'SINE' FUNCTION (offset +1F)
;
; The address of this routine is found in the table of addresses. It is called via the calculator literal +1F by the routines at DRAW, CD_PRMS1 and tan. It is also called indirectly via fp_calc_2.
;
; This subroutine handles the function SIN X and is the third of the four routines that use the series generator to produce Chebyshev polynomials.
;
; The approximation to SIN X is found as follows:
;
; * i. The argument X is reduced to W, such that SIN (π*W/2)=SIN X. Note that -1<=W<=1, as required for the series to converge.
; * ii. The argument Z=2*W*W-1 is formed.
; * iii. The series generator is used to return (SIN (π*W/2))/W.
; * iv. Finally a simple multiplication by W gives SIN X.
sin:
  RST $28                 ; X
; Perform step i.
  DEFB $39                ; get_argt: W
; Perform step ii. The subroutine from now on is common to both the SINE and COSINE functions.
C_ENT:
  DEFB $31                ; duplicate: W, W
  DEFB $31                ; duplicate: W, W, W
  DEFB $04                ; multiply: W, W*W
  DEFB $31                ; duplicate: W, W*W, W*W
  DEFB $0F                ; addition: W, 2*W*W
  DEFB $A1                ; stk_one: W, 2*W*W, 1
  DEFB $03                ; subtract: W, 2*W*W-1=Z
; Perform step iii, passing to the series generator the parameter '6' and the six constants required.
  DEFB $86                ; series_06: W, Z
  DEFB $14,$E6
  DEFB $5C,$1F,$0B
  DEFB $A3,$8F,$38,$EE
  DEFB $E9,$15,$63,$BB,$23
  DEFB $EE,$92,$0D,$CD,$ED
  DEFB $F1,$23,$5D,$1B,$EA
; At the end of the last loop the 'last value' is (SIN (π*W/2))/W.
;
; Perform step iv.
  DEFB $04                ; multiply: SIN (π*W/2)=SIN X (or COS X)
  DEFB $38                ; end_calc
  RET                     ; Finished: 'last value'=SIN X (or COS X).

; THE 'TAN' FUNCTION (offset +21)
;
; The address of this routine is found in the table of addresses. It is called indirectly via fp_calc_2.
;
; This subroutine handles the function TAN X. It simply returns SIN X/COS X, with arithmetic overflow if COS X=0.
tan:
  RST $28                 ; X
  DEFB $31                ; duplicate: X, X
  DEFB $1F                ; sin: X, SIN X
  DEFB $01                ; exchange: SIN X, X
  DEFB $20                ; cos: SIN X, COS X
  DEFB $05                ; division: SIN X/COS X=TAN X (report arithmetic overflow if needed)
  DEFB $38                ; end_calc: TAN X
  RET                     ; Finished: 'last value'=TAN X.

; THE 'ARCTAN' FUNCTION (offset +24)
;
; The address of this routine is found in the table of addresses. It is called via the calculator literal +24 by the routine at asn. It is also called indirectly via fp_calc_2.
;
; This subroutine handles the function ATN X and is the last of the four routines that use the series generator to produce Chebyshev polynomials. It returns a real number between -π/2 and π/2, which is equal to the value in radians of the angle whose tan is X.
;
; The approximation to ATN X is found as follows:
;
; i. The values W and Y are found for three cases of X, such that:
;
; * if -1<X<1 then W=0, Y=X (case i)
; * if 1<=X then W=π/2, Y=-1/X (case ii)
; * if X<=-1 then W=-π/2, Y=-1/X (case iii)
;
; In each case, -1<=Y<=1, as required for the series to converge.
;
; ii. The argument Z is formed, such that:
;
; * if -1<X<1 then Z=2*Y*Y-1=2*X*X-1 (case i)
; * otherwise Z=2*Y*Y-1=2/(X*X)-1 (cases ii and iii)
;
; iii. The series generator is used to produce the required function.
;
; iv. Finally a simple multiplication and addition give ATN X.
;
; HL Address of the first byte of the number (X)
;
; Perform step i.
atn:
  CALL re_stack           ; Use the full floating-point form of X.
  LD A,(HL)               ; Fetch the exponent of X.
  CP $81                  ; Jump forward for case i: Y=X.
  JR C,SMALL              ;
  RST $28                 ; X
  DEFB $A1                ; stk_one: X, 1
  DEFB $1B                ; negate: X, -1
  DEFB $01                ; exchange: -1, X
  DEFB $05                ; division: -1/X
  DEFB $31                ; duplicate: -1/X, -1/X
  DEFB $36                ; less_0: -1/X, (1/0)
  DEFB $A3                ; stk_pi_2: -1/X, (1/0), π/2
  DEFB $01                ; exchange: -1/X, π/2, (1/0)
  DEFB $00                ; jump_true to CASES for case ii: -1/X, π/2
  DEFB $06                ;
  DEFB $1B                ; negate: -1/X, -π/2
  DEFB $33                ; jump to CASES for case iii: -1/X, -π/2
  DEFB $03                ;
SMALL:
  RST $28
  DEFB $A0                ; stk_zero: Y, 0; continue for case i: W=0
; Perform step ii.
CASES:
  DEFB $01                ; exchange: W, Y
  DEFB $31                ; duplicate: W, Y, Y
  DEFB $31                ; duplicate: W, Y, Y, Y
  DEFB $04                ; multiply: W, Y, Y*Y
  DEFB $31                ; duplicate: W, Y, Y*Y, Y*Y
  DEFB $0F                ; addition: W, Y, 2*Y*Y
  DEFB $A1                ; stk_one: W, Y, 2*Y*Y, 1
  DEFB $03                ; subtract: W, Y, 2*Y*Y-1=Z
; Perform step iii, passing to the series generator the parameter '12', and the twelve constants required.
  DEFB $8C                ; series_0C: W, Y, Z
  DEFB $10,$B2
  DEFB $13,$0E
  DEFB $55,$E4,$8D
  DEFB $58,$39,$BC
  DEFB $5B,$98,$FD
  DEFB $9E,$00,$36,$75
  DEFB $A0,$DB,$E8,$B4
  DEFB $63,$42,$C4
  DEFB $E6,$B5,$09,$36,$BE
  DEFB $E9,$36,$73,$1B,$5D
  DEFB $EC,$D8,$DE,$63,$BE
  DEFB $F0,$61,$A1,$B3,$0C
; At the end of the last loop the 'last value' is:
;
; * ATN X/X (case i)
; * ATN (-1/X)/(-1/X) (cases ii and iii)
;
; Perform step iv.
  DEFB $04                ; multiply: W, ATN X (case i) or W, ATN (-1/X) (cases ii and iii)
  DEFB $0F                ; addition: ATN X (all cases now)
  DEFB $38                ; end_calc
  RET                     ; Finished: 'last value'=ATN X.

; THE 'ARCSIN' FUNCTION (offset +22)
;
; The address of this routine is found in the table of addresses. It is called via the calculator literal +22 by the routine at acs. It is also called indirectly via fp_calc_2.
;
; This subroutine handles the function ASN X and returns a real number from -π/2 to π/2 inclusive which is equal to the value in radians of the angle whose sine is X. Thereby if Y=ASN X then X=SIN Y.
;
; This subroutine uses the trigonometric identity TAN (Y/2)=SIN Y/(1+COS Y) to obtain TAN (Y/2) and hence (using ATN) Y/2 and finally Y.
asn:
  RST $28                 ; X
  DEFB $31                ; duplicate: X, X
  DEFB $31                ; duplicate: X, X, X
  DEFB $04                ; multiply: X, X*X
  DEFB $A1                ; stk_one: X, X*X, 1
  DEFB $03                ; subtract: X, X*X-1
  DEFB $1B                ; negate: X, 1-X*X
  DEFB $28                ; sqr: X, SQR (1-X*X)
  DEFB $A1                ; stk_one: X, SQR (1-X*X), 1
  DEFB $0F                ; addition: X, 1+SQR (1-X*X)
  DEFB $05                ; division: X/(1+SQR (1-X*X))=TAN (Y/2)
  DEFB $24                ; atn: Y/2
  DEFB $31                ; duplicate: Y/2, Y/2
  DEFB $0F                ; addition: Y=ASN X
  DEFB $38                ; end_calc
  RET                     ; Finished: 'last value'=ASN X.

; THE 'ARCCOS' FUNCTION (offset +23)
;
; The address of this routine is found in the table of addresses. It is called indirectly via fp_calc_2.
;
; This subroutine handles the function ACS X and returns a real number from 0 to π inclusive which is equal to the value in radians of the angle whose cosine is X.
;
; This subroutine uses the relation ACS X=π/2-ASN X.
acs:
  RST $28                 ; X
  DEFB $22                ; asn: ASN X
  DEFB $A3                ; stk_pi_2: ASN X, π/2
  DEFB $03                ; subtract: ASN X-π/2
  DEFB $1B                ; negate: π/2-ASN X=ACS X
  DEFB $38                ; end_calc
  RET                     ; Finished: 'last value'=ACS X.

; THE 'SQUARE ROOT' FUNCTION (offset +28)
;
; The address of this routine is found in the table of addresses. It is called via the calculator literal +28 by the routines at CD_PRMS1 and asn. It is also called indirectly via fp_calc_2.
;
; This subroutine handles the function SQR X and returns the positive square root of the real number X if X is positive, and zero if X is zero. A negative value of X gives rise to report A - invalid argument (via to_power).
;
; This subroutine treats the square root operation as being X**0.5 and therefore stacks the value 0.5 and proceeds directly into to_power.
sqr:
  RST $28                 ; X
  DEFB $31                ; duplicate: X, X
  DEFB $30                ; f_not: X, (1/0)
  DEFB $00                ; jump_true to LAST: X
  DEFB $1E                ;
; The jump is made if X=0; otherwise continue with:
  DEFB $A2                ; stk_half: X, 0.5
  DEFB $38                ; end_calc
; Continue into to_power to find the result of X**0.5.

; THE 'EXPONENTIATION' OPERATION (offset +06)
;
; The address of this routine is found in the table of addresses. It is called indirectly via fp_calc_2, and the routine at sqr continues here.
;
; This subroutine performs the binary operation of raising the first number, X, to the power of the second number, Y.
;
; The subroutine treats the result X**Y as being equivalent to EXP (Y*LN X). It returns this value unless X is zero, in which case it returns 1 if Y is also zero (0**0=1), returns zero if Y is positive, and reports arithmetic overflow if Y is negative.
to_power:
  RST $28                 ; X, Y
  DEFB $01                ; exchange: Y, X
  DEFB $31                ; duplicate: Y, X, X
  DEFB $30                ; f_not: Y, X, (1/0)
  DEFB $00                ; jump_true to XIS0: Y, X
  DEFB $07                ;
; The jump is made if X=0, otherwise EXP (Y*LN X) is formed.
  DEFB $25                ; ln: Y, LN X
; Giving report A if X is negative.
  DEFB $04                ; multiply: Y*LN X
  DEFB $38                ; end_calc
  JP exp                  ; Exit via exp to form EXP (Y*LN X).
; The value of X is zero so consider the three possible cases involved.
XIS0:
  DEFB $02                ; delete: Y
  DEFB $31                ; duplicate: Y, Y
  DEFB $30                ; f_not: Y, (1/0)
  DEFB $00                ; jump_true to ONE: Y
  DEFB $09                ;
; The jump is made if X=0 and Y=0, otherwise proceed.
  DEFB $A0                ; stk_zero: Y, 0
  DEFB $01                ; exchange: 0, Y
  DEFB $37                ; greater_0: 0, (1/0)
  DEFB $00                ; jump_true to LAST: 0
  DEFB $06                ;
; The jump is made if X=0 and Y is positive, otherwise proceed.
  DEFB $A1                ; stk_one: 0, 1
  DEFB $01                ; exchange: 1, 0
  DEFB $05                ; division: Exit via division as dividing by zero gives 'arithmetic overflow'.
; The result is to be 1 for the operation.
ONE:
  DEFB $02                ; delete: -
  DEFB $A1                ; stk_one: 1
; Now return with the 'last value' on the stack being 0**Y.
LAST:
  DEFB $38                ; end_calc: (1/0)
  RET                     ; Finished: 'last value' is 0 or 1.

; Unused
  DEFS $0492,$FF          ; These locations are 'spare'. They all hold +FF.

; Character set
;
; Used by the routines at PO_ANY and NEW.
;
; These locations hold the 'character set'. There are 8-byte representations for all the characters with codes +20 (space) to +7F (©).
CHARSET:
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00010000          ;
  DEFB %00010000          ;
  DEFB %00010000          ;
  DEFB %00010000          ;
  DEFB %00000000          ;
  DEFB %00010000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00100100          ;
  DEFB %00100100          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00100100          ;
  DEFB %01111110          ;
  DEFB %00100100          ;
  DEFB %00100100          ;
  DEFB %01111110          ;
  DEFB %00100100          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00001000          ;
  DEFB %00111110          ;
  DEFB %00101000          ;
  DEFB %00111110          ;
  DEFB %00001010          ;
  DEFB %00111110          ;
  DEFB %00001000          ;
  DEFB %00000000          ;
  DEFB %01100010          ;
  DEFB %01100100          ;
  DEFB %00001000          ;
  DEFB %00010000          ;
  DEFB %00100110          ;
  DEFB %01000110          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00010000          ;
  DEFB %00101000          ;
  DEFB %00010000          ;
  DEFB %00101010          ;
  DEFB %01000100          ;
  DEFB %00111010          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00001000          ;
  DEFB %00010000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00000100          ;
  DEFB %00001000          ;
  DEFB %00001000          ;
  DEFB %00001000          ;
  DEFB %00001000          ;
  DEFB %00000100          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00100000          ;
  DEFB %00010000          ;
  DEFB %00010000          ;
  DEFB %00010000          ;
  DEFB %00010000          ;
  DEFB %00100000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00010100          ;
  DEFB %00001000          ;
  DEFB %00111110          ;
  DEFB %00001000          ;
  DEFB %00010100          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00001000          ;
  DEFB %00001000          ;
  DEFB %00111110          ;
  DEFB %00001000          ;
  DEFB %00001000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00001000          ;
  DEFB %00001000          ;
  DEFB %00010000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00111110          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00011000          ;
  DEFB %00011000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00000010          ;
  DEFB %00000100          ;
  DEFB %00001000          ;
  DEFB %00010000          ;
  DEFB %00100000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00111100          ;
  DEFB %01000110          ;
  DEFB %01001010          ;
  DEFB %01010010          ;
  DEFB %01100010          ;
  DEFB %00111100          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00011000          ;
  DEFB %00101000          ;
  DEFB %00001000          ;
  DEFB %00001000          ;
  DEFB %00001000          ;
  DEFB %00111110          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00111100          ;
  DEFB %01000010          ;
  DEFB %00000010          ;
  DEFB %00111100          ;
  DEFB %01000000          ;
  DEFB %01111110          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00111100          ;
  DEFB %01000010          ;
  DEFB %00001100          ;
  DEFB %00000010          ;
  DEFB %01000010          ;
  DEFB %00111100          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00001000          ;
  DEFB %00011000          ;
  DEFB %00101000          ;
  DEFB %01001000          ;
  DEFB %01111110          ;
  DEFB %00001000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %01111110          ;
  DEFB %01000000          ;
  DEFB %01111100          ;
  DEFB %00000010          ;
  DEFB %01000010          ;
  DEFB %00111100          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00111100          ;
  DEFB %01000000          ;
  DEFB %01111100          ;
  DEFB %01000010          ;
  DEFB %01000010          ;
  DEFB %00111100          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %01111110          ;
  DEFB %00000010          ;
  DEFB %00000100          ;
  DEFB %00001000          ;
  DEFB %00010000          ;
  DEFB %00010000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00111100          ;
  DEFB %01000010          ;
  DEFB %00111100          ;
  DEFB %01000010          ;
  DEFB %01000010          ;
  DEFB %00111100          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00111100          ;
  DEFB %01000010          ;
  DEFB %01000010          ;
  DEFB %00111110          ;
  DEFB %00000010          ;
  DEFB %00111100          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00010000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00010000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00010000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00010000          ;
  DEFB %00010000          ;
  DEFB %00100000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00000100          ;
  DEFB %00001000          ;
  DEFB %00010000          ;
  DEFB %00001000          ;
  DEFB %00000100          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00111110          ;
  DEFB %00000000          ;
  DEFB %00111110          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00010000          ;
  DEFB %00001000          ;
  DEFB %00000100          ;
  DEFB %00001000          ;
  DEFB %00010000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00111100          ;
  DEFB %01000010          ;
  DEFB %00000100          ;
  DEFB %00001000          ;
  DEFB %00000000          ;
  DEFB %00001000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00111100          ;
  DEFB %01001010          ;
  DEFB %01010110          ;
  DEFB %01011110          ;
  DEFB %01000000          ;
  DEFB %00111100          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00111100          ;
  DEFB %01000010          ;
  DEFB %01000010          ;
  DEFB %01111110          ;
  DEFB %01000010          ;
  DEFB %01000010          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %01111100          ;
  DEFB %01000010          ;
  DEFB %01111100          ;
  DEFB %01000010          ;
  DEFB %01000010          ;
  DEFB %01111100          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00111100          ;
  DEFB %01000010          ;
  DEFB %01000000          ;
  DEFB %01000000          ;
  DEFB %01000010          ;
  DEFB %00111100          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %01111000          ;
  DEFB %01000100          ;
  DEFB %01000010          ;
  DEFB %01000010          ;
  DEFB %01000100          ;
  DEFB %01111000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %01111110          ;
  DEFB %01000000          ;
  DEFB %01111100          ;
  DEFB %01000000          ;
  DEFB %01000000          ;
  DEFB %01111110          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %01111110          ;
  DEFB %01000000          ;
  DEFB %01111100          ;
  DEFB %01000000          ;
  DEFB %01000000          ;
  DEFB %01000000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00111100          ;
  DEFB %01000010          ;
  DEFB %01000000          ;
  DEFB %01001110          ;
  DEFB %01000010          ;
  DEFB %00111100          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %01000010          ;
  DEFB %01000010          ;
  DEFB %01111110          ;
  DEFB %01000010          ;
  DEFB %01000010          ;
  DEFB %01000010          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00111110          ;
  DEFB %00001000          ;
  DEFB %00001000          ;
  DEFB %00001000          ;
  DEFB %00001000          ;
  DEFB %00111110          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00000010          ;
  DEFB %00000010          ;
  DEFB %00000010          ;
  DEFB %01000010          ;
  DEFB %01000010          ;
  DEFB %00111100          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %01000100          ;
  DEFB %01001000          ;
  DEFB %01110000          ;
  DEFB %01001000          ;
  DEFB %01000100          ;
  DEFB %01000010          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %01000000          ;
  DEFB %01000000          ;
  DEFB %01000000          ;
  DEFB %01000000          ;
  DEFB %01000000          ;
  DEFB %01111110          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %01000010          ;
  DEFB %01100110          ;
  DEFB %01011010          ;
  DEFB %01000010          ;
  DEFB %01000010          ;
  DEFB %01000010          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %01000010          ;
  DEFB %01100010          ;
  DEFB %01010010          ;
  DEFB %01001010          ;
  DEFB %01000110          ;
  DEFB %01000010          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00111100          ;
  DEFB %01000010          ;
  DEFB %01000010          ;
  DEFB %01000010          ;
  DEFB %01000010          ;
  DEFB %00111100          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %01111100          ;
  DEFB %01000010          ;
  DEFB %01000010          ;
  DEFB %01111100          ;
  DEFB %01000000          ;
  DEFB %01000000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00111100          ;
  DEFB %01000010          ;
  DEFB %01000010          ;
  DEFB %01010010          ;
  DEFB %01001010          ;
  DEFB %00111100          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %01111100          ;
  DEFB %01000010          ;
  DEFB %01000010          ;
  DEFB %01111100          ;
  DEFB %01000100          ;
  DEFB %01000010          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00111100          ;
  DEFB %01000000          ;
  DEFB %00111100          ;
  DEFB %00000010          ;
  DEFB %01000010          ;
  DEFB %00111100          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %11111110          ;
  DEFB %00010000          ;
  DEFB %00010000          ;
  DEFB %00010000          ;
  DEFB %00010000          ;
  DEFB %00010000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %01000010          ;
  DEFB %01000010          ;
  DEFB %01000010          ;
  DEFB %01000010          ;
  DEFB %01000010          ;
  DEFB %00111100          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %01000010          ;
  DEFB %01000010          ;
  DEFB %01000010          ;
  DEFB %01000010          ;
  DEFB %00100100          ;
  DEFB %00011000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %01000010          ;
  DEFB %01000010          ;
  DEFB %01000010          ;
  DEFB %01000010          ;
  DEFB %01011010          ;
  DEFB %00100100          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %01000010          ;
  DEFB %00100100          ;
  DEFB %00011000          ;
  DEFB %00011000          ;
  DEFB %00100100          ;
  DEFB %01000010          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %10000010          ;
  DEFB %01000100          ;
  DEFB %00101000          ;
  DEFB %00010000          ;
  DEFB %00010000          ;
  DEFB %00010000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %01111110          ;
  DEFB %00000100          ;
  DEFB %00001000          ;
  DEFB %00010000          ;
  DEFB %00100000          ;
  DEFB %01111110          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00001110          ;
  DEFB %00001000          ;
  DEFB %00001000          ;
  DEFB %00001000          ;
  DEFB %00001000          ;
  DEFB %00001110          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %01000000          ;
  DEFB %00100000          ;
  DEFB %00010000          ;
  DEFB %00001000          ;
  DEFB %00000100          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %01110000          ;
  DEFB %00010000          ;
  DEFB %00010000          ;
  DEFB %00010000          ;
  DEFB %00010000          ;
  DEFB %01110000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00010000          ;
  DEFB %00111000          ;
  DEFB %01010100          ;
  DEFB %00010000          ;
  DEFB %00010000          ;
  DEFB %00010000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %11111111          ;
  DEFB %00000000          ;
  DEFB %00011100          ;
  DEFB %00100010          ;
  DEFB %01111000          ;
  DEFB %00100000          ;
  DEFB %00100000          ;
  DEFB %01111110          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00111000          ;
  DEFB %00000100          ;
  DEFB %00111100          ;
  DEFB %01000100          ;
  DEFB %00111100          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00100000          ;
  DEFB %00100000          ;
  DEFB %00111100          ;
  DEFB %00100010          ;
  DEFB %00100010          ;
  DEFB %00111100          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00011100          ;
  DEFB %00100000          ;
  DEFB %00100000          ;
  DEFB %00100000          ;
  DEFB %00011100          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00000100          ;
  DEFB %00000100          ;
  DEFB %00111100          ;
  DEFB %01000100          ;
  DEFB %01000100          ;
  DEFB %00111100          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00111000          ;
  DEFB %01000100          ;
  DEFB %01111000          ;
  DEFB %01000000          ;
  DEFB %00111100          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00001100          ;
  DEFB %00010000          ;
  DEFB %00011000          ;
  DEFB %00010000          ;
  DEFB %00010000          ;
  DEFB %00010000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00111100          ;
  DEFB %01000100          ;
  DEFB %01000100          ;
  DEFB %00111100          ;
  DEFB %00000100          ;
  DEFB %00111000          ;
  DEFB %00000000          ;
  DEFB %01000000          ;
  DEFB %01000000          ;
  DEFB %01111000          ;
  DEFB %01000100          ;
  DEFB %01000100          ;
  DEFB %01000100          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00010000          ;
  DEFB %00000000          ;
  DEFB %00110000          ;
  DEFB %00010000          ;
  DEFB %00010000          ;
  DEFB %00111000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00000100          ;
  DEFB %00000000          ;
  DEFB %00000100          ;
  DEFB %00000100          ;
  DEFB %00000100          ;
  DEFB %00100100          ;
  DEFB %00011000          ;
  DEFB %00000000          ;
  DEFB %00100000          ;
  DEFB %00101000          ;
  DEFB %00110000          ;
  DEFB %00110000          ;
  DEFB %00101000          ;
  DEFB %00100100          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00010000          ;
  DEFB %00010000          ;
  DEFB %00010000          ;
  DEFB %00010000          ;
  DEFB %00010000          ;
  DEFB %00001100          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %01101000          ;
  DEFB %01010100          ;
  DEFB %01010100          ;
  DEFB %01010100          ;
  DEFB %01010100          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %01111000          ;
  DEFB %01000100          ;
  DEFB %01000100          ;
  DEFB %01000100          ;
  DEFB %01000100          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00111000          ;
  DEFB %01000100          ;
  DEFB %01000100          ;
  DEFB %01000100          ;
  DEFB %00111000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %01111000          ;
  DEFB %01000100          ;
  DEFB %01000100          ;
  DEFB %01111000          ;
  DEFB %01000000          ;
  DEFB %01000000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00111100          ;
  DEFB %01000100          ;
  DEFB %01000100          ;
  DEFB %00111100          ;
  DEFB %00000100          ;
  DEFB %00000110          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00011100          ;
  DEFB %00100000          ;
  DEFB %00100000          ;
  DEFB %00100000          ;
  DEFB %00100000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00111000          ;
  DEFB %01000000          ;
  DEFB %00111000          ;
  DEFB %00000100          ;
  DEFB %01111000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00010000          ;
  DEFB %00111000          ;
  DEFB %00010000          ;
  DEFB %00010000          ;
  DEFB %00010000          ;
  DEFB %00001100          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %01000100          ;
  DEFB %01000100          ;
  DEFB %01000100          ;
  DEFB %01000100          ;
  DEFB %00111000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %01000100          ;
  DEFB %01000100          ;
  DEFB %00101000          ;
  DEFB %00101000          ;
  DEFB %00010000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %01000100          ;
  DEFB %01010100          ;
  DEFB %01010100          ;
  DEFB %01010100          ;
  DEFB %00101000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %01000100          ;
  DEFB %00101000          ;
  DEFB %00010000          ;
  DEFB %00101000          ;
  DEFB %01000100          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %01000100          ;
  DEFB %01000100          ;
  DEFB %01000100          ;
  DEFB %00111100          ;
  DEFB %00000100          ;
  DEFB %00111000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %01111100          ;
  DEFB %00001000          ;
  DEFB %00010000          ;
  DEFB %00100000          ;
  DEFB %01111100          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00001110          ;
  DEFB %00001000          ;
  DEFB %00110000          ;
  DEFB %00001000          ;
  DEFB %00001000          ;
  DEFB %00001110          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00001000          ;
  DEFB %00001000          ;
  DEFB %00001000          ;
  DEFB %00001000          ;
  DEFB %00001000          ;
  DEFB %00001000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %01110000          ;
  DEFB %00010000          ;
  DEFB %00001100          ;
  DEFB %00010000          ;
  DEFB %00010000          ;
  DEFB %01110000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00010100          ;
  DEFB %00101000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00000000          ;
  DEFB %00111100          ;
  DEFB %01000010          ;
  DEFB %10011001          ;
  DEFB %10100001          ;
  DEFB %10100001          ;
  DEFB %10011001          ;
  DEFB %01000010          ;
  DEFB %00111100          ;

