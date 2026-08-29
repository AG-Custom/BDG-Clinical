#!/usr/bin/env python3
"""Gera SQL de importação a partir de medical-data-2026-08-07.json. Não executa no banco."""

from __future__ import annotations

import json
import re
import uuid
from datetime import datetime, timezone
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
JSON_PATH = ROOT / "BGD.CLINICAL.WebApi" / "medical-data-2026-08-07.json"
OUT_PATH = Path(__file__).resolve().parent / "import-medical-data-2026-08-07.sql"

EMPRESA = "0FD97B9D-8269-430A-91CB-34D6F854760E"
UNIDADE = "CDDBE9A2-A702-4CAC-961D-47E86EF29B05"
FUNCIONARIO = "5D8FFA3E-EED6-4BE8-ADDD-8DB67D7B533F"
TIPO_MED = "2A78E45E-B135-4E56-80B3-47C27BADC2AA"
TIPO_INS = "F50060BB-9614-44C9-BE87-33C3B46920CD"

UOM = {
    "mg": "DC6BF681-7EEA-4442-A0EB-B3061391BCCA",
    "ml": "52A272FE-90A5-4948-8BAF-BF5D856EC2EE",
    "un": "25EC8667-8780-45F1-8EB0-6DA934461D02",
    "fr": "ED556A13-1478-4D74-AC7F-FE08AA08D378",
    "amp": "6E918C23-501E-4E69-8F5A-ED1CA301DB0E",
    "cx": "BADB1D92-61BF-4B28-BF38-5F91B9D245F9",
    "g": "C21F5F64-4090-43CD-82FE-1497DE981BDB",
    "kg": "A28B33C7-BEBF-4285-A299-7F2E64A8D08C",
    "L": "F495267D-2FF0-46FE-BCB6-F77B3223C2E6",
}

NS = uuid.UUID("a125db3b-2026-4807-a125-0000db3b0001")

# JSON name -> existing product id
EXISTING = {
    "Drostanolona": "A9F9AE26-65E7-4BAB-AB35-00A743F9F672",
    "Testosterona Cipionato": "A29646CB-BA43-44AC-9746-09C1A6CE26A5",
    "Tirzepatida": "96E18117-8451-4301-9755-0D7ABF4905C0",
    "Testosterona Blend": "EAC5BDD9-E89A-4CEE-8DD5-126C723154F9",
    "Fast Booster": "51DA2725-76EF-4040-BB68-229E2E23F4D1",
    "Recovery Neuronal": "68DCB03C-C75F-4972-AAC4-5328C742195A",
    "Recovery Neural": "68DCB03C-C75F-4972-AAC4-5328C742195A",
    "Ácido alfa-lipóico": "E126662D-9225-4D64-810F-5B1FAA6267BD",
    "Nandrolona": "F1210D74-1ECB-45F5-A6DC-681C1842C08F",
    "Vitamina D": "12532251-5CA0-4EBE-BB58-723A6682CF18",
    "HCG": "A5213763-ED8E-4ED8-8EF1-7D02A2311BD9",
    "Metenolona": "B7A6E092-6C87-40F2-9DD3-9AA98968293E",
    "Testosterona Propionato + Nandrolona": "7FE770D1-D465-4FAB-87DE-AC4345283D30",
    "Propionato + Nandrolona": "7FE770D1-D465-4FAB-87DE-AC4345283D30",
    "Testosterona Cipionato + Nandrolona": "8828EAE9-9426-49CD-95EC-BC7758CBC792",
    "Cipionato de testosterona + Nandrolona": "8828EAE9-9426-49CD-95EC-BC7758CBC792",
    "NADH": "0187C350-B237-451D-A7F5-C8AAEB916CA4",
    "Lipo Grape": "95E994CB-FAC8-4E2D-8413-E7D2E805184E",
    "Complexo B": "A0748CDF-87B4-4CAD-BE5C-EBFFD686AFBD",
    "Seringa 5 mL": "D5E1B812-22FB-4564-8844-6BBC4ED50893",
    "Seringa 5ml": "D5E1B812-22FB-4564-8844-6BBC4ED50893",
    "Seringa 3 mL": "6E2DB789-5B67-4285-86A5-84A5F3E67794",
    "Seringa 3ml": "6E2DB789-5B67-4285-86A5-84A5F3E67794",
    "Seringa de insulina": "6D76D5DF-4CEC-4F1A-AFB0-9B14E9ECB3D4",
    "Seringa insulina": "6D76D5DF-4CEC-4F1A-AFB0-9B14E9ECB3D4",
    "Agulha 40x12": "02314742-EBD4-43C5-9312-DAE69C553BF8",
    "Agulha 40x1,20": "02314742-EBD4-43C5-9312-DAE69C553BF8",
    "Agulha 25x7": "2E02AC21-9A54-4E2E-9C74-8E145C06BC8E",
    "Agulha 25x0,70": "2E02AC21-9A54-4E2E-9C74-8E145C06BC8E",
    "Cateter Intravenoso 22 g": "626F4103-5F2E-48CD-B007-8C4267C5CAAD",
    "Cateter 22": "626F4103-5F2E-48CD-B007-8C4267C5CAAD",
    "Cateter 24": "DCBE9BC0-96B7-4BEF-BCBD-F480C7B912F0",
}

