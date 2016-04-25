set apiKey=%1
Set nuget=".nuget\Nuget.exe"
%nuget% setApiKey %apiKey%

Rem Call :packnPublish "EventBuilder.Attributes"
Rem Call :packnPublish "EventBuilder.Events"
Rem Call :packnPublish "EventBuilder"
Rem Call :packnPublish "EventBuilder.Events.Consumers"
goto :eof

:packnPublish
SETLOCAL
Set _project=%1
PushD %_project%
..\%nuget% pack %_project%.csproj
..\%nuget% push *.nupkg
PopD
ENDLOCAL