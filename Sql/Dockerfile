FROM mcr.microsoft.com/dotnet/sdk:7.0 AS base
#FROM mcr.microsoft.com/dotnet/sdk:latest AS base
WORKDIR /app

VOLUME /app/Sql/BenchmarkDotNet.Artifacts/

COPY ["Sql/Sql.csproj", "Sql/"]
COPY ["Common/Common.csproj", "Common/"]
RUN dotnet restore "Sql/Sql.csproj"
COPY . .
WORKDIR "/app/Sql"
#RUN dotnet build "Sql.csproj" -c Release -o /app/build

#FROM build AS publish
#RUN dotnet publish "Sql.csproj" -c Release -o /app/publish

ENV DOTNET_ENVIRONMENT=Release

#FROM base AS final
#WORKDIR /app
#COPY --from=build /src/Sql/Sql.csproj .
#COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "run", "-c", "Release", "--"]
CMD ["--list","tree"]


# ###### USE #######
# Build like:
# docker build --name csharp-benchmark-sql --file ./Sql/Dockerfile .
# Run like:
# docker run --rm -it csharp-benchmark-sql --anyCategories Reader