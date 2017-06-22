@echo off

cls

set msbuild="C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\msbuild.exe"

%msbuild% TPLinkSTBridgeService.sln /t:rebuild /p:configuration=release

echo.
echo.

timeout /t:3