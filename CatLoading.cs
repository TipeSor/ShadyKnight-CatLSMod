using BepInEx;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;

namespace CatLSMod
{
    public class CatLoading : MonoBehaviour
    {
        private static CatLoading _instance;

        public static CatLoading Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<CatLoading>();
                    if (_instance == null)
                    {
                        GameObject catObject = new GameObject("CatLoading");
                        _instance = catObject.AddComponent<CatLoading>();
                        DontDestroyOnLoad(catObject);
                    }
                }
                return _instance;
            }
        }

        public static string CatApiKey = "";

        private CatImage _catImage = new CatImage();
        private bool _showingCat;

        private string imagesFolderPath = Paths.PluginPath + "/Cats";

        public void Init()
        {
            Game.OnPlayableLevelLoaded += s => GetCat();
            Loading.OnLoadingStart += ShowCat;
            Loading.OnLoadingEnd += HideCat;
        }

        public void GetCat()
        {
            if (CatApiKey != "")
            {
                StartCoroutine(GetCatImage());
            }
        }

        private void ShowCat()
        {
            _showingCat = true;
        }

        private void HideCat()
        {
            _showingCat = false;
        }

        private IEnumerator GetCatImage()
        {
            string url = "https://api.thecatapi.com/v1/images/search";
            if (!string.IsNullOrEmpty(CatApiKey))
            {
                url += "?api_key=" + CatApiKey;
            }

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                yield return request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.Success)
                {
                    CatLSMain.Logger.LogError(request.error);
                }
                else
                {
                    CatLSMain.Logger.LogInfo("Got cat image URL");

                    string json = request.downloadHandler.text;
                    CatLSMain.Logger.LogInfo($"Received JSON: {json}");

                    // Remove the square brackets
                    if (json.StartsWith("[") && json.EndsWith("]"))
                    {
                        json = json.Substring(1, json.Length - 2);
                    }

                    Cat cat = JsonUtility.FromJson<Cat>(json);
                    if (cat != null)
                    {
                        string imagePath = imagesFolderPath + cat.id + ".png";
                        if (File.Exists(imagePath))
                        {
                            LoadImage(imagePath);
                            yield break;
                        }

                       yield return StartCoroutine(DownloadCatImage(cat));
                    }
                    else
                    {
                        CatLSMain.Logger.LogError("Failed to parse cat JSON.");
                    }
                }
            }
        }

        private void LoadImage(string imagePath)
        {
            
        }

        private IEnumerator DownloadCatImage(Cat cat)
        {
            if (!cat.url.EndsWith(".gif"))
            {
                using (UnityWebRequest imageRequest = UnityWebRequestTexture.GetTexture(cat.url))
                {
                    yield return imageRequest.SendWebRequest();

                    if (imageRequest.result != UnityWebRequest.Result.Success)
                    {
                        CatLSMain.Logger.LogError(imageRequest.error);
                    }
                    else
                    {
                        CatLSMain.Logger.LogInfo("Got cat image");

                        Texture2D texture = ((DownloadHandlerTexture)imageRequest.downloadHandler).texture;
                        if (texture != null)
                        {
                            _catImage = new CatImage
                            {
                                image = texture,
                                width = cat.width,
                                height = cat.height
                            };

                            string imagePath = imagesFolderPath + cat.id + ".png";
                            SaveImageToFile(texture, imagePath);

                            if (!Loading.loading.isDone) _showingCat = true;
                        }
                        else
                        {
                            CatLSMain.Logger.LogError("Failed to load texture.");
                        }
                    }
                }
            }
        }

        private void SaveImageToFile(Texture2D texture, string imagePath)
        {
            byte[] bytes = texture.EncodeToPNG();
            File.WriteAllBytes(imagePath, bytes);
        }

        private void OnGUI()
        {
            if (_showingCat)
            {
                if (_catImage != null)
                {
                    float catWidth = Mathf.Min(_catImage.width, Screen.width / 2);
                    float catHeight = _catImage.height * (catWidth / _catImage.width);
                    Rect catRect = new Rect((Screen.width - catWidth) / 2, (Screen.height - catHeight) / 2, catWidth, catHeight);
                    GUI.DrawTexture(catRect, _catImage.image, ScaleMode.ScaleToFit);
                }
            }
        }
    }

    [System.Serializable]
    public class Cat
    {
        public string id;
        public string url;
        public int width;
        public int height;
    }

    [System.Serializable]
    public class CatImage
    {
        public Texture2D image;
        public float width;
        public float height;
    }
}