DOSE_MEDS = {
    "coenzima q10",
    "vitamina d",
    "complexo b",
    "hcg",
    "nadh",
    "recovery neuronal",
    "recovery neural",
    "hmb + vitaminas e minerais",
    "insulin suport",
}

# Pacientes já cadastrados na empresa — reutilizar id (sem INSERT paciente/paciente_unidade)
EXISTING_PATIENTS_BY_NAME = {
    "andré luís lopes costa": "645235A9-5170-4705-A8FB-FEB8F7B9011E",
    "andre luis lopes costa": "645235A9-5170-4705-A8FB-FEB8F7B9011E",
    "cintia nonato": "3B480AC9-FE0C-4B01-92D7-EDA0243B76FB",
}


def normalize_name(name: str) -> str:
    return " ".join(name.strip().lower().split())


def gid(key: str) -> str:
    return str(uuid.uuid5(NS, key)).upper()


def sql_str(value: str | None) -> str:
    if value is None:
        return "NULL"
    return "N'" + value.replace("'", "''") + "'"


def sql_dec(value: float | int) -> str:
    return f"{float(value):.4f}".rstrip("0").rstrip(".") if "." in f"{float(value):.4f}" else f"{int(float(value))}"


def map_unit(raw: str | None) -> str:
    if not raw:
        return "un"
    u = raw.strip().lower()
    aliases = {
        "mg": "mg",
        "ml": "ml",
        "mL".lower(): "ml",
        "un": "un",
        "unidade": "un",
        "unidades": "un",
        "fr": "fr",
        "frasco": "fr",
        "frascos": "fr",
        "amp": "amp",
        "cx": "cx",
        "g": "g",
        "kg": "kg",
        "l": "L",
        "litro": "L",
    }
    return aliases.get(u, "un")


def package_control_unit(med_name: str) -> str:
    return "dose" if med_name.strip().lower() in DOSE_MEDS else "mg"


def resolve_product(name: str, created: dict[str, str]) -> str:
    if name in EXISTING:
        return EXISTING[name]
    if name in created:
        return created[name]
    # fuzzy: case-insensitive existing
    for k, v in EXISTING.items():
        if k.lower() == name.lower():
            return v
    raise KeyError(name)


