#!/bin/bash
export COMPlus_INTERNAL_ThreadSuspendInjection=0
dotnet restore
dotnet build src/Cygni.Snake.SampleBot/project.json
dotnet test test/Cygni.Snake.Client.Tests/
