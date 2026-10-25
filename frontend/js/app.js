/**
 * SISTEMA DE TRIAGEM PARA DIAGNÓSTICO POR IMAGEM
 * Frontend - Lógica de Apresentação e Ergonomia Operacional
 */

// Dados Locais Iniciais para Demonstração do Frontend (Passo 2)
const EXAMES_INICIAIS = [
  {
    id: 1,
    atendimento: 104520,
    pacienteNome: 'João Carlos de Oliveira',
    documentoCpf: '12345678901',
    idade: 51,
    sexo: 'M',
    pesoKg: 78.5,
    modalidade: 'TC',
    procedimento: 'Tomografia Computadorizada de Tórax com Contraste',
    sala: 'TC-01',
    horario: '08:30',
    status: 'AGUARDANDO_TRIAGEM'
  },
  {
    id: 2,
    atendimento: 104521,
    pacienteNome: 'Maria Aparecida Souza',
    documentoCpf: '98765432100',
    idade: 64,
    sexo: 'F',
    pesoKg: 64.0,
    modalidade: 'RM',
    procedimento: 'Ressonância Magnética de Crânio com Contraste',
    sala: 'RM-01',
    horario: '09:00',
    status: 'AGUARDANDO_TRIAGEM'
  },
  {
    id: 3,
    atendimento: 104522,
    pacienteNome: 'Carlos Eduardo Mendes',
    documentoCpf: '45678912344',
    idade: 38,
    sexo: 'M',
    pesoKg: 82.0,
    modalidade: 'TC',
    procedimento: 'Tomografia Computadorizada de Abdômen Total com Contraste',
    sala: 'TC-02',
    horario: '09:40',
    status: 'AGUARDANDO_TRIAGEM'
  },
  {
    id: 4,
    atendimento: 104523,
    pacienteNome: 'Fernanda Lima Rocha',
    documentoCpf: '78912345688',
    idade: 31,
    sexo: 'F',
    pesoKg: 58.0,
    modalidade: 'RM',
    procedimento: 'Ressonância Magnética de Coluna Lombar',
    sala: 'RM-02',
    horario: '10:15',
    status: 'AGUARDANDO_TRIAGEM'
  }
];

// Perguntas do Checklist por Modalidade
const PROTOCOLOS_CHECKLIST = {
  TC: [
    { codigo: 'TC_ALERGIA_IODO', enunciado: 'Histórico prévio de anafilaxia ou choque alérgico grave ao contraste iodado?', impeditivo: true },
    { codigo: 'TC_INSUFICIENCIA_RENAL', enunciado: 'Diagnóstico prévio de doença renal crônica ou diálise?', impeditivo: false },
    { codigo: 'TC_METFORMINA', enunciado: 'Faz uso contínuo de Cloridrato de Metformina?', impeditivo: false },
    { codigo: 'TC_JEJUM', enunciado: 'Cumpriu o jejum mínimo de 4 horas orientado?', impeditivo: false },
    { codigo: 'TC_GESTAO', enunciado: 'Possibilidade de gravidez ativa no momento?', impeditivo: true }
  ],
  RM: [
    { codigo: 'RM_MARCAPASSO', enunciado: 'Possui marcapasso cardíaco ou cardioversor-desfibrilador (CDI)?', impeditivo: true },
    { codigo: 'RM_CLIPE_ANEURISMA', enunciado: 'Possui clipes de aneurisma cerebral ou implantes vasculares?', impeditivo: true },
    { codigo: 'RM_IMPLANTE_COCLEAR', enunciado: 'Possui implante coclear ou neuroestimulador implantado?', impeditivo: true },
    { codigo: 'RM_CORPO_ESTRANHO_OLHO', enunciado: 'Já trabalhou com solda/metal ou teve fragmento metálico ocular?', impeditivo: true },
    { codigo: 'RM_PROTESE_METALICA', enunciado: 'Possui próteses metálicas, pinos ou placas cirúrgicas?', impeditivo: false },
    { codigo: 'RM_CLAUSTROFOBIA', enunciado: 'Apresenta claustrofobia importante ao entrar em tubo fechado?', impeditivo: false }
  ]
};

