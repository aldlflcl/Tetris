using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

namespace Tetris.UnityService
{
    public static class AuthenticationManager
    {
        public static string ProfileName => _profileName;
        private static string _profileName = string.Empty;

        private static bool _isInitialized = false;
        public static bool IsInitialized => _isInitialized;  
        
        #if UNITY_EDITOR

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Init()
        {
            Debug.Log("AuthenticationManager Initialize");
            _isInitialized = false;
            _profileName = string.Empty;
        }
        
        #endif
        
        public static async Task SignInAnonymously(string profileName)
        {
            try
            {
                SwitchProfileWhenSignedIn(profileName);

                await InitializeUnityServices(profileName);

                await AuthenticationService.Instance.SignInAnonymouslyAsync();

                _profileName = profileName;
                
                _isInitialized = true;
                
                Debug.Log($"PlayerId: {AuthenticationService.Instance.PlayerId}");
            }
            catch (AuthenticationException ex)
            {
                Debug.LogException(ex);
            }
        }

        private static void SwitchProfileWhenSignedIn(string profileName)
        {
            try
            {
                if (UnityServices.State == ServicesInitializationState.Initialized)
                {
                    if (AuthenticationService.Instance.IsSignedIn)
                    {
                        AuthenticationService.Instance.SignOut();
                    }

                    AuthenticationService.Instance.SwitchProfile(profileName);
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private static async Task InitializeUnityServices(string profileName)
        {
            try
            {
                var unityAuthenticationOptions = new InitializationOptions();
                Debug.Log(profileName);
                unityAuthenticationOptions.SetProfile(profileName);
                await UnityServices.InitializeAsync(unityAuthenticationOptions);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
    }
}