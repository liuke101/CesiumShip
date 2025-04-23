/*           INFINITY CODE           */
/*     https://infinity-code.com     */

#if (!UNITY_WP_8_1 && !UNITY_WEBGL) || UNITY_EDITOR
#define ALLOW_FILECACHE
#elif UNITY_WEBGL && UNITY_2020_2_OR_NEWER
#define ALLOW_FILECACHE
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

#if ALLOW_FILECACHE
using System.IO;
#endif


/// <summary>
/// Class for caching tiles in memory and the file system.
/// </summary>
[AddComponentMenu("Infinity Code/Online Maps/Plugins/Cache")]
[OnlineMapsPlugin("Cache", typeof(OnlineMapsControlBase), true)]
public partial class OnlineMapsCache:MonoBehaviour, IOnlineMapsSavableComponent
{
    private static OnlineMapsCache _instance;
    private static StringBuilder _stringBuilder;

    /// <summary>
    /// Event occurs when loading the tile from the file cache or memory cache.
    /// </summary>
    public Action<OnlineMapsTile> OnLoadedFromCache;

    [Obsolete("Use OnlineMapsTileManager.OnStartDownloadTile")]
    public Action<OnlineMapsTile> OnStartDownloadTile;

    private OnlineMapsSavableItem[] savableItems;

    /// <summary>
    /// The reference to an instance of the cache.
    /// </summary>
    public static OnlineMapsCache instance
    {
        get { return _instance; }
    }

    /// <summary>
    /// Clear all caches.
    /// </summary>
    public void ClearAllCaches()
    {
        ClearMemoryCache();
        ClearFileCache();
    }

    public OnlineMapsSavableItem[] GetSavableItems()
    {
        if (savableItems != null) return savableItems;

        savableItems = new[]
        {
            new OnlineMapsSavableItem("cache", "Cache", SaveSettings)
            {
                loadCallback = LoadSettings
            }
        };

        return savableItems;
    }

    protected static StringBuilder GetStringBuilder()
    {
        if (_stringBuilder == null) _stringBuilder = new StringBuilder();
        else _stringBuilder.Length = 0;

        return _stringBuilder;
    }

    private void LoadSettings(OnlineMapsJSONObject json)
    {
        json.DeserializeObject(this);
    }

    private void OnDestroy()
    {
        OnlineMaps.OnPreloadTiles -= OnPreloadTiles;
        OnlineMapsTileManager.OnLoadFromCache -= OnStartDownloadTileM;
        OnlineMapsTile.OnTileDownloaded -= OnTileDownloaded;
    }

    private void OnDisable()
    {
        if (saveFileCacheAtlasCoroutine != null)
        {
            StopCoroutine(saveFileCacheAtlasCoroutine);
            if (fileCacheAtlas != null) fileCacheAtlas.Save(this);
        }

        if (saveCustomCacheAtlasCoroutine != null)
        {
            StopCoroutine(saveCustomCacheAtlasCoroutine);
            if (customCacheAtlas != null) customCacheAtlas.Save(this);
        }
    }

    private void OnEnable()
    {
        _instance = this;
    }

    private void OnPreloadTiles(OnlineMaps map)
    {
        OnlineMapsTile[] tiles;

        lock (OnlineMapsTile.lockTiles)
        {
            tiles = map.tileManager.tiles.ToArray();
        }

        float start = Time.realtimeSinceStartup;
        for (int i = 0; i < tiles.Length; i++)
        {
            OnlineMapsTile tile = tiles[i];
            if (tile.status != OnlineMapsTileStatus.none || tile.cacheChecked) continue;
            if (!TryLoadFromCache(tile)) tile.cacheChecked = true;
            else if (OnLoadedFromCache != null) OnLoadedFromCache(tile);
            if (Time.realtimeSinceStartup - start > 0.02) return;
        }
    }

    private void OnStartDownloadTileM(OnlineMapsTile tile)
    {
        if (TryLoadFromCache(tile))
        {
            if (OnLoadedFromCache != null) OnLoadedFromCache(tile);
        }
        else
        {
#pragma warning disable 618
            if (OnStartDownloadTile != null) OnStartDownloadTile(tile);
#pragma warning restore 618
            else if (OnlineMapsTileManager.OnStartDownloadTile != null) OnlineMapsTileManager.OnStartDownloadTile(tile);
            else OnlineMapsTileManager.StartDownloadTile(tile);
        }
    }

    private void OnTileDownloaded(OnlineMapsTile tile)
    {
        if (useMemoryCache) AddMemoryCacheItem(tile);
        if (useFileCache) AddFileCacheItem(tile, tile.www.bytes);
    }

    private OnlineMapsJSONItem SaveSettings()
    {
        return OnlineMapsJSON.Serialize(new
        {
            useMemoryCache,
            maxMemoryCacheSize,
            memoryCacheUnloadRate,

            useFileCache,
            maxFileCacheSize,
            fileCacheUnloadRate,
            fileCacheLocation,
            fileCacheCustomPath,
            fileCacheTilePath
        });
    }

    private void Start()
    {
        OnlineMapsTileManager.OnLoadFromCache += OnStartDownloadTileM;
        OnlineMaps.OnPreloadTiles += OnPreloadTiles;
        OnlineMapsTile.OnTileDownloaded += OnTileDownloaded;
    }