// Estado da Interface
const UIState = {
  exames: [...EXAMES_INICIAIS],
  exameSelecionado: null,
  filtroModalidade: 'TODOS',
  buscaTexto: '',
  respostas: {}
};

// Inicialização
document.addEventListener('DOMContentLoaded', () => {
  initTema();
  initAutenticacao();
  initNavegacao();
  initWorklist();
  initCalculadora();
  initAtalhosTeclado();
});

/* ==============================================================================
   1. TEMA (DARK / LIGHT)
   ============================================================================== */
function initTema() {
  const salvo = localStorage.getItem('app_theme') || 'dark';
  aplicarTema(salvo);

  const btns = [
    document.getElementById('btnLoginThemeToggle'),
    document.getElementById('btnMainThemeToggle')
  ];

  btns.forEach(btn => {
    btn?.addEventListener('click', () => {
      const atual = document.documentElement.getAttribute('data-theme') || 'dark';
      aplicarTema(atual === 'dark' ? 'light' : 'dark');
    });
  });
}

function aplicarTema(tema) {
  document.documentElement.setAttribute('data-theme', tema);
  localStorage.setItem('app_theme', tema);

  const icones = [
    document.getElementById('loginThemeIcon'),
    document.getElementById('mainThemeIcon')
  ];

  icones.forEach(i => {
    if (i) i.className = tema === 'dark' ? 'fa-solid fa-moon' : 'fa-solid fa-sun';
  });
}

/* ==============================================================================
   2. LOGIN E NAVEGAÇÃO
   ============================================================================== */
function initAutenticacao() {
  const form = document.getElementById('form-login');
  form?.addEventListener('submit', (e) => {
    e.preventDefault();
    const user = document.getElementById('login-username').value.trim() || 'Operador Clínico';
    const perfil = user.includes('admin') ? 'ADMIN' : (user.includes('tec') ? 'TÉCNICO' : 'BIOMÉDICO');

    entrar(user, perfil);
  });

  document.getElementById('btnLogout')?.addEventListener('click', sair);

  // Toggle senha
  document.getElementById('btnToggleLoginPwd')?.addEventListener('click', () => {
    const input = document.getElementById('login-password');
    const isPwd = input.type === 'password';
    input.type = isPwd ? 'text' : 'password';
  });
}

function quickFillLogin(user, pwd) {
  document.getElementById('login-username').value = user;
  document.getElementById('login-password').value = pwd;
}

function entrar(nome, perfil) {
  document.getElementById('login-screen').classList.add('hidden');
  document.getElementById('app-layout').classList.remove('hidden');

  document.getElementById('sidebarUserName').textContent = nome;
  document.getElementById('sidebarUserRole').textContent = perfil;

  showToast(`Sessão iniciada como ${nome} (${perfil})`, 'success');
  renderizarWorklist();
}

function sair() {
  document.getElementById('app-layout').classList.add('hidden');
  document.getElementById('login-screen').classList.remove('hidden');
}

function initNavegacao() {
  document.querySelectorAll('.sidebar-nav-item').forEach(btn => {
    btn.addEventListener('click', () => {
      const view = btn.getAttribute('data-view');
      if (view) trocarView(view);
    });
  });

  document.getElementById('btnVoltarWorklist')?.addEventListener('click', () => {
    trocarView('view-worklist');
  });

  document.getElementById('btnRecarregarAuditoria')?.addEventListener('click', () => {
    showToast('Registros de auditoria atualizados.', 'info');
  });
}

