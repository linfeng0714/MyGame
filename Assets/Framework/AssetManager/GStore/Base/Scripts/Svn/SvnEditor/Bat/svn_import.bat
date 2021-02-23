@echo --------------------------------------------------
@echo svn import %1 %2
@echo --------------------------------------------------
@echo off

set local_path=%1
set svn_url=%2
set message=%3
set username=%4
set password=%5

for %%a in (%*) do set /a num+=1
if "%num%"=="3" goto un_need_user
if "%num%"=="5" goto need_user
exit

:need_user
svn import %local_path% %svn_url% --username=%username% --password=%password% --no-auth-cache -m %message%  
exit

:un_need_user
svn import %local_path% %svn_url% -m %message%
exit
