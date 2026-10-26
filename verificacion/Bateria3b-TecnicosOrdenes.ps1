# Bateria 3b — Tecnicos, Especialidades, TechnicianSpecialty y Ordenes de mantenimiento
# (aciertos/total como Bateria2-CRUD.ps1)
$ErrorActionPreference = "Stop"
$api = "http://localhost:5137"
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

Write-Host "=== Especialidades ==="
$r = Invoke-Api POST "/api/specialty" '{"name":"Especialidad TEST","description":"test"}'
Check "crear especialidad -> 201" ($r.status -eq 201) "status=$($r.status) $($r.raw)"
$specId = $r.body.data.idSpecialty
$r = Invoke-Api POST "/api/specialty" '{"name":"Especialidad TEST","description":"test"}'
Check "especialidad duplicada -> 409" ($r.status -eq 409) "status=$($r.status)"
$r = Invoke-Api PUT "/api/specialty/$specId" '{"name":"Especialidad TEST v2","description":"editada"}'
Check "editar especialidad -> 200" ($r.status -eq 200) "status=$($r.status)"
$r = Invoke-Api DELETE "/api/specialty/$specId"
Check "borrar especialidad sin tecnicos -> 200" ($r.status -eq 200) "status=$($r.status)"

Write-Host ""
Write-Host "=== Tecnicos ==="
$r = Invoke-Api POST "/api/technician" '{"fullName":"Tecnico Test","identificationNumber":"9-9999-9999","email":"tecnico.test@flota.test","phone":"8999-9999","hourlyRate":5000}'
Check "crear tecnico -> 201" ($r.status -eq 201) "status=$($r.status) $($r.raw)"
$techId = $r.body.data.idTechnician
$r = Invoke-Api POST "/api/technician" '{"fullName":"Tecnico Test2","identificationNumber":"9-9999-9999","email":"tecnico.test2@flota.test","phone":"8999-8888","hourlyRate":5000}'
Check "identificacion duplicada -> 409" ($r.status -eq 409) "status=$($r.status)"
$r = Invoke-Api POST "/api/technician" '{"fullName":"Tecnico Test3","identificationNumber":"8-8888-8888","email":"tecnico.test@flota.test","phone":"8999-7777","hourlyRate":5000}'
Check "correo duplicado -> 409" ($r.status -eq 409) "status=$($r.status)"
$r = Invoke-Api POST "/api/technician" '{"fullName":"Tecnico Malo","identificationNumber":"7-7777-7777","email":"tecnico.malo@flota.test","phone":"8999-6666","hourlyRate":-1}'
Check "tarifa invalida -> 400" ($r.status -eq 400) "status=$($r.status)"
$r = Invoke-Api PUT "/api/technician/$techId/deactivate"
Check "desactivar tecnico -> 200" ($r.status -eq 200) "status=$($r.status)"
$r = Invoke-Api PUT "/api/technician/$techId/activate"
Check "activar tecnico -> 200" ($r.status -eq 200) "status=$($r.status)"
$r = Invoke-Api GET "/api/technician/available?idSpecialty=1"
Check "GET available por especialidad -> 200" ($r.status -eq 200) "status=$($r.status)"

Write-Host ""
Write-Host "=== TechnicianSpecialty ==="
$rSpec = Invoke-Api GET "/api/specialty"
$specTestId = $rSpec.body.data[0].idSpecialty
$r = Invoke-Api POST "/api/technicianspecialty" '{"idTechnician":'$techId',"idSpecialty":'$specTestId',"certificationLevel":"Básico","sinceDate":"2020-01-01"}'
Check "crear asociacion tecnico-especialidad -> 201" ($r.status -eq 201) "status=$($r.status) $($r.raw)"
$r = Invoke-Api POST "/api/technicianspecialty" '{"idTechnician":'$techId',"idSpecialty":'$specTestId',"certificationLevel":"Intermedio","sinceDate":"2021-01-01"}'
Check "asociacion repetida -> 409" ($r.status -eq 409) "status=$($r.status)"
$r = Invoke-Api POST "/api/technicianspecialty" '{"idTechnician":10,"idSpecialty":'$specTestId',"certificationLevel":"Básico","sinceDate":"2020-01-01"}'
Check "tecnico inactivo -> 400" ($r.status -eq 400) "status=$($r.status)"
$r = Invoke-Api POST "/api/technicianspecialty" '{"idTechnician":9999,"idSpecialty":'$specTestId',"certificationLevel":"Básico","sinceDate":"2020-01-01"}'
Check "tecnico inexistente -> 404" ($r.status -eq 404) "status=$($r.status)"
$r = Invoke-Api PUT "/api/technicianspecialty/$techId/$specTestId" '{"certificationLevel":"Avanzado","sinceDate":"2022-01-01"}'
Check "actualizar asociacion -> 200" ($r.status -eq 200) "status=$($r.status)"
$r = Invoke-Api DELETE "/api/technicianspecialty/$techId/$specTestId"
Check "desasociar -> 200" ($r.status -eq 200) "status=$($r.status)"

