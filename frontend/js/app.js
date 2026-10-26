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

// Catálogo Central de Informações e Metadados dos Protocolos Anatômicos
const PROTOCOLOS_CATALOGO_INFO = {
  RM_COLUNA: {
    id: 'RM_COLUNA',
    nome: 'RM Coluna (Cervical / Torácica / Lombar)',
    modalidade: 'RM',
    icone: 'fa-bone',
    corIcone: 'var(--purple-400)',
    isCustom: false
  },
  RM_BRACO_ANTEBRACO: {
    id: 'RM_BRACO_ANTEBRACO',
    nome: 'RM Braço / Antebraço (Membro Superior)',
    modalidade: 'RM',
    icone: 'fa-hand-dots',
    corIcone: 'var(--purple-400)',
    isCustom: false
  },
  RM_BACIA_QUADRIL: {
    id: 'RM_BACIA_QUADRIL',
    nome: 'RM Bacia / Quadril (Articulação Coxofemoral)',
    modalidade: 'RM',
    icone: 'fa-person',
    corIcone: 'var(--purple-400)',
    isCustom: false
  },
  TC_TORAX: {
    id: 'TC_TORAX',
    nome: 'TC Tórax com Contraste Iodado',
    modalidade: 'TC',
    icone: 'fa-lungs',
    corIcone: 'var(--cyan-400)',
    isCustom: false
  },
  TC_ABDOMEN: {
    id: 'TC_ABDOMEN',
    nome: 'TC Abdômen Total com Contraste',
    modalidade: 'TC',
    icone: 'fa-circle-radiation',
    corIcone: 'var(--cyan-400)',
    isCustom: false
  }
};

function carregarProtocolosCustomizados() {
  try {
    const salvos = JSON.parse(localStorage.getItem('app_protocolos_custom') || '{}');
    Object.keys(salvos).forEach(key => {
      const p = salvos[key];
      PROTOCOLOS_CATALOGO_INFO[key] = {
        id: p.id,
        nome: p.nome,
        modalidade: p.modalidade,
        icone: p.icone || (p.modalidade === 'TC' ? 'fa-lungs' : 'fa-brain'),
        corIcone: p.modalidade === 'TC' ? 'var(--cyan-400)' : 'var(--purple-400)',
        isCustom: true
      };
      if (Array.isArray(p.perguntas)) {
        PROTOCOLOS_CHECKLIST[key] = p.perguntas;
      }
      if (p.metadata && typeof METADATA_REGRAS_CLINICAS !== 'undefined') {
        Object.assign(METADATA_REGRAS_CLINICAS, p.metadata);
      }
      if (typeof CATALOGO_PROCEDIMENTOS_FORMULARIO !== 'undefined') {
        CATALOGO_PROCEDIMENTOS_FORMULARIO[key] = Array.isArray(p.procedimentos) ? p.procedimentos : [];
      }
    });
  } catch (err) {
    console.warn('Erro ao carregar protocolos customizados do localStorage:', err);
  }
}

function salvarProtocolosCustomizadosStorage() {
  try {
    const salvos = {};
    Object.keys(PROTOCOLOS_CATALOGO_INFO).forEach(key => {
      const item = PROTOCOLOS_CATALOGO_INFO[key];
      if (item.isCustom) {
        const metadataMap = {};
        const perguntas = PROTOCOLOS_CHECKLIST[key] || [];
        perguntas.forEach(perg => {
          if (METADATA_REGRAS_CLINICAS[perg.codigo]) {
            metadataMap[perg.codigo] = METADATA_REGRAS_CLINICAS[perg.codigo];
          }
        });

        salvos[key] = {
          id: item.id,
          nome: item.nome,
          modalidade: item.modalidade,
          icone: item.icone,
          perguntas: perguntas,
          metadata: metadataMap,
          procedimentos: CATALOGO_PROCEDIMENTOS_FORMULARIO[key] || []
        };
      }
    });
    localStorage.setItem('app_protocolos_custom', JSON.stringify(salvos));
  } catch (err) {
    console.warn('Erro ao salvar protocolos customizados no localStorage:', err);
  }
}

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
  carregarProtocolosCustomizados();
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

  // Inicialização do Seletor Customizado de Protocolos (Fundo Escuro e Bordas Arredondadas)
  initCustomSelectProtocolo();

  carregarParametrosClinicosForm();
}

function initCustomSelectProtocolo() {
  const container = document.getElementById('customSelectProtocolo');
  const trigger = document.getElementById('customSelectTrigger');
  const menu = document.getElementById('customSelectMenu');
  const hiddenSelect = document.getElementById('selectProtocoloAdmin');

  if (!container || !trigger || !menu) return;

  menu.style.display = 'none';

  if (!container.dataset.initialized) {
    container.dataset.initialized = 'true';
    trigger.addEventListener('click', (e) => {
      e.stopPropagation();
      const isOpen = container.classList.toggle('is-open');
      menu.style.display = isOpen ? 'block' : 'none';
    });

    document.addEventListener('click', (e) => {
      if (!container.contains(e.target)) {
        container.classList.remove('is-open');
        menu.style.display = 'none';
      }
    });
  }

  atualizarOpcoesSeletorProtocolos(hiddenSelect?.value || 'RM_COLUNA');
}

function atualizarOpcoesSeletorProtocolos(protocoloSelecionadoId) {
  const container = document.getElementById('customSelectProtocolo');
  const menu = document.getElementById('customSelectMenu');
  const label = document.getElementById('customSelectLabel');
  const hiddenSelect = document.getElementById('selectProtocoloAdmin');
  if (!menu || !hiddenSelect) return;

  const currentVal = protocoloSelecionadoId || hiddenSelect.value || 'RM_COLUNA';
  const protocolos = Object.values(PROTOCOLOS_CATALOGO_INFO);

  // Renderiza opções dinâmicas no menu customizado
  menu.innerHTML = protocolos.map(p => {
    const isSel = p.id === currentVal;
    const badgeCustom = p.isCustom 
      ? '<span style="font-size: 0.68rem; background: rgba(56,189,248,0.18); color: var(--cyan-400); padding: 1px 6px; border-radius: 4px; margin-left: 6px; font-weight: 700; border: 1px solid rgba(56,189,248,0.3);">CUSTOM</span>' 
      : '';
    return `
      <div class="custom-select-option ${isSel ? 'is-selected' : ''}" data-value="${p.id}">
        <span><i class="fa-solid ${p.icone}" style="margin-right: 8px; color: ${p.corIcone};"></i> ${p.nome}${badgeCustom}</span>
        <i class="fa-solid fa-check check-icon" style="color: var(--cyan-400); display: ${isSel ? 'inline-block' : 'none'};"></i>
      </div>
    `;
  }).join('');

  // Atualiza também o native hidden select
  hiddenSelect.innerHTML = protocolos.map(p => `<option value="${p.id}" ${p.id === currentVal ? 'selected' : ''}>${p.nome}</option>`).join('');
  hiddenSelect.value = currentVal;

  // Atualiza o label visível
  const protoAtual = PROTOCOLOS_CATALOGO_INFO[currentVal] || protocolos[0];
  if (label && protoAtual) {
    label.innerHTML = `<i class="fa-solid ${protoAtual.icone}" style="color: ${protoAtual.corIcone}; margin-right: 8px;"></i> ${protoAtual.nome}`;
  }

  // Vincula cliques nas opções
  menu.querySelectorAll('.custom-select-option').forEach(option => {
    option.onclick = (e) => {
      e.stopPropagation();
      const val = option.getAttribute('data-value');
      const textSpan = option.querySelector('span');

      menu.querySelectorAll('.custom-select-option').forEach(o => {
        o.classList.remove('is-selected');
        const chk = o.querySelector('.check-icon');
        if (chk) chk.style.display = 'none';
      });

      option.classList.add('is-selected');
      const checkIcon = option.querySelector('.check-icon');
      if (checkIcon) checkIcon.style.display = 'inline-block';

      if (label && textSpan) {
        label.innerHTML = textSpan.innerHTML;
      }

      container?.classList.remove('is-open');
      menu.style.display = 'none';

      if (hiddenSelect) {
        hiddenSelect.value = val;
        hiddenSelect.dispatchEvent(new Event('change'));
      }
      renderizarDicionarioProtocolos(val);
    };
  });
}

async function carregarAdminView() {
  initCustomSelectProtocolo();

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

  const selectProto = document.getElementById('selectProtocoloAdmin');
  renderizarDicionarioProtocolos(selectProto?.value || 'RM_COLUNA');
}

