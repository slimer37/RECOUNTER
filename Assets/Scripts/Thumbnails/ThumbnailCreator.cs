using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Recounter.Thumbnails
{
    public static class ThumbnailCreator
    {
        static ThumbnailCreatorSettings settings;

        static AsyncOperationHandle<ThumbnailCreatorSettings> settingsHandle;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void InitSettings()
        {
            settings = null;

            settingsHandle = Addressables.LoadAssetAsync<ThumbnailCreatorSettings>("ThumbnailCreatorSettings");
            settings = settingsHandle.WaitForCompletion();
            ConfigurePreviewGenerator();
        }

        static void ConfigurePreviewGenerator()
        {
            RuntimePreviewGenerator.PreviewDirection = settings.ViewDirection;
            RuntimePreviewGenerator.Padding = settings.Padding;
            RuntimePreviewGenerator.OrthographicMode = settings.Orthographic;
            RuntimePreviewGenerator.BackgroundColor = settings.BackgroundColor;
            RuntimePreviewGenerator.RenderSupersampling = settings.RenderSupersampling;
            RuntimePreviewGenerator.MarkTextureNonReadable = true;
        }

        public static Texture2D CreateThumbnail(Transform subject)
        {
            if (!subject)
                throw new System.ArgumentNullException(nameof(subject));

            var tex = RuntimePreviewGenerator.GenerateModelPreview(
                subject,
                settings.Resolution.x,
                settings.Resolution.y,
                settings.ShouldCloneModel);

            tex.filterMode = settings.FilterMode;

            return tex;
        }
    }
}
