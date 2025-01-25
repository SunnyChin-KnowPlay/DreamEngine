using UnityEngine;

namespace MysticIsle.DreamEngine.Pool
{
    [System.Serializable]
    internal struct DelayedRecycleItem
    {
        public GameObject Object;
        public float RecycleTime;

        public DelayedRecycleItem(GameObject obj, float time)
        {
            Object = obj;
            RecycleTime = time;
        }
    }
}
