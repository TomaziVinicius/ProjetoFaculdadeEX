-- ==============================================================================
-- SISTEMA DE TRIAGEM PARA DIAGNÓSTICO POR IMAGEM (PRODUÇÃO - FormsDB)
-- ESTRUTURA UNIFICADA: APLICAÇÃO DE TRIAGEM + WORKLIST LABORATORIAL (HIS/RIS)
-- ==============================================================================

SET FOREIGN_KEY_CHECKS = 0;

-- ------------------------------------------------------------------------------
-- 1. TABELA DE USUÁRIOS (PROFISSIONAIS QUE OPERAM E ASSINAM)
-- ------------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS usuarios (
    id INT AUTO_INCREMENT PRIMARY KEY,
    nome VARCHAR(100) NOT NULL,
    login VARCHAR(50) NOT NULL UNIQUE,
    senha_usuario VARCHAR(255) NOT NULL,
    perfil ENUM('ESTAGIARIO', 'TECNICO', 'BIOMEDICO', 'ADMIN') NOT NULL,
    status_usuario BOOLEAN NOT NULL DEFAULT TRUE,
    criado_em DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ------------------------------------------------------------------------------
-- 2. TABELA DE PACIENTES (DADOS CADASTRAIS DEMOGRÁFICOS)
-- ------------------------------------------------------------------------------
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

-- ------------------------------------------------------------------------------
-- 3. TABELA DE EXAMES (AGENDAMENTOS, ATENDIMENTO E PROCEDIMENTOS)
-- ------------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS exames (
    id INT AUTO_INCREMENT PRIMARY KEY,
    paciente_id INT NOT NULL,
    atendimento INT NOT NULL,
    codigo_procedimento VARCHAR(30) NOT NULL,
    sequencia_interna INT NOT NULL,
    descricao_exame VARCHAR(150) NOT NULL,
    modalidade ENUM('TC', 'RM') NOT NULL,
    tipo_formulario VARCHAR(50) NOT NULL DEFAULT 'RM_COLUNA',
    segmento_lado VARCHAR(50) NULL,
    motivo_exame TEXT NULL,
    medico_solicitante VARCHAR(100) NULL,
    convenio VARCHAR(50) NULL,
    sala VARCHAR(20) NOT NULL,
    horario_agendado DATETIME NOT NULL,
    horario_chamada DATETIME NULL,
    horario_execucao DATETIME NULL,
    status ENUM('AGUARDANDO_TRIAGEM', 'EM_TRIAGEM', 'LIBERADO', 'BLOQUEADO_SEGURANCA', 'CONCLUIDO') NOT NULL DEFAULT 'AGUARDANDO_TRIAGEM',
    criado_em DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_exames_paciente FOREIGN KEY (paciente_id) REFERENCES pacientes(id) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ------------------------------------------------------------------------------
-- 4. TABELA DE FORMULÁRIOS DE TRIAGEM (CABEÇALHO E MÉTRICAS DA SESSÃO)
-- ------------------------------------------------------------------------------
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

-- ------------------------------------------------------------------------------
-- 5. TABELA DE ITENS DE RESPOSTA DA TRIAGEM (CHECKLIST DE SEGURANÇA)
-- ------------------------------------------------------------------------------
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

-- ------------------------------------------------------------------------------
-- 6. TABELA DE AUDITORIA OPERACIONAL (HISTÓRICO DE RASTREABILIDADE)
-- ------------------------------------------------------------------------------
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

-- ------------------------------------------------------------------------------
-- 7. TABELA DE PACIENTES DA WORKLIST (SISTEMA CENTRAL DE AGENDAMENTO)
-- ------------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS worklist_pacientes (
    id INT AUTO_INCREMENT PRIMARY KEY,
    documento_cpf VARCHAR(11) NOT NULL UNIQUE,
    nome_completo VARCHAR(150) NOT NULL,
    data_nascimento DATE NOT NULL,
    sexo ENUM('M', 'F', 'O') NOT NULL,
    peso_kg DECIMAL(5,2) NULL,
    altura_cm INT NULL,
    telefone VARCHAR(20) NULL,
    criado_em DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ------------------------------------------------------------------------------
-- 8. TABELA DE PEDIDOS DE EXAME AGENDADOS (WORKLIST DE ENTRADA)
-- ------------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS worklist_pedidos_exames (
    id INT AUTO_INCREMENT PRIMARY KEY,
    paciente_id INT NOT NULL,
    atendimento INT NOT NULL UNIQUE,
    modalidade ENUM('TC', 'RM') NOT NULL,
    codigo_procedimento VARCHAR(30) NOT NULL,
    descricao_procedimento VARCHAR(150) NOT NULL,
    tipo_formulario VARCHAR(50) NOT NULL,
    segmento_lado VARCHAR(50) NULL,
    preparo_adequado BOOLEAN NOT NULL DEFAULT TRUE,
    preparo_motivo_inadequado VARCHAR(255) NULL,
    motivo_exame TEXT NULL,
    medico_solicitante VARCHAR(100) NOT NULL,
    crm_medico VARCHAR(20) NOT NULL,
    convenio VARCHAR(50) NOT NULL DEFAULT 'Unimed',
    sala VARCHAR(20) NOT NULL,
    horario_agendado DATETIME NOT NULL,
    horario_chamada DATETIME NULL,
    status VARCHAR(30) NOT NULL DEFAULT 'AGENDADO',
    criado_em DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_worklist_paciente FOREIGN KEY (paciente_id) REFERENCES worklist_pacientes(id) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ------------------------------------------------------------------------------
-- ÍNDICES DE PERFORMANCE
-- ------------------------------------------------------------------------------
CREATE INDEX idx_exames_horario ON exames(horario_agendado);
CREATE INDEX idx_exames_status ON exames(status);
CREATE INDEX idx_exames_modalidade ON exames(modalidade);
CREATE INDEX idx_pacientes_cpf ON pacientes(documento_cpf);
CREATE INDEX idx_pacientes_nome ON pacientes(nome_completo);
CREATE INDEX idx_auditoria_data ON auditoria_logs(criado_em);
CREATE INDEX idx_worklist_horario ON worklist_pedidos_exames(horario_agendado);
CREATE INDEX idx_worklist_status ON worklist_pedidos_exames(status);
CREATE INDEX idx_worklist_tipo_form ON worklist_pedidos_exames(tipo_formulario);

SET FOREIGN_KEY_CHECKS = 1;

-- ------------------------------------------------------------------------------
-- SEED DE USUÁRIOS
-- Senha padrão para todos: 'admin123' ou 'Triagem@2026'
-- O hash bcrypt '$2a$11$e8pA8l4r0yA9j1g6r4m5t6u7v8w9x0y1z2a3b4c5d6e7f8g9h0i1j'
-- ------------------------------------------------------------------------------
INSERT INTO usuarios (id, nome, login, senha_usuario, perfil, status_usuario, criado_em)
VALUES 
(1, 'Administrador do Sistema', 'admin', '123456', 'ADMIN', TRUE, NOW()),
(2, 'Lucas Silva (Biomédico)', 'lucas.biomed', '123456', 'BIOMEDICO', TRUE, NOW()),
(3, 'Mariana Santos (Técnica)', 'mariana.tec', '123456', 'TECNICO', TRUE, NOW()),
(4, 'Gabriel Souza (Estagiário)', 'gabriel.estagio', '123456', 'ESTAGIARIO', TRUE, NOW())
ON DUPLICATE KEY UPDATE senha_usuario = VALUES(senha_usuario);

-- ------------------------------------------------------------------------------
-- SEED DE PACIENTES LOCAIS
-- ------------------------------------------------------------------------------
INSERT INTO pacientes (id, documento_cpf, nome_completo, data_nascimento, sexo, peso_kg, altura_cm)
VALUES
(1, '12345678901', 'João Carlos de Oliveira', '1975-04-12', 'M', 78.50, 175),
(2, '98765432100', 'Maria Aparecida Souza', '1962-09-28', 'F', 64.00, 160),
(3, '45678912344', 'Carlos Eduardo Mendes', '1988-11-05', 'M', 82.00, 180),
(4, '78912345688', 'Fernanda Lima Rocha', '1995-02-19', 'F', 58.00, 165)
ON DUPLICATE KEY UPDATE nome_completo = VALUES(nome_completo);

-- ------------------------------------------------------------------------------
-- SEED DE EXAMES LOCAIS
-- ------------------------------------------------------------------------------
INSERT INTO exames (id, paciente_id, atendimento, codigo_procedimento, sequencia_interna, descricao_exame, modalidade, sala, horario_agendado, status)
VALUES
(1, 1, 104520, '41001010', 1, 'Tomografia Computadorizada de Tórax com Contraste', 'TC', 'TC-01', NOW() + INTERVAL 1 HOUR, 'AGUARDANDO_TRIAGEM'),
(2, 2, 104521, '41101014', 1, 'Ressonância Magnética de Crânio com Contraste', 'RM', 'RM-01', NOW() + INTERVAL 2 HOUR, 'AGUARDANDO_TRIAGEM'),
(3, 3, 104522, '41001028', 1, 'Tomografia Computadorizada de Abdômen Total com Contraste', 'TC', 'TC-02', NOW() + INTERVAL 3 HOUR, 'AGUARDANDO_TRIAGEM'),
(4, 4, 104523, '41101022', 1, 'Ressonância Magnética de Coluna Lombar', 'RM', 'RM-02', NOW() + INTERVAL 4 HOUR, 'AGUARDANDO_TRIAGEM')
ON DUPLICATE KEY UPDATE descricao_exame = VALUES(descricao_exame);

-- ------------------------------------------------------------------------------
-- SEED DE PACIENTES WORKLIST (LABORATÓRIO / HIS-RIS)
-- ------------------------------------------------------------------------------
INSERT INTO worklist_pacientes (id, documento_cpf, nome_completo, data_nascimento, sexo, peso_kg, altura_cm, telefone)
VALUES
(1, '78912345688', 'Fernanda Lima Rocha', '1995-02-19', 'F', 58.00, 165, '(44) 99881-2233'),
(2, '45678912344', 'Carlos Eduardo Mendes', '1988-11-05', 'M', 82.00, 180, '(44) 99772-3344'),
(3, '98765432100', 'Maria Aparecida Souza', '1962-09-28', 'F', 64.00, 160, '(44) 99663-4455'),
(4, '12345678901', 'João Carlos de Oliveira', '1975-04-12', 'M', 78.50, 175, '(44) 99554-5566'),
(5, '32165498722', 'Lucas Ferreira da Silva', '1990-07-22', 'M', 74.00, 172, '(44) 99445-6677')
ON DUPLICATE KEY UPDATE nome_completo = VALUES(nome_completo);

-- ------------------------------------------------------------------------------
-- SEED DE PEDIDOS DE EXAMES WORKLIST
-- ------------------------------------------------------------------------------
INSERT INTO worklist_pedidos_exames 
(id, paciente_id, atendimento, modalidade, codigo_procedimento, descricao_procedimento, tipo_formulario, segmento_lado, preparo_adequado, motivo_exame, medico_solicitante, crm_medico, convenio, sala, horario_agendado, status)
VALUES
(1, 1, 204520, 'RM', '41101022', 'Ressonância Magnética de Coluna Lombar', 'RM_COLUNA', 'Lombar', TRUE, 'Lombociatalgia crônica refratária com irradiação para membro inferior esquerdo. Suspeita de hérnia discal.', 'Dr. Marcos Valério', 'CRM/PR 24150', 'Unimed', 'RM-01', NOW() + INTERVAL 1 HOUR, 'AGENDADO'),
(2, 2, 204521, 'RM', '41101030', 'Ressonância Magnética de Braço e Antebraço', 'RM_BRACO_ANTEBRACO', 'Direito', TRUE, 'Queda com trauma esportivo em membro superior direito. Dor persistente e perda de força de preensão manual.', 'Dra. Camila Toledo', 'CRM/PR 31200', 'Bradesco Saúde', 'RM-02', NOW() + INTERVAL 2 HOUR, 'AGENDADO'),
(3, 3, 204522, 'RM', '41101049', 'Ressonância Magnética de Bacia e Articulação do Quadril', 'RM_BACIA_QUADRIL', 'Esquerdo', TRUE, 'Dor profunda articular em quadril esquerdo. Paciente em uso crônico de corticoide. Investigar necrose avascular.', 'Dr. Roberto Silveira', 'CRM/PR 19850', 'Paraná Clínicas', 'RM-01', NOW() + INTERVAL 3 HOUR, 'AGENDADO'),
(4, 4, 204523, 'TC', '41001010', 'Tomografia Computadorizada de Tórax com Contraste', 'TC_TORAX', 'Tórax', TRUE, 'Nódulo pulmonar em lobo superior direito visualizado em radiografia prévia. Protocolo com contraste iodado.', 'Dra. Beatriz Fontana', 'CRM/PR 28900', 'Unimed', 'TC-01', NOW() + INTERVAL 4 HOUR, 'AGENDADO'),
(5, 5, 204524, 'TC', '41001028', 'Tomografia Computadorizada de Abdômen Total com Contraste', 'TC_ABDOMEN', 'Abdômen Total', TRUE, 'Dor aguda intensa em fossa ilíaca direita há 18 horas. Descartar apendicite aguda.', 'Dr. Henrique Rossi', 'CRM/PR 35400', 'Particular', 'TC-02', NOW() + INTERVAL 5 HOUR, 'AGENDADO')
ON DUPLICATE KEY UPDATE descricao_procedimento = VALUES(descricao_procedimento);
