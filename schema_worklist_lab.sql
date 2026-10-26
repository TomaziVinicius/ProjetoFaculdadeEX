-- ==============================================================================
-- SISTEMA DE TRIAGEM PARA DIAGNÓSTICO POR IMAGEM
-- BANCO DE DADOS 2: WORKLIST LABORATORIAL / HIS-RIS (HOSPITAL INFORMATION SYSTEM)
-- Este banco simula o sistema central de agendamento de exames do hospital/laboratório.
-- A aplicação de Triagem se conecta a esta Worklist para identificar os pacientes do dia
-- e carregar o questionário clínico específico (Coluna, Braço/Antebraço, Bacia/Quadril, TC).
-- ==============================================================================

CREATE DATABASE IF NOT EXISTS worklist_lab_db CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE worklist_lab_db;

-- 1. TABELA DE PACIENTES DA WORKLIST (CADASTRO DO LABORATÓRIO / CONVÊNIO)
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

-- 2. TABELA DE PEDIDOS DE EXAME AGENDADOS (WORKLIST DE ENTRADA)
CREATE TABLE IF NOT EXISTS worklist_pedidos_exames (
    id INT AUTO_INCREMENT PRIMARY KEY,
    paciente_id INT NOT NULL,
    atendimento INT NOT NULL UNIQUE,
    modalidade ENUM('TC', 'RM') NOT NULL,
    codigo_procedimento VARCHAR(30) NOT NULL,
    descricao_procedimento VARCHAR(150) NOT NULL,
    tipo_formulario VARCHAR(50) NOT NULL, -- 'RM_COLUNA', 'RM_BRACO_ANTEBRACO', 'RM_BACIA_QUADRIL', 'TC_TORAX', 'TC_ABDOMEN'
    segmento_lado VARCHAR(50) NULL,       -- 'Cervical', 'Lombar', 'Direito', 'Esquerdo', etc.
    preparo_adequado BOOLEAN NOT NULL DEFAULT TRUE,
    preparo_motivo_inadequado VARCHAR(255) NULL,
    motivo_exame TEXT NULL,
    medico_solicitante VARCHAR(100) NOT NULL,
    crm_medico VARCHAR(20) NOT NULL,
    convenio VARCHAR(50) NOT NULL DEFAULT 'Unimed',
    sala VARCHAR(20) NOT NULL,
    horario_agendado DATETIME NOT NULL,
    horario_chamada DATETIME NULL,
    status VARCHAR(30) NOT NULL DEFAULT 'AGENDADO', -- 'AGENDADO', 'EM_TRIAGEM', 'LIBERADO', 'BLOQUEADO_SEGURANCA', 'CONCLUIDO'
    criado_em DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_worklist_paciente FOREIGN KEY (paciente_id) REFERENCES worklist_pacientes(id) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE INDEX idx_worklist_horario ON worklist_pedidos_exames(horario_agendado);
CREATE INDEX idx_worklist_status ON worklist_pedidos_exames(status);
CREATE INDEX idx_worklist_tipo_form ON worklist_pedidos_exames(tipo_formulario);

-- ==============================================================================
-- CARGA INICIAL DE DEMONSTRAÇÃO (SEED DO BANCO DE DADOS 2)
-- Contempla os 3 relatórios especializados de RM solicitados + Exames de TC
-- ==============================================================================

INSERT INTO worklist_pacientes (id, documento_cpf, nome_completo, data_nascimento, sexo, peso_kg, altura_cm, telefone)
VALUES
(1, '78912345688', 'Fernanda Lima Rocha', '1995-02-19', 'F', 58.00, 165, '(44) 99881-2233'),
(2, '45678912344', 'Carlos Eduardo Mendes', '1988-11-05', 'M', 82.00, 180, '(44) 99772-3344'),
(3, '98765432100', 'Maria Aparecida Souza', '1962-09-28', 'F', 64.00, 160, '(44) 99663-4455'),
(4, '12345678901', 'João Carlos de Oliveira', '1975-04-12', 'M', 78.50, 175, '(44) 99554-5566'),
(5, '32165498722', 'Lucas Ferreira da Silva', '1990-07-22', 'M', 74.00, 172, '(44) 99445-6677')
ON DUPLICATE KEY UPDATE nome_completo = VALUES(nome_completo);

INSERT INTO worklist_pedidos_exames 
(id, paciente_id, atendimento, modalidade, codigo_procedimento, descricao_procedimento, tipo_formulario, segmento_lado, preparo_adequado, motivo_exame, medico_solicitante, crm_medico, convenio, sala, horario_agendado, status)
VALUES
-- 1. Formulário Especializado: RM DE COLUNA (Cervical / Torácica / Lombar)
(1, 1, 104520, 'RM', '41101022', 'Ressonância Magnética de Coluna Lombar', 'RM_COLUNA', 'Lombar', TRUE, 'Lombociatalgia crônica refratária com irradiação para membro inferior esquerdo. Suspeita de hérnia discal.', 'Dr. Marcos Valério', 'CRM/PR 24150', 'Unimed', 'RM-01', '2026-10-26 08:00:00', 'AGENDADO'),

-- 2. Formulário Especializado: RM DE BRAÇO / ANTEBRAÇO (Membro Superior)
(2, 2, 104521, 'RM', '41101030', 'Ressonância Magnética de Braço e Antebraço', 'RM_BRACO_ANTEBRACO', 'Direito', TRUE, 'Queda com trauma esportivo em membro superior direito. Dor persistente e perda de força de preensão manual.', 'Dra. Camila Toledo', 'CRM/PR 31200', 'Bradesco Saúde', 'RM-02', '2026-10-26 08:45:00', 'AGENDADO'),

-- 3. Formulário Especializado: RM DE BACIA / QUADRIL (Articulações Coxofemorais)
(3, 3, 104522, 'RM', '41101049', 'Ressonância Magnética de Bacia e Articulação do Quadril', 'RM_BACIA_QUADRIL', 'Esquerdo', TRUE, 'Dor profunda articular em quadril esquerdo. Paciente em uso crônico de corticoide. Investigar necrose avascular.', 'Dr. Roberto Silveira', 'CRM/PR 19850', 'Paraná Clínicas', 'RM-01', '2026-10-26 09:30:00', 'AGENDADO'),

-- 4. Exame de Tomografia Computadorizada: TC de Tórax
(4, 4, 104523, 'TC', '41001010', 'Tomografia Computadorizada de Tórax com Contraste', 'TC_TORAX', 'Tórax', TRUE, 'Nódulo pulmonar em lobo superior direito visualizado em radiografia prévia. Protocolo com contraste iodado.', 'Dra. Beatriz Fontana', 'CRM/PR 28900', 'Unimed', 'TC-01', '2026-10-26 10:15:00', 'AGENDADO'),

-- 5. Exame de Tomografia Computadorizada: TC de Abdômen Total
(5, 5, 104524, 'TC', '41001028', 'Tomografia Computadorizada de Abdômen Total com Contraste', 'TC_ABDOMEN', 'Abdômen Total', TRUE, 'Dor aguda intensa em fossa ilíaca direita há 18 horas. Descartar apendicite aguda.', 'Dr. Henrique Rossi', 'CRM/PR 35400', 'Particular', 'TC-02', '2026-10-26 11:00:00', 'AGENDADO')
ON DUPLICATE KEY UPDATE descricao_procedimento = VALUES(descricao_procedimento);
