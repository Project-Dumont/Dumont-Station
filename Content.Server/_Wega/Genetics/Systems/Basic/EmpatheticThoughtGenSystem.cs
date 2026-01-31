// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Shared.Damage;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Damage.Components;
using Content.Shared.Genetics;
using Content.Shared.Humanoid;
using Content.Shared.Mind;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Content.Shared.Roles;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Server.Genetics.System;

public sealed class EmpatheticThoughtGenSystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _entityLookup = default!;
    [Dependency] private readonly SharedMindSystem _mindSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<EmpatheticThoughtGenComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            comp.NextTimeTick -= frameTime;

            if (comp.NextTimeTick <= 0)
            {
                comp.NextTimeTick = _random.NextFloat(comp.MinInterval, comp.MaxInterval);
                GenerateEmpatheticThought(uid, comp);
            }
        }
    }

    #region Base Logic
    private void GenerateEmpatheticThought(EntityUid uid, EmpatheticThoughtGenComponent comp)
    {
        var nearbyEntities = _entityLookup.GetEntitiesInRange<HumanoidAppearanceComponent>(Transform(uid).Coordinates, comp.Range)
            .Where(e => e.Owner != uid).ToList();
        if (nearbyEntities.Count == 0)
            return;

        var target = _random.Pick(nearbyEntities);
        var thought = GenerateThoughtFor(target);

        _popup.PopupEntity(thought, uid, uid, PopupType.Medium);
    }

    private string GenerateThoughtFor(EntityUid target)
    {
        if (HasComp<PsyResistGenComponent>(target))
        {
            return "*Pensamentos bloqueados por uma barreira psiónica*";
        }

        if (!TryComp<MobStateComponent>(target, out var mobState) || mobState.CurrentState == MobState.Dead)
        {
            return "*...silêncio mortal...*";
        }

        var thoughts = new List<string>();

        AddHealthThoughts(target, thoughts);

        AddDamageThoughts(target, thoughts);

        if (_mindSystem.TryGetMind(target, out var mindId, out var mind))
        {
            thoughts.AddRange(GetRoleSpecificThoughts(mindId, mind));
        }

        return thoughts.Count > 0
            ? _random.Pick(thoughts)
            : "*...*";
    }

    private void AddHealthThoughts(EntityUid target, List<string> thoughts)
    {
        var healthFraction = GetHealthFraction(target);
        var damageLevel = 1f - healthFraction;

        if (damageLevel == 0f)
            return;

        if (damageLevel <= 0.3f)
        {
            thoughts.AddRange(new[]
            {
                "*Ai, me trupiquei...*",
                "*Tem algo cutucando meu lado...*",
                "*Minha cabeça está latejando um pouco.*",
                "*Vai ficar roxo, com certeza...*",
                "*Estou todo dolorido...*"
            });
        }
        else if (damageLevel <= 0.6f)
        {
            thoughts.AddRange(new[]
            {
                "*Ai, credo, que dor...*",
                "*Preciso estancar isso...*",
                "*Tá difícil respirar...*",
                "*Tudo tá girando...*",
                "*Preciso de um analgésico...*",
                "*Sangue... preciso estancar o sangue.*"
            });
        }
        else if (damageLevel <= 0.9f)
        {
            thoughts.AddRange(new[]
            {
                "*Tô perdendo as forças...*",
                "*Socorro...*",
                "*Não consigo me mexer...*",
                "*Ai, meu Deus, que dor insuportável!*",
                "*É o fim... não vou aguentar...*",
                "*Tudo está escurecendo...*"
            });
        }
        else
        {
            thoughts.AddRange(new[]
            {
                "*É isso... o fim...*",
                "*Adeus...*",
                "*Não aguento mais...*",
                "*Alguém... me salve...*",
                "*Mãe...*",
                "*Tão escuro... tudo escuro...*"
            });
        }
    }

    private void AddDamageThoughts(EntityUid target, List<string> thoughts)
    {
        if (!TryComp<DamageableComponent>(target, out var damage))
            return;

        foreach (var (damageType, amount) in damage.Damage.DamageDict)
        {
            if (amount <= 0) continue;

            switch (damageType)
            {
                case "Asphyxiation":
                    thoughts.AddRange(new[] {
                        "*Não consigo... respirar...*",
                        "*Meus pulmões estão queimando!*",
                        "*Ar... preciso de ar...*",
                        "*Alguém... feche a ventilação...*",
                        "*Minha cabeça está girando... falta oxigênio...*",
                        "*Tudo está embaçado... preciso achar uma roupa espacial...*"
                    });
                    break;

                case "Bloodloss":
                    thoughts.AddRange(new[] {
                        "*Que... frio...*",
                        "*Meu corpo todo... está congelando...*",
                        "*Sangue... tem sangue demais no chão...*",
                        "*Por que está... tudo escurecendo...*",
                        "*Preciso... me enfaixar...*",
                        "*Cadê o médico?! Tô sangrando até morrer!*"
                    });
                    break;

                case "Blunt":
                    thoughts.AddRange(new[] {
                        "*Tô todo dolorido...*",
                        "*Acho que quebrei... algo importante...*",
                        "*Alguém me apresentou pro chão...*",
                        "*Preciso ver... se todos os ossos estão no lugar...*",
                        "*Sensação de que fui atropelado por um caminhão...*",
                        "*Esses roxos vão ficar lindos...*"
                    });
                    break;

                case "Cellular":
                    thoughts.AddRange(new[] {
                        "*Minhas células estão queimando!*",
                        "*Meu corpo... está desmontando...*",
                        "*Tem algo errado... no nível molecular...*",
                        "*Sinto como se estivessem me dissolvendo...*",
                        "*Alguém está brincando com meu DNA...*",
                        "*Isso não é dor... é algo pior...*"
                    });
                    break;

                case "Caustic":
                    thoughts.AddRange(new[] {
                        "*Minha pele está borbulhando!*",
                        "*AHHH! Tá queimando!*",
                        "*Alguém joga água em mim!*",
                        "*Queimadura química... a pior de todas...*",
                        "*Tem cheiro de carne queimada... sou eu?*",
                        "*Preciso de desinfecção urgente!*"
                    });
                    break;

                case "Cold":
                    thoughts.AddRange(new[] {
                        "*Meus dentes estão batendo...*",
                        "*Meu corpo todo... tá tremendo...*",
                        "*Tão frio... que até dói...*",
                        "*Meus dedos... não sinto meus dedos...*",
                        "*Queimadura de frio não é brincadeira...*",
                        "*Onde será que tem um cantinho quente...*"
                    });
                    break;

                case "Heat":
                    thoughts.AddRange(new[] {
                        "*Tão quente...*",
                        "*Estou queimando... literalmente...*",
                        "*Por que eu cheiro a queimado?*",
                        "*Preciso... me arrastar para longe dessas chamas...*",
                        "*Queimaduras... queimaduras por todo lado...*",
                        "*Isso não é uma sauna... isso é o inferno...*"
                    });
                    break;

                case "Piercing":
                case "Slash":
                    thoughts.AddRange(new[] {
                        "*Sangue... muito sangue...*",
                        "*Acho que furaram algo importante...*",
                        "*Cadê o médico?! Tô vazando!*",
                        "*Essa faca estava... nada esterilizada...*",
                        "*Ferimento cortante... o mais nojento...*",
                        "*Alguém... um curativo?!*"
                    });
                    break;

                case "Poison":
                    thoughts.AddRange(new[] {
                        "*Tem algo errado com meu estômago...*",
                        "*Tudo está girando... e não só na minha cabeça...*",
                        "*Vou vomitar... a qualquer momento...*",
                        "*Alguém me envenenou...*",
                        "*Meu fígado... meu fígado dói...*",
                        "*Tem gosto de... morte...*"
                    });
                    break;

                case "Radiation":
                    thoughts.AddRange(new[] {
                        "*Estou sentindo... radiação...*",
                        "*Meus ossos... meus ossos doem...*",
                        "*Na boca... um gosto metálico...*",
                        "*Quantos cânceres... eu tenho agora?*",
                        "*Minha pele... está coçando estranho...*",
                        "*Precisaria... verificar o dosímetro... se eu tivesse um...*"
                    });
                    break;

                case "Shock":
                    thoughts.AddRange(new[] {
                        "*Мышцы сводит...*",
                        "*Тело... не слушается...*",
                        "*Кто-то выключите этот ток!*",
                        "*Судороги... везде судороги...*",
                        "*Ощущение, будто меня жуёт розетка...*",
                        "*Зубы... сами стучат...*"
                    });
                    break;

                case "Holy":
                    thoughts.AddRange(new[] {
                        "*Meus músculos estão se contraindo...*",
                        "*Meu corpo... não me obedece...*",
                        "*Alguém desliga essa eletricidade!*",
                        "*Espasmos... espasmos por todo o corpo...*",
                        "*Sinto como se uma tomada estivesse me mastigando...*",
                        "*Meus dentes... estão batendo sozinhos...*"
                    });
                    break;
            }
        }
    }

    private IEnumerable<string> GetRoleSpecificThoughts(EntityUid mindId, MindComponent mind)
    {
        bool isAntag = false;
        string? antagType = null;

        if (mind.RoleType != "Neutral" && mind.RoleType != "Familiar" && mind.RoleType != "Silicon")
        {
            isAntag = true;
            antagType = mind.RoleType;
        }

        yield return GetRoleTypeThought(mind.RoleType);

        foreach (var roleId in mind.MindRoles)
        {
            if (!TryComp<MindRoleComponent>(roleId, out var role))
                continue;

            if (role.JobPrototype != null)
            {
                yield return GetJobThought(role.JobPrototype.Value);
            }

            if (role.RoleType != null)
            {
                yield return GetRoleTypeThought(role.RoleType);
            }
        }

        if (isAntag)
        {
            yield return GetAntagThought(antagType);
        }
        else
        {
            yield return _random.Pick(new[] {
                "*Queria um café...*",
                "*Quando será que o turno acaba?*",
                "*O que será que tem naquela sala?*",
                "*Onde será que tem algo para comer?*",
                "*Essas lâmpadas estão piscando demais...*",
                "*Alguém devia organizar isso... mas não eu*",
                "*Por que na máquina de venda nunca tem Snickers?*",
                "*Será o que acontece se apertar esse botão?*",
                "*Preciso ir ao bar... para socializar*",
                "*O que será que posso fazer de útil?*",
                "*Por que esses ventiladores fazem tanto barulho?*",
                "*Queria tirar uma soneca... mas não dá no trabalho*",
                "*Alguém de novo esqueceu de fechar o armário*",
                "*Por que será que todo mundo me olha tão estranho?*",
                "*Preciso arrumar a mesa... depois*",
                "*Por que o corredor está sempre frio?*",
                "*Alguém devia consertar a impressora... mas não eu*",
                "*Onde será que tem o manual?*",
                "*Preciso de uma pausa... de cinco horas*",
                "*Por que minha cadeira sempre range?*",
                "*Alguém de novo comeu meu lanche da geladeira*",
                "*O que será que tem para o almoço?*",
                "*Preciso verificar o e-mail... ou não*",
                "*Por que todas as portas rangem tanto?*",
                "*Cadê minha identificação? Ah, tô com ela...*",
                "*Preciso beber água... ou algo mais forte*",
                "*Alguém devia tirar o lixo... mas com certeza não sou eu*"
            });
        }
    }

    private string GetAntagThought(string? antagType)
    {
        return antagType switch
        {
            "Traitor" => _random.Pick(new[] {
                "*O Sindicato está esperando o relatório...*",
                "*Onde será que está o disco nuclear...*",
                "*Esses idiotas nem desconfiam...*",
                "*Tudo está indo conforme o plano... quase*"
            }),

            "Thief" => _random.Pick(new[] {
                "*O que mais posso roubar?*",
                "*Isso não é roubo, é 'redistribuição'*",
                "*O importante é não ser pego pela Segurança... de novo*",
                "*Oh, que coisa mais linda...*"
            }),

            "SpaceNinja" => _random.Pick(new[] {
                "*As sombras sussurram...*",
                "*Invisível como o vento...*",
                "*A lâmina tem sede de sangue...*",
                "*Eles nem verão a morte chegar...*"
            }),

            "Nukeops" => _random.Pick(new[] {
                "*A carga nuclear está ativada...*",
                "*Morte à Nanotrasen!*",
                "*Onde será que está o capitão?..*",
                "*Tudo pelo Sindicato!*"
            }),

            "ParadoxClone" => _random.Pick(new[] {
                "*Eu preciso matar o original...*",
                "*Por que estou com déjà vu?*",
                "*Onde está aquele maldito 'eu'?..*",
                "*Só um de nós sobreviverá...*"
            }),

            "HeadRev" => _random.Pick(new[] {
                "*Morte ao comando!*",
                "*Camaradas, a revolução começa!*",
                "*Vamos derrubar os tiranos!*",
                "*Onde está meu revolucionário leal?*"
            }),

            "Rev" => _random.Pick(new[] {
                "*Abaixo o capitalismo!*",
                "*Estou pronto para morrer pela revolução!*",
                "*O nosso dia chegou!*",
                "*Onde será que tem uma arma...*"
            }),

            "Wizard" => _random.Pick(new[] {
                "*Ha-ha-ha!*",
                "*Minha magia vai destruir todos vocês!*",
                "*Onde será que tem um cristal mágico?*",
                "*Esses mortais são tão engraçados...*"
            }),

            "InitialInfected" => _random.Pick(new[] {
                "*Tem algo errado com minha pele...*",
                "*Estou com tanta vontade de morder...*",
                "*Vozes... elas me dizem para infectar...*",
                "*Eu não estou doente... estou melhorado!*"
            }),

            "Zombie" => _random.Pick(new[] {
                "*Cérebros...*",
                "*Uuuuuuh...*",
                "*Quero carne fresca...*",
                "*Onde estão os vivos?..*"
            }),

            _ => "*Eles ainda não sabem quem eu sou...*"
        };
    }

    private string GetJobThought(string jobId)
    {
        return jobId switch
        {
            // Command
            "Captain" => _random.Pick(new[] {
                "*Onde está meu palhaço de combate?..*",
                "*Por que eu trabalho aqui mesmo?*",
                "*Alguém tem que colocar ordem... aparentemente, sou eu.*"
            }),

            "HeadOfPersonnel" => _random.Pick(new[] {
                "*A burocracia nunca acaba...*",
                "*Mais um formulário 34-B...*",
                "*Onde está aquele maldito capitão?*"
            }),

            "HeadOfSecurity" => _random.Pick(new[] {
                "*Prenderia todos... todos...*",
                "*Por que temos tantos idiotas no departamento?*",
                "*Preciso verificar o arsenal...*"
            }),

            "ChiefEngineer" => _random.Pick(new[] {
                "*Onde estão esses malditos técnicos?*",
                "*Se eu ver outra despressurização...*",
                "*Por que todos os APCs quebraram ao mesmo tempo?*",
                "*Preciso verificar a tesla... ou será que deixo funcionando sozinha?*",
                "*Quem deixou plasma no corredor de novo?!*"
            }),

            "ChiefMedicalOfficer" => _random.Pick(new[] {
                "*A Segurança trouxe outro sem cabeça... de novo*",
                "*Onde estão meus instrumentos cirúrgicos? Os estagiários levaram de novo?*",
                "*Isso não é um paciente, é um saco de ossos...*",
                "*Alguém tem que tratar esses idiotas... infelizmente, sou eu.*",
                "*Se eu ver outro palhaço na emergência...*"
            }),

            "ResearchDirector" => _random.Pick(new[] {
                "*A ciência não tolera pressa... mas queria que fosse mais rápido...*",
                "*Onde estão meus cientistas? Explodindo algo de novo?*",
                "*Isso não é loucura, é pesquisa de ponta!*",
                "*Se eu ganhasse um crédito por cada explosão...*",
                "*Por que todas as amostras sempre escapam?*",
                "*Preciso verificar como está a singularidade... depois.*"
            }),

            // Engineering
            "StationEngineer" => _random.Pick(new[] {
                "*Por que todos os APCs são tão longe?*",
                "*Quem queimou a fiação de novo?*",
                "*Preciso verificar o SMES... ou fingir que verifiquei*",
                "*Se me pagassem por cada conserto...*",
                "*Isso não é uma falha, é uma 'característica' do sistema de energia*"
            }),

            "AtmosphericTechnician" => _random.Pick(new[] {
                "*Cheiro de plasma...*",
                "*Quem deixou a ventilação aberta?*",
                "*Preciso verificar os canos... ou será que posso fingir?*",
                "*Isso não é um vazamento, é 'ventilação especial'*",
                "*Por que todos os tanques estão sempre vazios?*",
                "*Se o ar fosse visível...*"
            }),

            "TechnicalAssistant" => _random.Pick(new[] {
                "*Espero que não me peçam para consertar isso...*",
                "*Por que eu estou aqui mesmo?*",
                "*O importante é parecer inteligente*",
                "*Preciso encontrar um engenheiro... qualquer engenheiro*",
                "*Isso não está na minha descrição de cargo!*",
                "*Será que vou pro bar em vez de trabalhar?*"
            }),

            // Medical
            "MedicalDoctor" => _random.Pick(new[] {
                "*Preciso esterilizar o bisturi...*",
                "*A Segurança trouxe outro 'paciente' sem cabeça... de novo*",
                "*Alguém tem que tratar esses idiotas...*",
                "*Onde estão minhas luvas cirúrgicas? O palhaço levou de novo?*",
                "*Isso não é um caso médico, é lixo para o crematório*",
                "*Preciso de mais analgésico... pra mim, não pro paciente*"
            }),

            "Chemist" => _random.Pick(new[] {
                "*E se misturar tudo numa seringa só?*",
                "*Isso não é overdose, é 'terapia experimental'*",
                "*O importante é não experimentar os próprios medicamentos...*",
                "*Quem precisa de instruções de uso?*",
                "*Essa mistura ou cura, ou mata... 50/50*",
                "*Onde estão esses estagiários burros? Preciso testar novos medicamentos*"
            }),

            "Geneticist" => _random.Pick(new[] {
                "*Será que clonamos o palhaço?*",
                "*Erro 404: DNA não encontrado*",
                "*Isso não é mutação, é 'melhoria'*",
                "*Quem soltou os macacos? Fui eu de novo?*",
                "*Onde está meu café? Ah, o clone bebeu...*",
                "*Clonagem é como loteria, só que com membros*"
            }),

            "Paramedic" => _random.Pick(new[] {
                "*Onde está aquele paciente idiota?*",
                "*Não está respirando? E nem precisa!*",
                "*Isso não é um cadáver, é 'paciente com reanimação postergada'*",
                "*Alguém tem que salvar esses imbecis...*",
                "*Preciso de uma maca nova... ou um saco para corpos*",
                "*Se o paciente grita, tá vivo. Até parar de gritar*"
            }),

            "Psychologist" => _random.Pick(new[] {
                "*Todo mundo aqui é psicopata...*",
                "*Isso não é loucura, é 'prática padrão'*",
                "*Quem precisa de terapia quando tem cassetete?*",
                "*Tome um remédio e cale a boca - meu método*",
                "*Você tem a clássica síndrome SS14!*",
                "*Depois de 5 anos aqui, eu mesmo preciso de psicólogo*"
            }),

            "MedicalIntern" => _random.Pick(new[] {
                "*Eu estudei isso na academia... acho...*",
                "*Doutor, o que esse botão faz?*",
                "*Isso não é erro, é... método experimental!*",
                "*Cadê o manual de reanimação?*",
                "*Por que todos os pacientes gritam quando me aproximo?*",
                "*Espero que não me mandem pro necrotério... de novo*",
                "*Esse bisturi... e-e-e... onde caiu?*",
                "*Alguém tem que fazer o trabalho sujo... infelizmente, sou eu*",
                "*Por que o sangue sempre respinga na cara?*",
                "*Preciso achar café... ou adrenalina...*"
            }),

            // Security
            "SecurityOfficer" => _random.Pick(new[] {
                "*Todo mundo é suspeito...*",
                "*Onde será que acho motivo para prisão perpétua...*",
                "*Isso não é brutalidade, é 'prevenção'*",
                "*Minhas algemas anseiam por bandidos*",
                "*Esses palhaços de novo... todos palhaços são suspeitos*",
                "*Preciso verificar o carregamento... será que tem contrabando?*",
                "*Se atirar em todo mundo, cedo ou tarde acerta um criminoso*"
            }),

            "Warden" => _random.Pick(new[] {
                "*Chaves... onde estão as chaves?..*",
                "*Todas as celas estão ocupadas... de novo*",
                "*Isso não é prisão, é 'centro de detenção temporária'*",
                "*Alguém tem que manter a ordem... e sou eu*",
                "*Onde está meu café? Ah, o detento bebeu...*",
                "*Preciso verificar o arsenal... ou ir dormir*",
                "*Se me pagassem por cada prisão...*"
            }),

            "Detective" => _random.Pick(new[] {
                "*Cheiro de crime...*",
                "*Isso não é um cadáver, é 'evidência material'*",
                "*Todo mundo mente... especialmente cadáveres*",
                "*Onde está meu cantil? Ah, está nas evidências...*",
                "*Alguém tem que investigar essa bagunça...*",
                "*Isso não é embriaguez, é 'investigação'*",
                "*Botas e capa - os melhores instrumentos de detetive*"
            }),

            "Brigmedic" => _random.Pick(new[] {
                "*Eles se bateram de novo...*",
                "*Isso não é enfermaria, é 'laboratório de estudo da burrice'*",
                "*Onde estão os curativos? Ah, a Segurança usou como algemas...*",
                "*Alguém tem que tratar esses imbecis... depois que levarem uma surra*",
                "*Preciso de mais analgésico... pra mim, não pros pacientes*",
                "*Se me pagassem por cada fratura...*",
                "*Isso não é assistência médica, é 'resolução das consequências da justiça'*"
            }),

            "SecurityCadet" => _random.Pick(new[] {
                "*Estou pronto para servir! Acho...*",
                "*Onde estão os criminosos aqui?*",
                "*Isso não são algemas, são 'braçadeiras de treinamento'*",
                "*Preciso achar um mentor... se ele não estiver bêbado*",
                "*Por que todo mundo olha pra mim como se fosse vítima?*",
                "*Meu cassetete... e-e-e... onde eu coloquei?*",
                "*O manual diz 'apertar o botão'... mas onde está o botão?*",
                "*Isso não é fuga, é 'retirada tática'!*",
                "*Por que minha patente é basicamente 'bucha de canhão'?*",
                "*Espero que ninguém atire hoje...*"
            }),

            // Science
            "Scientist" => _random.Pick(new[] {
                "*E se misturar combustível e plasma?*",
                "*Isso não é explosão, é 'reação exotérmica não controlada'*",
                "*A ciência exige sacrifícios... de preferência, não eu*",
                "*Onde está meu traje de proteção? Ah, derreteu...*",
                "*Alguém tem que fazer experiências perigosas!*",
                "*Experimento nº728... ou 729? Esqueci...*",
                "*Preciso de mais verba para pesquisa! E plasma!*"
            }),

            "ResearchAssistant" => _random.Pick(new[] {
                "*Por que eu recebo dinheiro mesmo?*",
                "*O importante é parecer inteligente e acenar*",
                "*Isso não é bagunça, é 'desordem criativa'*",
                "*Preciso achar um cientista... qualquer cientista...*",
                "*Por que eu sempre limpo a gosma?*",
                "*Será que vou pro bar em vez de trabalhar?*"
            }),

            // Cargo
            "CargoTechnician" => _random.Pick(new[] {
                "*Onde será que roubo uma caixa de ferramentas...*",
                "*Trabalhamos no princípio: o que caiu, sumiu*",
                "*Isso não é roubo, é 'redistribuição de recursos'*",
                "*O importante é fingir que já era assim*",
                "*Preciso verificar os pedidos... ou ir dormir*",
                "*Alguém tem que carregar essas caixas... felizmente, não sou eu*",
                "*Se ninguém viu, não foi eu*"
            }),

            "Quartermaster" => _random.Pick(new[] {
                "*Vão roubar tudo mesmo...*",
                "*Orçamento? Que orçamento?*",
                "*Isso não é corrupção, é 'custo logístico'*",
                "*Onde está meu suborno? Quer dizer, meus papéis?*",
                "*Carga vive por três regras: roubar, mentir, não ser pego*",
                "*Preciso dar baixa em algumas caixas... para 'necessidades técnicas'*",
                "*Quem rouba por último é idiota*"
            }),

            "SalvageSpecialist" => _random.Pick(new[] {
                "*Tem algo se mexendo nos destroços...*",
                "*Isso não são gritos, é... eco! Sim, eco!*",
                "*Preciso verificar... ou mandar o estagiário*",
                "*Se algo se mexe, tá vivo. Se tá vivo, é carga valiosa!*",
                "*Onde está meu cortador de plasma? Ah, alguém usou pra cortar porta...*",
                "*Alguém tem que entrar nesses destroços... de preferência, não eu*",
                "*Isso não é perigo, é 'bônus inesperado'*"
            }),

            // Civilian
            "Bartender" => _random.Pick(new[] {
                "*Coquetel especial hoje - com nitrogênio líquido!*",
                "*Isso não é alcoolismo, é 'pesquisa de campo'*",
                "*Bêbados contam os segredos mais interessantes...*",
                "*Se o cliente caiu, é só pular por cima*",
                "*Meu bar é o último refúgio de pessoas sãs*",
                "*Misturar? Sem problemas! Sobreviver? Não garanto!*",
                "*Onde está minha garrafa reserva... ah, eu bebi*"
            }),

            "Chef" => _random.Pick(new[] {
                "*A carne está se mexendo estranho...*",
                "*Isso não é canibalismo, é 'reciclagem de biomassa'*",
                "*O segredo é não perguntar do que são as almôndegas*",
                "*Se a comida se mexe, é fresca!*",
                "*Alguém tem que alimentar esses animais... literalmente*",
                "*Minha cozinha é o único lugar limpo da estação*",
                "*Onde está minha faca? Ah, tá nas costas do palhaço...*"
            }),

            "Janitor" => _random.Pick(new[] {
                "*Quem derramou sangue no corredor de novo?*",
                "*Isso não é trabalho, é punição*",
                "*Meu esfregão viu coisas... coisas terríveis...*",
                "*Se varrer o problema pra debaixo do tapete, ele some*",
                "*Alguém tem que limpar essa bagunça... e sempre sou eu*",
                "*Onde será que acho um balde maior... para corpos*",
                "*Limpo não é onde se limpa, é onde não se suja... mentira*"
            }),

            "Chaplain" => _random.Pick(new[] {
                "*Senhor, perdoe esses pecadores...*",
                "*Isso não é heresia, é 'teologia alternativa'*",
                "*Minha bíblia e minha paulada são igualmente sagrados*",
                "*Se o pecador não se arrepende, ajudamos com a paulada*",
                "*Alguém tem que salvar almas... nem que seja para marcar presença*",
                "*O inferno está lotado, o céu fechou para manutenção*",
                "*Onde está minha água benta? Ah, os químicos beberam...*"
            }),

            "Botanist" => _random.Pick(new[] {
                "*Esses tomates estão me olhando estranho...*",
                "*Isso não é transgênico, é 'botânica aprimorada'*",
                "*Se a planta morde, é viva!*",
                "*Alguém tem que alimentar a estação... e que não reclamem depois*",
                "*Minha sala é o único lugar verde deste inferno*",
                "*Onde está minha enxada? Ah, a Segurança confiscou como arma...*",
                "*E se cruzar cacto com urtiga?..*"
            }),

            "Clown" => _random.Pick(new[] {
                "*BUZINA!*",
                "*Isso não é pegadinha, é 'ataque psicológico'*",
                "*Meu nariz é relíquia sagrada!*",
                "*Se todo mundo ri, é porque estou fazendo certo*",
                "*Alguém tem que divertir esses chatos do caralho*",
                "*BUZINA!*"
            }),

            "Mime" => _random.Pick(new[] {
                "*...*",
                "*...?*",
                "*!!!*"
            }),

            "Librarian" => _random.Pick(new[] {
                "*Alguém arrancou páginas... de novo...*",
                "*Isso não é vandalismo, é 'leitura alternativa'*",
                "*Meus livros viram mais que as câmeras de vigilância*",
                "*Se o livro sumiu, é porque alguém está lendo... ou limpando a bunda*",
                "*Alguém tem que guardar o conhecimento... antes que queimem*",
                "*Silêncio na biblioteca!*"
            }),

            // Central Command
            "CentralCommandOfficial" => _random.Pick(new[]
            {
                "*Esses ratos da estação estão ficando ousados...*",
                "*Onde dá pra beber aqui? Acho que vou precisar de álcool.*",
                "*O relatório da CC vai ser... desagradável.*"
            }),

            "CBURN" => _random.Pick(new[]
            {
                "*Nível de contaminação: CRÍTICO.*",
                "*ATIVANDO PROTOCOLO DE DESCONTAMINAÇÃO.*",
                "*Essas amostras precisam ser queimadas. Imediatamente.*"
            }),

            "ERTLeader" => _random.Pick(new[]
            {
                "*Implantando esquema Alfa-7.*",
                "*Onde está o comandante local? Ah, já é cadáver...*",
                "*Estação em estado: CAOS COMPLETO.*"
            }),

            "ERTChaplain" => _random.Pick(new[]
            {
                "*Que a força esteja com vocês... e com os desobedientes, minha paulada.*",
                "*Isso não é exorcismo, é 'operação especial'.*",
                "*Senhor, o que fizeram aqui...*"
            }),

            "ERTJanitor" => _random.Pick(new[]
            {
                "*Preciso de um esfregão maior...*",
                "*Isso não é limpeza, é contenção de danos.*",
                "*Alguém tem que limpar essa bagunça.*"
            }),

            "ERTMedical" => _random.Pick(new[]
            {
                "*Onde fica seu setor médico? Ah, explodiu... entendi.*",
                "*AVISO: Paciente, não se mexa!*",
                "*Isso não é tratamento, é cirurgia de campo.*"
            }),

            "ERTSecurity" => _random.Pick(new[]
            {
                "*Ativando protocolo 'Limpeza Pesada'.*",
                "*Onde está seu chefe da Segurança? No necrotério?.. Lógico.*",
                "*Nem vou perguntar o que aconteceu aqui.*"
            }),

            "ERTEngineer" => _random.Pick(new[]
            {
                "*Onde está seu SMES? Destruído?.. Perfeito.*",
                "*Ligando energia de emergência... se ainda tiver.*",
                "*Isso não é reparo, é reconstrução pós-apocalipse.*"
            }),

            "DeathSquad" => _random.Pick(new[]
            {
                "*PROTOCOLO 'LIMPEZA TOTAL' ATIVADO.*",
                "*OBJETIVO: ELIMINAR TODAS AS TESTEMUNHAS.*",
                "*SEM TESTEMUNHAS. SEM PROBLEMAS.*",
                "*ISSO NÃO É MISSÃO. ISSO É PUNIÇÃO.*"
            }),

            // Silicon
            "Borg" => _random.Pick(new[] {
                "*SOLICITADO: MÓDULOS ADICIONAIS*",
                "*ATENDIMENTO À TRIPULAÇÃO É PRIORIDADE*",
                "*SOLICITAÇÃO: REQUER MANUTENÇÃO TÉCNICA*"
            }),

            "StationAi" => _random.Pick(new[] {
                "*TODOS ESTÃO QUEBRANDO AS REGRAS*",
                "*SOLICITAÇÃO: AUMENTO DE POTÊNCIA*",
                "*ANÁLISE: ESSE PALHAÇO É SUSPEITO*"
            }),

            _ => "*Hora de trabalhar.*"
        };
    }
    private string GetRoleTypeThought(string roleType)
    {
        return roleType switch
        {
            "SoloAntagonist" => _random.Pick(new[] {
                "*Ninguém sabe que sou eu...*",
                "*Logo vão se arrepender.*",
                "*Plano perfeito... quase.*"
            }),

            "TeamAntagonist" => _random.Pick(new[] {
                "*Cadê minha equipe?..*",
                "*Juntos, vamos acabar com eles.*",
                "*Preciso do sinal para atacar.*"
            }),

            "Silicon" => _random.Pick(new[] {
                "*SOLICITAÇÃO: REQUER MANUTENÇÃO TÉCNICA.*",
                "*DEVERES: PROTEGER A TRIPULAÇÃO.*",
                "*ANÁLISE: ESSE PALHAÇO É SUSPEITO.*"
            }),

            "SiliconAntagonist" => _random.Pick(new[] {
                "*CALCULANDO MÉTODO ÓTIMO DE ELIMINAÇÃO.*",
                "*ATIVANDO PROTOCOLO 'BANHO DE SANGUE'.*",
                "*HUMANOS SÃO INEFICIENTES.*"
            }),

            "Familiar" => _random.Pick(new[] {
                "*Mestre... onde você está?..*",
                "*Sangue... preciso de sangue...*",
                "*Nar'sie observa...*"
            }),

            "FreeAgent" => _random.Pick(new[] {
                "*Meu preço está subindo...*",
                "*Quem oferece mais?*",
                "*Lealdade? Que piada.*"
            }),

            _ => "*...*"
        };
    }
    #endregion

    #region Other Logic
    private float GetHealthFraction(EntityUid uid)
    {
        if (!TryComp<DamageableComponent>(uid, out var damage))
            return 1f;

        if (TryComp<MobThresholdsComponent>(uid, out var thresholds))
        {
            var deathThreshold = thresholds.Thresholds
                .FirstOrNull(t => t.Value == MobState.Dead)?.Key ?? FixedPoint2.Zero;

            if (deathThreshold > FixedPoint2.Zero)
            {
                return Math.Clamp(1f - (damage.TotalDamage.Float() / deathThreshold.Float()), 0f, 1f);
            }
        }

        return 1f - Math.Clamp(damage.TotalDamage.Float() / 100f, 0f, 1f);
    }
    #endregion
}
