@rem Use this to count the number of lines of code.
@rem Must have Cygwin installed and in the path.
@rem This counts all our .cs, .c, .cpp, .h files and excludes the #ziplib (we didn't write it!)
dir /b /s *.cs *.c *.cpp *.h | find /V "SharpZipLib" > _list.txt
wc @_list.txt
del _list.txt
pause
