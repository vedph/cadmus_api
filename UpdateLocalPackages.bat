@echo off
echo UPDATE LOCAL PACKAGES
set src=C:\Projects\_NuGet\

del .\local-packages\*.nupkg
xcopy %src%diffmatchpatch\1.0.1\*.nupkg .\local-packages\ /y

xcopy %src%fusi.tools\1.1.13\*.nupkg .\local-packages\ /y
xcopy %src%fusi.tools.config\1.0.12\*.nupkg .\local-packages\ /y
xcopy %src%fusi.text\1.1.10\*.nupkg .\local-packages\ /y
xcopy %src%fusi.antiquity\1.1.23\*.nupkg .\local-packages\ /y

xcopy %src%fusi.microsoft.extensions.configuration.inmemoryjson\1.0.1\*.nupkg .\local-packages\ /y

xcopy %src%messagingapi\1.0.1\*.nupkg .\local-packages\ /y
xcopy %src%messagingapi.sendgrid\1.0.2\*.nupkg .\local-packages\ /y

xcopy %src%cadmus.core\2.2.35\*.nupkg .\local-packages\ /y
xcopy %src%cadmus.index\1.0.11\*.nupkg .\local-packages\ /y
xcopy %src%cadmus.index.sql\1.0.32\*.nupkg .\local-packages\ /y
xcopy %src%cadmus.mongo\2.2.45\*.nupkg .\local-packages\ /y

xcopy %src%cadmus.parts\2.2.43\*.nupkg .\local-packages\ /y
xcopy %src%cadmus.philology.parts\2.2.43\*.nupkg .\local-packages\ /y

xcopy %src%cadmus.seed\1.0.38\*.nupkg .\local-packages\ /y
xcopy %src%cadmus.seed.parts\1.0.43\*.nupkg .\local-packages\ /y
xcopy %src%cadmus.seed.philology.parts\1.0.40\*.nupkg .\local-packages\ /y

xcopy %src%cadmus.archive.parts\2.2.36\*.nupkg .\local-packages\ /y
xcopy %src%cadmus.lexicon.parts\2.2.37\*.nupkg .\local-packages\ /y
pause
