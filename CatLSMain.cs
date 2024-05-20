using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace CatLSMod
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class CatLSMain : BaseUnityPlugin
    {
        public const string pluginGuid = "com.Tipe.catlsm";
        public const string pluginName = "Cat LS Mod";
        public const string pluginVersion = "0.0.0";
        
        internal static new ManualLogSource Logger;
        public static ConfigEntry<string> CatApiKey { get; private set; }
        
        void Awake()
        {
            CatLSMain.Logger = base.Logger;
            CatLSMain.Logger.LogInfo("Enjoy the cats :D");
            
            CatApiKey = Config.Bind("General", "ApiKey", "", "API Key for thecatapi.com");
            CatLoading.CatApiKey = CatApiKey.Value;
            
            CatLoading.Instance.Init();
        }
    }
}