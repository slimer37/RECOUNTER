using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UIElements;

namespace Recounter.Inventory.Editor
{
    public class ProductLibraryEditor : EditorWindow
    {
        [SerializeField] ProductLibrary _selectedLibrary;

        [SerializeField] int _selectedIndex;

        VisualElement _productPane;

        [OnOpenAsset]
        public static bool OpenAsset(int instanceID, int line)
        {
            if (EditorUtility.InstanceIDToObject(instanceID) is not ProductLibrary)
            {
                return true;
            }

            var window = GetWindow<ProductLibraryEditor>("Product Library Editor");
            window.minSize = new Vector2(450, 200);
            window.maxSize = new Vector2(1920, 720);

            return false;
        }

        void OnEnable()
        {
            if (_selectedLibrary) return;

            _selectedLibrary = Selection.activeObject as ProductLibrary;
        }

        void CreateGUI()
        {
            var splitView = new TwoPaneSplitView(0, 200, TwoPaneSplitViewOrientation.Horizontal);

            var productList = new ListView();

            splitView.Add(productList);

            _productPane = new ScrollView(ScrollViewMode.VerticalAndHorizontal);

            splitView.Add(_productPane);

            Product[] products = _selectedLibrary.Products;

            productList.makeItem = () =>
            {
                var element = new VisualElement();
                element.Add(new Label());
                element.style.justifyContent = Justify.Center;
                element.style.paddingLeft = 5;
                return element;
            };

            productList.bindItem = (item, index) => item.Q<Label>().text = products[index].DisplayName;
            productList.itemsSource = products;

            productList.selectionChanged += ProductList_selectionChanged;

            productList.selectedIndex = _selectedIndex;

            productList.selectionChanged += _ => _selectedIndex = productList.selectedIndex;

            rootVisualElement.Add(splitView);
        }

        void ProductList_selectionChanged(System.Collections.Generic.IEnumerable<object> obj)
        {
            var product = obj.First() as Product;

            if (product == null)
            {
                _productPane.Add(new Label("Select a product to edit."));
                return;
            }

            _productPane.Clear();

            _productPane.Add(new Label(product.DisplayName));
            var field = typeof(Product).GetField("_prefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var prefab = field.GetValue(product) as GameObject;
            var image = new Image();
            image.image = AssetPreview.GetAssetPreview(prefab);
            _productPane.Add(image);
        }
    }
}
