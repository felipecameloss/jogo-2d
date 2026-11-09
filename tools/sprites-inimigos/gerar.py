# Gera as folhas de sprite dos inimigos e chefes no estilo do player.png:
# sem linha preta (a borda usa o tom escuro do próprio material), luz de cima à esquerda,
# paleta terrosa tirada do jogador e sombra translúcida no chão.
#
# Uso (Python 3 com Pillow):  python tools/sprites-inimigos/gerar.py
# Saída: Assets/Art/Resources/Enemies/ (ou a pasta passada como argumento).
#
# Cada folha é uma tira de 8 quadros olhando para a direita (o jogo vira com flipX):
#   slime.png         32x32  salto (achata e estica)
#   bat.png           32x32  bater de asas, voando
#   golem.png         48x48  passo pesado
#   golem_anciao.png  64x64  passo pesado, runa pulsando     (chefe da Noite 4)
#   rei_da_noite.png  80x80  flutuando, capa e fiapos ao vento (chefe da Noite 7)
import os, sys, math
sys.path.insert(0, os.path.dirname(__file__))
from px import *

# ---------- paleta (derivada do player.png) ----------
EYE = (34, 22, 26)          # tom mais escuro do jogador
CREAM = (242, 232, 204)
CREAM2 = (220, 208, 178)
CREAM3 = (184, 172, 146)

SLIME = [(34, 58, 50), (52, 96, 60), (84, 140, 64), (128, 182, 80), (184, 216, 116), (236, 244, 190)]
BONE_IN = [(196, 214, 160), (150, 184, 118)]  # osso visto através do verde


# ======================= SLIME =======================
# 32x32, 8 quadros: salto com achatar e esticar. Olha para a direita.
SLIME_ANIM = [  # (escala x, escala y, altura do salto)
    (1.00, 1.00, 0),
    (1.14, 0.84, 0),
    (0.88, 1.16, 1),
    (0.94, 1.08, 3),
    (1.00, 1.00, 4),
    (0.94, 1.08, 2),
    (1.16, 0.80, 0),
    (1.06, 0.92, 0),
]


def slime_frame(sx, sy, lift):
    s = Spr(32, 32)
    cx, ground = 16.0, 27
    shadow(s, cx, ground + 0.5, 8.5 - lift * 0.6, 2.2)
    rx, rt, rb = 9.4 * sx, 10.5 * sy, 3.2 * sy
    base = ground - lift
    body = dome(cx, base, rx, rt, rb)
    cy = base - (rt + rb) * 0.45
    lvl = sphere_level(cx, cy, rx, (rt + rb) * 0.6, [-0.05, 0.42, 0.78])

    def level(x, y):
        return 1 + lvl(x, y)
    paint(s, body, SLIME, level)
    x0, y0, x1, y1 = bbox(body)
    h = y1 - y0
    # osso engolido, visto através da gelatina
    bx, by = int(cx - rx * 0.62), int(base - 5)
    bone = {(0, 0): 1, (4, 0): 1, (0, 1): 0, (1, 1): 0, (2, 1): 0, (3, 1): 0, (4, 1): 0, (0, 2): 1, (4, 2): 1}
    for (dx, dy), k in bone.items():
        if (bx + dx, by + dy) in body and (bx + dx, by + dy + 1) in body:
            s.put(bx + dx, by + dy, BONE_IN[k])
    # brilho interno embaixo (gelatina translúcida)
    for x in range(x0 + 2, x1 - 1):
        y = max(yy for (xx, yy) in body if xx == x) - 1
        if x > cx - 1:
            s.put(x, y, SLIME[3])
    # reflexo
    hx, hy = int(cx - rx * 0.5), int(y0 + 2)
    s.put(hx, hy, SLIME[5]); s.put(hx + 1, hy, SLIME[5]); s.put(hx, hy + 1, SLIME[5])
    s.put(hx + 3, hy - 1 if hy - 1 > y0 else hy, SLIME[4])
    # bolhas
    s.put(int(cx + rx * 0.05), int(base - 2.5), SLIME[4])
    s.put(int(cx - rx * 0.15), int(base - 5), SLIME[4])
    # olhos bravos (olhando para a direita): sobrancelha caída para dentro
    ey = int(y0 + h * 0.36) + 1
    fx = int(cx + 1)
    for ex in (fx, fx + 4):
        s.put(ex, ey, EYE); s.put(ex, ey + 1, EYE)
    s.put(fx + 1, ey - 1, SLIME[1]); s.put(fx + 3, ey - 1, SLIME[1])
    # boca com um dente
    my = ey + 3
    for mx in range(fx + 1, fx + 4):
        s.put(mx, my, EYE)
    s.put(fx + 3, my + 1, CREAM)
    return s


