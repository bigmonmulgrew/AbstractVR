New-Item -ItemType Directory -Path "Logs" -Force | Out-Null
adb logcat -v threadtime -s Unity > Logs/log_$(Get-Date -Format "yyyy-MM-dd_HH-mm-ss").txt