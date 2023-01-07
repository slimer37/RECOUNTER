using NaughtyAttributes;
using Recounter.Thumbnails;
using System;
using System.Globalization;
using UnityEngine;

namespace Recounter.Inventory
{
    [CreateAssetMenu(menuName = "Products/Product Library")]
    public class ProductLibrary : ScriptableObject
    {
        [SerializeField] Product[] _products;

        public Product[] Products => _products;
    }

    [Serializable]
    public class Product
    {
        [SerializeField] string _displayName;
        [SerializeField, AllowNesting, Label("Price ($)"), Min(0.01f)] float _price = 1;
        [SerializeField, TextArea(3, 5)] string _description;
        [SerializeField, ShowAssetPreview] GameObject _prefab;

        Texture2D _thumbnail;

        public Texture2D Thumbnail
        {
            get
            {
                if (!_thumbnail)
                    _thumbnail = ThumbnailCreator.CreateThumbnail(_prefab.transform);

                return _thumbnail;
            }
        }

        static readonly CultureInfo EnUsCulture = new("en-US");

        public string DisplayName => _displayName;
        public string Description => _description;
        public float Price => _price;
        public string FormattedPrice => _price.ToString("C", EnUsCulture);
    }
}