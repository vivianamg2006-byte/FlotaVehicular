# Bateria 1 — verificacion de las correcciones funcionales de FlotaVehicular.
# 35 checks: coherencia de mapeos de detalle, validacion de estado de vehiculo,
# validacion de contenido real de imagenes, reemplazo con compensacion y
# fallo simulado de guardado (trigger en la BD de prueba).
#
# Requisitos previos (ver README, seccion "Como reproducir la verificacion"):
#   1. BD independiente aplicada:  FlotaVehicular_RevVerify  (dotnet ef database update)
#   2. Archivos de prueba:         .\Nuevos-Archivos-Prueba.ps1
#   3. API en http://localhost:5137 apuntando a esa BD y a $storage.
# No modifica la BD FlotaVehicular ni el proyecto base ERP9.
$ErrorActionPreference = "Stop"
$api = "http://localhost:5137"
$files = Join-Path $PSScriptRoot "archivos-prueba"
$storage = Join-Path $env:TEMP "opencode\flota-imgs-verify"   # debe coincidir con Storage:VehicleImagesPath del API
$testDb = "FlotaVehicular_RevVerify"
$script:pass = 0; $script:fail = 0

function Check($name, $cond, $detail = "") {
    if ($cond) { $script:pass++; Write-Host "PASS  $name" }
    else { $script:fail++; Write-Host "FAIL  $name  $detail" }
}

function Invoke-Api($method, $url, $body = $null) {
    try {
        if ($null -ne $body) {
            $r = Invoke-WebRequest -Uri "$api$url" -Method $method -Body $body -ContentType "application/json" -UseBasicParsing
        } else {
            $r = Invoke-WebRequest -Uri "$api$url" -Method $method -UseBasicParsing
        }
        $data = $null
        try { $data = $r.Content | ConvertFrom-Json } catch { }
        return @{ status = $r.StatusCode; body = $data; raw = $r.Content }
    } catch {
        $resp = $_.Exception.Response
        $status = if ($resp) { [int]$resp.StatusCode } else { 0 }
        $raw = ""
        if ($resp) {
            try {
                $stream = $resp.GetResponseStream()
                $stream.Position = 0
                $sr = New-Object System.IO.StreamReader($stream)
                $raw = $sr.ReadToEnd()
            } catch { }
        }
        $data = $null
        try { $data = $raw | ConvertFrom-Json } catch { }
        return @{ status = $status; body = $data; raw = $raw }
    }
}

function Invoke-Multipart($method, $url, $filePath, $contentType, $formFields = @()) {
    $out = [System.IO.Path]::GetTempFileName()
    $args = @("-s", "-o", $out, "-w", "%{http_code}", "-X", $method, "$api$url",
              "-F", "file=@$filePath;type=$contentType")
    foreach ($f in $formFields) { $args += @("-F", $f) }
    $code = & curl.exe @args
    $raw = [System.IO.File]::ReadAllText($out)
    Remove-Item $out -Force
    $data = $null
    try { $data = $raw | ConvertFrom-Json } catch { }
    return @{ status = [int]$code; body = $data; raw = $raw }
}

# El DTO de imagen no expone la ruta interna: se resuelve desde el almacenamiento.
function DiskPath($img) { Join-Path $storage $img.fileName }

Write-Host "=== A. Coherencia de mapeos de detalle (Mapster) ==="
$r = Invoke-Api GET "/api/sparepart/1"
Check "GET sparepart/1 -> 200" ($r.status -eq 200) "status=$($r.status)"
$sp = $r.body.data
Check "sparepart/1 supplierCount coincide con suppliers.Count" ($sp.supplierCount -eq @($sp.suppliers).Count) "count=$($sp.supplierCount) list=$(@($sp.suppliers).Count)"
$missing = @($sp.suppliers) | Where-Object { $null -eq $_.offeredPrice -or $null -eq $_.estimatedDeliveryDays }
Check "cada supplier trae OfferedPrice y EstimatedDeliveryDays" ($missing.Count -eq 0) "faltan $($missing.Count)"

$r = Invoke-Api GET "/api/supplier/1"
Check "GET supplier/1 -> 200" ($r.status -eq 200) "status=$($r.status)"
$sup = $r.body.data
Check "supplier/1 sparePartCount coincide con spareParts.Count" ($sup.sparePartCount -eq @($sup.spareParts).Count) "count=$($sup.sparePartCount) list=$(@($sup.spareParts).Count)"
Check "supplier/1 tiene repuestos en la coleccion" (@($sup.spareParts).Count -gt 0) "vacio"

