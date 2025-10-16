using System;
using System.Collections.Generic;
using System.IO;
using HarfBuzzSharp;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Font = HarfBuzzSharp.Font;

namespace SkiaSharp.FontSubset.Samples
{
    internal class OTFSample : MonoBehaviour
    {
        [SerializeField]
        private UnityEngine.Font sourceFont;

        [SerializeField]
        private UnityEngine.Font subsetFont;

        [SerializeField]
        private InputField inputField;

        [SerializeField]
        private Button generateButton;

        private void Awake()
        {
            generateButton.onClick.AddListener(OnGenerateButtonClicked);
        }

        private void OnDestroy()
        {
            generateButton.onClick.RemoveListener(OnGenerateButtonClicked);
        }

        private async void OnGenerateButtonClicked()
        {
            try
            {
                var assetFullPath = Path.Combine(Directory.GetCurrentDirectory(), AssetDatabase.GetAssetPath(sourceFont));
                var fontBlob = Blob.FromFile(assetFullPath);
                var fontFace = new Face(fontBlob, 0);
                var font = new Font(fontFace);
                var builder = new FontSubsetBuilder();
                builder.SetFace(fontFace);

                var chars = inputField.text.ToCharArray();
                var glyphs = new HashSet<uint>();
                if (chars.Length == 0)
                {
                    Debug.LogWarning("No glyph to add in font subset file!");
                    return;
                }

                foreach (var ch in chars)
                {
                    var unicode = (uint)ch;
                    if (font.TryGetGlyph(unicode, out var glyph))
                        glyphs.Add(glyph);
                }

                // Adds glyphs
                if (glyphs.Count == 0)
                {
                    Debug.LogWarning("No glyph to add in font subset file!");
                    return;
                }

                builder.AddGlyphs(glyphs);

                var subsetFontFullPath = Path.Combine(Directory.GetCurrentDirectory(), AssetDatabase.GetAssetPath(subsetFont));
                Debug.Log("Generating font subset file...");
                await File.WriteAllBytesAsync(subsetFontFullPath, builder.Build());
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
                Debug.Log("Font subset file generated!");
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }
    }
}