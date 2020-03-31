@echo off
echo UPDATE LOCAL PACKAGES
set src=C:\Projects\_NuGet\

del .\local-packages\*.nupkg
xcopy %src%diffmatchpatch\1.0.1\*.nupkg .\local-packages\ /y

xcopy %src%fusi.tools\1.1.12\*.nupkg .\local-packages\ /y
xcopy %src%fusi.tools.config\1.0.10\*.nupkg .\local-packages\ /y
xcopy %src%fusi.text\1.1.10\*.nupkg .\local-packages\ /y
xcopy %src%fusi.antiquity\1.1.22\*.nupkg .\local-packages\ /y

xcopy %src%messagingapi\1.0.1\*.nupkg .\local-packages\ /y
xcopy %src%messagingapi.sendgrid\1.0.1\*.nupkg .\local-packages\ /y

xcopy %src%cadmus.core\2.2.28\*.nupkg .\local-packages\ /y
xcopy %src%cadmus.mongo\2.2.35\*.nupkg .\local-packages\ /y

xcopy %src%cadmus.parts\2.2.34\*.nupkg .\local-packages\ /y
xcopy %src%cadmus.philology.parts\2.2.35\*.nupkg .\local-packages\ /y

xcopy %src%cadmus.seed\1.0.30\*.nupkg .\local-packages\ /y
xcopy %src%cadmus.seed.parts\1.0.34\*.nupkg .\local-packages\ /y
xcopy %src%cadmus.seed.philology.parts\1.0.32\*.nupkg .\local-packages\ /y

xcopy %src%cadmus.archive.parts\2.2.28\*.nupkg .\local-packages\ /y
xcopy %src%cadmus.lexicon.parts\2.2.29\*.nupkg .\local-packages\ /y
pause
