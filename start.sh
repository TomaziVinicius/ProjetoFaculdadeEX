#!/usr/bin/env bash
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

export ASPNETCORE_ENVIRONMENT=Production
export ASPNETCORE_URLS="http://0.0.0.0:3005"
export DOTNET_CLI_TELEMETRY_OPTOUT=1

chmod +x ./TriagemImagem.Api
exec ./TriagemImagem.Api --urls "http://0.0.0.0:3005"
