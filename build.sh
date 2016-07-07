#!/bin/bash
dotnet restore
for path in src/*/project.json; do
    dirname="$(dirname "${path}")"
    dotnet build ${dirname} -f netstandard1.3 -c Release
done

for path in test/*/project.json; do
    dirname="$(dirname "${path}")"
    dotnet test ${dirname} 
done
