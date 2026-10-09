"""Re-traces the weapon silhouettes of the in-game pack (packs/ingame/svg) from the game's textures in reference/ingame
(create them with export_originals.py; local only). Contour at 8x bicubic upscale, so edges are smooth and details like the
depth charge lid, glasses outline or the car on the oil slick are kept. The ring/disc/shading part of each SVG is left alone.
Usage: trace_ingame.py [svg name ...]   then ./build.sh ingame"""
import sys
from pathlib import Path
import numpy as np
from PIL import Image
import contourpy
HERE=Path(__file__).resolve().parent
REF=str(HERE/'reference/ingame')+'/'
SVG=str(HERE/'packs/ingame/svg')+'/'
PAIRS={'Mortar':'Mortar','Machinegun':'gattlingun','Depthcharge':'DepthCharge','Missile':'Missile','Mine':'mine','Flamethrower':'FlameThrower','Shotgun':'Shotgun','Flare':'Shine','oil':'Oil'}
CX,CY,UP=32.08,32.4,8
# stacked grey levels: (luminance below, fill, radius limit in px). The light levels stay clear of the ring's own inner shadow.
LEVELS=[(190,'#b4b4b4',23),(140,'#828282',23),(90,'#505050',25.5),(45,'#000000',25.5)]

def dp(pts,eps):
    if len(pts)<3: return pts
    a,b=pts[0],pts[-1]; ab=b-a; n=np.hypot(*ab)
    d=np.abs(np.cross(ab,pts-a))/n if n>1e-9 else np.hypot(*(pts-a).T)
    i=int(np.argmax(d))
    if d[i]>eps: return np.vstack([dp(pts[:i+1],eps)[:-1],dp(pts[i:],eps)])
    return np.vstack([a,b])

def level_path(name,lum_below,rmax):
    lum=np.asarray(Image.open(REF+name+'.png').convert('L'),float)
    dark=Image.fromarray((255-lum).astype(np.float32),mode='F').resize((64*UP,64*UP),Image.BICUBIC)
    yy,xx=np.mgrid[:64*UP,:64*UP]; r=np.hypot((xx+.5)/UP-CX,(yy+.5)/UP-CY)
    z=np.where(r<rmax,np.asarray(dark),0)
    d=[]
    for sgm in contourpy.contour_generator(z=z).lines(255-lum_below):
        p=(sgm+0.5)/UP  # contourpy gives (col,row) in index space; +0.5 centres on the pixel, /UP back to 64 px space
        closed=np.allclose(p[0],p[-1]); p=dp(p,0.03)
        if len(p)<4 or abs(0.5*np.sum(p[:-1,0]*p[1:,1]-p[1:,0]*p[:-1,1]))<0.6: continue  # specks under 0.6 px^2
        q=p/2  # 64 px -> 32 unit viewBox
        d.append('M'+' L'.join(f'{x:.2f},{y:.2f}' for x,y in q[:-1 if closed else None])+('Z' if closed else ''))
    return ' '.join(d)

def silhouette_paths(name):
    return ''.join(f'  <path d="{d}" fill="{c}" fill-rule="evenodd"/>\n' for t,c,rm in LEVELS if (d:=level_path(name,t,rm)))

def apply(a,b):
    f=SVG+b+'.svg'; s=open(f).read()
    i=s.index('\n',s.index('url(#innerShade)'))+1
    s=s[:i]+'\n  <!-- In-game weapon silhouette: traced from the game texture, stacked grey levels (even-odd) -->\n%s</svg>\n'%silhouette_paths(a)
    open(f,'w').write(s)
if __name__=='__main__':
    for a,b in PAIRS.items():
        if len(sys.argv)>1 and b not in sys.argv[1:]: continue
        apply(a,b); print(b)
