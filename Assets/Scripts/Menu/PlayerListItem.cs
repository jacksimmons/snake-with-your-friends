using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Steamworks;
using TMPro;

public class PlayerListItem : MonoBehaviour
{
    public string playerName;
    public int connectionID;
    public ulong steamID;
    private bool _avatarReceived;

    public TextMeshProUGUI nameLabel;
    public RawImage icon;

    protected Callback<AvatarImageLoaded_t> imageLoaded;

    private void Awake()
    {
        if (!SteamManager.Initialized)
        {
            Debug.LogError("SteamManager was not initialised!");
            return;
        }

        imageLoaded = Callback<AvatarImageLoaded_t>.Create(OnImageLoaded);
    }

    private void OnImageLoaded(AvatarImageLoaded_t callback)
    {
        // If the callback is for us (matching ID)
        if (callback.m_steamID.m_SteamID == steamID)
        {
            icon.texture = GetSteamImageAsTexture(callback.m_iImage);
        }
        else
        {
            return;
        }
    }

    private Texture2D GetSteamImageAsTexture(int iImage)
    {
        Texture2D texture = null;

        bool isValid = SteamUtils.GetImageSize(iImage, out uint width, out uint height);
        if (isValid)
        {
            uint size = width * height * 4;
            byte[] image = new byte[size];

            isValid = SteamUtils.GetImageRGBA(iImage, image, (int)size);

            if (isValid)
            {
                texture = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false, true);
                texture.LoadRawTextureData(image);
                texture.Apply();
            }
        }
        _avatarReceived = true;
        return texture;
    }
}
