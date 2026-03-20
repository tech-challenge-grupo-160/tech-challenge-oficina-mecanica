FROM postgres:16

# Instala o pgAgent via gerenciador de pacotes do Debian
RUN apt-get update && apt-get install -y \
    pgagent \
    && rm -rf /var/lib/apt/lists/*

# Opcional: Script para habilitar a extensão no banco 'oficina_mecanica'
# Nota: O pgAgent precisa ser criado em cada banco onde será usado
COPY ./docker/postgres/init-db.sql /docker-entrypoint-initdb.d/
RUN echo "CREATE EXTENSION IF NOT EXISTS pgagent;" > /docker-entrypoint-initdb.d/00-pgagent.sql
