using BankerItems.ItemAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using UnityEngine;

namespace BankerItems
{
    [HarmonyPatch]
    public static class CustomAmmoUtility
    {
        public static string AddCustomAmmoType(string name, string fgSpriteObject, string bgSpriteObject, string fgTexture, string bgTexture)
        {
            var fgObj = new GameObject(fgSpriteObject);
            fgObj.SetActive(false);
            UnityEngine.Object.DontDestroyOnLoad(fgObj);
            FakePrefab.MarkAsFakePrefab(fgObj);
            var bgObj = new GameObject(bgSpriteObject);
            bgObj.SetActive(false);
            UnityEngine.Object.DontDestroyOnLoad(bgObj);
            FakePrefab.MarkAsFakePrefab(bgObj);
            dfTiledSprite fgSprite = fgObj.SetupDfSpriteFromTexture<dfTiledSprite>(ResourceExtractor.GetTextureFromResource(fgTexture), ShaderCache.Acquire("Daikon Forge/Default UI Shader"));
            dfTiledSprite bgSprite = bgObj.SetupDfSpriteFromTexture<dfTiledSprite>(ResourceExtractor.GetTextureFromResource(bgTexture), ShaderCache.Acquire("Daikon Forge/Default UI Shader"));
            fgSprite.zindex = 7;
            bgSprite.zindex = 5;
            GameUIAmmoType uiammotype = new GameUIAmmoType()
            {
                ammoBarBG = bgSprite,
                ammoBarFG = fgSprite,
                ammoType = GameUIAmmoType.AmmoType.CUSTOM,
                customAmmoType = name
            };
            addedAmmoTypes.Add(uiammotype);
            if (GameUIRoot.HasInstance)
            {
                foreach (GameUIAmmoController uiammocontroller in GameUIRoot.Instance.ammoControllers)
                {
                    if (uiammocontroller.m_initialized)
                    {
                        uiammocontroller.ammoTypes = uiammocontroller.ammoTypes.AddToArray(uiammotype);
                    }
                }
            }
            return name;
        }

        public static T SetupDfSpriteFromTexture<T>(this GameObject obj, Texture2D texture, Shader shader) where T : dfSprite
        {
            T sprite = obj.GetOrAddComponent<T>();
            dfAtlas atlas = obj.GetOrAddComponent<dfAtlas>();
            atlas.Material = new Material(shader);
            atlas.Material.mainTexture = texture;
            atlas.Items.Clear();
            dfAtlas.ItemInfo info = new dfAtlas.ItemInfo()
            {
                border = new RectOffset(),
                deleted = false,
                name = "main_sprite",
                region = new Rect(Vector2.zero, new Vector2(1, 1)),
                rotated = false,
                sizeInPixels = new Vector2(texture.width, texture.height),
                texture = null,
                textureGUID = "main_sprite"
            };
            atlas.AddItem(info);
            sprite.Atlas = atlas;
            sprite.SpriteName = "main_sprite";
            sprite.zindex = 0;
            return sprite;
        }

        [HarmonyPatch(typeof(GameUIAmmoController), nameof(GameUIAmmoController.Initialize))]
        [HarmonyPostfix]
        public static void AddMissingAmmotypes(GameUIAmmoController __instance)
        {
            __instance.ammoTypes = __instance.ammoTypes.Concat(addedAmmoTypes).ToArray();
        }

        public static List<GameUIAmmoType> addedAmmoTypes = new List<GameUIAmmoType>();
    }
}
