ARG BASE_IMAGE=mcr.microsoft.com/dotnet/aspnet:6.0-bullseye-slim-arm32v7
FROM ${BASE_IMAGE} AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/JoplinAsustorMediator/JoplinAsustorMediator.csproj", "src/JoplinAsustorMediator/"]
RUN dotnet restore "src/JoplinAsustorMediator/JoplinAsustorMediator.csproj"
COPY . .
WORKDIR "/src/src/JoplinAsustorMediator"
RUN dotnet build "JoplinAsustorMediator.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "JoplinAsustorMediator.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "JoplinAsustorMediator.dll"]