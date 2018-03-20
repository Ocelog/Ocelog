mkdir lib
mkdir lib\netstandard2.0
mkdir lib\net45
mkdir tools
mkdir content
mkdir content\controllers

copy ..\src\Ocelog\bin\Debug\netstandard2.0\Ocelog.dll lib\netstandard2.0
copy ..\src\Ocelog.Formatting.Json\bin\Debug\netstandard2.0\Ocelog.Formatting.Json.dll lib\netstandard2.0
copy ..\src\Ocelog.Formatting.Logstash\bin\Debug\netstandard2.0\Ocelog.Formatting.Logstash.dll lib\netstandard2.0
copy ..\src\Ocelog.Transport.UDP\bin\netstandard2.0\Ocelog.Transport.UDP.dll lib\netstandard2.0
copy ..\src\Ocelog.Testing\bin\netstandard2.0\Ocelog.Testing.dll lib\netstandard2.0

copy ..\src\Ocelog\bin\Debug\netstandard2.0\Ocelog.pdb lib\netstandard2.0
copy ..\src\Ocelog.Formatting.Json\bin\Debug\netstandard2.0\Ocelog.Formatting.Json.pdb lib\netstandard2.0
copy ..\src\Ocelog.Formatting.Logstash\bin\Debug\netstandard2.0\Ocelog.Formatting.Logstash.pdb lib\netstandard2.0
copy ..\src\Ocelog.Transport.UDP\bin\Debug\netstandard2.0\Ocelog.Transport.UDP.pdb lib\netstandard2.0
copy ..\src\Ocelog.Testing\bin\Debug\netstandard2.0\Ocelog.Testing.pdb lib\netstandard2.0

copy ..\src\Ocelog\bin\Debug\net45\Ocelog.dll lib\net45
copy ..\src\Ocelog.Formatting.Json\bin\Debug\net45\Ocelog.Formatting.Json.dll lib\net45
copy ..\src\Ocelog.Formatting.Logstash\bin\Debug\net45\Ocelog.Formatting.Logstash.dll lib\net45
copy ..\src\Ocelog.Transport.UDP\bin\Debug\net45\Ocelog.Transport.UDP.dll lib\net45
copy ..\src\Ocelog.Testing\bin\Debug\net45\Ocelog.Testing.dll lib\net45

copy ..\src\Ocelog\bin\Debug\net45\Ocelog.pdb lib\net45
copy ..\src\Ocelog.Formatting.Json\bin\Debug\net45\Ocelog.Formatting.Json.pdb lib\net45
copy ..\src\Ocelog.Formatting.Logstash\bin\Debug\net45\Ocelog.Formatting.Logstash.pdb lib\net45
copy ..\src\Ocelog.Transport.UDP\bin\Debug\net45\Ocelog.Transport.UDP.pdb lib\net45
copy ..\src\Ocelog.Testing\bin\Debug\net45\Ocelog.Testing.pdb lib\net45

nuget pack -Symbols
