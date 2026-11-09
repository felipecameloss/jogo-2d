# Gera as músicas e os efeitos sonoros de Brasa Viva por síntese (sem samples de terceiros).
#
# Uso (Python 3 com numpy e soundfile):  python tools/sons/gerar.py
# Saída: Assets/Art/Resources/Audio/ (ou a pasta passada como argumento).
#
# Músicas (loops sem emenda: as caudas das notas e o reverb dão a volta para o começo):
#   musica_dia.wav     calma: alaúde em arpejos, flauta doce, ré maior/mixolídio, 80 BPM
#   musica_noite.wav   tensão: ostinato grave, cordas em trêmulo, taikos, ré menor, 96 BPM
#   musica_chefe.wav   suspense: ostinato em semicolcheias no frígio, tambores de guerra, metais e coro, 112 BPM
# Efeitos:
#   sfx_passo_1..4     passo na grama/terra
#   sfx_machado_1..3   machado na madeira
#   sfx_picareta_1..3  picareta na pedra
#   sfx_chave          chaves tilintando e um brilho
import os
import sys

import numpy as np
import soundfile as sf

MUSIC_SR = 32000
SFX_SR = 44100
RNG = np.random.default_rng(7)

NOTE = {'C': -9, 'C#': -8, 'Db': -8, 'D': -7, 'D#': -6, 'Eb': -6, 'E': -5, 'F': -4, 'F#': -3, 'Gb': -3,
        'G': -2, 'G#': -1, 'Ab': -1, 'A': 0, 'A#': 1, 'Bb': 1, 'B': 2}


def hz(name):
    """'F#4' -> frequência."""
    return 440.0 * 2 ** ((NOTE[name[:-1]] + 12 * (int(name[-1]) - 4)) / 12)


def times(seconds, sr):
    return np.arange(int(seconds * sr)) / sr


def norm(x, peak=1.0):
    m = np.max(np.abs(x))
    return x * (peak / m) if m > 0 else x


# ---------------------------------------------------------------- filtros (no domínio da frequência)

def shaped(x, sr, lo=None, hi=None, order=2):
    """Passa-faixa suave (forma de Butterworth aplicada no espectro)."""
    n = len(x)
    X = np.fft.rfft(x)
    f = np.fft.rfftfreq(n, 1 / sr)
    f[0] = 1e-3
    g = np.ones_like(f)
    if lo:
        g /= np.sqrt(1 + (lo / f) ** (2 * order))
    if hi:
        g /= np.sqrt(1 + (f / hi) ** (2 * order))
    return np.fft.irfft(X * g, n)


def noise(n, sr, lo=None, hi=None, rng=RNG):
    return shaped(rng.standard_normal(n), sr, lo, hi)


# ---------------------------------------------------------------- instrumentos

def pluck(f, dur, sr, bright=0.55, decay=2.0, harm=10, pos=0.18):
    """Corda dedilhada (alaúde/harpa) por síntese aditiva: harmônicos agudos morrem antes."""
    ring = min(4.0, dur + 5.0 / decay)
    t = times(ring, sr)
    out = np.zeros_like(t)
    for k in range(1, harm + 1):
        fk = f * k * (1 + 0.0003 * k * k)
        if fk > 0.45 * sr:
            break
        a = abs(np.sin(np.pi * k * pos)) / k ** (1.6 - bright)
        dk = decay * (1 + (k - 1) * (0.9 - 0.6 * bright))
        out += a * np.sin(2 * np.pi * fk * t + RNG.uniform(0, 6.28)) * np.exp(-dk * t)
    out *= 1 - np.exp(-t / 0.0015)
    late = t > dur + 0.35
    out[late] *= np.exp(-(t[late] - dur - 0.35) / 0.12)  # abafa depois da nota
    return norm(out)


