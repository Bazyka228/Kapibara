import sys
from pathlib import Path
sys.path.insert(0, r'C:/Users/fedor/AppData/Local/Temp/capy-fonttools')
from fontTools.ttLib import TTFont
from fontTools.varLib.instancer import instantiateVariableFont
root=Path(__file__).resolve().parents[2]
source=Path(__file__).parent/'Nunito-Variable.ttf'
for name, weight in [('SemiBold',600),('ExtraBold',800)]:
    font=instantiateVariableFont(TTFont(source),{'wght':weight},inplace=True,updateFontNames=True)
    required='АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯабвгдеёжзийклмнопрстуфхцчшщъыьэюя0123456789×›−+%·'
    missing=[c for c in required if ord(c) not in font.getBestCmap()]
    assert not missing,missing
    path=root/'Assets/Capybara67/Resources/Fonts'/('Nunito-'+name+'.ttf')
    font.save(path)
    print(name,weight,path.stat().st_size,'bytes; Cyrillic verified')
