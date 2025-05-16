#!/bin/bash

echo "Running tests..."
dotnet test .\Test\Bachelor.Test\Bachelor.Test.csproj
if [ $? -ne 0 ]; then
    echo "Tests failed. App will not run."
    exit 1
fi

echo "Tests passed. Running app..."
dotnet run --project .\Bachelor.csproj