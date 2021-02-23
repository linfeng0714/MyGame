@echo --------------------------------------------------
@echo svn commit add delete motify %1 %2
@echo --------------------------------------------------
@echo off

set localPath=%1
set commitMessage=%2
set username=%3
set password=%4
for %%a in (%*) do set /a num+=1
if "%num%"=="2" goto un_need_user
if "%num%"=="4" goto need_user
exit

:need_user
for /f "usebackq tokens=2*" %%i in (`svn status %localPath% --username=%username% --password=%password% --no-auth-cache ^| findstr /r "^\?"`) do svn add "%%i %%j" --username=%username% --password=%password% --no-auth-cache 
for /f "usebackq tokens=2*" %%i in (`svn status %localPath% --username=%username% --password=%password% --no-auth-cache ^| findstr /r "^\!"`) do svn delete "%%i %%j" --username=%username% --password=%password% --no-auth-cache 
svn commit %localPath% -m "%commitMessage%" --username=%username% --password=%password% --no-auth-cache
exit

:un_need_user
for /f "usebackq tokens=2*" %%i in (`svn status %localPath% ^| findstr /r "^\?"`) do svn add "%%i %%j"
for /f "usebackq tokens=2*" %%i in (`svn status %localPath% ^| findstr /r "^\!"`) do svn delete "%%i %%j"
svn commit -m "%commitMessage%" %localPath%
exit