def slime():
    return [slime_frame(*a) for a in SLIME_ANIM]



# ======================= MORCEGO =======================
# 32x32, 8 quadros de bater de asas. Voa: a sombra fica no chão, longe do corpo.
FUR = [(36, 24, 40), (66, 42, 68), (100, 68, 100), (136, 98, 130), (172, 136, 160)]
WING = [(38, 22, 42), (76, 44, 76), (112, 70, 104), (148, 102, 130)]
GLOW_EYE = (255, 214, 92)
EAR_IN = (176, 108, 88)

BAT_ANIM = [  # (ângulo da asa em graus, deslocamento vertical do corpo)
    (55, 1), (32, 1), (6, 0), (-22, -1), (-38, -1), (-18, -1), (14, 0), (40, 0),
]

# Asa direita em coordenadas locais (ombro na origem, apontando para a direita, y para baixo).
WING_SHAPE = [(0, -1.5), (3.5, -3.8), (7.5, -3.2), (12.2, -1.6),   # borda de ataque até a ponta
              (10.4, 1.2), (9.0, 0.6), (8.0, 3.4), (6.4, 1.8), (4.8, 4.2), (3.0, 2.4), (0.4, 3.0)]
WING_BONES = [(12.2, -1.6), (8.0, 3.4), (4.8, 4.2)]


def _rot(p, a, sx=1.0):
    ca, sa = math.cos(a), math.sin(a)
    x, y = p[0] * sx, p[1]
    return (x * ca + y * sa, -x * sa + y * ca)


def bat_wing(s, ox, oy, angle, side, ramp, length=1.0):
    a = math.radians(angle)
    pts = [_rot((p[0] * length, p[1]), a) for p in WING_SHAPE]
    pts = [(ox + side * p[0], oy + p[1]) for p in pts]
    m = poly(pts)
    shoulder = (ox, oy)

    def level(x, y):
        d = math.hypot(x + 0.5 - ox, y + 0.5 - oy)
        return 1 if d < 3.2 else 2
    paint(s, m, ramp, level)
    # ossos dos dedos: do pulso até cada ponta
    wrist = _rot((3.5 * length, -3.0), a)
    wrist = (ox + side * wrist[0], oy + wrist[1])
    for b in WING_BONES:
        t = _rot((b[0] * length, b[1]), a)
        t = (ox + side * t[0], oy + t[1])
        n = 8
        for i in range(1, n):
            px_ = wrist[0] + (t[0] - wrist[0]) * i / n
            py_ = wrist[1] + (t[1] - wrist[1]) * i / n
            q = (int(px_), int(py_))
            if q in m and (q[0], q[1] + 1) in m and (q[0] + side, q[1]) in m:
                s.put(q[0], q[1], ramp[3])
    # braço: ombro até o pulso
    for i in range(0, 6):
        q = (int(ox + (wrist[0] - ox) * i / 5), int(oy + (wrist[1] - oy) * i / 5))
        if q in m and (q[0], q[1] - 1) in m:
            s.put(q[0], q[1], ramp[3])
    return m


