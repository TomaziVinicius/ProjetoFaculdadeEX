-- ==============================================================================
-- CENÁRIOS DE TESTE, VALIDAÇÕES E CASOS DE ERRO (TC & RM)
-- SISTEMA DE TRIAGEM SEGURA EM DIAGNÓSTICO POR IMAGEM
-- ==============================================================================
-- Arquivo: cenarios_teste_erros.sql
-- Finalidade: Inserir dados pré-configurados para avaliar todas as validações de
--             erro da Worklist, semáforo clínico, tetos de dose e perfis no Swagger.
-- Compatibilidade: SQLite (triagem_imagem.db) e MySQL/MariaDB.
-- ==============================================================================

-- ------------------------------------------------------------------------------
-- 1. USUÁRIO INATIVO (Para testar bloqueio ou reativação no Admin)
-- ------------------------------------------------------------------------------
-- Login: 'operador.bloqueado' | Senha: '123' | Status: FALSE (Inativo)
-- Teste Swagger: PUT /api/v1/admin/usuarios/{id}/toggle-status
INSERT OR IGNORE INTO usuarios (id, nome, login, senha_usuario, perfil, status_usuario, criado_em)
VALUES (10, 'Operador Bloqueado Teste', 'operador.bloqueado', '123456', 'TECNICO', 0, CURRENT_TIMESTAMP);


-- ------------------------------------------------------------------------------
-- 2. PACIENTES ESPECÍFICOS PARA OS CENÁRIOS DE ERRO E LIMITES CLÍNICOS
-- ------------------------------------------------------------------------------

-- Paciente 10: Usado para exame já LIBERADO (Erro E9)
INSERT OR IGNORE INTO pacientes (id, documento_cpf, nome_completo, data_nascimento, sexo, peso_kg, altura_cm, criado_em)
VALUES (10, '11122233344', 'Marcos Roberto (Cenário E9 - Já Liberado)', '1980-05-15', 'M', 75.00, 175, CURRENT_TIMESTAMP);

-- Paciente 11: Usado para exame BLOQUEADO POR SEGURANÇA (Erro E10)
INSERT OR IGNORE INTO pacientes (id, documento_cpf, nome_completo, data_nascimento, sexo, peso_kg, altura_cm, criado_em)
VALUES (11, '22233344455', 'Clara Meireles (Cenário E10 - Bloqueada)', '1972-08-20', 'F', 62.00, 160, CURRENT_TIMESTAMP);

-- Paciente 12: Obeso 125 kg (Para testar Teto de 150 mL de Iodo em TC)
-- 125 kg * 1.5 mL/kg = 187.5 mL -> Corta obrigatoriamente para 150.0 mL
INSERT OR IGNORE INTO pacientes (id, documento_cpf, nome_completo, data_nascimento, sexo, peso_kg, altura_cm, criado_em)
VALUES (12, '33344455566', 'Geraldo Magela (Obeso TC 125kg - Teto 150mL)', '1965-03-10', 'M', 125.00, 178, CURRENT_TIMESTAMP);

-- Paciente 13: Obesa 150 kg (Para testar Teto de 25 mL de Gadolínio em RM)
-- 150 kg * 0.2 mL/kg = 30.0 mL -> Corta obrigatoriamente para 25.0 mL
INSERT OR IGNORE INTO pacientes (id, documento_cpf, nome_completo, data_nascimento, sexo, peso_kg, altura_cm, criado_em)
VALUES (13, '44455566677', 'Valdirene Ramos (Obesa RM 150kg - Teto 25mL)', '1978-11-25', 'F', 150.00, 165, CURRENT_TIMESTAMP);

-- Paciente 14: Cardiopata com Marca-passo em RM (Para testar Semáforo Vermelho)
INSERT OR IGNORE INTO pacientes (id, documento_cpf, nome_completo, data_nascimento, sexo, peso_kg, altura_cm, criado_em)
VALUES (14, '55566677788', 'Arthur Antunes (Cardiopata - Marca-passo RM)', '1955-09-30', 'M', 80.00, 170, CURRENT_TIMESTAMP);

