FROM mcr.microsoft.com/dotnet/sdk:8.0 AS restore
WORKDIR /src

COPY Bizcord.MicroServices.sln ./
COPY src/Bizcord.ChannelManagement.Api/Bizcord.ChannelManagement.Api.csproj src/Bizcord.ChannelManagement.Api/
COPY src/Bizcord.ChannelManagement.Application/Bizcord.ChannelManagement.Application.csproj src/Bizcord.ChannelManagement.Application/
COPY src/Bizcord.ChannelManagement.Contracts/Bizcord.ChannelManagement.Contracts.csproj src/Bizcord.ChannelManagement.Contracts/
COPY src/Bizcord.ChannelManagement.Domain/Bizcord.ChannelManagement.Domain.csproj src/Bizcord.ChannelManagement.Domain/
COPY src/Bizcord.ChannelManagement.Infrastructure/Bizcord.ChannelManagement.Infrastructure.csproj src/Bizcord.ChannelManagement.Infrastructure/

RUN dotnet restore src/Bizcord.ChannelManagement.Api/Bizcord.ChannelManagement.Api.csproj

FROM restore AS publish
COPY src ./src
RUN dotnet publish src/Bizcord.ChannelManagement.Api/Bizcord.ChannelManagement.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=publish /app/publish ./

USER app
ENTRYPOINT ["dotnet", "Bizcord.ChannelManagement.Api.dll"]