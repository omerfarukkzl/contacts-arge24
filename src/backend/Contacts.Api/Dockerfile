FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/backend/Contacts.Api/Contacts.Api.csproj src/backend/Contacts.Api/
RUN dotnet restore src/backend/Contacts.Api/Contacts.Api.csproj

COPY src/backend/Contacts.Api/ src/backend/Contacts.Api/
RUN dotnet publish src/backend/Contacts.Api/Contacts.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 5050
ENTRYPOINT ["dotnet", "Contacts.Api.dll"]
