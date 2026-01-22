for /f "delims=" %%i in ('powershell -NoProfile -Command "$p = Start-Process dotnet -ArgumentList ''run'',''--project'',''src/Noobish.Demo'' -PassThru; $p.Id"') do set APP_PID=%%i
dotnet trace collect --format speedscope --output trace.speedscope.json --process-id %APP_PID%
