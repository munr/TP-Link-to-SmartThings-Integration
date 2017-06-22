@echo off

goto check_permissions

:check_permissions

net session >nul 2>&1
if %errorLevel% == 0 (
	goto install_service
) else (
	goto bad_permissions
)

pause >nul


:install_service

echo Installing Service...
echo.
pushd .\TPLinkSTBridgeService\bin\Debug\
TPLinkSTBridgeService.exe install
popd
echo.
net start tplinkstbridge
goto end


:bad_permissions

echo Unable to install TPLinkToSmartThings Bridge Windows Service
echo Please run this batch file using administrator mode to install the service
goto end


:end

timeout /t:3