const METADATA_REGRAS_CLINICAS = {
  // RM Coluna
  COLUNA_PREPARO: {
    tipo: 'INFORMATIVA',
    gatilho: 'Se NÃO -> Alerta de conferência de preparo',
    fundamento: 'Garante que o paciente retirou adornos metálicos, próteses móveis e realizou esvaziamento vesical.'
  },
  COLUNA_MARCAPASSO: {
    tipo: 'IMPEDITIVA',
    gatilho: 'Se SIM -> 🔴 Semáforo Vermelho + Bloqueio do Exame',
    fundamento: 'O campo magnético de 1.5T/3.0T induz voltagem nos cabos-eletrodos do marcapasso, com risco de arritmia letal e aquecimento miocárdico.'
  },
  COLUNA_CLIPE_ANEURISMA: {
    tipo: 'IMPEDITIVA',
    gatilho: 'Se SIM -> 🔴 Semáforo Vermelho + Bloqueio do Exame',
    fundamento: 'Clipes ferromagnéticos sofrem torque magnético com força suficiente para lacerar a parede arterial cerebral.'
  },
  COLUNA_IMPLANTE_COCLEAR: {
    tipo: 'IMPEDITIVA',
    gatilho: 'Se SIM -> 🔴 Semáforo Vermelho + Bloqueio do Exame',
    fundamento: 'O ímã interno do implante coclear pode desmagnetizar, deslocar-se cirurgicamente ou sofrer aquecimento por radiofrequência.'
  },
  COLUNA_SINTOMA_DOR_IRRADIADA: {
    tipo: 'INFORMATIVA',
    gatilho: 'Se SIM -> Direcionamento do protocolo (cortes axiais focados)',
    fundamento: 'Identifica radiculopatia por hérnia discal ou estenose de canal para planejamento de sequências T2/STIR.'
  },
  COLUNA_CIRURGIA_PREVIA: {
    tipo: 'INFORMATIVA',
    gatilho: 'Se SIM -> Ajuste de banda passante contra artefatos',
    fundamento: 'Materiais de artrodese (titânio) geram artefatos de suscetibilidade magnética que exigem sequências específicas (WARP/SEMAC).'
  },
  COLUNA_TRAUMA: {
    tipo: 'INFORMATIVA',
    gatilho: 'Se SIM -> Prioridade para detecção de edema ósseo',
    fundamento: 'Orienta sequência STIR/TIRM de alta sensibilidade para fraturas por compressão ou luxação vertebral.'
  },
  COLUNA_OSTEOPOROSE: {
    tipo: 'INFORMATIVA',
    gatilho: 'Se SIM -> Cuidado no posicionamento e manuseio',
    fundamento: 'Mobilização cuidadosa do paciente para evitar fraturas iatrogênicas ao transferir da maca para a mesa.'
  },
  COLUNA_TUMOR_QUIMIO: {
    tipo: 'INFORMATIVA',
    gatilho: 'Se SIM -> Indicação de estudo com contraste gadolínio',
    fundamento: 'Pesquisa de metástases ósseas espinhais e infiltração epidural com realce paramagnético.'
  },
  COLUNA_CLAUSTROFOBIA: {
    tipo: 'ALERTA',
    gatilho: 'Se SIM -> 🟡 Semáforo Amarelo (Acolhimento / Sedação)',
    fundamento: 'Exame de coluna exige 25-35 minutos imóvel; claustrofobia severa requer acolhimento, iluminação e suporte anestésico.'
  },

  // RM Braço
  BRACO_PREPARO: {
    tipo: 'INFORMATIVA',
    gatilho: 'Se NÃO -> Conferir vestimenta e retirada de metais',
    fundamento: 'Retirada de anéis, relógios, pulseiras e roupas com zíperes condutores.'
  },
  BRACO_MARCAPASSO: {
    tipo: 'IMPEDITIVA',
    gatilho: 'Se SIM -> 🔴 Semáforo Vermelho + Bloqueio do Exame',
    fundamento: 'Contraindicação absoluta em campo magnético estático elevado.'
  },
  BRACO_CORPO_ESTRANHO_METAL: {
    tipo: 'IMPEDITIVA',
    gatilho: 'Se SIM -> 🔴 Semáforo Vermelho + Exige RX de Órbitas',
    fundamento: 'Soldadores ou operadores de tornos mecânicos com risco de farpas intraoculares que provocam cegueira.'
  },
  BRACO_SINTOMAS: {
    tipo: 'INFORMATIVA',
    gatilho: 'Se SIM -> Planejamento do campo de visão (FOV)',
    fundamento: 'Delimita a extensão da bobina dedicada para o punho, cotovelo ou ombro.'
  },
  BRACO_ESPORTES: {
    tipo: 'INFORMATIVA',
    gatilho: 'Se SIM -> Investigação de lesão por sobrecarga',
    fundamento: 'Suspeita de epicondilite lateral/medial, lesões de bíceps distal ou estiramento ligamentar.'
  },
  BRACO_RUPTURA_TENDAO: {
    tipo: 'INFORMATIVA',
    gatilho: 'Se SIM -> Prioridade de protocolo músculo-esquelético',
    fundamento: 'Sequências de alta resolução espacial para avaliar retração tendínea e atrofia gordurosa.'
  },
  BRACO_TRAUMA_CIRURGIA: {
    tipo: 'INFORMATIVA',
    gatilho: 'Se SIM -> Avaliação de consolidação e implantes',
    fundamento: 'Identifica consolidação viciosa, pseudoartrose ou artefatos metálicos cirúrgicos prévios.'
  },
  BRACO_CLAUSTROFOBIA: {
    tipo: 'ALERTA',
    gatilho: 'Se SIM -> 🟡 Semáforo Amarelo (Acolhimento)',
    fundamento: 'Posicionamento ou tubo fechado pode aumentar a ansiedade do paciente.'
  },

  // RM Bacia/Quadril
  BACIA_PREPARO: {
    tipo: 'INFORMATIVA',
    gatilho: 'Se NÃO -> Orientação e higienização',
    fundamento: 'Esvaziamento vesical prévio para evitar artefatos de movimento por repleção urinária.'
  },
  BACIA_MARCAPASSO: {
    tipo: 'IMPEDITIVA',
    gatilho: 'Se SIM -> 🔴 Semáforo Vermelho + Bloqueio do Exame',
    fundamento: 'Contraindicação absoluta ao magneto de alto campo.'
  },
  BACIA_CLIPE_ANEURISMA: {
    tipo: 'IMPEDITIVA',
    gatilho: 'Se SIM -> 🔴 Semáforo Vermelho + Bloqueio do Exame',
    fundamento: 'Risco de torque e sangramento subaracnóideo maciço por deslocamento ferromagnético.'
  },
  BACIA_USO_CORTICOIDE: {
    tipo: 'INFORMATIVA',
    gatilho: 'Se SIM -> Investigação de Osteonecrose Avascular',
    fundamento: 'Corticoterapia prolongada é a causa não-traumática mais frequente de colapso da cabeça do fêmur.'
  },
  BACIA_DOENCA_INFANCIA: {
    tipo: 'INFORMATIVA',
    gatilho: 'Se SIM -> Correlação de displasia residual',
    fundamento: 'Avalia osteoartrite secundária a Legg-Calvé-Perthes ou displasia congênita do quadril.'
  },
  BACIA_RUPTURA_TENDAO: {
    tipo: 'INFORMATIVA',
    gatilho: 'Se SIM -> FOV focado nos tendões glúteos e labrum',
    fundamento: 'Diferenciação entre bursite trocantérica e lesões de glúteo médio/mínimo (manguito rotador do quadril).'
  },
  BACIA_TRAUMA_CIRURGIA: {
    tipo: 'INFORMATIVA',
    gatilho: 'Se SIM -> Avaliação de artroplastia ou fratura oculta',
    fundamento: 'Detecção de fraturas subcapital femoral incompletas ou afrouxamento asséptico de prótese.'
  },
  BACIA_CLAUSTROFOBIA: {
    tipo: 'ALERTA',
    gatilho: 'Se SIM -> 🟡 Semáforo Amarelo (Acolhimento)',
    fundamento: 'O paciente entra com o centro do corpo no isocentro do gantry fechado.'
  },

  // TC Tórax & Abdômen
  TC_ALERGIA_IODO: {
    tipo: 'IMPEDITIVA',
    gatilho: 'Se SIM -> 🔴 Semáforo Vermelho + Bloqueio de Contraste',
    fundamento: 'Histórico de anafilaxia ao contraste iodado. Exige pré-medicação oral de 12-24h ou cancelamento.'
  },
  TC_INSUFICIENCIA_RENAL: {
    tipo: 'IMPEDITIVA',
    gatilho: 'Se SIM -> 🔴 Semáforo Vermelho + Bloqueio até dosar Creatinina',
    fundamento: 'Prevenção de Nefropatia Induzida por Contraste (NIC) em pacientes com eGFR < 30 mL/min/1.73m².'
  },
  TC_METFORMINA: {
    tipo: 'ALERTA',
    gatilho: 'Se SIM -> 🟡 Semáforo Amarelo + Suspensão 48h pós-exame',
    fundamento: 'Em caso de lesão renal pós-contraste, acúmulo de metformina induz acidose lática potencialmente fatal.'
  },
  TC_JEJUM: {
    tipo: 'ALERTA',
    gatilho: 'Se NÃO -> 🟡 Semáforo Amarelo + Risco de Êmese',
    fundamento: 'Contraste iodado hiperosmolar pode provocar náusea súbita; jejum de 4h evita broncoaspiração traqueal.'
  },
  TC_GESTAO: {
    tipo: 'ALERTA',
    gatilho: 'Se SIM -> 🟡 Semáforo Amarelo + Princípio ALARA / Radiologista',
    fundamento: 'Radiação ionizante é potencialmente teratogênica, especialmente no 1º trimestre da embriogênese.'
  },
  TC_CIRURGIA_ABDOMINAL: {
    tipo: 'INFORMATIVA',
    gatilho: 'Se SIM -> Correlação com fios de sutura e pneumoperitônio',
    fundamento: 'Orienta diagnóstico diferencial entre gás cirúrgico fisiológico e perfuração visceral ativa.'
  }
};

// Catálogo de Procedimentos Médicos (TUSS / RIS / HIS) integrados a cada formulário
const CATALOGO_PROCEDIMENTOS_FORMULARIO = {
  RM_COLUNA: [
    { codigoTuss: '41101153', nome: 'RM de Coluna Cervical', regiao: 'Cervical' },
    { codigoTuss: '41101161', nome: 'RM de Coluna Torácica (Dorsal)', regiao: 'Torácica' },
    { codigoTuss: '41101170', nome: 'RM de Coluna Lombar / Lombossacra', regiao: 'Lombar' },
    { codigoTuss: '41101188', nome: 'RM de Articulações Sacroilíacas', regiao: 'Pélvica' }
  ],
  RM_BRACO_ANTEBRACO: [
    { codigoTuss: '41101013', nome: 'RM de Braço / Úmero', regiao: 'Braço' },
    { codigoTuss: '41101021', nome: 'RM de Cotovelo', regiao: 'Cotovelo' },
    { codigoTuss: '41101030', nome: 'RM de Antebraço', regiao: 'Antebraço' },
    { codigoTuss: '41101048', nome: 'RM de Punho', regiao: 'Punho' },
    { codigoTuss: '41101056', nome: 'RM de Ombro / Manguito Rotador', regiao: 'Ombro' }
  ],
  RM_BACIA_QUADRIL: [
    { codigoTuss: '41101218', nome: 'RM de Bacia / Pelve Óssea', regiao: 'Bacia' },
    { codigoTuss: '41101226', nome: 'RM de Articulação Coxofemoral (Quadril)', regiao: 'Quadril' },
    { codigoTuss: '41101234', nome: 'RM de Pelve com Contraste (Próstata / Útero)', regiao: 'Pelve' }
  ],
  TC_TORAX: [
    { codigoTuss: '41001095', nome: 'TC de Tórax com Contraste', regiao: 'Tórax' },
    { codigoTuss: '41001109', nome: 'TC de Tórax de Alta Resolução (TCAR)', regiao: 'Tórax' },
    { codigoTuss: '41001117', nome: 'Angiotomografia Arterial de Tórax (TEP / Aorta)', regiao: 'Vascular' }
  ],
  TC_ABDOMEN: [
    { codigoTuss: '41001125', nome: 'TC de Abdômen Total com Contraste', regiao: 'Abdômen' },
    { codigoTuss: '41001133', nome: 'TC de Abdômen Superior', regiao: 'Abdômen Superior' },
    { codigoTuss: '41001141', nome: 'TC de Pelve / Pelve e Abdômen', regiao: 'Pelve' },
    { codigoTuss: '41001150', nome: 'Enterotomografia com Contraste Oral e Venoso', regiao: 'Entérico' }
  ]
};

