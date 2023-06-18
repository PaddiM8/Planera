#!/bin/sh

cd api
dotnet tool install --global dotnet-ef || echo "Didn't install dotnet-ef"
dotnet ef migrations add Initial || echo "Didn't add migration"
dotnet ef database update