def flute(f, dur, sr):
    """Flauta doce: seno com poucos harmônicos, sopro e vibrato que entra devagar."""
    t = times(dur + 0.25, sr)
    depth = 0.005 * np.clip((t - 0.2) / 0.4, 0, 1)
    inst = f * (1 + depth * np.sin(2 * np.pi * 5.0 * t))
    ph = 2 * np.pi * np.cumsum(inst) / sr
    tone = np.sin(ph) + 0.25 * np.sin(2 * ph) + 0.08 * np.sin(3 * ph) + 0.03 * np.sin(4 * ph)
    breath = norm(noise(len(t), sr, f * 1.5, f * 6)) * 0.05
    env = np.clip(t / 0.07, 0, 1) * (0.9 + 0.1 * np.exp(-t / 0.15))
    rel = t > dur
    env[rel] *= np.exp(-(t[rel] - dur) / 0.07)
    return norm((tone + breath) * env)


def strings(freqs, dur, sr, attack=1.0, release=1.5, bright=6, tremolo=0.0, trem_rate=8.0):
    """Cordas em naipe: dente de serra suave, três vozes desafinadas por nota."""
    t = times(dur + release, sr)
    out = np.zeros_like(t)
    for f in freqs:
        for det in (-0.006, 0.0, 0.0055):
            fv = f * (1 + det)
            vib = 1 + 0.003 * np.sin(2 * np.pi * RNG.uniform(4.0, 5.0) * t + RNG.uniform(0, 6.28))
            ph = 2 * np.pi * np.cumsum(fv * vib) / sr
            for k in range(1, bright + 1):
                if fv * k > 0.45 * sr:
                    break
                out += np.sin(k * ph + RNG.uniform(0, 6.28)) / k ** 1.4
    x = np.clip(t / attack, 0, 1)
    env = x * x * (3 - 2 * x)
    rel = t > dur
    env[rel] *= np.exp(-(t[rel] - dur) / (release / 3))
    if tremolo:
        env *= 1 - tremolo * (0.5 + 0.5 * np.sin(2 * np.pi * trem_rate * t))
    return norm(out * env)


def brass(f, dur, sr, attack=0.2, release=0.25, harm=14):
    """Metal grave: fica mais brilhante quanto mais forte toca."""
    t = times(dur + release, sr)
    x = np.clip(t / attack, 0, 1)
    env = x * x * (3 - 2 * x) * (0.85 + 0.15 * np.exp(-t / 0.3))
    rel = t > dur
    env[rel] *= np.exp(-(t[rel] - dur) / (release / 3))
    ph = 2 * np.pi * np.cumsum(f * (1 + 0.002 * np.sin(2 * np.pi * 5 * t))) / sr
    out = np.zeros_like(t)
    for k in range(1, harm + 1):
        if f * k > 0.45 * sr:
            break
        out += env ** (1 + 0.45 * (k - 1)) * np.sin(k * ph) / k
    return norm(out)


AH = ((700, 1.0, 110), (1220, 0.5, 130), (2600, 0.22, 170))  # formantes da vogal "á"


def choir(freqs, dur, sr, attack=1.2, release=1.6):
    """Coro em "á": harmônicos pesados pelas formantes da vogal."""
    t = times(dur + release, sr)
    out = np.zeros_like(t)
    for f in freqs:
        for det in (-0.004, 0.003, 0.0075):
            fv = f * (1 + det)
            ph = 2 * np.pi * np.cumsum(fv * (1 + 0.006 * np.sin(2 * np.pi * RNG.uniform(5, 6) * t))) / sr
            for k in range(1, int(4000 / fv) + 1):
                fk = k * fv
                a = sum(g * np.exp(-((fk - F) / bw) ** 2) for F, g, bw in AH) + 0.03 / k
                out += a * np.sin(k * ph + RNG.uniform(0, 6.28))
    x = np.clip(t / attack, 0, 1)
    env = x * x * (3 - 2 * x)
    rel = t > dur
    env[rel] *= np.exp(-(t[rel] - dur) / (release / 3))
    return norm(out * env)


