FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["BGD.CLINICAL.WebApi/BGD.CLINICAL.WebApi.csproj", "BGD.CLINICAL.WebApi/"]
COPY ["BGD.CLINICAL.Application/BGD.CLINICAL.Application.csproj", "BGD.CLINICAL.Application/"]
COPY ["BGD.CLINICAL.Domain/BGD.CLINICAL.Domain.csproj", "BGD.CLINICAL.Domain/"]
COPY ["BGD.CLINICAL.Infra.Data/BGD.CLINICAL.Infra.Data.csproj", "BGD.CLINICAL.Infra.Data/"]
COPY ["BGD.CLINICAL.Infra.ExternalApis/BGD.CLINICAL.Infra.ExternalApis.csproj", "BGD.CLINICAL.Infra.ExternalApis/"]

RUN dotnet restore "BGD.CLINICAL.WebApi/BGD.CLINICAL.WebApi.csproj"

COPY . .

RUN dotnet publish "BGD.CLINICAL.WebApi/BGD.CLINICAL.WebApi.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "BGD.CLINICAL.WebApi.dll"]