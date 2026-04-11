using System;

namespace BS.Foundation.Ids
{
    /// <summary>
    /// 出生点 ID。
    /// 用于跨场景传送后的玩家落点定位。
    /// </summary>
    [Serializable]
    public readonly struct SpawnPointId : IEquatable<SpawnPointId>
    {
        public string Value => _value;

        [UnityEngine.SerializeField]
        private readonly string _value;

        public SpawnPointId(string value)
        {
            _value = value ?? string.Empty;
        }

        public bool IsValid => !string.IsNullOrWhiteSpace(_value);

        public bool Equals(SpawnPointId other)
        {
            return string.Equals(_value, other._value, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return obj is SpawnPointId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return _value == null ? 0 : StringComparer.Ordinal.GetHashCode(_value);
        }

        public override string ToString()
        {
            return _value;
        }

        public static implicit operator SpawnPointId(string value)
        {
            return new SpawnPointId(value);
        }
    }
}
