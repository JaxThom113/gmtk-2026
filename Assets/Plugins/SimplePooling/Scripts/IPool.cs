using UnityEngine;

namespace Sezylrin.SimplePooling
{
    internal interface IPool
    {
        internal void Release(ReferenceMap reference);
        public void Clear();

        public int CountAll { get; }
        public int CountActive { get; }
        public int CountInactive { get; }

        public void ResetCount();
    }
}
