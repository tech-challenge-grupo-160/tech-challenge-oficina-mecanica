FROM postgres:16

# Instala o pgAgent via gerenciador de pacotes do Debian
RUN apt-get update && apt-get install -y \
    pgagent \
    && rm -rf /var/lib/apt/lists/*