def main() -> None:
    data = json.loads(JSON_PATH.read_text(encoding="utf-8"))
    now = "2026-08-07T21:15:15.876Z"
    lines: list[str] = []

    def w(s: str = "") -> None:
        lines.append(s)

    w("/*")
    w("  Importação medical-data-2026-08-07")
    w(f"  Empresa: {EMPRESA}")
    w(f"  Unidade: {UNIDADE}")
    w(f"  Funcionario/aplicador: {FUNCIONARIO}")
    w("  Pré-condições:")
    w("  - Não existir paciente com observacao LEGACY_ID= nesta empresa")
    w("  - Saldo de estoque dos produtos na unidade deve estar zerado (entradas somam ao saldo)")
    w("  - Rodar UMA vez dentro de TRANSACTION")
    w("  NÃO execute parcialmente. Revise e COMMIT/ROLLBACK manualmente.")
    w("*/")
    w("SET XACT_ABORT ON;")
    w("SET NOCOUNT ON;")
    w("BEGIN TRANSACTION;")
    w()
    w(f"DECLARE @EmpresaId uniqueidentifier = '{EMPRESA}';")
    w(f"DECLARE @UnidadeId uniqueidentifier = '{UNIDADE}';")
    w(f"DECLARE @FuncionarioId uniqueidentifier = '{FUNCIONARIO}';")
    w(f"DECLARE @TipoMedicamentoId uniqueidentifier = '{TIPO_MED}';")
    w(f"DECLARE @TipoInsumoId uniqueidentifier = '{TIPO_INS}';")
    w(f"DECLARE @Now datetime2 = '{now}';")
    w()
    w("IF NOT EXISTS (SELECT 1 FROM empresa WHERE id = @EmpresaId)")
    w("BEGIN RAISERROR(N'Empresa não encontrada.', 16, 1); ROLLBACK TRANSACTION; RETURN; END;")
    w("IF NOT EXISTS (SELECT 1 FROM unidade WHERE id = @UnidadeId AND empresa_id = @EmpresaId)")
    w("BEGIN RAISERROR(N'Unidade não encontrada nesta empresa.', 16, 1); ROLLBACK TRANSACTION; RETURN; END;")
    w("IF NOT EXISTS (SELECT 1 FROM funcionario WHERE id = @FuncionarioId)")
    w("BEGIN RAISERROR(N'Funcionário aplicador não encontrado.', 16, 1); ROLLBACK TRANSACTION; RETURN; END;")
    w("IF EXISTS (SELECT 1 FROM paciente WHERE empresa_id = @EmpresaId AND observacao LIKE N'%LEGACY_ID=%')")
    w("BEGIN RAISERROR(N'Já existem pacientes migrados (LEGACY_ID). Abortado.', 16, 1); ROLLBACK TRANSACTION; RETURN; END;")
    w()

    # Build product map from stockItems
    created_products: dict[str, str] = {}
    stock_items = data.get("stockItems") or []
    for item in stock_items:
        name = item["name"]
        if name in EXISTING or any(k.lower() == name.lower() for k in EXISTING):
            continue
        pid = gid(f"produto:{item['id']}")
        created_products[name] = pid

    w("/* ========== PRODUTOS NOVOS ========== */")
    for item in stock_items:
        name = item["name"]
        if name not in created_products:
            continue
        pid = created_products[name]
        is_med = (item.get("type") or "").lower() == "medication"
        tipo = "@TipoMedicamentoId" if is_med else "@TipoInsumoId"
        uom_key = map_unit(item.get("unit"))
        uom_id = UOM[uom_key]
        min_stock = float(item.get("minimumStock") or 0)
        valor = float(item.get("unitValue") or 0)
        w(
            "INSERT INTO produto ("
            "id, empresa_id, tipo_produto_id, unidade_medida_id, nome, estoque_minimo, valor, "
            "controla_estoque, unidade_embalagem_id, conteudo_por_embalagem, unidade_conteudo_id, "
            "concentracao_por_conteudo, ativo, criado_em"
            ") VALUES ("
            f"'{pid}', @EmpresaId, {tipo}, '{uom_id}', {sql_str(name)}, {sql_dec(min_stock)}, {sql_dec(valor)}, "
            f"1, '{UOM['un']}', 1, '{uom_id}', 1, 1, @Now);"
        )
    w()

    # Resolve helper used by rest
    def prod_id(name: str) -> str:
        return resolve_product(name, created_products)

    w("/* ========== LOTES + ESTOQUE (currentStock) ========== */")
    for item in stock_items:
        qty = float(item.get("currentStock") or 0)
        if qty <= 0:
            continue
        name = item["name"]
        try:
            pid = prod_id(name)
        except KeyError:
            continue
        is_med = (item.get("type") or "").lower() == "medication"
        mov_id = gid(f"mov:{item['id']}")
        if is_med:
            lote_id = gid(f"lote:{item['id']}")
            w(
                "INSERT INTO lote_produto (id, empresa_id, unidade_id, produto_id, codigo, data_validade, ativo, criado_em) "
                f"VALUES ('{lote_id}', @EmpresaId, @UnidadeId, '{pid}', N'MIGRACAO-20260807', '2028-12-31', 1, @Now);"
            )
            w(
                "INSERT INTO movimentacao_estoque ("
                "id, empresa_id, unidade_id, produto_id, lote_produto_id, tipo, motivo, quantidade, "
                "quantidade_embalagem, data, origem, funcionario_id, observacao, criado_em"
                ") VALUES ("
                f"'{mov_id}', @EmpresaId, @UnidadeId, '{pid}', '{lote_id}', N'Entrada', N'Ajuste', "
                f"{sql_dec(qty)}, {sql_dec(qty)}, @Now, N'MIGRACAO_MEDICAL_DATA', @FuncionarioId, "
                f"{sql_str(f'Migração estoque — {name}')}, @Now);"
            )
        else:
            w(
                "INSERT INTO movimentacao_estoque ("
                "id, empresa_id, unidade_id, produto_id, tipo, motivo, quantidade, data, origem, "
                "funcionario_id, observacao, criado_em"
                ") VALUES ("
                f"'{mov_id}', @EmpresaId, @UnidadeId, '{pid}', N'Entrada', N'Ajuste', "
                f"{sql_dec(qty)}, @Now, N'MIGRACAO_MEDICAL_DATA', @FuncionarioId, "
                f"{sql_str(f'Migração estoque — {name}')}, @Now);"
            )
    w()

    # Medications used by patients
    med_names: set[str] = set()
    for p in data["patients"]:
        for m in p.get("medications") or []:
            med_names.add(m["medication"])

    # Also from supply configs
    supply_cfgs = {c["medication"]: c.get("supplies") or [] for c in (data.get("medicationSupplyConfigs") or [])}

    w("/* ========== PROCEDIMENTOS (+ kits) ========== */")
    proc_ids: dict[str, str] = {}
    for med in sorted(med_names):
        try:
            pid = prod_id(med)
        except KeyError:
            # create orphan medication product if somehow missing from stock
            pid = gid(f"produto-extra:{med}")
            created_products[med] = pid
            w(
                "INSERT INTO produto ("
                "id, empresa_id, tipo_produto_id, unidade_medida_id, nome, estoque_minimo, valor, "
                "controla_estoque, unidade_embalagem_id, conteudo_por_embalagem, unidade_conteudo_id, "
                "concentracao_por_conteudo, ativo, criado_em"
                ") VALUES ("
                f"'{pid}', @EmpresaId, @TipoMedicamentoId, '{UOM['mg']}', {sql_str(med)}, 0, 0, "
                f"1, '{UOM['un']}', 1, '{UOM['mg']}', 1, 1, @Now);"
            )
        proc_id = gid(f"procedimento:{med}")
        proc_ids[med] = proc_id
        proc_name = f"Aplicação — {med}"
        if len(proc_name) > 200:
            proc_name = proc_name[:200]
        w(
            "INSERT INTO procedimento (id, empresa_id, nome, produto_aplicado_id, observacoes, ativo, criado_em) "
            f"VALUES ('{proc_id}', @EmpresaId, {sql_str(proc_name)}, '{pid}', "
            f"{sql_str('Procedimento migrado do sistema anterior')}, 1, @Now);"
        )
        for supply in supply_cfgs.get(med, []):
            sname = supply["name"]
            try:
                sid = prod_id(sname)
            except KeyError:
                sid = created_products.get(sname) or gid(f"produto-supply:{supply.get('id', sname)}")
                if sname not in created_products and sname not in EXISTING:
                    created_products[sname] = sid
                    w(
                        "INSERT INTO produto ("
                        "id, empresa_id, tipo_produto_id, unidade_medida_id, nome, estoque_minimo, valor, "
                        "controla_estoque, ativo, criado_em"
                        ") VALUES ("
                        f"'{sid}', @EmpresaId, @TipoInsumoId, '{UOM['un']}', {sql_str(sname)}, 0, 0, 1, 1, @Now);"
                    )
                else:
                    sid = prod_id(sname)
            item_id = gid(f"itemproc:{med}:{supply.get('id', sname)}")
            qty = float(supply.get("quantity") or 1)
            w(
                "INSERT INTO item_procedimento (id, procedimento_id, produto_id, quantidade, criado_em) "
                f"VALUES ('{item_id}', '{proc_id}', '{sid}', {sql_dec(qty)}, @Now);"
            )
    w()

    w("/* ========== PACIENTES + PACOTES + COMPRAS + 1 APLICACAO ========== */")
    corrections = 0
    reused_patients = 0
    patients = data["patients"]
    for p in patients:
        legacy_id = str(p["id"])
        name = p["name"]
        start = p.get("startDate") or now
        existing_patient_id = EXISTING_PATIENTS_BY_NAME.get(normalize_name(name))
        if existing_patient_id:
            paciente_id = existing_patient_id
            reused_patients += 1
            w(f"/* Paciente já existente reutilizado: {name} → {paciente_id} */")
            w(
                "UPDATE paciente SET observacao = CASE "
                "WHEN observacao IS NULL OR LTRIM(RTRIM(observacao)) = N'' "
                f"THEN {sql_str(f'LEGACY_ID={legacy_id}; Migração medical-data-2026-08-07')} "
                f"WHEN observacao LIKE N'%LEGACY_ID=%' THEN observacao "
                f"ELSE CONCAT(observacao, N'; LEGACY_ID={legacy_id}') END, "
                "atualizado_em = @Now "
                f"WHERE id = '{paciente_id}' AND empresa_id = @EmpresaId;"
            )
            # Garante vínculo com a unidade da migração (idempotente)
            pu_id = gid(f"paciente_unidade:{legacy_id}:{UNIDADE}")
            w(
                "IF NOT EXISTS (SELECT 1 FROM paciente_unidade "
                f"WHERE paciente_id = '{paciente_id}' AND unidade_id = @UnidadeId) "
                "INSERT INTO paciente_unidade (id, paciente_id, unidade_id, criado_em) "
                f"VALUES ('{pu_id}', '{paciente_id}', @UnidadeId, @Now);"
            )
        else:
            paciente_id = gid(f"paciente:{legacy_id}")
            obs = f"LEGACY_ID={legacy_id}; startDate={start}; Migração medical-data-2026-08-07"
            if len(obs) > 2000:
                obs = obs[:2000]
            w(
                "INSERT INTO paciente ("
                "id, empresa_id, unidade_id, nome, cpf, telefone, email, data_nascimento, observacao, ativo, criado_em"
                ") VALUES ("
                f"'{paciente_id}', @EmpresaId, @UnidadeId, {sql_str(name)}, NULL, NULL, NULL, NULL, "
                f"{sql_str(obs)}, 1, @Now);"
            )
            pu_id = gid(f"paciente_unidade:{legacy_id}")
            w(
                "INSERT INTO paciente_unidade (id, paciente_id, unidade_id, criado_em) "
                f"VALUES ('{pu_id}', '{paciente_id}', @UnidadeId, @Now);"
            )

        for m in p.get("medications") or []:
            med = m["medication"]
            unit = package_control_unit(med)
            if unit == "mg":
                total = float(m.get("totalMg") or 0)
                completed = float(m.get("completedMg") or 0)
            else:
                total = float(m.get("totalDoses") or 0)
                completed = float(m.get("completedDoses") or 0)

            if completed > total:
                total = completed
                corrections += 1

            if total <= 0:
                # still create empty? skip package if no contracted qty
                continue

            try:
                produto_id = prod_id(med)
            except KeyError:
                raise RuntimeError(f"Produto não mapeado: {med}")

            pacote_id = gid(f"pacote:{legacy_id}:{med}")
            item_pacote_id = gid(f"itempacote:{legacy_id}:{med}")
            compra_id = gid(f"compra:{legacy_id}:{med}")
            pacote_nome = f"Migração — {name} — {med}"
            if len(pacote_nome) > 160:
                pacote_nome = pacote_nome[:160]

            restante = total - completed
            status = "Concluido" if restante <= 0 else "Ativo"

            w(
                "INSERT INTO pacote (id, empresa_id, nome, descricao, valor, ativo, criado_em) VALUES ("
                f"'{pacote_id}', @EmpresaId, {sql_str(pacote_nome)}, "
                f"{sql_str('Pacote exclusivo migrado')}, 0, 1, @Now);"
            )
            w(
                "INSERT INTO item_pacote (id, pacote_id, produto_id, quantidade_total, unidade_medida, criado_em) VALUES ("
                f"'{item_pacote_id}', '{pacote_id}', '{produto_id}', {sql_dec(total)}, {sql_str(unit)}, @Now);"
            )
            w(
                "INSERT INTO compra_paciente ("
                "id, empresa_id, paciente_id, pacote_id, unidade_id, data_compra, status, observacao, criado_em"
                ") VALUES ("
                f"'{compra_id}', @EmpresaId, '{paciente_id}', '{pacote_id}', @UnidadeId, '{start}', "
                f"N'{status}', {sql_str('Migração sistema anterior')}, @Now);"
            )

            if completed > 0:
                app_id = gid(f"aplicacao:{legacy_id}:{med}")
                proc_id = proc_ids[med]
                w(
                    "INSERT INTO aplicacao_paciente ("
                    "id, empresa_id, paciente_id, compra_paciente_id, produto_id, procedimento_id, "
                    "funcionario_id, unidade_id, data_aplicacao, quantidade_utilizada, observacao, "
                    "realizado, cancelada, criado_em"
                    ") VALUES ("
                    f"'{app_id}', @EmpresaId, '{paciente_id}', '{compra_id}', '{produto_id}', '{proc_id}', "
                    f"@FuncionarioId, @UnidadeId, '{start}', {sql_dec(completed)}, "
                    f"{sql_str('Migração sistema anterior — saldo utilizado importado')}, 1, 0, @Now);"
                )
        w()

    w("/* Fim dos inserts */")
    w(f"PRINT N'Importação gerada. Correções total=max(total,completed): {corrections}.';")
    w(
        f"PRINT N'Pacientes no JSON: {len(patients)}. "
        f"Reutilizados: {reused_patients}. Novos: {len(patients) - reused_patients}. "
        f"Produtos novos: {len(created_products)}.';"
    )
    w("PRINT N'Revise os dados e execute COMMIT TRANSACTION; ou ROLLBACK TRANSACTION;';")
    w("-- COMMIT TRANSACTION;")
    w("-- ROLLBACK TRANSACTION;")

    OUT_PATH.write_text("\n".join(lines) + "\n", encoding="utf-8")
    print(f"Wrote {OUT_PATH}")
    print(
        f"patients={len(patients)} reused={reused_patients} "
        f"new_products={len(created_products)} corrections={corrections}"
    )


if __name__ == "__main__":
    main()