-- Paciente 15: Alérgica Grave a Iodo em TC (Para testar Semáforo Vermelho)
INSERT OR IGNORE INTO pacientes (id, documento_cpf, nome_completo, data_nascimento, sexo, peso_kg, altura_cm, criado_em)
VALUES (15, '66677788899', 'Juliana Silva (Choque Alérgico Iodo TC)', '1992-07-14', 'F', 68.00, 168, CURRENT_TIMESTAMP);

-- Paciente 16: Diabético em Uso Contínuo de Metformina (Para testar Semáforo Amarelo)
INSERT OR IGNORE INTO pacientes (id, documento_cpf, nome_completo, data_nascimento, sexo, peso_kg, altura_cm, criado_em)
VALUES (16, '77788899900', 'Benedito Dias (Uso de Metformina TC)', '1958-12-05', 'M', 84.00, 172, CURRENT_TIMESTAMP);


-- ------------------------------------------------------------------------------
-- 3. EXAMES CONFIGURADOS PARA CADA TESTE DE VALIDAÇÃO
-- ------------------------------------------------------------------------------

-- EXAME 10 (Atendimento: 900501) - STATUS: 'LIBERADO'
-- TESTE DO ERRO E9: Tentar chamar este paciente via POST /api/v1/worklist/10/chamar
-- RESPOSTA ESPERADA: 400 Bad Request ("Não é permitido chamar o paciente pois o exame já se encontra finalizado (LIBERADO)")
INSERT OR IGNORE INTO exames (
    id, paciente_id, atendimento, codigo_procedimento, sequencia_interna, 
    descricao_exame, modalidade, TipoFormulario, SegmentoLado, MotivoExame, 
    MedicoSolicitante, Convenio, sala, horario_agendado, horario_chamada, horario_execucao, status, criado_em
)
VALUES (
    10, 10, 900501, '41001010', 1,
    'Tomografia Computadorizada de Tórax com Contraste', 'TC', 'TC_TORAX', 'Tórax',
    'Controle oncológico anual.', 'Dr. Roberto Silveira (CRM/PR 19850)', 'Unimed',
    'TC-01', datetime('now', '-2 hours'), datetime('now', '-1 hours'), datetime('now', '-30 minutes'), 'LIBERADO', CURRENT_TIMESTAMP
);

-- EXAME 11 (Atendimento: 900502) - STATUS: 'BLOQUEADO_SEGURANCA'
-- TESTE DO ERRO E10: Tentar chamar este paciente via POST /api/v1/worklist/11/chamar
-- RESPOSTA ESPERADA: 400 Bad Request ("Não é permitido chamar o paciente pois o exame foi bloqueado por contraindicação clínica de segurança")
INSERT OR IGNORE INTO exames (
    id, paciente_id, atendimento, codigo_procedimento, sequencia_interna, 
    descricao_exame, modalidade, TipoFormulario, SegmentoLado, MotivoExame, 
    MedicoSolicitante, Convenio, sala, horario_agendado, horario_chamada, horario_execucao, status, criado_em
)
VALUES (
    11, 11, 900502, '41101022', 1,
    'Ressonância Magnética de Coluna Lombar', 'RM', 'RM_COLUNA', 'Lombar',
    'Lombociatalgia crônica refratária com irradiação.', 'Dra. Camila Toledo (CRM/PR 31200)', 'Bradesco Saúde',
    'RM-01', datetime('now', '-3 hours'), datetime('now', '-2 hours'), NULL, 'BLOQUEADO_SEGURANCA', CURRENT_TIMESTAMP
);

