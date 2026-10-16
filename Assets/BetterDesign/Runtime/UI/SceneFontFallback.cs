using System;
using System.Text;
using TMPro;
using UnityEngine;
namespace BetterDesign
{
    [ExecuteAlways,DisallowMultipleComponent]
    public sealed class SceneFontFallback : MonoBehaviour,ITextPreprocessor
    {
        [SerializeField] TMP_FontAsset primary,common,latin;
        [SerializeField] TMP_Text[] labels;
        readonly StringBuilder buffer = new StringBuilder(2048);
        bool bound;
        public bool IsConfigured { get { return primary != null && common != null && latin != null && labels != null && labels.Length > 0; } }
        void OnEnable() { Bind(); }
        public void Bind()
        {
            Unbind(); if (!IsConfigured) return;
            TMP_Text.OnFontAssetRequest += Resolve;
            bound = true;
            for (int i = 0; i < labels.Length; i++) if (labels[i] != null) { labels[i].richText = true; labels[i].textPreprocessor = this; labels[i].SetAllDirty(); }
        }
        TMP_FontAsset Resolve(int hash,string fontName)
        {
            if (fontName == common.name) return common;
            return fontName == latin.name ? latin : null;
        }
        public bool Supports(char value) { return char.IsControl(value) || primary.HasCharacter(value) || common.HasCharacter(value) || latin.HasCharacter(value); }
        public string PreprocessText(string text)
        {
            if (!IsConfigured || string.IsNullOrEmpty(text)) return text;
            buffer.Clear(); TMP_FontAsset active = null;
            for (int i = 0; i < text.Length; i++)
            {
                char value = text[i];
                if (value == '<')
                {
                    int end = text.IndexOf('>',i);
                    if (end >= 0) { buffer.Append(text,i,end-i+1); i = end; continue; }
                }
                TMP_FontAsset next = char.IsControl(value) || primary.HasCharacter(value) ? null : common.HasCharacter(value) ? common : latin.HasCharacter(value) ? latin : null;
                if (next != active)
                {
                    if (active != null) buffer.Append("</font>");
                    if (next != null) buffer.Append("<font=\"").Append(next.name).Append("\">");
                    active = next;
                }
                buffer.Append(value);
            }
            if (active != null) buffer.Append("</font>");
            return buffer.ToString();
        }
        void Unbind()
        {
            if (bound) TMP_Text.OnFontAssetRequest -= Resolve;
            bound = false;
            if (labels == null) return;
            for (int i = 0; i < labels.Length; i++)
                if (labels[i] != null && ReferenceEquals(labels[i].textPreprocessor,this)) labels[i].textPreprocessor = null;
        }
        void OnDisable() { Unbind(); }
        void OnDestroy() { Unbind(); }
    }
}
