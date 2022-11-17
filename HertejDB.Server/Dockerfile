FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["HertejDB.Server/HertejDB.Server.csproj", "HertejDB.Server/"]
RUN dotnet restore "HertejDB.Server/HertejDB.Server.csproj"
COPY . .
WORKDIR "/src/HertejDB.Server"
RUN dotnet build "HertejDB.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "HertejDB.Server.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HertejDB.Server.dll"]
