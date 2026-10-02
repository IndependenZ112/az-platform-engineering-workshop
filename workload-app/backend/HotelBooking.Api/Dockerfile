FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

ARG NUGET_SOURCE=https://api.nuget.org/v3/index.json

COPY HotelBooking.Api.csproj ./
RUN --mount=type=cache,target=/root/.nuget/packages \
    --mount=type=cache,target=/root/.local/share/NuGet/v3-cache \
    dotnet restore --source "$NUGET_SOURCE"

COPY . .
RUN --mount=type=cache,target=/root/.nuget/packages \
    --mount=type=cache,target=/root/.local/share/NuGet/v3-cache \
    dotnet publish --configuration Release --no-restore --output /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

COPY --from=build /app/publish .

USER $APP_UID
ENTRYPOINT ["dotnet", "HotelBooking.Api.dll"]