def bat_frame(angle, bob):
    s = Spr(32, 32)
    cx, cy = 16.0, 13.0 + bob
    shadow(s, 16, 27.5, 5.0 + max(0, -angle) * 0.03, 1.6)
    # asa de trás (esquerda) um pouco menor e mais escura, asa da frente (direita)
    bat_wing(s, cx - 2.5, cy - 0.5, angle, -1, [WING[0], WING[0], WING[1], WING[2]], 0.92)
    bat_wing(s, cx + 2.5, cy - 0.5, angle, 1, WING)
    # corpo peludo, com peito mais claro
    body = ellipse(cx, cy, 4.3, 4.6)
    paint(s, body, FUR, lambda x, y: 1 + sphere_level(cx, cy, 4.3, 4.6, [0.1, 0.55])(x, y))
    for (x, y) in [(int(cx - 1), int(cy + 2)), (int(cx), int(cy + 2)), (int(cx), int(cy + 3)), (int(cx - 1), int(cy + 1))]:
        s.put(x, y, FUR[3])
    # orelhas pontudas, desenhadas pixel a pixel por cima da cabeça
    top = int(cy) - 8
    for ex in (int(cx) - 3, int(cx) + 1):
        s.put(ex + 1, top, FUR[1])
        for r in range(1, 4):
            s.put(ex, top + r, FUR[2] if r > 1 else FUR[1])
            s.put(ex + 1, top + r, EAR_IN if r > 1 else FUR[2])
            s.put(ex + 2, top + r, FUR[0] if r < 3 else FUR[1])
    # pés pendurados
    for fx in (int(cx - 2), int(cx + 1)):
        s.put(fx, int(cy + 4.6), FUR[0]); s.put(fx, int(cy + 5.6), FUR[1])
    # rosto virado para a direita: olhos que brilham e presas
    ey = int(cy - 1)
    fx = int(cx - 1)
    for ex in (fx, fx + 3):
        s.put(ex, ey, GLOW_EYE); s.put(ex, ey - 1, FUR[0])
    for mx in range(fx, fx + 3):
        s.put(mx, ey + 2, FUR[0])
    s.put(fx, ey + 3, CREAM); s.put(fx + 2, ey + 3, CREAM)
    return s


def bat():
    return [bat_frame(*a) for a in BAT_ANIM]



# ======================= GOLEM =======================
# Corpo de pedras empilhadas, cabeça afundada nos ombros, olhos que brilham.
# Coordenadas pensadas num quadro de 48x48 com o chão na linha 43; `k` amplia para o chefe.
STONE = [(40, 30, 34), (72, 56, 54), (104, 82, 70), (138, 110, 88), (174, 146, 114), (210, 186, 148)]
MOSS = [(44, 74, 60), (60, 98, 70), (88, 134, 84), (126, 172, 108)]   # verdes da túnica do jogador
AMBER = [(150, 84, 34), (236, 190, 70), (255, 238, 168)]


def _hash(x, y, seed=0):
    h = (x * 374761393 + y * 668265263 + seed * 2147483647) & 0xffffffff
    h = (h ^ (h >> 13)) * 1274126177 & 0xffffffff
    return (h ^ (h >> 16)) / 0xffffffff


class Stone:
    """Desenha partes de pedra com sombreamento esférico e contorno seletivo."""

    def __init__(self, s, ramp, k, ox, oy):
        self.s, self.ramp, self.k, self.ox, self.oy = s, ramp, k, ox, oy

    def P(self, x, y):
        return (self.ox + (x - 24) * self.k, self.oy + (y - 43) * self.k)

    def poly(self, pts, dx=0, dy=0):
        return poly([self.P(x + dx, y + dy) for (x, y) in pts])

    def ell(self, cx, cy, rx, ry, dx=0, dy=0):
        X, Y = self.P(cx + dx, cy + dy)
        return ellipse(X, Y, rx * self.k, ry * self.k)

    def boulder(self, m, back=False, cuts=(-0.1, 0.35, 0.72)):
        x0, y0, x1, y1 = bbox(m)
        cx, cy = (x0 + x1 + 1) / 2, (y0 + y1 + 1) / 2
        f = sphere_level(cx, cy, (x1 - x0 + 1) / 2 + 0.5, (y1 - y0 + 1) / 2 + 0.5, list(cuts))
        ramp = self.ramp
        if back:
            ramp = [ramp[0]] + ramp[:-1]
        paint(self.s, m, ramp, lambda x, y: 1 + f(x, y))
        return m

    def moss(self, m, x_from, x_to, depth=2, seed=1, ramp=MOSS):
        """Musgo nas superfícies de cima da máscara, entre x_from e x_to (coord. do quadro)."""
        cols = {}
        for (x, y) in m:
            if x_from <= x <= x_to:
                cols[x] = min(y, cols.get(x, 999))
        for x, ty in cols.items():
            d = depth + (1 if _hash(x, 7, seed) > 0.55 else 0) - (1 if _hash(x, 3, seed) > 0.8 else 0)
            for r in range(d):
                if (x, ty + r) not in m:
                    break
                if r == 0:
                    c = ramp[3] if x < (x_from + x_to) / 2 else ramp[2]
                elif r == d - 1:
                    c = ramp[0]
                else:
                    c = ramp[1] if x > (x_from + x_to) / 2 else ramp[2]
                self.s.put(x, ty + r, c)

    def crack(self, pts, dx=0, dy=0, col=None, glow=None):
        col = col or self.ramp[0]
        pts = [self.P(x + dx, y + dy) for (x, y) in pts]
        for (a, b) in zip(pts, pts[1:]):
            n = int(max(abs(b[0] - a[0]), abs(b[1] - a[1]))) + 1
            for i in range(n + 1):
                x = int(a[0] + (b[0] - a[0]) * i / n)
                y = int(a[1] + (b[1] - a[1]) * i / n)
                if self.s.get(x, y) and self.s.get(x, y)[3] == 255:
                    self.s.put(x, y, col)
                    if glow and _hash(x, y, 5) > 0.5:
                        self.s.put(x, y, glow)


