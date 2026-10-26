/**
 * SISTEMA DE TRIAGEM PARA DIAGNÓSTICO POR IMAGEM
 * Frontend - Lógica de Apresentação, Ergonomia Operacional e Integração HTTP REST
 */

// Dados Locais de Contingência (Fallback para apresentação sem o backend rodando)
const EXAMES_INICIAIS = [
  {
    id: 1,
    atendimento: 104520,
    pacienteNome: 'Fernanda Lima Rocha',
    documentoCpf: '78912345688',
    idade: 31,
    sexo: 'F',
    pesoKg: 58.0,
    modalidade: 'RM',
    tipoFormulario: 'RM_COLUNA',
    segmentoLado: 'Lombar',
    procedimento: 'Ressonância Magnética de Coluna Lombar',
    sala: 'RM-01',
    horario: '08:00',
    status: 'AGUARDANDO_TRIAGEM'
  },
  {
    id: 2,
    atendimento: 104521,
    pacienteNome: 'Carlos Eduardo Mendes',
    documentoCpf: '45678912344',
    idade: 38,
    sexo: 'M',
    pesoKg: 82.0,
    modalidade: 'RM',
    tipoFormulario: 'RM_BRACO_ANTEBRACO',
    segmentoLado: 'Direito',
    procedimento: 'Ressonância Magnética de Braço e Antebraço',
    sala: 'RM-02',
    horario: '08:45',
    status: 'AGUARDANDO_TRIAGEM'
  },
  {
    id: 3,
    atendimento: 104522,
    pacienteNome: 'Maria Aparecida Souza',
    documentoCpf: '98765432100',
    idade: 64,
    sexo: 'F',
    pesoKg: 64.0,
    modalidade: 'RM',
    tipoFormulario: 'RM_BACIA_QUADRIL',
    segmentoLado: 'Esquerdo',
    procedimento: 'Ressonância Magnética de Bacia e Articulação do Quadril',
    sala: 'RM-01',
    horario: '09:30',
    status: 'AGUARDANDO_TRIAGEM'
  },
  {
    id: 4,
    atendimento: 104523,
    pacienteNome: 'João Carlos de Oliveira',
    documentoCpf: '12345678901',
    idade: 51,
    sexo: 'M',
    pesoKg: 78.5,
    modalidade: 'TC',
    tipoFormulario: 'TC_TORAX',
    segmentoLado: 'Tórax',
    procedimento: 'Tomografia Computadorizada de Tórax com Contraste',
    sala: 'TC-01',
    horario: '10:15',
    status: 'AGUARDANDO_TRIAGEM'
  },
  {
    id: 5,
    atendimento: 104524,
    pacienteNome: 'Lucas Ferreira da Silva',
    documentoCpf: '32165498722',
    idade: 36,
    sexo: 'M',
    pesoKg: 74.0,
    modalidade: 'TC',
    tipoFormulario: 'TC_ABDOMEN',
    segmentoLado: 'Abdômen Total',
    procedimento: 'Tomografia Computadorizada de Abdômen Total com Contraste',
    sala: 'TC-02',
    horario: '11:00',
    status: 'AGUARDANDO_TRIAGEM'
  }
];

