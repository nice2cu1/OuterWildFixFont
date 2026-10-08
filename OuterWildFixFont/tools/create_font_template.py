"""Build the font-free serialization template from this project's 2.1.0 release.

Run with: uv run --with UnityPy python tools/create_font_template.py FONT_BUNDLE
The result contains Unity metadata, an empty texture and material, but no font.
"""
import struct
import sys
from pathlib import Path

import UnityPy

env = UnityPy.load(sys.argv[1])
bundle = next(iter(env.files.values()))
serialized = next(iter(bundle.files.values()))
font = next(o for o in env.objects if o.type.name == "Font" and o.read_typetree()["m_FontData"])
tree = font.read_typetree()
material_id = tree["m_DefaultMaterial"]["m_PathID"]
texture_id = tree["m_Texture"]["m_PathID"]
asset = next(o for o in env.objects if o.type.name == "AssetBundle")
keep = {font.path_id, material_id, texture_id, asset.path_id}
serialized.objects = {k: v for k, v in serialized.objects.items() if k in keep}
tree.update(m_Name="GameFont", m_FontNames=["OuterWildFixFont Embedded"], m_FallbackFonts=[],
            m_CharacterRects=[], m_KerningValues=[], m_FontData=b"OWFF_FONT_BYTES\0",
            m_FontSize=913.25, m_LineSpacing=914.25, m_Ascent=915.25, m_Descent=-916.25)
font.save_typetree(tree)
asset_tree = asset.read_typetree()
preload = [{"m_FileID": 1, "m_PathID": 10101}] + [
    {"m_FileID": 0, "m_PathID": i} for i in [texture_id, material_id, font.path_id]]
asset_tree.update(m_Name="OuterWildFixFont.FileFont", m_AssetBundleName="OuterWildFixFont.FileFont",
                  m_PreloadTable=preload,
                  m_Container=[("assets/gamefont.ttf", {"preloadIndex": 0, "preloadSize": len(preload),
                                "asset": {"m_FileID": 0, "m_PathID": font.path_id}})])
asset.save_typetree(asset_tree)
data = serialized.save()
parsed = UnityPy.load(data)
font = next(o for o in parsed.objects if o.type.name == "Font")
sf = font.assets_file
marker = data.index(b"OWFF_FONT_BYTES\0")
assert len(b"OWFF_FONT_BYTES\0") == 16
assert marker == data.rindex(b"OWFF_FONT_BYTES\0")
record = struct.pack("<qIIi", font.path_id, font.byte_start - sf.header.data_offset,
                     font.byte_size, font.type_id)
size_offset = data.index(record, 20, sf.header.data_offset) + 12
font_end = font.byte_start + font.byte_size
assert all(o.byte_start < font.byte_start for o in parsed.objects if o.path_id != font.path_id)
offsets = [data.index(struct.pack("<f", value), font.byte_start, font_end)
           for value in (913.25, 914.25, 915.25, -916.25)]
header = struct.pack("<4s8I", b"OWFF", 1, marker, size_offset, font_end, *offsets)
output = Path(__file__).resolve().parent.parent / "Resources" / "FontTemplate.bin"
output.parent.mkdir(exist_ok=True)
output.write_bytes(header + data)
print(f"Wrote {output}: {output.stat().st_size} bytes; no source font data retained")
