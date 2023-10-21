@echo off

start /d ".\Services\Silo\bin\Debug\net7.0\" Silo.exe"
timeout /t 10 /nobreak

start /d ".\Services\Silo\bin\Debug\net7.0\" Silo.exe"
timeout /t 9 /nobreak
start /d ".\Services\Silo\bin\Debug\net7.0\" Silo.exe"
timeout /t 8 /nobreak
start /d ".\Services\Silo\bin\Debug\net7.0\" Silo.exe"
timeout /t 7 /nobreak
start /d ".\Services\Silo\bin\Debug\net7.0\" Silo.exe"
timeout /t 6 /nobreak
start /d ".\Services\Silo\bin\Debug\net7.0\" Silo.exe"
timeout /t 5 /nobreak