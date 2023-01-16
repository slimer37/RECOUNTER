﻿using Recounter.Inventory;
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

        ProductListingTile[] tiles;

        void Awake()
        {
            InitializeTiles();
            _listingTilePrefab.gameObject.SetActive(false);

            _searchField.onSubmit.AddListener(Search);
            _searchButton.onClick.AddListener(Search);
        }

        void Search() => Search(_searchField.text);

        void Search(string query)
        {
            query = query.ToLowerInvariant();

            foreach (var tile in tiles)
            {
                var match = tile.Product.DisplayName.ToLowerInvariant().Contains(query);
                tile.gameObject.SetActive(match);
            }
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
