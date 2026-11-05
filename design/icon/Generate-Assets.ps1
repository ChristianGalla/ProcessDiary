$ErrorActionPreference = 'Stop'
$gimp = Join-Path $env:LOCALAPPDATA 'Programs\GIMP 3\bin\gimp-console-3.exe'
if (-not (Test-Path $gimp)) { throw "GIMP 3 console not found at $gimp" }

$env:PD_ICON_DIR = $PSScriptRoot
$script = Join-Path $PSScriptRoot 'render_assets.py'
$code = "exec(open(r'$script', encoding='utf-8').read())"

& $gimp --no-interface --batch-interpreter python-fu-eval --batch $code --quit