def bell(f, sr, ring=2.5, partials=((1, 1.0, 1.0), (2.0, 0.5, 1.7), (2.76, 0.35, 2.5), (5.4, 0.2, 4.0), (8.93, 0.1, 6.0))):
    t = times(ring, sr)
    out = sum(a * np.sin(2 * np.pi * f * r * t) * np.exp(-t * d * 2.2 / ring) for r, a, d in partials if f * r < 0.45 * sr)
    return norm(out * (1 - np.exp(-t / 0.001)))


def drum(sr, f0=140, f1=70, decay=0.35, click=0.15, lo=200, hi=900, length=0.8):
    """Tambor: seno com queda de afinação e um estalo de ruído."""
    t = times(length, sr)
    f = f1 + (f0 - f1) * np.exp(-t / 0.045)
    body = np.sin(2 * np.pi * np.cumsum(f) / sr) * np.exp(-t / decay)
    hit = norm(noise(len(t), sr, lo, hi)) * np.exp(-t / 0.014) * click
    return norm((body + hit) * (1 - np.exp(-t / 0.0008)))


def taiko(sr):
    return drum(sr, 96, 46, 0.42, 0.35, 70, 700, 1.4)


def tom(sr, pitch=1.0):
    return drum(sr, 190 * pitch, 115 * pitch, 0.16, 0.25, 150, 1500, 0.5)


def frame_drum(sr):
    return drum(sr, 150, 92, 0.18, 0.25, 250, 1600, 0.6)


def shaker(sr):
    t = times(0.09, sr)
    return norm(noise(len(t), sr, 5000, 11000)) * np.clip(t / 0.008, 0, 1) * np.exp(-t / 0.03)


def riser(seconds, sr):
    """Ruído que sobe de tom e de volume (fim do loop do chefe)."""
    t = times(seconds, sr)
    n = len(t)
    parts = np.array_split(np.arange(n), 16)
    out = np.zeros(n)
    base = RNG.standard_normal(n)
    for i, idx in enumerate(parts):
        c = 300 * (14 ** (i / 15))
        out[idx] = shaped(base, sr, c * 0.7, c * 1.6)[idx]
    return norm(out * (t / seconds) ** 2)


# ---------------------------------------------------------------- mixagem em loop

class Loop:
    """Buffer circular: o que passa do fim volta para o começo, então o loop não tem emenda."""

    def __init__(self, seconds, sr):
        self.sr = sr
        self.n = int(round(seconds * sr))
        self.buf = np.zeros(self.n)

    def add(self, start, sig, gain=1.0):
        s = int(round(start * self.sr)) % self.n
        sig = sig * gain
        while len(sig):
            take = min(len(sig), self.n - s)
            self.buf[s:s + take] += sig[:take]
            sig = sig[take:]
            s = 0


def reverb(x, sr, seconds=2.4, wet=0.3, bright=5000, seed=3):
    """Reverb por convolução circular com resposta de sala sintética (mantém o loop sem emenda)."""
    n = len(x)
    rng = np.random.default_rng(seed)
    t = times(seconds, sr)
    ir = rng.standard_normal(len(t)) * np.exp(-6.9 * t / seconds)
    ir = shaped(ir, sr, 120, bright)
    ir[: int(0.015 * sr)] = 0
    ir /= np.sqrt(np.sum(ir ** 2))
    pad = np.zeros(n)
    m = min(len(ir), n)
    pad[:m] = ir[:m]
    wet_sig = np.fft.irfft(np.fft.rfft(x) * np.fft.rfft(pad), n)
    return x * (1 - wet) + wet_sig * wet


def master(x, peak, drive=1.3):
    x = x - np.mean(x)
    x = norm(x)
    x = np.tanh(drive * x) / np.tanh(drive)
    return norm(x, peak)


