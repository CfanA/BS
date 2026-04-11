using System;

namespace BS.Foundation.Ids
{
    /// <summary>
    /// 统一的场景 ID。
    /// 对外只传 SceneId，避免到处散落场景名字符串。
    /// </summary>
    [Serializable]
    public readonly struct SceneId : IEquatable<SceneId>
    {
        public string Value => _value;

        [UnityEngine.SerializeField]
        private readonly string _value;

        public SceneId(string value)
        {
            _value = value ?? string.Empty;
        }

        public bool IsValid => !string.IsNullOrWhiteSpace(_value);

        public bool Equals(SceneId other)
        {
            return string.Equals(_value, other._value, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return obj is SceneId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return _value == null ? 0 : StringComparer.Ordinal.GetHashCode(_value);
        }

        public override string ToString()
        {
            return _value;
        }

        public static implicit operator SceneId(string value)
        {
            return new SceneId(value);
        }
    }
}
