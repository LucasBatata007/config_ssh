# Estágio 1: Build da API
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia e restaura dependências
COPY ProductApi/ProductApi.csproj ./ProductApi/
RUN dotnet restore "./ProductApi/ProductApi.csproj"

# Copia o resto do código e publica
COPY . .
WORKDIR "/src/ProductApi"
RUN dotnet publish "ProductApi.csproj" -c Release -o /app/publish

# Estágio 2: Imagem final de runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# --- INSTALAÇÃO DO TRACER DATADOG ---
# A imagem aspnet:8.0 é baseada em Debian.
# Precisamos do curl para baixar o pacote e do dpkg (que já vem) para instalar.

### NOVO: Instala o curl e baixa o tracer .NET ###
RUN apt-get update && \
    apt-get install -y curl && \
    ### CORREÇÃO: Usando o link "latest" para um download mais robusto ###
    curl -Lo datadog-dotnet-tracer.deb https://github.com/DataDog/dd-trace-dotnet/releases/latest/download/datadog-dotnet-tracer_amd64.deb

### Instala o tracer e limpa o arquivo .deb ###
RUN dpkg -i datadog-dotnet-tracer.deb && \
    rm datadog-dotnet-tracer.deb
# --- FIM DA INSTALAÇÃO DO TRACER ---

COPY --from=build /app/publish .

# Expõe a porta que o Kestrel vai usar DENTRO do container
ENV ASPNETCORE_URLS=http://*:8080
EXPOSE 8080

# Ponto de entrada
ENTRYPOINT ["dotnet", "ProductApi.dll"]