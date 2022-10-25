dotnet publish Flow.Launcher.Plugin.RevitAPISearch -c Release -r win-x64
Compress-Archive -LiteralPath Flow.Launcher.Plugin.RevitAPISearch/bin/Release/win-x64/publish -DestinationPath Flow.Launcher.Plugin.RevitAPISearch/bin/RevitAPISearch.zip -Force
