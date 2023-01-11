using System;
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
        ListView _productList;

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

            _productPane = new ScrollView(ScrollViewMode.VerticalAndHorizontal);

            _productPane.style.paddingLeft = _productPane.style.paddingRight = 5;
            _productPane.Q("unity-content-container").style.flexShrink = 1;

            CreateProductList();

            var leftPane = new VisualElement();

            var searchButton = new Button(Search)
            {
                text = "Search"
            };

            leftPane.Add(searchButton);
            leftPane.Add(_productList);

            splitView.Add(leftPane);

            splitView.Add(_productPane);

            rootVisualElement.Add(splitView);
        }
        void Search()
        {
            SearchPrompt.Open(_selectedLibrary, i =>
            {
                _productList.selectedIndex = i;
            });
        }

        ListView CreateProductList()
        {
            _productList = new()
            {
                showAlternatingRowBackgrounds = AlternatingRowBackground.ContentOnly,
                showAddRemoveFooter = true
            };

            _productList.makeItem = () =>
            {
                var element = new VisualElement();
                element.Add(new Label());
                element.style.justifyContent = Justify.Center;
                element.style.paddingLeft = 5;
                return element;
            };

            var productsProp = new SerializedObject(_selectedLibrary).FindProperty("_products");

            _productList.bindItem = (item, index) => item.Q<Label>().text = productsProp.GetArrayElementAtIndex(index).FindPropertyRelative("_displayName").stringValue;

            _productList.BindProperty(productsProp);

            _productList.selectionChanged += _ => _selectedIndex = _productList.selectedIndex;
            _productList.selectionChanged += _ => UpdateProductSelection();

            _productList.selectedIndex = _selectedIndex;

            return _productList;
        }

        Image _prefabImage;

        void UpdateProductSelection()
        {
            _productPane.Clear();

            if (_productList.selectedIndex < 0)
            {
                _productPane.Add(new Label("Select a product to edit."));
                return;
            }

            var numProducts = _selectedLibrary.Products.Length;
            if (_productList.selectedIndex == numProducts)
            {
                _productList.selectedIndex = numProducts - 1;
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
            .GetArrayElementAtIndex(_productList.selectedIndex)
            .FindPropertyRelative(fieldName);

        class PriceField : FloatField
        {
            public PriceField(string label) : base(label) { }

            protected override float StringToValue(string str) => base.StringToValue(str.Replace("$", ""));
            protected override string ValueToString(float v) => v.ToString("C");
        }
    }


    public class SearchPrompt : EditorWindow
    {
        static ProductLibrary _library;
        static Action<int> _selected;

        public static void Open(ProductLibrary library, Action<int> selected)
        {
            _library = library;
            _selected = selected;

            var window = CreateInstance<SearchPrompt>();

            var mouse = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
            var rect = new Rect(mouse.x - 450, mouse.y + 10, 10, 10);
            window.ShowAsDropDown(rect, new Vector2(500, 300));
        }

        string value;
        Vector2 scroll;

        void OnGUI()
        {
            EditorGUILayout.BeginHorizontal("Box");

            EditorGUILayout.LabelField("Search: ", EditorStyles.boldLabel);

            value = EditorGUILayout.TextField(value);

            EditorGUILayout.EndHorizontal();

            GetSearchResults();
        }

        void GetSearchResults()
        {
            if (string.IsNullOrWhiteSpace(value)) return;

            EditorGUILayout.BeginVertical();

            scroll = EditorGUILayout.BeginScrollView(scroll);

            var compareType = StringComparison.InvariantCultureIgnoreCase;

            for (var i = 0; i < _library.Products.Length; i++)
            {
                var product = _library.Products[i];

                if (product.DisplayName.Contains(value, compareType))
                {
                    if (GUILayout.Button(product.DisplayName))
                    {
                        _selected.Invoke(i);
                    }
                }
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }
    }
}
