$sourceDir = "C:\Users\MATHIAS\source\repos\Clientes"
$targetDir = "C:\Users\MATHIAS\source\repos\Clientes\AlojamientoPrototipo\Microservices\Usuarios"

function Copy-And-Replace {
    param(
        [string]$SourcePath,
        [string]$TargetPath,
        [hashtable]$Replacements
    )
    
    if (Test-Path $SourcePath) {
        $files = Get-ChildItem -Path $SourcePath -Filter *.cs -Recurse
        foreach ($file in $files) {
            $relativePath = $file.FullName.Substring($SourcePath.Length + 1)
            $destFile = Join-Path $TargetPath $relativePath
            $destDir = Split-Path $destFile -Parent
            
            if (-not (Test-Path $destDir)) {
                New-Item -ItemType Directory -Path $destDir | Out-Null
            }
            
            $content = Get-Content $file.FullName -Raw
            foreach ($key in $Replacements.Keys) {
                $content = $content.Replace($key, $Replacements[$key])
            }
            
            Set-Content -Path $destFile -Value $content -Encoding UTF8
            Write-Host "Copied and updated: $destFile"
        }
    } else {
        Write-Host "Source path not found: $SourcePath"
    }
}

$replacements = @{
    "Microservicio.Cliente.DatAccess" = "Usuarios.DataAccess"
    "Microservicio.Clientes.DataManagement" = "Usuarios.DataManagement"
    "Microservicio.Clientes.Business" = "Usuarios.Business"
    "Microservicio.Clientes.Api" = "Usuarios.API"
}

# DataAccess
Copy-And-Replace -SourcePath "$sourceDir\Microservicio.Cliente.DatAccess\Entities\Usuarios" -TargetPath "$targetDir\Usuarios.DataAccess\Entities" -Replacements $replacements
Copy-And-Replace -SourcePath "$sourceDir\Microservicio.Cliente.DatAccess\Contexts" -TargetPath "$targetDir\Usuarios.DataAccess\Contexts" -Replacements $replacements

# DataManagement (only Repositories/Interfaces for now to see)
Copy-And-Replace -SourcePath "$sourceDir\Microservicio.Clientes.DataManagement\Repositories" -TargetPath "$targetDir\Usuarios.DataManagement\Repositories" -Replacements $replacements
Copy-And-Replace -SourcePath "$sourceDir\Microservicio.Clientes.DataManagement\Interfaces" -TargetPath "$targetDir\Usuarios.DataManagement\Interfaces" -Replacements $replacements

# Business
Copy-And-Replace -SourcePath "$sourceDir\Microservicio.Clientes.Business\DTOs" -TargetPath "$targetDir\Usuarios.Business\DTOs" -Replacements $replacements
Copy-And-Replace -SourcePath "$sourceDir\Microservicio.Clientes.Business\Interfaces" -TargetPath "$targetDir\Usuarios.Business\Interfaces" -Replacements $replacements
Copy-And-Replace -SourcePath "$sourceDir\Microservicio.Clientes.Business\Services" -TargetPath "$targetDir\Usuarios.Business\Services" -Replacements $replacements

# API
Copy-And-Replace -SourcePath "$sourceDir\Microservicio.Clientes.Api\Controllers" -TargetPath "$targetDir\Usuarios.API\Controllers" -Replacements $replacements
