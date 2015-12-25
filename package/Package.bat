mkdir lib
mkdir lib\net46
mkdir tools
mkdir content
mkdir content\controllers

copy ..\src\Ocelog\bin\Debug\Ocelog.dll lib\net46
copy ..\src\Ocelog.Formatting.Logstash\bin\Debug\Ocelog.Formatting.Logstash.dll lib\net46
copy ..\src\Ocelog.Transport.UDP\bin\Debug\Ocelog.Transport.UDP.dll lib\net46
copy ..\src\Ocelog.Testing\bin\Debug\Ocelog.Testing.dll lib\net46

nuget pack
