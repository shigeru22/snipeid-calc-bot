# build environment stage
FROM mcr.microsoft.com/dotnet/sdk:6.0-bullseye-slim AS build-env
WORKDIR /App

# copy development files, restore, and publish build
COPY . ./
RUN dotnet restore
RUN dotnet publish -c Release -o out

# create runtime image
FROM mcr.microsoft.com/dotnet/runtime:6.0-bullseye-slim
WORKDIR /App
COPY --from=build-env /App/out/ .
ENTRYPOINT ["dotnet", "LeaderpointsBot.Client.dll"]
