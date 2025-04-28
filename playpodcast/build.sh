#
# build for Windows and Linux
#

dotnet publish -c Release -r linux-x64 --self-contained -p:PublishReadyToRun=true
dotnet publish -c Release -r win-x64 --self-contained -p:PublishReadyToRun=true
