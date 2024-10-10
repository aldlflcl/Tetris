using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tetris.Utils
{
    public class PooledObjectFactory
    {
        private readonly int _poolSize;

        private readonly IDictionary<Type, Stack<PooledObject>> _pools = new Dictionary<Type, Stack<PooledObject>>();
        private readonly IDictionary<Type, PooledObject> _pooledObjectPrefabs = new Dictionary<Type, PooledObject>();

        public PooledObjectFactory(int poolSize, IEnumerable<PooledObject> pooledObjects)
        {
            _poolSize = poolSize;
            foreach (var pooledObject in pooledObjects)
            {
                _pooledObjectPrefabs[pooledObject.GetType()] = pooledObject;
            }
        }

        public T Request<T>() where T : PooledObject
        {
            if (!_pools.TryGetValue(typeof(T), out var pool))
            {
                pool = CreatePool<T>();
            }

            if (pool.Count < 0)
            {
                Debug.Log($"{typeof(T).Name} pool is empty, start instantiate");
                Fill<T>(pool);
            }

            var pooledObject = pool.Pop() as T;
            pooledObject.gameObject.SetActive(true);
            return pooledObject;
        }

        public void Return<T>(T pooledObject) where T : PooledObject
        {
            pooledObject.gameObject.SetActive(false);
            _pools[typeof(T)].Push(pooledObject);
        }

        private Stack<PooledObject> CreatePool<T>() where T : PooledObject
        {
            var pool = new Stack<PooledObject>(_poolSize);

            Fill<T>(pool);

            _pools.Add(typeof(T), pool);
            return pool;
        }

        private void Fill<T>(Stack<PooledObject> pool) where T : PooledObject
        {
            for (int i = 0; i < _poolSize; i++)
            {
                pool.Push(CreateInstance<T>());
            }
        }

        private T CreateInstance<T>() where T : PooledObject
        {
            var go = GameObject.Instantiate(_pooledObjectPrefabs[typeof(T)]);
            go.gameObject.hideFlags = HideFlags.HideInHierarchy;
            go.gameObject.SetActive(false);
            go.Init(this);

            return go as T;
        }
    }

    public abstract class PooledObject : MonoBehaviour
    {
        protected PooledObjectFactory _factory;

        public void Init(PooledObjectFactory factory)
        {
            _factory = factory;
        }

        public void Return()
        {
            _factory.Return(this);
        }
    }
}