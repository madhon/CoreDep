FROM mcr.microsoft.com/dotnet/aspnet:10.0-azurelinux3.0-distroless AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:10.0-azurelinux3.0 AS build
WORKDIR /src
COPY ["CoreDep.csproj", "CoreDep.csproj"]
RUN dotnet restore "CoreDep.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "CoreDep.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CoreDep.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CoreDep.dll"]
