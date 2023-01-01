FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["RPlay/RPlay.csproj", "RPlay/"]
RUN dotnet restore "RPlay/RPlay.csproj"
COPY . .
WORKDIR "/src/RPlay"
RUN dotnet build "RPlay.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RPlay.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RPlay.dll"]