// Checklists de Segurança Especializados (Centro de Diagnóstico por Imagem - UNICIVE)
const PROTOCOLOS_CHECKLIST = {
  // 1. QUESTIONÁRIO PARA RESSONÂNCIA DA COLUNA (Cervical / Torácica / Lombar)
  RM_COLUNA: [
    { codigo: 'COLUNA_PREPARO', enunciado: 'O preparo prévio do exame foi cumprido adequadamente conforme orientação?', impeditivo: false },
    { codigo: 'COLUNA_MARCAPASSO', enunciado: 'Possui marcapasso cardíaco ou cardioversor-desfibrilador (CDI)?', impeditivo: true },
    { codigo: 'COLUNA_CLIPE_ANEURISMA', enunciado: 'Possui clipes de aneurisma cerebral ou implantes vasculares metálicos?', impeditivo: true },
    { codigo: 'COLUNA_IMPLANTE_COCLEAR', enunciado: 'Possui implante coclear ou neuroestimulador implantado?', impeditivo: true },
    { codigo: 'COLUNA_SINTOMA_DOR_IRRADIADA', enunciado: 'Apresenta dor que se irradia para membros ou perda de força e sensibilidade?', impeditivo: false },
    { codigo: 'COLUNA_CIRURGIA_PREVIA', enunciado: 'Já realizou cirurgia prévia na coluna ou fixação com placas e parafusos?', impeditivo: false },
    { codigo: 'COLUNA_TRAUMA', enunciado: 'Sofreu algum trauma recente, queda ou batida na região da coluna?', impeditivo: false },
    { codigo: 'COLUNA_OSTEOPOROSE', enunciado: 'Possui diagnóstico de osteoporose severa ou osteopenia com risco de fratura?', impeditivo: false },
    { codigo: 'COLUNA_TUMOR_QUIMIO', enunciado: 'Possui histórico de tumor, quimioterapia ou radioterapia?', impeditivo: false },
    { codigo: 'COLUNA_CLAUSTROFOBIA', enunciado: 'Apresenta claustrofobia importante ao entrar no aparelho fechado?', impeditivo: false }
  ],

  // 2. QUESTIONÁRIO PARA RESSONÂNCIA DE BRAÇO / ANTEBRAÇO (Membro Superior)
  RM_BRACO_ANTEBRACO: [
    { codigo: 'BRACO_PREPARO', enunciado: 'O preparo prévio do exame foi cumprido adequadamente conforme orientação?', impeditivo: false },
    { codigo: 'BRACO_MARCAPASSO', enunciado: 'Possui marcapasso cardíaco ou neuroestimulador implantado?', impeditivo: true },
    { codigo: 'BRACO_CORPO_ESTRANHO_METAL', enunciado: 'Já trabalhou com solda/metal ou teve fragmento metálico ocular?', impeditivo: true },
    { codigo: 'BRACO_SINTOMAS', enunciado: 'Apresenta alteração de sensibilidade, formigamento, inchaço ou limitação de movimento?', impeditivo: false },
    { codigo: 'BRACO_ESPORTES', enunciado: 'Pratica esportes ou atividades com movimentos repetitivos de membros superiores?', impeditivo: false },
    { codigo: 'BRACO_RUPTURA_TENDAO', enunciado: 'Suspeita ou diagnóstico de ruptura / estiramento de tendão ou músculo?', impeditivo: false },
    { codigo: 'BRACO_TRAUMA_CIRURGIA', enunciado: 'Histórico de trauma, fratura, infecção ou cirurgia prévia no braço/antebraço?', impeditivo: false },
    { codigo: 'BRACO_CLAUSTROFOBIA', enunciado: 'Apresenta claustrofobia importante ao entrar no aparelho fechado?', impeditivo: false }
  ],

  // 3. QUESTIONÁRIO PARA RESSONÂNCIA DE BACIA / QUADRIL (Articulações Coxofemorais)
  RM_BACIA_QUADRIL: [
    { codigo: 'BACIA_PREPARO', enunciado: 'O preparo prévio do exame foi cumprido adequadamente conforme orientação?', impeditivo: false },
    { codigo: 'BACIA_MARCAPASSO', enunciado: 'Possui marcapasso cardíaco ou implante metálico ferromagnético?', impeditivo: true },
    { codigo: 'BACIA_CLIPE_ANEURISMA', enunciado: 'Possui clipes metálicos cerebrais ou prótese incompatível com alto campo?', impeditivo: true },
    { codigo: 'BACIA_USO_CORTICOIDE', enunciado: 'Fez ou faz uso contínuo de medicamentos corticosteroides (risco de osteonecrose da cabeça femoral)?', impeditivo: false },
    { codigo: 'BACIA_DOENCA_INFANCIA', enunciado: 'Apresentou doença do quadril durante a infância (Perthes, epifisiólise, displasia)?', impeditivo: false },
    { codigo: 'BACIA_RUPTURA_TENDAO', enunciado: 'Suspeita de ruptura tendínea glútea, bursite trocantérica ou lesão labral?', impeditivo: false },
    { codigo: 'BACIA_TRAUMA_CIRURGIA', enunciado: 'Histórico de trauma prévio, queda ou cirurgia no quadril/fêmur?', impeditivo: false },
    { codigo: 'BACIA_CLAUSTROFOBIA', enunciado: 'Apresenta claustrofobia importante ao entrar no aparelho fechado?', impeditivo: false }
  ],

  // 4. QUESTIONÁRIOS DE TOMOGRAFIA COMPUTADORIZADA (TC de Tórax e Abdômen)
  TC_TORAX: [
    { codigo: 'TC_ALERGIA_IODO', enunciado: 'Histórico prévio de anafilaxia ou choque alérgico grave ao contraste iodado?', impeditivo: true },
    { codigo: 'TC_INSUFICIENCIA_RENAL', enunciado: 'Diagnóstico prévio de doença renal crônica ou diálise (Creatinina alterada)?', impeditivo: true },
    { codigo: 'TC_METFORMINA', enunciado: 'Faz uso contínuo de Cloridrato de Metformina?', impeditivo: false },
    { codigo: 'TC_JEJUM', enunciado: 'Cumpriu o jejum mínimo de 4 horas orientado?', impeditivo: false },
    { codigo: 'TC_GESTAO', enunciado: 'Possibilidade de gravidez ativa no momento (contraindicação radiológica)?', impeditivo: false }
  ],

  TC_ABDOMEN: [
    { codigo: 'TC_ALERGIA_IODO', enunciado: 'Histórico prévio de anafilaxia ou choque alérgico grave ao contraste iodado?', impeditivo: true },
    { codigo: 'TC_INSUFICIENCIA_RENAL', enunciado: 'Diagnóstico prévio de doença renal crônica ou diálise (Creatinina alterada)?', impeditivo: true },
    { codigo: 'TC_METFORMINA', enunciado: 'Faz uso contínuo de Cloridrato de Metformina?', impeditivo: false },
    { codigo: 'TC_JEJUM', enunciado: 'Cumpriu o jejum mínimo de 4 horas orientado?', impeditivo: false },
    { codigo: 'TC_CIRURGIA_ABDOMINAL', enunciado: 'Histórico de cirurgia abdominal prévia ou dor aguda com suspeita cirúrgica?', impeditivo: false },
    { codigo: 'TC_GESTAO', enunciado: 'Possibilidade de gravidez ativa no momento (contraindicação radiológica)?', impeditivo: false }
  ],

  // Fallbacks genéricos
  TC: [
    { codigo: 'TC_ALERGIA_IODO', enunciado: 'Histórico prévio de anafilaxia ou choque alérgico grave ao contraste iodado?', impeditivo: true },
    { codigo: 'TC_INSUFICIENCIA_RENAL', enunciado: 'Diagnóstico prévio de doença renal crônica ou diálise?', impeditivo: true },
    { codigo: 'TC_METFORMINA', enunciado: 'Faz uso contínuo de Cloridrato de Metformina?', impeditivo: false },
    { codigo: 'TC_JEJUM', enunciado: 'Cumpriu o jejum mínimo de 4 horas orientado?', impeditivo: false },
    { codigo: 'TC_GESTAO', enunciado: 'Possibilidade de gravidez ativa no momento?', impeditivo: false }
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

// Estado da Aplicação
const UIState = {
  online: false,
  usuarioLogado: { id: 1, nome: 'Lucas Santos', login: 'lucas.biomed', perfil: 'BIOMÉDICO' },
  exames: [...EXAMES_INICIAIS],
  exameSelecionado: null,
  filtroModalidade: 'TODOS',
  buscaTexto: '',
  respostas: {}
};

// Inicialização Geral
document.addEventListener('DOMContentLoaded', () => {
  initTema();
  initAutenticacao();
  initNavegacao();
  initWorklist();
  initCalculadora();
  initAdminView();
  initAtalhosTeclado();
  verificarConexaoApi();
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
   2. CONECTIVIDADE E STATUS DA API
   ============================================================================== */
async function verificarConexaoApi() {
  const pill = document.getElementById('apiStatusPill');
  const dot = document.getElementById('apiStatusDot');
  const text = document.getElementById('apiStatusText');

  try {
    const statusBancos = await ApiClient.obterStatusBancos();
    UIState.online = true;
    if (dot) dot.style.background = 'var(--emerald-400)';
    if (text) text.textContent = 'API .NET Online (Bancos 1 & 2 Conectados)';
    if (pill) {
      pill.style.borderColor = 'rgba(16, 185, 129, 0.4)';
      pill.title = `Banco 1 (Triagem): ${statusBancos.banco1_aplicacao?.provedor} | Banco 2 (Worklist Lab): ${statusBancos.banco2_worklist_lab?.provedor} (${statusBancos.banco2_worklist_lab?.pedidosAgendados} exames)`;
    }
    return true;
  } catch (err) {
    UIState.online = false;
    if (dot) dot.style.background = 'var(--amber-400)';
    if (text) text.textContent = 'Modo Demonstração (API Offline)';
    if (pill) pill.style.borderColor = 'rgba(245, 158, 11, 0.4)';
    return false;
  }
}

/* ==============================================================================
   3. LOGIN E AUTENTICAÇÃO
   ============================================================================== */
function initAutenticacao() {
  const form = document.getElementById('form-login');
  const alertBox = document.getElementById('loginAlertBox');

  form?.addEventListener('submit', async (e) => {
    e.preventDefault();
    const username = document.getElementById('login-username').value.trim();
    const password = document.getElementById('login-password').value;

    const btnSubmit = document.getElementById('btnLoginSubmit');
    if (btnSubmit) {
      btnSubmit.disabled = true;
      btnSubmit.innerHTML = '<i class="fa-solid fa-spinner fa-spin"></i> Autenticando...';
    }

    try {
      // 1. Tenta autenticar na API real .NET
      const user = await ApiClient.login(username, password);
      UIState.usuarioLogado = user;
      UIState.online = true;
      entrar(user.nome, user.perfilDescricao || user.perfil);
    } catch (err) {
      if (err.message && err.message.includes('401')) {
        if (alertBox) {
          alertBox.textContent = 'Usuário ou senha inválidos. Verifique suas credenciais.';
          alertBox.classList.remove('hidden');
        }
        showToast('Login inválido.', 'error');
      } else {
        // Fallback offline caso o backend MySQL ainda não tenha sido iniciado
        const perfil = username.includes('admin')
          ? 'ADMIN'
          : (username.includes('tec')
            ? 'TÉCNICO'
            : (username.includes('estagio') ? 'ESTAGIÁRIO' : 'BIOMÉDICO'));
        UIState.usuarioLogado = { id: 1, nome: username, login: username, perfil: perfil };
        entrar(username, perfil);
        showToast('Iniciado em Modo Demonstração Offline.', 'info');
      }
    } finally {
      if (btnSubmit) {
        btnSubmit.disabled = false;
        btnSubmit.innerHTML = '<i class="fa-solid fa-right-to-bracket"></i> Acessar Posto de Triagem';
      }
    }
  });

  document.getElementById('btnLogout')?.addEventListener('click', sair);

  // Toggle visibilidade de senha
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

  // Atualiza avatar do operador conforme especialidade
  const avatar = document.getElementById('sidebarAvatar');
  const perfUpper = (perfil || '').toString().toUpperCase();
  const isAdmin = perfUpper.includes('ADMIN') || UIState.usuarioLogado.perfil === 4 || UIState.usuarioLogado.perfil === 'ADMIN';

  if (avatar) {
    if (isAdmin) {
      avatar.innerHTML = '<i class="fa-solid fa-user-shield"></i>';
    } else if (perfUpper.includes('ESTAGI')) {
      avatar.innerHTML = '<i class="fa-solid fa-graduation-cap"></i>';
    } else if (perfUpper.includes('TEC')) {
      avatar.innerHTML = '<i class="fa-solid fa-user-nurse"></i>';
    } else {
      avatar.innerHTML = '<i class="fa-solid fa-user-doctor"></i>';
    }
  }

  // Controle de visibilidade de itens administrativos
  const navAdmin = document.getElementById('navAdmin');
  const navLabelAdmin = document.getElementById('navLabelAdmin');
  if (navAdmin) navAdmin.style.display = isAdmin ? 'flex' : 'none';
  if (navLabelAdmin) navLabelAdmin.style.display = isAdmin ? 'block' : 'none';

  document.querySelectorAll('.admin-only').forEach(el => {
    if (isAdmin) el.classList.remove('hidden');
    else el.classList.add('hidden');
  });

  showToast(`Sessão iniciada como ${nome} (${perfil})`, 'success');
  carregarWorklist();
  if (isAdmin) {
    carregarAdminView();
  }
}

function sair() {
  document.getElementById('app-layout').classList.add('hidden');
  document.getElementById('login-screen').classList.remove('hidden');
}

/* ==============================================================================
   4. NAVEGAÇÃO ENTRE TELAS
   ============================================================================== */
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
    carregarAuditoria();
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
    'view-auditoria': 'Histórico Inviolável de Auditoria Clínica',
    'view-admin': 'Painel de Controle Administrativo (TI & Governança)'
  };

  document.getElementById('topbarTitle').textContent = titulos[viewId] || 'Posto de Triagem';

  if (viewId === 'view-auditoria') {
    carregarAuditoria();
  } else if (viewId === 'view-admin') {
    carregarAdminView();
  }
}

/* ==============================================================================
   5. WORKLIST (GRADE DE ATENDIMENTOS)
   ============================================================================== */
function normalizarStatus(statusRaw, statusDescricao) {
  const desc = (statusDescricao || '').toString().trim().toUpperCase();
  if (desc === 'LIBERADO' || statusRaw === 3 || statusRaw === '3') return 'LIBERADO';
  if (desc === 'BLOQUEADO_SEGURANCA' || desc === 'BLOQUEADO' || statusRaw === 4 || statusRaw === '4') return 'BLOQUEADO_SEGURANCA';
  if (desc === 'EM_TRIAGEM' || statusRaw === 2 || statusRaw === '2') return 'EM_TRIAGEM';
  if (desc === 'CONCLUIDO' || statusRaw === 5 || statusRaw === '5') return 'CONCLUIDO';
  return 'AGUARDANDO_TRIAGEM';
}

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
}

async function carregarWorklist() {
  try {
    const examesRemotos = await ApiClient.obterWorklist();
    if (Array.isArray(examesRemotos) && examesRemotos.length > 0) {
      UIState.exames = examesRemotos.map(e => ({
        id: e.id,
        atendimento: e.atendimento,
        pacienteNome: e.pacienteNome,
        documentoCpf: e.pacienteCpf || e.documentoCpf,
        idade: e.idade,
        sexo: e.sexo,
        pesoKg: e.pesoKg,
        modalidade: e.modalidadeDescricao || (e.modalidade === 1 ? 'TC' : 'RM'),
        tipoFormulario: e.tipoFormulario || (e.modalidadeDescricao === 'RM' || e.modalidade === 2 ? 'RM_COLUNA' : 'TC_TORAX'),
        segmentoLado: e.segmentoLado || null,
        motivoExame: e.motivoExame || null,
        medicoSolicitante: e.medicoSolicitante || null,
        convenio: e.convenio || 'Unimed',
        procedimento: e.descricaoExame || e.procedimento,
        sala: e.sala,
        horario: e.horarioAgendado ? new Date(e.horarioAgendado).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) : '08:00',
        status: normalizarStatus(e.status, e.statusDescricao),
        statusRiscoSemaforo: e.statusRiscoSemaforo,
        possuiTriagem: e.possuiTriagem,
        volumeContrasteMl: e.volumeContrasteMl,
        caminhoArquivoArmazenado: e.caminhoArquivoArmazenado
      }));
      verificarConexaoApi();
    }
  } catch (err) {
    console.warn('API indisponível, utilizando dados estáticos em memória.');
    verificarConexaoApi();
  }

  // Sincroniza referência do exame atualmente selecionado
  if (UIState.exameSelecionado) {
    const sincronizado = UIState.exames.find(x => x.id === UIState.exameSelecionado.id);
    if (sincronizado) {
      UIState.exameSelecionado = sincronizado;
    }
  }

  renderizarWorklist();

  // Se a tabela de exames da aba Admin estiver presente, atualiza-a em sincronia
  if (document.getElementById('adminExamesTableBody')) {
    renderizarAdminExames();
  }
}

