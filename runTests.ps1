(ls -Recurse test\*.csproj | % FullName) | ForEach-Object { dotnet test $_ -c Debug }
