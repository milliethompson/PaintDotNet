@rem echo off
if "%SIGNPDN%" == "1" (
    if exist "%PDNSPC%" (
        if exist "%PDNPVK%" (
            signcode -a sha1 -spc "%PDNSPC%" -v "%PDNPVK%" -n "Paint.NET v2.5" -i http://www.eecs.wsu.edu/paint.net -t http://timestamp.comodoca.com/authenticode "%1"
        )
    )
)

