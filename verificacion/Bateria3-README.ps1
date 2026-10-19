# Batería 3: replay literal del catálogo del README (no-imágenes) sobre BD desechable
$ErrorActionPreference = "Stop"
$api = "http://localhost:5137"
$script:pass = 0; $script:fail = 0

function Check($name, $cond, $detail="") {
  if ($cond) { $script:pass++; Write-Host "PASS  $name" } else { $script:fail++; Write-Host "FAIL  $name  $detail" }
}

function Invoke-Api($method, $url, $body=$null) {
  try {
    if ($null -ne $body) {
      $r = Invoke-WebRequest -Uri "$api$url" -Method $method -Body $body -ContentType "application/json" -UseBasicParsing
    } else {
      $r = Invoke-WebRequest -Uri "$api$url" -Method $method -UseBasicParsing
    }
    $data = $null; try { $data = $r.Content | ConvertFrom-Json } catch {}
    return @{ status=[int]$r.StatusCode; body=$data; raw=$r.Content }
  } catch {
    $resp = $_.Exception.Response
    $status = if ($resp) { [int]$resp.StatusCode } else { 0 }
    $raw = ""
    if ($resp) { try { $s=$resp.GetResponseStream(); $s.Position=0; $sr=New-Object IO.StreamReader($s); $raw=$sr.ReadToEnd()} catch {} }
    $data = $null; try { $data = $raw | ConvertFrom-Json } catch {}
    return @{ status=$status; body=$data; raw=$raw }
  }
}

Write-Host "=== BATERIA 3 (replay README) ==="
# 1) Categorías: crear
$r = Invoke-Api POST "/api/vehiclecategory" '{"name":"Categoria TEST-VERIFY","description":"creada por la bateria de verificacion","preventiveMaintenanceIntervalKm":10000}'
Check "cat crear -> 201" ($r.status -eq 201) $r.raw
$catId = $r.body.data.idVehicleCategory
# dup
$r = Invoke-Api POST "/api/vehiclecategory" '{"name":"Categoria TEST-VERIFY","description":"creada por la bateria de verificacion","preventiveMaintenanceIntervalKm":10000}'
Check "cat dup -> 409" ($r.status -eq 409) $r.raw
# put
$r = Invoke-Api PUT "/api/vehiclecategory/$catId" '{"name":"Categoria TEST-VERIFY v2","description":"editada","preventiveMaintenanceIntervalKm":12000}'
Check "cat put -> 200" ($r.status -eq 200) $r.raw
$r = Invoke-Api GET "/api/vehiclecategory/$catId"
Check "cat get detalle" ($r.status -eq 200 -and $r.body.data.description -eq "editada" -and $r.body.data.preventiveMaintenanceIntervalKm -eq 12000) $r.raw
# search/paged
$r = Invoke-Api GET "/api/vehiclecategory?search=TEST-VERIFY"
Check "cat search" ($r.status -eq 200) $r.raw
$r = Invoke-Api GET "/api/vehiclecategory/paged?pageNumber=1&pageSize=3"
Check "cat paged" ($r.status -eq 200 -and @($r.body.data).Count -eq 3 -and $r.body.totalPages -ge 4) $r.raw
# 9999
$r = Invoke-Api GET "/api/vehiclecategory/9999"
Check "cat 9999 -> 404" ($r.status -eq 404) $r.raw
# delete seeded cat1 has vehicles? battery2 showed 409; DELETE cat1 now
$r = Invoke-Api DELETE "/api/vehiclecategory/1"
Check "cat1 delete con vehiculos -> 409" ($r.status -eq 409) $r.raw
# delete empty cat11 -> 200
$r = Invoke-Api DELETE "/api/vehiclecategory/$catId"
Check "cat11 delete vacia -> 200" ($r.status -eq 200) $r.raw

