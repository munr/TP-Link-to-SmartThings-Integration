@echo off

goto check_permissions

:check_permissions

net session >nul 2>&1
if %errorLevel% == 0 (
	goto remove_service
) else (
	goto bad_permissions
)

pause >nul


:remove_service

echo Removing Service...
echo.
TPLinkSTBridgeService.exe uninstall
goto end


:bad_permissions

echo Unable to remove TPLinkToSmartThings Bridge Windows Service
echo Please run this batch file using administrator mode to remove the service
goto end


:end

timeout /t:3
