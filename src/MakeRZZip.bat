rem %1 = Debug or Release
if exist "bin\%1\RotoZoomerSource.zip" del "bin\%1\RotoZoomerSource.zip" > nul
tools\zip "bin/%1\RotoZoomerSource.zip" Effects/RotoZoomer/*.txt Effects/RotoZoomer/*.rtf Effects/RotoZoomer/*.cs Effects/RotoZoomer/*.resx Effects/RotoZoomer/*.csproj
