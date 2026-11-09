# Gera os sprites de armas, equipamentos, drops, recursos do HUD e o chão,
# no mesmo estilo de tools/sprites-inimigos (sem linha preta: o contorno usa o tom
# escuro do próprio material; luz de cima à esquerda; paleta do baú e do jogador).
#
# Uso (Python 3 com Pillow):  python tools/sprites-itens/gerar.py
# Saída: Assets/Art/... (ou a pasta raiz passada como argumento). Tudo a 24 px por unidade.
#
#   Resources/Weapons/icone_*.png   24x24  ícones das 5 armas (baú e ferraria)
#   Resources/Weapons/virote.png    16x16  projétil da Besta, apontando para a direita
#   Resources/Weapons/faca.png      16x16  projétil das Facas, apontando para a direita
#   Resources/Weapons/machado.png   16x16  projétil do Machado (o jogo gira o sprite)
#   Resources/Weapons/bomba.png     2 quadros de 16x16, pavio piscando, com sombra
#   Resources/Weapons/explosao.png  7 quadros de 48x48
#   Resources/Weapons/golpe_espada.png  5 quadros de 48x48, arco de cima para baixo olhando para a direita
#   Resources/Gear/*.png            24x24  ícones dos 7 equipamentos dos baús
#   Resources/Pickups/*.png         16x16  gema de XP, moeda, cura, ímã e chave
#   Resources/Hud/madeira.png, pedra.png  16x16
#   Sprites/chao.png                48x48  ladrilho contínuo de grama (2 unidades, o tileSize do InfiniteBackground)
import os, sys, math
HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, os.path.join(HERE, '..', 'sprites-inimigos'))
from px import *

# ---------- paleta (do baú, da ferraria e do jogador) ----------
WOOD = [(52, 32, 30), (84, 52, 38), (134, 86, 54), (170, 114, 70), (206, 152, 96)]
LEATHER = [(46, 28, 30), (84, 50, 40), (122, 74, 50), (158, 104, 66), (196, 142, 94)]
STEEL = [(54, 60, 76), (96, 108, 128), (142, 154, 170), (196, 206, 218), (240, 244, 250)]
IRON = [(28, 24, 34), (46, 42, 56), (70, 66, 84), (102, 100, 120), (152, 152, 172)]
GOLD = [(112, 64, 28), (178, 118, 42), (244, 204, 84), (255, 232, 140), (255, 250, 214)]
GREEN = [(34, 54, 44), (52, 90, 58), (84, 134, 70), (128, 178, 90), (190, 218, 130)]
RED = [(70, 20, 32), (126, 36, 44), (184, 58, 52), (226, 102, 72), (250, 164, 122)]
STONE = [(46, 44, 54), (76, 74, 86), (110, 108, 120), (150, 148, 158), (196, 194, 202)]
MEAT = [(70, 30, 26), (122, 56, 36), (170, 88, 48), (210, 130, 68), (240, 182, 116)]
BONE = [(150, 136, 112), (206, 194, 166), (242, 232, 204)]
GEM = [(26, 36, 80), (38, 74, 146), (62, 128, 210), (124, 192, 242), (226, 248, 255)]
CREAM = (242, 232, 204)
FIRE = [(96, 28, 32), (176, 52, 40), (232, 112, 50), (250, 182, 72), (255, 236, 150), (255, 255, 236)]
SMOKE = [(42, 38, 46), (68, 62, 72), (98, 92, 100), (132, 126, 130)]
GRASS = [(36, 54, 40), (44, 66, 44), (52, 77, 48), (62, 89, 53), (78, 106, 60)]


def hsh(x, y, seed=0):
    h = (x * 374761393 + y * 668265263 + seed * 1442695041) & 0xffffffff
    h = (h ^ (h >> 13)) * 1274126177 & 0xffffffff
    return (h ^ (h >> 16)) / 0xffffffff


def dark_of(c):
    """Tom de contorno: o próprio material bem escurecido, puxando para o roxo do jogador."""
    return (int(c[0] * 0.36 + 8), int(c[1] * 0.30 + 4), int(c[2] * 0.36 + 12))


def outline(s):
    """Contorno externo de 1 px (vizinhança 4) com o tom escuro do pixel vizinho mais escuro."""
    add = {}
    for (x, y), col in s.c.items():
        if col[3] < 255:
            continue
        for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
            q = (x + dx, y + dy)
            n = s.c.get(q)
            if (n is None or n[3] < 255) and 0 <= q[0] < s.w and 0 <= q[1] < s.h:
                d = dark_of(col)
                if q not in add or sum(d) < sum(add[q]):
                    add[q] = d
    for (x, y), d in add.items():
        s.put(x, y, d)
    return s


