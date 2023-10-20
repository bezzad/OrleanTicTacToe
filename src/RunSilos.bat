@echo off

start /d ".\Services\Silo\bin\Debug\net7.0\" Silo.exe --InstanceId 0"
timeout /t 10 /nobreak

start /d ".\Services\Silo\bin\Debug\net7.0\" Silo.exe --InstanceId 1"
timeout /t 9 /nobreak
start /d ".\Services\Silo\bin\Debug\net7.0\" Silo.exe --InstanceId 2"
timeout /t 8 /nobreak
start /d ".\Services\Silo\bin\Debug\net7.0\" Silo.exe --InstanceId 3"
timeout /t 7 /nobreak
start /d ".\Services\Silo\bin\Debug\net7.0\" Silo.exe --InstanceId 4"
timeout /t 6 /nobreak
start /d ".\Services\Silo\bin\Debug\net7.0\" Silo.exe --InstanceId 5"
timeout /t 5 /nobreak