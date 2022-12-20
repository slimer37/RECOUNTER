using System;

public class ConstrainedUndoRedo<T> where T : class
{
    public delegate T TransferState(T src, T dest);

    readonly TransferState _record;
    readonly TransferState _restore;

    readonly int _capacity;

    readonly T[] _stateCache;

    int _index;
    int _availableRedos;
    int _availableUndos;

    public ConstrainedUndoRedo(int capacity, T[] emptyStates, T initialState, TransferState record, TransferState restore)
    {
        if (emptyStates.Length != capacity)
            throw new ArgumentException("Wrong number of states.", nameof(emptyStates));

        _capacity = capacity;
        _stateCache = emptyStates;

        _record = record;
        _restore = restore;

        _stateCache[0] = _record(initialState, _stateCache[_index]);
    }

    public ConstrainedUndoRedo(int capacity, T[] emptyStates, T initialState, TransferState transferState)
        : this(capacity, emptyStates, initialState, transferState, transferState) { }

    public void RecordState(T item)
    {
        _index = (_index + 1) % _capacity;

        _stateCache[_index] = _record(item, _stateCache[_index]);

        if (_availableUndos < _capacity - 1)
            _availableUndos++;

        _availableRedos = 0;
    }

    public bool Undo(T input, out T result)
    {
        result = default;

        if (_availableUndos == 0) return false;

        _index--;

        if (_index < 0)
            _index += _capacity;

        result = _restore(_stateCache[_index], input);

        _availableRedos++;
        _availableUndos--;

        return true;
    }

    public bool Redo(T input, out T result)
    {
        result = default;

        if (_availableRedos == 0) return false;

        _index++;

        if (_index > _capacity - 1)
            _index -= _capacity;

        result = _restore(_stateCache[_index], input);

        _availableRedos--;
        _availableUndos++;

        return true;
    }
}