﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;
using AsyncImageLoader.Loaders;
using Avalonia.Media.Imaging;

namespace StabilityMatrix.Avalonia;

public readonly record struct ImageLoadFailedEventArgs(string Url, Exception Exception);

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public class FallbackRamCachedWebImageLoader : RamCachedWebImageLoader
{
    private readonly WeakEventManager<ImageLoadFailedEventArgs> loadFailedEventManager = new();
    
    public event EventHandler<ImageLoadFailedEventArgs> LoadFailed
    {
        add => loadFailedEventManager.AddEventHandler(value);
        remove => loadFailedEventManager.RemoveEventHandler(value);
    }
    
    protected void OnLoadFailed(string url, Exception exception) => loadFailedEventManager.RaiseEvent(
        this, new ImageLoadFailedEventArgs(url, exception), nameof(LoadFailed));

    /// <summary>
    /// Attempts to load bitmap
    /// </summary>
    /// <param name="url">Target url</param>
    /// <returns>Bitmap</returns>
    protected override async Task<Bitmap?> LoadAsync(string url)
    {
        // Try to load from local file first
        if (File.Exists(url))
        {
            try
            {
                return new Bitmap(url);
            }
            catch (Exception e)
            {
                OnLoadFailed(url, e);
                return null;
            }
        }
        
        var internalOrCachedBitmap = 
            await LoadFromInternalAsync(url).ConfigureAwait(false)
            ?? await LoadFromGlobalCache(url).ConfigureAwait(false);
        
        if (internalOrCachedBitmap != null) return internalOrCachedBitmap;

        try
        {
            var externalBytes = await LoadDataFromExternalAsync(url).ConfigureAwait(false);
            if (externalBytes == null) return null;

            using var memoryStream = new MemoryStream(externalBytes);
            var bitmap = new Bitmap(memoryStream);
            await SaveToGlobalCache(url, externalBytes).ConfigureAwait(false);
            return bitmap;
        }
        catch (Exception)
        {
            return null;
        }
    }

}
