set Drive=%~d0
set TempPath=%~p0
set FullPath=%Drive%%TempPath%
pushd "%FullPath%"
set SETUPEXECMD=start Setup.exe
if '%Drive%' == '\\' set SETUPEXECMD=Setup.exe
%SETUPEXECMD%
