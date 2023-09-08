using System;
using System.Collections;
using UnityEngine;
using RenderHeads.Media.AVProVideo;  // Make sure AVPro Video is imported

public class AVProHelper : MonoBehaviour
{
    [SerializeField] private MediaPlayer mediaPlayer;  // Reference to the AVPro Media Player
    [SerializeField] private ResolveToRenderTexture resolveToRT;  // Reference to the ResolveToRenderTexture component

    public RenderTexture externalTexture;

    void Start()
    {
        if (mediaPlayer == null || resolveToRT == null)
        {
            Debug.LogError("MediaPlayer and ResolveToRenderTexture must be set.");
            return;
        }

        externalTexture = new RenderTexture(256, 256, 16);
        externalTexture.name = $"{gameObject.name}_RendText";
        LinkToResolveToRenderTexture();
        StartCoroutine(CreateTexture());
    }

    private IEnumerator CreateTexture()
    {
        // Loop until texture is non-null
        while (true)
        {
            Texture texture = mediaPlayer.TextureProducer.GetTexture();

            if (texture != null)
            {
                // Initialize a new RenderTexture
                externalTexture = new RenderTexture((int)(texture.width), (int)(texture.height), 24);  // Depth set to 24 as an example


                // Link to ResolveToRenderTexture


                yield break;  // Exit the loop
            }

            yield return new WaitForSeconds(0.1f);  // Wait for 0.1 seconds before checking again
        }
    }

    // Function to link the external texture to the ResolveToRenderTexture component
    void LinkToResolveToRenderTexture()
    {
        if (resolveToRT != null)
        {
            // Assuming ResolveToRenderTexture has a way to set the target texture
            resolveToRT.ExternalTexture = externalTexture;
            resolveToRT.Resolve();
            Debug.Log($"set external texture for {gameObject.name}");
        }
    }

    // Function to get the external texture for use in other objects
    public RenderTexture GetExternalTexture()
    {
        return externalTexture;
    }

    private void OnDestroy()
    {
        externalTexture.DiscardContents();
        externalTexture = null;
    }
}
