FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY PassoCerto.Web.csproj ./
RUN dotnet restore PassoCerto.Web.csproj

COPY . .
RUN dotnet publish PassoCerto.Web.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

USER $APP_UID

ENTRYPOINT ["dotnet", "PassoCerto.Web.dll"]