# ---------------------------------------------------------------- músicas

def musica_dia():
    sr, beat = MUSIC_SR, 60 / 80
    song = Loop(16 * 4 * beat, sr)
    # (acorde do naipe, notas do arpejo do grave ao agudo, baixo)
    chords = [
        (['D3', 'A3', 'D4', 'F#4'], ['D3', 'A3', 'D4', 'F#4', 'A4'], 'D2'),
        (['C3', 'G3', 'C4', 'E4'], ['C3', 'G3', 'C4', 'E4', 'G4'], 'C2'),
        (['D3', 'A3', 'D4', 'F#4'], ['D3', 'A3', 'D4', 'F#4', 'A4'], 'D2'),
        (['G2', 'D3', 'G3', 'B3'], ['G2', 'D3', 'G3', 'B3', 'D4'], 'G2'),
        (['B2', 'F#3', 'B3', 'D4'], ['B2', 'F#3', 'B3', 'D4', 'F#4'], 'B1'),
        (['C3', 'G3', 'C4', 'E4'], ['C3', 'G3', 'C4', 'E4', 'G4'], 'C2'),
        (['G2', 'D3', 'G3', 'B3'], ['G2', 'D3', 'G3', 'B3', 'D4'], 'G2'),
        (['A2', 'E3', 'A3', 'C#4'], ['A2', 'E3', 'A3', 'C#4', 'E4'], 'A1'),
    ]
    melody = [
        [('A4', 2), ('F#4', 1), ('G4', 1), ('A4', 3), (None, 1)],
        [('G4', 2), ('E4', 1), ('C5', 1), ('B4', 2), ('G4', 2)],
        [('A4', 1), ('B4', 1), ('A4', 1), ('F#4', 1), ('D4', 3), (None, 1)],
        [('G4', 2), ('B4', 1), ('D5', 1), ('C5', 2), ('B4', 2)],
        [('B4', 2), ('D5', 1), ('F#5', 1), ('E5', 2), ('D5', 2)],
        [('E5', 2), ('D5', 1), ('C5', 1), ('G4', 3), (None, 1)],
        [('B4', 1), ('C5', 1), ('D5', 1), ('B4', 1), ('G4', 2), ('A4', 2)],
        [('E5', 2), ('C#5', 1), ('B4', 1), ('A4', 3), (None, 1)],
    ]
    patterns = [[0, 1, 2, 3, 4, 3, 2, 1], [0, 2, 1, 3, 2, 4, 3, 1]]
    for i, (pad, arp, bass) in enumerate(chords):
        t0 = i * 8 * beat
        song.add(t0, strings([hz(n) for n in pad], 8 * beat, sr, attack=1.6, release=2.2, bright=5), 0.09)
        for bar in range(2):
            for k, idx in enumerate(patterns[bar]):
                vel = 1.0 if k == 0 else 0.7 if k % 2 == 0 else 0.55
                song.add(t0 + bar * 4 * beat + k * beat / 2 + RNG.uniform(0, 0.01),
                         pluck(hz(arp[idx]), beat * 0.9, sr, bright=0.55, decay=1.8), 0.15 * vel)
            root = hz(bass)
            song.add(t0 + bar * 4 * beat, pluck(root, 2 * beat, sr, bright=0.3, decay=1.0, harm=6), 0.22)
            song.add(t0 + bar * 4 * beat + 2 * beat, pluck(root * 1.5, 2 * beat, sr, bright=0.3, decay=1.0, harm=6), 0.15)
            song.add(t0 + bar * 4 * beat, frame_drum(sr), 0.13)
            song.add(t0 + bar * 4 * beat + 2 * beat, frame_drum(sr), 0.07)
            if i >= 4:
                for k in range(4):
                    song.add(t0 + bar * 4 * beat + k * beat + beat / 2, shaker(sr), 0.025)
        # melodia: harpa na primeira metade, flauta doce na segunda
        b = 0.0
        for note, length in melody[i]:
            if note:
                if i < 4:
                    song.add(t0 + b * beat, pluck(hz(note), length * beat, sr, bright=0.75, decay=1.6), 0.13)
                else:
                    song.add(t0 + b * beat, flute(hz(note), length * beat * 0.95, sr), 0.15)
            b += length
    return master(reverb(song.buf, sr, 2.6, 0.3), 0.72)


