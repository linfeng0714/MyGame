@echo --------------------------------------------------
@echo svn export diff %1 %2 %3 %4
@echo --------------------------------------------------
@echo off

set remotepath=%~1
set base=%2
set target=%3
set localpath=%~4
set username=%5
set password=%6

if not "%username%" == "" (
	set username_token=--username=%username%
	set password_token=--password=%password%
)

setlocal enabledelayedexpansion
FOR /F "tokens=1,2" %%i IN ('svn diff %username_token% %password_token% --summarize -r %base%:%target% %remotepath%') DO (
    IF NOT %%i == D (		
		set line=%%j
		set filename=!line:%remotepath%=!
		set exportpath=%localpath%!filename!
		set exportdir=!exportpath:/=\!\..
		
		IF NOT EXIST !exportdir! (
			mkdir !exportdir!
		)		
        svn export %username_token% %password_token% -q --force !line!@%target% "!exportpath:%%20= !"
        echo !filename!
    )
)