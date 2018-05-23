@echo off
echo UPDATE PLUGINS

set target1=CadmusApi\bin\Release\netcoreapp2.0\Plugins\
set target2=CadmusTool\bin\Release\netcoreapp2.0\Plugins\

md %target1%
del %target1%*.* /q

md %target2%
del %target2%*.* /q

xcopy ..\Cadmus\Cadmus.Parts\bin\Debug\netstandard2.0\*.dll %target1% /y
xcopy ..\Cadmus\Cadmus.Lexicon.Parts\bin\Debug\netstandard2.0\*.dll %target1% /y
xcopy ..\Cadmus\Cadmus.Philology.Parts\bin\Debug\netstandard2.0\*.dll %target1% /y

xcopy ..\Cadmus\Cadmus.Parts\bin\Debug\netstandard2.0\*.dll %target2% /y
xcopy ..\Cadmus\Cadmus.Lexicon.Parts\bin\Debug\netstandard2.0\*.dll %target2% /y
xcopy ..\Cadmus\Cadmus.Philology.Parts\bin\Debug\netstandard2.0\*.dll %target2% /y

pause