function trocarView(viewId) {
  document.querySelectorAll('.view-panel').forEach(v => v.classList.add('hidden'));
  document.getElementById(viewId)?.classList.remove('hidden');

  document.querySelectorAll('.sidebar-nav-item').forEach(btn => {
    btn.classList.toggle('active', btn.getAttribute('data-view') === viewId);
  });

  const titulos = {
    'view-worklist': 'Grade de Atendimentos do Dia (Worklist)',
    'view-triagem': 'Ficha de Triagem & Checklist de Segurança',
    'view-auditoria': 'Histórico Inviolável de Auditoria Clínica'
  };

  document.getElementById('topbarTitle').textContent = titulos[viewId] || 'Posto de Triagem';
}

/* ==============================================================================
   3. GRADE DE ATENDIMENTOS (WORKLIST)
   ============================================================================== */
function initWorklist() {
  document.getElementById('inputSearchWorklist')?.addEventListener('input', (e) => {
    UIState.buscaTexto = e.target.value.toLowerCase().trim();
    renderizarWorklist();
  });

  document.querySelectorAll('.filter-chip').forEach(chip => {
    chip.addEventListener('click', () => {
      document.querySelectorAll('.filter-chip').forEach(c => c.classList.remove('active'));
      chip.classList.add('active');
      UIState.filtroModalidade = chip.getAttribute('data-filter');
      renderizarWorklist();
    });
  });

  renderizarWorklist();
}

function renderizarWorklist() {
  const tbody = document.getElementById('worklistTableBody');
  if (!tbody) return;

  const filtrados = UIState.exames.filter(item => {
    if (UIState.filtroModalidade === 'TC' && item.modalidade !== 'TC') return false;
    if (UIState.filtroModalidade === 'RM' && item.modalidade !== 'RM') return false;
    if (UIState.filtroModalidade === 'AGUARDANDO_TRIAGEM' && item.status !== 'AGUARDANDO_TRIAGEM') return false;
    if (UIState.filtroModalidade === 'LIBERADO' && item.status !== 'LIBERADO') return false;

    if (UIState.buscaTexto) {
      const busca = UIState.buscaTexto;
      const match = item.pacienteNome.toLowerCase().includes(busca)
                 || item.documentoCpf.includes(busca)
                 || item.atendimento.toString().includes(busca)
                 || item.sala.toLowerCase().includes(busca);
      if (!match) return false;
    }
    return true;
  });

  if (filtrados.length === 0) {
    tbody.innerHTML = `
      <tr>
        <td colspan="8" style="text-align: center; padding: 32px; color: var(--text-muted);">
          Nenhum agendamento encontrado para o filtro aplicado.
        </td>
      </tr>
    `;
    return;
  }

  tbody.innerHTML = filtrados.map(item => {
    const isLiberado = item.status === 'LIBERADO';
    const isBloqueado = item.status === 'BLOQUEADO_SEGURANCA';

    const statusBadge = isLiberado
      ? '<span class="badge-status status-liberado"><i class="fa-solid fa-check-circle"></i> Liberado</span>'
      : (isBloqueado
        ? '<span class="badge-status status-bloqueado"><i class="fa-solid fa-ban"></i> Bloqueio</span>'
        : '<span class="badge-status status-aguardando"><i class="fa-solid fa-clock"></i> Aguardando</span>');

    const modBadgeClass = item.modalidade === 'TC' ? 'badge-tc' : 'badge-rm';

    return `
      <tr>
        <td><strong>${item.horario}</strong></td>
        <td><span style="font-family: var(--font-mono); font-weight: 700;">#${item.atendimento}</span></td>
        <td>
          <div class="paciente-col">
            <span class="paciente-nome">${item.pacienteNome}</span>
            <span class="paciente-doc">CPF: ${item.documentoCpf} | ${item.idade} anos</span>
          </div>
        </td>
        <td><span class="badge-modalidade ${modBadgeClass}">${item.modalidade}</span></td>
        <td><span style="font-size: 0.88rem;">${item.procedimento}</span></td>
        <td><span style="font-family: var(--font-mono);">${item.sala}</span></td>
        <td>${statusBadge}</td>
        <td style="text-align: right;">
          <button type="button" class="btn-action btn-iniciar-triagem" onclick="abrirTriagem(${item.id})">
            <i class="fa-solid fa-clipboard-check"></i> ${isLiberado ? 'Ver Ficha' : 'Iniciar Triagem'}
          </button>
        </td>
      </tr>
    `;
  }).join('');

  // Atualiza contadores
  const aguardando = UIState.exames.filter(e => e.status === 'AGUARDANDO_TRIAGEM').length;
  const liberados = UIState.exames.filter(e => e.status === 'LIBERADO').length;
  const bloqueados = UIState.exames.filter(e => e.status === 'BLOQUEADO_SEGURANCA').length;

  document.getElementById('statTotal').textContent = UIState.exames.length;
  document.getElementById('statAguardando').textContent = aguardando;
  document.getElementById('statLiberados').textContent = liberados;
  document.getElementById('statBloqueados').textContent = bloqueados;
  document.getElementById('badgeWorklistCount').textContent = aguardando;
}

