/**
 * SISTEMA DE TRIAGEM PARA DIAGNÓSTICO POR IMAGEM
 * Estrutura Base do Frontend (Passo 1)
 */

document.addEventListener('DOMContentLoaded', () => {
  initTheme();
});

function initTheme() {
  const saved = localStorage.getItem('app_theme') || 'dark';
  applyTheme(saved);

  const btns = [
    document.getElementById('btnToggleTheme'),
    document.getElementById('btnToggleThemeTop')
  ];

  btns.forEach(btn => {
    btn?.addEventListener('click', () => {
      const current = document.documentElement.getAttribute('data-theme') || 'dark';
      const next = current === 'dark' ? 'light' : 'dark';
      applyTheme(next);
    });
  });
}

function applyTheme(theme) {
  document.documentElement.setAttribute('data-theme', theme);
  localStorage.setItem('app_theme', theme);

  const icon = document.getElementById('themeIcon');
  if (icon) {
    icon.className = theme === 'dark' ? 'fa-solid fa-moon' : 'fa-solid fa-sun';
  }
}

function acessarSistema() {
  document.getElementById('login-screen').classList.add('hidden');
  document.getElementById('app-shell').classList.remove('hidden');
}

function sairSistema() {
  document.getElementById('app-shell').classList.add('hidden');
  document.getElementById('login-screen').classList.remove('hidden');
}
