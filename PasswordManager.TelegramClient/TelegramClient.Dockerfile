FROM mcr.microsoft.com/dotnet/sdk:8.0.203-jammy AS build
WORKDIR /src
COPY ["PasswordManager.TelegramClient/PasswordManager.TelegramClient.csproj", "PasswordManager.TelegramClient/"]
RUN dotnet restore "PasswordManager.TelegramClient/PasswordManager.TelegramClient.csproj"
COPY . .
WORKDIR "/src/PasswordManager.TelegramClient"
RUN dotnet build "PasswordManager.TelegramClient.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PasswordManager.TelegramClient.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0.3-jammy AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PasswordManager.TelegramClient.dll"]
