@echo off
cls
dotnet pack DM7_PPLUS_Integration -o _pack -c Release
dotnet publish Stammdatenexport_Ueberpruefung -o _pack -c Release