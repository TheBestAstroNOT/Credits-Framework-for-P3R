# Set Working Directory
Split-Path $MyInvocation.MyCommand.Path | Push-Location
[Environment]::CurrentDirectory = $PWD

Remove-Item "$env:RELOADEDIIMODS/p3rpc.credits.framework.reloaded/*" -Force -Recurse
dotnet publish "./p3rpc.credits.framework.reloaded.csproj" -c Release -o "$env:RELOADEDIIMODS/p3rpc.credits.framework.reloaded" /p:OutputPath="./bin/Release" /p:ReloadedILLink="true"

# Restore Working Directory
Pop-Location