def musica_noite():
    sr, beat = MUSIC_SR, 60 / 96
    song = Loop(16 * 4 * beat, sr)
    chords = [
        (['D3', 'F3', 'A3', 'D4'], 'D2'), (['Bb2', 'F3', 'Bb3', 'D4'], 'Bb1'),
        (['G2', 'D3', 'G3', 'Bb3'], 'G1'), (['A2', 'E3', 'A3', 'C#4'], 'A1'),
        (['D3', 'F3', 'A3', 'D4'], 'D2'), (['Bb2', 'F3', 'Bb3', 'D4'], 'Bb1'),
        (['Eb3', 'G3', 'Bb3', 'Eb4'], 'Eb2'), (['A2', 'E3', 'A3', 'C#4'], 'A1'),
    ]
    # ostinato em colcheias, acentos 3+3+2: fundamental, oitava e uma bordadura no fim do compasso
    shape = [0, 0, 12, 0, 0, 12, 0, 1]
    accent = [1.0, 0.6, 0.7, 1.0, 0.6, 0.7, 0.9, 0.6]
    for i, (pad, bass) in enumerate(chords):
        t0 = i * 8 * beat
        song.add(t0, strings([hz(n) for n in pad], 8 * beat, sr, attack=0.6, release=1.0, bright=7, tremolo=0.55, trem_rate=9), 0.12)
        root = hz(bass)
        song.add(t0, strings([root, root * 2], 8 * beat, sr, attack=1.0, release=1.0, bright=3), 0.07)  # zumbido grave (acima de 50 Hz)
        for bar in range(2):
            bt = t0 + bar * 4 * beat
            for k in range(8):
                semis = shape[k] if not (k == 7 and bar == 1) else -1
                song.add(bt + k * beat / 2, pluck(root * 2 * 2 ** (semis / 12), beat * 0.3, sr, bright=0.25, decay=7.0, harm=8), 0.2 * accent[k])
            song.add(bt, taiko(sr), 0.45)
            song.add(bt + 1.5 * beat, taiko(sr), 0.2)
            song.add(bt + 2 * beat, taiko(sr), 0.35)
            song.add(bt + 3.5 * beat, tom(sr, 0.8), 0.18)
            if bar == 1 and i % 2 == 1:  # virada a cada 4 compassos
                for k, at in enumerate((3.0, 3.25, 3.5, 3.75)):
                    song.add(bt + at * beat, tom(sr, 1.15 - 0.1 * k), 0.16)
        if i % 2 == 1:  # sinos estranhos: semitom que não resolve
            for at, note in ((0, 'A5'), (3, 'Bb5'), (6, 'A5')):
                song.add(t0 + at * beat, bell(hz(note), sr, 3.0), 0.05)
    return master(reverb(song.buf, sr, 2.2, 0.24), 0.82)


