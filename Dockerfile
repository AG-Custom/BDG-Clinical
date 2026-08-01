FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["AG.CLINICAL.WebApi/AG.CLINICAL.WebApi.csproj", "AG.CLINICAL.WebApi/"]
COPY ["AG.CLINICAL.Application/AG.CLINICAL.Application.csproj", "AG.CLINICAL.Application/"]
COPY ["AG.CLINICAL.Domain/AG.CLINICAL.Domain.csproj", "AG.CLINICAL.Domain/"]
COPY ["AG.CLINICAL.Infra.Data/AG.CLINICAL.Infra.Data.csproj", "AG.CLINICAL.Infra.Data/"]
COPY ["AG.CLINICAL.Infra.ExternalApis/AG.CLINICAL.Infra.ExternalApis.csproj", "AG.CLINICAL.Infra.ExternalApis/"]

RUN dotnet restore "AG.CLINICAL.WebApi/AG.CLINICAL.WebApi.csproj"

COPY . .

RUN dotnet publish "AG.CLINICAL.WebApi/AG.CLINICAL.WebApi.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "AG.CLINICAL.WebApi.dll"]