function renderizarWorklist() {
  const tbody = document.getElementById('worklistTableBody');
  if (!tbody) return;

  const filtrados = UIState.exames.filter(item => {
    if (UIState.filtroModalidade === 'TC' && item.modalidade !== 'TC') return false;
    if (UIState.filtroModalidade === 'RM' && item.modalidade !== 'RM') return false;
    if (UIState.filtroModalidade === 'AGUARDANDO_TRIAGEM' && item.status !== 'AGUARDANDO_TRIAGEM') return false;
    if (UIState.filtroModalidade === 'LIBERADO' && item.status !== 'LIBERADO') return false;
    if (UIState.filtroModalidade === 'BLOQUEADO_SEGURANCA' && item.status !== 'BLOQUEADO_SEGURANCA') return false;

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
    const isEmTriagem = item.status === 'EM_TRIAGEM';

    const statusBadge = isLiberado
      ? '<span class="badge-status status-liberado"><i class="fa-solid fa-check-circle"></i> Liberado</span>'
      : (isBloqueado
        ? '<span class="badge-status status-bloqueado"><i class="fa-solid fa-ban"></i> Não Permitido (Bloqueado)</span>'
        : (isEmTriagem
          ? '<span class="badge-status" style="background: rgba(245, 158, 11, 0.2); color: var(--amber-400); border: 1px solid rgba(245, 158, 11, 0.4);"><i class="fa-solid fa-spinner fa-spin"></i> Em Triagem</span>'
          : '<span class="badge-status status-aguardando"><i class="fa-solid fa-clock"></i> Aguardando</span>'));

    const modBadgeClass = item.modalidade === 'TC' ? 'badge-tc' : 'badge-rm';
    const btnTexto = isLiberado 
      ? 'Ver Ficha' 
      : (isBloqueado 
        ? 'Ver Bloqueio' 
        : (isEmTriagem ? 'Continuar Triagem' : 'Iniciar Triagem'));
    const btnIcone = isLiberado 
      ? 'fa-file-lines' 
      : (isBloqueado ? 'fa-ban' : 'fa-clipboard-check');

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
        <td><span style="font-family: var(--font-mono); font-weight: 700;">${item.sala}</span></td>
        <td>${statusBadge}</td>
        <td style="text-align: right;">
          <button type="button" class="btn-action btn-iniciar-triagem" onclick="abrirTriagem(${item.id})">
            <i class="fa-solid ${btnIcone}"></i> ${btnTexto}
          </button>
        </td>
      </tr>
    `;
  }).join('');

  // Atualiza contadores dos cards estatísticos
  const aguardando = UIState.exames.filter(e => e.status === 'AGUARDANDO_TRIAGEM').length;
  const liberados = UIState.exames.filter(e => e.status === 'LIBERADO').length;
  const bloqueados = UIState.exames.filter(e => e.status === 'BLOQUEADO_SEGURANCA').length;

  document.getElementById('statTotal').textContent = UIState.exames.length;
  document.getElementById('statAguardando').textContent = aguardando;
  document.getElementById('statLiberados').textContent = liberados;
  document.getElementById('statBloqueados').textContent = bloqueados;
  document.getElementById('badgeWorklistCount').textContent = aguardando;
}

function obterPerguntasExameAtual() {
  if (!UIState.exameSelecionado) return [];
  const item = UIState.exameSelecionado;
  const tipoForm = item.tipoFormulario || (item.modalidade === 'TC' ? 'TC_TORAX' : 'RM_COLUNA');
  return PROTOCOLOS_CHECKLIST[tipoForm] || PROTOCOLOS_CHECKLIST[item.modalidade] || PROTOCOLOS_CHECKLIST.TC || [];
}

/* ==============================================================================
   6. FICHA DE TRIAGEM, CHECKLIST E SEMÁFORO CLÍNICO
   ============================================================================== */
async function abrirTriagem(exameId) {
  const item = UIState.exames.find(e => e.id === exameId);
  if (!item) return;

  UIState.exameSelecionado = item;
  UIState.respostas = {};

  const isConcluidoOuAssinado = item.status === 'LIBERADO' || item.status === 'BLOQUEADO_SEGURANCA';

  // Se o exame estiver aguardando e ainda não foi assinado, transiciona para EM_TRIAGEM
  if (!isConcluidoOuAssinado && item.status === 'AGUARDANDO_TRIAGEM') {
    item.status = 'EM_TRIAGEM';
    if (UIState.online) {
      ApiClient.chamarPaciente(item.id, UIState.usuarioLogado.id || 1).catch(() => {});
    }
  }

  // Preenche dados do paciente na tela de triagem
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
  const inputPeso = document.getElementById('calcPesoInput');
  const inputCreat = document.getElementById('calcCreatininaInput');
  if (inputPeso) {
    inputPeso.value = item.pesoKg || 70.0;
    inputPeso.disabled = isConcluidoOuAssinado;
  }
  if (inputCreat) {
    inputCreat.value = '';
    inputCreat.disabled = isConcluidoOuAssinado;
  }
  recalcularDose();

  // Renderiza checklist especializado conforme o tipo de formulário do exame
  const tipoForm = item.tipoFormulario || (item.modalidade === 'TC' ? 'TC_TORAX' : 'RM_COLUNA');
  const titulosEspecializados = {
    RM_COLUNA: 'Questionário Clínico: Ressonância da Coluna (Cervical / Torácica / Lombar)',
    RM_BRACO_ANTEBRACO: 'Questionário Clínico: Ressonância de Braço / Antebraço (Membro Superior)',
    RM_BACIA_QUADRIL: 'Questionário Clínico: Ressonância de Bacia / Quadril (Articulações Coxofemorais)',
    TC_TORAX: 'Checklist de Segurança: Tomografia Computadorizada de Tórax (Contraste Iodado)',
    TC_ABDOMEN: 'Checklist de Segurança: Tomografia Computadorizada de Abdômen Total'
  };

  const perguntas = obterPerguntasExameAtual();
  document.getElementById('checklistProtocoloTitulo').textContent = 
    titulosEspecializados[tipoForm] || `Checklist de Segurança - ${item.modalidade}`;

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
          <button type="button" class="btn-choice" id="btn-nao-${p.codigo}" ${isConcluidoOuAssinado ? 'disabled style="pointer-events:none;"' : ''} onclick="definirResposta('${p.codigo}', 'NAO')">
            <i class="fa-solid fa-check"></i> NÃO
          </button>
          <button type="button" class="btn-choice" id="btn-sim-${p.codigo}" ${isConcluidoOuAssinado ? 'disabled style="pointer-events:none;"' : ''} onclick="definirResposta('${p.codigo}', 'SIM')">
            <i class="fa-solid fa-xmark"></i> SIM
          </button>
        </div>
      </div>
    `;
  }).join('');

  // Resposta padrão inicial: NÃO para todas
  perguntas.forEach(p => definirResposta(p.codigo, 'NAO', false));

  const obsElem = document.getElementById('txtObservacoesClinicas');
  if (obsElem) {
    obsElem.value = item.observacoesClinicas || '';
    obsElem.disabled = isConcluidoOuAssinado;
  }

  // Se a API estiver online, carrega respostas prévias e laudo arquivado se houver
  if (UIState.online) {
    try {
      const triagemExistente = await ApiClient.obterTriagemPorExameId(item.id);
      if (triagemExistente && Array.isArray(triagemExistente.respostas) && triagemExistente.respostas.length > 0) {
        triagemExistente.respostas.forEach(r => {
          definirResposta(r.codigoPergunta, r.respostaValor, false);
        });
        if (triagemExistente.observacoesClinicas && obsElem) {
          obsElem.value = triagemExistente.observacoesClinicas;
        }
        if (triagemExistente.volumeContrasteMl) {
          document.getElementById('calcDoseResultado').textContent = triagemExistente.volumeContrasteMl.toFixed(1);
        }
        if (triagemExistente.caminhoArquivoArmazenado) {
          item.caminhoArquivoArmazenado = triagemExistente.caminhoArquivoArmazenado;
        }
      }
    } catch {
      // Sem triagem prévia persistida, mantém respostas padrão
    }
  }

  avaliarSemaforo();
  configurarPainelAssinatura(item, isConcluidoOuAssinado);

  document.getElementById('navTriagem').removeAttribute('disabled');
  trocarView('view-triagem');
}

