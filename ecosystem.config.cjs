module.exports = {
  apps: [
    {
      name: 'formularios-api',
      script: './start.sh',
      cwd: '/home/imi-formularios/htdocs/formularios.imi.local',
      interpreter: '/bin/bash',
      autorestart: true,
      watch: false,
      max_memory_restart: '500M',
      env: {
        ASPNETCORE_ENVIRONMENT: 'Production',
        ASPNETCORE_URLS: 'http://0.0.0.0:3005'
      }
    }
  ]
};
