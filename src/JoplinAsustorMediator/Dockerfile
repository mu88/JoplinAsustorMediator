ARG BASE_IMAGE=mcr.microsoft.com/dotnet/aspnet:5.0.5-buster-slim-arm32v7
FROM ${BASE_IMAGE} AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["src/JoplinAsustorMediator/JoplinAsustorMediator.csproj", "JoplinAsustorMediator/"]
RUN dotnet restore "src/JoplinAsustorMediator/JoplinAsustorMediator.csproj"
COPY . .
WORKDIR "/src/JoplinAsustorMediator"
RUN dotnet build "JoplinAsustorMediator.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "JoplinAsustorMediator.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "JoplinAsustorMediator.dll"]
