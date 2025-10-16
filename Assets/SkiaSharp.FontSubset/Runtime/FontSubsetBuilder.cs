using System;
using System.Collections.Generic;
using System.IO;
using HarfBuzzSharp;
using Font = HarfBuzzSharp.Font;

namespace SkiaSharp.FontSubset
{
    public class FontSubsetBuilder
    {
        private IntPtr faceHandle;

        private readonly HashSet<uint> _glyphCodepoints = new();

        public FontSubsetBuilder SetFont(Font font)
        {
            var face = HarfBuzzSubsetNative.hb_font_get_face(font.Handle);
            if (face == IntPtr.Zero)
                throw new FontSubsetException("Failed to get face from font.");

            faceHandle = face;
            return this;
        }

        public FontSubsetBuilder SetFace(Face face)
        {
            faceHandle = face.Handle;
            return this;
        }

        public FontSubsetBuilder AddGlyph(uint glyphId)
        {
            _glyphCodepoints.Add(glyphId);
            return this;
        }

        public FontSubsetBuilder AddGlyphs(IEnumerable<uint> glyphIds)
        {
            foreach (var cp in glyphIds)
                _glyphCodepoints.Add(cp);
            return this;
        }

        public FontSubsetBuilder AddGlyphs(IEnumerable<GlyphInfo> glyphInfos)
        {
            foreach (var info in glyphInfos)
                _glyphCodepoints.Add(info.Codepoint);
            return this;
        }

        public byte[] Build()
        {
            if (faceHandle == IntPtr.Zero)
                throw new FontSubsetException("Face must be set before building subset.");

            var subsetInput = HarfBuzzSubsetNative.hb_subset_input_create_or_fail();
            if (subsetInput == IntPtr.Zero)
                throw new FontSubsetException("Failed to create subset input.");

            try
            {
                var glyphSet = HarfBuzzSubsetNative.hb_subset_input_glyph_set(subsetInput);
                if (glyphSet == IntPtr.Zero)
                    throw new FontSubsetException("Failed to get glyph set from subset input.");

                foreach (var codepoint in _glyphCodepoints)
                    HarfBuzzSubsetNative.hb_set_add(glyphSet, codepoint);

                var subsetFacePtr = HarfBuzzSubsetNative.hb_subset_or_fail(faceHandle, subsetInput);
                if (subsetFacePtr == IntPtr.Zero)
                    throw new FontSubsetException("Failed to subset the face.");

                var faceType = typeof(Face);
                var faceConstructor = faceType.GetConstructor(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
                    null,
                    new[] { typeof(IntPtr) },
                    null);
                if (faceConstructor == null)
                    throw new FontSubsetException("Failed to get Face constructor.");
                using var subsetFace = (Face)faceConstructor.Invoke(new object[] { subsetFacePtr });

                var subsetBlobPtr = HarfBuzzSubsetNative.hb_face_reference_blob(subsetFacePtr);
                if (subsetBlobPtr == IntPtr.Zero)
                    throw new FontSubsetException("Failed to reference blob from subset face.");

                var blobType = typeof(Blob);
                var blobConstructor = blobType.GetConstructor(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
                    null,
                    new[] { typeof(IntPtr) },
                    null);
                if (blobConstructor == null)
                    throw new FontSubsetException("Failed to get Blob constructor.");
                using var subsetBlob = (Blob)blobConstructor.Invoke(new object[] { subsetBlobPtr });

                using var stream = subsetBlob.AsStream();
                using var ms = new MemoryStream();
                stream.CopyTo(ms);
                return ms.ToArray();
            }
            finally
            {
                HarfBuzzSubsetNative.hb_subset_input_destroy(subsetInput);
            }
        }
    }
}