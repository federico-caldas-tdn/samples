FROM microsoft/dotnet:2.2-runtime AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY ./nuget  ./nuget
COPY ./NuGet.Config ./
COPY Gateway ./
RUN dotnet restore "./Gateway/TeamDotNet.Api.Gateway/TeamDotNet.Api.Gateway.csproj"
COPY . .
WORKDIR "/src/Gateway/TeamDotNet.Api.Gateway"
RUN dotnet build "TeamDotNet.Api.Gateway.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "TeamDotNet.Api.Gateway.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "TeamDotNet.Api.Gateway.dll"]