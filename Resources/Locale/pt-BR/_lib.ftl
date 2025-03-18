### Special messages used by internal localizer stuff.

# Used internally by the PRESSURE() function.
zzzz-fmt-pressure = { TOSTRING($divided, "F1") } { $places ->
    [0] kPa
    [1] MPa
    [2] GPa
    [3] TPa
    [4] PBa
    *[5] ???
}

# Used internally by the POWERWATTS() function.
zzzz-fmt-power-watts = { TOSTRING($divided, "F1") } { $places ->
    [0] W
    [1] kW
    [2] MW
    [3] GW
    [4] TW
    *[5] ???
}

# Used internally by the POWERJOULES() function.
# Reminder: 1 joule = 1 watt for 1 second (multiply watts by seconds to get joules).
# Therefore 1 kilowatt-hour is equal to 3,600,000 joules (3.6MJ)
zzzz-fmt-power-joules = { TOSTRING($divided, "F1") } { $places ->
    [0] J
    [1] kJ
    [2] MJ
    [3] GJ
    [4] TJ
    *[5] ???
}

# Usado internamente pela função ARTIGO-UM().
zzzz-artigo-indefinido = { GENDER($ent) ->
    *[male] um
    [female] uma
    [neuter] ume
}

# Usado internamente pela função ARTIGO-O().
zzzz-artigo-definido = { GENDER($ent) ->
    *[male] o
    [female] a
    [neuter] { "" }
}

# Usado internamente pela função PREPOSICAO-DE().
zzzz-preposicao-de = { GENDER($ent) ->
    *[male] do
    [female] da
    [neuter] de
}

# Usado internamente pela função PREPOSICAO-EM().
zzzz-preposicao-em = { GENDER($ent) ->
    *[male] no
    [female] na
    [neuter] em
}

# Usado internamente pela função PRONOME-ELE().
zzzz-pronome-ele = { GENDER($ent) ->
    *[male] ele
    [female] ela
    [neuter] elu
}

# Usado internamente pela função PRONOME-DELE().
zzzz-pronome-dele = { GENDER($ent) ->
    *[male] dele
    [female] dela
    [neuter] delu
}

# Usado internamente pela função MAKEGENDER()
zzzz-genero-terminacao = { GENDER($ent) ->
    *[male] o
    [female] a
    [neuter] e
}

# Used internally by the THE() function.
zzzz-the = { PROPER($ent) ->
   *[false] o { $ent }
     [true] a { $ent }
   }

# Used internally by the SUBJECT() function.
zzzz-subject-pronoun = { GENDER($ent) ->
   *[male] ele
    [female] ela
    [epicene] eles
    [neuter] elo
   }

# Used internally by the OBJECT() function.
zzzz-object-pronoun = { GENDER($ent) ->
   *[male] dele
    [female] dela
    [epicene] deles
    [neuter] delo
   }

# Used internally by the POSS-PRONOUN() function.
zzzz-possessive-pronoun = { GENDER($ent) ->
   *[male] dele
    [female] dela
    [epicene] deles
    [neuter] delos
   }

# Used internally by the POSS-ADJ() function.
zzzz-possessive-adjective = { GENDER($ent) ->
   *[male] seu
    [female] sua
    [epicene] seus
    [neuter] seu
   }

# Used internally by the REFLEXIVE() function.
zzzz-reflexive-pronoun = { GENDER($ent) ->
   *[male] ele mesmo
    [female] ela mesma
    [epicene] eles mesmos
    [neuter] elos mesmos
   }

# Used internally by the CONJUGATE-BE() function.
zzzz-conjugate-be = { GENDER($ent) ->
    [epicene] são
   *[other] é
   }

# Used internally by the CONJUGATE-HAVE() function.
zzzz-conjugate-have = { GENDER($ent) ->
    [epicene] têm
   *[other] tem
   }
