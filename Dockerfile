FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/Practica2.Api/Practica2.Api.csproj src/Practica2.Api/
RUN dotnet restore src/Practica2.Api/Practica2.Api.csproj


COPY src/ src/
RUN dotnet publish src/Practica2.Api/Practica2.Api.csproj -c Release -o /app/publish --no-restore


FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

COPY --from=build /app/publish .

USER $APP_UID

ENTRYPOINT ["dotnet", "Practica2.Api.dll"]
