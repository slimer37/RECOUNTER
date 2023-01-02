using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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
    [SerializeField] string instructions;

    List<Item> _contents;

    int _selectedItemIndex;

    void Awake()
    {
        _levelIndicator.gameObject.SetActive(false);
        _contents = new List<Item>();

        _increaseIndex.performed += IncreaseIndex;
        _decreaseIndex.performed += DecreaseIndex;
    }

    void IncreaseIndex(InputAction.CallbackContext obj) =>
        _selectedItemIndex = (_selectedItemIndex + 1) % _contents.Count;

    void DecreaseIndex(InputAction.CallbackContext obj) =>
        _selectedItemIndex = (_contents.Count + _selectedItemIndex - 1) % _contents.Count;

    public override void OnHover(bool hover)
    {
        if (hover)
        {
            _decreaseIndex.Enable();
            _increaseIndex.Enable();
        }
        else
        {
            _decreaseIndex.Disable();
            _increaseIndex.Disable();
        }
    }

    protected override HudInfo FormHud(Employee e)
    {
        if (!_flaps.FlapsAreOpen) return _boxItem.GetHud(e);

        var isStoringItem = e.RightHand.IsFull;

        if (isStoringItem && _contents.Count == _capacity)
        {
            return new() { icon = Icon.Invalid, text = "Box is full" };
        }

        if (!isStoringItem && _contents.Count == 0)
        {
            return new() { icon = Icon.Invalid, text = "Box is empty" };
        }

        var hud = new HudInfo() { icon = Icon.Hand };

        var spacesLeft = _capacity - _contents.Count;
        var spacesText = $"{spacesLeft} space" + (spacesLeft > 1 ? "s" : "");

        if (isStoringItem)
        {
            hud.text = $"Store {e.RightHand.HeldObject.name}\n{spacesText} left";
        }
        else if (_contents.Count == 1)
        {
            hud.text = $"Take {_contents[0].name}";
        }
        else
        {
            var index = _selectedItemIndex;
            hud.text = $"Take {_contents[index].name}\n{index + 1}/{_contents.Count}\n" + instructions;
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

        var isStoringItem = e.RightHand.IsFull;

        if (isStoringItem)
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

        ClampSelectedIndex();

        UpdateLevel();
    }

    void StoreItem(Item item)
    {
        _contents.Add(item);

        item.gameObject.SetActive(false);
    }

    void RetrieveItem(Hotbar hotbar)
    {
        var item = _contents[_selectedItemIndex];

        item.transform.position = transform.position;

        hotbar.TryAddItem(item);

        _contents.RemoveAt(_selectedItemIndex);
    }

    void UpdateLevel()
    {
        _levelIndicator.gameObject.SetActive(_contents.Count > 0);

        var filledAmount = (float)_contents.Count / _capacity;
        _levelIndicator.localPosition = Vector3.up * Mathf.Lerp(_lowestY, _highestY, filledAmount);
        _levelIndicator.localEulerAngles = _randomRotation * Random.Range(0, _maxRotations);
    }

    void ClampSelectedIndex()
    {
        _selectedItemIndex = Mathf.Clamp(_selectedItemIndex, 0, _contents.Count - 1);
    }
}
