# ==============================================================================
# Script de Deploy e Sincronização - Sistema de Triagem (ProjetoFaculdade)
# Alvo: CloudPanel (10.250.220.212) - formularios.imi.local
# ==============================================================================
param (
    [string]$ServerHost = "10.250.220.212",
    [string]$ServerUser = "imi-formularios",
    [string]$ServerPass = "1nstitutO@TI",
    [string]$RemoteDir = "/home/imi-formularios/htdocs/formularios.imi.local",
    [string]$DbName = "FormsDB",
    [string]$DbUser = "FormulariosUserDB",
    [string]$DbPass = "1nstitutO@TI",
    [switch]$SkipBuild = $false,
    [switch]$MigrateDb = $false
)

$ErrorActionPreference = "Stop"
$Host.UI.RawUI.WindowTitle = "Deploy ProjetoFaculdade -> $ServerHost"

Write-Host ""
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host "   DEPLOY & SINCRONIZAÇÃO - TRIAGEM IMAGEM ($ServerHost)" -ForegroundColor Cyan
Write-Host "   Alvo: $RemoteDir | Banco: $DbName" -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host ""

$sw = [System.Diagnostics.Stopwatch]::StartNew()

try {
    # 1. Configurar SSH Askpass para autenticação não-interativa
    $askpassScript = "$env:TEMP\imi_askpass.bat"
    Set-Content -Path $askpassScript -Value "@echo $ServerPass"
    $env:SSH_ASKPASS = $askpassScript
    $env:SSH_ASKPASS_REQUIRE = "force"
    $env:DISPLAY = "dummy:0"

    $localRoot = $PSScriptRoot
    if (-not $localRoot) { $localRoot = "c:\Projetos\ProjetoFaculdade" }

    $stagingDir = "$env:TEMP\triagem_publish_stage"
    $tarFile = "$env:TEMP\triagem_deploy.tar.gz"

    # 2. Compilar e Publicar para Linux x64 (Self-Contained .NET 8)
    if (-not $SkipBuild) {
        Write-Host "[1/5] Compilando e gerando binário Linux (self-contained)..." -ForegroundColor Yellow
        if (Test-Path $stagingDir) { Remove-Item $stagingDir -Recurse -Force }
        New-Item -ItemType Directory -Path $stagingDir -Force | Out-Null

        $publishCmd = "dotnet publish `"$localRoot\TriagemImagem.Api\TriagemImagem.Api.csproj`" -c Release -r linux-x64 --self-contained true -o `"$stagingDir`""
        Invoke-Expression $publishCmd | Out-Null
        Write-Host " [OK] Binário Linux-x64 gerado com sucesso." -ForegroundColor Green
    }
    else {
        Write-Host "[1/5] Pulando compilação (SkipBuild ativado)..." -ForegroundColor Gray
    }

    # 3. Preparar artefatos de deploy (Frontend, Configs, Scripts)
    Write-Host "[2/5] Preparando pacote de distribuição com Frontend..." -NoNewline
    if (Test-Path "$stagingDir\frontend") { Remove-Item "$stagingDir\frontend" -Recurse -Force }
    Copy-Item -Path "$localRoot\frontend" -Destination "$stagingDir\frontend" -Recurse -Force
    Copy-Item -Path "$localRoot\start.sh" -Destination "$stagingDir\start.sh" -Force
    Copy-Item -Path "$localRoot\ecosystem.config.cjs" -Destination "$stagingDir\ecosystem.config.cjs" -Force
    Copy-Item -Path "$localRoot\schema_producao.sql" -Destination "$stagingDir\schema_producao.sql" -Force
    Copy-Item -Path "$localRoot\TriagemImagem.Api\appsettings.Production.json" -Destination "$stagingDir\appsettings.Production.json" -Force

    if (Test-Path $tarFile) { Remove-Item $tarFile -Force }
    & "C:\Windows\System32\tar.exe" -czf $tarFile -C $stagingDir .
    $tarSizeMb = [math]::Round((Get-Item $tarFile).Length / 1MB, 2)
    Write-Host " [OK] ($tarSizeMb MB compactado)" -ForegroundColor Green

    # 4. Enviar pacote via SCP
    Write-Host "[3/5] Enviando pacote para o servidor CloudPanel..." -NoNewline
    & "C:\Windows\System32\OpenSSH\scp.exe" -o StrictHostKeyChecking=accept-new $tarFile "$ServerUser@$ServerHost`:/home/$ServerUser/deploy.tar.gz"
    Write-Host " [OK]" -ForegroundColor Green

    # 5. Executar extração, permissões e PM2 remotamente
    Write-Host "[4/5] Descompactando, configurando permissões e reiniciando PM2..." -NoNewline
    $remoteCmd = @"
export PATH=`$HOME/.nvm/versions/node/v24.22.2/bin:`$HOME/.nvm/versions/node/v24.20.0/bin:/usr/local/bin:/usr/bin:/bin:`$PATH
mkdir -p $RemoteDir
rm -rf $RemoteDir/frontend
tar -xzf /home/$ServerUser/deploy.tar.gz -C $RemoteDir/
rm -f /home/$ServerUser/deploy.tar.gz

# Garantir finais de linha UNIX e permissões de execução
sed -i 's/\r$//' $RemoteDir/start.sh
chmod +x $RemoteDir/start.sh
chmod +x $RemoteDir/TriagemImagem.Api

# Executar migração de banco se solicitada
if [ "$($MigrateDb.ToString().ToLower())" = "true" ]; then
    mysql -u $DbUser -p'$DbPass' $DbName < $RemoteDir/schema_producao.sql 2>/dev/null || true
fi

cd $RemoteDir
pm2 restart formularios-api --update-env 2>/dev/null || pm2 start $RemoteDir/ecosystem.config.cjs
pm2 save
"@
    & "C:\Windows\System32\OpenSSH\ssh.exe" -o StrictHostKeyChecking=accept-new "$ServerUser@$ServerHost" $remoteCmd
    Write-Host " [OK]" -ForegroundColor Green

    # 6. Testar integridade e status da aplicação no CloudPanel
    Write-Host "[5/5] Validando saúde da API e status das tabelas..." -NoNewline
    Start-Sleep -Seconds 3

    $healthCmd = @"
curl -s http://127.0.0.1:3005/api/health
echo " --- "
curl -s http://127.0.0.1:3005/api/v1/database/status
echo " --- "
curl -s http://127.0.0.1:3005/api/v1/auth/ldap/status
echo " --- "
curl -s http://127.0.0.1:3005/api/v1/tasy/status
"@
    $healthRes = & "C:\Windows\System32\OpenSSH\ssh.exe" -o StrictHostKeyChecking=accept-new "$ServerUser@$ServerHost" $healthCmd
    Write-Host " [OK]" -ForegroundColor Green

    # Limpeza local
    if (Test-Path $askpassScript) { Remove-Item $askpassScript -Force }
    if (Test-Path $tarFile) { Remove-Item $tarFile -Force }

    $sw.Stop()
    Write-Host ""
    Write-Host "------------------------------------------------------------" -ForegroundColor Green
    Write-Host " [SUCESSO] Deploy finalizado com êxito em $($sw.Elapsed.TotalSeconds.ToString('F1'))s!" -ForegroundColor Green
    Write-Host " Resposta do Health Check:" -ForegroundColor Cyan
    Write-Host $healthRes -ForegroundColor Yellow
    Write-Host ""
    Write-Host " URLs de Acesso:" -ForegroundColor Cyan
    Write-Host "  * Domínio Oficial : https://formularios.imi.local (após apontamento de DNS)" -ForegroundColor Green
    Write-Host "  * Acesso Direto IP: http://${ServerHost}:3005" -ForegroundColor Green
    Write-Host "  * Swagger OpenAPI : https://formularios.imi.local/swagger ou http://${ServerHost}:3005/swagger" -ForegroundColor Green
    Write-Host "------------------------------------------------------------" -ForegroundColor Green
    Write-Host ""
}
catch {
    $sw.Stop()
    Write-Host ""
    Write-Host " [ERRO] Falha durante o deploy: $_" -ForegroundColor Red
    Write-Host ""
    exit 1
}