function configurarPainelAssinatura(item, isConcluidoOuAssinado) {
  const painelPre = document.getElementById('painelPreAssinatura');
  const painelPos = document.getElementById('painelPosAssinatura');
  const badgeEstado = document.getElementById('badgeEstadoAssinatura');
  const btnSalvar = document.getElementById('btnSalvarRascunho');
  const btnLiberar = document.getElementById('btnLiberarExame');
  const btnBloquear = document.getElementById('btnBloquearExame');

  const operadorNome = UIState.usuarioLogado.nome || 'Operador Clínico';
  const operadorPerfil = (UIState.usuarioLogado.perfilDescricao || UIState.usuarioLogado.perfil || 'BIOMÉDICO').toUpperCase();

  if (isConcluidoOuAssinado) {
    if (painelPre) painelPre.classList.add('hidden');
    if (painelPos) painelPos.classList.remove('hidden');

    const isLiberado = item.status === 'LIBERADO';
    if (badgeEstado) {
      badgeEstado.className = isLiberado ? 'badge-status status-liberado' : 'badge-status status-bloqueado';
      badgeEstado.innerHTML = isLiberado 
        ? '<i class="fa-solid fa-circle-check"></i> Assinatura Digital Válida (Liberado)'
        : '<i class="fa-solid fa-ban"></i> Bloqueio Homologado (Não Permitido)';
    }

    const posTexto = document.getElementById('posAssinaturaTexto');
    if (posTexto) {
      posTexto.innerHTML = isLiberado
        ? `Exame liberado para execução na sala <strong>${item.sala}</strong>. Assinado digitalmente pelo operador responsável.`
        : `Exame com contraindicação radiológica absoluta homologada. Protocolo de segurança encerrado.`;
    }

    if (btnSalvar) btnSalvar.style.display = 'none';
    if (btnLiberar) btnLiberar.style.display = 'none';
    if (btnBloquear) btnBloquear.style.display = 'none';

    // Botão de reabertura para Administrador
    const btnResetAdmin = document.getElementById('btnResetarStatusExameDetalhe');
    const isAdmin = operadorPerfil.includes('ADMIN') || UIState.usuarioLogado.perfil === 4;
    if (btnResetAdmin) {
      if (isAdmin) {
        btnResetAdmin.classList.remove('hidden');
        btnResetAdmin.onclick = async () => {
          if (confirm(`Deseja retornar o Exame #${item.atendimento} para o status AGUARDANDO_TRIAGEM?`)) {
            await ApiClient.resetStatusExameAdmin(item.id);
            showToast(`Exame #${item.atendimento} resetado para Aguardando Triagem.`, 'success');
            await carregarWorklist();
            abrirTriagem(item.id);
          }
        };
      } else {
        btnResetAdmin.classList.add('hidden');
      }
    }

    const btnVerDoc = document.getElementById('btnVerLaudoArquivado');
    if (btnVerDoc) {
      btnVerDoc.onclick = () => {
        exibirModalArquivoSalvo(item.caminhoArquivoArmazenado || 'C:\\Users\\vinicius.tomazi\\Documents\\Triagem_Arquivada.pdf');
      };
    }
  } else {
    if (painelPre) painelPre.classList.remove('hidden');
    if (painelPos) painelPos.classList.add('hidden');

    if (badgeEstado) {
      badgeEstado.className = 'badge-status status-aguardando';
      badgeEstado.innerHTML = '<i class="fa-solid fa-pen-fancy"></i> Pré-Assinatura (Em Aberto)';
    }

    const elemNome = document.getElementById('assinaturaNomeOperador');
    const elemPerfil = document.getElementById('assinaturaPerfilOperador');
    const elemTime = document.getElementById('assinaturaTimestamp');
    const elemDose = document.getElementById('assinaturaVolumeResumo');
    const elemLote = document.getElementById('assinaturaLoteResumo');

    if (elemNome) elemNome.textContent = operadorNome;
    if (elemPerfil) elemPerfil.textContent = `${operadorPerfil} - Apto para abertura, laudo, dosagem e assinatura`;
    if (elemTime) elemTime.textContent = new Date().toLocaleString([], { dateStyle: 'short', timeStyle: 'medium' });
    if (elemDose) elemDose.textContent = `${document.getElementById('calcDoseResultado')?.textContent || '0.0'} mL`;
    if (elemLote) elemLote.textContent = `Lote: ${document.getElementById('calcLoteInput')?.value || 'LOT-2026-X49B'}`;

    if (btnSalvar) btnSalvar.style.display = 'inline-flex';
  }
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

  const perguntas = obterPerguntasExameAtual();
  const pergunta = perguntas.find(p => p.codigo === codigo);
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
  const btnBloquear = document.getElementById('btnBloquearExame');

  const item = UIState.exameSelecionado;
  const perguntas = obterPerguntasExameAtual();

  const impeditivosAtivos = [];
  const atencoesAtivas = [];

  perguntas.forEach((p, index) => {
    if (UIState.respostas[p.codigo] === 'SIM') {
      if (p.impeditivo) {
        impeditivosAtivos.push({ codigo: p.codigo, index: index + 1, enunciado: p.enunciado });
      } else {
        atencoesAtivas.push({ codigo: p.codigo, index: index + 1, enunciado: p.enunciado });
      }
    }
  });

  banner.classList.remove('semaforo-verde', 'semaforo-amarelo', 'semaforo-vermelho');

  const isJaBloqueado = item && item.status === 'BLOQUEADO_SEGURANCA';

  if (impeditivosAtivos.length > 0 || isJaBloqueado) {
    banner.classList.add('semaforo-vermelho');
    orb.innerHTML = '<i class="fa-solid fa-ban"></i>';
    titulo.textContent = 'SEMÁFORO CLÍNICO: BLOQUEIO DE SEGURANÇA MANDATÓRIO';

    let motivosHtml = '';
    if (impeditivosAtivos.length > 0) {
      motivosHtml = `
        <div style="margin-top: 6px;">
          <div style="font-weight: 700; color: #fca5a5; margin-bottom: 4px;">
            <i class="fa-solid fa-triangle-exclamation"></i> ${impeditivosAtivos.length} Impedimento(s) Ativo(s) Detectado(s):
          </div>
          <ul style="margin: 0; padding-left: 18px; font-size: 0.85rem; color: #fecaca; line-height: 1.4;">
            ${impeditivosAtivos.map(m => `<li><strong>[Q${m.index}]</strong> ${m.enunciado}</li>`).join('')}
          </ul>
        </div>
      `;
    } else {
      motivosHtml = `<div style="font-size: 0.85rem; color: #fca5a5; margin-top: 4px;">Exame com bloqueio de segurança registrado por contraindicação médica.</div>`;
    }

    descricao.innerHTML = `
      ${motivosHtml}
      <div style="font-size: 0.8rem; color: var(--text-muted); margin-top: 6px;">
        <i class="fa-solid fa-shield-halved"></i> Diretriz CBR/ACR: A liberação operacional está proibida. Confirme os dados e encerre a ficha como Não Permitido.
      </div>
    `;

    badge.textContent = 'BLOQUEIO VERMELHO';

    if (btnLiberar) {
      btnLiberar.style.display = 'none';
      btnLiberar.disabled = true;
    }

    if (btnBloquear) {
      btnBloquear.style.display = 'inline-flex';
      btnBloquear.disabled = false;
      btnBloquear.innerHTML = isJaBloqueado
        ? '<i class="fa-solid fa-ban"></i> Exame Bloqueado (Ver Detalhes)'
        : '<i class="fa-solid fa-ban"></i> Encerrar Ficha &amp; Bloquear Exame';
    }
  } else if (atencoesAtivas.length > 0) {
    banner.classList.add('semaforo-amarelo');
    orb.innerHTML = '<i class="fa-solid fa-triangle-exclamation"></i>';
    titulo.textContent = 'SEMÁFORO CLÍNICO: ATENÇÃO CLÍNICA REQUERIDA';

    const motivosHtml = `
      <div style="margin-top: 6px;">
        <div style="font-weight: 700; color: #fde047; margin-bottom: 4px;">
          <i class="fa-solid fa-circle-exclamation"></i> ${atencoesAtivas.length} Alerta(s) Moderado(s):
        </div>
        <ul style="margin: 0; padding-left: 18px; font-size: 0.85rem; color: #fef08a; line-height: 1.4;">
          ${atencoesAtivas.map(m => `<li><strong>[Q${m.index}]</strong> ${m.enunciado}</li>`).join('')}
        </ul>
      </div>
    `;

    descricao.innerHTML = `
      ${motivosHtml}
      <div style="font-size: 0.8rem; color: var(--text-muted); margin-top: 6px;">
        Avaliar protocolo de hidratação, jejum ou precauções operacionais antes da injeção/exame.
      </div>
    `;

    badge.textContent = 'ALERTA AMARELO';

    if (btnLiberar) {
      btnLiberar.style.display = 'inline-flex';
      btnLiberar.disabled = false;
      btnLiberar.innerHTML = '<i class="fa-solid fa-shield-check"></i> Liberar Exame com Alerta';
    }

    if (btnBloquear) btnBloquear.style.display = 'none';
  } else {
    banner.classList.add('semaforo-verde');
    orb.innerHTML = '<i class="fa-solid fa-check"></i>';
    titulo.textContent = 'SEMÁFORO CLÍNICO: NENHUM IMPEDIMENTO DETECTADO';
    descricao.textContent = 'Todos os critérios de segurança validados. O exame está apto para liberação operacional.';
    badge.textContent = 'LIBERADO VERDE';

    if (btnLiberar) {
      btnLiberar.style.display = 'inline-flex';
      btnLiberar.disabled = false;
      btnLiberar.innerHTML = '<i class="fa-solid fa-shield-check"></i> Liberar Exame com Segurança';
    }

    if (btnBloquear) btnBloquear.style.display = 'none';
  }
}

/* ==============================================================================
   7. CALCULADORA DE CONTRASTE & GRAVAÇÃO DA TRIAGEM
   ============================================================================== */
function initCalculadora() {
  document.getElementById('calcPesoInput')?.addEventListener('input', recalcularDose);
  document.getElementById('calcCreatininaInput')?.addEventListener('input', recalcularDose);

  document.getElementById('btnSalvarRascunho')?.addEventListener('click', async () => {
    try {
      const res = await persistirTriagem(false);
      if (UIState.exameSelecionado.status === 'AGUARDANDO_TRIAGEM') {
        UIState.exameSelecionado.status = 'EM_TRIAGEM';
        const itemFila = UIState.exames.find(e => e.id === UIState.exameSelecionado.id);
        if (itemFila) itemFila.status = 'EM_TRIAGEM';
      }
      const caminho = res?.caminhoArquivoArmazenado;
      if (caminho) {
        UIState.exameSelecionado.caminhoArquivoArmazenado = caminho;
        showToast('Ficha de triagem salva e arquivada em Documentos!', 'success');
        exibirModalArquivoSalvo(caminho);
      } else {
        showToast('Rascunho da triagem salvo com sucesso no sistema.', 'success');
      }
      await carregarWorklist();
    } catch (err) {
      showToast(`Erro ao salvar: ${err.message}`, 'error');
    }
  });

  // Botão de Bloqueio/Recusa por contraindicação clínica
  document.getElementById('btnBloquearExame')?.addEventListener('click', () => {
    abrirModalConfirmacaoBloqueio();
  });

  document.getElementById('btnLiberarExame')?.addEventListener('click', async () => {
    if (!UIState.exameSelecionado) return;

    // Se houver qualquer resposta impeditiva com SIM, redireciona para confirmação de bloqueio
    const perguntas = obterPerguntasExameAtual();
    const temBloqueio = perguntas.some(p => UIState.respostas[p.codigo] === 'SIM' && p.impeditivo);
    if (temBloqueio) {
      abrirModalConfirmacaoBloqueio();
      return;
    }

    const btn = document.getElementById('btnLiberarExame');
    btn.disabled = true;
    btn.innerHTML = '<i class="fa-solid fa-spinner fa-spin"></i> Assinando &amp; Liberando...';

    try {
      // 1. Salva a triagem e solicita a liberação
      const res = await persistirTriagem(true);

      // 2. Imediatamente atualiza o status local para LIBERADO
      UIState.exameSelecionado.status = 'LIBERADO';
      const itemFila = UIState.exames.find(e => e.id === UIState.exameSelecionado.id);
      if (itemFila) itemFila.status = 'LIBERADO';

      const caminho = res?.caminhoArquivoArmazenado;
      if (caminho) {
        UIState.exameSelecionado.caminhoArquivoArmazenado = caminho;
        if (itemFila) itemFila.caminhoArquivoArmazenado = caminho;
      }

      showToast(`Exame #${UIState.exameSelecionado.atendimento} assinado digitalmente e liberado!`, 'success');

      // 3. Recarrega a Worklist do servidor
      await carregarWorklist();

      // 4. Retorna para a fila de exames
      trocarView('view-worklist');

      // 5. Se gerou documento, exibe comprovante
      if (caminho) {
        exibirModalArquivoSalvo(caminho);
      }
    } catch (err) {
      showToast(`Erro ao liberar exame: ${err.message}`, 'error');
    } finally {
      btn.disabled = false;
      btn.innerHTML = '<i class="fa-solid fa-shield-check"></i> Liberar Exame com Segurança';
    }
  });
}

