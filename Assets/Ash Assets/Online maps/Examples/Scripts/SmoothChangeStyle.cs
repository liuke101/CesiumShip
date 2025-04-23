using System.Collections.Generic;
using UnityEngine;

namespace InfinityCode.OnlineMapsDemos
{
    /// <summary>
    /// Smoothly transitions between different map styles.
    /// </summary>
    [AddComponentMenu("Infinity Code/Online Maps/Demos/SmoothChangeStyle")]
    public class SmoothChangeStyle : MonoBehaviour
    {
        /// <summary>
        /// (Optional) The control for the Online Maps.
        /// </summary>
        public OnlineMapsTileSetControl control;

        /// <summary>
        /// The duration for the transition.
        /// </summary>
        public float duration = 1;

        // Cache for the map tiles
        private OnlineMapsCache cache;

        // Overlay textures for the map tiles
        private Dictionary<ulong, Texture2D> overlayTextures;

        // Progress of the transition
        private float progress = 1;

        // The target map type for the transition
        private OnlineMapsProvider.MapType targetMapType;

        /// <summary>
        /// Finishes the transition by updating the map type and setting up tile preloading.
        /// </summary>
        private void FinishTransition()
        {
            // Unsubscribe from the OnUpdateBefore event
            control.map.OnUpdateBefore -= FinishTransition;
            
            // Set the new map type
            progress = 1;
            control.map.activeType = targetMapType;
            
            // Subscribe to the OnPreloadTiles event
            OnlineMapsTileManager.OnPreloadTiles += OnPreloadTiles;
        }

        /// <summary>
        /// Handles preloading of tiles.
        /// </summary>
        private void OnPreloadTiles()
        {
            if (overlayTextures == null) return;
            
            // Unsubscribe from the OnPreloadTiles event
            OnlineMapsTileManager.OnPreloadTiles -= OnPreloadTiles;
            
            // Iterate through all tiles
            foreach (OnlineMapsTile tile in control.map.tileManager.tiles)
            {
                // If the texture is in the cache, set it to a tile
                Texture2D texture;
                if (overlayTextures.TryGetValue(tile.key, out texture))
                {
                    tile.texture = texture;
                    tile.status = OnlineMapsTileStatus.loaded;
                }

                tile.overlayBackTexture = null;
            }
            
            overlayTextures = null;
        }

        /// <summary>
        /// Handles completion of WWW requests for map tiles.
        /// </summary>
        private void OnWWWComplete(OnlineMapsWWW www)
        {
            // If there is an error, display it in the console
            if (www.hasError)
            {
                Debug.LogError(www.error);
                return;
            }
            
            if (overlayTextures == null) return;
            
            // Get the tile from the request
            OnlineMapsTile tile = www["tile"] as OnlineMapsTile;
            if (tile == null) return;

            // Create a texture from the downloaded image
            Texture2D texture = new Texture2D(1, 1);
            www.LoadImageIntoTexture(texture);
            
            // Set wrap mode to work around seams between tiles
            texture.wrapMode = TextureWrapMode.Clamp;
            
            // Set the texture as the overlay texture for the tile
            tile.overlayBackTexture = texture;
            
            // Cache the texture to restore it immediately after finishing the transition
            overlayTextures[tile.key] = texture;

            // Add texture to the file cache
            cache.SetTileTexture(tile, targetMapType, texture);
            
            // Redraw the map
            control.map.Redraw();
        }

        /// <summary>
        /// Sets the map style to the specified style name.
        /// </summary>
        public void SetStyle(string styleName)
        {
            // Find the map type
            targetMapType = OnlineMapsProvider.FindMapType(styleName);
            
            // If the map type is not found, display an error message
            if (targetMapType == null)
            {
                Debug.LogError("Can not find map type: " + styleName);
                return;
            }

            // If duration is 0, then immediately change the map type
            // Otherwise, start the transition
            if (duration <= 0) control.map.activeType = targetMapType;
            else StartTransition();
        }

        /// <summary>
        /// Unity's Start method, used for initialization.
        /// </summary>
        private void Start()
        {
            // Initialize the control
            if (control == null)
            {
                control = OnlineMapsTileSetControl.instance;
                if (control == null)
                {
                    Debug.LogError("Can not find OnlineMapsTileSetControl.");
                }
            }

            // Initialize the cache
            cache = OnlineMapsCache.instance;
        }

        /// <summary>
        /// Starts the transition between map styles.
        /// </summary>
        private void StartTransition()
        {
            // Reset the progress
            progress = 0;
            
            // Clear the overlay textures
            overlayTextures = new Dictionary<ulong, Texture2D>();

            // Iterate through all the map tiles
            for (int i = 0; i < control.map.tileManager.tiles.Count; i++)
            {
                // Get the tile
                OnlineMapsTile tile = control.map.tileManager.tiles[i];
                
                // Reset the alpha channel of the overlay texture
                tile.overlayBackAlpha = 0;

                // If cache exists, try to get the texture from it
                if (cache != null)
                {
                    Texture2D texture;
                    if (cache.GetTileTexture(tile, targetMapType, out texture))
                    {
                        // Set the texture to the back overlay
                        tile.overlayBackTexture = texture;
                        
                        // Cache the texture to restore it immediately after finishing the transition
                        overlayTextures[tile.key] = texture;
                        continue;
                    }
                }

                // Reset the overlay texture
                tile.overlayBackTexture = null;
                
                // Create a new WWW request for the tile
                string url = targetMapType.GetURL(tile);
                OnlineMapsWWW www = new OnlineMapsWWW(url);
                
                // Store the tile in the WWW request to use it in the callback
                www["tile"] = tile;
                
                // Subscribe to the completion of the request
                www.OnComplete += OnWWWComplete;
            }
        }

        /// <summary>
        /// Unity's Update method, used for frame-by-frame updates.
        /// </summary>
        private void Update()
        {
            // If the transition is complete, we do not need to do anything.
            if (progress >= 1) return;
            
            // Update the progress of the transition.
            progress += Time.deltaTime / duration;
            
            // If the transition is complete, we need to finish it.
            if (progress >= 1)
            {
                progress = 1;
                
                // Delay the completion of the transition until the next frame.
                // This is necessary to make that changing the map type and initializing the tiles be performed in the same frame.
                control.map.OnUpdateBefore += FinishTransition;
            }
            
            // Update the alpha channel of the overlay textures.
            foreach (OnlineMapsTile tile in control.map.tileManager.tiles)
            {
                tile.overlayBackAlpha = progress;
            }
            
            // Redraw the map.
            control.map.Redraw();
        }
    }
}