/* ==============================================================================
   4. FICHA DE TRIAGEM & SEMÁFORO CLÍNICO
   ============================================================================== */
function abrirTriagem(exameId) {
  const item = UIState.exames.find(e => e.id === exameId);
  if (!item) return;

  UIState.exameSelecionado = item;
  UIState.respostas = {};

  // Preenche dados do paciente
  document.getElementById('triagemPacienteNome').textContent = item.pacienteNome;
  document.getElementById('triagemAtendimentoNum').textContent = item.atendimento;
  document.getElementById('triagemCpf').textContent = item.documentoCpf;
  document.getElementById('triagemIdade').textContent = `${item.idade} anos`;
  document.getElementById('triagemSexo').textContent = item.sexo === 'M' ? 'Masculino' : 'Feminino';
  document.getElementById('triagemProcedimento').textContent = item.procedimento;
  document.getElementById('triagemSala').textContent = item.sala;

  const badgeMod = document.getElementById('triagemModalidadeBadge');
  badgeMod.textContent = item.modalidade;
  badgeMod.className = `badge-modalidade ${item.modalidade === 'TC' ? 'badge-tc' : 'badge-rm'}`;

  // Calculadora
  document.getElementById('calcPesoInput').value = item.pesoKg || 70.0;
  document.getElementById('calcCreatininaInput').value = '';
  recalcularDose();

  // Renderiza checklist da modalidade
  const perguntas = PROTOCOLOS_CHECKLIST[item.modalidade] || PROTOCOLOS_CHECKLIST.TC;
  document.getElementById('checklistProtocoloTitulo').textContent = 
    item.modalidade === 'TC' 
      ? 'Checklist de Segurança - Tomografia Computadorizada (TC)' 
      : 'Checklist de Segurança - Ressonância Magnética (RM)';

  const container = document.getElementById('checklistItemsContainer');
  container.innerHTML = perguntas.map((p, index) => {
    return `
      <div class="checklist-card" id="card-${p.codigo}" tabindex="0" data-codigo="${p.codigo}">
        <div class="question-text">
          <div class="question-enunciado">
            <span style="color: var(--cyan-400); font-weight: 700; margin-right: 6px;">[Q${index + 1}]</span>
            ${p.enunciado}
          </div>
          ${p.impeditivo ? '<div class="question-alerta"><i class="fa-solid fa-triangle-exclamation"></i> Item impeditivo de segurança clínica (Bloqueio Absoluto)</div>' : ''}
        </div>

        <div class="btn-choice-group">
          <button type="button" class="btn-choice" id="btn-nao-${p.codigo}" onclick="definirResposta('${p.codigo}', 'NAO')">
            <i class="fa-solid fa-check"></i> NÃO
          </button>
          <button type="button" class="btn-choice" id="btn-sim-${p.codigo}" onclick="definirResposta('${p.codigo}', 'SIM')">
            <i class="fa-solid fa-xmark"></i> SIM
          </button>
        </div>
      </div>
    `;
  }).join('');

  // Resposta padrão: NÃO
  perguntas.forEach(p => definirResposta(p.codigo, 'NAO', false));
  avaliarSemaforo();

  document.getElementById('navTriagem').removeAttribute('disabled');
  trocarView('view-triagem');
}