async function persistirTriagem(liberar = false) {
  if (!UIState.exameSelecionado) return null;

  const perguntas = obterPerguntasExameAtual();

  const respostasDto = perguntas.map(p => ({
    codigoPergunta: p.codigo,
    perguntaEnunciado: p.enunciado,
    respostaValor: UIState.respostas[p.codigo] || 'NAO',
    impeditivo: p.impeditivo,
    justificativa: null
  }));

  const obsElem = document.getElementById('txtObservacoesClinicas') || document.getElementById('triagemObservacoes');

  const payload = {
    exameId: UIState.exameSelecionado.id,
    usuarioResponsavelId: UIState.usuarioLogado.id || 1,
    pesoKg: parseFloat(document.getElementById('calcPesoInput').value) || UIState.exameSelecionado.pesoKg,
    volumeContrasteMl: parseFloat(document.getElementById('calcDoseResultado').textContent) || null,
    loteContraste: document.getElementById('calcLoteInput')?.value.trim() || null,
    observacoesClinicas: obsElem?.value.trim() || null,
    concluirELiberar: liberar,
    respostas: respostasDto
  };

  // Se a API estiver online, envia o DTO real
  if (UIState.online) {
    const res = await ApiClient.salvarTriagem(payload);
    if (liberar) {
      await ApiClient.liberarExame(UIState.exameSelecionado.id, UIState.usuarioLogado.id || 1);
    }
    return res;
  }
  return null;
}

function abrirModalConfirmacaoBloqueio() {
  const item = UIState.exameSelecionado;
  if (!item) return;

  document.getElementById('modalBloqueioConfirmacao')?.remove();

  const perguntas = obterPerguntasExameAtual();
  const impeditivos = [];
  perguntas.forEach((p, idx) => {
    if (UIState.respostas[p.codigo] === 'SIM' && p.impeditivo) {
      impeditivos.push({ codigo: p.codigo, index: idx + 1, enunciado: p.enunciado });
    }
  });

  const isTc = item.modalidade === 'TC';
  const explicacaoRisco = isTc
    ? 'Risco iminente de reação anafilactoide / choque alérgico grave ao contraste iodado ou sobrecarga renal aguda irreversível (Nefropatia por Contraste).'
    : 'Risco iminente de deslocamento/atração mecânica por torque ferromagnético, superaquecimento por radiofrequência ou perda total do ritmo cardíaco em campo de 1.5T / 3.0T.';

  const container = document.createElement('div');
  container.id = 'modalBloqueioConfirmacao';
  container.style.position = 'fixed';
  container.style.inset = '0';
  container.style.background = 'rgba(15, 23, 42, 0.85)';
  container.style.backdropFilter = 'blur(8px)';
  container.style.display = 'flex';
  container.style.alignItems = 'center';
  container.style.justifyContent = 'center';
  container.style.zIndex = '10000';
  container.style.padding = '20px';

  const motivosListHtml = impeditivos.length > 0
    ? impeditivos.map(i => `
        <div style="background: rgba(239, 68, 68, 0.15); border-left: 4px solid var(--coral-500); padding: 10px 14px; border-radius: 4px; margin-bottom: 8px;">
          <div style="font-weight: 700; color: #f87171; font-size: 0.9rem;">
            <i class="fa-solid fa-triangle-exclamation"></i> [Q${i.index}] ${i.enunciado}
          </div>
          <div style="font-size: 0.78rem; color: #cbd5e1; margin-top: 4px;">
            Classificação: <strong>Contraindicação Absoluta de Segurança Radiológica</strong>
          </div>
        </div>
      `).join('')
    : `
        <div style="background: rgba(239, 68, 68, 0.15); border-left: 4px solid var(--coral-500); padding: 10px 14px; border-radius: 4px; margin-bottom: 8px; font-size: 0.85rem; color: #fca5a5;">
          Bloqueio clínico acionado pelo operador com base em contraindicação presencial.
        </div>
      `;

  const textoCondutaPadrao = impeditivos.length > 0
    ? `Exame NÃO REALIZADO devido a contraindicação absoluta identificada na triagem (${impeditivos.map(i => `Q${i.index}`).join(', ')}). Risco à integridade física do paciente. Paciente orientado a retornar com médico assistente para conduta médica alternativa.`
    : `Exame NÃO REALIZADO por contraindicação ou recusa clínica na triagem pré-exame. Orientado a contatar médico solicitante.`;

  container.innerHTML = `
    <div style="background: #0f172a; border: 1px solid rgba(239, 68, 68, 0.5); border-radius: 14px; max-width: 680px; width: 100%; max-height: 90vh; overflow-y: auto; padding: 26px 30px; box-shadow: 0 25px 60px -15px rgba(239, 68, 68, 0.35); color: #f8fafc; font-family: var(--font-body, sans-serif);">
      
      <!-- Cabeçalho de Alerta -->
      <div style="display: flex; align-items: flex-start; gap: 16px; margin-bottom: 20px; border-bottom: 1px solid rgba(239, 68, 68, 0.25); padding-bottom: 16px;">
        <div style="width: 50px; height: 50px; border-radius: 50%; background: rgba(239, 68, 68, 0.2); border: 2px solid var(--coral-500); color: #f87171; display: flex; align-items: center; justify-content: center; font-size: 1.6rem; flex-shrink: 0;">
          <i class="fa-solid fa-ban"></i>
        </div>
        <div>
          <h3 style="font-family: var(--font-title); font-size: 1.3rem; font-weight: 800; color: #fff; margin: 0;">
            Bloqueio de Segurança Clínica Mandatório
          </h3>
          <p style="font-size: 0.85rem; color: #fca5a5; margin-top: 4px;">
            Este exame NÃO PODE ser liberado por apresentar critérios de contraindicação absoluta (Protocolo de Aviação).
          </p>
        </div>
      </div>

      <!-- Seção 1: Por que está sendo recusado -->
      <div style="margin-bottom: 18px;">
        <div style="font-size: 0.8rem; font-weight: 700; color: #f87171; text-transform: uppercase; letter-spacing: 0.5px; margin-bottom: 8px;">
          1. Motivo(s) da Recusa e Impedimento Clínico
        </div>
        ${motivosListHtml}
        <div style="background: rgba(30, 41, 59, 0.7); border: 1px solid rgba(148, 163, 184, 0.2); border-radius: 6px; padding: 10px 14px; font-size: 0.82rem; color: #94a3b8;">
          <strong style="color: #38bdf8;">Justificativa Médica Radiológica:</strong> ${explicacaoRisco}
        </div>
      </div>

      <!-- Seção 2: Conferência dos Dados do Paciente e Exame -->
      <div style="margin-bottom: 18px;">
        <div style="font-size: 0.8rem; font-weight: 700; color: #38bdf8; text-transform: uppercase; letter-spacing: 0.5px; margin-bottom: 8px;">
          2. Conferência Obrigatória dos Dados do Exame
        </div>
        <div style="background: rgba(30, 41, 59, 0.85); border: 1px solid rgba(56, 189, 248, 0.25); border-radius: 8px; padding: 14px 16px;">
          <div style="display: grid; grid-template-columns: repeat(auto-fit, minmax(180px, 1fr)); gap: 10px; font-size: 0.85rem;">
            <div>
              <span style="color: #64748b; font-size: 0.75rem; display: block; text-transform: uppercase; font-weight: 600;">Paciente:</span>
              <strong style="color: #fff;">${item.pacienteNome}</strong>
            </div>
            <div>
              <span style="color: #64748b; font-size: 0.75rem; display: block; text-transform: uppercase; font-weight: 600;">Atendimento / CPF:</span>
              <span style="color: #38bdf8; font-family: var(--font-mono); font-weight: 700;">#${item.atendimento}</span> | ${item.documentoCpf}
            </div>
            <div>
              <span style="color: #64748b; font-size: 0.75rem; display: block; text-transform: uppercase; font-weight: 600;">Idade / Sexo:</span>
              <span>${item.idade} anos | ${item.sexo === 'M' ? 'Masculino' : 'Feminino'}</span>
            </div>
            <div>
              <span style="color: #64748b; font-size: 0.75rem; display: block; text-transform: uppercase; font-weight: 600;">Procedimento Agendado:</span>
              <strong style="color: #fff;">${item.procedimento}</strong>
            </div>
            <div>
              <span style="color: #64748b; font-size: 0.75rem; display: block; text-transform: uppercase; font-weight: 600;">Modalidade / Sala:</span>
              <span class="badge-modalidade ${item.modalidade === 'TC' ? 'badge-tc' : 'badge-rm'}" style="margin-right: 6px;">${item.modalidade}</span> Sala ${item.sala}
            </div>
            <div>
              <span style="color: #64748b; font-size: 0.75rem; display: block; text-transform: uppercase; font-weight: 600;">Médico Solicitante / Convênio:</span>
              <span>${item.medicoSolicitante || 'Dr. Roberto Albuquerque'} (${item.convenio || 'Unimed'})</span>
            </div>
          </div>
        </div>
      </div>

      <!-- Seção 3: Conduta / Justificativa do Operador -->
      <div style="margin-bottom: 18px;">
        <label for="txtCondutaBloqueio" style="display: block; font-size: 0.8rem; font-weight: 700; color: #e2e8f0; text-transform: uppercase; letter-spacing: 0.5px; margin-bottom: 6px;">
          3. Conduta Operacional Registrada no Laudo
        </label>
        <textarea id="txtCondutaBloqueio" class="form-input" rows="2" style="width: 100%; font-size: 0.85rem; border-color: rgba(239, 68, 68, 0.4);">${textoCondutaPadrao}</textarea>
      </div>

      <!-- Checkbox de Confirmação -->
      <div style="background: rgba(239, 68, 68, 0.1); border: 1px solid rgba(239, 68, 68, 0.3); border-radius: 8px; padding: 12px 14px; margin-bottom: 22px; display: flex; align-items: flex-start; gap: 10px;">
        <input type="checkbox" id="chkConfirmaDadosBloqueio" style="width: 18px; height: 18px; margin-top: 2px; cursor: pointer; accent-color: var(--coral-500);">
        <label for="chkConfirmaDadosBloqueio" style="font-size: 0.85rem; color: #f8fafc; cursor: pointer; line-height: 1.4;">
          <strong>Confirmo que os dados do paciente foram conferidos verbalmente</strong> e que as contraindicações impeditivas foram ratificadas junto ao paciente/acompanhante.
        </label>
      </div>

      <!-- Ações do Modal -->
      <div style="display: flex; justify-content: flex-end; align-items: center; gap: 12px; border-top: 1px solid rgba(148, 163, 184, 0.2); padding-top: 16px;">
        <button type="button" class="btn btn--secondary" onclick="document.getElementById('modalBloqueioConfirmacao').remove()">
          <i class="fa-solid fa-arrow-left"></i> Voltar e Revisar
        </button>
        <button type="button" class="btn-bloquear-final" id="btnConfirmarBloqueioModal" disabled style="opacity: 0.5; cursor: not-allowed;">
          <i class="fa-solid fa-ban"></i> Confirmar Bloqueio &amp; Encerrar Ficha
        </button>
      </div>
    </div>
  `;

  document.body.appendChild(container);

  const chk = document.getElementById('chkConfirmaDadosBloqueio');
  const btnConfirmar = document.getElementById('btnConfirmarBloqueioModal');

  chk.addEventListener('change', () => {
    if (chk.checked) {
      btnConfirmar.removeAttribute('disabled');
      btnConfirmar.style.opacity = '1';
      btnConfirmar.style.cursor = 'pointer';
    } else {
      btnConfirmar.setAttribute('disabled', 'true');
      btnConfirmar.style.opacity = '0.5';
      btnConfirmar.style.cursor = 'not-allowed';
    }
  });

  btnConfirmar.addEventListener('click', async () => {
    if (!chk.checked) return;

    btnConfirmar.disabled = true;
    btnConfirmar.innerHTML = '<i class="fa-solid fa-spinner fa-spin"></i> Encerrando Ficha...';

    const conduta = document.getElementById('txtCondutaBloqueio')?.value.trim() || textoCondutaPadrao;
    const obsElem = document.getElementById('txtObservacoesClinicas');
    if (obsElem) obsElem.value = conduta;

    try {
      // 1. Salva a triagem na API (gera o arquivo físico e grava o semáforo vermelho)
      const res = await persistirTriagem(false);

      // 2. Chama a API de cancelamento para registrar o status BLOQUEADO_SEGURANCA
      if (UIState.online) {
        await ApiClient.cancelarExame(item.id, UIState.usuarioLogado.id || 1, conduta);
      }

      // 3. Atualiza estado local
      item.status = 'BLOQUEADO_SEGURANCA';
      const itemFila = UIState.exames.find(e => e.id === item.id);
      if (itemFila) itemFila.status = 'BLOQUEADO_SEGURANCA';

      // 4. Remove o modal de confirmação
      document.getElementById('modalBloqueioConfirmacao')?.remove();

      showToast(`Exame #${item.atendimento} BLOQUEADO por segurança clínica. Ficha encerrada!`, 'success');

      // 5. Recarrega a fila e volta para a Worklist
      await carregarWorklist();
      trocarView('view-worklist');

      // 6. Exibe comprovante do laudo arquivado se disponível
      const caminho = res?.caminhoArquivoArmazenado;
      if (caminho) {
        exibirModalArquivoSalvo(caminho);
      }
    } catch (err) {
      showToast(`Erro ao bloquear exame: ${err.message}`, 'error');
      btnConfirmar.disabled = false;
      btnConfirmar.innerHTML = '<i class="fa-solid fa-ban"></i> Confirmar Bloqueio &amp; Encerrar Ficha';
    }
  });
}

