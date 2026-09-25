; ==============================================================================
; Finnish Key Helper - AutoHotkey Script (v1 and v2 compatible)
;
; Mappings:
;   Ctrl + Alt + ;         -> ä
;   Ctrl + Alt + Shift + ; -> Ä
;   Ctrl + Alt + '         -> ö
;   Ctrl + Alt + Shift + ' -> Ö
;
; Note: In Windows, AltGr is equivalent to Ctrl + Alt, so this also works
; with AltGr + ; and AltGr + '.
; ==============================================================================

#NoEnv
#SingleInstance force
SendMode Input
SetWorkingDir %A_ScriptDir%

; Ctrl + Alt + ; -> ä
^!';::Send, ä

; Ctrl + Alt + Shift + ; -> Ä
^+!';::Send, Ä

; Ctrl + Alt + ' -> ö
^!'::Send, ö

; Ctrl + Alt + Shift + ' -> Ö
^+!'::Send, Ö