function definirResposta(codigo, valor, dispararAvaliacao = true) {
  UIState.respostas[codigo] = valor;

  const btnNao = document.getElementById(`btn-nao-${codigo}`);
  const btnSim = document.getElementById(`btn-sim-${codigo}`);
  const card = document.getElementById(`card-${codigo}`);

  if (!btnNao || !btnSim || !card) return;

  btnNao.classList.remove('active-nao');
  btnSim.classList.remove('active-sim-danger', 'active-sim-warning');
  card.classList.remove('is-danger', 'is-warning');

  const modalidade = UIState.exameSelecionado?.modalidade || 'TC';
  const pergunta = (PROTOCOLOS_CHECKLIST[modalidade] || []).find(p => p.codigo === codigo);
  const isImpeditivo = pergunta ? pergunta.impeditivo : false;

  if (valor === 'NAO') {
    btnNao.classList.add('active-nao');
  } else if (valor === 'SIM') {
    if (isImpeditivo) {
      btnSim.classList.add('active-sim-danger');
      card.classList.add('is-danger');
    } else {
      btnSim.classList.add('active-sim-warning');
      card.classList.add('is-warning');
    }
  }

  if (dispararAvaliacao) avaliarSemaforo();
}

function avaliarSemaforo() {
  const banner = document.getElementById('semaforoBanner');
  const orb = document.getElementById('semaforoOrb');
  const titulo = document.getElementById('semaforoTitulo');
  const descricao = document.getElementById('semaforoDescricao');
  const badge = document.getElementById('semaforoBadgeState');
  const btnLiberar = document.getElementById('btnLiberarExame');

  const modalidade = UIState.exameSelecionado?.modalidade || 'TC';
  const perguntas = PROTOCOLOS_CHECKLIST[modalidade] || [];

  let temBloqueio = false;
  let temAtencao = false;

  perguntas.forEach(p => {
    if (UIState.respostas[p.codigo] === 'SIM') {
      if (p.impeditivo) temBloqueio = true;
      else temAtencao = true;
    }
  });

  banner.classList.remove('semaforo-verde', 'semaforo-amarelo', 'semaforo-vermelho');

  if (temBloqueio) {
    banner.classList.add('semaforo-vermelho');
    orb.innerHTML = '<i class="fa-solid fa-ban"></i>';
    titulo.textContent = 'SEMÁFORO CLÍNICO: BLOQUEIO DE SEGURANÇA MANDATÓRIO';
    descricao.textContent = 'Contraindicação impeditiva ativa no Checklist. Liberação impedida por protocolo de aviação.';
    badge.textContent = 'BLOQUEIO VERMELHO';
    btnLiberar.setAttribute('disabled', 'true');
    btnLiberar.innerHTML = '<i class="fa-solid fa-lock"></i> Liberação Bloqueada (Risco)';
  } else if (temAtencao) {
    banner.classList.add('semaforo-amarelo');
    orb.innerHTML = '<i class="fa-solid fa-triangle-exclamation"></i>';
    titulo.textContent = 'SEMÁFORO CLÍNICO: ATENÇÃO CLÍNICA REQUERIDA';
    descricao.textContent = 'Alertas moderados identificados. Avaliar protocolo de hidratação ou precauções.';
    badge.textContent = 'ALERTA AMARELO';
    btnLiberar.removeAttribute('disabled');
    btnLiberar.innerHTML = '<i class="fa-solid fa-shield-check"></i> Liberar Exame com Segurança';
  } else {
    banner.classList.add('semaforo-verde');
    orb.innerHTML = '<i class="fa-solid fa-check"></i>';
    titulo.textContent = 'SEMÁFORO CLÍNICO: NENHUM IMPEDIMENTO DETECTADO';
    descricao.textContent = 'Todos os critérios de segurança validados. O exame está apto para liberação operacional.';
    badge.textContent = 'LIBERADO VERDE';
    btnLiberar.removeAttribute('disabled');
    btnLiberar.innerHTML = '<i class="fa-solid fa-shield-check"></i> Liberar Exame com Segurança';
  }
}

