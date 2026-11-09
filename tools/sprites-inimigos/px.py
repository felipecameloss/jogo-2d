# Motor mínimo de pixel art: máscaras, sombreamento por rampa e contorno seletivo
# no estilo do player.png (sem linha preta; a borda usa o tom escuro do próprio material,
# luz vindo de cima à esquerda).
import math
from PIL import Image

SHADOW = (0, 0, 0, 58)  # mesma sombra translúcida do jogador


class Spr:
    def __init__(self, w, h):
        self.w, self.h = w, h
        self.c = {}

    def put(self, x, y, col):
        if 0 <= x < self.w and 0 <= y < self.h:
            self.c[(x, y)] = tuple(col) if len(col) == 4 else tuple(col) + (255,)

    def get(self, x, y):
        return self.c.get((x, y))

    def img(self):
        im = Image.new('RGBA', (self.w, self.h), (0, 0, 0, 0))
        for (x, y), col in self.c.items():
            im.putpixel((x, y), col)
        return im


# ---------- máscaras (conjuntos de pixels) ----------

def ellipse(cx, cy, rx, ry):
    m = set()
    for y in range(int(cy - ry - 2), int(cy + ry + 3)):
        for x in range(int(cx - rx - 2), int(cx + rx + 3)):
            dx = (x + 0.5 - cx) / rx
            dy = (y + 0.5 - cy) / ry
            if dx * dx + dy * dy <= 1.0:
                m.add((x, y))
    return m


def dome(cx, base, rx, ry_top, ry_bot):
    """Gota apoiada no chão: metade de cima alta, metade de baixo achatada."""
    cy = base - ry_bot
    m = set()
    for y in range(int(cy - ry_top - 2), int(base + 2)):
        for x in range(int(cx - rx - 2), int(cx + rx + 3)):
            dx = (x + 0.5 - cx) / rx
            py = y + 0.5 - cy
            dy = py / (ry_top if py < 0 else ry_bot)
            if dx * dx + dy * dy <= 1.0 and y < base:
                m.add((x, y))
    return m


def poly(pts):
    xs = [p[0] for p in pts]
    ys = [p[1] for p in pts]
    m = set()
    for y in range(int(min(ys)) - 1, int(max(ys)) + 2):
        for x in range(int(min(xs)) - 1, int(max(xs)) + 2):
            px, py = x + 0.5, y + 0.5
            inside = False
            j = len(pts) - 1
            for i in range(len(pts)):
                xi, yi = pts[i]
                xj, yj = pts[j]
                if (yi > py) != (yj > py):
                    xint = xi + (py - yi) * (xj - xi) / (yj - yi)
                    if px < xint:
                        inside = not inside
                j = i
            if inside:
                m.add((x, y))
    return m


def rect(x0, y0, x1, y1):
    """Retângulo inclusivo."""
    return {(x, y) for y in range(y0, y1 + 1) for x in range(x0, x1 + 1)}


def rrect(x0, y0, x1, y1, r=1):
    m = rect(x0, y0, x1, y1)
    if r >= 1:
        for (x, y) in [(x0, y0), (x1, y0), (x0, y1), (x1, y1)]:
            m.discard((x, y))
    if r >= 2:
        for (x, y) in [(x0 + 1, y0), (x0, y0 + 1), (x1 - 1, y0), (x1, y0 + 1),
                       (x0 + 1, y1), (x0, y1 - 1), (x1 - 1, y1), (x1, y1 - 1)]:
            m.discard((x, y))
    return m


def shift(m, dx, dy):
    return {(x + dx, y + dy) for (x, y) in m}


def flip_h(m, cx2):
    """Espelha em torno de x = cx2/2 (cx2 = soma das bordas)."""
    return {(cx2 - x, y) for (x, y) in m}


def bbox(m):
    xs = [p[0] for p in m]
    ys = [p[1] for p in m]
    return min(xs), min(ys), max(xs), max(ys)


# ---------- sombreamento ----------

LIGHT = (-0.55, -0.65, 0.75)
_l = math.sqrt(sum(v * v for v in LIGHT))
LIGHT = tuple(v / _l for v in LIGHT)


def sphere_level(cx, cy, rx, ry, cuts, light=LIGHT):
    """Nível da rampa por normal de elipsoide. cuts: limiares crescentes."""
    def f(x, y):
        nx = (x + 0.5 - cx) / rx
        ny = (y + 0.5 - cy) / ry
        d = nx * nx + ny * ny
        nz = math.sqrt(max(0.0, 1.0 - min(d, 1.0)))
        i = nx * light[0] + ny * light[1] + nz * light[2]
        lvl = 0
        for c in cuts:
            if i > c:
                lvl += 1
        return lvl
    return f


def block_level(m, base=2, light=3, dark=1, top=1, left=1, right=1, bottom=1):
    """Sombreamento plano de bloco: topo e esquerda claros, direita e base escuros."""
    x0, y0, x1, y1 = bbox(m)

    def f(x, y):
        if y - y0 < top and (x, y - top) not in m:
            return light
        if x - x0 < left and (x - left, y) not in m:
            return light
        if (x + right, y) not in m:
            return dark
        if (x, y + bottom) not in m:
            return dark
        return base
    return f


def paint(spr, m, ramp, level, outline=True, out_light=1, out_dark=0, only=None):
    """Pinta a máscara com a rampa. Borda de baixo/direita no tom mais escuro;
    de cima/esquerda um tom acima, como no jogador."""
    for (x, y) in m:
        lvl = level(x, y) if callable(level) else level
        if outline:
            r = (x + 1, y) not in m
            b = (x, y + 1) not in m
            l = (x - 1, y) not in m
            t = (x, y - 1) not in m
            if r or b:
                lvl = out_dark
            elif l or t:
                lvl = min(lvl, out_light)
        lvl = max(0, min(len(ramp) - 1, lvl))
        spr.put(x, y, ramp[lvl])


def shadow(spr, cx, cy, rx, ry):
    for (x, y) in ellipse(cx, cy, rx, ry):
        spr.put(x, y, SHADOW)


def outer_outline(spr, ramp_of, dark_only=False):
    """Escurece a silhueta externa (pixels opacos vizinhos de transparente/sombra)."""
    changes = {}
    for (x, y), col in spr.c.items():
        if col[3] < 255:
            continue
        for dx, dy in ((1, 0), (0, 1), (-1, 0), (0, -1)):
            n = spr.c.get((x + dx, y + dy))
            if n is None or n[3] < 255:
                rp = ramp_of(col)
                if rp:
                    changes[(x, y)] = rp
                break
    spr.c.update(changes)


def sheet(frames):
    w, h = frames[0].w, frames[0].h
    im = Image.new('RGBA', (w * len(frames), h), (0, 0, 0, 0))
    for i, f in enumerate(frames):
        im.alpha_composite(f.img(), (i * w, 0))
    return im
