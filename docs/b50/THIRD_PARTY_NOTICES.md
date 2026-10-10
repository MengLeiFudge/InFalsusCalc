# Third-party notices

## InFalsus-SaveData-Parser and InFalsus-Resource

Repositories: <https://github.com/REDDRAGON-HL/InFalsus-SaveData-Parser> and <https://github.com/REDDRAGON-HL/InFalsus-Resource>

These repositories declare the MIT License. Their save-format and game-metadata implementations are references for the focused B50 parser and maintenance extractor. No upstream parser or resource-extraction source file is vendored into the browser runtime. The generated songlist is extracted from the current local installation; it is not the upstream `songs.json` snapshot.

The upstream `LICENSE` files contain the following notice:

```text
MIT License

Copyright (c) 2026 RedDragon

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```

## Maintenance-only image libraries

The optional local catalog extractor uses [UnityPy](https://github.com/K0lb3/UnityPy) under MIT and [Pillow](https://github.com/python-pillow/Pillow) under MIT-CMU. Neither dependency is bundled into the browser app. Versions used by the current extractor are pinned in `scripts/requirements-game-catalog.txt`.

## Game content

The local extractor reads the installed game and emits only small 320×320 WebP jacket derivatives required by B50. It does not redistribute original AssetBundles, textures, chart payloads, audio, or video. In Falsus names and jacket artwork remain the property of their respective rights holders; inclusion of a small preview does not grant a license to reuse it elsewhere. This is an unofficial community tool and is not affiliated with the rights holders.
