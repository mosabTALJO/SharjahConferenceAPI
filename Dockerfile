# 1. Base runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080

# 2. Build image
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy the csproj and restore dependencies
COPY ["SharjahConferenceAPI/SharjahConferenceAPI.csproj", "SharjahConferenceAPI/"]
RUN dotnet restore "SharjahConferenceAPI/SharjahConferenceAPI.csproj"

# Copy the rest of the code and build
COPY . .
WORKDIR "/src/SharjahConferenceAPI"
RUN dotnet build "SharjahConferenceAPI.csproj" -c Release -o /app/build

# 3. Publish the application
FROM build AS publish
RUN dotnet publish "SharjahConferenceAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 4. Final production image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SharjahConferenceAPI.dll"]