function renderizarDicionarioProtocolos(protocoloId) {
  const tbody = document.getElementById('adminProtocoloRegrasTableBody');
  if (!tbody) return;

  const perguntas = PROTOCOLOS_CHECKLIST[protocoloId] || [];
  const info = PROTOCOLOS_CATALOGO_INFO[protocoloId];
  const modalidade = (info?.modalidade || (protocoloId.startsWith('TC') ? 'TC' : 'RM')) === 'TC' 
    ? 'Tomografia Computadorizada (TC)' 
    : 'Ressonância Magnética (RM)';

  const badgeMod = document.getElementById('protocoloModalidadeBadge');
  if (badgeMod) badgeMod.textContent = modalidade;

  const btnExcluir = document.getElementById('btnExcluirProtocoloCustom');
  if (btnExcluir) {
    if (info && info.isCustom) {
      btnExcluir.classList.remove('hidden');
    } else {
      btnExcluir.classList.add('hidden');
    }
  }

  // 1. Procedimentos do Catálogo Integrados
  const listaProcsContainer = document.getElementById('listaProcedimentosIntegrados');
  const badgeQtdProcs = document.getElementById('badgeQtdProcedimentos');
  const procs = CATALOGO_PROCEDIMENTOS_FORMULARIO[protocoloId] || [];

  if (badgeQtdProcs) {
    badgeQtdProcs.textContent = `${procs.length} ${procs.length === 1 ? 'Procedimento' : 'Procedimentos'}`;
  }

  if (listaProcsContainer) {
    if (procs.length === 0) {
      listaProcsContainer.innerHTML = '<div style="font-size: 0.8rem; color: var(--text-muted); padding: 8px;">Nenhum procedimento associado.</div>';
    } else {
      listaProcsContainer.innerHTML = procs.map(proc => `
        <div style="display: flex; justify-content: space-between; align-items: center; background: rgba(255,255,255,0.03); border: 1px solid var(--border-subtle); border-radius: 6px; padding: 6px 10px;">
          <div style="display: flex; align-items: center; gap: 8px;">
            <code style="font-family: var(--font-mono); font-size: 0.72rem; color: var(--cyan-400); background: rgba(56,189,248,0.1); padding: 2px 6px; border-radius: 4px;">TUSS ${proc.codigoTuss}</code>
            <span style="font-size: 0.82rem; font-weight: 500; color: #fff;">${proc.nome}</span>
          </div>
          <div style="display: flex; align-items: center; gap: 6px;">
            <span class="badge-status" style="font-size: 0.7rem; background: rgba(255,255,255,0.05); color: var(--text-secondary);">${proc.regiao}</span>
            <button type="button" title="Editar Procedimento" onclick="adminEditarProcedimento('${protocoloId}', '${proc.codigoTuss}')" style="background: none; border: none; color: var(--cyan-400); cursor: pointer; padding: 2px 5px; font-size: 0.8rem;">
              <i class="fa-solid fa-pen"></i>
            </button>
            <button type="button" title="Desvincular do Formulário" onclick="adminExcluirProcedimento('${protocoloId}', '${proc.codigoTuss}')" style="background: none; border: none; color: var(--coral-400); cursor: pointer; padding: 2px 5px; font-size: 0.8rem;">
              <i class="fa-solid fa-trash"></i>
            </button>
          </div>
        </div>
      `).join('');
    }
  }

  // 2. Exames Ativos na Worklist com este Formulário
  const listaWorklistContainer = document.getElementById('listaWorklistProtocolo');
  const badgeQtdWorklist = document.getElementById('badgeQtdWorklistProtocolo');
  const examesNaWorklist = UIState.exames.filter(e => {
    const formDoExame = e.tipoFormulario || (e.modalidade === 'TC' ? 'TC_TORAX' : 'RM_COLUNA');
    return formDoExame === protocoloId;
  });

  if (badgeQtdWorklist) {
    badgeQtdWorklist.textContent = `${examesNaWorklist.length} ${examesNaWorklist.length === 1 ? 'Paciente' : 'Pacientes'}`;
  }

  if (listaWorklistContainer) {
    if (examesNaWorklist.length === 0) {
      listaWorklistContainer.innerHTML = '<div style="font-size: 0.8rem; color: var(--text-muted); padding: 8px;"><i class="fa-solid fa-check-circle" style="color: var(--emerald-400); margin-right: 4px;"></i>Nenhum paciente na fila de hoje com este formulário.</div>';
    } else {
      listaWorklistContainer.innerHTML = examesNaWorklist.map(ex => {
        let badgeClass = 'status-aguardando';
        let label = 'Aguardando';
        if (ex.status === 'LIBERADO') { badgeClass = 'status-liberado'; label = 'Liberado'; }
        else if (ex.status === 'BLOQUEADO_SEGURANCA') { badgeClass = 'status-bloqueado'; label = 'Bloqueado'; }
        else if (ex.status === 'EM_TRIAGEM') { badgeClass = 'status-em-triagem'; label = 'Em Triagem'; }

        return `
          <div style="display: flex; justify-content: space-between; align-items: center; background: rgba(255,255,255,0.03); border: 1px solid var(--border-subtle); border-radius: 6px; padding: 6px 10px;">
            <div style="display: flex; align-items: center; gap: 8px;">
              <span style="font-family: var(--font-mono); font-size: 0.75rem; color: var(--amber-400); font-weight: 700;">#${ex.atendimento}</span>
              <span style="font-size: 0.82rem; font-weight: 500; color: #fff;">${ex.pacienteNome}</span>
              <span style="font-size: 0.72rem; color: var(--text-muted);">(${ex.sala} • ${ex.horario})</span>
            </div>
            <span class="badge-status ${badgeClass}" style="font-size: 0.7rem;">${label}</span>
          </div>
        `;
      }).join('');
    }
  }

  // 3. Tabela de Regras Clínicas Individuais
  let impeditivas = 0;
  let alertas = 0;
  let informativas = 0;

  tbody.innerHTML = '';

  perguntas.forEach(p => {
    const meta = METADATA_REGRAS_CLINICAS[p.codigo] || {
      tipo: p.impeditivo ? 'IMPEDITIVA' : 'INFORMATIVA',
      gatilho: p.impeditivo ? 'Se SIM -> 🔴 Bloqueio de Segurança' : 'Critério informativo clínico',
      fundamento: 'Coleta de dados clínicos para direcionamento técnico do exame.'
    };

    let badgeClass = 'status-liberado';
    let labelTipo = '🟢 Normal';
    if (meta.tipo === 'IMPEDITIVA' || p.impeditivo) {
      badgeClass = 'status-bloqueado';
      labelTipo = '🔴 Impeditiva';
      impeditivas++;
    } else if (meta.tipo === 'ALERTA') {
      badgeClass = 'status-aguardando';
      labelTipo = '🟡 Alerta';
      alertas++;
    } else {
      informativas++;
    }

    const tr = document.createElement('tr');
    tr.innerHTML = `
      <td><code style="font-family: var(--font-mono); font-size: 0.8rem; color: var(--cyan-400); background: rgba(56,189,248,0.1); padding: 3px 8px; border-radius: 4px;">${p.codigo}</code></td>
      <td style="font-size: 0.88rem; font-weight: 500; color: #fff;">${p.enunciado}</td>
      <td><span class="badge-status ${badgeClass}">${labelTipo}</span></td>
      <td style="font-size: 0.85rem; color: ${meta.tipo === 'IMPEDITIVA' ? 'var(--coral-400)' : meta.tipo === 'ALERTA' ? 'var(--amber-400)' : 'var(--text-secondary)'}; font-weight: 600;">${meta.gatilho}</td>
      <td style="font-size: 0.82rem; color: var(--text-muted); line-height: 1.4;">${meta.fundamento}</td>
    `;
    tbody.appendChild(tr);
  });

  const elImp = document.getElementById('protocoloTotalImpeditivas');
  const elAlert = document.getElementById('protocoloTotalAlertas');
  const elInfo = document.getElementById('protocoloTotalInformativas');

  if (elImp) elImp.textContent = `${impeditivas} ${impeditivas === 1 ? 'Pergunta' : 'Perguntas'}`;
  if (elAlert) elAlert.textContent = `${alertas} ${alertas === 1 ? 'Pergunta' : 'Perguntas'}`;
  if (elInfo) elInfo.textContent = `${informativas} ${informativas === 1 ? 'Pergunta' : 'Perguntas'}`;
}

/**
 * Validador Estrito de Conformidade e Unicidade de Códigos TUSS e RIS
 * Regra: Obrigatório, formato válido (mín. 4 caracteres) e NÃO REPETIDO no catálogo global
 */
function validarCodigoTussERis(codigoTuss, codigoOriginal = null) {
  if (!codigoTuss || !codigoTuss.trim()) {
    return { 
      valido: false, 
      mensagem: 'O Código TUSS / RIS é estritamente obrigatório.' 
    };
  }

  const codLimpo = codigoTuss.trim().toUpperCase();

  if (codLimpo.length < 4) {
    return { 
      valido: false, 
      mensagem: 'O Código TUSS / RIS deve possuir no mínimo 4 caracteres (Ex: 41101014 ou RMCRAN).' 
    };
  }

  // Validação de unicidade no catálogo global de procedimentos
  for (const protoKey of Object.keys(CATALOGO_PROCEDIMENTOS_FORMULARIO)) {
    const procs = CATALOGO_PROCEDIMENTOS_FORMULARIO[protoKey] || [];
    for (const proc of procs) {
      if (proc.codigoTuss && proc.codigoTuss.trim().toUpperCase() === codLimpo) {
        if (codigoOriginal && codigoOriginal.trim().toUpperCase() === codLimpo) {
          continue; // Mesma entidade em edição
        }
        const nomeProto = PROTOCOLOS_CATALOGO_INFO[protoKey]?.nome || protoKey;
        return {
          valido: false,
          mensagem: `Código TUSS/RIS "${codLimpo}" já cadastrado no exame "${proc.nome}" (Protocolo: ${nomeProto}). Códigos duplicados não são permitidos.`,
          duplicadoEm: protoKey,
          procExistente: proc
        };
      }
    }
  }

  return { valido: true, codigoFormatado: codLimpo };
}

function adminAbrirVincularProcedimento() {
  const select = document.getElementById('selectProtocoloAdmin');
  const protocoloId = select ? select.value : 'RM_COLUNA';
  adminAbrirModalVincularProcedimento(protocoloId);
}

function adminAbrirModalVincularProcedimento(protocoloId) {
  document.getElementById('modalVincularProcAdmin')?.remove();

  const info = PROTOCOLOS_CATALOGO_INFO[protocoloId] || { nome: protocoloId, modalidade: 'RM' };

  const container = document.createElement('div');
  container.id = 'modalVincularProcAdmin';
  container.style.position = 'fixed';
  container.style.inset = '0';
  container.style.background = 'rgba(10, 14, 23, 0.85)';
  container.style.backdropFilter = 'blur(10px)';
  container.style.display = 'flex';
  container.style.alignItems = 'center';
  container.style.justifyContent = 'center';
  container.style.zIndex = '9999';

  container.innerHTML = `
    <div style="background: var(--bg-card); border: 1px solid var(--cyan-500); border-radius: 14px; max-width: 540px; width: 95%; box-shadow: 0 25px 60px rgba(0,0,0,0.8), 0 0 25px var(--cyan-glow); overflow: hidden;">
      <div style="padding: 16px 20px; border-bottom: 1px solid var(--bg-card-border); display: flex; justify-content: space-between; align-items: center; background: rgba(30, 41, 59, 0.5);">
        <div style="display: flex; align-items: center; gap: 10px;">
          <div style="width: 36px; height: 36px; border-radius: 8px; background: rgba(56, 189, 248, 0.2); color: var(--cyan-400); display: flex; align-items: center; justify-content: center; font-size: 1.1rem;">
            <i class="fa-solid fa-link"></i>
          </div>
          <div>
            <h3 style="font-family: var(--font-title); font-size: 1.1rem; color: #fff;">Associar Exame ao Formulário</h3>
            <p style="font-size: 0.78rem; color: var(--text-muted); margin-top: 2px;">Protocolo: <strong style="color: var(--cyan-400);">${info.nome}</strong></p>
          </div>
        </div>
        <button type="button" class="btn btn--secondary" onclick="document.getElementById('modalVincularProcAdmin').remove()" style="padding: 5px 8px;">
          <i class="fa-solid fa-xmark"></i>
        </button>
      </div>

      <form id="formVincularProcModal" style="padding: 20px; display: flex; flex-direction: column; gap: 14px;">
        <div class="form-field">
          <label class="form-label" for="vincularProcCodigo">
            <i class="fa-solid fa-barcode" style="color: var(--cyan-400);"></i> Código TUSS / RIS (* Obrigatório & Único):
          </label>
          <input type="text" id="vincularProcCodigo" class="form-input" placeholder="Ex: 41101999 (TUSS ANS) ou RM_OMBRO (RIS)" required>
          <span style="font-size: 0.72rem; color: var(--text-muted); margin-top: 3px;">Validação estrita: não são permitidos códigos repetidos em nenhum protocolo.</span>
        </div>

        <div class="form-field">
          <label class="form-label" for="vincularProcNome">
            <i class="fa-solid fa-file-signature" style="color: var(--cyan-400);"></i> Nome do Exame / Procedimento Médico (* Obrigatório):
          </label>
          <input type="text" id="vincularProcNome" class="form-input" placeholder="Ex: RM de Ombro com Contraste" required>
        </div>

        <div class="form-field">
          <label class="form-label" for="vincularProcRegiao">
            <i class="fa-solid fa-map-pin" style="color: var(--cyan-400);"></i> Região Anatômica / Segmento (* Obrigatório):
          </label>
          <input type="text" id="vincularProcRegiao" class="form-input" placeholder="Ex: Ombro, Mão, Joelho, Crânio" required>
        </div>

        <div style="display: flex; justify-content: flex-end; gap: 10px; margin-top: 8px; padding-top: 12px; border-top: 1px solid rgba(255,255,255,0.08);">
          <button type="button" class="btn btn--secondary" onclick="document.getElementById('modalVincularProcAdmin').remove()">
            Cancelar
          </button>
          <button type="submit" class="btn btn--primary" id="btnSalvarVincularProc">
            <i class="fa-solid fa-check"></i> Associar Procedimento
          </button>
        </div>
      </form>
    </div>
  `;

  document.body.appendChild(container);

  document.getElementById('formVincularProcModal').addEventListener('submit', (e) => {
    e.preventDefault();
    const codigoInput = document.getElementById('vincularProcCodigo').value.trim();
    const nomeInput = document.getElementById('vincularProcNome').value.trim();
    const regiaoInput = document.getElementById('vincularProcRegiao').value.trim();

    if (!nomeInput) {
      showToast('O nome do exame/procedimento é obrigatório.', 'error');
      document.getElementById('vincularProcNome')?.focus();
      return;
    }
    if (!regiaoInput) {
      showToast('A região anatômica é obrigatória.', 'error');
      document.getElementById('vincularProcRegiao')?.focus();
      return;
    }

    // Validação estrita de unicidade e obrigatoriedade TUSS/RIS
    const validacao = validarCodigoTussERis(codigoInput);
    if (!validacao.valido) {
      showToast(validacao.mensagem, 'error');
      document.getElementById('vincularProcCodigo')?.focus();
      return;
    }

    if (!CATALOGO_PROCEDIMENTOS_FORMULARIO[protocoloId]) {
      CATALOGO_PROCEDIMENTOS_FORMULARIO[protocoloId] = [];
    }

    CATALOGO_PROCEDIMENTOS_FORMULARIO[protocoloId].push({
      codigoTuss: validacao.codigoFormatado,
      nome: nomeInput,
      regiao: regiaoInput
    });

    salvarProtocolosCustomizadosStorage();
    renderizarDicionarioProtocolos(protocoloId);
    container.remove();
    showToast(`Exame "${nomeInput}" (TUSS/RIS ${validacao.codigoFormatado}) associado com sucesso!`, 'success');
  });
}