/* ==============================================================================
   5. CALCULADORA DE CONTRASTE
   ============================================================================== */
function initCalculadora() {
  document.getElementById('calcPesoInput')?.addEventListener('input', recalcularDose);
  document.getElementById('calcCreatininaInput')?.addEventListener('input', recalcularDose);

  document.getElementById('btnSalvarRascunho')?.addEventListener('click', () => {
    showToast('Rascunho da triagem salvo.', 'success');
  });

  document.getElementById('btnLiberarExame')?.addEventListener('click', () => {
    if (!UIState.exameSelecionado) return;

    UIState.exameSelecionado.status = 'LIBERADO';
    showToast(`Exame #${UIState.exameSelecionado.atendimento} liberado com sucesso!`, 'success');
    renderizarWorklist();
    trocarView('view-worklist');
  });
}

function recalcularDose() {
  const peso = parseFloat(document.getElementById('calcPesoInput').value) || 0;
  const isTc = UIState.exameSelecionado?.modalidade === 'TC';

  const proporcao = isTc ? 1.5 : 0.2;
  let dose = Math.round(peso * proporcao * 10) / 10;
  const teto = isTc ? 130 : 25;
  if (dose > teto) dose = teto;

  document.getElementById('calcDoseResultado').textContent = dose > 0 ? dose.toFixed(1) : '0.0';
  document.getElementById('calcDoseDetalhes').textContent = 
    `Proporção clínica recomendada: ${proporcao} mL / kg (${isTc ? 'Contraste Iodado' : 'Gadolínio'}).`;
}

/* ==============================================================================
   6. ATALHOS DE TECLADO (1 = NÃO, 2 = SIM, TAB = AVANÇAR)
   ============================================================================== */
function initAtalhosTeclado() {
  window.addEventListener('keydown', (e) => {
    const view = document.getElementById('view-triagem');
    if (!view || view.classList.contains('hidden')) return;

    const ativo = document.activeElement;
    if (ativo && (ativo.tagName === 'INPUT' || ativo.tagName === 'TEXTAREA')) return;

    const cards = Array.from(document.querySelectorAll('.checklist-card'));
    let atual = cards.find(c => c === ativo || c.contains(ativo));
    if (!atual && cards.length > 0) atual = cards[0];
    if (!atual) return;

    const codigo = atual.getAttribute('data-codigo');

    if (e.key === '1') {
      e.preventDefault();
      definirResposta(codigo, 'NAO');
      avancarPergunta(cards, atual);
    } else if (e.key === '2') {
      e.preventDefault();
      definirResposta(codigo, 'SIM');
      avancarPergunta(cards, atual);
    }
  });
}

function avancarPergunta(cards, atual) {
  const i = cards.indexOf(atual);
  if (i >= 0 && i < cards.length - 1) cards[i + 1].focus();
}

// Funções de Toast
function showToast(mensagem, tipo = 'info') {
  const container = document.getElementById('toast-container');
  if (!container) return;

  const t = document.createElement('div');
  t.className = `toast toast-${tipo}`;
  t.innerHTML = `<span>${mensagem}</span>`;
  container.appendChild(t);

  setTimeout(() => {
    t.style.opacity = '0';
    setTimeout(() => t.remove(), 300);
  }, 3500);
}
