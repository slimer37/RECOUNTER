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
                return false;
            }

            var window = GetWindow<ProductLibraryEditor>("Product Library Editor");
            window.minSize = new Vector2(450, 200);
            window.maxSize = new Vector2(1920, 720);

            return true;
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
            _productPane.Q("unity-content-container").style.flexShrink = 1;

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
            _productPane.Clear();

            if (obj.First() is not Product product)
            {
                _productPane.Add(new Label("Select a product to edit."));
                return;
            }

            var serializedObject = new SerializedObject(_selectedLibrary);

            var nameProp = GetProductProperty(serializedObject, "_displayName");
            var nameField = CreateTextField(nameProp, "");

            nameField.style.unityFontStyleAndWeight = FontStyle.Bold;

            var priceProp = GetProductProperty(serializedObject, "_price");
            var priceField = CreatePriceField(priceProp, "Price");

            var descProp = GetProductProperty(serializedObject, "_description");
            var descField = CreateTextField(descProp, "Description");

            descField.multiline = true;
            descField.style.minHeight = 40;
            descField.Q("unity-text-input").style.whiteSpace = WhiteSpace.Normal;

            var prefabProp = GetProductProperty(serializedObject, "_prefab");
            var prefabField = CreatePrefabField(prefabProp);

            prefabField.TrackPropertyValue(prefabProp, PrefabChanged);

            _prefabImage = new() { image = AssetPreview.GetAssetPreview(prefabProp.objectReferenceValue) };

            // Construct layout

            _productPane.Add(nameField);
            _productPane.Add(priceField);
            _productPane.Add(descField);
            _productPane.Add(prefabField);
            _productPane.Add(_prefabImage);
        }

        PriceField CreatePriceField(SerializedProperty prop, string label)
        {
            var field = new PriceField(label);

            field.BindProperty(prop);

            return field;
        }

        TextField CreateTextField(SerializedProperty nameProp, string label)
        {
            var field = new TextField(label);

            field.BindProperty(nameProp);

            return field;
        }

        ObjectField CreatePrefabField(SerializedProperty prop)
        {
            var prefabField = new ObjectField("Prefab")
            {
                allowSceneObjects = false,
                objectType = typeof(GameObject),
                value = prop.objectReferenceValue
            };

            prefabField.BindProperty(prop);

            return prefabField;
        }

        void PrefabChanged(SerializedProperty prop)
        {
            _prefabImage.image = AssetPreview.GetAssetPreview(prop.objectReferenceValue);
        }

        SerializedProperty GetProductProperty(SerializedObject serializedObject, string fieldName) => serializedObject
            .FindProperty("_products")
            .GetArrayElementAtIndex(_selectedIndex)
            .FindPropertyRelative(fieldName);

        class PriceField : FloatField
        {
            public PriceField(string label) : base(label) { }

            protected override float StringToValue(string str) => base.StringToValue(str.Replace("$", ""));
            protected override string ValueToString(float v) => v.ToString("C");
        }
    }
}