function exibirModalArquivoSalvo(caminho) {
  document.getElementById('savedDocModal')?.remove();

  const container = document.createElement('div');
  container.id = 'savedDocModal';
  container.style.position = 'fixed';
  container.style.inset = '0';
  container.style.background = 'rgba(15, 23, 42, 0.75)';
  container.style.backdropFilter = 'blur(6px)';
  container.style.display = 'flex';
  container.style.alignItems = 'center';
  container.style.justifyContent = 'center';
  container.style.zIndex = '9999';

  container.innerHTML = `
    <div style="background: #0f172a; border: 1px solid rgba(148, 163, 184, 0.3); border-radius: 12px; max-width: 620px; width: 90%; padding: 28px; box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.7); color: #f8fafc;">
      <div style="display: flex; align-items: center; gap: 14px; margin-bottom: 16px;">
        <div style="width: 48px; height: 48px; border-radius: 50%; background: rgba(16, 185, 129, 0.2); color: #34d399; display: flex; align-items: center; justify-content: center; font-size: 1.5rem;">
          <i class="fa-solid fa-file-circle-check"></i>
        </div>
        <div>
          <h3 style="font-family: var(--font-title); font-size: 1.25rem; font-weight: 700; color: #fff;">Ficha Clínica Arquivada com Sucesso!</h3>
          <p style="font-size: 0.85rem; color: #94a3b8; margin-top: 2px;">Documento timbrado oficial gerado e salvo na pasta Documentos do usuário.</p>
        </div>
      </div>

      <div style="background: rgba(30, 41, 59, 0.8); border: 1px solid rgba(56, 189, 248, 0.3); border-radius: 8px; padding: 14px 16px; margin-bottom: 20px;">
        <div style="font-size: 0.75rem; color: #38bdf8; text-transform: uppercase; font-weight: 700; letter-spacing: 0.5px;">Caminho Gravado no Banco de Dados (caminho_arquivo_armazenado):</div>
        <div style="font-family: var(--font-mono); font-size: 0.85rem; color: #34d399; word-break: break-all; margin-top: 6px; user-select: all;">
          ${caminho}
        </div>
      </div>

      <div style="display: flex; justify-content: flex-end; gap: 12px;">
        <button type="button" class="btn btn--secondary" onclick="document.getElementById('savedDocModal').remove()">
          <i class="fa-solid fa-xmark"></i> Fechar
        </button>
        <button type="button" class="btn btn--primary" onclick="window.print()">
          <i class="fa-solid fa-print"></i> Imprimir Laudo
        </button>
      </div>
    </div>
  `;

  document.body.appendChild(container);
}

function recalcularDose() {
  const peso = parseFloat(document.getElementById('calcPesoInput')?.value) || 0;
  const isTc = UIState.exameSelecionado?.modalidade === 'TC';

  const cfg = JSON.parse(localStorage.getItem('cfg_clinica') || '{}');
  const proporcao = isTc ? (cfg.iodoProporcao || 1.5) : (cfg.gadoProporcao || 0.2);
  const teto = isTc ? (cfg.iodoTeto || 150) : (cfg.gadoTeto || 25);

  let dose = Math.round(peso * proporcao * 10) / 10;
  if (dose > teto) dose = teto;

  const resultadoElem = document.getElementById('calcDoseResultado');
  const detalhesElem = document.getElementById('calcDoseDetalhes');
  if (resultadoElem) resultadoElem.textContent = dose > 0 ? dose.toFixed(1) : '0.0';
  if (detalhesElem) {
    detalhesElem.textContent = 
      `Proporção clínica recomendada: ${proporcao} mL / kg (${isTc ? `Contraste Iodado (Teto ${teto} mL)` : `Gadolínio (Teto ${teto} mL)`}).`;
  }

  const resumoDose = document.getElementById('assinaturaVolumeResumo');
  if (resumoDose) resumoDose.textContent = `${dose > 0 ? dose.toFixed(1) : '0.0'} mL`;
}

/* ==============================================================================
   8. AUDITORIA CLÍNICA
   ============================================================================== */
