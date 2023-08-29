#!/bin/sh

cd api
dotnet tool install --global dotnet-ef || echo "Didn't install dotnet-ef"
dotnet ef database update
