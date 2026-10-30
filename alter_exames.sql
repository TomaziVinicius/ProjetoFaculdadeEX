ALTER TABLE exames 
ADD COLUMN tipo_formulario VARCHAR(50) NOT NULL DEFAULT 'RM_COLUNA' AFTER modalidade,
ADD COLUMN segmento_lado VARCHAR(50) NULL AFTER tipo_formulario,
ADD COLUMN motivo_exame TEXT NULL AFTER segmento_lado,
ADD COLUMN medico_solicitante VARCHAR(100) NULL AFTER motivo_exame,
ADD COLUMN convenio VARCHAR(50) NULL AFTER medico_solicitante;
