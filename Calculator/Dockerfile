FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY Calculator.csproj ./
RUN dotnet restore Calculator.csproj
COPY . ./
RUN dotnet publish Calculator.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish ./
EXPOSE 5055
USER $APP_UID
ENTRYPOINT ["dotnet", "Calculator.dll"]
