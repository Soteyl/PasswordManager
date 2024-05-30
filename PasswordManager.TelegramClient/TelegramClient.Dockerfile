FROM mcr.microsoft.com/dotnet/runtime:8.0.3-jammy AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0.203-jammy AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["PasswordManager.TelegramClient/PasswordManager.TelegramClient.csproj", "PasswordManager.TelegramClient/"]
RUN dotnet restore "PasswordManager.TelegramClient/PasswordManager.TelegramClient.csproj"
COPY . .
WORKDIR "/src/PasswordManager.TelegramClient"
RUN dotnet build "PasswordManager.TelegramClient.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "PasswordManager.TelegramClient.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PasswordManager.TelegramClient.dll"]
