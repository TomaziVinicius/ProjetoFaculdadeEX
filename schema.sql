-- ==============================================================================
-- SISTEMA DE TRIAGEM PARA DIAGNÓSTICO POR IMAGEM
-- MODELAGEM RELACIONAL DO BANCO DE DADOS
-- ==============================================================================

-- 1. TABELA DE USUÁRIOS (PROFISSIONAIS QUE OPERAM E ASSINAM)
CREATE TABLE IF NOT EXISTS usuarios (
    id INT AUTO_INCREMENT PRIMARY KEY,
    nome VARCHAR(100) NOT NULL,
    login VARCHAR(50) NOT NULL UNIQUE,
    senha_usuario VARCHAR(255) NOT NULL,
    perfil ENUM('ESTAGIARIO', 'TECNICO', 'BIOMEDICO', 'ADMIN') NOT NULL,
    status_usuario BOOLEAN NOT NULL DEFAULT TRUE,
    criado_em DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 2. TABELA DE PACIENTES (DADOS CADASTRAIS DEMOGRÁFICOS)
CREATE TABLE IF NOT EXISTS pacientes (
    id INT AUTO_INCREMENT PRIMARY KEY,
    documento_cpf VARCHAR(11) NOT NULL UNIQUE,
    nome_completo VARCHAR(100) NOT NULL,
    data_nascimento DATE NOT NULL,
    sexo ENUM('M', 'F', 'O') NOT NULL,
    peso_kg DECIMAL(5,2) NULL,
    altura_cm INT NULL,
    criado_em DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 3. TABELA DE EXAMES (AGENDAMENTOS, ATENDIMENTO E PROCEDIMENTOS)
CREATE TABLE IF NOT EXISTS exames (
    id INT AUTO_INCREMENT PRIMARY KEY,
    paciente_id INT NOT NULL,
    atendimento INT NOT NULL,
    codigo_procedimento VARCHAR(30) NOT NULL,
    sequencia_interna INT NOT NULL,
    descricao_exame VARCHAR(150) NOT NULL,
    modalidade ENUM('TC', 'RM') NOT NULL,
    sala VARCHAR(20) NOT NULL,
    horario_agendado DATETIME NOT NULL,
    horario_chamada DATETIME NULL,
    horario_execucao DATETIME NULL,
    status ENUM('AGUARDANDO_TRIAGEM', 'EM_TRIAGEM', 'LIBERADO', 'BLOQUEADO_SEGURANCA', 'CONCLUIDO') NOT NULL DEFAULT 'AGUARDANDO_TRIAGEM',
    criado_em DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_exames_paciente FOREIGN KEY (paciente_id) REFERENCES pacientes(id) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 4. TABELA DE FORMULÁRIOS DE TRIAGEM (CABEÇALHO E MÉTRICAS DA SESSÃO)
CREATE TABLE IF NOT EXISTS formularios_triagem (
    id INT AUTO_INCREMENT PRIMARY KEY,
    exame_id INT NOT NULL UNIQUE,
    usuario_responsavel_id INT NOT NULL,
    volume_contraste_ml DECIMAL(5,2) NULL,
    lote_contraste VARCHAR(50) NULL,
    caminho_arquivo_armazenado VARCHAR(255) NULL,
    status_risco ENUM('VERDE', 'AMARELO', 'VERMELHO') NOT NULL,
    observacoes_clinicas TEXT NULL,
    iniciado_em DATETIME NOT NULL,
    finalizado_em DATETIME NULL,
    CONSTRAINT fk_triagem_exame FOREIGN KEY (exame_id) REFERENCES exames(id) ON DELETE RESTRICT,
    CONSTRAINT fk_triagem_usuario FOREIGN KEY (usuario_responsavel_id) REFERENCES usuarios(id) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 5. TABELA DE ITENS DE RESPOSTA DA TRIAGEM (QUESTÕES DE SEGURANÇA / CHECKLIST DE AVIAÇÃO)
CREATE TABLE IF NOT EXISTS itens_resposta_triagem (
    id INT AUTO_INCREMENT PRIMARY KEY,
    formulario_id INT NOT NULL,
    codigo_pergunta VARCHAR(50) NOT NULL,
    pergunta_enunciado VARCHAR(255) NOT NULL,
    resposta_valor VARCHAR(100) NOT NULL,
    impeditivo BOOLEAN NOT NULL DEFAULT FALSE,
    justificativa VARCHAR(255) NULL,
    CONSTRAINT fk_itens_formulario FOREIGN KEY (formulario_id) REFERENCES formularios_triagem(id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 6. TABELA DE AUDITORIA OPERACIONAL (HISTÓRICO INVIOLÁVEL DE RASTREABILIDADE)
CREATE TABLE IF NOT EXISTS auditoria_logs (
    id INT AUTO_INCREMENT PRIMARY KEY,
    usuario_id INT NOT NULL,
    exame_id INT NULL,
    tipo_evento VARCHAR(50) NOT NULL,
    detalhes TEXT NOT NULL,
    ip_origem VARCHAR(45) NULL,
    criado_em DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_auditoria_usuario FOREIGN KEY (usuario_id) REFERENCES usuarios(id) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ÍNDICES PARA CONSULTAS E WORKLIST
CREATE INDEX idx_exames_horario ON exames(horario_agendado);
CREATE INDEX idx_exames_status ON exames(status);
CREATE INDEX idx_exames_modalidade ON exames(modalidade);
CREATE INDEX idx_pacientes_cpf ON pacientes(documento_cpf);
CREATE INDEX idx_pacientes_nome ON pacientes(nome_completo);
CREATE INDEX idx_auditoria_data ON auditoria_logs(criado_em);

-- DADOS INICIAIS (SEED) PARA DEMONSTRAÇÃO E TESTES
INSERT INTO usuarios (id, nome, login, senha_usuario, perfil, status_usuario, criado_em)
VALUES 
(1, 'Administrador do Sistema', 'admin', '$2a$11$e8pA8l4r0yA9j1g6r4m5t6u7v8w9x0y1z2a3b4c5d6e7f8g9h0i1j', 'ADMIN', TRUE, NOW()),
(2, 'Lucas Silva (Biomédico)', 'lucas.biomed', '$2a$11$e8pA8l4r0yA9j1g6r4m5t6u7v8w9x0y1z2a3b4c5d6e7f8g9h0i1j', 'BIOMEDICO', TRUE, NOW()),
(3, 'Mariana Santos (Técnica)', 'mariana.tec', '$2a$11$e8pA8l4r0yA9j1g6r4m5t6u7v8w9x0y1z2a3b4c5d6e7f8g9h0i1j', 'TECNICO', TRUE, NOW())
ON DUPLICATE KEY UPDATE id=id;

INSERT INTO pacientes (id, documento_cpf, nome_completo, data_nascimento, sexo, peso_kg, altura_cm)
VALUES
(1, '12345678901', 'João Carlos de Oliveira', '1975-04-12', 'M', 78.50, 175),
(2, '98765432100', 'Maria Aparecida Souza', '1962-09-28', 'F', 64.00, 160),
(3, '45678912344', 'Carlos Eduardo Mendes', '1988-11-05', 'M', 82.00, 180),
(4, '78912345688', 'Fernanda Lima Rocha', '1995-02-19', 'F', 58.00, 165)
ON DUPLICATE KEY UPDATE id=id;

INSERT INTO exames (id, paciente_id, atendimento, codigo_procedimento, sequencia_interna, descricao_exame, modalidade, sala, horario_agendado, status)
VALUES
(1, 1, 104520, '41001010', 1, 'Tomografia Computadorizada de Tórax com Contraste', 'TC', 'TC-01', '2026-10-25 08:30:00', 'AGUARDANDO_TRIAGEM'),
(2, 2, 104521, '41101014', 1, 'Ressonância Magnética de Crânio com Contraste', 'RM', 'RM-01', '2026-10-25 09:00:00', 'AGUARDANDO_TRIAGEM'),
(3, 3, 104522, '41001028', 1, 'Tomografia Computadorizada de Abdômen Total com Contraste', 'TC', 'TC-02', '2026-10-25 09:40:00', 'AGUARDANDO_TRIAGEM'),
(4, 4, 104523, '41101022', 1, 'Ressonância Magnética de Coluna Lombar', 'RM', 'RM-02', '2026-10-25 10:15:00', 'AGUARDANDO_TRIAGEM')
ON DUPLICATE KEY UPDATE id=id;
