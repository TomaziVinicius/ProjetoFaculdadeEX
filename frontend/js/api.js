/**
 * SISTEMA DE TRIAGEM PARA DIAGNÓSTICO POR IMAGEM
 * Cliente HTTP / Camada de Integração RESTful com o Backend .NET
 * Mapeado 1-to-1 com os DTOs da camada TriagemImagem.Application
 */

const ApiClient = {
  baseUrl: localStorage.getItem('api_base_url') || 'http://localhost:5000/api/v1',

  /**
   * Executa requisições HTTP padronizadas com tratamento de erros.
   */
  async request(endpoint, options = {}) {
    const url = `${this.baseUrl}${endpoint}`;
    const defaultHeaders = {
      'Content-Type': 'application/json',
      'Accept': 'application/json'
    };

    try {
      const response = await fetch(url, {
        ...options,
        headers: {
          ...defaultHeaders,
          ...options.headers
        }
      });

      if (!response.ok) {
        const errorData = await response.json().catch(() => ({}));
        throw new Error(errorData.message || `Erro na comunicação com a API (HTTP ${response.status})`);
      }

      return await response.json();
    } catch (err) {
      console.warn(`[ApiClient] Requisição para '${url}' não concluída:`, err.message);
      throw err;
    }
  },

  // 1. Autenticação (LoginRequestDto -> UsuarioAutenticadoDto)
  async login(login, senha) {
    return await this.request('/auth/login', {
      method: 'POST',
      body: JSON.stringify({ login, senha })
    });
  },

  // 2. Worklist (FiltroWorklistDto -> List<ExameWorklistDto>)
  async obterWorklist(filtros = {}) {
    const params = new URLSearchParams();
    if (filtros.data) params.append('data', filtros.data);
    if (filtros.modalidade) params.append('modalidade', filtros.modalidade);
    if (filtros.status) params.append('status', filtros.status);
    if (filtros.termoBusca) params.append('termoBusca', filtros.termoBusca);

    const query = params.toString() ? `?${params.toString()}` : '';
    return await this.request(`/worklist${query}`, { method: 'GET' });
  },

  // 3. Detalhes de um Exame por ID
  async obterExamePorId(exameId) {
    return await this.request(`/worklist/${exameId}`, { method: 'GET' });
  },

  // 4. Chamada de Paciente na Sala
  async chamarPaciente(exameId, usuarioId) {
    return await this.request(`/worklist/${exameId}/chamar`, {
      method: 'POST',
      body: JSON.stringify({ usuarioId })
    });
  },

  // 5. Cálculo Clínico de Meio de Contraste (CalcularDoseRequestDto -> ResultadoDoseDto)
  async calcularDose(modalidade, pesoKg, fatorMlPorKg = null) {
    return await this.request('/triagem/calcular-dose', {
      method: 'POST',
      body: JSON.stringify({
        modalidade: modalidade === 'TC' ? 1 : 2, // 1 = TC, 2 = RM (Enum ModalidadeExame)
        pesoKg: parseFloat(pesoKg),
        fatorMlPorKg: fatorMlPorKg ? parseFloat(fatorMlPorKg) : null
      })
    });
  },

  // 6. Salvar Ficha de Triagem e Checklist (SalvarTriagemRequestDto -> DetalhesTriagemDto)
  async salvarTriagem(dadosTriagem) {
    return await this.request('/triagem', {
      method: 'POST',
      body: JSON.stringify(dadosTriagem)
    });
  },

  // 7. Obter Formulário de Triagem existente
  async obterTriagemPorExameId(exameId) {
    return await this.request(`/triagem/exame/${exameId}`, { method: 'GET' });
  },

  // 8. Liberar Exame para Execução
  async liberarExame(exameId, usuarioId, observacao = null) {
    return await this.request(`/triagem/${exameId}/liberar`, {
      method: 'POST',
      body: JSON.stringify({ usuarioId, observacao })
    });
  },

  // 9. Cancelar Exame por Contraindicação Clínica
  async cancelarExame(exameId, usuarioId, motivo) {
    return await this.request(`/triagem/${exameId}/cancelar`, {
      method: 'POST',
      body: JSON.stringify({ usuarioId, motivo })
    });
  },

  // 10. Trilha de Auditoria Clínica (-> List<AuditoriaLogDto>)
  async obterAuditoria(limite = 50) {
    return await this.request(`/auditoria?limite=${limite}`, { method: 'GET' });
  },

  // 11. Status de Conexão com os Dois Bancos de Dados (Banco 1: Triagem, Banco 2: Laboratório)
  async obterStatusBancos() {
    return await this.request('/database/status', { method: 'GET' });
  },

  // 12. Consulta de Agendamentos diretamente no Banco de Dados 2 (Laboratório / HIS-RIS)
  async obterWorklistLaboratorio() {
    return await this.request('/worklist/laboratorio', { method: 'GET' });
  },

  // 13. Módulos Administrativos e Gestão de Operadores
  async listarUsuariosAdmin() {
    return await this.request('/admin/usuarios', { method: 'GET' });
  },

  async criarUsuarioAdmin(dados) {
    return await this.request('/admin/usuarios', {
      method: 'POST',
      body: JSON.stringify(dados)
    });
  },

  async toggleStatusUsuarioAdmin(usuarioId) {
    return await this.request(`/admin/usuarios/${usuarioId}/toggle-status`, {
      method: 'PUT'
    });
  },

  async resetSenhaUsuarioAdmin(usuarioId) {
    return await this.request(`/admin/usuarios/${usuarioId}/reset-senha`, {
      method: 'PUT'
    });
  },

  async resetStatusExameAdmin(exameId) {
    return await this.request(`/admin/exames/${exameId}/reset-status`, {
      method: 'POST'
    });
  },

  async alterarSalaExameAdmin(exameId, novaSala) {
    return await this.request(`/admin/exames/${exameId}/alterar-sala`, {
      method: 'POST',
      body: JSON.stringify({ novaSala })
    });
  },

  async sincronizarExamesHojeAdmin() {
    return await this.request('/admin/exames/sincronizar-hoje', {
      method: 'POST'
    });
  },

  async criarExameAdmin(dados) {
    return await this.request('/admin/exames', {
      method: 'POST',
      body: JSON.stringify(dados)
    });
  },

  async editarExameAdmin(exameId, dados) {
    return await this.request(`/admin/exames/${exameId}`, {
      method: 'PUT',
      body: JSON.stringify(dados)
    });
  },

  async excluirExameAdmin(exameId) {
    return await this.request(`/admin/exames/${exameId}`, {
      method: 'DELETE'
    });
  }
};