-- EXAME 12 (Atendimento: 900503) - STATUS: 'AGUARDANDO_TRIAGEM'
-- TESTE DE TETO DE CONTRASTE IODADO EM TC:
-- Paciente de 125 kg -> POST /api/v1/triagem/calcular-dose (modalidade: 1, pesoKg: 125)
-- RESPOSTA ESPERADA: volumeCalculadoMl = 187.5, volumeAjustadoMl = 150.0, tetoMaximoAtingido = true
INSERT OR IGNORE INTO exames (
    id, paciente_id, atendimento, codigo_procedimento, sequencia_interna, 
    descricao_exame, modalidade, TipoFormulario, SegmentoLado, MotivoExame, 
    MedicoSolicitante, Convenio, sala, horario_agendado, status, criado_em
)
VALUES (
    12, 12, 900503, '41001028', 1,
    'Tomografia Computadorizada de Abdômen Total com Contraste', 'TC', 'TC_TORAX', 'Abdômen',
    'Dor abdominal difusa em paciente obeso.', 'Dr. Marcos Valério (CRM/PR 24150)', 'Unimed',
    'TC-02', datetime('now', '+1 hours'), 'AGUARDANDO_TRIAGEM', CURRENT_TIMESTAMP
);

-- EXAME 13 (Atendimento: 900504) - STATUS: 'AGUARDANDO_TRIAGEM'
-- TESTE DE TETO DE GADOLÍNIO EM RM:
-- Paciente de 150 kg -> POST /api/v1/triagem/calcular-dose (modalidade: 2, pesoKg: 150)
-- RESPOSTA ESPERADA: volumeCalculadoMl = 30.0, volumeAjustadoMl = 25.0, tetoMaximoAtingido = true
INSERT OR IGNORE INTO exames (
    id, paciente_id, atendimento, codigo_procedimento, sequencia_interna, 
    descricao_exame, modalidade, TipoFormulario, SegmentoLado, MotivoExame, 
    MedicoSolicitante, Convenio, sala, horario_agendado, status, criado_em
)
VALUES (
    13, 13, 900504, '41101049', 1,
    'Ressonância Magnética de Bacia e Articulação do Quadril', 'RM', 'RM_BACIA_QUADRIL', 'Bacia',
    'Coxalgia severa progressiva.', 'Dr. Roberto Silveira (CRM/PR 19850)', 'Paraná Clínicas',
    'RM-02', datetime('now', '+2 hours'), 'AGUARDANDO_TRIAGEM', CURRENT_TIMESTAMP
);

-- EXAME 14 (Atendimento: 900505) - STATUS: 'AGUARDANDO_TRIAGEM'
-- TESTE DE SEMÁFORO VERMELHO (MARCA-PASSO EM RM):
-- Enviar resposta RM_MARCAPASSO = "SIM" -> Sistema bloqueia liberação com modal explicativo
INSERT OR IGNORE INTO exames (
    id, paciente_id, atendimento, codigo_procedimento, sequencia_interna, 
    descricao_exame, modalidade, TipoFormulario, SegmentoLado, MotivoExame, 
    MedicoSolicitante, Convenio, sala, horario_agendado, status, criado_em
)
VALUES (
    14, 14, 900505, '41101022', 1,
    'Ressonância Magnética de Coluna Cervical', 'RM', 'RM_COLUNA', 'Cervical',
    'Cervicobraquialgia intensa com parestesia.', 'Dra. Camila Toledo (CRM/PR 31200)', 'Bradesco Saúde',
    'RM-01', datetime('now', '+3 hours'), 'AGUARDANDO_TRIAGEM', CURRENT_TIMESTAMP
);

-- EXAME 15 (Atendimento: 900506) - STATUS: 'AGUARDANDO_TRIAGEM'
-- TESTE DE SEMÁFORO VERMELHO (ALERGIA GRAVE A IODO EM TC):
-- Enviar resposta TC_ALERGIA_IODO = "SIM" -> Bloqueio imediato por risco de choque anafilático
INSERT OR IGNORE INTO exames (
    id, paciente_id, atendimento, codigo_procedimento, sequencia_interna, 
    descricao_exame, modalidade, TipoFormulario, SegmentoLado, MotivoExame, 
    MedicoSolicitante, Convenio, sala, horario_agendado, status, criado_em
)
VALUES (
    15, 15, 900506, '41001010', 1,
    'Tomografia Computadorizada de Crânio com Contraste', 'TC', 'TC_TORAX', 'Crânio',
    'Cefaleia refratária com escotomas.', 'Dr. Marcos Valério (CRM/PR 24150)', 'Unimed',
    'TC-01', datetime('now', '+4 hours'), 'AGUARDANDO_TRIAGEM', CURRENT_TIMESTAMP
);