Write-Host ""
Write-Host "=== B. Validacion de estado del vehiculo ==="
$r = Invoke-Api PUT "/api/vehicle/1/status" '{"status":99}'
Check '{"status":99} -> 400 (entero rechazado en el binding JSON)' ($r.status -eq 400) "status=$($r.status)"
Check '{"status":99} respuesta en forma Result<T>}' ($null -ne $r.body.status -and $r.body.isSuccess -eq $false) $r.raw
$r = Invoke-Api PUT "/api/vehicle/1/status" '{"status":"NoExiste"}'
Check '{"status":"NoExiste"} -> 400' ($r.status -eq 400) "status=$($r.status)"
$r = Invoke-Api PUT "/api/vehicle/1/status" '{"status":"DadoDeBaja"}'
Check 'DadoDeBaja rechazado en ChangeStatus (exclusivo de DELETE)' ($r.status -eq 400 -and $r.body.detail -match "DELETE") "status=$($r.status) detail=$($r.body.detail)"
$r = Invoke-Api PUT "/api/vehicle/1/status" '{"status":"FueraDeServicio"}'
Check 'estado valido FueraDeServicio -> 200' ($r.status -eq 200) "status=$($r.status)"
$r2 = Invoke-Api GET "/api/vehicle/1"
Check 'vehiculo 1 quedo FueraDeServicio' ($r2.body.data.status -eq "FueraDeServicio") "status=$($r2.body.data.status)"
$r = Invoke-Api PUT "/api/vehicle/1/status" '{"status":"Operativo"}'
Check 'restaurado a Operativo -> 200' ($r.status -eq 200) "status=$($r.status)"
$r = Invoke-Api PUT "/api/vehicle/9999/status" '{"status":"Operativo"}'
Check 'vehiculo inexistente -> 404' ($r.status -eq 404) "status=$($r.status)"

Write-Host ""
Write-Host "=== C. Validacion de contenido real de imagenes ==="
$r = Invoke-Multipart POST "/api/vehicle/2/images" "$files\real.png" "image/png"
Check "PNG real -> 201" ($r.status -eq 201) "status=$($r.status) $($r.raw)"
$r = Invoke-Multipart POST "/api/vehicle/2/images" "$files\real.jpg" "image/jpeg" @("isMain=false")
Check "JPEG real -> 201" ($r.status -eq 201) "status=$($r.status) $($r.raw)"
$r = Invoke-Multipart POST "/api/vehicle/2/images" "$files\real.gif" "image/gif"
Check "GIF real -> 201" ($r.status -eq 201) "status=$($r.status) $($r.raw)"
$r = Invoke-Multipart POST "/api/vehicle/2/images" "$files\fake.png" "image/png"
Check "texto plano con nombre .png -> 400 (firma no reconocida)" ($r.status -eq 400 -and $r.body.detail -match "firma") "status=$($r.status) $($r.raw)"
$r = Invoke-Multipart POST "/api/vehicle/2/images" "$files\corrupt.png" "image/png"
Check "PNG truncado sin IEND -> 400 (corrupto)" ($r.status -eq 400 -and $r.body.detail -match "corrupto") "status=$($r.status) $($r.raw)"
$r = Invoke-Multipart POST "/api/vehicle/2/images" "$files\real.jpg" "image/png"
Check "JPEG declarado como image/png -> 400 (tipo no coincide)" ($r.status -eq 400 -and $r.body.detail -match "real es JPEG") "status=$($r.status) $($r.raw)"
$r = Invoke-Multipart POST "/api/vehicle/2/images" "$files\big.png" "image/png"
Check "archivo de 6 MB -> 413" ($r.status -eq 413) "status=$($r.status)"
$r = Invoke-Multipart POST "/api/vehicle/9999/images" "$files\real.png" "image/png"
Check "upload a vehiculo inexistente -> 404" ($r.status -eq 404) "status=$($r.status)"

