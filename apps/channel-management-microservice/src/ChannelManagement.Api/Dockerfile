FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source
COPY ChannelManagement.Api.csproj ./
RUN dotnet restore ChannelManagement.Api.csproj
COPY . ./
RUN dotnet publish ChannelManagement.Api.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
COPY --from=build /app/publish ./
USER $APP_UID
ENTRYPOINT ["dotnet", "ChannelManagementService.dll"]