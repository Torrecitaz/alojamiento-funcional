$env:DOTNET_ROLL_FORWARD="LatestMajor"
$env:Microservices__UsuariosUrl="http://localhost:5001"
$env:Microservices__AlojamientosUrl="http://localhost:5002"
$env:Microservices__ReservasUrl="http://localhost:5003"
$env:Microservices__FacturacionUrl="http://localhost:5004"
${env:ReverseProxy__Clusters__usuarios-cluster__Destinations__destination1__Address}="http://localhost:5001"
${env:ReverseProxy__Clusters__alojamientos-cluster__Destinations__destination1__Address}="http://localhost:5002"
${env:ReverseProxy__Clusters__reservas-cluster__Destinations__destination1__Address}="http://localhost:5003"
${env:ReverseProxy__Clusters__facturacion-cluster__Destinations__destination1__Address}="http://localhost:5004"

# Crear directorio de logs
New-Item -ItemType Directory -Force -Path "logs" | Out-Null

Write-Host "Iniciando Usuarios.API..."
Start-Process dotnet -ArgumentList "run --project Microservices/Usuarios/Usuarios.API/Usuarios.API.csproj" -NoNewWindow -RedirectStandardOutput "logs/usuarios.log" -RedirectStandardError "logs/usuarios_error.log"

Write-Host "Iniciando Alojamientos.API..."
Start-Process dotnet -ArgumentList "run --project Microservices/Alojamientos/Alojamientos.API/Alojamientos.API.csproj" -NoNewWindow -RedirectStandardOutput "logs/alojamientos.log" -RedirectStandardError "logs/alojamientos_error.log"

Write-Host "Iniciando Reservas.API..."
Start-Process dotnet -ArgumentList "run --project Microservices/Reservas/Reservas.API/Reservas.API.csproj" -NoNewWindow -RedirectStandardOutput "logs/reservas.log" -RedirectStandardError "logs/reservas_error.log"

Write-Host "Iniciando Facturacion.API..."
Start-Process dotnet -ArgumentList "run --project Microservices/Facturacion/Facturacion.API/Facturacion.API.csproj" -NoNewWindow -RedirectStandardOutput "logs/facturacion.log" -RedirectStandardError "logs/facturacion_error.log"

Write-Host "Esperando 15 segundos para que los microservicios se compilen e inicien..."
Start-Sleep -Seconds 15

Write-Host "Iniciando ApiGateway..."
Start-Process dotnet -ArgumentList "run --project ApiGateway/ApiGateway/ApiGateway.csproj" -NoNewWindow -RedirectStandardOutput "logs/apigateway.log" -RedirectStandardError "logs/apigateway_error.log"

Write-Host "Todos los servicios iniciados en segundo plano."