function adminEditarProcedimento(protocoloId, codigoTuss) {
  const procs = CATALOGO_PROCEDIMENTOS_FORMULARIO[protocoloId] || [];
  const proc = procs.find(p => p.codigoTuss === codigoTuss);
  if (!proc) return;

  document.getElementById('modalEditarProcAdmin')?.remove();

  const container = document.createElement('div');
  container.id = 'modalEditarProcAdmin';
  container.style.position = 'fixed';
  container.style.inset = '0';
  container.style.background = 'rgba(10, 14, 23, 0.85)';
  container.style.backdropFilter = 'blur(10px)';
  container.style.display = 'flex';
  container.style.alignItems = 'center';
  container.style.justifyContent = 'center';
  container.style.zIndex = '9999';

  container.innerHTML = `
    <div style="background: var(--bg-card); border: 1px solid var(--cyan-500); border-radius: 14px; max-width: 540px; width: 95%; box-shadow: 0 25px 60px rgba(0,0,0,0.8), 0 0 25px var(--cyan-glow); overflow: hidden;">
      <div style="padding: 16px 20px; border-bottom: 1px solid var(--bg-card-border); display: flex; justify-content: space-between; align-items: center; background: rgba(30, 41, 59, 0.5);">
        <div style="display: flex; align-items: center; gap: 10px;">
          <div style="width: 36px; height: 36px; border-radius: 8px; background: rgba(56, 189, 248, 0.2); color: var(--cyan-400); display: flex; align-items: center; justify-content: center; font-size: 1.1rem;">
            <i class="fa-solid fa-pen-to-square"></i>
          </div>
          <div>
            <h3 style="font-family: var(--font-title); font-size: 1.1rem; color: #fff;">Editar Procedimento do Catálogo</h3>
            <p style="font-size: 0.78rem; color: var(--text-muted); margin-top: 2px;">Atualize o código TUSS/RIS, nome ou região anatômica.</p>
          </div>
        </div>
        <button type="button" class="btn btn--secondary" onclick="document.getElementById('modalEditarProcAdmin').remove()" style="padding: 5px 8px;">
          <i class="fa-solid fa-xmark"></i>
        </button>
      </div>

      <form id="formEditarProcModal" style="padding: 20px; display: flex; flex-direction: column; gap: 14px;">
        <div class="form-field">
          <label class="form-label" for="editProcCodigo">
            <i class="fa-solid fa-barcode" style="color: var(--cyan-400);"></i> Código TUSS / RIS (* Obrigatório & Único):
          </label>
          <input type="text" id="editProcCodigo" class="form-input" value="${proc.codigoTuss}" required>
        </div>

        <div class="form-field">
          <label class="form-label" for="editProcNome">
            <i class="fa-solid fa-file-signature" style="color: var(--cyan-400);"></i> Nome do Exame / Procedimento (* Obrigatório):
          </label>
          <input type="text" id="editProcNome" class="form-input" value="${proc.nome}" required>
        </div>

        <div class="form-field">
          <label class="form-label" for="editProcRegiao">
            <i class="fa-solid fa-map-pin" style="color: var(--cyan-400);"></i> Região Anatômica (* Obrigatório):
          </label>
          <input type="text" id="editProcRegiao" class="form-input" value="${proc.regiao}" required>
        </div>

        <div style="display: flex; justify-content: flex-end; gap: 10px; margin-top: 8px; padding-top: 12px; border-top: 1px solid rgba(255,255,255,0.08);">
          <button type="button" class="btn btn--secondary" onclick="document.getElementById('modalEditarProcAdmin').remove()">
            Cancelar
          </button>
          <button type="submit" class="btn btn--primary">
            <i class="fa-solid fa-check"></i> Salvar Alterações
          </button>
        </div>
      </form>
    </div>
  `;

  document.body.appendChild(container);

  document.getElementById('formEditarProcModal').addEventListener('submit', (e) => {
    e.preventDefault();
    const novoCodigo = document.getElementById('editProcCodigo').value.trim();
    const novoNome = document.getElementById('editProcNome').value.trim();
    const novaRegiao = document.getElementById('editProcRegiao').value.trim();

    if (!novoNome) {
      showToast('O nome do exame/procedimento é obrigatório.', 'error');
      return;
    }
    if (!novaRegiao) {
      showToast('A região anatômica é obrigatória.', 'error');
      return;
    }

    // Validação estrita de unicidade permitindo o mesmo código se não foi alterado
    const validacao = validarCodigoTussERis(novoCodigo, proc.codigoTuss);
    if (!validacao.valido) {
      showToast(validacao.mensagem, 'error');
      document.getElementById('editProcCodigo')?.focus();
      return;
    }

    proc.codigoTuss = validacao.codigoFormatado;
    proc.nome = novoNome;
    proc.regiao = novaRegiao;

    salvarProtocolosCustomizadosStorage();
    renderizarDicionarioProtocolos(protocoloId);
    container.remove();
    showToast('Procedimento atualizado com sucesso!', 'success');
  });
}

