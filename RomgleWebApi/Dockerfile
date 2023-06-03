FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["RomgleWebApi/RotmgleWebApi.csproj", "RomgleWebApi/"]
RUN dotnet restore "RomgleWebApi/RotmgleWebApi.csproj"
COPY . .
WORKDIR "/src/RomgleWebApi"
RUN dotnet build "RotmgleWebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RotmgleWebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY RomgleWebApi/Assets Assets
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "RotmgleWebApi.dll"]