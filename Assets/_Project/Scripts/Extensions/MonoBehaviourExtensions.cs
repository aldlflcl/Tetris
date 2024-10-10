using UnityEngine;

namespace Tetris.Extensions
{
    public static class MonoBehaviourExtensions
    {
        public static T GetOrAddComponent<T>(this MonoBehaviour target) where T: Component
        {
            if (target.TryGetComponent(typeof(T), out var component))
            {
                return component as T;
            }

            return target.gameObject.AddComponent<T>();
        }
    }
}