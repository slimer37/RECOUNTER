using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Recounter.Inventory.Editor
{
    public class ProductLibraryEditor : EditorWindow
    {
        [SerializeField] ProductLibrary _selectedLibrary;

        [SerializeField] int _selectedIndex;

        VisualElement _productPane;
        ToolbarSearchField _searchField;

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

            _productPane.style.paddingLeft = 5;

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

            productList.selectionChanged += _ => _selectedIndex = productList.selectedIndex;
            productList.selectionChanged += ProductList_selectionChanged;

            productList.selectedIndex = _selectedIndex;

            _searchField = new ToolbarSearchField();

            rootVisualElement.Add(_searchField);

            rootVisualElement.Add(splitView);
        }

        Image _prefabImage;

        void ProductList_selectionChanged(IEnumerable<object> obj)
        {
            if (obj.First() is not Product product)
            {
                _productPane.Add(new Label("Select a product to edit."));
                return;
            }

            var serializedObject = new SerializedObject(_selectedLibrary);

            _productPane.Clear();

            var label = new Label(product.DisplayName);

            var prefabProp = GetProductProperty(serializedObject, "_prefab");

            var prefabField = new ObjectField("Prefab")
            {
                allowSceneObjects = false,
                objectType = typeof(GameObject),
                value = prefabProp.objectReferenceValue
            };

            prefabField.BindProperty(prefabProp);

            prefabField.TrackPropertyValue(prefabProp, UpdatePrefabImage);

            _prefabImage = new()
            {
                image = AssetPreview.GetAssetPreview(prefabProp.objectReferenceValue)
            };

            _productPane.Add(label);
            _productPane.Add(prefabField);
            _productPane.Add(_prefabImage);
        }

        void UpdatePrefabImage(SerializedProperty prop)
        {
            _prefabImage.image = AssetPreview.GetAssetPreview(prop.objectReferenceValue);
        }

        SerializedProperty GetProductProperty(SerializedObject serializedObject, string fieldName) => serializedObject
            .FindProperty("_products")
            .GetArrayElementAtIndex(_selectedIndex)
            .FindPropertyRelative(fieldName);
    }
}