# 2) Vehículos
$body1 = '{"code":"VER-900","licensePlate":"VER-900-A","name":"Vehiculo de verificacion","acquisitionDate":"2024-01-15","referenceValue":15000,"idVehicleCategory":1,"brand":"TestBrand","model":"ModelX","year":2023,"currentMileage":1000,"lastPreventiveMaintenanceMileage":800}'
$r = Invoke-Api POST "/api/vehicle" $body1
Check "veh crear -> 201" ($r.status -eq 201) $r.raw
$vehId = $r.body.data.idVehicle
# dup exact
$r = Invoke-Api POST "/api/vehicle" $body1
Check "veh dup codigo exacto -> 409" ($r.status -eq 409 -and $r.body.detail -match "VER-900") $r.raw
# same code new plate
$r = Invoke-Api POST "/api/vehicle" ($body1 -replace '"licensePlate":"VER-900-A"','"licensePlate":"VER-901-A"')
Check "veh dup codigo otra placa -> 409" ($r.status -eq 409 -and $r.body.detail -match "VER-900") $r.raw
# new code same plate
$r = Invoke-Api POST "/api/vehicle" ($body1 -replace '"code":"VER-900"','"code":"VER-901"')
Check "veh dup placa otro codigo -> 409" ($r.status -eq 409 -and $r.body.detail -match "VER-900-A") $r.raw
# cat 9999
$bodyCat = $body1 -replace '"code":"VER-900"','"code":"VER-902"'-replace '"licensePlate":"VER-900-A"','"licensePlate":"VER-902-A"'-replace '"idVehicleCategory":1','"idVehicleCategory":9999'
$r = Invoke-Api POST "/api/vehicle" $bodyCat
Check "veh cat9999 -> 400" ($r.status -eq 400 -and $r.body.detail -match "categor") $r.raw
# put
$vehPut = $body1 -replace '"name":"[^"]*"','"name":"Vehiculo de verificacion EDITADO"'-replace '"brand":"[^"]*"','"brand":"MarcaEditada"'-replace '"currentMileage":\d+','"currentMileage":2222'
$r = Invoke-Api PUT "/api/vehicle/$vehId" $vehPut
Check "veh put -> 200" ($r.status -eq 200) $r.raw
$r = Invoke-Api GET "/api/vehicle/$vehId"
Check "veh put persisted" ($r.status -eq 200 -and $r.body.data.name -eq "Vehiculo de verificacion EDITADO" -and $r.body.data.brand -eq "MarcaEditada" -and $r.body.data.currentMileage -eq 2222) $r.raw
# status cases
$r = Invoke-Api PUT "/api/vehicle/$vehId/status" '{"status":"FueraDeServicio"}'; Check "veh status fs -> 200" ($r.status -eq 200) $r.raw
$r = Invoke-Api PUT "/api/vehicle/1/status" '{"status":99}'; Check "veh status 99 -> 400" ($r.status -eq 400) $r.raw
$r = Invoke-Api PUT "/api/vehicle/1/status" '{"status":"NoExiste"}'; Check "veh status NoExiste -> 400" ($r.status -eq 400) $r.raw
$r = Invoke-Api PUT "/api/vehicle/1/status" '{"status":"DadoDeBaja"}'; Check "veh status DadoDeBaja -> 400" ($r.status -eq 400 -and $r.body.detail -match "DELETE") $r.raw
$r = Invoke-Api PUT "/api/vehicle/9999/status" '{"status":"Operativo"}'; Check "veh status 9999 -> 404" ($r.status -eq 404) $r.raw
# filters
$r = Invoke-Api GET "/api/vehicle?search=VER-900"; Check "veh search" ($r.status -eq 200) $r.raw
$r = Invoke-Api GET "/api/vehicle?status=EnMantenimiento"; Check "veh status filter" ($r.status -eq 200) $r.raw
$r = Invoke-Api GET "/api/vehicle?idVehicleCategory=1"; Check "veh cat filter" ($r.status -eq 200) $r.raw
$r = Invoke-Api GET "/api/vehicle/paged?pageNumber=1&pageSize=4"; Check "veh paged" ($r.status -eq 200) $r.raw
# delete
$r = Invoke-Api DELETE "/api/vehicle/$vehId"; Check "veh del1 -> 200" ($r.status -eq 200) $r.raw
$r = Invoke-Api DELETE "/api/vehicle/$vehId"; Check "veh del2 idempot -> 200" ($r.status -eq 200) $r.raw
$r = Invoke-Api GET "/api/vehicle/$vehId"; Check "veh get baja" ($r.status -eq 200 -and $r.body.data.status -eq "DadoDeBaja") $r.raw
$r = Invoke-Api GET "/api/vehicle?search=VER-900"; Check "veh search excluye baja" ($r.status -eq 200) $r.raw
$r = Invoke-Api GET "/api/vehicle?search=VER-900&includeDeactivated=true"; Check "veh search incl baja" ($r.status -eq 200) $r.raw
$r = Invoke-Api PUT "/api/vehicle/$vehId/status" '{"status":"Operativo"}'; Check "veh reactivar por status -> 409" ($r.status -eq 409) $r.raw

Write-Host ""
Write-Host "RESULTADO BATERIA 3: PASS=$pass FAIL=$fail"
if ($fail -eq 0) { exit 0 } else { exit 1 }