def render(s, fn, cx, cy, ang, scale=1.0):
    """Desenha uma forma definida em coordenadas locais (u ao longo do eixo, v perpendicular)
    girada de `ang` graus (0 = apontando para a direita, 45 = para cima à direita).
    fn(u, v, lit) devolve uma cor ou None; lit(v) diz se aquele lado recebe a luz."""
    a = math.radians(ang)
    dx, dy = math.cos(a), -math.sin(a)
    nx, ny = math.sin(a), math.cos(a)
    lv = -0.6 * nx - 0.8 * ny          # luz vem de cima à esquerda
    lit = (lambda v: v < 0) if lv < 0 else (lambda v: v > 0)
    for y in range(s.h):
        for x in range(s.w):
            px_, py_ = x + 0.5 - cx, y + 0.5 - cy
            u = (px_ * dx + py_ * dy) / scale
            v = (px_ * nx + py_ * ny) / scale
            c = fn(u, v, lit)
            if c:
                s.put(x, y, c)
    return s


def seg_dist(px_, py_, a, b):
    ax, ay = a; bx, by = b
    vx, vy = bx - ax, by - ay
    t = max(0, min(1, ((px_ - ax) * vx + (py_ - ay) * vy) / (vx * vx + vy * vy)))
    return math.hypot(px_ - ax - vx * t, py_ - ay - vy * t)


def line(s, a, b, col, w=0.5):
    for y in range(s.h):
        for x in range(s.w):
            if seg_dist(x + 0.5, y + 0.5, a, b) <= w:
                s.put(x, y, col)


def sparkle(s, x, y, big=True):
    s.put(x, y, GOLD[4])
    for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
        s.put(x + dx, y + dy, GOLD[3])
    if big:
        for dx, dy in ((2, 0), (-2, 0), (0, 2), (0, -2)):
            s.put(x + dx, y + dy, GOLD[2])


# =========================== ARMAS (formas locais) ===========================
# Todas apontam para +u; desenhadas a 45° viram ícone, a 0° viram projétil.

def f_espada(u, v, lit):
    av = abs(v)
    if -11.6 <= u < -9.4 and av < 1.6:
        return GOLD[3] if lit(v) else GOLD[1]
    if -9.4 <= u < -5.2 and av < 1.05:
        return LEATHER[3] if (int(math.floor(u)) % 2 and lit(v)) else LEATHER[2] if int(math.floor(u)) % 2 else LEATHER[1]
    if -5.2 <= u < -3.6 and av < 4.8:
        if av > 3.8:
            return GOLD[1]
        return GOLD[3] if lit(v) or u < -4.4 else GOLD[2]
    if -3.6 <= u < 11:
        hw = 1.75 if u < 8.2 else 1.75 * (11 - u) / 2.8
        if av < hw:
            if av < 0.5 and u < 7:
                return STEEL[2]
            return STEEL[4] if lit(v) else STEEL[3]
    return None


def f_besta(u, v, lit):
    av = abs(v)
    # ponta do virote
    if 7.6 <= u < 10.8 and av < (10.8 - u) * 0.6:
        return STEEL[4] if lit(v) else STEEL[3]
    # arco de aço, curvado para a frente
    pu = 4.4 - 0.065 * v * v
    if av < 9.6 and abs(u - pu) < 1.05:
        if av > 8.4:
            return IRON[1]
        return STEEL[4] if (lit(v) and u < pu) else STEEL[2]
    # corda puxada até a noz
    end = (4.4 - 0.065 * 9.0 * 9.0, 9.0)
    if seg_dist(u, v, end, (-4.2, 0)) < 0.5 or seg_dist(u, v, (end[0], -end[1]), (-4.2, 0)) < 0.5:
        return CREAM
    # virote: penas e haste
    if -4.6 <= u < -2.4 and av < 1.5:
        return RED[3] if lit(v) else RED[2]
    if -4.6 <= u < 7.6 and av < 0.55:
        return WOOD[4]
    # coronha
    if -11.2 <= u < 6 and (av < 1.2 or (u < -7.5 and av < 2.1)):
        return WOOD[3] if lit(v) else WOOD[2]
    # gatilho
    if -6.4 <= u < -4.8 and 1.5 <= v < 3.6:
        return IRON[3]
    return None


def f_faca(u, v, lit):
    av = abs(v)
    d = math.hypot(u + 10.3, v)
    if 1.0 < d < 2.2:
        return STEEL[3] if lit(v) else STEEL[2]
    if -8.6 <= u < -2.6 and av < 1.15:
        if abs(u + 5.6) < 0.6:
            return LEATHER[4]
        return WOOD[2] if lit(v) else WOOD[1]
    if -2.6 <= u < -1.4 and av < 2.4:
        return STEEL[3]
    if -1.4 <= u < 10.6:
        spine = -1.35 + max(0.0, u - 5.5) * 0.26
        edge = 2.05 - max(0.0, u - 2.5) * 0.25
        if spine <= v <= edge:
            if v < spine + 0.9:
                return STEEL[4]
            if v > edge - 0.9:
                return STEEL[3]
            return STEEL[2]
    return None


