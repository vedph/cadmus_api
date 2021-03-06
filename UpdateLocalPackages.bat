@echo off
echo UPDATE LOCAL PACKAGES
set src=C:\Projects\_NuGet\

del .\local-packages\*.nupkg
xcopy %src%diffmatchpatch\1.0.1\*.nupkg .\local-packages\ /y

xcopy %src%fusi.tools\1.1.15\*.nupkg .\local-packages\ /y
xcopy %src%fusi.tools.config\1.0.17\*.nupkg .\local-packages\ /y
xcopy %src%fusi.text\1.1.12\*.nupkg .\local-packages\ /y
xcopy %src%fusi.antiquity\1.1.25\*.nupkg .\local-packages\ /y

xcopy %src%fusi.microsoft.extensions.configuration.inmemoryjson\1.0.2\*.nupkg .\local-packages\ /y

xcopy %src%messagingapi\1.0.1\*.nupkg .\local-packages\ /y

xcopy %src%cadmus.core\2.3.2\*.nupkg .\local-packages\ /y
xcopy %src%cadmus.index\1.1.3\*.nupkg .\local-packages\ /y
xcopy %src%cadmus.index.sql\1.1.5\*.nupkg .\local-packages\ /y
xcopy %src%cadmus.mongo\2.3.5\*.nupkg .\local-packages\ /y

xcopy %src%cadmus.parts\2.3.2\*.nupkg .\local-packages\ /y
xcopy %src%cadmus.philology.parts\2.3.3\*.nupkg .\local-packages\ /y

xcopy %src%cadmus.seed\1.1.3\*.nupkg .\local-packages\ /y
xcopy %src%cadmus.seed.parts\1.1.3\*.nupkg .\local-packages\ /y
xcopy %src%cadmus.seed.philology.parts\1.1.5\*.nupkg .\local-packages\ /y

xcopy %src%cadmus.archive.parts\2.3.2\*.nupkg .\local-packages\ /y
xcopy %src%cadmus.lexicon.parts\2.3.2\*.nupkg .\local-packages\ /y
pause
