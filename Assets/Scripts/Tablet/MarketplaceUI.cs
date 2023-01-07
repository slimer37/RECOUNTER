using Recounter.Inventory;
using UnityEngine;

namespace Recounter.Tablet
{
    public class MarketplaceUI : MonoBehaviour
    {
        [SerializeField] ProductLibrary _productLibrary;
        [SerializeField] ProductListingTile _listingTilePrefab;
        [SerializeField] Transform _tileParent;

        void Awake()
        {
            InitializeTiles();
            _listingTilePrefab.gameObject.SetActive(false);
        }

        void InitializeTiles()
        {
            foreach (var product in _productLibrary.Products)
            {
                var tile = Instantiate(_listingTilePrefab, _tileParent);
                tile.InitializeToProduct(product);
            }
        }
    }
}
