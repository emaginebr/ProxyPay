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
COPY ["Ganesha.API/Ganesha.API.csproj", "Ganesha.API/"]
COPY ["Ganesha.GraphQL/Ganesha.GraphQL.csproj", "Ganesha.GraphQL/"]
COPY ["Ganesha.Application/Ganesha.Application.csproj", "Ganesha.Application/"]
COPY ["Ganesha.Domain/Ganesha.Domain.csproj", "Ganesha.Domain/"]
COPY ["Ganesha/Ganesha.csproj", "Ganesha/"]
COPY ["Ganesha.Infra.Interfaces/Ganesha.Infra.Interfaces.csproj", "Ganesha.Infra.Interfaces/"]
COPY ["Ganesha.Infra/Ganesha.Infra.csproj", "Ganesha.Infra/"]
RUN dotnet restore "Ganesha.API/Ganesha.API.csproj"
COPY . .
WORKDIR "/src/Ganesha.API"
RUN dotnet build "Ganesha.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Ganesha.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Ganesha.API.dll"]
