@echo off
dotnet tool restore
dotnet bare ./DM7_PPLUS_Integration/Implementierung/PPLUS/message_schema.bare ./DM7_PPLUS_Integration/Implementierung/PPLUS/Messages.cs -lt list -ns DM7_PPLUS_Integration.Messages.PPLUS
dotnet bare ./DM7_PPLUS_Integration/Implementierung/DM7/message_schema.bare ./DM7_PPLUS_Integration/Implementierung/DM7/Messages.cs -lt list -ns DM7_PPLUS_Integration.Messages.DM7