def musica_chefe():
    sr, beat = MUSIC_SR, 60 / 112
    song = Loop(16 * 4 * beat, sr)
    chords = [
        (['D4', 'F4', 'A4'], 'D3'), (['Eb4', 'G4', 'Bb4'], 'Eb3'),
        (['D4', 'F4', 'A4'], 'D3'), (['Eb4', 'G4', 'Bb4'], 'Eb3'),
        (['D4', 'F4', 'Bb4'], 'Bb2'), (['C#4', 'E4', 'A4'], 'A2'),
        (['D4', 'G4', 'Bb4'], 'G2'), (['C#4', 'E4', 'Bb4'], 'A2'),
    ]
    for i, (voices, bass) in enumerate(chords):
        t0 = i * 8 * beat
        root = hz(bass)
        song.add(t0, choir([hz(n) for n in voices], 8 * beat, sr, attack=1.0, release=1.2), 0.11)
        for bar in range(2):
            bt = t0 + bar * 4 * beat
            # semicolcheias: fundamental, quinta e, no último tempo, a segunda menor (frígio)
            for b in range(4):
                cell = [0, 0, 7, 0] if b < 3 else [0, 1, 0, 12]
                for k, semis in enumerate(cell):
                    song.add(bt + (b + k / 4) * beat, pluck(root * 2 ** (semis / 12), beat * 0.2, sr, bright=0.3, decay=9.0, harm=8),
                             0.17 * (1.0 if k == 0 else 0.65))
            # tambores de guerra
            for b in range(4):
                song.add(bt + b * beat, taiko(sr), 0.42 if b % 2 == 0 else 0.28)
            if bar == 1:
                for k in range(4):
                    song.add(bt + (3 + k / 4) * beat, tom(sr, 1.2 - 0.1 * k), 0.15)
            # metais: golpe no começo de cada compasso; notas longas na segunda metade
            if i < 4:
                for f in (root / 2, root * 0.75):
                    song.add(bt, brass(f, 1.5 * beat, sr), 0.17)
            else:
                for f in (root / 2, root * 0.75):
                    song.add(bt, brass(f, 3.6 * beat, sr, attack=0.4), 0.17)
        if i >= 4:  # trêmulo agudo dissonante
            song.add(t0, strings([hz('A5'), hz('Bb5')], 8 * beat, sr, attack=0.5, release=0.6, bright=4, tremolo=0.7, trem_rate=12), 0.035)
    song.add(15 * 4 * beat, riser(4 * beat, sr), 0.09)
    return master(reverb(song.buf, sr, 1.8, 0.22), 0.88)


# ---------------------------------------------------------------- efeitos

def passo(seed):
    rng = np.random.default_rng(seed)
    sr = SFX_SR
    t = times(0.16, sr)
    crunch = norm(noise(len(t), sr, rng.uniform(250, 400), rng.uniform(1800, 3200), rng)) * np.exp(-t / 0.028) * (1 - np.exp(-t / 0.002))
    d = int(rng.uniform(0.018, 0.03) * sr)
    crunch[d:] += crunch[:len(t) - d] * 0.45  # ponta do pé, logo depois do calcanhar
    thump = np.sin(2 * np.pi * np.cumsum(85 + 40 * np.exp(-t / 0.02)) / sr) * np.exp(-t / 0.03) * 0.6
    grass = norm(noise(len(t), sr, 4000, 9000, rng)) * np.exp(-t / 0.04) * 0.15
    return norm(crunch + thump + grass, 0.8)


def machado(seed):
    rng = np.random.default_rng(seed)
    sr = SFX_SR
    t = times(0.35, sr)
    crack = norm(noise(len(t), sr, 1500, 7000, rng)) * np.exp(-t / 0.006)
    body = np.zeros_like(t)
    for f, a, d in zip(np.array([175, 330, 620, 1140]) * rng.uniform(0.93, 1.07), (1, 0.7, 0.45, 0.25), (0.07, 0.05, 0.035, 0.02)):
        body += a * np.sin(2 * np.pi * f * t) * np.exp(-t / d)
    thud = np.sin(2 * np.pi * np.cumsum(70 + 50 * np.exp(-t / 0.02)) / sr) * np.exp(-t / 0.05)
    splinter = norm(noise(len(t), sr, 2000, 5000, rng)) * np.exp(-t / 0.05) * 0.12
    splinter = np.concatenate([np.zeros(int(0.005 * sr)), splinter])[:len(t)]
    return norm(0.9 * crack + 0.8 * norm(body) + 0.6 * thud + splinter, 0.85)