GOLEM_TORSO = [(12, 22), (14, 17), (19, 14), (27, 14), (32, 16), (35, 21), (35, 29), (32, 34), (25, 35.5), (16, 35), (12, 30)]
GOLEM_HEAD = [(23, 11), (25, 8.5), (31, 8), (34.5, 10), (35, 15), (32, 17.5), (25, 17.5), (22.5, 15)]


def golem_pose(i):
    p = i * math.pi / 4
    sn = math.sin(p)
    return dict(
        front_lift=round(2 * max(0.0, sn)), back_lift=round(2 * max(0.0, -sn)),
        front_dx=round(1.4 * math.cos(p)), back_dx=-round(1.4 * math.cos(p)),
        bob=-1 if abs(sn) > 0.9 else 0,
        arm_front=round(1.2 * -sn), arm_back=round(1.2 * sn),
    )


def golem_frame(i, size=48, k=1.0, ramp=STONE, extra=None, moss=MOSS, eye=AMBER):
    s = Spr(size, size)
    ground = size - 5
    st = Stone(s, ramp, k, size / 2, ground)
    q = golem_pose(i)
    b = q['bob']
    X, Y = st.P(24, 43)
    shadow(s, X, Y + 0.5, 14 * k, 3 * k)
    if extra and 'behind' in extra:
        extra['behind'](st, q)
    # braço de trás (esquerda), mais escuro
    st.boulder(st.ell(11.5, 21, 5, 5, dy=b + q['arm_back']), back=True)
    st.boulder(st.poly([(7, 23), (13, 23), (13.5, 31), (7, 31.5)], dy=b + q['arm_back']), back=True)
    st.boulder(st.ell(9.5, 34, 4.8, 4.2, dy=b + q['arm_back']), back=True)
    # pernas
    st.boulder(st.poly([(15, 32), (21, 32), (21.5, 40), (22.5, 42.5), (13.5, 42.5), (14.5, 40)],
                        dx=q['back_dx'], dy=-q['back_lift']), back=True)
    st.boulder(st.poly([(25, 32), (31, 32), (31.5, 40), (33, 42.5), (24, 42.5), (24.5, 40)],
                        dx=q['front_dx'], dy=-q['front_lift']))
    # tronco
    torso = st.boulder(st.poly(GOLEM_TORSO, dy=b), cuts=(-0.15, 0.3, 0.68))
    st.crack([(17.5, 21), (18.5, 23), (17.5, 25)], dy=b, col=ramp[1])
    st.crack([(28, 28), (29.5, 30.5)], dy=b, col=ramp[1])
    st.moss(torso, *[int(v) for v in (st.P(14, 0)[0], st.P(24, 0)[0])], depth=max(2, int(2 * k)), seed=2, ramp=moss)
    # cabeça afundada, virada para a direita
    head = st.boulder(st.poly(GOLEM_HEAD, dy=b), cuts=(-0.1, 0.35, 0.7))
    st.moss(head, *[int(v) for v in (st.P(24, 0)[0], st.P(30, 0)[0])], depth=max(2, int(1.6 * k)), seed=4, ramp=moss)
    # olhos: faixa escura com duas fendas brilhando
    ex0, ey = st.P(27, 12.5 + b)
    w = max(1, int(round(2 * k)))
    for xx in range(int(ex0), int(st.P(34, 0)[0])):
        for yy in range(int(ey), int(ey + max(2, round(2 * k)))):
            if (xx, yy) in head:
                s.put(xx, yy, ramp[0])
    for e in (27.5, 31.5):
        gx, gy = st.P(e, 13 + b)
        for dx in range(w):
            s.put(int(gx) + dx, int(gy), eye[2] if dx == 0 else eye[1])
    # braço da frente (direita) com punho grande
    st.boulder(st.ell(34, 20.5, 5.6, 5.4, dy=b + q['arm_front']))
    st.boulder(st.poly([(32.5, 23), (39, 23.5), (40, 31), (33.5, 31.5)], dy=b + q['arm_front']))
    fist = st.boulder(st.ell(37, 34.5, 5.6, 4.8, dy=b + q['arm_front']))
    st.moss(st.ell(34, 20.5, 5.6, 5.4, dy=b + q['arm_front']),
            *[int(v) for v in (st.P(30, 0)[0], st.P(36, 0)[0])], depth=max(2, int(2 * k)), seed=6, ramp=moss)
    # dedos do punho
    for fx in (35.5, 38.5):
        st.crack([(fx, 33.5), (fx, 36.5)], dy=b + q['arm_front'], col=ramp[1])
    if extra and 'front' in extra:
        extra['front'](st, q)
    return s


