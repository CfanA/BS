using System;

namespace BS.Foundation.Ids
{
    /// <summary>
    /// 轻量的剧情标记 ID。
    /// 对外不直接传裸字符串，减少拼写错误带来的问题。
    /// </summary>
    [Serializable]
    public readonly struct FlagId : IEquatable<FlagId>
    {
        public string Value => _value;

        [UnityEngine.SerializeField]
        private readonly string _value;

        public FlagId(string value)
        {
            _value = value ?? string.Empty;
        }

        public bool IsValid => !string.IsNullOrWhiteSpace(_value);

        public bool Equals(FlagId other)
        {
            return string.Equals(_value, other._value, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return obj is FlagId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return _value == null ? 0 : StringComparer.Ordinal.GetHashCode(_value);
        }

        public override string ToString()
        {
            return _value;
        }

        public static implicit operator FlagId(string value)
        {
            return new FlagId(value);
        }
    }
}
