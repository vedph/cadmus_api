@echo off
echo UPDATE PLUGINS

set target=CadmusTool\bin\Release\netcoreapp3.0\Plugins\

md %target%
del %target%*.* /q

xcopy ..\Cadmus2\Cadmus.Parts\bin\Release\netstandard2.0\*.dll %target% /y
xcopy ..\Cadmus2\Cadmus.Lexicon.Parts\bin\Release\netstandard2.0\*.dll %target% /y
xcopy ..\Cadmus2\Cadmus.Philology.Parts\bin\Release\netstandard2.0\*.dll %target% /y

pause
