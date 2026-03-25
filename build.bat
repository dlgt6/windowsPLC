@echo off
echo ========================================
echo 自动化计算工具 - 构建脚本
echo ========================================
echo.

echo [1/4] 清理之前的构建...
dotnet clean
if %errorlevel% neq 0 (
    echo 清理失败！
    pause
    exit /b 1
)

echo [2/4] 恢复依赖...
dotnet restore
if %errorlevel% neq 0 (
    echo 依赖恢复失败！
    pause
    exit /b 1
)

echo [3/4] 构建项目...
dotnet build --configuration Release
if %errorlevel% neq 0 (
    echo 构建失败！
    pause
    exit /b 1
)

echo [4/4] 发布为单文件可执行程序...
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -p:PublishTrimmed=true -p:TrimMode=link -p:PublishReadyToRun=true -o ./publish
if %errorlevel% neq 0 (
    echo 发布失败！
    pause
    exit /b 1
)

echo.
echo ========================================
echo 构建成功！
echo ========================================
echo.
echo 可执行文件位置: .\publish\自动化计算工具.exe
echo.

if exist ".\publish\自动化计算工具.exe" (
    for %%I in (".\publish\自动化计算工具.exe") do echo 文件大小: %%~zI 字节
)

echo.
echo 按任意键运行程序...
pause > nul

start "" ".\publish\自动化计算工具.exe"

echo.
echo 程序已启动！
pause
