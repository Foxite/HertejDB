FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine AS build
WORKDIR /src
COPY ["HertejDB.Common/HertejDB.Common.csproj", "HertejDB.Common/"]
COPY ["HertejDB.Server/HertejDB.Server.csproj", "HertejDB.Server/"]
RUN dotnet restore "HertejDB.Server/HertejDB.Server.csproj"
COPY . .
WORKDIR "/src/HertejDB.Server"
RUN dotnet build "HertejDB.Server.csproj" -c Release -o /app/build
RUN dotnet publish "HertejDB.Server.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:7.0-alpine AS final
WORKDIR /app
EXPOSE 80

COPY --from=build /app/publish .
CMD ["HertejDB.Server"]
