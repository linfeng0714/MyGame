@echo --------------------------------------------------
@echo svn checkout to version %1 %2 %3
@echo --------------------------------------------------
@echo off

set svn_url=%1
set local_path=%2
set version=%3
set username=%4
set password=%5

for %%a in (%*) do set /a num+=1
if "%num%"=="3" goto un_need_user
if "%num%"=="5" goto need_user
exit

:need_user
svn checkout %svn_url% %local_path% -r %version% --username=%username% --password=%password%  --no-auth-cache
exit

:un_need_user
svn checkout %svn_url% %local_path% -r %version%
exit
