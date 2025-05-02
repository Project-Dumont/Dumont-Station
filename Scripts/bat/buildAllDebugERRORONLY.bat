@echo off
cd ../../

git submodule update --init --recursive

:: Redireciona a saída do build e filtra só erros
dotnet build -c Debug 2>&1 | findstr /R /C:"error"

pause
