using System;
using System.Runtime.InteropServices;
using hb_blob_t = System.IntPtr;
using hb_face_t = System.IntPtr;
using hb_set_t = System.IntPtr;
using hb_subset_input_t = System.IntPtr;

namespace SkiaSharp.FontSubset
{
    internal static class HarfBuzzSubsetNative
    {
#if (__IOS__ || __TVOS__ || __WATCHOS__) && !UNITY_EDITOR
		private const string HARFBUZZ = "@rpath/libHarfBuzzSharp.framework/libHarfBuzzSharp";
#else
        private const string HARFBUZZ = "libHarfBuzzSharp";
#endif

        [DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
        internal static extern hb_subset_input_t hb_subset_input_create_or_fail();

        [DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
        internal static extern hb_set_t hb_subset_input_glyph_set(hb_subset_input_t input);

        [DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void hb_set_add(hb_set_t set, uint value);

        [DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
        internal static extern hb_face_t hb_subset_or_fail(hb_face_t source, hb_subset_input_t input);

        [DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
        internal static extern hb_blob_t hb_face_reference_blob(hb_face_t face);

        [DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void hb_subset_input_destroy(hb_subset_input_t input);

        [DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
        internal static extern hb_face_t hb_font_get_face(IntPtr font);
    }
}