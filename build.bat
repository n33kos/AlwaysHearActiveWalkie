dotnet build %~dp0AlwaysHearWalkie.csproj
powershell -Command Copy-Item -Force -Path "%~dp0bin\Debug\netstandard2.1\AlwaysHearWalkie.dll" -Destination "%~dp0thunderstore\AlwaysHearWalkie.dll"
powershell -Command Copy-Item -Force -Path "%~dp0bin\Debug\netstandard2.1\AlwaysHearWalkie.dll" -Destination """E:\SteamLibrary\steamapps\common\Lethal Company\BepInEx\plugins\AlwaysHearWalkie.dll"""
Start-Process & "E:\SteamLibrary\steamapps\common\Lethal Company\Lethal Company.exe"