@echo off
echo UPDATE LOCAL PACKAGES
set src=C:\Projects\_NuGet\

del .\local-packages\*.nupkg
xcopy %src%diffmatchpatch\1.0.1\*.nupkg .\local-packages\ /y

xcopy %src%fusi.tools\1.1.14\*.nupkg .\local-packages\ /y
xcopy %src%fusi.tools.config\1.0.14\*.nupkg .\local-packages\ /y
xcopy %src%fusi.text\1.1.11\*.nupkg .\local-packages\ /y
xcopy %src%fusi.antiquity\1.1.24\*.nupkg .\local-packages\ /y

xcopy %src%fusi.microsoft.extensions.configuration.inmemoryjson\1.0.1\*.nupkg .\local-packages\ /y

xcopy %src%messagingapi\1.0.1\*.nupkg .\local-packages\ /y

xcopy %src%cadmus.core\2.2.39\*.nupkg .\local-packages\ /y
xcopy %src%cadmus.index\1.0.15\*.nupkg .\local-packages\ /y
xcopy %src%cadmus.index.sql\1.0.36\*.nupkg .\local-packages\ /y
xcopy %src%cadmus.mongo\2.2.49\*.nupkg .\local-packages\ /y

xcopy %src%cadmus.parts\2.2.47\*.nupkg .\local-packages\ /y
xcopy %src%cadmus.philology.parts\2.2.53\*.nupkg .\local-packages\ /y

xcopy %src%cadmus.seed\1.0.42\*.nupkg .\local-packages\ /y
xcopy %src%cadmus.seed.parts\1.0.50\*.nupkg .\local-packages\ /y
xcopy %src%cadmus.seed.philology.parts\1.0.50\*.nupkg .\local-packages\ /y

xcopy %src%cadmus.archive.parts\2.2.40\*.nupkg .\local-packages\ /y
xcopy %src%cadmus.lexicon.parts\2.2.41\*.nupkg .\local-packages\ /y
pause
