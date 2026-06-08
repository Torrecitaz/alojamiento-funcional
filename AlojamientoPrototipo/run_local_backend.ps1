$env:DOTNET_ROLL_FORWARD="LatestMajor"
$env:ASPNETCORE_ENVIRONMENT="Development"
$env:Microservices__UsuariosUrl="http://localhost:5001"
$env:Microservices__AlojamientosUrl="http://localhost:5002"
$env:Microservices__ReservasUrl="http://localhost:5003"
$env:Microservices__FacturacionUrl="http://localhost:5004"
${env:ReverseProxy__Clusters__usuarios-cluster__Destinations__destination1__Address}="http://localhost:5001"
${env:ReverseProxy__Clusters__alojamientos-cluster__Destinations__destination1__Address}="http://localhost:5002"
${env:ReverseProxy__Clusters__reservas-cluster__Destinations__destination1__Address}="http://localhost:5003"
${env:ReverseProxy__Clusters__facturacion-cluster__Destinations__destination1__Address}="http://localhost:5004"
$env:GrpcUrls__Alojamientos="http://localhost:5002"

# Crear directorio de logs
New-Item -ItemType Directory -Force -Path "logs" | Out-Null

Write-Host "Iniciando Usuarios.API..."
Start-Process dotnet -ArgumentList "--roll-forward LatestMajor bin/Debug/net8.0/Usuarios.API.dll --urls http://localhost:5001" -WorkingDirectory "Microservices/Usuarios/Usuarios.API" -NoNewWindow -RedirectStandardOutput "../../../logs/usuarios.log" -RedirectStandardError "../../../logs/usuarios_error.log"

Write-Host "Iniciando Alojamientos.API..."
Start-Process dotnet -ArgumentList "--roll-forward LatestMajor bin/Debug/net8.0/Alojamientos.API.dll --urls http://localhost:5002" -WorkingDirectory "Microservices/Alojamientos/Alojamientos.API" -NoNewWindow -RedirectStandardOutput "../../../logs/alojamientos.log" -RedirectStandardError "../../../logs/alojamientos_error.log"

Write-Host "Iniciando Reservas.API..."
Start-Process dotnet -ArgumentList "--roll-forward LatestMajor bin/Debug/net8.0/Reservas.API.dll --urls http://localhost:5003" -WorkingDirectory "Microservices/Reservas/Reservas.API" -NoNewWindow -RedirectStandardOutput "../../../logs/reservas.log" -RedirectStandardError "../../../logs/reservas_error.log"

Write-Host "Iniciando Facturacion.API..."
Start-Process dotnet -ArgumentList "--roll-forward LatestMajor bin/Debug/net8.0/Facturacion.API.dll --urls http://localhost:5004" -WorkingDirectory "Microservices/Facturacion/Facturacion.API" -NoNewWindow -RedirectStandardOutput "../../../logs/facturacion.log" -RedirectStandardError "../../../logs/facturacion_error.log"

Write-Host "Esperando 5 segundos para que los microservicios se inicien..."
Start-Sleep -Seconds 5

Write-Host "Iniciando ApiGateway..."
Start-Process dotnet -ArgumentList "--roll-forward LatestMajor bin/Debug/net8.0/ApiGateway.dll --urls http://localhost:5028" -WorkingDirectory "ApiGateway/ApiGateway" -NoNewWindow -RedirectStandardOutput "../../logs/apigateway.log" -RedirectStandardError "../../logs/apigateway_error.log"

Write-Host "Todos los servicios iniciados. Manteniendo proceso activo..."
Start-Sleep -Seconds 86400