async function carregarAuditoria() {
  const tbody = document.getElementById('auditoriaTableBody');
  if (!tbody) return;

  tbody.innerHTML = `
    <tr>
      <td colspan="5" style="text-align: center; padding: 24px; color: var(--text-muted);">
        <i class="fa-solid fa-spinner fa-spin"></i> Carregando registros auditáveis...
      </td>
    </tr>
  `;

  try {
    const logs = await ApiClient.obterAuditoria(50);
    if (Array.isArray(logs) && logs.length > 0) {
      tbody.innerHTML = logs.map(l => {
        const isAlerta = l.tipoEvento.includes('BLOQUEIO') || l.tipoEvento.includes('FALHA') || l.tipoEvento.includes('CONTRAINDICADA');
        const badgeColor = isAlerta ? 'var(--coral-400)' : 'var(--emerald-400)';

        return `
          <tr>
            <td style="font-family: var(--font-mono); font-size: 0.85rem;">
              ${new Date(l.criadoEm).toLocaleString()}
            </td>
            <td><strong>${l.usuarioNome || `Usuário #${l.usuarioId}`}</strong></td>
            <td>
              <span style="font-family: var(--font-mono); font-size: 0.82rem; color: ${badgeColor}; font-weight: 700;">
                ${l.tipoEvento}
              </span>
            </td>
            <td style="font-size: 0.88rem;">${l.detalhes}</td>
            <td style="font-family: var(--font-mono); font-size: 0.82rem; color: var(--text-muted);">
              ${l.ipOrigem || '127.0.0.1'}
            </td>
          </tr>
        `;
      }).join('');
      return;
    }
  } catch (err) {
    console.warn('API indisponível para auditoria, exibindo logs locais de demonstração.');
  }

  // Logs locais de demonstração para quando a API estiver offline
  tbody.innerHTML = `
    <tr>
      <td style="font-family: var(--font-mono); font-size: 0.85rem;">25/10/2026 10:15:32</td>
      <td><strong>Lucas Santos (Biomédico)</strong></td>
      <td><span style="color: var(--emerald-400); font-weight: 700;">LOGIN_SUCESSO</span></td>
      <td>Sessão iniciada no posto de triagem TC/RM.</td>
      <td style="font-family: var(--font-mono); color: var(--text-muted);">127.0.0.1</td>
    </tr>
    <tr>
      <td style="font-family: var(--font-mono); font-size: 0.85rem;">25/10/2026 09:42:10</td>
      <td><strong>Mariana Costa (Técnica RM)</strong></td>
      <td><span style="color: var(--coral-400); font-weight: 700;">TRIAGEM_CONTRAINDICADA</span></td>
      <td>Impedimento ferromagnético mandatório detectado (RM_MARCAPASSO = SIM). Exame bloqueado.</td>
      <td style="font-family: var(--font-mono); color: var(--text-muted);">127.0.0.1</td>
    </tr>
  `;
}

/* ==============================================================================
   9. ATALHOS DE TECLADO DA AVIAÇÃO (1 = NÃO, 2 = SIM, TAB = AVANÇAR)
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

// Funções de Feedback Visual (Toast)
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

/* ==============================================================================
   10. MODAL DO BANCO DE DADOS 2 (WORKLIST DO LABORATÓRIO / HIS-RIS)
   ============================================================================== */
async function abrirModalWorklistLab() {
  document.getElementById('modalBanco2Lab')?.remove();

  const container = document.createElement('div');
  container.id = 'modalBanco2Lab';
  container.style.position = 'fixed';
  container.style.inset = '0';
  container.style.background = 'rgba(15, 23, 42, 0.8)';
  container.style.backdropFilter = 'blur(8px)';
  container.style.display = 'flex';
  container.style.alignItems = 'center';
  container.style.justifyContent = 'center';
  container.style.zIndex = '9999';

  container.innerHTML = `
    <div style="background: var(--bg-card); border: 1px solid var(--bg-card-border); border-radius: 14px; max-width: 900px; width: 95%; max-height: 85vh; display: flex; flex-direction: column; box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.7); overflow: hidden;">
      <div style="padding: 20px 24px; border-bottom: 1px solid var(--bg-card-border); display: flex; justify-content: space-between; align-items: center; background: rgba(30, 41, 59, 0.5);">
        <div style="display: flex; align-items: center; gap: 12px;">
          <div style="width: 40px; height: 40px; border-radius: 8px; background: rgba(56, 189, 248, 0.2); color: var(--cyan-400); display: flex; align-items: center; justify-content: center; font-size: 1.25rem;">
            <i class="fa-solid fa-server"></i>
          </div>
          <div>
            <h3 style="font-family: var(--font-title); font-size: 1.15rem; color: #fff;">Banco de Dados 2: Worklist Laboratorial (HIS/RIS)</h3>
            <p style="font-size: 0.82rem; color: var(--text-muted); margin-top: 2px;">Visualização em tempo real dos agendamentos médicos e formulários clínicos associados.</p>
          </div>
        </div>
        <button type="button" class="btn btn--secondary" onclick="document.getElementById('modalBanco2Lab').remove()" style="padding: 6px 10px;">
          <i class="fa-solid fa-xmark"></i>
        </button>
      </div>

      <div style="padding: 20px 24px; overflow-y: auto; flex: 1;" id="modalBanco2Conteudo">
        <div style="text-align: center; padding: 30px; color: var(--text-muted);">
          <i class="fa-solid fa-spinner fa-spin"></i> Carregando agendamentos do Banco 2...
        </div>
      </div>

      <div style="padding: 14px 24px; border-top: 1px solid var(--bg-card-border); display: flex; justify-content: space-between; align-items: center; background: rgba(15, 23, 42, 0.7);">
        <span style="font-size: 0.8rem; color: var(--emerald-400);">
          <i class="fa-solid fa-circle-check"></i> Conexão ativa com o banco da clínica (worklist_lab.db / MySQL)
        </span>
        <button type="button" class="btn btn--secondary" onclick="document.getElementById('modalBanco2Lab').remove()">
          Fechar
        </button>
      </div>
    </div>
  `;

  document.body.appendChild(container);

  try {
    const pedidos = await ApiClient.obterWorklistLaboratorio();
    const lista = Array.isArray(pedidos) ? pedidos : (pedidos.value || []);
    const div = document.getElementById('modalBanco2Conteudo');

    if (!div) return;

    if (lista.length === 0) {
      div.innerHTML = '<p style="text-align: center; color: var(--text-muted);">Nenhum pedido agendado no Banco 2.</p>';
      return;
    }

    div.innerHTML = `
      <table class="worklist-table" style="font-size: 0.84rem;">
        <thead>
          <tr>
            <th>Atendimento</th>
            <th>Paciente &amp; CPF</th>
            <th>Procedimento</th>
            <th>Formulário Especializado</th>
            <th>Região / Lado</th>
            <th>Médico &amp; Convênio</th>
            <th>Status</th>
          </tr>
        </thead>
        <tbody>
          ${lista.map(p => `
            <tr>
              <td><span style="font-family: var(--font-mono); font-weight: 700;">#${p.atendimento}</span></td>
              <td>
                <div style="font-weight: 600; color: #fff;">${p.paciente}</div>
                <div style="font-size: 0.75rem; color: var(--text-muted);">CPF: ${p.cpf}</div>
              </td>
              <td>
                <span class="badge-modalidade ${p.modalidade === 'TC' ? 'badge-tc' : 'badge-rm'}" style="margin-right: 6px;">${p.modalidade}</span>
                ${p.procedimento}
              </td>
              <td>
                <span style="display: inline-block; padding: 2px 8px; border-radius: 4px; font-weight: 700; font-size: 0.75rem; background: rgba(56, 189, 248, 0.15); color: var(--cyan-400); border: 1px solid rgba(56, 189, 248, 0.3);">
                  ${p.tipoFormulario}
                </span>
              </td>
              <td><span style="font-weight: 600; color: var(--sky-400);">${p.segmentoLado || 'Geral'}</span></td>
              <td>
                <div>${p.medico}</div>
                <div style="font-size: 0.75rem; color: var(--text-muted);">${p.convenio}</div>
              </td>
              <td><span class="badge-status status-aguardando">${p.status}</span></td>
            </tr>
          `).join('')}
        </tbody>
      </table>
    `;
  } catch (err) {
    const div = document.getElementById('modalBanco2Conteudo');
    if (div) {
      div.innerHTML = `<p style="color: var(--coral-400); text-align: center;">Erro ao consultar Banco 2: ${err.message}</p>`;
    }
  }
}

/* ==============================================================================
   11. ATALHOS DE TECLADO OPERACIONAIS ([1] = NÃO, [2] = SIM)
   ============================================================================== */
function initAtalhosTeclado() {
  document.addEventListener('keydown', (e) => {
    const activeView = document.querySelector('.view-panel:not(.hidden)');
    if (activeView?.id !== 'view-triagem') return;

    // Se estiver digitando em um input ou textarea, não intercepta
    if (['INPUT', 'TEXTAREA'].includes(document.activeElement?.tagName)) return;

    if (e.key === '1' || e.key === '2') {
      const activeCard = document.activeElement?.closest('.checklist-card') 
        || document.querySelector('.checklist-card:focus')
        || document.querySelector('.checklist-card');

      if (!activeCard) return;

      const codigo = activeCard.getAttribute('data-codigo');
      if (!codigo) return;

      if (e.key === '1') {
        definirResposta(codigo, 'NAO');
      } else if (e.key === '2') {
        definirResposta(codigo, 'SIM');
      }
    }
  });
}

/* ==============================================================================
   12. PAINEL ADMINISTRATIVO E GESTÃO DO SISTEMA (PERFIL ADMIN)
   ============================================================================== */
function initAdminView() {
  // Sincronizar exames para hoje
  document.getElementById('btnAdminSincronizarHoje')?.addEventListener('click', async () => {
    try {
      const res = await ApiClient.sincronizarExamesHojeAdmin();
      showToast(res.message || 'Exames sincronizados para a data de hoje!', 'success');
      await carregarWorklist();
      await carregarAdminView();
    } catch (err) {
      showToast(`Erro ao sincronizar: ${err.message}`, 'error');
    }
  });

  // Exportar auditoria em CSV
  document.getElementById('btnAdminExportarCsv')?.addEventListener('click', async () => {
    try {
      const logs = await ApiClient.obterAuditoria(200);
      exportarCsvAuditoria(logs);
      showToast('Relatório de auditoria exportado com sucesso!', 'success');
    } catch (err) {
      showToast(`Erro ao exportar auditoria: ${err.message}`, 'error');
    }
  });

  // Modal Novo Operador
  document.getElementById('btnAdminAbrirModalNovoUsuario')?.addEventListener('click', () => {
    abrirModalNovoUsuario();
  });

  // Form de Parâmetros Clínicos
  document.getElementById('formAdminParametrosClinicos')?.addEventListener('submit', (e) => {
    e.preventDefault();
    const config = {
      iodoProporcao: parseFloat(document.getElementById('cfgIodoProporcao').value) || 1.5,
      iodoTeto: parseFloat(document.getElementById('cfgIodoTeto').value) || 150,
      gadoProporcao: parseFloat(document.getElementById('cfgGadoProporcao').value) || 0.2,
      gadoTeto: parseFloat(document.getElementById('cfgGadoTeto').value) || 25,
      creatininaAlerta: parseFloat(document.getElementById('cfgCreatininaAlerta').value) || 1.5,
      jejumHoras: parseInt(document.getElementById('cfgJejumHoras').value) || 4
    };
    localStorage.setItem('cfg_clinica', JSON.stringify(config));
    recalcularDose();
    showToast('Parâmetros clínicos e tetos institucionais salvos com sucesso!', 'success');
  });

  carregarParametrosClinicosForm();
}

async function carregarAdminView() {
  try {
    const usuarios = await ApiClient.listarUsuariosAdmin();
    renderizarAdminUsuarios(usuarios);
    const countElem = document.getElementById('adminStatOperadores');
    if (countElem && Array.isArray(usuarios)) countElem.textContent = usuarios.length;
  } catch (err) {
    console.warn('Erro ao listar usuários admin:', err);
  }

  try {
    const statusBancos = await ApiClient.obterStatusBancos();
    const stat1 = document.getElementById('adminStatBanco1');
    const stat2 = document.getElementById('adminStatBanco2');
    if (stat1) stat1.textContent = statusBancos.banco1_aplicacao?.statusConexao === 'CONECTADO' ? 'Online' : 'Conectado';
    if (stat2) stat2.textContent = `${statusBancos.banco2_worklist_lab?.pedidosAgendados || 0} exames`;
  } catch {}

  const countExames = document.getElementById('adminStatExamesTotal');
  if (countExames) countExames.textContent = UIState.exames.length;
  renderizarAdminExames();
}

function renderizarAdminUsuarios(usuarios) {
  const tbody = document.getElementById('adminUsuariosTableBody');
  if (!tbody || !Array.isArray(usuarios)) return;

  tbody.innerHTML = usuarios.map(u => {
    const isAtivo = u.statusUsuario;
    const statusBadge = isAtivo
      ? '<span class="badge-status status-liberado"><i class="fa-solid fa-circle-check"></i> Ativo</span>'
      : '<span class="badge-status status-bloqueado"><i class="fa-solid fa-circle-xmark"></i> Inativo</span>';

    const perfilBadgeClass = u.perfil === 'ADMIN' ? 'badge-tc' : 'badge-rm';

    return `
      <tr>
        <td><strong>${u.nome}</strong></td>
        <td><span style="font-family: var(--font-mono); color: var(--cyan-400);">${u.login}</span></td>
        <td><span class="badge-modalidade ${perfilBadgeClass}">${u.perfil}</span></td>
        <td>${statusBadge}</td>
        <td><span style="font-size: 0.85rem; color: var(--text-muted);">${new Date(u.criadoEm).toLocaleDateString()}</span></td>
        <td style="text-align: right;">
          <button type="button" class="btn btn--secondary" style="font-size: 0.78rem; padding: 4px 8px; margin-right: 6px;" onclick="adminToggleStatusUsuario(${u.id})">
            <i class="fa-solid ${isAtivo ? 'fa-user-slash' : 'fa-user-check'}"></i> ${isAtivo ? 'Inativar' : 'Ativar'}
          </button>
          <button type="button" class="btn btn--secondary" style="font-size: 0.78rem; padding: 4px 8px;" onclick="adminResetSenhaUsuario(${u.id})">
            <i class="fa-solid fa-key"></i> Reset Senha
          </button>
        </td>
      </tr>
    `;
  }).join('');
}

function renderizarAdminExames() {
  const tbody = document.getElementById('adminExamesTableBody');
  if (!tbody) return;

  tbody.innerHTML = UIState.exames.map(item => {
    const isLiberado = item.status === 'LIBERADO';
    const isBloqueado = item.status === 'BLOQUEADO_SEGURANCA';

    const statusBadge = isLiberado
      ? '<span class="badge-status status-liberado"><i class="fa-solid fa-check-circle"></i> Liberado</span>'
      : (isBloqueado
        ? '<span class="badge-status status-bloqueado"><i class="fa-solid fa-ban"></i> Bloqueado</span>'
        : '<span class="badge-status status-aguardando"><i class="fa-solid fa-clock"></i> Aguardando</span>');

    return `
      <tr>
        <td><span style="font-family: var(--font-mono); font-weight: 700;">#${item.atendimento}</span></td>
        <td><strong>${item.pacienteNome}</strong></td>
        <td><span class="badge-modalidade ${item.modalidade === 'TC' ? 'badge-tc' : 'badge-rm'}">${item.modalidade}</span></td>
        <td><span style="font-size: 0.85rem;">${item.procedimento}</span></td>
        <td><span style="font-family: var(--font-mono); font-weight: 700; color: var(--cyan-400);">${item.sala}</span></td>
        <td>${statusBadge}</td>
        <td style="text-align: right;">
          <button type="button" class="btn btn--secondary" style="font-size: 0.78rem; padding: 4px 8px; margin-right: 6px;" onclick="adminAlterarSala(${item.id}, '${item.sala}')">
            <i class="fa-solid fa-door-open"></i> Trocar Sala
          </button>
          <button type="button" class="btn btn--secondary" style="font-size: 0.78rem; padding: 4px 8px; border-color: rgba(239, 68, 68, 0.3); color: #f87171;" onclick="adminResetStatusExame(${item.id})">
            <i class="fa-solid fa-rotate-left"></i> Resetar p/ Aguardando
          </button>
        </td>
      </tr>
    `;
  }).join('');
}

async function adminToggleStatusUsuario(id) {
  try {
    const res = await ApiClient.toggleStatusUsuarioAdmin(id);
    showToast(res.message || 'Status do operador atualizado!', 'success');
    await carregarAdminView();
  } catch (err) {
    showToast(`Erro ao alterar status: ${err.message}`, 'error');
  }
}

async function adminResetSenhaUsuario(id) {
  if (!confirm('Deseja redefinir a senha deste operador para a senha inicial padrão "123456"?')) return;
  try {
    const res = await ApiClient.resetSenhaUsuarioAdmin(id);
    showToast(res.message || 'Senha resetada para 123456 com sucesso!', 'success');
  } catch (err) {
    showToast(`Erro ao resetar senha: ${err.message}`, 'error');
  }
}

async function adminResetStatusExame(id) {
  if (!confirm('Deseja retornar este exame para AGUARDANDO_TRIAGEM para novo teste?')) return;
  try {
    const res = await ApiClient.resetStatusExameAdmin(id);
    showToast(res.message || 'Exame retornado para Aguardando Triagem!', 'success');
    await carregarWorklist();
    await carregarAdminView();
  } catch (err) {
    showToast(`Erro ao resetar status do exame: ${err.message}`, 'error');
  }
}

async function adminAlterarSala(id, salaAtual) {
  const novaSala = prompt(`Informe a nova sala para este exame (Sala atual: ${salaAtual}):`, salaAtual);
  if (!novaSala || novaSala.trim() === salaAtual) return;

  try {
    const res = await ApiClient.alterarSalaExameAdmin(id, novaSala.trim());
    showToast(res.message || 'Sala alterada com sucesso!', 'success');
    await carregarWorklist();
    await carregarAdminView();
  } catch (err) {
    showToast(`Erro ao alterar sala: ${err.message}`, 'error');
  }
}

function abrirModalNovoUsuario() {
  document.getElementById('modalNovoUsuarioAdmin')?.remove();

  const container = document.createElement('div');
  container.id = 'modalNovoUsuarioAdmin';
  container.style.position = 'fixed';
  container.style.inset = '0';
  container.style.background = 'rgba(15, 23, 42, 0.8)';
  container.style.backdropFilter = 'blur(6px)';
  container.style.display = 'flex';
  container.style.alignItems = 'center';
  container.style.justifyContent = 'center';
  container.style.zIndex = '9999';

  container.innerHTML = `
    <div style="background: #0f172a; border: 1px solid rgba(56, 189, 248, 0.4); border-radius: 12px; max-width: 520px; width: 90%; padding: 26px; box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.7); color: #f8fafc;">
      <div style="display: flex; align-items: center; gap: 12px; margin-bottom: 20px;">
        <div style="width: 44px; height: 44px; border-radius: 50%; background: rgba(56, 189, 248, 0.2); color: var(--cyan-400); display: flex; align-items: center; justify-content: center; font-size: 1.3rem;">
          <i class="fa-solid fa-user-plus"></i>
        </div>
        <div>
          <h3 style="font-family: var(--font-title); font-size: 1.2rem; font-weight: 700; color: #fff;">Cadastrar Novo Operador</h3>
          <p style="font-size: 0.82rem; color: var(--text-muted); margin-top: 2px;">Crie acesso para Biomédico, Técnico de RM ou Estagiário de Biomedicina.</p>
        </div>
      </div>

      <form id="formNovoOperadorAdmin">
        <div class="form-field" style="margin-bottom: 14px;">
          <label class="form-label" for="novoOpNome">Nome Completo do Operador</label>
          <input type="text" id="novoOpNome" class="form-input" placeholder="Ex: Lucas Gabriel Rocha" required>
        </div>

        <div class="form-field" style="margin-bottom: 14px;">
          <label class="form-label" for="novoOpLogin">Login de Acesso</label>
          <input type="text" id="novoOpLogin" class="form-input" placeholder="Ex: lucas.rocha" required>
        </div>

        <div class="form-field" style="margin-bottom: 14px;">
          <label class="form-label" for="novoOpSenha">Senha Inicial</label>
          <input type="password" id="novoOpSenha" class="form-input" value="123456" required>
        </div>

        <div class="form-field" style="margin-bottom: 22px;">
          <label class="form-label" for="novoOpPerfil">Perfil de Acesso</label>
          <select id="novoOpPerfil" class="form-input" style="cursor: pointer;">
            <option value="1">Estagiário de Biomedicina (ESTAGIARIO)</option>
            <option value="2">Técnico em Radiologia (TECNICO)</option>
            <option value="3">Biomédico Especialista (BIOMEDICO)</option>
            <option value="4">Administrador do Sistema (ADMIN)</option>
          </select>
        </div>

        <div style="display: flex; justify-content: flex-end; gap: 10px;">
          <button type="button" class="btn btn--secondary" onclick="document.getElementById('modalNovoUsuarioAdmin').remove()">
            Cancelar
          </button>
          <button type="submit" class="btn btn--primary" id="btnSalvarNovoOp">
            <i class="fa-solid fa-check"></i> Cadastrar Operador
          </button>
        </div>
      </form>
    </div>
  `;

  document.body.appendChild(container);

  document.getElementById('formNovoOperadorAdmin').addEventListener('submit', async (e) => {
    e.preventDefault();
    const btnSalvar = document.getElementById('btnSalvarNovoOp');
    btnSalvar.disabled = true;
    btnSalvar.innerHTML = '<i class="fa-solid fa-spinner fa-spin"></i> Cadastrando...';

    const payload = {
      nome: document.getElementById('novoOpNome').value.trim(),
      login: document.getElementById('novoOpLogin').value.trim(),
      senha: document.getElementById('novoOpSenha').value || '123456',
      perfil: parseInt(document.getElementById('novoOpPerfil').value) || 1
    };

    try {
      await ApiClient.criarUsuarioAdmin(payload);
      showToast(`Operador "${payload.nome}" cadastrado com sucesso!`, 'success');
      container.remove();
      await carregarAdminView();
    } catch (err) {
      showToast(`Erro ao criar operador: ${err.message}`, 'error');
      btnSalvar.disabled = false;
      btnSalvar.innerHTML = '<i class="fa-solid fa-check"></i> Cadastrar Operador';
    }
  });
}

function carregarParametrosClinicosForm() {
  const cfg = JSON.parse(localStorage.getItem('cfg_clinica') || '{}');
  if (cfg.iodoProporcao) document.getElementById('cfgIodoProporcao').value = cfg.iodoProporcao;
  if (cfg.iodoTeto) document.getElementById('cfgIodoTeto').value = cfg.iodoTeto;
  if (cfg.gadoProporcao) document.getElementById('cfgGadoProporcao').value = cfg.gadoProporcao;
  if (cfg.gadoTeto) document.getElementById('cfgGadoTeto').value = cfg.gadoTeto;
  if (cfg.creatininaAlerta) document.getElementById('cfgCreatininaAlerta').value = cfg.creatininaAlerta;
  if (cfg.jejumHoras) document.getElementById('cfgJejumHoras').value = cfg.jejumHoras;
}

function exportarCsvAuditoria(logs) {
  if (!Array.isArray(logs) || logs.length === 0) {
    showToast('Nenhum registro de auditoria disponível para exportação.', 'info');
    return;
  }
  const cabecalho = ['ID', 'Data_Hora', 'Usuario', 'Tipo_Evento', 'Detalhes', 'IP_Origem'];
  const linhas = logs.map(l => [
    l.id,
    `"${new Date(l.criadoEm).toLocaleString()}"`,
    `"${(l.usuarioNome || l.usuarioId || '').toString().replace(/"/g, '""')}"`,
    `"${(l.tipoEvento || '').toString().replace(/"/g, '""')}"`,
    `"${(l.detalhes || '').toString().replace(/"/g, '""')}"`,
    `"${l.ipOrigem || '127.0.0.1'}"`
  ]);
  const csvContent = 'data:text/csv;charset=utf-8,\uFEFF' + [cabecalho.join(';'), ...linhas.map(e => e.join(';'))].join('\n');
  const encodedUri = encodeURI(csvContent);
  const link = document.createElement('a');
  link.setAttribute('href', encodedUri);
  link.setAttribute('download', `Auditoria_TriagemImagem_${new Date().toISOString().slice(0, 10)}.csv`);
  document.body.appendChild(link);
  link.click();
  link.remove();
}

