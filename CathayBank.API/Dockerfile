FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["CathayBank.API/CathayBank.API.csproj", "CathayBank.API/"]
RUN dotnet restore "CathayBank.API/CathayBank.API.csproj"

COPY . .
WORKDIR "/src/CathayBank.API"

RUN dotnet build "CathayBank.API.csproj" -c Release -o /app/build --no-restore

FROM build AS publish
RUN dotnet publish "CathayBank.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=publish /app/publish .

EXPOSE 8080
EXPOSE 8081

ENV ASPNETCORE_URLS=http://+:8080;https://+:8081

ENTRYPOINT ["dotnet", "CathayBank.API.dll"]