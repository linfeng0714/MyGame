@echo --------------------------------------------------
@echo svn update to version %1 %2
@echo --------------------------------------------------
@echo off

set localPath=%1
set version=%2
set username=%3
set password=%4
for %%a in (%*) do set /a num+=1


if "%num%"=="2" goto un_need_user
if "%num%"=="4" goto need_user
exit

:un_need_user
svn update %localPath% -r %version% 
exit

:need_user
svn update %localPath% -r %version% --username=%username% --password=%password% --no-auth-cache
exit
