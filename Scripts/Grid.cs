using System.Collections.Generic;
using Godot;
using Match3.Extends;

namespace Match3;

public partial class Grid : Node2D {
	[Export]
	public GridContainer _gridContainer;

	[Export]
	public PackedScene _slotPrefab;

	[Export]
	public PackedScene _gemPrefab;

	[Export]
	public Timer _timer;

	public Slot[,] _slots;

	public Slot _slot;

	public Vector2 _slotSize;

	public override void _Ready() {
		_CreateSlots();

		_timer.Timeout += FillGrid;
		// _timer.Timeout += () => {
		// 	SpawnGem(0, 0);
		// };
	}

	public override void _Input(InputEvent @event) {
		var isMouseLeftUp = @event is InputEventMouseButton {
			Pressed: false,
			ButtonIndex: MouseButton.Left
		};

		if (isMouseLeftUp) {
			_slot = null;
		}
	}

	public void _CreateSlots() {
		var width = _gridContainer.Columns;
		var amount = width * width;

		_slots = new Slot[width, width];

		for (var i = 0; i < amount; i++) {
			var field = new Vector2I {
				X = i % width,
				Y = i / width
			};

			var slot = _slotPrefab.Instantiate<Slot>();
			slot.Setup(this, field);

			_slots[field.X, field.Y] = slot;
			_gridContainer.AddChild(slot);
		}

		_slotSize = _slots[0, 0].CustomMinimumSize;
	}

	public void FillGrid() {
		var width = _gridContainer.Columns;
		var column = 0;

		while (column < width) {
			TryFillColumn(column);

			column += 1;
		}
	}

	public bool TryFillColumn(int column) {
		var gemsToSpawn = GetEmptySlots(column);
		var gemsSpawned = 0;

		while (gemsSpawned < gemsToSpawn) {
			SpawnGem(column, gemsSpawned);

			gemsSpawned += 1;
		}

		return gemsToSpawn > 0;
	}

	public int GetEmptySlots(int column) {
		var height = _gridContainer.Columns;
		var field = new Vector2I(column, 0);

		var emptySlots = 0;

		while (field.Y < height) {
			var checkSlot = _slots[field.X, field.Y];
			if (checkSlot.HasGem) {
				break;
			}

			emptySlots += 1;
			field.Y += 1;
		}

		return emptySlots;
	}

	public void SpawnGem(int column, int offset) {
		var gem = _gemPrefab.InstantiateTo<Gem>(this);
		var field = new Vector2I(column, 0);
		var slot = _slots[field.X, field.Y];
		var position = slot.GlobalPosition;
		// GD.Print($"Beginning at slot {field} ({position})");

		position.Y -= (offset + 1) * _slotSize.Y;

		gem.GlobalPosition = position;

		var success = TryGetFallToSlot(field, out var targetSlot);
		if (!success) {
			return;
		}

		var blockedGemTypes = GetSpawnBlockedGemTypes(targetSlot);

		gem.DetermineType(blockedGemTypes); // Only on first spawn!
		gem.FallTo(targetSlot);

		// GD.Print($"TargetSlot: {targetSlot.field}");
	}

	/**
	 * Note:
	 * Only use this for the initial level spawn!
	 * It only checks for the order of gem creation in that case and
	 * re-spawn should not make exclusion-checks.
	 */
	public int[] GetSpawnBlockedGemTypes(Slot targetSlot) {
		var blockedGemTypes = new List<int>();

		var checkList = new [] {
			new [] { Vector2I.Left, Vector2I.Left * 2 },
			new [] { Vector2I.Down, Vector2I.Down * 2 }
		};

		foreach (var direction in checkList) {
			var hasSlot1 = TryGetSlot(targetSlot.field + direction[0], out var slot1);
			var hasSlot2 = TryGetSlot(targetSlot.field + direction[1], out var slot2);

			var hasTwoSlots = hasSlot1 && hasSlot2;
			if (!hasTwoSlots) {
				continue;
			}

			if (slot1.HasMatchingGem(slot2)) {
				blockedGemTypes.Add(slot1.gem.type);
			}
		}

		return blockedGemTypes.ToArray();
	}

	public bool TryGetSlot(Vector2I field, out Slot slot) {
		try {
			slot = _slots[field.X, field.Y];

			return true;
		}
		catch {
			slot = default;

			return false;
		}
	}

	public bool TryGetFallToSlot(Vector2I startField, out Slot slot) {
		var height = _gridContainer.Columns;
		var field = startField;

		slot = null;

		while (field.Y < height) {
			var checkSlot = _slots[field.X, field.Y];
			if (!checkSlot.HasGem) {
				slot = checkSlot;
				field.Y += 1;

				continue;
			}

			break;
		}

		return slot != null;
	}

	public void OnSelect(Slot slot) {
		_slot = slot;
	}

	public void OnEnter(Slot slot) {
		if (_slot == null) {
			return;
		}

		var isNext = _slot.IsNextTo(slot);

		GD.Print($"Trying to switch {_slot.field} and {slot.field} are next to each other: {isNext}");

		_slot = null;
	}
}