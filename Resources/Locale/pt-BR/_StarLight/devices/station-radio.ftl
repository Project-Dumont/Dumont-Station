signal-port-name-cd-player = Saída do toca-CDs da rádio
signal-port-description-cd-player = A saída do toca-CDs da rádio. Conecte ao rádio rig. Ele toca junto com o toca-discos.

signal-port-name-radio-mixer = Saída da mesa de mixagem da rádio
signal-port-description-radio-mixer = A saída de controle da mesa de mixagem da rádio. Conecte ao rádio rig que alimenta o servidor que ela deve controlar.

vinyl-player-examine-empty = Está vazio.
vinyl-player-examine-loaded = Há uma cópia de { $vinyl } dentro.
vinyl-player-examine-playing = Está tocando.
vinyl-player-examine-paused = Está pausado.
vinyl-player-verb-play = Tocar
vinyl-player-verb-pause = Pausar
vinyl-player-verb-rewind = Rebobinar
vinyl = vinil
station-radio-server-cancelled-anchor = Esta estação já possui um servidor de rádio!

station-radio-receiver-examine-full-volume = Está tocando no volume máximo.
station-radio-receiver-examine-low-volume = Está tocando.
ui-options-radio-music-volume = Volume da música da rádio:

station-radio-server-examine-recording = O servidor da estação não está gravando.
station-radio-server-examine-not-recording = O servidor da estação está gravando no momento.

station-radio-server-microphone-on-use = O microfone está { $radioState }.

item-slot-component-slot-name-vinyl = vinil
item-slot-component-slot-name-cd = CD

station-radio-mixer-title = Mesa de mixagem da rádio
station-radio-mixer-no-server = Não conectada a um servidor de rádio ligado. Conecte a mesa ao rádio rig.
station-radio-mixer-voice = Voz
station-radio-mixer-master = Mestre
station-radio-mixer-monitor = Monitor
station-radio-mixer-level = {$level}%
station-radio-mixer-empty = sem disco
station-radio-mixer-playlist = Playlist
station-radio-mixer-next = Próxima
station-radio-mixer-eject = Ejetar
station-radio-mixer-tracks = Faixas de { $name }
station-radio-mixer-track = { $number }. { $disc }

signal-port-name-broadcast-recorder = Saída do gravador de rádio
signal-port-description-broadcast-recorder = A conexão do gravador de rádio. Conecte ao rádio rig para gravar e reproduzir a transmissão desse servidor.
item-slot-component-slot-name-broadcast-tape = fita de transmissão

broadcast-recorder-title = Gravador de rádio
broadcast-recorder-no-tape = Sem fita
broadcast-recorder-no-server = Não conectado a um servidor de rádio ligado. Conecte o gravador de rádio ao rádio rig.
broadcast-recorder-mode-stopped = Parado
broadcast-recorder-mode-recording = Gravando
broadcast-recorder-mode-playing = Reproduzindo
broadcast-recorder-record = Gravar
broadcast-recorder-play = Reproduzir
broadcast-recorder-stop = Parar
broadcast-recorder-rewind = Rebobinar
broadcast-recorder-popup-no-tape = Não há fita no gravador de rádio.
broadcast-recorder-popup-no-power = O gravador está sem energia.
broadcast-recorder-popup-no-server = O gravador de rádio não está conectado a um servidor de rádio ligado.
broadcast-recorder-popup-tape-full = A fita está cheia.
broadcast-tape-examine = Tem [color=yellow]{ $recorded }[/color] de [color=yellow]{ $max }[/color] gravado.

wire-name-station-radio-music = MUS
wire-name-station-radio-microphone = MIC
wire-name-station-radio-interference = INT

radio-advert-popup-paid = O anúncio rendeu { $reward } spesos de { $listeners } { $listeners ->
    [one] ouvinte
   *[other] ouvintes
} ({ $quality }% de tempo no ar sem interferência).
radio-advert-popup-repeat = Os ouvintes ouviram este anúncio há pouco tempo, não rende nada.
radio-advert-popup-no-listeners = Ninguém estava ouvindo o anúncio.
radio-advert-popup-too-soon = Os ouvintes estão cansados de anúncios, espere mais { $minutes } { $minutes ->
    [one] minuto
   *[other] minutos
} antes que o próximo pague.
radio-advert-popup-drowned = O anúncio foi abafado pela música ou pela conversa, não rendeu nada.

station-radio-mixer-listeners = Ouvintes: { $count }
station-radio-mixer-interference = Interferência no ar!

station-radio-receiver-title = Rádio da estação
station-radio-receiver-power = Energia
station-radio-receiver-knob = Volume
station-radio-receiver-increase = Aumentar volume
station-radio-receiver-decrease = Diminuir volume

record-crate-full = A caixa de vinil está cheia.
record-crate-examine = Ela contém [color=yellow]{ $count }[/color] de { $capacity } vinis.
cd-rack-full = O rack de CD está cheio.
cd-rack-examine = Ele contém [color=yellow]{ $count }[/color] de { $capacity } CDs.
