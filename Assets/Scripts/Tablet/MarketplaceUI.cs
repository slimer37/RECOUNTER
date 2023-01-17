using Recounter.Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Recounter.Tablet
{
    public class MarketplaceUI : MonoBehaviour
    {
        [SerializeField] ProductLibrary _productLibrary;
        [SerializeField] ProductListingTile _listingTilePrefab;
        [SerializeField] Transform _tileParent;

        [Header("Searching")]
        [SerializeField] TMP_InputField _searchField;
        [SerializeField] Button _searchButton;
        [SerializeField] Page _page;
        [SerializeField] GameObject _emptySearchMessage;
        [SerializeField] Tablet _tablet;

        ProductListingTile[] tiles;

        void Start()
        {
            InitializeTiles();
            _listingTilePrefab.gameObject.SetActive(false);

            _searchField.onSubmit.AddListener(Search);
            _searchButton.onClick.AddListener(Search);

            // Disable tablet while typing to prevent "T" putting down the tablet.
            _searchField.onSelect.AddListener(_ => DisableTablet());
            _searchField.onTextSelection.AddListener((_, _, _) => DisableTablet());

            _searchField.onEndEdit.AddListener(_ => EnableTablet());
        }

        void EnableTablet()
        {
            if (!_tablet) return;
            _tablet.enabled = true;
        }

        void DisableTablet() => _tablet.enabled = false;

        void Search() => Search(_searchField.text);

        void Search(string query)
        {
            _page.Open();

            query = query.ToLowerInvariant();

            var noneFound = true;

            foreach (var tile in tiles)
            {
                var match = tile.Product.DisplayName.ToLowerInvariant().Contains(query);
                tile.gameObject.SetActive(match);

                if (match)
                    noneFound = false;
            }

            _emptySearchMessage.SetActive(noneFound);
        }

        void InitializeTiles()
        {
            var allProducts = _productLibrary.Products;

            tiles = new ProductListingTile[allProducts.Length];

            for (var i = 0; i < allProducts.Length; i++)
            {
                var tile = Instantiate(_listingTilePrefab, _tileParent);
                tile.InitializeToProduct(allProducts[i]);
                tiles[i] = tile;
            }
        }
    }
}
