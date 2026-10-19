# Bateria 2 — CRUD, duplicados, referencias inexistentes, filtros, paginacion,
# borrado logico, integridad FK y asociaciones (63 checks).
# Se ejecuta DESPUES de Bateria1-Fixes.ps1 sobre la misma BD y el mismo API.
#
# Requisitos previos (ver README, seccion "Como reproducir la verificacion"):
#   1. BD independiente aplicada:  FlotaVehicular_RevVerify  (dotnet ef database update)
#   2. API en http://localhost:5137 apuntando a esa BD.
# No modifica la BD FlotaVehicular ni el proyecto base ERP9.
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

Write-Host "=== E. Categorias de vehiculo ==="
$catBody = '{"name":"Categoria TEST-VERIFY","description":"creada por la bateria de verificacion","preventiveMaintenanceIntervalKm":10000}'
$r = Invoke-Api POST "/api/vehiclecategory" $catBody
Check "crear categoria -> 201" ($r.status -eq 201) "status=$($r.status) $($r.raw)"
$catId = $r.body.data.idVehicleCategory
$r = Invoke-Api GET "/api/vehiclecategory/$catId"
Check "GET categoria creada -> 200 con vehicleCount=0" ($r.status -eq 200 -and $r.body.data.vehicleCount -eq 0)
$r = Invoke-Api POST "/api/vehiclecategory" $catBody
Check "duplicar nombre de categoria -> 409" ($r.status -eq 409) "status=$($r.status) $($r.raw)"
$r = Invoke-Api PUT "/api/vehiclecategory/$catId" '{"name":"Categoria TEST-VERIFY v2","description":"editada","preventiveMaintenanceIntervalKm":12000}'
Check "editar categoria -> 200 con nombre nuevo" ($r.status -eq 200 -and $r.body.data.name -eq "Categoria TEST-VERIFY v2") "status=$($r.status)"
$r = Invoke-Api GET "/api/vehiclecategory/$catId"
Check "persisten descripcion e intervalo (mapster sobre entidad)" ($r.body.data.description -eq "editada" -and $r.body.data.preventiveMaintenanceIntervalKm -eq 12000) "desc=$($r.body.data.description) intervalo=$($r.body.data.preventiveMaintenanceIntervalKm)"
$r = Invoke-Api POST "/api/vehiclecategory" '{"name":"Categoria TEST-VERIFY VACIA","description":"sin vehiculos","preventiveMaintenanceIntervalKm":5000}'
Check "crear categoria vacia -> 201" ($r.status -eq 201) "status=$($r.status)"
$emptyCatId = $r.body.data.idVehicleCategory
$r = Invoke-Api GET "/api/vehiclecategory?search=TEST-VERIFY"
Check "filtro search encuentra la categoria" ($r.status -eq 200 -and @($r.body.data).Count -ge 1) "count=$(@($r.body.data).Count)"
$r = Invoke-Api GET "/api/vehiclecategory/paged?pageNumber=1&pageSize=3"
Check "paginacion pageSize=3 devuelve 3" (@($r.body.data).Count -eq 3 -and $r.body.pageSize -eq 3 -and $r.body.totalPages -ge 4) "count=$(@($r.body.data).Count) totalPages=$($r.body.totalPages)"

