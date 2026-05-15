FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Attendance_Management_System.csproj", "."]
RUN dotnet restore "Attendance_Management_System.csproj"
COPY . .
RUN dotnet build "Attendance_Management_System.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Attendance_Management_System.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Attendance_Management_System.dll"]