-- EXAME 16 (Atendimento: 900507) - STATUS: 'AGUARDANDO_TRIAGEM'
-- TESTE DE SEMÁFORO AMARELO (METFORMINA EM TC):
-- Enviar resposta TC_DIABETES_METFORMINA = "SIM" -> Semáforo Amarelo com conduta de suspensão por 48h
INSERT OR IGNORE INTO exames (
    id, paciente_id, atendimento, codigo_procedimento, sequencia_interna, 
    descricao_exame, modalidade, TipoFormulario, SegmentoLado, MotivoExame, 
    MedicoSolicitante, Convenio, sala, horario_agendado, status, criado_em
)
VALUES (
    16, 16, 900507, '41001010', 1,
    'Tomografia Computadorizada de Abdômen e Pelve', 'TC', 'TC_TORAX', 'Abdômen',
    'Avaliação de esteatose hepática e litíase biliar.', 'Dra. Beatriz Fontana (CRM/PR 28900)', 'Paraná Clínicas',
    'TC-02', datetime('now', '+5 hours'), 'AGUARDANDO_TRIAGEM', CURRENT_TIMESTAMP
);


-- ------------------------------------------------------------------------------
-- 4. FORMULÁRIO DE TRIAGEM PRÉ-CONCLUÍDO (Para o Exame 10 Liberado)
-- ------------------------------------------------------------------------------
INSERT OR IGNORE INTO formularios_triagem (
    id, exame_id, usuario_responsavel_id, volume_contraste_ml, lote_contraste, 
    caminho_arquivo_armazenado, status_risco, observacoes_clinicas, iniciado_em, finalizado_em
)
VALUES (
    10, 10, 2, 112.50, 'LOT-IODO-900', 
    'Relatorio_Atendimento_900501.html', 'VERDE', 'Triagem concluída com sucesso e exame liberado para o gantry.',
    datetime('now', '-1 hours'), datetime('now', '-30 minutes')
);

