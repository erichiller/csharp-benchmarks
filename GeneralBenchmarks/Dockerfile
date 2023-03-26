FROM mcr.microsoft.com/dotnet/sdk:7.0 AS base
WORKDIR /app

VOLUME /app/GeneralBenchmarks/BenchmarkDotNet.Artifacts/

COPY ["GeneralBenchmarks/GeneralBenchmarks.csproj", "GeneralBenchmarks/"]
COPY ["Common/Common.csproj", "Common/"]
RUN dotnet restore "GeneralBenchmarks/GeneralBenchmarks.csproj"
COPY . .
WORKDIR "/app/GeneralBenchmarks"
RUN #dotnet build "GeneralBenchmarks.csproj" -c Release -o /app/build

RUN #dotnet publish "GeneralBenchmarks.csproj" -c Release -o /app/publish

#FROM base AS final
#WORKDIR /app
#COPY --from=publish /app/publish .
#ENTRYPOINT ["dotnet", "GeneralBenchmarks.dll"]
ENTRYPOINT ["dotnet", "run", "-c", "Release", "--"]


