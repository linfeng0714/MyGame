@echo --------------------------------------------------
@echo svn checkout %1 %2
@echo --------------------------------------------------
@echo off

set svn_url=%1
set local_path=%2
set username=%3
set password=%4

for %%a in (%*) do set /a num+=1
if "%num%"=="2" goto un_need_user
if "%num%"=="4" goto need_user
exit

:need_user
svn checkout %svn_url% %local_path% --username=%username% --password=%password% --no-auth-cache
exit

:un_need_user
svn checkout %svn_url% %local_path%
exit