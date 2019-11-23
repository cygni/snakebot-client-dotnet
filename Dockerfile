FROM mcr.microsoft.com/dotnet/core/sdk:3.0
WORKDIR /app

# copy project.json and restore as distinct layers
COPY . .
RUN dotnet restore

# copy and build everything else
RUN dotnet build
RUN dotnet test Cygni.Snake.Client.Tests/
ENTRYPOINT ["dotnet", "run", "-p", "Cygni.Snake.SampleBot"]
