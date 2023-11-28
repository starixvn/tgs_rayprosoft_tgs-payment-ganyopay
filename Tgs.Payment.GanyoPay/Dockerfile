#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
ARG BuildConfiguration=Release
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["Tgs.Payment.GanyoPay/Tgs.Payment.GanyoPay.csproj", "Tgs.Payment.GanyoPay/"]
COPY ["GanyoPay.Core/GanyoPay.Core.csproj", "GanyoPay.Core/"]
COPY ["GanyoPay.Client/GanyoPay.Client.csproj", "GanyoPay.Client/"]
RUN dotnet restore "./Tgs.Payment.GanyoPay/./Tgs.Payment.GanyoPay.csproj"
COPY . .
WORKDIR "/src/Tgs.Payment.GanyoPay"
RUN dotnet build "./Tgs.Payment.GanyoPay.csproj" -c $BuildConfiguration -o /app/build

FROM build AS publish
ARG BuildConfiguration=Release
RUN dotnet publish "./Tgs.Payment.GanyoPay.csproj" -c $BuildConfiguration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Tgs.Payment.GanyoPay.dll"]