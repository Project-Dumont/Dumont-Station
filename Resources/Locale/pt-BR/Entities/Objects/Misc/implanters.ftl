# Base
ent-BaseImplanter = implantador

ent-Implanter = { ent-BaseImplanter }
    .desc = Uma seringa descartável exclusivamente projetada para implantação e extração de implantes subdérmicos.

ent-ImplanterAdmeme = { ent-BaseImplanter }
    .desc = { ent-Implanter.desc }
    .suffix = Admeme

ent-BaseImplantOnlyImplanter = { ent-BaseImplanter }
    .desc = Uma seringa descartável exclusivamente projetada para implantação de implantes subdérmicos.

ent-BaseImplantOnlyImplanterSyndi = { ent-BaseImplanter } do Sindicato
    .desc = { ent-Implanter.desc } Tenha certeza de remover qualquer DNA residual com sabão ou pano de limpeza após usar!

# Central de Comando
ent-RadioImplanterCentcomm = { ent-BaseImplantOnlyImplanterCentcomm }
    .desc = { ent-BaseImplantOnlyImplanterCentcomm.desc }
    .suffix = rádio da Central de Comando

ent-DeathRattleImplanterCentcomm = { ent-BaseImplantOnlyImplanterCentcomm }
    .desc = { ent-BaseImplantOnlyImplanterCentcomm.desc }
    .suffix = { ent-DeathRattleImplanter.suffix } da Central de Comando

# Tripulantes
# Diversão
ent-SadTromboneImplanter = { ent-BaseImplanter }
    .desc = { ent-BaseImplantOnlyImplanter.desc }
    .suffix = trombone triste

ent-LightImplanter = { ent-BaseImplanter }
    .desc = { ent-BaseImplantOnlyImplanter.desc }
    .suffix = luz

ent-BikeHornImplanter = { ent-BaseImplanter }
    .desc = { ent-BaseImplantOnlyImplanter.desc }
    .suffix = buzina

# Segurança
ent-TrackingImplanter = { ent-BaseImplanter }
    .desc = { ent-BaseImplantOnlyImplanter.desc }
    .suffix = rastreador

# Segurança e Comando
ent-MindShieldImplanter = { ent-BaseImplanter }
    .desc = { ent-BaseImplantOnlyImplanter.desc }
    .suffix = psicoproteção

# Antagonistas
# Traidores
ent-StorageImplanter = { ent-BaseImplantOnlyImplanterSyndi }
    .desc = { ent-BaseImplantOnlyImplanterSyndi.desc }
    .suffix = armazenamento

ent-FreedomImplanter = { ent-BaseImplantOnlyImplanterSyndi }
    .desc = { ent-BaseImplantOnlyImplanterSyndi.desc }
    .suffix = liberdade

ent-RadioImplanter = { ent-BaseImplantOnlyImplanterSyndi }
    .desc = { ent-BaseImplantOnlyImplanterSyndi.desc }
    .suffix = rádio do Sindicato

ent-UplinkImplanter = { ent-BaseImplantOnlyImplanterSyndi }
    .desc = { ent-BaseImplantOnlyImplanterSyndi.desc }
    .suffix = uplink

ent-EmpImplanter = { ent-BaseImplantOnlyImplanterSyndi }
    .desc = { ent-BaseImplantOnlyImplanterSyndi.desc }
    .suffix = PEM

ent-ScramImplanter = { ent-BaseImplantOnlyImplanterSyndi }
    .desc = { ent-BaseImplantOnlyImplanterSyndi.desc }
    .suffix = fuga

ent-DnaScramblerImplanter = { ent-BaseImplantOnlyImplanterSyndi }
    .desc = { ent-BaseImplantOnlyImplanterSyndi.desc }
    .suffix = embaralhador de DNA

ent-ChameleonControllerImplanter = { ent-BaseImplantOnlyImplanterSyndi }
    .desc = { ent-BaseImplantOnlyImplanterSyndi.desc }
    .suffix = controlador camaleão

ent-FakeMindShieldImplanter = { ent-BaseImplantOnlyImplanterSyndi }
    .desc = { ent-BaseImplantOnlyImplanterSyndi.desc }
    .suffix = { ent-MindShieldImplanter.suffix } falsa

# Nukies
ent-MicroBombImplanter = { ent-BaseImplantOnlyImplanterSyndi }
    .desc = { ent-BaseImplantOnlyImplanterSyndi.desc }
    .suffix = microbomba

ent-MacroBombImplanter = { ent-BaseImplantOnlyImplanterSyndi }
    .desc = { ent-BaseImplantOnlyImplanterSyndi.desc }
    .suffix = macrobomba

ent-DeathRattleImplanter = { ent-BaseImplantOnlyImplanterSyndi }
    .desc = { ent-BaseImplantOnlyImplanterSyndi.desc }
    .suffix = notificador de morte

ent-DeathAcidifierImplanter = { ent-BaseImplantOnlyImplanterSyndi }
    .desc = { ent-BaseImplantOnlyImplanterSyndi.desc }
    .suffix = acidificador de morte

# Nukies Especial
ent-HostageImplanter = { ent-BaseImplantOnlyImplanterSyndi }
    .desc = { ent-BaseImplantOnlyImplanterSyndi.desc }
    .suffix = refém
