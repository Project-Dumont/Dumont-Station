# Base
ent-BaseImplanter = implantador

ent-Implanter = { ent-BaseImplanter }
    .desc = Uma seringa descartável exclusivamente projetada para implantação e extração de implantes subdérmicos.

ent-ImplanterAdmeme = { ent-BaseImplanter }
    .desc = { ent-Implanter.desc }
    .suffix = Admeme

ent-BaseImplantOnlyImplanter = { ent-BaseImplanter }
    .desc = Uma seringa descartável exclusivamente projetada para implantação de implantes subdérmicos.

ent-BaseImplantOnlyImplanterSyndi = { ent-BaseImplanter } do sindicato
    .desc = { ent-Implanter.desc }. Tenha certeza de remover qualquer DNA residual com um sabão ou pano de limpeza após usar!

# Diversão
ent-SadTromboneImplanter = { ent-BaseImplanter }
    .suffix = Trombone Triste

ent-LightImplanter = { ent-BaseImplanter }
    .suffix = Luz

ent-BikeHornImplanter = { ent-BaseImplanter }
    .suffix = Buzina

# Segurança
ent-TrackingImplanter = { ent-BaseImplanter }
    .suffix = Rastreador
    
# Segurança e Comando
ent-MindShieldImplanter = { ent-BaseImplanter }
    .suffix = MindShield

# Traidores
ent-StorageImplanter = { ent-BaseImplantOnlyImplanterSyndi }
    .suffix = Armazenamento

ent-FreedomImplanter = { ent-BaseImplantOnlyImplanterSyndi }
    .suffix = Liberdade

ent-RadioImplanter = { ent-BaseImplantOnlyImplanterSyndi }
    .suffix = Rádio do Sindicato

ent-UplinkImplanter = { ent-BaseImplantOnlyImplanterSyndi }
    .suffix = Uplink

ent-EmpImplanter = { ent-BaseImplantOnlyImplanterSyndi }
    .suffix = PEM

ent-ScramImplanter = { ent-BaseImplantOnlyImplanterSyndi }
    .suffix = Fuga

ent-DnaScramblerImplanter = { ent-BaseImplantOnlyImplanterSyndi }
    .suffix = Embaralhador de DNA

ent-ChameleonControllerImplanter = { ent-BaseImplantOnlyImplanterSyndi }
    .suffix = Controle Camaleão

# Nukies
ent-MicroBombImplanter = { ent-BaseImplantOnlyImplanterSyndi }
    .suffix = Microbomba

ent-MacroBombImplanter = { ent-BaseImplantOnlyImplanterSyndi }
    .suffix = Macrobomba

ent-DeathRattleImplanter = { ent-BaseImplantOnlyImplanterSyndi }
    .suffix = Notificador de Morte

ent-DeathAcidifierImplanter = { ent-BaseImplantOnlyImplanterSyndi }
    .suffix = Acidificador de Morte

ent-FakeMindShieldImplanter = { ent-BaseImplantOnlyImplanterSyndi }
    .suffix = MindShield Falso

# Nukies Especial
ent-HostageImplanter = { ent-BaseImplantOnlyImplanterSyndi }
    .suffix = Refém

# Central de Comando
ent-RadioImplanterCentcomm = { ent-BaseImplanter }
    .suffix = Rádio da Central

ent-DeathRattleImplanterCentcomm = { ent-BaseImplanter }
    .suffix = Notificador de Morte da Central
