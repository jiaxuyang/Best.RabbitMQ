dotnet restore Best.RabbitMQ.sln
"C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\amd64\MSBuild.exe" /t:pack /p:Configuration=Debug /p:IncludeSymbols=true;IncludeSource=true src\Best.RabbitMQ\Best.RabbitMQ.csproj

MKDIR pkg
MKDIR symbols

move /Y .\src\Best.RabbitMQ\bin\Debug\*.symbols.nupkg .\symbols\

move /Y .\src\Best.RabbitMQ\bin\Debug\*.nupkg .\pkg\