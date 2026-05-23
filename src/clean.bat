for %%f in (. CpuCount PdnLib Effects Effects\RotateZoom PdnLib PdnLib\Threading SystemLayer StylusReader Data Data\TgaFileType Setup) do (
    rmdir /s /q "%%f\bin"
    rmdir /s /q "%%f\obj"
    rmdir /s /q "%%f\Release"
    rmdir /s /q "%%f\Debug"
    rmdir /s /q "%%f\Release and Package"
)

del Help\PaintDotNet.chm

