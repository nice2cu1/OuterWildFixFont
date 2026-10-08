using System;
using UnityEngine;
using UnityEngine.UI;

namespace OuterWildFixFont
{
    // 日志中的每个文本独立使用字形贴图，避免共用贴图超过容量
    public sealed class ShipLogTextFont : MonoBehaviour
    {
        private Text _text;
        private Font _font;
        private Material _material;
        private Texture _texture;
        private int _ownerId;
        private bool _failed;
        private bool _useConfiguredSize;
        private int _originalFontSize;
        private string _completeText;
        private int _largestTextLength = 512;
        private float _rasterPixels = 64;
        private float _originalDensity = -1;
        private int _cachedFontSize;
        private float _samplingDensity;

        internal void Initialize(Text text, bool useConfiguredSize)
        {
            _text = text;
            _useConfiguredSize = useConfiguredSize;
            _originalFontSize = text.fontSize;
            OuterWildFixFont.RegisterShipLogTextFont(text, this);
        }

        internal bool Owns(Font font) => _font && font == _font && _ownerId == GetInstanceID();

        internal void SetText(string completeText)
        {
            if (_completeText == completeText) return;
            _completeText = completeText;
            // 保留历史上限，避免复用条目时切换采样档位让旧字形不断占用贴图
            int length = completeText?.Length ?? 0;
            if (length <= _largestTextLength) return;
            _largestTextLength = length;
            float budget = Mathf.Sqrt(4096f * 4096f * 0.25f / _largestTextLength);
            _rasterPixels = budget >= 64 ? 64 : budget >= 48 ? 48 : budget >= 32 ? 32 : 16;
            _originalDensity = -1;
            if (_text) _text.SetAllDirty();
        }

        internal float SamplingDensity(float original)
        {
            if (_originalDensity == original && _cachedFontSize == _text.fontSize) return _samplingDensity;
            _originalDensity = original;
            _cachedFontSize = _text.fontSize;
            _samplingDensity = Mathf.Min(original * 2f, _rasterPixels / Math.Max(1, _cachedFontSize));
            return _samplingDensity;
        }

        internal void Apply()
        {
            if (!_text || !_text.isActiveAndEnabled) return;
            if (!OuterWildFixFont.IsChinese)
            {
                if (Owns(_text.font))
                {
                    _text.font = TextTranslation.GetFont(false);
                    if (_useConfiguredSize) _text.fontSize = _originalFontSize;
                }
                return;
            }
            if (_failed) return;
            if (!_font || _ownerId != GetInstanceID())
            {
                try
                {
                    _font = OuterWildFixFont.CreateShipLogTextFont();
                    _ownerId = GetInstanceID();
                    _material = _font.material;
                    _texture = _material.mainTexture;
                }
                catch (Exception exception)
                {
                    _failed = true;
                    Debug.LogError($"[OuterWildFixFont] Failed to create a ship-log atlas: {exception.Message}");
                    return;
                }
            }
            if (_text.font != _font) _text.font = _font;
            if (_useConfiguredSize && _text.fontSize != OuterWildFixFont.ShipLogFontSize)
                _text.fontSize = OuterWildFixFont.ShipLogFontSize;
        }

        private void LateUpdate()
        {
            // 只检查当前组件，不再每帧遍历列表或查找组件
            Apply();
        }

        private void OnDestroy()
        {
            OuterWildFixFont.UnregisterShipLogTextFont(_text, this);
            if (_ownerId != GetInstanceID()) return;
            // 动态贴图扩容后可能已经替换了初始纹理
            var currentTexture = _material ? _material.mainTexture : null;
            if (currentTexture && currentTexture != _texture) Destroy(currentTexture);
            if (_texture) Destroy(_texture);
            if (_material) Destroy(_material);
            if (_font) Destroy(_font);
        }
    }
}