def f_machado(u, v, lit):
    av = abs(v)
    # lâmina (barba) do lado +v, perto do topo do cabo
    if 0.2 <= v <= 8.6:
        h = 2.0 + (v / 8.6) ** 2 * 4.2
        if abs(u - 6) < h:
            if v > 7.3:
                return STEEL[4]
            if v > 6.1:
                return STEEL[3]
            return IRON[3] if u > 6 else IRON[2]
    # contrapeso
    if -2.6 <= v < 0.2 and abs(u - 6) < 1.5:
        return IRON[3]
    # cabo com empunhadura de couro
    if -11 <= u < 9.4 and av < 1.05:
        if u < -5.5:
            return LEATHER[3] if int(math.floor(u)) % 2 == 0 else LEATHER[2]
        return WOOD[3] if lit(v) else WOOD[2]
    return None


def f_virote(u, v, lit):
    av = abs(v)
    if 3.6 <= u < 7 and av < (7 - u) * 0.62:
        return STEEL[4] if lit(v) else STEEL[3]
    if -6.6 <= u < -3.4 and av < 1.6 and av > 0.4 - (u + 3.4) * 0.3:
        return RED[3] if lit(v) else RED[2]
    if -6.6 <= u < 3.6 and av < 0.55:
        return WOOD[4] if lit(v) else WOOD[3]
    return None


def icon_arma(fn):
    s = Spr(24, 24)
    render(s, fn, 12, 12, 45)
    return outline(s)


def icone_bombas():
    s = Spr(24, 24)
    cx, cy, r = 11, 14, 7.6
    body = ellipse(cx, cy, r, r)
    lvl = sphere_level(cx, cy, r, r, [-0.2, 0.3, 0.7])
    for (x, y) in body:
        s.put(x, y, IRON[1 + lvl(x, y)])
    s.put(int(cx - 3), int(cy - 4), IRON[4]); s.put(int(cx - 4), int(cy - 3), IRON[4])
    # gargalo
    for (x, y) in rect(14, 5, 17, 7):
        s.put(x, y, STEEL[3] if x < 16 else STEEL[2])
    # pavio
    for p in [(17, 4), (18, 3), (19, 3)]:
        s.put(*p, LEATHER[4])
    sparkle(s, 20, 2, big=False)
    s.put(20, 2, (255, 255, 255))
    return outline(s)


# =========================== PROJÉTEIS E EFEITOS ===========================

def virote():
    s = Spr(16, 16)
    render(s, f_virote, 8, 8, 0)
    return outline(s)


def faca():
    s = Spr(16, 16)
    render(s, f_faca, 8, 8, 0, 0.68)
    return outline(s)


def machado():
    s = Spr(16, 16)
    # Cabo curto e centro de massa perto da cabeça, para girar bem; a 45° como o ícone.
    k, u0, v0 = 0.78, 2.5, 2.5
    c = math.sqrt(0.5) * k
    render(s, lambda u, v, lit: f_machado(u, v, lit) if u > -6.5 else None,
           8 - c * (u0 + v0), 8 - c * (v0 - u0), 45, k)
    return outline(s)


def bomba():
    frames = []
    for i in range(2):
        s = Spr(16, 16)
        shadow(s, 7.5, 14, 5, 1.5)
        cx, cy, r = 7.5, 9.5, 4.6
        body = ellipse(cx, cy, r, r)
        lvl = sphere_level(cx, cy, r, r, [-0.2, 0.3, 0.7])
        for (x, y) in body:
            s.put(x, y, IRON[1 + lvl(x, y)])
        s.put(5, 7, IRON[4])
        s.put(9, 4, STEEL[3]); s.put(10, 4, STEEL[2])
        s.put(10, 3, LEATHER[4]); s.put(11, 2, LEATHER[4])
        outline(s)
        if i == 0:
            sparkle(s, 12, 1, big=False); s.put(12, 1, (255, 255, 255))
        else:
            s.put(12, 1, GOLD[3]); s.put(13, 1, FIRE[2])
        frames.append(s)
    return frames


