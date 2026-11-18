# SkiaSharp.FontSubset

[![License](https://img.shields.io/github/license/iDreamsOfGame/SkiaSharp.FontSubset)](https://github.com/iDreamsOfGame/SkiaSharp.FontSubset/blob/master/LICENSE)
[![openupm](https://img.shields.io/npm/v/io.github.idreamsofgame.skiasharp.fontsubset?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/io.github.idreamsofgame.skiasharp.fontsubset)

Font subsetting for Unity, powered by the [HarfBuzz](https://harfbuzz.github.io/) subset API that ships with [Skia for Unity (Standalone)](https://openupm.com/packages/io.github.idreamsofgame.skiaforunity.standalone/) (SkiaSharp / HarfBuzzSharp).

| | |
|---|---|
| **Package name** | `io.github.idreamsofgame.skiasharp.fontsubset` |
| **Unity version** | 2021.3 or newer |
| **License** | [MIT](Assets/SkiaSharp.FontSubset/LICENSE.md) |

## Introduction

Game fonts are usually much larger than they need to be: a full CJK font easily weighs several megabytes, even if your game only ever displays a fixed set of characters. **SkiaSharp.FontSubset** takes a TTF/OTF font and a list of glyphs, and produces a new, much smaller font file that contains exactly those glyphs — ready to be bundled with your build.

Under the hood it calls the native `hb-subset` API through `HarfBuzzSubsetNative` and exposes a small, fluent C# builder API:

- **`FontSubsetBuilder`** — describes and produces the subset:
  - `SetFont(HarfBuzzSharp.Font font)` — use the face of a shaped HarfBuzz font.
  - `SetFace(HarfBuzzSharp.Face face)` — set the source face directly.
  - `AddGlyph(uint glyphId)` — add a single glyph ID.
  - `AddGlyphs(IEnumerable<uint> glyphIds)` — add a collection of glyph IDs.
  - `AddGlyphs(IEnumerable<GlyphInfo> glyphInfos)` — add glyphs from shaped text (uses `GlyphInfo.Codepoint`, which holds the glyph ID after shaping).
  - `Build()` — subsets the face and returns the resulting font file as a `byte[]`.
- **`FontSubsetException`** — thrown when the source face is missing or any native subsetting step fails.

All builder methods (except `Build()`) are fluent and can be chained; duplicate glyph IDs are ignored.

## Supported platforms

The native Skia/HarfBuzz libraries shipped by the **SkiaForUnity.Standalone** dependency only target desktop, so this package supports:

| Platform | Supported |
|---|---|
| Unity Editor (Windows / macOS / Linux) | ✅ |
| Windows standalone (x86 / x64) | ✅ |
| macOS standalone | ✅ |
| Linux standalone (x64) | ✅ |
| iOS / Android / WebGL | ❌ (excluded from compilation; the package won't break builds for these platforms) |

## Installation

> This package depends on [`io.github.idreamsofgame.skiaforunity.standalone`](https://openupm.com/packages/io.github.idreamsofgame.skiaforunity.standalone/) (installed automatically). The instructions below account for it.

### Option 1: Install via Git URL

1. Open your Unity project, then open **Window → Package Manager**.
2. Click the **＋** button in the top-left corner and choose **Add package from git URL...**
3. Enter:

   ```
   https://github.com/iDreamsOfGame/SkiaSharp.FontSubset.git?path=Assets/SkiaSharp.FontSubset
   ```

   To install a specific release, append the tag, e.g.:

   ```
   https://github.com/iDreamsOfGame/SkiaSharp.FontSubset.git?path=Assets/SkiaSharp.FontSubset#upm/1.1.3
   ```

4. **Important:** because the SkiaForUnity.Standalone dependency is distributed through OpenUPM, also add a scoped registry to `Packages/manifest.json` (see the manual OpenUPM setup below) — otherwise Unity cannot resolve the dependency.

### Option 2: Install via OpenUPM

**Using [OpenUPM CLI](https://openupm.com/docs/getting-started-cli) (recommended):**

```bash
openupm add io.github.idreamsofgame.skiasharp.fontsubset
```

The CLI registers the scoped registry and all required scopes automatically.

**Manual setup:** edit `Packages/manifest.json` of your project:

1. Add the OpenUPM scoped registry:

   ```json
   {
     "scopedRegistries": [
       {
         "name": "package.openupm.com",
         "url": "https://package.openupm.com",
         "scopes": [
           "io.github.idreamsofgame.skiasharp.fontsubset",
           "io.github.idreamsofgame.skiaforunity.standalone",
           "org.nuget.system.runtime.compilerservices.unsafe"
         ]
       }
     ]
   }
   ```

2. Add the package to `dependencies`:

   ```json
   {
     "dependencies": {
       "io.github.idreamsofgame.skiasharp.fontsubset": "1.1.3"
     }
   }
   ```

3. Save the file and switch back to the Unity Editor — the package (and its dependency) will be resolved and imported automatically.

## Usage

A typical subsetting workflow has three steps:

1. Load the source font into a `HarfBuzzSharp.Face`.
2. Map the characters you need to **glyph IDs** and add them to a `FontSubsetBuilder`.
3. Call `Build()` to get the subset font file bytes and save them.

```csharp
using System.IO;
using System.Threading.Tasks;
using HarfBuzzSharp;
using SkiaSharp.FontSubset;
using UnityEngine;

public class FontSubsettingExample : MonoBehaviour
{
    [SerializeField]
    private TextAsset sourceFont; // The original .ttf / .otf font file

    private async void Start()
    {
        // 1. Load the source font.
        using var blob = new Blob(sourceFont.bytes);
        using var face = new Face(blob, 0);
        using var font = new Font(face);

        // 2. Map the characters you need to glyph IDs.
        var builder = new FontSubsetBuilder();
        builder.SetFace(face);

        foreach (var character in "Hello, 世界！ABC123")
        {
            if (font.TryGetGlyph((uint)character, out var glyphId))
                builder.AddGlyph(glyphId);
        }

        // 3. Build the subset font and save it to disk.
        byte[] subsetData = builder.Build();
        var outputPath = Path.Combine(Application.streamingAssetsPath, "subset.ttf");
        await File.WriteAllBytesAsync(outputPath, subsetData);

        Debug.Log($"Font subset written to {outputPath}");
    }
}
```

### Notes

- **Glyph IDs, not Unicode code points.** The builder adds *glyph IDs*. For simple 1:1 mappings, `font.TryGetGlyph(unicode, out glyphId)` is enough. If you already shaped your text with HarfBuzzSharp, pass the resulting `GlyphInfo` collection straight to `AddGlyphs(IEnumerable<GlyphInfo>)` — after shaping, `GlyphInfo.Codepoint` contains the glyph ID.
- **Name conflicts.** `HarfBuzzSharp.Font` conflicts with `UnityEngine.Font`; the samples disambiguate with `using Font = HarfBuzzSharp.Font;`.
- **Where to put the output.** Writing the subset into your `Assets/` folder from an Editor script (like the samples do) lets you import it as a regular font asset; writing it to `Application.persistentDataPath` or `StreamingAssets` works for runtime loading.

## Samples

This repository includes example Editor scenes under [`Assets/Samples/`](Assets/Samples/) — `TTFSample.unity` and `OTFSample.unity`. Each scene takes a source font and a target font asset, generates a subset from the text typed into an input field, and saves it back into the project when you press the **Generate** button.

## License

Released under the [MIT License](Assets/SkiaSharp.FontSubset/LICENSE.md).
