# Build context = ./EventPlatform (the backend solution root)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy everything and restore the specific project (keeps it simple and reliable).
COPY . .
RUN dotnet restore "src/Services/EventService/Presentation/EventService.WebAPI/EventService.WebAPI.csproj"
RUN dotnet publish "src/Services/EventService/Presentation/EventService.WebAPI/EventService.WebAPI.csproj" \
    -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "EventService.WebAPI.dll"]
