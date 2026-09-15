"""Recreate the OFL Nunito wght=800 static source used for offline TMP baking."""
from pathlib import Path
from fontTools.ttLib import TTFont
from fontTools.varLib.instancer import instantiateVariableFont

source = Path(__file__).resolve().parent / "Source"
original = TTFont(source / "Nunito-Variable.ttf")
static = instantiateVariableFont(original, {"wght": 800}, inplace=False)
renamed = {
    1: "Harvest Body Round", 2: "ExtraBold",
    3: "HarvestBodyRound-ExtraBold-20260915",
    4: "Harvest Body Round ExtraBold", 6: "HarvestBodyRound-ExtraBold",
    16: "Harvest Body Round", 17: "ExtraBold",
}
for record in static["name"].names:
    if record.nameID in renamed:
        record.string = renamed[record.nameID].encode(record.getEncoding(), errors="replace")
static["OS/2"].usWeightClass = 800
static.save(source / "HarvestBodyRound-ExtraBold.ttf")
static.close()
original.close()