Write-Host ""
Write-Host "=== F. Vehiculos: CRUD, duplicados, referencias, filtros ==="
$vehBody = '{"code":"VER-900","licensePlate":"VER-900-A","name":"Vehiculo de verificacion","acquisitionDate":"2024-01-15","referenceValue":15000,"idVehicleCategory":' + $catId + ',"brand":"TestBrand","model":"ModelX","year":2023,"currentMileage":1000,"lastPreventiveMaintenanceMileage":800}'
$r = Invoke-Api POST "/api/vehicle" $vehBody
Check "crear vehiculo -> 201" ($r.status -eq 201) "status=$($r.status) $($r.raw)"
$vehId = $r.body.data.idVehicle
Check "vehiculo nuevo arranca Operativo" ($r.body.data.status -eq "Operativo") "status=$($r.body.data.status)"
$r = Invoke-Api POST "/api/vehicle" $vehBody
Check "duplicar placa -> 409" ($r.status -eq 409) "status=$($r.status) $($r.raw)"
$r = Invoke-Api POST "/api/vehicle" ($vehBody -replace 'VER-900-A','VER-901-A')
Check "duplicar codigo -> 409" ($r.status -eq 409) "status=$($r.status) $($r.raw)"
$r = Invoke-Api POST "/api/vehicle" ($vehBody -replace '"idVehicleCategory":\d+','"idVehicleCategory":9999')
Check "vehiculo con categoria inexistente -> 400/404" ($r.status -eq 400 -or $r.status -eq 404) "status=$($r.status)"
$vehEdit = $vehBody -replace '"name":"[^"]*"','"name":"Vehiculo de verificacion EDITADO"' -replace '"brand":"[^"]*"','"brand":"MarcaEditada"' -replace '"currentMileage":\d+','"currentMileage":2222'
$r = Invoke-Api PUT "/api/vehicle/$vehId" $vehEdit
Check "editar vehiculo -> 200 con nombre nuevo" ($r.status -eq 200 -and $r.body.data.name -eq "Vehiculo de verificacion EDITADO") "status=$($r.status)"
$r = Invoke-Api GET "/api/vehicle/$vehId"
Check "persisten campos mapeados (brand, kilometraje)" ($r.body.data.brand -eq "MarcaEditada" -and $r.body.data.currentMileage -eq 2222) "brand=$($r.body.data.brand) km=$($r.body.data.currentMileage)"
$r = Invoke-Api GET "/api/vehicle?search=VER-900"
Check "filtro search por codigo/placa" ($r.status -eq 200 -and @($r.body.data).Count -ge 1)
$r = Invoke-Api GET "/api/vehicle?status=EnMantenimiento"
Check "filtro por status (lista puede ser vacia, 200)" ($r.status -eq 200)
$r = Invoke-Api GET "/api/vehicle?idVehicleCategory=$catId"
Check "filtro por categoria devuelve solo los de esa categoria" ($r.status -eq 200 -and @(@($r.body.data) | Where-Object { $_.idVehicleCategory -ne $catId }).Count -eq 0)
# totalRecords debe coincidir con el listado activo (excluye dados de baja)
$rAll = Invoke-Api GET "/api/vehicle?pageSize=50"
$activeTotal = @($rAll.body.data).Count
$rInc = Invoke-Api GET "/api/vehicle?includeDeactivated=true&pageSize=50"
$physicalTotal = @($rInc.body.data).Count
$r = Invoke-Api GET "/api/vehicle/paged?pageNumber=1&pageSize=4"
Check "paginacion vehiculos pageSize=4" (@($r.body.data).Count -eq 4 -and $r.body.totalRecords -eq $activeTotal) "count=$(@($r.body.data).Count) total=$($r.body.totalRecords) activos=$activeTotal"
Check "totalRecords excluye dados de baja" ($activeTotal -lt $physicalTotal -and $r.body.totalRecords -eq $activeTotal) "activos=$activeTotal fisicos=$physicalTotal"

Write-Host ""
Write-Host "=== G. Repuestos y proveedores: CRUD y filtros ==="
$spBody = '{"code":"VER-SP-1","name":"Repuesto verificacion","unitOfMeasure":"unidad","currentStock":5,"minStock":2}'
$r = Invoke-Api POST "/api/sparepart" $spBody
Check "crear repuesto -> 201" ($r.status -eq 201) "status=$($r.status) $($r.raw)"
$spId = $r.body.data.idSparePart
$r = Invoke-Api POST "/api/sparepart" $spBody
Check "duplicar codigo de repuesto -> 409" ($r.status -eq 409) "status=$($r.status)"
$r = Invoke-Api POST "/api/sparepart" '{"code":"VER-SP-2","name":"Repuesto bajo minimo","unitOfMeasure":"unidad","currentStock":1,"minStock":5}'
Check "crear repuesto bajo minimo -> 201" ($r.status -eq 201)
$lowId = $r.body.data.idSparePart
$r = Invoke-Api PUT "/api/sparepart/$spId" '{"code":"VER-SP-1","name":"Repuesto verificacion EDITADO","unitOfMeasure":"litro","currentStock":7,"minStock":3}'
Check "editar repuesto -> 200" ($r.status -eq 200) "status=$($r.status) $($r.raw)"
$r = Invoke-Api GET "/api/sparepart/$spId"
Check "persisten nombre, unidad y stock (mapster sobre entidad)" ($r.body.data.name -eq "Repuesto verificacion EDITADO" -and $r.body.data.unitOfMeasure -eq "litro" -and $r.body.data.currentStock -eq 7 -and $r.body.data.minStock -eq 3) "name=$($r.body.data.name) stock=$($r.body.data.currentStock)"
$r = Invoke-Api GET "/api/sparepart/below-minimum-stock"
Check "listado below-minimum incluye el repuesto bajo stock" ($r.status -eq 200 -and @(@($r.body.data) | Where-Object { $_.idSparePart -eq $lowId }).Count -eq 1) "status=$($r.status)"
$r = Invoke-Api GET "/api/sparepart?belowMinimumOnly=true"
Check "filtro belowMinimumOnly=true" ($r.status -eq 200 -and @(@($r.body.data) | Where-Object { $_.idSparePart -eq $lowId }).Count -eq 1) "status=$($r.status)"

