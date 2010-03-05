@echo off
call C:\PROGRA~2\MONO-2~1.3\bin\setmonopath.bat
cd /D C:\PROGRA~2\MONO-2~1.3\lib\xsp\test
xsp2 --root E:\Projects\cassinidev\CassiniDev.FixtureExamples.TestWeb --port 8090 --applications /:.
