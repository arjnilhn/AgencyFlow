# Build aşaması
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app
COPY . .
RUN dotnet publish -c Release -o out

# Çalıştırma aşaması
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/out .
# Veritabanını build aşamasından runtime aşamasına kopyalıyoruz
COPY agencyflow.db .
ENTRYPOINT ["dotnet", "AgencyFlow.dll"]