def golem():
    return [golem_frame(i) for i in range(8)]



# ======================= GOLEM ANCIÃO (chefe da Noite 4) =======================
# O mesmo corpo do Golem, 1,35x maior, em basalto antigo: cristais âmbar nas costas,
# runa e trincas de lava no peito (a cor dos tiros dele), barba de musgo e cipós.
BASALT = [(28, 26, 36), (48, 46, 60), (72, 70, 84), (100, 98, 108), (134, 130, 134), (170, 164, 158)]
LAVA = [(122, 42, 30), (204, 88, 42), (246, 152, 66), (255, 222, 140)]
MOSS_OLD = [(34, 56, 50), (52, 88, 64), (82, 126, 80), (120, 166, 104)]
RUNE = ["X.X.X", ".XXX.", "..X..", "..X..", "..X..", ".X.X.", "X...X"]
PULSE = [1, 2, 3, 3, 2, 1, 1, 1]  # brilho da runa ao longo dos 8 quadros


CRYSTAL = [(110, 40, 34), (196, 92, 44), (240, 150, 70), (255, 214, 130), (255, 244, 210)]
SIGIL = ["...X...", "..X.X..", ".X...X.", "X..X..X", ".X...X.", "..X.X..", "...X..."]


def crystal(st, base, tip, w, dy):
    """Prisma de cristal: face esquerda clara, direita escura, ponta brilhante."""
    s = st.s
    bx, by = base
    tx, ty = tip
    ax, ay = tx - bx, ty - by
    L = math.hypot(ax, ay)
    px_, py_ = -ay / L, ax / L  # perpendicular

    def at(u, v):
        return (bx + ax * v + px_ * u, by + ay * v + py_ * u)
    pts = [at(-w / 2, 0), at(-w / 2, 0.68), at(0, 1), at(w / 2, 0.68), at(w / 2, 0)]
    m = st.poly(pts, dy=dy)
    B = st.P(bx, by + dy)
    T = st.P(tx, ty + dy)

    def level(x, y):
        side = (x + 0.5 - B[0]) * (T[1] - B[1]) - (y + 0.5 - B[1]) * (T[0] - B[0])
        return 3 if side > 0 else 2
    paint(s, m, CRYSTAL, level)
    # brilho na ponta e na aresta clara
    tx_, ty_ = int(T[0]), int(T[1])
    for d in range(1, 3):
        if (tx_, ty_ + d) in m:
            s.put(tx_, ty_ + d, CRYSTAL[4])
            break
    return m


def anciao_behind(st, q):
    b = q['bob']
    crystal(st, (13.5, 19), (8, 7.5), 4.4, b)
    crystal(st, (18.5, 16), (17, 3.5), 5.0, b)
    crystal(st, (22.5, 15), (24.5, 8), 3.4, b)