Write-Host ""
Write-Host "=== Ordenes de mantenimiento ==="
$rVeh = Invoke-Api GET "/api/vehicle"
$vehOp = $null
$vehMant = $null
$vehBad = $null
$vehOut = $null
foreach ($v in $rVeh.body.data) {
    if ($v.status -eq "Operativo" -and $vehOp -eq $null) { $vehOp = $v }
    if ($v.status -eq "EnMantenimiento" -and $vehMant -eq $null) { $vehMant = $v }
    if ($v.status -eq "DadoDeBaja" -and $vehBad -eq $null) { $vehBad = $v }
    if ($v.status -eq "FueraDeServicio" -and $vehOut -eq $null) { $vehOut = $v }
}
$r = Invoke-Api POST "/api/maintenanceorder" '{"idVehicle":'$($vehOp.idVehicle)',"maintenanceType":"Preventivo","idRequiredSpecialty":1,"reason":"Revisión general de prueba","scheduledDate":null,"priority":"Media"}'
Check "crear orden valida -> 201" ($r.status -eq 201) "status=$($r.status) $($r.raw)"
if ($r.status -eq 201) {
    Check "nace Solicitada" ($r.body.data.status -eq "Solicitada") "status=$($r.body.data.status)"
    Check "TotalCost 0" ($r.body.data.totalCost -eq 0) "cost=$($r.body.data.totalCost)"
    Check "codigo OM-" ($r.body.data.orderCode -like "OM-*") "code=$($r.body.data.orderCode)"
}
$r = Invoke-Api POST "/api/maintenanceorder" '{"idVehicle":9999,"maintenanceType":"Preventivo","reason":"Prueba","priority":"Media"}'
Check "vehiculo inexistente -> 404" ($r.status -eq 404) "status=$($r.status)"
$r = Invoke-Api POST "/api/maintenanceorder" '{"idVehicle":'$($vehBad.idVehicle)',"maintenanceType":"Correctivo","reason":"Prueba fallo","priority":"Alta"}'
Check "vehiculo DadoDeBaja -> 400" ($r.status -eq 400) "status=$($r.status)"
$r = Invoke-Api POST "/api/maintenanceorder" '{"idVehicle":'$($vehOut.idVehicle)',"maintenanceType":"Correctivo","reason":"Prueba fuera","priority":"Alta"}'
Check "vehiculo FueraDeServicio -> 400" ($r.status -eq 400) "status=$($r.status)"
$r = Invoke-Api POST "/api/maintenanceorder" '{"idVehicle":'$($vehMant.idVehicle)',"maintenanceType":"Correctivo","reason":"Prueba mantenimiento","priority":"Alta"}'
Check "EnMantenimiento con no-Emergencia -> 400" ($r.status -eq 400) "status=$($r.status)"
$r = Invoke-Api POST "/api/maintenanceorder" '{"idVehicle":'$($vehMant.idVehicle)',"maintenanceType":"Emergencia","reason":"Emergencia en curso","priority":"Critica"}'
Check "EnMantenimiento con Emergencia -> 201" ($r.status -eq 201) "status=$($r.status)"
$r = Invoke-Api POST "/api/maintenanceorder" '{"idVehicle":'$($vehOp.idVehicle)',"maintenanceType":"Preventivo","idRequiredSpecialty":9999,"reason":"Prueba especialidad","priority":"Media"}'
Check "especialidad inexistente -> 404" ($r.status -eq 404) "status=$($r.status)"
$r = Invoke-Api POST "/api/maintenanceorder" '{"idVehicle":'$($vehOp.idVehicle)',"maintenanceType":"Preventivo","reason":"x","priority":"Media"}'
Check "motivo corto -> 400" ($r.status -eq 400) "status=$($r.status)"
$r = Invoke-Api POST "/api/maintenanceorder" '{"idVehicle":'$($vehOp.idVehicle)',"maintenanceType":"NoExiste","reason":"Motivo de prueba valido","priority":"Media"}'
Check "enum invalido -> 400" ($r.status -eq 400) "status=$($r.status)"
$past = (Get-Date).AddDays(-1).ToString("yyyy-MM-dd")
$r = Invoke-Api POST "/api/maintenanceorder" '{"idVehicle":'$($vehOp.idVehicle)',"maintenanceType":"Preventivo","reason":"Motivo de prueba valido","priority":"Media","scheduledDate":"'$past'"}'
Check "fecha programada pasada -> 400" ($r.status -eq 400) "status=$($r.status)"
$r = Invoke-Api GET "/api/maintenanceorder?status=Solicitada"
Check "filtro status -> 200" ($r.status -eq 200) "status=$($r.status)"
$r = Invoke-Api GET "/api/maintenanceorder/paged?pageNumber=1&pageSize=5"
Check "paged -> 200" ($r.status -eq 200) "status=$($r.status)"
$r = Invoke-Api GET "/api/maintenanceorder/paged?pageNumber=1&pageSize=51"
Check "pageSize>50 -> 400" ($r.status -eq 400) "status=$($r.status)"

Write-Host ""
Write-Host "=== Handler global ==="
try {
    $resp = Invoke-WebRequest -Uri "$api/api/vehicle" -Method POST -Body "{" -ContentType "application/json" -UseBasicParsing
    Check "JSON malformado" ($false)
} catch {
    $status = [int]$_.Exception.Response.StatusCode
    $raw = ""
    try { $stream = $_.Exception.Response.GetResponseStream(); $stream.Position = 0; $sr = New-Object System.IO.StreamReader($stream); $raw = $sr.ReadToEnd() } catch {}
    $data = $raw | ConvertFrom-Json
    Check "JSON malformado -> 400" ($status -eq 400) "status=$status"
    Check "JSON malformado en formato Result" ($data.isSuccess -eq $false) "isSuccess=$($data.isSuccess)"
}

Write-Host ""
Write-Host "=== Resumen bateria 3b ==="
Write-Host "PASS: $script:pass   FAIL: $script:fail"
if ($script:fail -gt 0) { exit 1 } else { exit 0 }