Write-Host ""
Write-Host "=== D. Reemplazo de imagen con compensacion ==="
$r = Invoke-Api GET "/api/vehicle/2/images"
$imgs = @($r.body.data)
Check "vehiculo 2 tiene 3 imagenes validas" ($imgs.Count -eq 3) "count=$($imgs.Count)"
$main = @($imgs | Where-Object { $_.isMain })
Check "vehiculo 2 tiene exactamente 1 imagen principal" ($main.Count -eq 1) "count=$($main.Count)"
$mainId = $main[0].idVehicleImage
$oldRel = DiskPath $main[0]
Check "archivo original de la imagen principal existe en disco" (Test-Path $oldRel) $oldRel
$oldBytes = [System.IO.File]::ReadAllBytes($oldRel)

# Reemplazo valido (real.jpg con tipo correcto)
$r = Invoke-Multipart PUT "/api/vehicle/2/images/$mainId" "$files\real.jpg" "image/jpeg"
Check "reemplazo valido -> 200" ($r.status -eq 200) "status=$($r.status) $($r.raw)"
$newRel = DiskPath $r.body.data
$newBytes = [System.IO.File]::ReadAllBytes($newRel)
Check "el contenido en disco cambio (bytes distintos)" (-not [System.Linq.Enumerable]::SequenceEqual($oldBytes, $newBytes))
Check "archivo anterior fue borrado tras persistir" ((Test-Path $oldRel) -eq $false) $oldRel
$r = Invoke-Api GET "/api/vehicle/2"
$mainAfter = @($r.body.data.images | Where-Object { $_.isMain })
Check "imagen principal conserva coherencia (1 sola, coincide con imagePath)" ($mainAfter.Count -eq 1 -and $r.body.data.imagePath -eq $mainAfter[0].publicUrl) "imagePath=$($r.body.data.imagePath)"

# Reemplazo invalido: el archivo no debe tocarse
$curRel = DiskPath $mainAfter[0]
$curBytes = [System.IO.File]::ReadAllBytes($curRel)
$r2 = Invoke-Multipart PUT "/api/vehicle/2/images/$mainId" "$files\fake.png" "image/png"
Check "reemplazo con contenido falso -> 400" ($r2.status -eq 400) "status=$($r2.status)"
Check "archivo actual sigue intacto tras reemplazo invalido" ([System.Linq.Enumerable]::SequenceEqual($curBytes, [System.IO.File]::ReadAllBytes($curRel)))

Write-Host ""
Write-Host "=== D2. Fallo simulado de guardado en el reemplazo (trigger en BD de prueba) ==="
$filesBefore = @(Get-ChildItem $storage -File).Count
sqlcmd -S localhost -d $testDb -C -E -Q "IF OBJECT_ID('trg_sim_fail','TR') IS NOT NULL DROP TRIGGER trg_sim_fail; EXEC('CREATE TRIGGER trg_sim_fail ON VehicleImage AFTER UPDATE AS BEGIN RAISERROR(''Simulated persistence failure (test)'', 16, 1); END');" | Out-Null
$preFailBytes = [System.IO.File]::ReadAllBytes($curRel)
$r = Invoke-Multipart PUT "/api/vehicle/2/images/$mainId" "$files\real.png" "image/png"
Check "reemplazo con fallo de BD simulado -> 500 Result<T> conservando imagen anterior" ($r.status -eq 500 -and $r.body.detail -match "conserva la imagen anterior") "status=$($r.status) $($r.raw)"
Check "archivo anterior sigue en disco tras el fallo" ([System.Linq.Enumerable]::SequenceEqual($preFailBytes, [System.IO.File]::ReadAllBytes($curRel)))
$curDbRel = ((sqlcmd -S localhost -d $testDb -C -E -Q "SET NOCOUNT ON; SELECT RelativePath FROM VehicleImage WHERE IdVehicleImage = $mainId" -W -h -1 | ForEach-Object { $_.Trim() } | Where-Object { $_ -match '\.jpg$' }) -join "").Replace('/', '\')
Check "BD conserva la referencia antigua (no hubo cambio parcial)" ($curDbRel -eq $curRel) "db=$curDbRel cur=$curRel"
$filesAfter = @(Get-ChildItem $storage -File).Count
Check "el archivo nuevo fue compensado (mismo numero de archivos en disco)" ($filesBefore -eq $filesAfter) "before=$filesBefore after=$filesAfter"
sqlcmd -S localhost -d $testDb -C -E -Q "DROP TRIGGER trg_sim_fail;" | Out-Null

Write-Host ""
Write-Host "=== Resumen ==="
Write-Host "PASS: $script:pass   FAIL: $script:fail"
if ($script:fail -gt 0) { exit 1 } else { exit 0 }