def puff_cloud(s, cx, cy, R, ramp, alpha, seed, holes=0.0):
    """Nuvem de bolhas sombreadas, como a fumaça desenhada à mão."""
    puffs = [(cx, cy, R * 0.62)]
    n = 8
    for k in range(n):
        if hsh(k, seed, 3) < holes:
            continue
        a = k * 2 * math.pi / n + hsh(k, seed) * 0.6
        d = R * (0.5 + 0.12 * hsh(k, seed, 1))
        puffs.append((cx + math.cos(a) * d, cy + math.sin(a) * d * 0.9, R * (0.36 + 0.14 * hsh(k, seed, 2))))
    owner = {}
    for i, (px_, py_, pr) in enumerate(puffs):
        for q in ellipse(px_, py_, pr, pr):
            owner[q] = i
    for (x, y), i in owner.items():
        px_, py_, pr = puffs[i]
        lv = sphere_level(px_, py_, pr, pr, [-0.1, 0.35, 0.75])(x, y)
        g = math.hypot(x + 0.5 - cx, y + 0.5 - cy) / R   # mais quente no centro
        k = lv + (1 if g < 0.45 else 0)
        k = max(0, min(len(ramp) - 1, k))
        col = ramp[k]
        s.put(x, y, col + (alpha,))


def explosao():
    frames = []
    # (raio, rampa, alpha, buracos)
    steps = [
        (5, FIRE[2:], 255, 0.0),
        (10, FIRE[1:], 255, 0.0),
        (15, FIRE[1:], 255, 0.0),
        (18, FIRE[0:4], 255, 0.1),
        (19, [SMOKE[0], SMOKE[1], FIRE[1], FIRE[2]], 240, 0.2),
        (20, SMOKE, 190, 0.35),
        (21, SMOKE[:3], 110, 0.55),
    ]
    for i, (R, ramp, alpha, holes) in enumerate(steps):
        s = Spr(48, 48)
        puff_cloud(s, 24, 24, R, ramp, alpha, 7 + i * 0 + (i >= 4) * 5, holes)
        if i <= 1:
            for (x, y) in ellipse(23, 23, R * 0.35, R * 0.35):
                s.put(x, y, FIRE[5])
        if 2 <= i <= 5:   # fagulhas voando
            for k in range(10):
                a = hsh(k, 11) * 2 * math.pi
                d = R + 2 + (i - 2) * 2 + hsh(k, 12) * 3
                x, y = int(24 + math.cos(a) * d), int(24 + math.sin(a) * d)
                s.put(x, y, FIRE[4] if i < 4 else FIRE[2])
        frames.append(s)
    return frames


def golpe_espada():
    frames = []
    cx, cy, R = 12, 24, 21
    # (ângulo da frente, ângulo da cauda, opacidade)
    steps = [(-45, -80, 1.0), (5, -78, 1.0), (52, -62, 1.0), (78, -10, 0.7), (82, 45, 0.35)]
    for head, tail, op in steps:
        s = Spr(48, 48)
        for y in range(48):
            for x in range(48):
                dx, dy = x + 0.5 - cx, y + 0.5 - cy
                r = math.hypot(dx, dy)
                ang = math.degrees(math.atan2(dy, dx))
                if not (tail <= ang <= head) or dx < 0:
                    continue
                t = (ang - tail) / max(1, head - tail)  # 0 cauda, 1 frente
                w = 2.0 + 8.0 * t
                if R - w <= r <= R:
                    if r > R - 1.3:
                        col, a = (255, 255, 255), 255
                    elif r > R - 3.2:
                        col, a = (214, 234, 250), 240
                    else:
                        col, a = (156, 196, 232), 190
                    a = int(a * op * (0.45 + 0.55 * t))
                    if a > 10:
                        s.put(x, y, col + (a,))
        frames.append(s)
    return frames


# =========================== EQUIPAMENTOS ===========================

def amuleto_carvalho():
    s = Spr(24, 24)
    # cordão
    for (x, y) in ellipse(12, 9, 6.2, 7):
        if (x, y) not in ellipse(12, 9, 5.0, 5.8) and y <= 11:
            s.put(x, y, LEATHER[3] if x < 12 else LEATHER[2])
    for (x, y) in rect(11, 8, 12, 9):
        s.put(x, y, GOLD[3] if x == 11 else GOLD[2])
    # disco de madeira
    cx, cy, r = 12, 16, 6.6
    lvl = sphere_level(cx, cy, r, r, [-0.1, 0.4])
    for (x, y) in ellipse(cx, cy, r, r):
        d = math.hypot(x + 0.5 - cx, y + 0.5 - cy)
        if d > r - 1.2:
            s.put(x, y, GOLD[2] if (x + y) < 26 else GOLD[1])
        else:
            s.put(x, y, WOOD[2 + lvl(x, y)] if (y % 3) else WOOD[2 + max(0, lvl(x, y) - 1)])
    # folha de carvalho entalhada (verde), em diagonal, com lóbulos
    leaf = render(Spr(24, 24), lambda u, v, lit: (
        (GREEN[4] if v < -0.3 else GREEN[3]) if abs(v) < 2.0 * math.sqrt(max(0, 1 - (u / 4.2) ** 2)) + 0.5 * math.cos(u * 2.4) and abs(u) < 4.2 else None), cx, cy, 45)
    for q, c in leaf.c.items():
        if q in ellipse(cx, cy, r - 1.4, r - 1.4):
            s.c[q] = c
    render(s, lambda u, v, lit: GREEN[1] if abs(v) < 0.45 and -4 < u < 3.5 else None, cx, cy, 45)
    s.put(int(cx - 3), int(cy - 3), WOOD[4])
    return outline(s)


