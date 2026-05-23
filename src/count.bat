@rem Use this to count the number of lines of code in Paint.NET
@rem First number in the last row is the number of lines of code.
@rem This counts all our .cs, .c, .cpp, .h files and excludes the #ziplib (we didn't write it!)
dir /b /s *.cs *.c *.cpp *.h | find /V "SharpZipLib" > _list.txt
tools\wc @_list.txt
del _list.txt
pause
