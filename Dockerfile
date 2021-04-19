FROM mcr.microsoft.com/dotnet/aspnet:5.0
WORKDIR /source 

# copy csproj and restore as distinct layers
COPY *.sln .
COPY CDT.Cosmos.Cms.Website/*.csproj ./CDT.Cosmos.Cms.Website/
RUN dotnet restore "CDT.Cosmos.Cms.Website/*.csproj" 

# copy everything else and build app
COPY CDT.Cosmos.Cms.Website/. ./CDT.Cosmos.Cms.Website/
WORKDIR /source/CDT.Cosmos.Cms.Website
RUN dotnet build "CDT.Cosmos.Cms.Website.csproj" -c Release -o /app

FROM build AS publish  
RUN dotnet publish "CDT.Cosmos.Cms.Website.csproj" -c Release -o /app  

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:5.0
WORKDIR /app
COPY --from=build /app ./
EXPOSE 2222 8080 80
ENTRYPOINT ["dotnet", "CDT.Cosmos.Cms.Website.dll"]