def anciao_front(st, q, i):
    s, b = st.s, q['bob']
    glow = LAVA[PULSE[i]]
    # sigilo entalhado no peito: borda escura e linhas que brilham
    cx, cy = st.P(22.5, 24.5 + b)
    ox, oy = int(cx) - 3, int(cy) - 3
    pix = {(ox + c, oy + r) for r, row in enumerate(SIGIL) for c, ch in enumerate(row) if ch == 'X'}
    for (x, y) in pix:
        for dx in (-1, 0, 1):
            for dy in (-1, 0, 1):
                n = (x + dx, y + dy)
                if n not in pix and s.get(*n) and s.get(*n)[3] == 255 and (dx == 0 or dy == 0):
                    s.put(n[0], n[1], BASALT[1])
    for (x, y) in pix:
        s.put(x, y, LAVA[3] if (x, y) == (ox + 3, oy + 3) else glow)
    # trincas de lava saindo das pontas do sigilo
    k = st.k
    def back(px, py):
        return ((px - st.ox) / k + 24, (py - st.oy) / k + 43 - b)
    L = back(ox - 1, oy + 3); D = back(ox + 3, oy + 7); R = back(ox + 7, oy + 3)
    st.crack([L, (L[0] - 2.5, L[1] + 2), (L[0] - 3.5, L[1] + 5)], dy=b, col=LAVA[0], glow=glow)
    st.crack([D, (D[0] + 1, D[1] + 3), (D[0] + 0.5, D[1] + 6)], dy=b, col=LAVA[0], glow=glow)
    st.crack([R, (R[0] + 3, R[1] - 1.5), (R[0] + 4.5, R[1] + 0.5)], dy=b, col=LAVA[0], glow=glow)
    # barba de musgo pendurada no queixo
    for x in range(*[int(v) for v in (st.P(24.5, 0)[0], st.P(33.5, 0)[0])]):
        y0 = int(st.P(0, 17.0 + b)[1])
        n = 2 + int(_hash(x, 1, 9) * 3.2)
        for r in range(n):
            s.put(x, y0 + r, MOSS_OLD[3] if r == 0 else (MOSS_OLD[2] if r < n - 1 else MOSS_OLD[1]))
    # cipó caindo do ombro da frente
    vx, vy = st.P(38, 17 + b + q['arm_front'])
    for r in range(10):
        x = int(vx + (1 if r in (3, 4, 5) else 0))
        s.put(x, int(vy) + r, MOSS_OLD[1])
        if r % 3 == 1:
            s.put(x + 1, int(vy) + r, MOSS_OLD[3])
    # cristal pequeno na cabeça
    crystal(st, (29, 9), (29.5, 3.5), 2.6, b)


def golem_anciao():
    frames = []
    for i in range(8):
        frames.append(golem_frame(i, size=64, k=1.35, ramp=BASALT, moss=MOSS_OLD, eye=LAVA[1:],
                                  extra={'behind': anciao_behind, 'front': lambda st, q, i=i: anciao_front(st, q, i)}))
    return frames



# ======================= REI DA NOITE (chefe da Noite 7) =======================
# Rei espectral que flutua: capa rasgada, armadura escura, capuz com rosto vazio,
# coroa de ouro, lua crescente no peito e espadão com brilho violeta (a cor dos tiros dele).
ARMOR = [(22, 18, 32), (42, 36, 60), (68, 64, 96), (100, 98, 130), (146, 146, 174), (200, 202, 220)]
CAPE = [(26, 14, 36), (48, 24, 62), (74, 38, 92), (104, 58, 124), (138, 88, 154)]
GOLD = [(84, 50, 30), (150, 100, 40), (212, 158, 56), (240, 200, 90), (255, 238, 170)]
VIOLET = [(96, 48, 160), (158, 98, 232), (206, 160, 255), (244, 226, 255)]
VOID = (12, 8, 18)
STEEL = [(40, 34, 56), (92, 90, 120), (150, 152, 178), (210, 212, 228)]
EYE_FLICKER = [2, 3, 3, 2, 2, 3, 2, 2]