function adminExcluirProcedimento(protocoloId, codigoTuss) {
  const procs = CATALOGO_PROCEDIMENTOS_FORMULARIO[protocoloId] || [];
  const proc = procs.find(p => p.codigoTuss === codigoTuss);
  if (!proc) return;

  if (!confirm(`Deseja desvincular o procedimento "${proc.nome}" (TUSS ${proc.codigoTuss}) deste formulário?`)) return;

  CATALOGO_PROCEDIMENTOS_FORMULARIO[protocoloId] = procs.filter(p => p.codigoTuss !== codigoTuss);
  salvarProtocolosCustomizadosStorage();
  renderizarDicionarioProtocolos(protocoloId);
  showToast('Procedimento desvinculado com sucesso!', 'success');
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
        <td style="text-align: right; white-space: nowrap;">
          <button type="button" class="btn btn--secondary" style="font-size: 0.78rem; padding: 4px 8px; margin-right: 4px; border-color: rgba(56, 189, 248, 0.4); color: var(--cyan-400);" onclick="adminAbrirModalEditarExame(${item.id})">
            <i class="fa-solid fa-pen-to-square"></i> Editar
          </button>
          <button type="button" class="btn btn--secondary" style="font-size: 0.78rem; padding: 4px 8px; margin-right: 4px;" onclick="adminAlterarSala(${item.id}, '${item.sala}')">
            <i class="fa-solid fa-door-open"></i> Sala
          </button>
          <button type="button" class="btn btn--secondary" style="font-size: 0.78rem; padding: 4px 8px; margin-right: 4px; border-color: rgba(245, 158, 11, 0.3); color: var(--amber-400);" onclick="adminResetStatusExame(${item.id})">
            <i class="fa-solid fa-rotate-left"></i> Resetar
          </button>
          <button type="button" class="btn btn--secondary" style="font-size: 0.78rem; padding: 4px 8px; border-color: rgba(239, 68, 68, 0.4); color: #f87171;" onclick="adminExcluirExame(${item.id})">
            <i class="fa-solid fa-trash-can"></i> Excluir
          </button>
        </td>
      </tr>
    `;
  }).join('');
}

function adminAbrirModalEditarExame(exameId) {
  document.getElementById('modalEditarExameAdmin')?.remove();

  const item = UIState.exames.find(e => e.id === exameId);
  if (!item) {
    showToast('Exame não encontrado.', 'error');
    return;
  }

  const container = document.createElement('div');
  container.id = 'modalEditarExameAdmin';
  container.style.position = 'fixed';
  container.style.inset = '0';
  container.style.background = 'rgba(10, 14, 23, 0.85)';
  container.style.backdropFilter = 'blur(10px)';
  container.style.display = 'flex';
  container.style.alignItems = 'center';
  container.style.justifyContent = 'center';
  container.style.zIndex = '9999';

  container.innerHTML = `
    <div style="background: var(--bg-card); border: 1px solid var(--cyan-500); border-radius: 16px; max-width: 680px; width: 95%; max-height: 90vh; display: flex; flex-direction: column; box-shadow: 0 25px 60px rgba(0,0,0,0.8), 0 0 25px var(--cyan-glow); overflow: hidden;">
      <div style="padding: 18px 24px; border-bottom: 1px solid var(--bg-card-border); display: flex; justify-content: space-between; align-items: center; background: rgba(30, 41, 59, 0.5);">
        <div style="display: flex; align-items: center; gap: 12px;">
          <div style="width: 38px; height: 38px; border-radius: 8px; background: rgba(56, 189, 248, 0.2); color: var(--cyan-400); display: flex; align-items: center; justify-content: center; font-size: 1.15rem;">
            <i class="fa-solid fa-pen-to-square"></i>
          </div>
          <div>
            <h3 style="font-family: var(--font-title); font-size: 1.15rem; color: #fff;">Editar Dados do Exame #${item.atendimento}</h3>
            <p style="font-size: 0.8rem; color: var(--text-muted); margin-top: 2px;">Atualize dados cadastrais, clínicos ou formulário de triagem associado.</p>
          </div>
        </div>
        <button type="button" class="btn btn--secondary" onclick="document.getElementById('modalEditarExameAdmin').remove()" style="padding: 6px 10px;">
          <i class="fa-solid fa-xmark"></i>
        </button>
      </div>

      <form id="formEditarExameModal" style="padding: 22px 24px; overflow-y: auto; display: flex; flex-direction: column; gap: 16px;">
        <div style="display: grid; grid-template-columns: 2fr 1fr; gap: 14px;">
          <div class="form-field">
            <label class="form-label" for="editExameNome">Nome do Paciente:</label>
            <input type="text" id="editExameNome" class="form-input" value="${item.pacienteNome || ''}" required>
          </div>
          <div class="form-field">
            <label class="form-label" for="editExameCpf">CPF do Paciente:</label>
            <input type="text" id="editExameCpf" class="form-input" value="${item.documentoCpf || ''}">
          </div>
        </div>

        <div style="display: grid; grid-template-columns: 1fr 1fr 1fr; gap: 14px;">
          <div class="form-field">
            <label class="form-label" for="editExamePeso">Peso (Kg):</label>
            <input type="number" step="0.1" id="editExamePeso" class="form-input" value="${item.pesoKg || 70}" required>
          </div>
          <div class="form-field">
            <label class="form-label" for="editExameModalidade">Modalidade:</label>
            <select id="editExameModalidade" class="form-input" style="cursor: pointer;">
              <option value="RM" ${item.modalidade === 'RM' ? 'selected' : ''}>Ressonância (RM)</option>
              <option value="TC" ${item.modalidade === 'TC' ? 'selected' : ''}>Tomografia (TC)</option>
            </select>
          </div>
          <div class="form-field">
            <label class="form-label" for="editExameSala">Sala Operacional:</label>
            <input type="text" id="editExameSala" class="form-input" value="${item.sala || 'RM-01'}" required>
          </div>
        </div>

        <div class="form-field">
          <label class="form-label" for="editExameProcedimento">Descrição do Procedimento:</label>
          <input type="text" id="editExameProcedimento" class="form-input" value="${item.procedimento || ''}" required>
        </div>

        <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 14px;">
          <div class="form-field">
            <label class="form-label" for="editExameSegmento">Segmento / Lateralidade:</label>
            <input type="text" id="editExameSegmento" class="form-input" value="${item.segmentoLado || ''}" placeholder="Ex: Lombar, Direito, Tórax">
          </div>
          <div class="form-field">
            <label class="form-label" for="editExameHorario">Horário Agendado:</label>
            <input type="text" id="editExameHorario" class="form-input" value="${item.horario || '08:00'}" placeholder="HH:mm">
          </div>
        </div>

        <div class="form-field">
          <label class="form-label" for="editExameTipoFormulario">
            <i class="fa-solid fa-file-waveform" style="color: var(--cyan-400);"></i> Formulário / Checklist de Triagem:
          </label>
          <select id="editExameTipoFormulario" class="form-input" style="cursor: pointer;">
            ${Object.values(PROTOCOLOS_CATALOGO_INFO).map(p => `
              <option value="${p.id}" ${item.tipoFormulario === p.id ? 'selected' : ''}>${p.nome}</option>
            `).join('')}
          </select>
        </div>

        <div style="display: flex; justify-content: flex-end; gap: 10px; margin-top: 10px; padding-top: 14px; border-top: 1px solid rgba(255,255,255,0.08);">
          <button type="button" class="btn btn--secondary" onclick="document.getElementById('modalEditarExameAdmin').remove()">
            Cancelar
          </button>
          <button type="submit" class="btn btn--primary" id="btnSalvarEdicaoExame">
            <i class="fa-solid fa-check"></i> Salvar Alterações
          </button>
        </div>
      </form>
    </div>
  `;

  document.body.appendChild(container);

  document.getElementById('formEditarExameModal').addEventListener('submit', async (e) => {
    e.preventDefault();
    const btn = document.getElementById('btnSalvarEdicaoExame');
    btn.disabled = true;
    btn.innerHTML = '<i class="fa-solid fa-spinner fa-spin"></i> Salvando...';

    const novoNome = document.getElementById('editExameNome').value.trim();
    const novoCpf = document.getElementById('editExameCpf').value.trim();
    const novoPeso = parseFloat(document.getElementById('editExamePeso').value) || item.pesoKg;
    const novaMod = document.getElementById('editExameModalidade').value;
    const novoProc = document.getElementById('editExameProcedimento').value.trim();
    const novoSeg = document.getElementById('editExameSegmento').value.trim();
    const novaSala = document.getElementById('editExameSala').value.trim();
    const novoForm = document.getElementById('editExameTipoFormulario').value;
    const novoHorario = document.getElementById('editExameHorario').value.trim();

    const payload = {
      pacienteNome: novoNome,
      documentoCpf: novoCpf,
      pesoKg: novoPeso,
      modalidade: novaMod === 'TC' ? 1 : 2,
      procedimento: novoProc,
      segmentoLado: novoSeg,
      tipoFormulario: novoForm,
      sala: novaSala,
      horario: novoHorario
    };

    try {
      if (UIState.online) {
        await ApiClient.editarExameAdmin(item.id, payload);
      }
      // Atualiza localmente
      item.pacienteNome = novoNome;
      item.documentoCpf = novoCpf;
      item.pesoKg = novoPeso;
      item.modalidade = novaMod;
      item.procedimento = novoProc;
      item.segmentoLado = novoSeg;
      item.tipoFormulario = novoForm;
      item.sala = novaSala;
      item.horario = novoHorario;

      showToast(`Exame #${item.atendimento} atualizado com sucesso!`, 'success');
      container.remove();
      renderizarAdminExames();
      carregarWorklist();
      const selProto = document.getElementById('selectProtocoloAdmin');
      if (selProto) renderizarDicionarioProtocolos(selProto.value);
    } catch (err) {
      showToast(`Erro ao editar exame: ${err.message}`, 'error');
      btn.disabled = false;
      btn.innerHTML = '<i class="fa-solid fa-check"></i> Salvar Alterações';
    }
  });
}

async function adminExcluirExame(exameId) {
  const item = UIState.exames.find(e => e.id === exameId);
  if (!item) return;

  const confirmar = confirm(`Tem certeza que deseja EXCLUIR permanentemente o exame #${item.atendimento} do paciente ${item.pacienteNome}?\n\nEsta ação removerá o agendamento da Worklist.`);
  if (!confirmar) return;

  try {
    if (UIState.online) {
      await ApiClient.excluirExameAdmin(exameId);
    }
    UIState.exames = UIState.exames.filter(e => e.id !== exameId);
    showToast(`Exame #${item.atendimento} excluído com sucesso!`, 'success');
    renderizarAdminExames();
    carregarWorklist();
    const selProto = document.getElementById('selectProtocoloAdmin');
    if (selProto) renderizarDicionarioProtocolos(selProto.value);
  } catch (err) {
    showToast(`Erro ao excluir exame: ${err.message}`, 'error');
  }
}

function adminAbrirModalNovoExame() {
  document.getElementById('modalNovoExameAdmin')?.remove();

  const container = document.createElement('div');
  container.id = 'modalNovoExameAdmin';
  container.style.position = 'fixed';
  container.style.inset = '0';
  container.style.background = 'rgba(10, 14, 23, 0.85)';
  container.style.backdropFilter = 'blur(10px)';
  container.style.display = 'flex';
  container.style.alignItems = 'center';
  container.style.justifyContent = 'center';
  container.style.zIndex = '9999';

  container.innerHTML = `
    <div style="background: var(--bg-card); border: 1px solid var(--cyan-500); border-radius: 16px; max-width: 680px; width: 95%; max-height: 90vh; display: flex; flex-direction: column; box-shadow: 0 25px 60px rgba(0,0,0,0.8), 0 0 25px var(--cyan-glow); overflow: hidden;">
      <div style="padding: 18px 24px; border-bottom: 1px solid var(--bg-card-border); display: flex; justify-content: space-between; align-items: center; background: rgba(30, 41, 59, 0.5);">
        <div style="display: flex; align-items: center; gap: 12px;">
          <div style="width: 38px; height: 38px; border-radius: 8px; background: rgba(56, 189, 248, 0.2); color: var(--cyan-400); display: flex; align-items: center; justify-content: center; font-size: 1.15rem;">
            <i class="fa-solid fa-calendar-plus"></i>
          </div>
          <div>
            <h3 style="font-family: var(--font-title); font-size: 1.15rem; color: #fff;">Novo Agendamento de Exame (Teste Clínico)</h3>
            <p style="font-size: 0.8rem; color: var(--text-muted); margin-top: 2px;">Cadastre um novo paciente na grade para testar triagem, semáforo e contraste.</p>
          </div>
        </div>
        <button type="button" class="btn btn--secondary" onclick="document.getElementById('modalNovoExameAdmin').remove()" style="padding: 6px 10px;">
          <i class="fa-solid fa-xmark"></i>
        </button>
      </div>

      <form id="formNovoExameModal" style="padding: 22px 24px; overflow-y: auto; display: flex; flex-direction: column; gap: 16px;">
        <div style="display: grid; grid-template-columns: 2fr 1fr; gap: 14px;">
          <div class="form-field">
            <label class="form-label" for="novoExameNome">Nome Completo do Paciente:</label>
            <input type="text" id="novoExameNome" class="form-input" placeholder="Ex: Roberto Alencar da Silva" required>
          </div>
          <div class="form-field">
            <label class="form-label" for="novoExameCpf">CPF do Paciente:</label>
            <input type="text" id="novoExameCpf" class="form-input" placeholder="000.000.000-00">
          </div>
        </div>

        <div style="display: grid; grid-template-columns: 1fr 1fr 1fr 1fr; gap: 14px;">
          <div class="form-field">
            <label class="form-label" for="novoExameIdade">Idade:</label>
            <input type="number" id="novoExameIdade" class="form-input" value="42" required>
          </div>
          <div class="form-field">
            <label class="form-label" for="novoExameSexo">Sexo:</label>
            <select id="novoExameSexo" class="form-input" style="cursor: pointer;">
              <option value="M">Masculino</option>
              <option value="F">Feminino</option>
            </select>
          </div>
          <div class="form-field">
            <label class="form-label" for="novoExamePeso">Peso (Kg):</label>
            <input type="number" step="0.1" id="novoExamePeso" class="form-input" value="75.0" required>
          </div>
          <div class="form-field">
            <label class="form-label" for="novoExameModalidade">Modalidade:</label>
            <select id="novoExameModalidade" class="form-input" style="cursor: pointer;" onchange="adminAtualizarFormulariosModalNovo(this.value)">
              <option value="RM">Ressonância (RM)</option>
              <option value="TC">Tomografia (TC)</option>
            </select>
          </div>
        </div>

        <div class="form-field">
          <label class="form-label" for="novoExameProcedimento">Descrição do Procedimento / Exame:</label>
          <input type="text" id="novoExameProcedimento" class="form-input" value="Ressonância Magnética de Coluna Lombar" required>
        </div>

        <div style="display: grid; grid-template-columns: 1fr 1fr 1fr; gap: 14px;">
          <div class="form-field">
            <label class="form-label" for="novoExameSegmento">Segmento / Região:</label>
            <input type="text" id="novoExameSegmento" class="form-input" value="Lombar" placeholder="Ex: Tórax, Lombar, Braço">
          </div>
          <div class="form-field">
            <label class="form-label" for="novoExameSala">Sala Operacional:</label>
            <input type="text" id="novoExameSala" class="form-input" value="RM-01" required>
          </div>
          <div class="form-field">
            <label class="form-label" for="novoExameHorario">Horário:</label>
            <input type="text" id="novoExameHorario" class="form-input" value="11:30" placeholder="HH:mm">
          </div>
        </div>

        <div style="display: grid; grid-template-columns: 1fr 2fr; gap: 14px;">
          <div class="form-field">
            <label class="form-label" for="novoExameCodigoTuss">
              <i class="fa-solid fa-barcode" style="color: var(--cyan-400);"></i> Código TUSS / RIS (* Obrigatório):
            </label>
            <input type="text" id="novoExameCodigoTuss" class="form-input" value="41101170" placeholder="Ex: 41101170 ou 41001095" required>
          </div>
          <div class="form-field">
            <label class="form-label" for="novoExameTipoFormulario">
              <i class="fa-solid fa-file-waveform" style="color: var(--cyan-400);"></i> Formulário / Checklist de Triagem:
            </label>
            <select id="novoExameTipoFormulario" class="form-input" style="cursor: pointer;">
              ${Object.values(PROTOCOLOS_CATALOGO_INFO).filter(p => p.modalidade === 'RM').map(p => `
                <option value="${p.id}">${p.nome}</option>
              `).join('')}
            </select>
          </div>
        </div>

        <div style="display: flex; justify-content: flex-end; gap: 10px; margin-top: 10px; padding-top: 14px; border-top: 1px solid rgba(255,255,255,0.08);">
          <button type="button" class="btn btn--secondary" onclick="document.getElementById('modalNovoExameAdmin').remove()">
            Cancelar
          </button>
          <button type="submit" class="btn btn--primary" id="btnSalvarNovoExame">
            <i class="fa-solid fa-check"></i> Agendar Exame
          </button>
        </div>
      </form>
    </div>
  `;

  document.body.appendChild(container);

  document.getElementById('formNovoExameModal').addEventListener('submit', async (e) => {
    e.preventDefault();
    const btn = document.getElementById('btnSalvarNovoExame');
    btn.disabled = true;
    btn.innerHTML = '<i class="fa-solid fa-spinner fa-spin"></i> Agendando...';

    const modalidade = document.getElementById('novoExameModalidade').value;
    const codigoTussVal = document.getElementById('novoExameCodigoTuss')?.value.trim();

    if (!codigoTussVal) {
      showToast('O código TUSS/RIS do procedimento é obrigatório.', 'error');
      btn.disabled = false;
      btn.innerHTML = '<i class="fa-solid fa-check"></i> Agendar Exame';
      document.getElementById('novoExameCodigoTuss')?.focus();
      return;
    }

    const payload = {
      pacienteNome: document.getElementById('novoExameNome').value.trim(),
      documentoCpf: document.getElementById('novoExameCpf').value.trim(),
      idade: parseInt(document.getElementById('novoExameIdade').value) || 35,
      sexo: document.getElementById('novoExameSexo').value,
      pesoKg: parseFloat(document.getElementById('novoExamePeso').value) || 70,
      modalidade: modalidade === 'TC' ? 1 : 2,
      codigoProcedimento: codigoTussVal,
      procedimento: document.getElementById('novoExameProcedimento').value.trim(),
      segmentoLado: document.getElementById('novoExameSegmento').value.trim(),
      sala: document.getElementById('novoExameSala').value.trim(),
      tipoFormulario: document.getElementById('novoExameTipoFormulario').value,
      horario: document.getElementById('novoExameHorario').value.trim()
    };

    try {
      let novoExameObj;
      if (UIState.online) {
        const res = await ApiClient.criarExameAdmin(payload);
        novoExameObj = res.exame;
      }

      if (!novoExameObj) {
        novoExameObj = {
          id: Date.now(),
          atendimento: 104500 + UIState.exames.length + 1,
          pacienteNome: payload.pacienteNome,
          documentoCpf: payload.documentoCpf,
          pesoKg: payload.pesoKg,
          modalidade: modalidade,
          procedimento: payload.procedimento,
          segmentoLado: payload.segmentoLado,
          sala: payload.sala,
          tipoFormulario: payload.tipoFormulario,
          horario: payload.horario,
          status: 'AGUARDANDO_TRIAGEM'
        };
      }

      UIState.exames.unshift(novoExameObj);
      showToast(`Exame #${novoExameObj.atendimento} agendado com sucesso!`, 'success');
      container.remove();
      renderizarAdminExames();
      carregarWorklist();
      const selProto = document.getElementById('selectProtocoloAdmin');
      if (selProto) renderizarDicionarioProtocolos(selProto.value);
    } catch (err) {
      showToast(`Erro ao criar exame: ${err.message}`, 'error');
      btn.disabled = false;
      btn.innerHTML = '<i class="fa-solid fa-check"></i> Agendar Exame';
    }
  });
}

