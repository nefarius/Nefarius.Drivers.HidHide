$content = Get-Content -Path "..\README.md"
$content = $content -replace '<img[^>]*>', ''
Set-Content -Path "README_temp.md" -Value $content