def rei_frame(i):
    s = Spr(80, 80)
    p = i * math.pi / 4
    b = round(1.6 * math.sin(p))          # flutua
    shadow(s, 40, 74.5, 13 - b * 0.6, 2.6)

    def P(pts, dy=0):
        return poly([(x, y + dy) for (x, y) in pts])

    # ---- capa (atrás), puxada para a esquerda porque ele anda para a direita
    tat = []
    xs = [15, 19, 23, 27, 31, 35, 39, 43, 47, 51, 55]
    for j, x in enumerate(xs):
        wave = math.sin(p + j * 0.9)
        y = 62 + (4 if j % 2 == 0 else 0) + 2.2 * wave
        tat.append((x - 1.5 * wave, y))
    cape_pts = [(30, 25), (50, 25), (54, 34), (56, 50)] + list(reversed(tat)) + [(13, 56), (22, 36)]
    cape = P(cape_pts, b)

    def cape_level(x, y):
        fold = math.sin((x - 2 * math.sin(p)) * 0.55 + y * 0.08)
        lvl = 2 if fold > -0.2 else 1
        if fold > 0.6 and x < 40:
            lvl = 3
        if y < 31 + b:
            lvl = min(lvl, 1)
        return lvl
    paint(s, cape, CAPE, cape_level)
    # forro da capa aparecendo nas pontas rasgadas
    for (x, y) in cape:
        if (x, y + 1) not in cape and (x, y - 3) in cape and _hash(x, y, 3) > 0.4:
            s.put(x, y - 1, VIOLET[0])

    # ---- fiapos de sombra no lugar das pernas
    for j, (wx, length) in enumerate([(34, 14), (38.5, 17), (43, 15), (47, 11)]):
        sway = 2.0 * math.sin(p + j * 1.3)
        top = 49 + b
        pts = [(wx - 2.6, top), (wx + 2.6, top), (wx + 1.4 + sway * 0.5, top + length * 0.6),
               (wx + sway, top + length), (wx - 1.2 + sway * 0.5, top + length * 0.6)]
        m = poly(pts)
        paint(s, m, [VOID, CAPE[0], CAPE[1], CAPE[2]], lambda x, y: 2 if y < top + length * 0.35 else 1)

    # ---- tronco: peitoral, cinto de ouro e saiote de placas
    chest = P([(31, 28), (49, 28), (48.5, 40), (46, 46), (34, 46), (31.5, 40)], b)
    paint(s, chest, ARMOR, lambda x, y: 1 + sphere_level(40, 33 + b, 10, 10, [-0.2, 0.3, 0.7])(x, y))
    skirt = P([(33, 46), (47, 46), (49.5, 53), (30.5, 53)], b)
    paint(s, skirt, ARMOR, lambda x, y: 2 if (x - 31) % 5 else 1)
    for x in range(33, 48):
        s.put(x, 45 + b, GOLD[3] if x < 40 else GOLD[2])
        s.put(x, 46 + b, GOLD[1])
    s.put(40, 45 + b, VIOLET[2]); s.put(41, 45 + b, VIOLET[1])
    # lua crescente no peito
    moon = ellipse(40.5, 35.5 + b, 3.6, 3.8) - ellipse(42.3, 34.6 + b, 3.0, 3.2)
    for (x, y) in moon:
        s.put(x, y, VIOLET[3] if x < 39 else VIOLET[2])

    # ---- ombreiras com espinhos
    for (cx, cy, rx, ry, back) in ((30, 28.5, 6.2, 4.6, True), (50.5, 28.5, 6.6, 5.0, False)):
        for sx_ in (-3, 0, 3):
            spike = P([(cx + sx_ - 1.8, cy - 2), (cx + sx_ + 1.8, cy - 2), (cx + sx_ + 0.8 * (1 if not back else -1), cy - 10 + abs(sx_) * 0.9)], b)
            paint(s, spike, ARMOR, 4 if sx_ < 0 else 3, out_light=2, out_dark=1)
        m = ellipse(cx, cy + b, rx, ry)
        ramp = ARMOR if not back else [ARMOR[0]] + ARMOR[:-1]
        paint(s, m, ramp, lambda x, y, cx=cx, cy=cy, rx=rx, ry=ry: 1 + sphere_level(cx, cy + b, rx, ry, [-0.1, 0.35, 0.7])(x, y))
        for x in range(int(cx - rx + 2), int(cx + rx - 1)):
            if (x, int(cy + b + 2)) in m:
                s.put(x, int(cy + b + 2), GOLD[2] if x < cx else GOLD[1])

    # ---- capuz, rosto vazio e olhos
    hx, hy = 41.5, 19.5 + b
    hood = ellipse(hx, hy, 7.6, 8.2)
    paint(s, hood, CAPE, lambda x, y: 1 + sphere_level(hx, hy, 7.6, 8.2, [-0.1, 0.4, 0.75])(x, y))
    face = ellipse(43.6, 21.5 + b, 4.4, 5.2)
    for (x, y) in face:
        s.put(x, y, VOID)
    e = VIOLET[EYE_FLICKER[i]]
    for ex in (41, 45):
        s.put(ex - 1, 21 + b, VIOLET[0]); s.put(ex + 2, 21 + b, VIOLET[0])
        s.put(ex, 21 + b, VIOLET[3]); s.put(ex + 1, 21 + b, e)
        s.put(ex, 22 + b, VIOLET[1]); s.put(ex + 1, 22 + b, VIOLET[0])

    # ---- coroa sobre o capuz
    top = 12 + b
    band = rect(34, top, 49, top + 2) - {(34, top), (49, top)}
    paint(s, band, GOLD, lambda x, y: 4 if (y == top and x < 42) else (3 if x < 42 else 2), out_light=2, out_dark=1)
    for k, tx in enumerate((35, 38.5, 41.8, 45.2, 48.5)):
        h = 6 if k == 2 else (5 if k in (1, 3) else 4)
        pt = poly([(tx - 1.6, top + 0.5), (tx + 1.6, top + 0.5), (tx, top - h)])
        paint(s, pt, GOLD, 4 if tx < 42 else 3, out_light=2, out_dark=1)
        s.put(int(tx), int(top - h + 1), GOLD[4])
    for gx in (37, 41, 45):
        s.put(gx, top + 1, VIOLET[2]); s.put(gx + 1, top + 1, VIOLET[1])

    # ---- espadão apoiado à frente, ponta para baixo
    sw = 54
    blade = P([(sw - 1.5, 45), (sw + 2.5, 45), (sw + 2.5, 64), (sw + 0.5, 68), (sw - 1.5, 64)], b)
    paint(s, blade, STEEL, lambda x, y: 3 if x < sw + 0.5 else 2)
    for y in range(47 + b, 63 + b):
        if (y + i) % 4 == 0:
            s.put(sw, y, VIOLET[2])
        elif (sw, y) in blade:
            s.put(sw, y, VIOLET[1])
    guard = rect(sw - 4, 43 + b, sw + 5, 44 + b)
    paint(s, guard, GOLD, lambda x, y: 3 if x < sw else 2)
    grip = rect(sw, 37 + b, sw + 1, 42 + b)
    paint(s, grip, CAPE, 2)
    pommel = ellipse(sw + 1, 36 + b, 1.6, 1.6)
    paint(s, pommel, GOLD, 3)
    s.put(sw, 36 + b, VIOLET[2])

    # ---- braço da frente segurando o cabo
    arm = P([(48, 31), (53.5, 32), (55, 40), (50.5, 41)], b)
    paint(s, arm, ARMOR, lambda x, y: 2 if x < 52 else 1)
    gaunt = ellipse(54.5, 40.5 + b, 3.2, 2.8)
    paint(s, gaunt, ARMOR, lambda x, y: 1 + sphere_level(54.5, 40.5 + b, 3.2, 2.8, [0.0, 0.5])(x, y))
    return s


def rei_da_noite():
    return [rei_frame(i) for i in range(8)]


SHEETS = {
    'slime': slime, 'bat': bat, 'golem': golem,
    'golem_anciao': golem_anciao, 'rei_da_noite': rei_da_noite,
}

if __name__ == '__main__':
    here = os.path.dirname(os.path.abspath(__file__))
    out = sys.argv[1] if len(sys.argv) > 1 else os.path.join(here, '..', '..', 'Assets', 'Art', 'Resources', 'Enemies')
    os.makedirs(out, exist_ok=True)
    for name, make in SHEETS.items():
        sheet(make()).save(os.path.join(out, name + '.png'))
        print('gerado', name + '.png')