-- ------------------------------------------------------------------------------
-- 5. RESUMO DE COMANDOS PARA TESTAR NO SWAGGER (http://localhost:5000/swagger)
-- ------------------------------------------------------------------------------
-- 1. Erro E1: GET  /api/v1/worklist/0              -> 400 Bad Request (ID <= 0)
-- 2. Erro E2: GET  /api/v1/worklist/999999         -> 404 Not Found (Exame inexistente)
-- 3. Erro E3: GET  /api/v1/worklist?modalidade=99  -> 400 Bad Request (Modalidade inválida)
-- 4. Erro E4: GET  /api/v1/worklist?status=99      -> 400 Bad Request (Status inválido)
-- 5. Erro E6: POST /api/v1/worklist/0/chamar       -> 400 Bad Request (ID <= 0)
-- 6. Erro E7: POST /api/v1/worklist/1/chamar       -> 400 Bad Request (usuarioId: 0)
-- 7. Erro E8: POST /api/v1/worklist/999999/chamar  -> 404 Not Found (Exame inexistente)
-- 8. Erro E9: POST /api/v1/worklist/10/chamar      -> 400 Bad Request (Exame 10 já está LIBERADO)
-- 9. Erro E10: POST /api/v1/worklist/11/chamar     -> 400 Bad Request (Exame 11 está BLOQUEADO_SEGURANCA)

-- ------------------------------------------------------------------------------
-- 6. NOVOS CENÁRIOS: TUSS/RIS OBRIGATÓRIO, PROTOCOLOS INÉDITOS E CRUD ADMIN
-- ------------------------------------------------------------------------------

-- CENÁRIO E11: VALIDAÇÃO DE UNICIDADE E OBRIGATORIEDADE DE CÓDIGO TUSS/RIS
-- Tentativa de vincular ou criar exame com código TUSS já existente no catálogo:
-- TUSS '41101153' (Já pertence à 'RM de Coluna Cervical' em RM_COLUNA).
-- Comportamento esperado: O sistema rejeita com erro:
-- "Código TUSS/RIS '41101153' já cadastrado no exame 'RM de Coluna Cervical' (Protocolo: RM Coluna). Códigos duplicados não são permitidos."

-- CENÁRIO E12: EXAME COM PROTOCOLO ANATÔMICO INÉDITO (FALLBACK DE SEGURANÇA RM/TC)
-- Inserção de exame com protocolo não mapeado ('RM_CRANIO' ou 'TC_FACE'):
INSERT OR IGNORE INTO pacientes (id, nome_completo, documento_cpf, data_nascimento, sexo, peso_kg, criado_em)
VALUES (17, 'Beatriz Alvarenga (Cenário E12 - Protocolo Inédito)', '55566677788', '1988-06-20', 'F', 61.0, CURRENT_TIMESTAMP);

INSERT OR IGNORE INTO exames (
    id, paciente_id, atendimento, codigo_procedimento, sequencia_interna, 
    descricao_exame, modalidade, TipoFormulario, SegmentoLado, MotivoExame, 
    MedicoSolicitante, Convenio, sala, horario_agendado, status, criado_em
)
VALUES (
    17, 17, 900508, '41101999', 1,
    'Ressonância Magnética de Encéfalo com Protocolo Inédito', 'RM', 'RM_CRANIO_INEDITO', 'Crânio',
    'Cefaleia refratária para avaliação de fallback de segurança.', 'Dr. Marcos Valério (CRM/PR 24150)', 'Unimed',
    'RM-01', datetime('now', '+6 hours'), 'AGUARDANDO_TRIAGEM', CURRENT_TIMESTAMP
);
-- Comportamento esperado: Ao abrir a triagem na tela, obterPerguntasExameAtual() detecta que o protocolo
-- não existe e invoca o Fallback de Segurança Universal de RM (marcapasso, clipes cerebrais, implante coclear),
-- impedindo acidentes com o magneto e sem travar a interface.

-- CENÁRIO E13: EDIÇÃO DE EXAME E RECALCULO DE CONTRASTE VIA ADMIN (PUT /api/v1/admin/exames/{id})
-- Altera o peso do paciente do Exame 1 de 58 kg para 120 kg.
-- Payload: { "pacienteNome": "Fernanda Lima Rocha", "pesoKg": 120.0, "modalidade": 2, "procedimento": "RM Coluna Lombar", "sala": "RM-01" }
-- Comportamento esperado: 200 OK, peso atualizado no banco, dose recalculada para o teto rígido de 25 mL de Gadolínio.

-- CENÁRIO E14: EXCLUSÃO DE EXAME COM LIMPEZA EM CASCATA (DELETE /api/v1/admin/exames/{id})
-- Endpoint: DELETE /api/v1/admin/exames/17
-- Comportamento esperado: 200 OK. Remove formulários/respostas vinculados, desaloca da grade e gera log na tabela 'auditoria_logs' com tipo 'EXAME_EXCLUIDO'.

-- CENÁRIO E15: EXCLUSÃO DE PROTOCOLO CUSTOMIZADO COM EXAME ATIVO NA GRADE (TRAVA DE SEGURANÇA)
-- Tentar excluir o protocolo no painel enquanto houver exame agendado utilizando o protocolo.
-- Comportamento esperado: O sistema impede a exclusão e exibe o alerta:
-- "Não é possível excluir o protocolo pois existem exames agendados na grade utilizando-o."