$supBody = '{"name":"Proveedor VER-TEST","contact":"contacto@vertest.test","address":"Calle Test 123","generalConditions":"Neto 30 dias"}'
$r = Invoke-Api POST "/api/supplier" $supBody
Check "crear proveedor -> 201" ($r.status -eq 201) "status=$($r.status) $($r.raw)"
$supId = $r.body.data.idSupplier
$r = Invoke-Api POST "/api/supplier" $supBody
Check "duplicar nombre de proveedor -> 409" ($r.status -eq 409) "status=$($r.status)"
$r = Invoke-Api PUT "/api/supplier/$supId" '{"name":"Proveedor VER-TEST","contact":"editado@vertest.test","address":"Avenida Editada 45","generalConditions":"Contado 60 dias"}'
Check "editar proveedor -> 200" ($r.status -eq 200) "status=$($r.status) $($r.raw)"
$r = Invoke-Api GET "/api/supplier/$supId"
Check "persisten direccion y condiciones (mapster sobre entidad)" ($r.body.data.address -eq "Avenida Editada 45" -and $r.body.data.generalConditions -eq "Contado 60 dias") "addr=$($r.body.data.address)"
$r = Invoke-Api PUT "/api/supplier/$supId/deactivate"
Check "desactivar proveedor -> 200 con status=false" ($r.status -eq 200 -and $r.body.data.status -eq $false)
$r = Invoke-Api GET "/api/supplier?includeInactive=false"
Check "listado por defecto excluye inactivos" ($r.status -eq 200 -and @(@($r.body.data) | Where-Object { $_.idSupplier -eq $supId }).Count -eq 0)
$r = Invoke-Api GET "/api/supplier?includeInactive=true"
Check "includeInactive=true lo incluye" ($r.status -eq 200 -and @(@($r.body.data) | Where-Object { $_.idSupplier -eq $supId }).Count -eq 1) "status=$($r.status)"
$r = Invoke-Api PUT "/api/supplier/$supId/activate"
Check "reactivar proveedor -> 200 con status=true" ($r.status -eq 200 -and $r.body.data.status -eq $true)

Write-Host ""
Write-Host "=== H. Asociaciones repuesto-proveedor ==="
$linkBody = '{"idSparePart":' + $spId + ',"idSupplier":' + $supId + ',"offeredPrice":15.75,"estimatedDeliveryDays":7}'
$r = Invoke-Api POST "/api/sparepartsupplier" $linkBody
Check "crear asociacion -> 201" ($r.status -eq 201) "status=$($r.status) $($r.raw)"
$r = Invoke-Api POST "/api/sparepartsupplier" $linkBody
Check "duplicar misma asociacion -> 409" ($r.status -eq 409) "status=$($r.status)"
$r = Invoke-Api POST "/api/sparepartsupplier" ('{"idSparePart":9999,"idSupplier":' + $supId + ',"offeredPrice":10,"estimatedDeliveryDays":3}')
Check "asociacion con repuesto inexistente -> 404/400" ($r.status -eq 400 -or $r.status -eq 404) "status=$($r.status)"
$r = Invoke-Api POST "/api/sparepartsupplier" ('{"idSparePart":' + $spId + ',"idSupplier":9999,"offeredPrice":10,"estimatedDeliveryDays":3}')
Check "asociacion con proveedor inexistente -> 404/400" ($r.status -eq 400 -or $r.status -eq 404) "status=$($r.status)"
$r = Invoke-Api PUT "/api/sparepartsupplier/$spId/$supId" '{"offeredPrice":20.5,"estimatedDeliveryDays":10}'
Check "actualizar precio y dias -> 200 con valores nuevos" ($r.status -eq 200 -and $r.body.data.offeredPrice -eq 20.5 -and $r.body.data.estimatedDeliveryDays -eq 10) "status=$($r.status)"
$r = Invoke-Api GET "/api/sparepartsupplier?idSparePart=$spId"
Check "filtro independiente idSparePart" ($r.status -eq 200 -and @(@($r.body.data) | Where-Object { $_.idSparePart -ne $spId }).Count -eq 0)
$r = Invoke-Api GET "/api/sparepartsupplier?idSupplier=$supId"
Check "filtro independiente idSupplier" ($r.status -eq 200 -and @(@($r.body.data) | Where-Object { $_.idSupplier -ne $supId }).Count -eq 0)
$r = Invoke-Api GET "/api/sparepartsupplier?idSparePart=$spId&idSupplier=$supId"
Check "ambos filtros combinados devuelven 1" ($r.status -eq 200 -and @($r.body.data).Count -eq 1) "count=$(@($r.body.data).Count)"
$r = Invoke-Api GET "/api/sparepart/$spId"
Check "detalle del repuesto refleja la asociacion (supplierCount=1, lista=1)" ($r.body.data.supplierCount -eq 1 -and @($r.body.data.suppliers).Count -eq 1 -and $r.body.data.suppliers[0].offeredPrice -eq 20.5)
$r = Invoke-Api GET "/api/supplier/$supId"
Check "detalle del proveedor refleja la asociacion (sparePartCount=1, lista=1)" ($r.body.data.sparePartCount -eq 1 -and @($r.body.data.spareParts).Count -eq 1)

