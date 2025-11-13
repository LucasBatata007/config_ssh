# Estágio 1: Build da API
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia e restaura dependências (incluindo o pacote do Datadog)
COPY ProductApi/ProductApi.csproj ./ProductApi/
RUN dotnet restore "./ProductApi/ProductApi.csproj"

# Copia o resto do código e publica
COPY . .
WORKDIR "/src/ProductApi"
RUN dotnet publish "ProductApi.csproj" -c Release -o /app/publish

# Estágio 2: Imagem final de runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Expõe a porta que o Kestrel vai usar DENTRO do container
ENV ASPNETCORE_URLS=http://*:8080
EXPOSE 8080

# Ponto de entrada
ENTRYPOINT ["dotnet", "ProductApi.dll"]