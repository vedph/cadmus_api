@echo off
echo UPDATE LOCAL PACKAGES
set src=C:\Projects\_NuGet\

del .\local-packages\*.nupkg
xcopy %src%diffmatchpatch\1.0.1\*.nupkg .\local-packages\ /y

xcopy %src%fusi.tools\1.1.15\*.nupkg .\local-packages\ /y
xcopy %src%fusi.tools.config\1.0.16\*.nupkg .\local-packages\ /y
xcopy %src%fusi.text\1.1.11\*.nupkg .\local-packages\ /y
xcopy %src%fusi.antiquity\1.1.25\*.nupkg .\local-packages\ /y

xcopy %src%fusi.microsoft.extensions.configuration.inmemoryjson\1.0.1\*.nupkg .\local-packages\ /y

xcopy %src%messagingapi\1.0.1\*.nupkg .\local-packages\ /y

xcopy %src%cadmus.core\2.2.43\*.nupkg .\local-packages\ /y
xcopy %src%cadmus.index\1.0.19\*.nupkg .\local-packages\ /y
xcopy %src%cadmus.index.sql\1.0.43\*.nupkg .\local-packages\ /y
xcopy %src%cadmus.mongo\2.2.55\*.nupkg .\local-packages\ /y

xcopy %src%cadmus.parts\2.2.52\*.nupkg .\local-packages\ /y
xcopy %src%cadmus.philology.parts\2.2.58\*.nupkg .\local-packages\ /y

xcopy %src%cadmus.seed\1.0.47\*.nupkg .\local-packages\ /y
xcopy %src%cadmus.seed.parts\1.0.55\*.nupkg .\local-packages\ /y
xcopy %src%cadmus.seed.philology.parts\1.0.55\*.nupkg .\local-packages\ /y

xcopy %src%cadmus.archive.parts\2.2.45\*.nupkg .\local-packages\ /y
xcopy %src%cadmus.lexicon.parts\2.2.46\*.nupkg .\local-packages\ /y
pause