function adminAtualizarFormulariosModalNovo(modalidade) {
  const sel = document.getElementById('novoExameTipoFormulario');
  const sala = document.getElementById('novoExameSala');
  const proc = document.getElementById('novoExameProcedimento');
  if (!sel) return;

  const opcoes = Object.values(PROTOCOLOS_CATALOGO_INFO).filter(p => p.modalidade === modalidade);
  sel.innerHTML = opcoes.map(p => `<option value="${p.id}">${p.nome}</option>`).join('');

  if (modalidade === 'TC') {
    sel.value = opcoes.some(o => o.id === 'TC_TORAX') ? 'TC_TORAX' : (opcoes[0]?.id || 'TC_TORAX');
    if (sala && sala.value.startsWith('RM')) sala.value = 'TC-01';
    if (proc && proc.value.includes('Ressonância')) proc.value = 'Tomografia Computadorizada de Tórax com Contraste';
  } else {
    sel.value = opcoes.some(o => o.id === 'RM_COLUNA') ? 'RM_COLUNA' : (opcoes[0]?.id || 'RM_COLUNA');
    if (sala && sala.value.startsWith('TC')) sala.value = 'RM-01';
    if (proc && proc.value.includes('Tomografia')) proc.value = 'Ressonância Magnética de Coluna Lombar';
  }
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

/* ==============================================================================
   12. GESTÃO DINÂMICA DE PROTOCOLOS ANATÔMICOS (PAINEL ADMINISTRATIVO)
   ============================================================================== */

const PERGUNTAS_PADRAO_RM = [
  {
    codigoSufixo: 'MARCAPASSO',
    enunciado: 'Possui marcapasso cardíaco ou cardioversor-desfibrilador (CDI)?',
    impeditivo: true,
    tipo: 'IMPEDITIVA',
    gatilho: 'Se SIM -> 🔴 Semáforo Vermelho + Bloqueio do Exame',
    fundamento: 'Indução de corrente e arritmia letal em campo magnético estático.'
  },
  {
    codigoSufixo: 'CLIPE_ANEURISMA',
    enunciado: 'Possui clipes de aneurisma cerebral ou implantes vasculares metálicos?',
    impeditivo: true,
    tipo: 'IMPEDITIVA',
    gatilho: 'Se SIM -> 🔴 Semáforo Vermelho + Bloqueio do Exame',
    fundamento: 'Torque magnético sobre clipe com risco de laceração vascular e hemorragia.'
  },
  {
    codigoSufixo: 'IMPLANTE_COCLEAR',
    enunciado: 'Possui implante coclear ou neuroestimulador implantado?',
    impeditivo: true,
    tipo: 'IMPEDITIVA',
    gatilho: 'Se SIM -> 🔴 Semáforo Vermelho + Bloqueio do Exame',
    fundamento: 'Desmagnetização do ímã interno e aquecimento por radiofrequência.'
  },
  {
    codigoSufixo: 'CORPO_ESTRANHO_OLHO',
    enunciado: 'Já trabalhou com solda/metal ou teve fragmento metálico ocular?',
    impeditivo: true,
    tipo: 'IMPEDITIVA',
    gatilho: 'Se SIM -> 🔴 Semáforo Vermelho + Exige RX de Órbitas',
    fundamento: 'Deslocamento de farpas ferromagnéticas intraoculares com perda visual.'
  },
  {
    codigoSufixo: 'CLAUSTROFOBIA',
    enunciado: 'Apresenta claustrofobia importante ao entrar no aparelho fechado?',
    impeditivo: false,
    tipo: 'ALERTA',
    gatilho: 'Se SIM -> 🟡 Semáforo Amarelo (Acolhimento / Sedação)',
    fundamento: 'Exige acolhimento, iluminação amena ou avaliação anestésica.'
  },
  {
    codigoSufixo: 'PROTESE_METALICA',
    enunciado: 'Possui próteses metálicas, pinos ou placas cirúrgicas na região?',
    impeditivo: false,
    tipo: 'INFORMATIVA',
    gatilho: 'Se SIM -> Sequências WARP/SEMAC para redução de artefatos',
    fundamento: 'Ajuste de banda passante contra distorções de suscetibilidade magnética.'
  }
];

const PERGUNTAS_PADRAO_TC = [
  {
    codigoSufixo: 'ALERGIA_IODO',
    enunciado: 'Histórico prévio de anafilaxia ou choque alérgico grave ao contraste iodado?',
    impeditivo: true,
    tipo: 'IMPEDITIVA',
    gatilho: 'Se SIM -> 🔴 Semáforo Vermelho + Bloqueio de Contraste',
    fundamento: 'Risco de reação anafilactoide severa sem protocolo prévio de dessensibilização.'
  },
  {
    codigoSufixo: 'INSUFICIENCIA_RENAL',
    enunciado: 'Diagnóstico prévio de doença renal crônica ou diálise (Creatinina alterada)?',
    impeditivo: true,
    tipo: 'IMPEDITIVA',
    gatilho: 'Se SIM -> 🔴 Semáforo Vermelho + Bloqueio até dosar Creatinina',
    fundamento: 'Prevenção de Nefropatia Induzida por Contraste (NIC) se eGFR < 30 mL/min.'
  },
  {
    codigoSufixo: 'METFORMINA',
    enunciado: 'Faz uso contínuo de Cloridrato de Metformina?',
    impeditivo: false,
    tipo: 'ALERTA',
    gatilho: 'Se SIM -> 🟡 Semáforo Amarelo + Suspensão 48h pós-exame',
    fundamento: 'Prevenção de acidose lática potencialmente fatal em caso de perda renal transitória.'
  },
  {
    codigoSufixo: 'JEJUM',
    enunciado: 'Cumpriu o jejum mínimo de 4 horas orientado?',
    impeditivo: false,
    tipo: 'ALERTA',
    gatilho: 'Se NÃO -> 🟡 Semáforo Amarelo + Risco de Êmese',
    fundamento: 'Contraste iodado hiperosmolar pode provocar náusea súbita e broncoaspiração.'
  },
  {
    codigoSufixo: 'GESTAO',
    enunciado: 'Possibilidade de gravidez ativa no momento (exposição à radiação)?',
    impeditivo: false,
    tipo: 'ALERTA',
    gatilho: 'Se SIM -> 🟡 Semáforo Amarelo + Princípio ALARA',
    fundamento: 'Radiação ionizante é potencialmente teratogênica, em especial no 1º trimestre.'
  }
];

function adminAbrirModalNovoProtocolo() {
  document.getElementById('modalNovoProtocoloAdmin')?.remove();

  const container = document.createElement('div');
  container.id = 'modalNovoProtocoloAdmin';
  container.style.position = 'fixed';
  container.style.inset = '0';
  container.style.background = 'rgba(10, 14, 23, 0.88)';
  container.style.backdropFilter = 'blur(12px)';
  container.style.display = 'flex';
  container.style.alignItems = 'center';
  container.style.justifyContent = 'center';
  container.style.zIndex = '9999';

  container.innerHTML = `
    <div style="background: var(--bg-card); border: 1px solid var(--cyan-500); border-radius: 16px; max-width: 820px; width: 95%; max-height: 92vh; display: flex; flex-direction: column; box-shadow: 0 25px 60px rgba(0,0,0,0.8), 0 0 25px var(--cyan-glow); overflow: hidden;">
      <!-- Cabeçalho -->
      <div style="padding: 18px 24px; border-bottom: 1px solid var(--bg-card-border); display: flex; justify-content: space-between; align-items: center; background: rgba(30, 41, 59, 0.5);">
        <div style="display: flex; align-items: center; gap: 12px;">
          <div style="width: 40px; height: 40px; border-radius: 8px; background: rgba(56, 189, 248, 0.2); color: var(--cyan-400); display: flex; align-items: center; justify-content: center; font-size: 1.25rem;">
            <i class="fa-solid fa-folder-plus"></i>
          </div>
          <div>
            <h3 style="font-family: var(--font-title); font-size: 1.2rem; color: #fff;">Novo Protocolo Anatômico & Regras Clínicas</h3>
            <p style="font-size: 0.82rem; color: var(--text-muted); margin-top: 2px;">Cadastre uma região inédita (ex: Crânio, Joelho, Face), defina seu checklist e vincule procedimentos TUSS.</p>
          </div>
        </div>
        <button type="button" class="btn btn--secondary" onclick="document.getElementById('modalNovoProtocoloAdmin').remove()" style="padding: 6px 10px;">
          <i class="fa-solid fa-xmark"></i>
        </button>
      </div>

      <!-- Formulário com Scroll -->
      <form id="formNovoProtocoloModal" style="padding: 22px 24px; overflow-y: auto; display: flex; flex-direction: column; gap: 20px;">
        
        <!-- 1. Identificação Geral -->
        <div style="background: rgba(15, 23, 42, 0.55); border: 1px solid var(--border-subtle); border-radius: 10px; padding: 16px; display: flex; flex-direction: column; gap: 14px;">
          <div style="font-size: 0.88rem; font-weight: 700; color: #fff; display: flex; align-items: center; gap: 8px;">
            <i class="fa-solid fa-tag" style="color: var(--cyan-400);"></i> 1. Identificação do Protocolo
          </div>

          <div style="display: grid; grid-template-columns: 2fr 1fr; gap: 14px;">
            <div class="form-field">
              <label class="form-label" for="novoProtoNome">Nome do Protocolo Anatômico:</label>
              <input type="text" id="novoProtoNome" class="form-input" placeholder="Ex: RM Crânio / Encéfalo (Neurológico)" required>
            </div>
            <div class="form-field">
              <label class="form-label" for="novoProtoCodigo">Código Identificador (ID):</label>
              <input type="text" id="novoProtoCodigo" class="form-input" placeholder="Ex: RM_CRANIO" style="font-family: var(--font-mono); text-transform: uppercase;" required>
            </div>
          </div>

          <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 14px;">
            <div class="form-field">
              <label class="form-label" for="novoProtoModalidade">Modalidade Base:</label>
              <select id="novoProtoModalidade" class="form-input" style="cursor: pointer;">
                <option value="RM">Ressonância Magnética (RM)</option>
                <option value="TC">Tomografia Computadorizada (TC)</option>
              </select>
            </div>
            <div class="form-field">
              <label class="form-label" for="novoProtoIcone">Ícone Visual:</label>
              <select id="novoProtoIcone" class="form-input" style="cursor: pointer;">
                <option value="fa-brain">🧠 Cérebro / Crânio (Neurologia)</option>
                <option value="fa-bone">🦴 Ossos / Articulações / Joelho</option>
                <option value="fa-hand-dots">✋ Membro Superior (Punho / Cotovelo / Ombro)</option>
                <option value="fa-person">🚶 Membro Inferior / Bacia / Perna</option>
                <option value="fa-lungs">🫁 Tórax / Pulmão / Mediastino</option>
                <option value="fa-circle-radiation">☢️ Abdômen / Pelve Total</option>
                <option value="fa-heart-pulse">❤️ Cardíaco / Vascular / Angio</option>
                <option value="fa-eye">👁️ Órbitas / Face / Mandíbula</option>
                <option value="fa-ear-listen">👂 Ouvido / Mastoides</option>
              </select>
            </div>
          </div>
        </div>

        <!-- 2. Checklist Base de Segurança da Modalidade -->
        <div style="background: rgba(15, 23, 42, 0.55); border: 1px solid var(--border-subtle); border-radius: 10px; padding: 16px;">
          <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 10px;">
            <div style="font-size: 0.88rem; font-weight: 700; color: #fff; display: flex; align-items: center; gap: 8px;">
              <i class="fa-solid fa-shield-halved" style="color: var(--emerald-400);"></i> 2. Regras de Segurança Universal (Modalidade)
            </div>
            <span style="font-size: 0.75rem; color: var(--text-muted);">Recomendado manter todas ativas para proteção do paciente</span>
          </div>
          <div id="containerPerguntasBaseModal" style="display: flex; flex-direction: column; gap: 8px;">
            <!-- Renderizado dinamicamente de acordo com a modalidade selecionada -->
          </div>
        </div>

        <!-- 3. Perguntas Específicas da Região Anatômica -->
        <div style="background: rgba(15, 23, 42, 0.55); border: 1px solid var(--border-subtle); border-radius: 10px; padding: 16px;">
          <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 12px; flex-wrap: wrap; gap: 10px;">
            <div>
              <div style="font-size: 0.88rem; font-weight: 700; color: #fff; display: flex; align-items: center; gap: 8px;">
                <i class="fa-solid fa-clipboard-question" style="color: var(--cyan-400);"></i> 3. Perguntas Especializadas da Região Anatômica
              </div>
              <p style="font-size: 0.78rem; color: var(--text-muted); margin-top: 2px;">Adicione critérios específicos desta anatomia (ex: válvula DVP no crânio, lesão de menisco no joelho).</p>
            </div>
            <button type="button" class="btn btn--secondary" id="btnAdicionarPerguntaEspecifica" style="font-size: 0.8rem; padding: 5px 12px; border-color: rgba(56, 189, 248, 0.4); color: var(--cyan-400);">
              <i class="fa-solid fa-plus" style="margin-right: 4px;"></i> Adicionar Pergunta
            </button>
          </div>

          <div id="containerPerguntasEspecificas" style="display: flex; flex-direction: column; gap: 12px;">
            <!-- Linhas de perguntas dinâmicas adicionadas aqui -->
          </div>
        </div>

        <!-- 4. Procedimento TUSS / RIS Integrado (* Obrigatório & Único) -->
        <div style="background: rgba(15, 23, 42, 0.55); border: 1px solid var(--border-subtle); border-radius: 10px; padding: 16px;">
          <div style="font-size: 0.88rem; font-weight: 700; color: #fff; display: flex; align-items: center; gap: 8px; margin-bottom: 4px;">
            <i class="fa-solid fa-list-check" style="color: var(--amber-400);"></i> 4. Procedimento TUSS / RIS Integrado (* Obrigatório &amp; Não Repetido)
          </div>
          <p style="font-size: 0.78rem; color: var(--text-muted); margin-bottom: 12px;">
            Todo protocolo requer obrigatoriamente um código TUSS (8 dígitos) ou RIS válido e não repetido no catálogo.
          </p>

          <div style="display: grid; grid-template-columns: 1fr 2fr 1fr; gap: 12px;">
            <div class="form-field">
              <label class="form-label" for="novoProtoTussCodigo">Código TUSS / RIS (* Obrigatório):</label>
              <input type="text" id="novoProtoTussCodigo" class="form-input" placeholder="Ex: 41101014 ou RMCRAN" required>
            </div>
            <div class="form-field">
              <label class="form-label" for="novoProtoTussNome">Nome do Procedimento Médico (* Obrigatório):</label>
              <input type="text" id="novoProtoTussNome" class="form-input" placeholder="Ex: RM de Crânio / Encéfalo com Contraste" required>
            </div>
            <div class="form-field">
              <label class="form-label" for="novoProtoTussRegiao">Região Anatômica (* Obrigatório):</label>
              <input type="text" id="novoProtoTussRegiao" class="form-input" placeholder="Ex: Crânio" required>
            </div>
          </div>
        </div>

        <!-- Rodapé e Ações -->
        <div style="display: flex; justify-content: flex-end; gap: 10px; padding-top: 10px; border-top: 1px solid rgba(255,255,255,0.08);">
          <button type="button" class="btn btn--secondary" onclick="document.getElementById('modalNovoProtocoloAdmin').remove()">
            Cancelar
          </button>
          <button type="submit" class="btn btn--primary" id="btnSalvarNovoProtocolo" style="padding: 8px 20px;">
            <i class="fa-solid fa-check"></i> Salvar & Ativar Protocolo
          </button>
        </div>
      </form>
    </div>
  `;

  document.body.appendChild(container);

  // Renderiza perguntas base iniciais
  const renderizarPerguntasBase = (modalidade) => {
    const containerBase = document.getElementById('containerPerguntasBaseModal');
    if (!containerBase) return;
    const lista = modalidade === 'TC' ? PERGUNTAS_PADRAO_TC : PERGUNTAS_PADRAO_RM;

    containerBase.innerHTML = lista.map((p, idx) => {
      const badgeClass = p.tipo === 'IMPEDITIVA' ? 'status-bloqueado' : (p.tipo === 'ALERTA' ? 'status-aguardando' : 'status-liberado');
      const badgeTexto = p.tipo === 'IMPEDITIVA' ? '🔴 Impeditiva' : (p.tipo === 'ALERTA' ? '🟡 Alerta' : '🟢 Informativa');

      return `
        <label style="display: flex; align-items: flex-start; gap: 10px; background: rgba(255,255,255,0.02); border: 1px solid var(--border-subtle); border-radius: 6px; padding: 8px 12px; cursor: pointer;">
          <input type="checkbox" class="chk-pergunta-base" data-idx="${idx}" checked style="margin-top: 3px; accent-color: var(--cyan-400); cursor: pointer;">
          <div style="flex: 1;">
            <div style="display: flex; justify-content: space-between; align-items: center; gap: 8px;">
              <span style="font-size: 0.84rem; font-weight: 500; color: #fff;">${p.enunciado}</span>
              <span class="badge-status ${badgeClass}" style="font-size: 0.68rem;">${badgeTexto}</span>
            </div>
            <div style="font-size: 0.74rem; color: var(--text-muted); margin-top: 2px;">
              <strong>Fundamento:</strong> ${p.fundamento}
            </div>
          </div>
        </label>
      `;
    }).join('');
  };

  renderizarPerguntasBase('RM');

  // Ao alterar a modalidade, atualiza perguntas base e sugestão de código
  const selectMod = document.getElementById('novoProtoModalidade');
  const inputNome = document.getElementById('novoProtoNome');
  const inputCodigo = document.getElementById('novoProtoCodigo');
  const inputTussNome = document.getElementById('novoProtoTussNome');
  const inputTussRegiao = document.getElementById('novoProtoTussRegiao');

  const atualizarSugestaoCodigo = () => {
    const mod = selectMod.value;
    const nome = inputNome.value.trim();
    if (!nome) return;

    // Remove acentos e caracteres especiais
    const limpo = nome
      .normalize('NFD').replace(/[\u0300-\u036f]/g, '')
      .toUpperCase()
      .replace(/^(RM|TC)\s*/, '')
      .replace(/[^A-Z0-9]+/g, '_')
      .replace(/^_+|_+$/g, '');

    inputCodigo.value = `${mod}_${limpo}`.slice(0, 25);
    if (!inputTussNome.value) inputTussNome.value = nome;
    if (!inputTussRegiao.value) {
      const palavras = limpo.split('_');
      inputTussRegiao.value = palavras[0] ? palavras[0].charAt(0).toUpperCase() + palavras[0].slice(1).toLowerCase() : '';
    }
  };

  inputNome.addEventListener('input', atualizarSugestaoCodigo);
  selectMod.addEventListener('change', () => {
    renderizarPerguntasBase(selectMod.value);
    atualizarSugestaoCodigo();
  });

  // Contador de perguntas dinâmicas
  let contadorPerguntasCustom = 0;
  const containerPerguntasEspecificas = document.getElementById('containerPerguntasEspecificas');

  const adicionarLinhaPerguntaAnatomica = (enunciadoPadrao = '', tipoPadrao = 'IMPEDITIVA', gatilhoPadrao = '', fundamentoPadrao = '') => {
    contadorPerguntasCustom++;
    const rowId = `pergunta_custom_${contadorPerguntasCustom}`;
    const card = document.createElement('div');
    card.id = rowId;
    card.style.background = 'rgba(15, 23, 42, 0.8)';
    card.style.border = '1px solid rgba(56, 189, 248, 0.3)';
    card.style.borderRadius = '8px';
    card.style.padding = '12px 14px';
    card.style.display = 'flex';
    card.style.flexDirection = 'column';
    card.style.gap = '10px';

    card.innerHTML = `
      <div style="display: flex; justify-content: space-between; align-items: center;">
        <span style="font-size: 0.78rem; font-family: var(--font-mono); color: var(--cyan-400); font-weight: 700;">
          <i class="fa-solid fa-circle-question"></i> Pergunta Anatômica #${contadorPerguntasCustom}
        </span>
        <button type="button" title="Remover Pergunta" style="background: none; border: none; color: #f87171; cursor: pointer; padding: 2px 6px; font-size: 0.85rem;" onclick="document.getElementById('${rowId}')?.remove()">
          <i class="fa-solid fa-trash-can"></i> Remover
        </button>
      </div>

      <div style="display: grid; grid-template-columns: 3fr 1.5fr; gap: 10px;">
        <div class="form-field">
          <label class="form-label" style="font-size: 0.75rem;">Enunciado da Pergunta Clínica:</label>
          <input type="text" class="form-input custom-p-enunciado" value="${enunciadoPadrao}" placeholder="Ex: Possui derivação ventrículo-peritoneal (DVP) com regulagem magnética?" required>
        </div>
        <div class="form-field">
          <label class="form-label" style="font-size: 0.75rem;">Classificação do Semáforo:</label>
          <select class="form-input custom-p-tipo" style="cursor: pointer;">
            <option value="IMPEDITIVA" ${tipoPadrao === 'IMPEDITIVA' ? 'selected' : ''}>🔴 Impeditiva (Bloqueio Automático)</option>
            <option value="ALERTA" ${tipoPadrao === 'ALERTA' ? 'selected' : ''}>🟡 Alerta Operacional (Amarelo)</option>
            <option value="INFORMATIVA" ${tipoPadrao === 'INFORMATIVA' ? 'selected' : ''}>🟢 Critério Informativo Clínico</option>
          </select>
        </div>
      </div>

      <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 10px;">
        <div class="form-field">
          <label class="form-label" style="font-size: 0.75rem;">Gatilho no Semáforo:</label>
          <input type="text" class="form-input custom-p-gatilho" value="${gatilhoPadrao}" placeholder="Ex: Se SIM -> Bloqueio de Segurança">
        </div>
        <div class="form-field">
          <label class="form-label" style="font-size: 0.75rem;">Fundamento Radiológico / Justificativa:</label>
          <input type="text" class="form-input custom-p-fundamento" value="${fundamentoPadrao}" placeholder="Ex: Válvulas programáveis podem descalibrar sob campo de 1.5T/3.0T.">
        </div>
      </div>
    `;

    containerPerguntasEspecificas.appendChild(card);
  };

  document.getElementById('btnAdicionarPerguntaEspecifica').addEventListener('click', () => {
    adicionarLinhaPerguntaAnatomica();
  });

  // Submissão do Formulário
  document.getElementById('formNovoProtocoloModal').addEventListener('submit', (e) => {
    e.preventDefault();
    const btnSalvar = document.getElementById('btnSalvarNovoProtocolo');
    btnSalvar.disabled = true;
    btnSalvar.innerHTML = '<i class="fa-solid fa-spinner fa-spin"></i> Ativando Protocolo...';

    const nome = inputNome.value.trim();
    let codigo = inputCodigo.value.trim().toUpperCase().replace(/[^A-Z0-9_]/g, '_');
    const modalidade = selectMod.value;
    const icone = document.getElementById('novoProtoIcone').value || 'fa-brain';

    if (!codigo) {
      codigo = `${modalidade}_${Date.now()}`;
    }

    if (PROTOCOLOS_CATALOGO_INFO[codigo]) {
      showToast(`O código de protocolo "${codigo}" já existe. Escolha outro código identificador.`, 'error');
      btnSalvar.disabled = false;
      btnSalvar.innerHTML = '<i class="fa-solid fa-check"></i> Salvar & Ativar Protocolo';
      return;
    }

    // Coleta perguntas base marcadas
    const listaBase = modalidade === 'TC' ? PERGUNTAS_PADRAO_TC : PERGUNTAS_PADRAO_RM;
    const perguntasFinais = [];
    const metadataFinais = {};

    container.querySelectorAll('.chk-pergunta-base:checked').forEach(chk => {
      const idx = parseInt(chk.getAttribute('data-idx'));
      const p = listaBase[idx];
      if (p) {
        const codigoPerg = `${codigo}_${p.codigoSufixo}`;
        perguntasFinais.push({
          codigo: codigoPerg,
          enunciado: p.enunciado,
          impeditivo: p.impeditivo
        });
        metadataFinais[codigoPerg] = {
          tipo: p.tipo,
          gatilho: p.gatilho,
          fundamento: p.fundamento
        };
      }
    });

    // Coleta perguntas específicas adicionadas
    container.querySelectorAll('#containerPerguntasEspecificas > div').forEach((card, idx) => {
      const enunciado = card.querySelector('.custom-p-enunciado')?.value.trim();
      const tipo = card.querySelector('.custom-p-tipo')?.value || 'INFORMATIVA';
      let gatilho = card.querySelector('.custom-p-gatilho')?.value.trim();
      let fundamento = card.querySelector('.custom-p-fundamento')?.value.trim();

      if (enunciado) {
        const codigoPerg = `${codigo}_ANAT_${idx + 1}`;
        const impeditivo = tipo === 'IMPEDITIVA';

        if (!gatilho) {
          gatilho = impeditivo ? 'Se SIM -> 🔴 Semáforo Vermelho + Bloqueio do Exame' : (tipo === 'ALERTA' ? 'Se SIM -> 🟡 Semáforo Amarelo' : 'Critério informativo clínico');
        }
        if (!fundamento) {
          fundamento = 'Critério específico da região anatômica para segurança e direcionamento técnico.';
        }

        perguntasFinais.push({
          codigo: codigoPerg,
          enunciado: enunciado,
          impeditivo: impeditivo
        });
        metadataFinais[codigoPerg] = {
          tipo: tipo,
          gatilho: gatilho,
          fundamento: fundamento
        };
      }
    });

    if (perguntasFinais.length === 0) {
      showToast('Selecione pelo menos uma pergunta de segurança ou adicione uma pergunta personalizada.', 'error');
      btnSalvar.disabled = false;
      btnSalvar.innerHTML = '<i class="fa-solid fa-check"></i> Salvar & Ativar Protocolo';
      return;
    }

    // Coleta e valida procedimento inicial obrigatório
    const tussCodigo = document.getElementById('novoProtoTussCodigo')?.value.trim();
    const tussNome = document.getElementById('novoProtoTussNome')?.value.trim();
    const tussRegiao = document.getElementById('novoProtoTussRegiao')?.value.trim();

    if (!tussNome) {
      showToast('O nome do procedimento médico integrado é obrigatório.', 'error');
      btnSalvar.disabled = false;
      btnSalvar.innerHTML = '<i class="fa-solid fa-check"></i> Salvar & Ativar Protocolo';
      document.getElementById('novoProtoTussNome')?.focus();
      return;
    }

    if (!tussRegiao) {
      showToast('A região anatômica do procedimento é obrigatória.', 'error');
      btnSalvar.disabled = false;
      btnSalvar.innerHTML = '<i class="fa-solid fa-check"></i> Salvar & Ativar Protocolo';
      document.getElementById('novoProtoTussRegiao')?.focus();
      return;
    }

    const validacaoTuss = validarCodigoTussERis(tussCodigo);
    if (!validacaoTuss.valido) {
      showToast(validacaoTuss.mensagem, 'error');
      btnSalvar.disabled = false;
      btnSalvar.innerHTML = '<i class="fa-solid fa-check"></i> Salvar & Ativar Protocolo';
      document.getElementById('novoProtoTussCodigo')?.focus();
      return;
    }

    const procedimentosIniciais = [
      {
        codigoTuss: validacaoTuss.codigoFormatado,
        nome: tussNome,
        regiao: tussRegiao
      }
    ];

    // Registra nas estruturas do sistema
    PROTOCOLOS_CATALOGO_INFO[codigo] = {
      id: codigo,
      nome: nome,
      modalidade: modalidade,
      icone: icone,
      corIcone: modalidade === 'TC' ? 'var(--cyan-400)' : 'var(--purple-400)',
      isCustom: true
    };

    PROTOCOLOS_CHECKLIST[codigo] = perguntasFinais;
    Object.assign(METADATA_REGRAS_CLINICAS, metadataFinais);
    CATALOGO_PROCEDIMENTOS_FORMULARIO[codigo] = procedimentosIniciais;

    // Salva no LocalStorage para persistência
    salvarProtocolosCustomizadosStorage();

    // Atualiza o seletor visual e renderiza o novo protocolo
    atualizarOpcoesSeletorProtocolos(codigo);
    renderizarDicionarioProtocolos(codigo);

    container.remove();
    showToast(`Protocolo anatômico "${nome}" criado e ativado com sucesso!`, 'success');
  });
}

function adminExcluirProtocoloCustomizado(protocoloId) {
  const hiddenSelect = document.getElementById('selectProtocoloAdmin');
  const idParaExcluir = protocoloId || hiddenSelect?.value;
  if (!idParaExcluir) return;

  const info = PROTOCOLOS_CATALOGO_INFO[idParaExcluir];
  if (!info || !info.isCustom) {
    showToast('Apenas protocolos personalizados adicionados pela equipe podem ser excluídos.', 'info');
    return;
  }

  // Verifica se há exames na grade usando este protocolo
  const examesUsando = UIState.exames.filter(e => (e.tipoFormulario || '') === idParaExcluir);
  if (examesUsando.length > 0) {
    alert(`Não é possível excluir o protocolo "${info.nome}" pois existem ${examesUsando.length} exame(s) agendado(s) na grade utilizando-o.\n\nEdite ou exclua os exames agendados antes de remover o protocolo.`);
    return;
  }

  if (!confirm(`Tem certeza que deseja EXCLUIR permanentemente o protocolo "${info.nome}"?\n\nAs perguntas e regras clínicas deste protocolo serão removidas do sistema.`)) {
    return;
  }

  delete PROTOCOLOS_CATALOGO_INFO[idParaExcluir];
  delete PROTOCOLOS_CHECKLIST[idParaExcluir];
  delete CATALOGO_PROCEDIMENTOS_FORMULARIO[idParaExcluir];

  salvarProtocolosCustomizadosStorage();
  atualizarOpcoesSeletorProtocolos('RM_COLUNA');
  renderizarDicionarioProtocolos('RM_COLUNA');
  showToast(`Protocolo "${info.nome}" excluído com sucesso!`, 'success');
}
