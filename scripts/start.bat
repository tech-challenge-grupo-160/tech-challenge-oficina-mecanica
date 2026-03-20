@echo off
REM Colors and styling
setlocal enabledelayedexpansion

echo.
echo ========== Sistema Integrado de Gestao de Oficina Mecanica ==========
echo Building Docker images...
echo.

REM Build and start containers
docker-compose up --build -d

if %ERRORLEVEL% EQU 0 (
    echo.
    echo [SUCCESS] Containers started successfully!
    echo Waiting for services to be ready...
    timeout /t 10 /nobreak
    
    echo.
    echo [SUCCESS] Services are ready!
    echo.
    echo ===== API Information =====
    echo URL: http://localhost:8080
    echo Swagger: http://localhost:8080/swagger
    echo.
    echo ===== Database Information =====
    echo Host: localhost
    echo Port: 5432
    echo Database: oficina_mecanica
    echo Username: postgres
    echo Password: postgres
    echo.
    echo ===== Useful commands =====
    echo View logs: docker-compose logs -f api
    echo Stop services: docker-compose down
    echo Remove volumes: docker-compose down -v
) else (
    echo.
    echo [ERROR] Failed to start containers!
    exit /b 1
)

endlocal
