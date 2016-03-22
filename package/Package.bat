mkdir lib
mkdir lib\net46
mkdir tools
mkdir content
mkdir content\controllers

copy ..\src\Ocelog\bin\Debug-46\Ocelog.dll lib\net46
copy ..\src\Ocelog.Formatting.Json\bin\Debug-46\Ocelog.Formatting.Json.dll lib\net46
copy ..\src\Ocelog.Formatting.Logstash\bin\Debug-46\Ocelog.Formatting.Logstash.dll lib\net46
copy ..\src\Ocelog.Transport.UDP\bin\Debug-46\Ocelog.Transport.UDP.dll lib\net46
copy ..\src\Ocelog.Testing\bin\Debug-46\Ocelog.Testing.dll lib\net46

copy ..\src\Ocelog\bin\Debug-46\Ocelog.pdb lib\net46
copy ..\src\Ocelog.Formatting.Json\bin\Debug-46\Ocelog.Formatting.Json.pdb lib\net46
copy ..\src\Ocelog.Formatting.Logstash\bin\Debug-46\Ocelog.Formatting.Logstash.pdb lib\net46
copy ..\src\Ocelog.Transport.UDP\bin\Debug-46\Ocelog.Transport.UDP.pdb lib\net46
copy ..\src\Ocelog.Testing\bin\Debug-46\Ocelog.Testing.pdb lib\net46

nuget pack -Symbols
