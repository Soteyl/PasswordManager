FROM mcr.microsoft.com/dotnet/aspnet:8.0.3-jammy AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0.203-jammy AS build
WORKDIR /src
COPY ["PasswordStorageService/PasswordStorageService.csproj", "PasswordStorageService/"]
RUN dotnet restore "PasswordStorageService/PasswordStorageService.csproj"
COPY . .
WORKDIR "/src/PasswordStorageService"
RUN dotnet build "PasswordStorageService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PasswordStorageService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PasswordStorageService.dll"]
