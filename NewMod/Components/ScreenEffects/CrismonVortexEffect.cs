using System;
using Reactor.Utilities.Attributes;
using UnityEngine;

namespace NewMod.Components.ScreenEffects
{
    [RegisterInIl2Cpp]
    public class CrismonVortexEffect(IntPtr ptr) : MonoBehaviour(ptr)
    {
        public Color tint = new Color(1f, 0.05f, 0.03f, 1f);
        public Color highlightTint = new Color(1f, 0.28f, 0.12f, 1f);
        public Color coreTint = new Color(0.75f, 0f, 0f, 1f);
        public float opacity = 0.78f;
        public float tiling = 1.15f;
        public float detailTiling = 2.35f;
        public float detailStrength = 0.45f;
        public float brightness = 1.2f;
        public float contrast = 1.65f;
        public Vector2 scrollDir = new Vector2(1f, 0f);
        public float scrollSpeed = 0.12f;
        public float distortStrength = 0.055f;
        public float distortSpeed = 0.35f;
        public float distortScale = 1.85f;
        public float swirlStrength = 0.95f;
        public float swirlSpeed = 0.55f;
        public float detailSwirlStrength = 1.35f;
        public float detailSwirlSpeed = -0.75f;
        public Vector2 center = new Vector2(0.5f, 0.5f);
        public float radius = 0.95f;
        public float edgeSoftness = 0.26f;
        public float coreRadius = 0.28f;
        public float coreSoftness = 0.16f;
        public float coreIntensity = 1.25f;
        public float pulseStrength = 0.16f;
        public float pulseSpeed = 0.9f;
        public float frontWidth = 0.72f;
        public float frontSoftness = 0.2f;
        public float frontTravel = 0.48f;
        public float leadingEdgeWidth = 0.055f;
        public float leadingEdgeStrength = 1.15f;
        public bool useCircular = false;
        public Material _mat;

        public void OnEnable()
        {
            var shader = NewModAsset.CrismonVortex.LoadAsset();
            var crimTex = NewModAsset.CrismonTexture.LoadAsset();

            if (shader == null || crimTex == null)
            {
                enabled = false;
                return;
            }

            _mat = new Material(shader) { hideFlags = HideFlags.DontSave };
            _mat.SetTexture("_CrimsonTex", crimTex);
        }

        public void OnDisable()
        {
            if (_mat != null)
            {
                Destroy(_mat);
                _mat = null;
            }
        }

        public void OnRenderImage(RenderTexture src, RenderTexture dst)
        {
            _mat.SetColor("_Tint", tint);
            _mat.SetColor("_HighlightTint", highlightTint);
            _mat.SetColor("_CoreTint", coreTint);

            _mat.SetFloat("_Opacity", opacity);
            _mat.SetFloat("_Tiling", tiling);
            _mat.SetFloat("_DetailTiling", detailTiling);
            _mat.SetFloat("_DetailStrength", detailStrength);
            _mat.SetFloat("_Brightness", brightness);
            _mat.SetFloat("_Contrast", contrast);

            _mat.SetVector("_ScrollDir", new Vector4(scrollDir.x, scrollDir.y, 0f, 0f));
            _mat.SetFloat("_ScrollSpeed", scrollSpeed);

            _mat.SetFloat("_DistortStrength", distortStrength);
            _mat.SetFloat("_DistortSpeed", distortSpeed);
            _mat.SetFloat("_DistortScale", distortScale);

            _mat.SetFloat("_SwirlStrength", swirlStrength);
            _mat.SetFloat("_SwirlSpeed", swirlSpeed);
            _mat.SetFloat("_DetailSwirlStrength", detailSwirlStrength);
            _mat.SetFloat("_DetailSwirlSpeed", detailSwirlSpeed);

            _mat.SetVector("_Center", new Vector4(center.x, center.y, 0f, 0f));
            _mat.SetFloat("_Radius", radius);
            _mat.SetFloat("_EdgeSoftness", edgeSoftness);

            _mat.SetFloat("_CoreRadius", coreRadius);
            _mat.SetFloat("_CoreSoftness", coreSoftness);
            _mat.SetFloat("_CoreIntensity", coreIntensity);

            _mat.SetFloat("_PulseStrength", pulseStrength);
            _mat.SetFloat("_PulseSpeed", pulseSpeed);

            _mat.SetFloat("_FrontWidth", frontWidth);
            _mat.SetFloat("_FrontSoftness", frontSoftness);
            _mat.SetFloat("_FrontTravel", frontTravel);

            _mat.SetFloat("_LeadingEdgeWidth", leadingEdgeWidth);
            _mat.SetFloat("_LeadingEdgeStrength", leadingEdgeStrength);

            _mat.SetFloat("_UseCircular", useCircular ? 1f : 0f);

            Graphics.Blit(src, dst, _mat);
        }
    }
}