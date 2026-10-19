# Genera los 6 archivos de prueba usados por Bateria1-Fixes.ps1.
# Salida: .\archivos-prueba\ (real.png, real.jpg, real.gif, corrupt.png, fake.png, big.png)
# Los binarios validos van embebidos en base64 para que el script sea autocontenido.
$ErrorActionPreference = "Stop"
$dir = Join-Path $PSScriptRoot "archivos-prueba"
New-Item -ItemType Directory -Force -Path $dir | Out-Null

# PNG real 32x32, JPEG real 2x2 y GIF real 1x1 (firmas y marcas de fin correctas)
[IO.File]::WriteAllBytes((Join-Path $dir "real.png"),
    [Convert]::FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAYAAACqaXHeAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAACKSURBVHhe7dAhAQBADIDAJfnM33jzVABxBsm8v2s2DDYNYLBpAINNAxhsGsBg0wAGmwYw2DSAwaYBDDYNYLBpAINNAxhsGsBg0wAGmwYw2DSAwaYBDDYNYLBpAINNAxhsGsBg0wAGmwYw2DSAwaYBDDYNYLBpAINNAxhsGsBg0wAGmwYw2DSAweYAK7DCd8dBtEMAAAAASUVORK5CYII="))
[IO.File]::WriteAllBytes((Join-Path $dir "real.jpg"),
    [Convert]::FromBase64String("/9j/4AAQSkZJRgABAQEAYABgAAD/2wBDAAMCAgMCAgMDAwMEAwMEBQgFBQQEBQoHBwYIDAoMDAsKCwsNDhIQDQ4RDgsLEBYQERMUFRUVDA8XGBYUGBIUFRT/2wBDAQMEBAUEBQkFBQkUDQsNFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBT/wAARCABAAEADASIAAhEBAxEB/8QAHwAAAQUBAQEBAQEAAAAAAAAAAAECAwQFBgcICQoL/8QAtRAAAgEDAwIEAwUFBAQAAAF9AQIDAAQRBRIhMUEGE1FhByJxFDKBkaEII0KxwRVS0fAkM2JyggkKFhcYGRolJicoKSo0NTY3ODk6Q0RFRkdISUpTVFVWV1hZWmNkZWZnaGlqc3R1dnd4eXqDhIWGh4iJipKTlJWWl5iZmqKjpKWmp6ipqrKztLW2t7i5usLDxMXGx8jJytLT1NXW19jZ2uHi4+Tl5ufo6erx8vP09fb3+Pn6/8QAHwEAAwEBAQEBAQEBAQAAAAAAAAECAwQFBgcICQoL/8QAtREAAgECBAQDBAcFBAQAAQJ3AAECAxEEBSExBhJBUQdhcRMiMoEIFEKRobHBCSMzUvAVYnLRChYkNOEl8RcYGRomJygpKjU2Nzg5OkNERUZHSElKU1RVVldYWVpjZGVmZ2hpanN0dXZ3eHl6goOEhYaHiImKkpOUlZaXmJmaoqOkpaanqKmqsrO0tba3uLm6wsPExcbHyMnK0tPU1dbX2Nna4uPk5ebn6Onq8vP09fb3+Pn6/9oADAMBAAIRAxEAPwClRRRX9kH8nhRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFAH//2Q=="))
[IO.File]::WriteAllBytes((Join-Path $dir "real.gif"),
    [Convert]::FromBase64String("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"))
# PNG truncado: cabecera valida pero sin marca IEND de fin (debe dar 400 "corrupto")
[IO.File]::WriteAllBytes((Join-Path $dir "corrupt.png"),
    [Convert]::FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAYAAACqaXHeAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAACKSURBVHhe7dAhAQBADIDAJfnM33jzVABxBsm8v2s2DDYNYLBpAINNAxhsGsBg0wAGmwYw2DSAwaYBDDYNYLBpAINNAxhsGsBg0wAGmwYw2DSAwaYBDDYNYLBpAINNAxhsGsBg0wAGmwYw2DSAwaYBDDYNYLBpAINNAxhsGsBg0wAGmwYw2DSAwaYBDDYNYLBpAIM="))
# Texto plano con extension .png (debe dar 400 "firma no reconocida")
[IO.File]::WriteAllBytes((Join-Path $dir "fake.png"),
    [Convert]::FromBase64String("ZXN0byBubyBlcyB1bmEgaW1hZ2VuLCBzb2xvIHRleHRvIHBsYW5vIGNvbiBleHRlbnNpb24gcG5n"))
# 6 MB: firma PNG + relleno de ceros (debe dar 413 por tamano)
$sig = [byte[]](0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A)
$big = New-Object byte[] (6 * 1024 * 1024)
[Array]::Copy($sig, $big, 8)
[IO.File]::WriteAllBytes((Join-Path $dir "big.png"), $big)

Write-Host "Archivos generados en $dir"
Get-ChildItem $dir -File | Format-Table Name, Length -AutoSize
