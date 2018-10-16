@echo off
echo UPDATE PLUGINS

set target=CadmusTool\bin\Debug\netcoreapp2.1\Plugins\

md %target%
del %target%*.* /q

xcopy ..\Cadmus2\Cadmus.Parts\bin\Debug\netstandard2.0\*.dll %target% /y
xcopy ..\Cadmus2\Cadmus.Lexicon.Parts\bin\Debug\netstandard2.0\*.dll %target% /y
xcopy ..\Cadmus2\Cadmus.Philology.Parts\bin\Debug\netstandard2.0\*.dll %target% /y

pause
