# AutoDispatch.Web — runtime-only образ (копия AutoCall V3 Deploy/AutoCall.Web/Dockerfile). Сборка на машине разработчика:
# Scripts/publish-autodispatch-web.ps1 кладёт результат dotnet publish в ./publish и собирает образ через docker context ububuilder.
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

# volume: логи-файлы (резерв к БД логов), ключи Data Protection (cookie переживают пересоздание контейнера),
# загрузки /Download (ClickOnce AutoDispatch App), документы dispatch /files (V1 FilePath, ~150 ГБ — AI/wiki/deploy.md)
RUN mkdir -p /logs /keys /download /files
VOLUME ["/logs", "/keys", "/download", "/files"]

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY publish/ ./

ENTRYPOINT ["dotnet", "AutoDispatch.Web.Server.dll"]
