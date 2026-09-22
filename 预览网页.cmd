@echo off
chcp 65001 >nul
setlocal
where node >nul 2>&1
if errorlevel 1 (
    echo 未找到 Node.js。请安装 Node.js 22 或更新版本，然后重新打开本文件。
    pause
    exit /b 1
)
node "%~dp0scripts\preview-site.mjs" --open
if errorlevel 1 pause