Write-Host ""
Write-Host "=== I. Borrado logico e integridad referencial ==="
$r = Invoke-Api DELETE "/api/vehicle/$vehId"
Check "DELETE vehiculo -> 200 (baja logica)" ($r.status -eq 200) "status=$($r.status)"
$r = Invoke-Api GET "/api/vehicle/$vehId"
Check "vehiculo sigue consultable tras baja" ($r.status -eq 200 -and $r.body.data.status -eq "DadoDeBaja")
$r = Invoke-Api GET "/api/vehicle?search=VER-900"
Check "listado por defecto lo excluye" (@($r.body.data | Where-Object { $_.idVehicle -eq $vehId }).Count -eq 0)
$r = Invoke-Api GET "/api/vehicle?search=VER-900&includeDeactivated=true"
Check "includeDeactivated=true lo muestra" (@($r.body.data | Where-Object { $_.idVehicle -eq $vehId }).Count -eq 1)
$r = Invoke-Api DELETE "/api/vehicle/$vehId"
Check "DELETE repetido -> idempotente 200" ($r.status -eq 200) "status=$($r.status)"
$r = Invoke-Api PUT "/api/vehicle/$vehId/status" '{"status":"Operativo"}'
Check "cambiar estado de un dado de baja -> 409" ($r.status -eq 409) "status=$($r.status)"
$r = Invoke-Api DELETE "/api/vehiclecategory/$catId"
Check "eliminar categoria con vehiculos -> 409 (FK Restrict)" ($r.status -eq 409) "status=$($r.status) $($r.raw)"
$r = Invoke-Api DELETE "/api/sparepart/$spId"
Check "eliminar repuesto con asociaciones -> 409 (FK Restrict)" ($r.status -eq 409) "status=$($r.status)"
$r = Invoke-Api DELETE "/api/supplier/$supId"
Check "eliminar proveedor con asociaciones -> 409 (FK Restrict)" ($r.status -eq 409) "status=$($r.status)"
$r = Invoke-Api DELETE "/api/sparepartsupplier/$spId/$supId"
Check "borrar asociacion -> 200" ($r.status -eq 200) "status=$($r.status)"
$r = Invoke-Api DELETE "/api/sparepartsupplier/$spId/$supId"
Check "borrar asociacion inexistente -> 404" ($r.status -eq 404) "status=$($r.status)"
$r = Invoke-Api DELETE "/api/sparepart/$spId"
Check "repuesto sin asociaciones -> 200" ($r.status -eq 200) "status=$($r.status) $($r.raw)"
$r = Invoke-Api DELETE "/api/supplier/$supId"
Check "proveedor sin asociaciones -> 200" ($r.status -eq 200) "status=$($r.status) $($r.raw)"
$r = Invoke-Api DELETE "/api/sparepart/$lowId"
$r = Invoke-Api DELETE "/api/vehiclecategory/$emptyCatId"
Check "categoria sin vehiculos -> 200" ($r.status -eq 200) "status=$($r.status) $($r.raw)"
$r = Invoke-Api GET "/api/sparepart/9999"
Check "GET repuesto inexistente -> 404" ($r.status -eq 404)
$r = Invoke-Api GET "/api/supplier/9999"
Check "GET proveedor inexistente -> 404" ($r.status -eq 404)
$r = Invoke-Api GET "/api/vehiclecategory/9999"
Check "GET categoria inexistente -> 404" ($r.status -eq 404)
$r = Invoke-Api GET "/api/vehicle/9999/images"
Check "imagenes de vehiculo inexistente -> 404" ($r.status -eq 404)

Write-Host ""
Write-Host "=== Resumen bateria 2 ==="
Write-Host "PASS: $script:pass   FAIL: $script:fail"
if ($script:fail -gt 0) { exit 1 } else { exit 0 }
