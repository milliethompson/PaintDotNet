rem %1 = Debug or Release
if exist "bin\%1\RotateZoomSource.zip" del "bin\%1\RotateZoomSource.zip" > nul
tools\zip "bin/%1\RotateZoomSource.zip" Effects/RotateZoom/*.txt Effects/RotateZoom/*.rtf Effects/RotateZoom/*.cs Effects/RotateZoom/*.resx Effects/RotateZoom/*.csproj
