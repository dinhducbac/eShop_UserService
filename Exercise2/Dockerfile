#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.
# Add a new comment to trigger build.
# basic nginx dockerfile starting with Ubuntu 20.04
FROM ubuntu:20.04
RUN apt-get -y update
RUN apt-get -y install nginx
FROM mcr.microsoft.com/dotnet/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src
COPY ["eShopUserService.csproj", "Exercise2/"]
RUN dotnet restore "Exercise2/eShopUserService.csproj"
COPY . .
RUN dotnet build "eShopUserService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "eShopUserService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "eShopUserService.dll"]