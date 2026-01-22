dotnet run --project src/Noobish.Demo &
app_pid=$!

# Trace the already running app; this avoids UI launch issues when tracing via "dotnet run".
dotnet trace collect --format speedscope --output trace.speedscope.json --process-id $app_pid
wait $app_pid
