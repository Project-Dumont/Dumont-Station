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
ent-RadioImplanterCentcomm = { ent-BaseImplanter }
    .suffix = rádio da Central de Comando

ent-DeathRattleImplanterCentcomm = { ent-BaseImplanter }
    .suffix = { ent-DeathRattleImplanter.suffix } da Central de Comando

# Tripulantes
# Diversão
ent-SadTromboneImplanter = { ent-BaseImplanter }
    .suffix = trombone triste

ent-LightImplanter = { ent-BaseImplanter }
    .suffix = luz

ent-BikeHornImplanter = { ent-BaseImplanter }
    .suffix = buzina

# Segurança
ent-TrackingImplanter = { ent-BaseImplanter }
    .suffix = rastreador

# Segurança e Comando
ent-MindShieldImplanter = { ent-BaseImplanter }
    .suffix = psicoproteção

# Antagonistas
# Traidores
ent-StorageImplanter = { ent-BaseImplantOnlyImplanterSyndi }
    .suffix = armazenamento

ent-FreedomImplanter = { ent-BaseImplantOnlyImplanterSyndi }
    .suffix = liberdade

ent-RadioImplanter = { ent-BaseImplantOnlyImplanterSyndi }
    .suffix = rádio do Sindicato

ent-UplinkImplanter = { ent-BaseImplantOnlyImplanterSyndi }
    .suffix = uplink

ent-EmpImplanter = { ent-BaseImplantOnlyImplanterSyndi }
    .suffix = PEM

ent-ScramImplanter = { ent-BaseImplantOnlyImplanterSyndi }
    .suffix = fuga

ent-DnaScramblerImplanter = { ent-BaseImplantOnlyImplanterSyndi }
    .suffix = embaralhador de DNA

ent-ChameleonControllerImplanter = { ent-BaseImplantOnlyImplanterSyndi }
    .suffix = controle camaleão

ent-FakeMindShieldImplanter = { ent-BaseImplantOnlyImplanterSyndi }
    .suffix = { ent-MindShieldImplanter.suffix } falsa

# Nukies
ent-MicroBombImplanter = { ent-BaseImplantOnlyImplanterSyndi }
    .suffix = microbomba

ent-MacroBombImplanter = { ent-BaseImplantOnlyImplanterSyndi }
    .suffix = macrobomba

ent-DeathRattleImplanter = { ent-BaseImplantOnlyImplanterSyndi }
    .suffix = notificador de morte

ent-DeathAcidifierImplanter = { ent-BaseImplantOnlyImplanterSyndi }
    .suffix = acidificador de morte

# Nukies Especial
ent-HostageImplanter = { ent-BaseImplantOnlyImplanterSyndi }
    .suffix = refém
