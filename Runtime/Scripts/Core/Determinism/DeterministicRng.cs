using System;
using UnityEngine;

namespace MysticIsle.DreamEngine
{
    /// <summary>
    /// 可序列化的确定性随机数生成器（xorshift32）。
    /// - 持久化 Seed 与当前 State，读档后可继续同一随机序列。
    /// - 提供常用的取整/浮点/范围/布尔等接口。
    /// </summary>
    [Serializable]
    public sealed class DeterministicRng
    {
        [SerializeField]
        private int seed;
        [SerializeField]
        private uint state; // xorshift32 状态，必须非 0

        /// <summary>
        /// 当前种子（用于重置）。
        /// </summary>
        public int SeedValue => seed;
        /// <summary>
        /// 当前内部状态（调试/显示用）。
        /// </summary>
        public uint StateValue => state;

        public DeterministicRng() { }
        public DeterministicRng(int seed) { Seed(seed); }

        /// <summary>
        /// 以指定种子重置 RNG（开始新序列）。
        /// </summary>
        public void Seed(int newSeed)
        {
            seed = newSeed;
            // 将种子转换为非 0 的初始状态。避免 xorshift 的 0 吸收态。
            unchecked
            {
                uint s = (uint)newSeed;
                if (s == 0) s = 0x9E3779B9u; // 黄金分割常数
                s ^= s << 13; s ^= s >> 17; s ^= s << 5; // 简单搅动
                if (s == 0) s = 0x85EBCA6Bu; // 再保底
                state = s;
            }
        }

        /// <summary>
        /// 确保状态已初始化；如未初始化，则用当前种子（或 TickCount）初始化。
        /// </summary>
        private void EnsureInitialized()
        {
            if (state == 0)
            {
                Seed(seed == 0 ? Environment.TickCount : seed);
            }
        }

        /// <summary>
        /// 生成下一个无符号 32 位随机数，并推进内部状态。
        /// </summary>
        public uint NextUInt()
        {
            EnsureInitialized();
            uint x = state;
            x ^= x << 13;
            x ^= x >> 17;
            x ^= x << 5;
            state = x == 0 ? 0xD1B54A32u : x; // 防止退化到 0
            return state;
        }

        /// <summary>
        /// 返回 [0, max) 的随机整数。
        /// </summary>
        public int NextInt(int max)
        {
            if (max <= 0) return 0;
            return (int)(NextUInt() % (uint)max);
        }

        /// <summary>
        /// 返回 [min, max) 的随机整数。
        /// </summary>
        public int NextInt(int min, int max)
        {
            if (max <= min) return min;
            uint range = (uint)(max - min);
            return (int)(NextUInt() % range) + min;
        }

        /// <summary>
        /// 返回 [0, 1) 的浮点随机数。
        /// </summary>
        public float NextFloat01()
        {
            // 使用 24 位精度避免达到 1.0
            return (NextUInt() >> 8) * (1.0f / (1u << 24));
        }

        /// <summary>
        /// 返回 [0, 1) 的双精度随机数。
        /// </summary>
        public double NextDouble01()
        {
            // 使用 53 位近似：合成 53 位随机（这里简化为 32 位转双精度）
            return (NextUInt() / (double)uint.MaxValue);
        }

        /// <summary>
        /// 在 [min, max) 之间的随机浮点数。
        /// </summary>
        public float NextRange(float min, float max)
        {
            if (max <= min) return min;
            return min + NextFloat01() * (max - min);
        }

        /// <summary>
        /// 按概率返回布尔（probTrue in [0,1]）。
        /// </summary>
        public bool NextBool(float probTrue = 0.5f)
        {
            if (probTrue <= 0) return false;
            if (probTrue >= 1) return true;
            return NextFloat01() < probTrue;
        }
    }
}
