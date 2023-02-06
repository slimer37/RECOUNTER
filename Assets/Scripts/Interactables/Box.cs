using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem;
using Recounter.Inventory;

public class Box : Interactable
{
    [SerializeField] BoxFlaps _flaps;

    [Header("Holding")]
    [SerializeField] Item _boxItem;

    [Header("Items")]
    [SerializeField] int _capacity;

    [Header("Level Indicator")]
    [SerializeField] Transform _levelIndicator;
    [SerializeField] float _lowestY;
    [SerializeField] float _highestY;
    [SerializeField] float _noiseAmplitude;
    [SerializeField] Vector3 _randomRotation;
    [SerializeField] int _maxRotations;

    [Header("Interface")]
    [SerializeField] InputAction _decreaseIndex;
    [SerializeField] InputAction _increaseIndex;
    [SerializeField] string _instructions;

    [Header("Spinning Preview")]
    [SerializeField] Vector3 _hoverPosition;
    [SerializeField] Vector3 _startRot;
    [SerializeField] Vector3 _spin;
    [SerializeField, Layer] int _spinningItemLayer;

    List<Item> _contents;

    int _selectedItemIndex;

    bool _isHovered;

    Transform _displayedItem;

    Vector3 _spinnerEuler;

    bool _isStoringItem;

    bool _showSpinningPreview;

    void Awake()
    {
        _levelIndicator.gameObject.SetActive(false);
        _contents = new List<Item>();

        _increaseIndex.performed += IncreaseIndex;
        _decreaseIndex.performed += DecreaseIndex;
    }

    public void Fill(List<Item> items)
    {
        if (items.Count > _capacity)
        {
            throw new System.ArgumentOutOfRangeException(nameof(items), "Too many products to fill box.");
        }

        _contents.AddRange(items);

        foreach (var item in items)
        {
            item.gameObject.SetActive(false);
        }

        UpdateLevel();
        SelectLastAddedItem();
        UpdateSpinningItem();
    }

    void IncreaseIndex(InputAction.CallbackContext obj) => ChangeIndex(1);
    void DecreaseIndex(InputAction.CallbackContext obj) => ChangeIndex(-1);

    public override void OnHover(bool hover)
    {
        _isHovered = hover;

        if (hover)
        {
            _decreaseIndex.Enable();
            _increaseIndex.Enable();
        }
        else
        {
            _decreaseIndex.Disable();
            _increaseIndex.Disable();

            _displayedItem?.gameObject.SetActive(false);
        }
    }

    void ChangeIndex(int inc)
    {
        if (_contents.Count == 0) return;

        _selectedItemIndex = (_selectedItemIndex + inc) % _contents.Count;

        if (_selectedItemIndex < 0)
            _selectedItemIndex += _contents.Count;

        UpdateSpinningItem();
    }

    void UpdateSpinningItem()
    {
        if (_contents.Count == 0)
        {
            _displayedItem = null;
            return;
        }

        var newDisplayItem = _contents[_selectedItemIndex].transform;

        if (_displayedItem == newDisplayItem) return;

        if (_displayedItem)
        {
            _displayedItem.gameObject.RestoreHierarchyLayers();
            _displayedItem.gameObject.SetActive(false);
        }

        _displayedItem = newDisplayItem;

        _spinnerEuler = _startRot;

        _displayedItem.gameObject.SetHierarchyLayers(_spinningItemLayer);
    }

    void Update()
    {
        if (!_isHovered || !_displayedItem || !_showSpinningPreview) return;

        if (_isStoringItem) return;

        _displayedItem.position = transform.TransformPoint(_hoverPosition);
        _displayedItem.eulerAngles = _spinnerEuler;
        _spinnerEuler += _spin * Time.deltaTime;
    }

    protected override HudInfo FormHud(Employee e)
    {
        if (!_flaps.FlapsAreOpen)
        {
            _showSpinningPreview = false;
            _displayedItem?.gameObject.SetActive(false);

            return _boxItem.GetHud(e);
        }

        _isStoringItem = e.RightHand.IsFull;

        _showSpinningPreview = !_isStoringItem;
        _displayedItem?.gameObject.SetActive(_showSpinningPreview);

        if (_isStoringItem && _contents.Count == _capacity)
        {
            return new() { icon = Icon.Invalid, text = "Box is full" };
        }

        if (!_isStoringItem && _contents.Count == 0)
        {
            return new() { icon = Icon.Invalid, text = "Box is empty" };
        }

        var hud = new HudInfo() { icon = Icon.Hand };

        if (_isStoringItem)
        {
            hud.icon = Icon.Insert;

            var spacesLeft = _capacity - _contents.Count;
            var spacesText = $"{spacesLeft} space" + (spacesLeft > 1 ? "s" : "");

            hud.text = $"Store {e.RightHand.HeldObject.name}\n{spacesText} left";
        }
        else if (_contents.Count == 1)
        {
            hud.icon = Icon.Extract;
            hud.text = $"Take {_contents[0].name}";
        }
        else
        {
            hud.icon = Icon.Extract;

            var index = _selectedItemIndex;
            hud.text = $"Take {_contents[index].name}\n{index + 1}/{_contents.Count}\n" + _instructions;
        }

        return hud;
    }

    protected override bool CanInteract(Employee e) => !_flaps.Animating && (_flaps.FlapsAreOpen || !e.LeftHand.IsFull);

    protected override void OnInteract(Employee e)
    {
        if (!_flaps.FlapsAreOpen)
        {
            _boxItem.Interact(e);
            return;
        }

        if (_isStoringItem)
        {
            if (_contents.Count == _capacity) return;

            e.ItemHotbar.TryRemoveActiveItem(out var item);

            StoreItem(item);
        }
        else
        {
            if (_contents.Count == 0) return;

            RetrieveItem(e.ItemHotbar);
        }

        UpdateLevel();

        UpdateSpinningItem();
    }

    void StoreItem(Item item)
    {
        _contents.Add(item);

        item.gameObject.SetActive(false);

        SelectLastAddedItem();
    }

    void RetrieveItem(Hotbar hotbar)
    {
        _displayedItem = null;

        var item = _contents[_selectedItemIndex];

        item.transform.position = transform.position;

        hotbar.TryAddItem(item);

        _contents.RemoveAt(_selectedItemIndex);

        ClampSelectedIndex();
    }

    void UpdateLevel()
    {
        _levelIndicator.gameObject.SetActive(_contents.Count > 0);

        var filledAmount = (float)_contents.Count / _capacity;
        _levelIndicator.localPosition = Vector3.up * Mathf.Lerp(_lowestY, _highestY, filledAmount);
        _levelIndicator.localEulerAngles = _randomRotation * Random.Range(0, _maxRotations);
    }

    void SelectLastAddedItem()
    {
        _selectedItemIndex = _contents.Count - 1;
    }

    void ClampSelectedIndex()
    {
        _selectedItemIndex = Mathf.Clamp(_selectedItemIndex, 0, _contents.Count - 1);
    }
}
