FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS build-env
WORKDIR /source 
RUN sudo apt -y install dotnet-sdk-5.0

# copy csproj and restore as distinct layers
COPY *.sln .
COPY CDT.Cosmos.Cms.Website/*.csproj ./CDT.Cosmos.Cms.Website/
RUN dotnet restore

# copy everything else and build app
COPY CDT.Cosmos.Cms.Website/. ./CDT.Cosmos.Cms.Website/
WORKDIR /source/CDT.Cosmos.Cms.Website

RUN dotnet publish "CDT.Cosmos.Cms.Website.csproj" --output /app/ --configuration Release 

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:5.0
WORKDIR /app
COPY --from=build /app ./
EXPOSE 2222 8080 80
ENTRYPOINT ["dotnet", "CDT.Cosmos.Cms.Website.dll"]