def picareta(seed):
    rng = np.random.default_rng(seed)
    sr = SFX_SR
    t = times(0.5, sr)
    click = norm(noise(len(t), sr, 3000, 12000, rng)) * np.exp(-t / 0.004)
    ping = np.zeros_like(t)
    for f, a, d in zip(np.array([1850, 2930, 4150, 5600]) * rng.uniform(0.92, 1.08), (0.6, 0.45, 0.3, 0.2), (0.18, 0.12, 0.08, 0.05)):
        ping += a * np.sin(2 * np.pi * f * t) * np.exp(-t / d)
    stone = norm(noise(len(t), sr, 300, 1500, rng)) * np.exp(-t / 0.04) * 0.7
    thud = np.sin(2 * np.pi * np.cumsum(90 + 60 * np.exp(-t / 0.015)) / sr) * np.exp(-t / 0.04) * 0.5
    gravel = np.zeros_like(t)
    for g in range(10):  # pedrinhas caindo
        at = int(rng.uniform(0.06, 0.35) * sr)
        grain = norm(noise(int(0.015 * sr), sr, 1000, 6000, rng)) * np.exp(-np.arange(int(0.015 * sr)) / (0.004 * sr))
        end = min(len(t), at + len(grain))
        gravel[at:end] += grain[:end - at] * rng.uniform(0.05, 0.15) * (1 - at / len(t))
    return norm(click + 0.5 * norm(ping) + stone + thud + gravel, 0.85)


def chave():
    sr = SFX_SR
    rng = np.random.default_rng(42)
    t = times(1.0, sr)
    out = np.zeros_like(t)
    for at in (0.0, 0.045, 0.085, 0.15):  # chaves tilintando
        s = int((at + rng.uniform(0, 0.01)) * sr)
        tt = t[: len(t) - s]
        tink = sum(a * np.sin(2 * np.pi * f * tt) * np.exp(-tt / d)
                   for f, a, d in zip(np.array([3100, 4650, 6300, 8100]) * rng.uniform(0.95, 1.05), (0.5, 0.35, 0.25, 0.15), (0.09, 0.07, 0.05, 0.03)))
        out[s:] += tink * rng.uniform(0.6, 1.0)
    for at, note in ((0.05, 'E6'), (0.12, 'G#6'), (0.19, 'B6')):  # brilho: arpejo de mi maior
        s = int(at * sr)
        b = bell(hz(note), sr, 0.8)
        end = min(len(t), s + len(b))
        out[s:end] += b[:end - s] * 0.45
    return norm(out * (1 - np.exp(-t / 0.001)), 0.8)


def write(folder, name, data, sr):
    sf.write(os.path.join(folder, name + '.wav'), data.astype(np.float32), sr, subtype='PCM_16')
    print('gerado %s.wav (%.1f s)' % (name, len(data) / sr))


if __name__ == '__main__':
    here = os.path.dirname(os.path.abspath(__file__))
    out = sys.argv[1] if len(sys.argv) > 1 else os.path.join(here, '..', '..', 'Assets', 'Art', 'Resources', 'Audio')
    os.makedirs(out, exist_ok=True)
    for i in range(4):
        write(out, 'sfx_passo_%d' % (i + 1), passo(10 + i), SFX_SR)
    for i in range(3):
        write(out, 'sfx_machado_%d' % (i + 1), machado(20 + i), SFX_SR)
        write(out, 'sfx_picareta_%d' % (i + 1), picareta(30 + i), SFX_SR)
    write(out, 'sfx_chave', chave(), SFX_SR)
    write(out, 'musica_dia', musica_dia(), MUSIC_SR)
    write(out, 'musica_noite', musica_noite(), MUSIC_SR)
    write(out, 'musica_chefe', musica_chefe(), MUSIC_SR)