def pedra_amolar():
    s = Spr(24, 24)

    def f(u, v, lit):
        if (abs(u) / 9.6) ** 4 + (abs(v) / 3.7) ** 4 > 1:
            return None
        sv = v if lit(-1) else -v
        k = 3
        if sv < -2.2:
            k = 4
        elif sv > 2.0:
            k = 1
        elif sv > 0.6:
            k = 2
        if hsh(int(u * 2), int(v * 2), 5) > 0.9:
            k = max(1, k - 1)
        return STONE[k] if True else None
    render(s, f, 11, 13, 30)
    outline(s)
    sparkle(s, 19, 4)
    return s


def cota_malha():
    s = Spr(24, 24)
    m = poly([(7.5, 2.5), (16.5, 2.5), (21, 4.5), (23.4, 12.5), (19.4, 13.6), (18.2, 9.5),
              (18.4, 21.5), (5.6, 21.5), (5.8, 9.5), (4.6, 13.6), (0.6, 12.5), (3, 4.5)])
    m = {q for q in m if not (q[1] < 6 and abs(q[0] + 0.5 - 12) < (6 - q[1]) * 0.9 + 1.2)}  # gola em V
    for (x, y) in m:
        off = 2 if (y // 2) % 2 else 0
        k = 2
        if y % 2 == 0 and (x + off) % 4 == 0:
            k = 4
        elif y % 2 == 0 and (x + off) % 4 == 1:
            k = 3
        elif y % 2 == 1 and (x + off) % 4 == 2:
            k = 1
        if x > 15 and k > 1:
            k -= 1
        s.put(x, y, STEEL[k])
    # gola e cinto de couro
    for (x, y) in m:
        if any((x + dx, y + dy) not in m and (y + dy) < 7 for dx, dy in ((0, -1), (1, -1), (-1, -1))) and y < 7:
            s.put(x, y, LEATHER[3])
        if y in (15, 16):
            s.put(x, y, LEATHER[3] if y == 15 else LEATHER[2])
    for (x, y) in rect(11, 15, 12, 16):
        s.put(x, y, GOLD[3] if (x, y) != (12, 16) else GOLD[1])
    return outline(s)


def luvas_couro():
    s = Spr(24, 24)

    def part(m, base=2):
        x0, y0, x1, y1 = bbox(m)
        for (x, y) in m:
            k = base
            if x == x0 or y == y0:
                k = base + 1
            if x == x1:
                k = base - 1
            s.put(x, y, LEATHER[k])
    part(rrect(6, 9, 17, 17, 1), 2)
    for fx, top in ((7, 4), (10, 3), (13, 3), (16, 5)):
        part(rrect(fx, top, fx + 1, 10, 1) | rect(fx, top + 1, fx + 1, 10), 3 if fx < 13 else 2)
    thumb = render(Spr(24, 24), lambda u, v, lit: LEATHER[3] if -2 < u < 3.6 and abs(v) < 1.3 else None, 4.6, 11.5, 130)
    s.c.update(thumb.c)
    # punho
    for (x, y) in rect(5, 17, 18, 21):
        s.put(x, y, LEATHER[1] if y > 17 else LEATHER[2])
    for x in range(6, 18, 2):
        s.put(x, 19, LEATHER[4])
    # nós dos dedos
    for x in (8, 11, 14):
        s.put(x, 11, LEATHER[1])
    return outline(s)


def alforje():
    s = Spr(24, 24)
    # alça
    for (x, y) in ellipse(12, 10, 7.5, 8):
        if (x, y) not in ellipse(12, 10, 6.2, 6.8) and y <= 9:
            s.put(x, y, LEATHER[2] if x > 12 else LEATHER[3])
    body = rrect(4, 9, 19, 21, 2)
    for (x, y) in body:
        s.put(x, y, LEATHER[1] if x >= 18 or y >= 20 else LEATHER[2])
    flap = {q for q in rect(4, 8, 19, 15) if q[1] < 12 or ((q[0] + 0.5 - 12) / 8) ** 2 + ((q[1] + 0.5 - 12) / 3.6) ** 2 <= 1}
    flap = {q for q in flap if q not in {(4, 8), (19, 8)}}
    for (x, y) in flap:
        k = 3
        if y == 8 or x == 4:
            k = 4
        if x == 19:
            k = 2
        s.put(x, y, LEATHER[k])
    # costura do lado de baixo da aba
    for (x, y) in flap:
        if (x, y + 2) not in flap and (x, y + 1) in flap and x % 2 == 0 and 5 < x < 18:
            s.put(x, y, CREAM)
    # fivela
    for (x, y) in rect(10, 13, 13, 16):
        s.put(x, y, GOLD[3] if x < 12 else GOLD[2])
    s.put(11, 14, LEATHER[0]); s.put(12, 14, LEATHER[0])
    for y in range(17, 21):
        s.put(11, y, LEATHER[3])
    return outline(s)


def ervas_curandeiro():
    s = Spr(24, 24)
    base = (12, 21.5)
    tie = 15.5
    tips = [(4.5, 6), (8, 3), (12.5, 2), (16.5, 3.5), (20, 7.5)]
    for i, t in enumerate(tips):
        line(s, base, t, GREEN[1], 0.5)
    for i, t in enumerate(tips):
        for j, k in enumerate((0.45, 0.62, 0.8, 0.95)):
            x = base[0] + (t[0] - base[0]) * k
            y = base[1] + (t[1] - base[1]) * k
            side = 1 if (i + j) % 2 else -1
            lx, ly = x + side * 1.4, y + 0.3
            for (px_, py_) in ellipse(lx, ly, 1.5, 1.0):
                s.put(px_, py_, GREEN[3] if side < 0 else GREEN[2])
            s.put(int(lx - 0.6), int(ly - 0.5), GREEN[4])
    for t, col in ((tips[1], (236, 226, 250)), (tips[3], (236, 226, 250)), (tips[2], (196, 164, 230))):
        x, y = int(t[0]), int(t[1])
        for dx, dy in ((0, 0), (1, 0), (0, -1), (-1, 0), (0, 1)):
            s.put(x + dx, y + dy, col if (dx, dy) != (0, 0) else GOLD[3])
    # amarração de barbante
    for x in range(9, 16):
        for y in (int(tie), int(tie) + 1):
            if any(abs(px_ - x) <= 1 for px_ in [base[0] + (tt[0] - base[0]) * (base[1] - y) / (base[1] - tt[1]) for tt in tips]):
                s.put(x, y, CREAM if y == int(tie) else BONE[1])
    return outline(s)


def moeda_fortuna():
    s = Spr(24, 24)
    cx, cy, r = 11.5, 12.5, 9.2
    lvl = sphere_level(cx, cy, r, r, [-0.3, 0.35])
    for (x, y) in ellipse(cx, cy, r, r):
        d = math.hypot(x + 0.5 - cx, y + 0.5 - cy)
        if d > r - 1.3:
            s.put(x, y, GOLD[3] if (x + y) < 22 else GOLD[1])
        elif d > r - 2.2:
            s.put(x, y, GOLD[1] if (x + y) < 22 else GOLD[3])
        else:
            s.put(x, y, GOLD[1 + lvl(x, y)])
    # trevo de quatro folhas
    for ox, oy in ((0, -2.4), (2.4, 0), (0, 2.4), (-2.4, 0)):
        for (x, y) in ellipse(cx + ox, cy - 0.5 + oy, 2.0, 2.0):
            s.put(x, y, GREEN[3] if (x + y) < cx + cy - 1 else GREEN[2])
    line(s, (cx + 0.5, cy + 1), (cx + 3, cy + 5), GREEN[1], 0.5)
    s.put(int(cx - 3), int(cy - 3), GREEN[4])
    outline(s)
    sparkle(s, 19, 4)
    return s


# =========================== DROPS (16x16) ===========================

def gema_xp():
    s = Spr(16, 16)
    top, right, bot, left, mid = (8, 2), (12.5, 7), (8, 14.5), (3.5, 7), (8, 7)
    m = poly([top, right, bot, left])
    for (x, y) in m:
        px_ = x + 0.5
        if y < 7:
            k = 4 if px_ < 8 else 3
        else:
            k = 2 if px_ < 8 else 1
        s.put(x, y, GEM[k])
    for p in [(6, 5), (7, 4)]:
        s.put(*p, GEM[4])
    s.put(6, 4, (255, 255, 255))
    return outline(s)


def moeda():
    s = Spr(16, 16)
    cx, cy = 8, 8
    for (x, y) in ellipse(cx, cy, 5.2, 5.6):
        d = ((x + 0.5 - cx) / 5.2) ** 2 + ((y + 0.5 - cy) / 5.6) ** 2
        if d > 0.62:
            s.put(x, y, GOLD[3] if (x + y) < 15 else GOLD[1])
        else:
            s.put(x, y, GOLD[2])
    for y in range(6, 11):
        s.put(8, y, GOLD[1])
    s.put(7, 6, GOLD[3]); s.put(7, 10, GOLD[3])
    s.put(5, 5, GOLD[4])
    return outline(s)


def cura():
    s = Spr(16, 16)
    # osso
    line(s, (6.5, 9.5), (3.2, 12.8), BONE[1], 0.75)
    for (x, y) in ellipse(2.6, 12.3, 1.2, 1.2) | ellipse(3.7, 13.6, 1.2, 1.2):
        s.put(x, y, BONE[2] if x < 3 else BONE[1])
    # carne assada
    cx, cy = 9.2, 6.8
    m = render(Spr(16, 16), lambda u, v, lit: (0, 0, 0) if (u / 5.6) ** 2 + (v / 4.1) ** 2 <= 1 else None, cx, cy, -38)
    lvl = sphere_level(cx, cy, 5, 4.5, [-0.2, 0.3, 0.7])
    for q in m.c:
        s.put(q[0], q[1], MEAT[1 + lvl(*q)])
    s.put(7, 4, MEAT[4]); s.put(8, 4, MEAT[4]); s.put(7, 5, MEAT[4])
    s.put(11, 6, MEAT[1]); s.put(10, 9, MEAT[1])
    return outline(s)


def ima():
    s = Spr(16, 16)
    cx, cy = 8, 7
    ring = {q for q in ellipse(cx, cy, 6, 6) if q not in ellipse(cx, cy, 2.6, 2.6) and q[1] < cy}
    legs = rect(2, 7, 4, 13) | rect(11, 7, 13, 13)
    m = ring | legs
    for (x, y) in m:
        if y >= 11:
            s.put(x, y, STEEL[4] if x in (2, 11) else STEEL[3] if x in (3, 12) else STEEL[2])
        else:
            k = 3 if x in (2, 11) or (y <= 2) else 2
            if x in (4, 13) or (x > 9 and y < 5 and (x + 1, y) not in m):
                k = 1
            s.put(x, y, RED[k])
    s.put(4, 3, RED[4])
    return outline(s)


def chave():
    s = Spr(16, 16)
    bow = {q for q in ellipse(4.5, 8, 3.4, 3.4) if q not in ellipse(4.5, 8, 1.5, 1.5)}
    shaft = rect(7, 7, 14, 8)
    teeth = rect(11, 9, 12, 10) | rect(13, 9, 14, 11)
    m = bow | shaft | teeth
    for (x, y) in m:
        k = 2
        if (x, y - 1) not in m or (x - 1, y) not in m:
            k = 3
        if (x, y + 1) not in m and y > 7:
            k = 1
        s.put(x, y, GOLD[k])
    s.put(3, 6, GOLD[4])
    return outline(s)


# =========================== HUD (16x16) ===========================

def madeira():
    s = Spr(16, 16)

    def log(x0, y0, x1, y1):
        for (x, y) in rect(x0, y0, x1, y1):
            k = 2
            if y == y0:
                k = 3
            if y == y1:
                k = 1
            if (x + y * 3) % 5 == 0 and y0 < y < y1:
                k = 1
            s.put(x, y, WOOD[k])
        cy = (y0 + y1) / 2 + 0.5
        for (x, y) in ellipse(x1 + 0.5, cy, 2.0, (y1 - y0 + 1) / 2):
            d = math.hypot(x + 0.5 - x1 - 0.5, (y + 0.5 - cy) * 1.0)
            s.put(x, y, WOOD[4] if d > 1.2 or (x, y) == (x1, int(cy)) else WOOD[3])
        s.put(x1, int(cy) - 0, WOOD[2])
    log(1, 8, 11, 13)
    log(3, 3, 12, 8)
    return outline(s)


def pedra():
    s = Spr(16, 16)
    m = poly([(2, 12.5), (3.5, 6), (7, 3), (11.5, 4), (14, 8.5), (13, 13.5), (6, 14)])
    for (x, y) in m:
        k = 2
        if x + y < 11 or (x - 1, y) not in m or (x, y - 1) not in m:
            k = 4 if x + y < 11 else 3
        elif x > 10 and y > 8 or (x, y + 1) not in m or (x + 1, y) not in m:
            k = 1
        s.put(x, y, STONE[k])
    for p in [(8, 7), (9, 8), (9, 9), (10, 10)]:   # fenda
        s.put(*p, STONE[1])
    s.put(5, 6, STONE[4])
    return outline(s)


# =========================== CHÃO (48x48, contínuo) ===========================

def chao():
    N = 48
    s = Spr(N, N)
    G = 6  # ruído de valor com grade 6x6, repetindo nas bordas

    def noise(x, y, seed):
        gx, gy = x / N * G, y / N * G
        x0, y0 = int(gx), int(gy)
        fx, fy = gx - x0, gy - y0
        fx, fy = fx * fx * (3 - 2 * fx), fy * fy * (3 - 2 * fy)
        def g(i, j):
            return hsh(i % G, j % G, seed)
        a = g(x0, y0) + (g(x0 + 1, y0) - g(x0, y0)) * fx
        b = g(x0, y0 + 1) + (g(x0 + 1, y0 + 1) - g(x0, y0 + 1)) * fx
        return a + (b - a) * fy
    for y in range(N):
        for x in range(N):
            n = noise(x, y, 1) * 0.75 + hsh(x, y, 9) * 0.25
            k = 1 if n < 0.36 else 2 if n < 0.62 else 3
            s.put(x, y, GRASS[k])
    def wput(x, y, c):
        s.put(x % N, y % N, c)
    # tufos de capim
    tufts = [(5, 6), (20, 3), (40, 9), (13, 19), (29, 15), (44, 25), (7, 33), (22, 29), (15, 42), (38, 45), (27, 40), (2, 22)]
    for i, (x, y) in enumerate(tufts):
        hi = GRASS[4] if i % 3 else GRASS[3]
        wput(x, y, hi); wput(x + 2, y, hi); wput(x + 1, y + 1, hi); wput(x - 1, y + 1, GRASS[3])
        wput(x, y + 2, GRASS[0]); wput(x + 1, y + 2, GRASS[0])
    # florzinhas e pedrinhas
    for (x, y, c) in [(33, 6, (196, 186, 116)), (10, 30, (176, 160, 196))]:
        wput(x, y, c); wput(x, y + 1, GRASS[0])
    for (x, y) in [(36, 28)]:
        wput(x, y, STONE[3]); wput(x + 1, y, STONE[2]); wput(x, y + 1, STONE[1]); wput(x + 1, y + 1, GRASS[0])
    return s


# =========================== saída ===========================

def write_meta(path, wrap_repeat=False):
    import uuid
    meta = path + '.meta'
    if os.path.exists(meta):
        return  # mantém o GUID: as referências no Unity continuam valendo
    tpl = open(os.path.join(HERE, 'sprite.meta.txt')).read()
    tpl = tpl.replace('{GUID}', uuid.uuid4().hex).replace('{SID}', uuid.uuid4().hex).replace('{WRAP}', '0' if wrap_repeat else '1')
    open(meta, 'w').write(tpl)


def save(root, rel, img, wrap=False):
    path = os.path.join(root, rel)
    os.makedirs(os.path.dirname(path), exist_ok=True)
    img.save(path)
    write_meta(path, wrap)
    print('gerado', rel)


ICONS = {
    'Resources/Weapons/icone_besta.png': lambda: icon_arma(f_besta),
    'Resources/Weapons/icone_espada.png': lambda: icon_arma(f_espada),
    'Resources/Weapons/icone_facas.png': lambda: icon_arma(f_faca),
    'Resources/Weapons/icone_machado.png': lambda: icon_arma(f_machado),
    'Resources/Weapons/icone_bombas.png': icone_bombas,
    'Resources/Weapons/virote.png': virote,
    'Resources/Weapons/faca.png': faca,
    'Resources/Weapons/machado.png': machado,
    'Resources/Gear/amuleto_carvalho.png': amuleto_carvalho,
    'Resources/Gear/pedra_amolar.png': pedra_amolar,
    'Resources/Gear/cota_malha.png': cota_malha,
    'Resources/Gear/luvas_couro.png': luvas_couro,
    'Resources/Gear/alforje.png': alforje,
    'Resources/Gear/ervas_curandeiro.png': ervas_curandeiro,
    'Resources/Gear/moeda_fortuna.png': moeda_fortuna,
    'Resources/Pickups/gema_xp.png': gema_xp,
    'Resources/Pickups/moeda.png': moeda,
    'Resources/Pickups/cura.png': cura,
    'Resources/Pickups/ima.png': ima,
    'Resources/Pickups/chave.png': chave,
    'Resources/Hud/madeira.png': madeira,
    'Resources/Hud/pedra.png': pedra,
}
SHEETS = {
    'Resources/Weapons/bomba.png': bomba,
    'Resources/Weapons/explosao.png': explosao,
    'Resources/Weapons/golpe_espada.png': golpe_espada,
}

if __name__ == '__main__':
    root = sys.argv[1] if len(sys.argv) > 1 else os.path.join(HERE, '..', '..', 'Assets', 'Art')
    for rel, make in ICONS.items():
        save(root, rel, make().img())
    for rel, make in SHEETS.items():
        save(root, rel, sheet(make()))
    save(root, 'Sprites/chao.png', chao().img(), wrap=True)
