#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
ENV ASPNETCORE_ENVIRONMENT=Docker
WORKDIR /app
EXPOSE 80

# Build libgdiplus with pango
RUN apt-get update && apt-get install  -y --allow-unauthenticated \
     libgif-dev autoconf libtool automake build-essential gettext libglib2.0-dev libcairo2-dev libtiff-dev libexif-dev
RUN apt-get update && apt-get install -y libpango1.0-dev

RUN apt-get install -y --allow-unauthenticated git
RUN git clone https://github.com/mono/libgdiplus.git /libgdiplus
WORKDIR /libgdiplus
RUN ./autogen.sh --with-pango --prefix=/usr
RUN make
RUN make install

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["ProxyPay.API/ProxyPay.API.csproj", "ProxyPay.API/"]
COPY ["ProxyPay.GraphQL/ProxyPay.GraphQL.csproj", "ProxyPay.GraphQL/"]
COPY ["ProxyPay.Application/ProxyPay.Application.csproj", "ProxyPay.Application/"]
COPY ["ProxyPay.Domain/ProxyPay.Domain.csproj", "ProxyPay.Domain/"]
COPY ["ProxyPay.DTO/ProxyPay.DTO.csproj", "ProxyPay.DTO/"]
COPY ["ProxyPay.Infra.Interfaces/ProxyPay.Infra.Interfaces.csproj", "ProxyPay.Infra.Interfaces/"]
COPY ["ProxyPay.Infra/ProxyPay.Infra.csproj", "ProxyPay.Infra/"]
RUN dotnet restore "ProxyPay.API/ProxyPay.API.csproj"
COPY . .
WORKDIR "/src/ProxyPay.API"
RUN dotnet build "ProxyPay.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ProxyPay.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ProxyPay.API.dll"]
