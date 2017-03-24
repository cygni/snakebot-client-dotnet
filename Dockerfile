FROM microsoft/dotnet:1.1.1-sdk
WORKDIR /app

# copy project.json and restore as distinct layers
COPY . .
RUN dotnet restore

# copy and build everything else
RUN dotnet build
# RUN dotnet test test/Cygni.Snake.Client.Tests/
ENTRYPOINT ["dotnet", "run", "-p", "src/Cygni.Snake.SampleBot"]
