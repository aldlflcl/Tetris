using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;

namespace Tetris.NetworkTest
{
    public class SyncedEventExample : NetworkBehaviour
    {
        public GameObject Effect;

        [ContextMenu("Start Test")]
        public void ClientCreateSyncedEffect()
        {
            Assert.IsTrue(IsOwner);
            var time = NetworkManager.LocalTime.Time;
            CreateSyncedEffectServerRpc(time);
            StartCoroutine(WaitAndSpawnSyncedEffect(0));
        }

        [Rpc(SendTo.Server)]
        private void CreateSyncedEffectServerRpc(double time)
        {
            Debug.Log("Received Rpc");
            CreateSyncedEffectClientRpc(time);
        }

        private IEnumerator WaitAndSpawnSyncedEffect(float timeToWait)
        {
            if (timeToWait > 0)
            {
                yield return new WaitForSeconds(timeToWait);
            }

            Instantiate(Effect, transform.position, Quaternion.identity);
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void CreateSyncedEffectClientRpc(double time)
        {
            if (IsOwner == false)
            {
                Debug.Log(OwnerClientId);
                var timeToWait = time - NetworkManager.ServerTime.Time;
                StartCoroutine(WaitAndSpawnSyncedEffect((float)timeToWait));
            }
        }
    }
}