@echo --------------------------------------------------
@echo svn revert all %1
@echo --------------------------------------------------
@echo off

set localPath=%1
set username=%2
set password=%3
for %%a in (%*) do set /a num+=1
if "%num%"=="1" goto un_need_user
if "%num%"=="3" goto need_user
exit

:need_user
svn revert -R %localPath% --username=%username% --password=%password% --no-auth-cache
for /f "usebackq tokens=2*" %%i in (`svn status %localPath% --username=%username% --password=%password% ^| findstr /r "^\?"`) do svn delete --force "%%i %%j"
exit

:un_need_user
svn revert -R %localPath%
for /f "usebackq tokens=2*" %%i in (`svn status %localPath% ^| findstr /r "^\?"`) do svn delete --force "%%i %%j"
exit
