#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["d-lama-service/d-lama-service.csproj", "d-lama-service/"]
RUN dotnet restore "d-lama-service/d-lama-service.csproj"
COPY . .
WORKDIR "/src/d-lama-service"
RUN dotnet build "d-lama-service.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "d-lama-service.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "d-lama-service.dll"]