    /// <summary>
    /// Base class for cache atlas.
    /// </summary>
    /// <typeparam name="T">Type of cache item</typeparam>
    public abstract class CacheAtlas<T> where T: CacheItem
    {
        /// <summary>
        /// Version of the atlas.
        /// </summary>
        protected const short ATLAS_VERSION = 1;

        /// <summary>
        /// Capacity of the atlas.
        /// </summary>
        protected int capacity = 256;
        
        /// <summary>
        /// Count of items in the atlas.
        /// </summary>
        protected int count = 0;

        /// <summary>
        /// Items of the atlas.
        /// </summary>
        protected T[] items;

        /// <summary>
        /// Name of the atlas.
        /// </summary>
        protected abstract string atlasName { get; }

        /// <summary>
        /// Size of the atlas.
        /// </summary>
        public int size { get; protected set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public CacheAtlas()
        {
            size = 0;
            items = new T[capacity];
        }

        /// <summary>
        /// Checks whether the atlas contains the specified file.
        /// </summary>
        /// <param name="filename">Name of the file</param>
        /// <returns>True - contains, false - not contains.</returns>
        public bool Contains(string filename)
        {
            int hash = filename.GetHashCode();
            for (int i = 0; i < count; i++)
            {
                if (items[i].hash == hash && items[i].key == filename) return true;
            }
            return false;
        }

        /// <summary>
        /// Creates a new item.
        /// </summary>
        /// <param name="filename">Name of the file</param>
        /// <param name="size">Size of the file</param>
        /// <param name="time">Creation time</param>
        /// <returns>Item</returns>
        public abstract T CreateItem(string filename, int size, long time);

        /// <summary>
        /// Deletes old items.
        /// </summary>
        /// <param name="cache">Cache</param>
        public abstract void DeleteOldItems(OnlineMapsCache cache);

        /// <summary>
        /// Loads the atlas from the cache.
        /// </summary>
        /// <param name="cache">Cache</param>
        public void Load(OnlineMapsCache cache)
        {
#if ALLOW_FILECACHE
            StringBuilder builder = cache.GetFileCacheFolder();
            builder.Append("/").Append(atlasName);
            string filename = builder.ToString();

            if (!File.Exists(filename)) return;

            FileStream stream = new FileStream(filename, FileMode.Open);
            BinaryReader reader = new BinaryReader(stream);

            byte c1 = reader.ReadByte();
            byte c2 = reader.ReadByte();

            if (c1 == 'T' && c2 == 'C')
            {
                int cacheVersion = reader.ReadInt16();
                if (cacheVersion > 0)
                {
                    // For future versions
                }
            }
            else stream.Position = 0;

            size = reader.ReadInt32();

            long l = stream.Length;
            while (stream.Position < l)
            {
                filename = reader.ReadString();
                int s = reader.ReadInt32();
                long time = reader.ReadInt64();
                T item = CreateItem(filename, s, time);
                if (capacity <= count)
                {
                    capacity *= 2;
                    Array.Resize(ref items, capacity);
                }
                items[count++] = item;
            }

            reader.Close();
#endif
        }

        /// <summary>
        /// Saves the atlas.
        /// </summary>
        /// <param name="cache">Cache</param>
        public void Save(OnlineMapsCache cache)
        {
#if ALLOW_FILECACHE
            StringBuilder builder = cache.GetFileCacheFolder();
            builder.Append("/").Append(atlasName);
            string filename = builder.ToString();

            FileInfo fileInfo = new FileInfo(filename);
            if (!Directory.Exists(fileInfo.DirectoryName)) Directory.CreateDirectory(fileInfo.DirectoryName);

            T[] itemsCopy = new T[items.Length];
            items.CopyTo(itemsCopy, 0);

#if !UNITY_WEBGL
            OnlineMapsThreadManager.AddThreadAction(() =>
            {
#endif
                try
                {
                    FileStream stream = new FileStream(filename, FileMode.Create);
                    BinaryWriter writer = new BinaryWriter(stream);

                    writer.Write((byte)'T');
                    writer.Write((byte)'C');
                    writer.Write(ATLAS_VERSION);

                    writer.Write(size);

                    for (int i = 0; i < count; i++)
                    {
                        T item = itemsCopy[i];
                        writer.Write(item.key);
                        writer.Write(item.size);
                        writer.Write(item.time);
                    }

                    writer.Close();
                }
                catch
                {
                }
#if !UNITY_WEBGL
        });
#endif
#endif
        }
    }

    /// <summary>
    /// Base class for cache item.
    /// </summary>
    public abstract class CacheItem
    {
        /// <summary>
        /// Size of the item.
        /// </summary>
        public int size;
        
        /// <summary>
        /// Hash of the key.
        /// </summary>
        public int hash;
        
        /// <summary>
        /// Key of the item.
        /// </summary>
        public string key;
        
        /// <summary>
        /// Creation time.
        /// </summary>
        public long time;

        /// <summary>
        /// Creates a new item.
        /// </summary>
        /// <returns>Item</returns>
        public static CacheItem Create()
        {
            return null;
        }
    }

    /// <summary>
    /// Location of the file cache
    /// </summary>
    public enum CacheLocation
    {
        /// <summary>
        /// Application.persistentDataPath
        /// </summary>
        persistentDataPath,
        /// <summary>
        /// Custom
        /// </summary>
        custom
    }
}