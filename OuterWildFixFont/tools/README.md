# Font serialization template

`Resources/FontTemplate.bin` supplies the Unity 2019.4 font object metadata
needed by the game's legacy UI. It contains an empty texture, a material,
an AssetBundle directory and a font object with a 16-byte placeholder. It
contains no original font bytes or baked glyphs.

The template was extracted from this project's published
[2.1.0 release](https://github.com/nice2cu1/OuterWildFixFont/releases/tag/2.1.0).
To regenerate it, extract `Font/fonts` from that release and run:

```sh
uv run --with UnityPy python tools/create_font_template.py PATH_TO_FONT_BUNDLE
```

At runtime `RuntimeFontData` inserts the complete local `Fonts/GameFont.ttf`
into the font object's data field, adjusts its metrics and serialization
sizes, and wraps it in an uncompressed UnityFS container in memory. There
are no system font lookups, font installation, Python runtime requirements
or external font bundles. The game still loads a generated AssetBundle
internally; this is not direct TTF import through `new Font(path)`.

The font object is last in the serialized file so the insertion cannot
change the offsets of the material, texture or directory. The generator
checks this invariant. The container is released after loading, while the
font remains alive for Unity's normal dynamic glyph rendering.
