@echo off
dotnet tool restore
dotnet bare ./DM7_PPLUS_Integration/Implementierung/message_schema.bare ./DM7_PPLUS_Integration/Implementierung/Messages.cs -lt list -ns DM7_PPLUS_Integration.Messages