paint-success = { CAPITALIZE(ARTIGO-O($target)) } { $target } foi { MAKEGENERO("coberto", $target) } de tinta!
paint-failure = Não dá pra cobrir { ARTIGO-O($target) } { $target } de tinta!
paint-failure-painted = { CAPITALIZE(ARTIGO-O($target)) } { $target } já está { MAKEGENERO("coberto", $target) } de tinta!
paint-empty = { CAPITALIZE(ARTIGO-O($used)) } { $used } está vazio!
paint-removed = Você limpa a tinta!
paint-closed = Você precisa abrir { ARTIGO-O($used) } { $used } antes!
paint-verb = Pintar
paint-remove-verb = Remover tinta
