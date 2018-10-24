
$projects = ls -Recurse test\*.csproj | % FullName

$results = $projects | % { 
                            dotnet test $_ -c Debug | Write-Host
                            $? 
                        }

if($results -contains $false) {
    echo "Tests failed!"
    exit 1
}
