using System.Collections.Generic;
using Godot;
using Match3.Extends;

namespace Match3;

public partial class Grid : Node2D {
	public static Grid instance;

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

	[Export]
	public GridSwitch _gridSwitch;

	public readonly List<Gem> _fallingGems = new ();

	public override void _Ready() {
		instance = this;

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

	public void AddFallingGem(Gem gem) {
		_fallingGems.Add(gem);
	}

	public void RemoveFallingGem(Gem gem) {
		_fallingGems.Remove(gem);

		GD.Print($"Falling gems remaining: {_fallingGems.Count}");
		if (_fallingGems.Count > 0) {
			return;
		}

		GD.Print("Now checking field!");
		_ProcessPostGemDrop();
	}

	public void _ProcessPostGemDrop() {
		var width = _gridContainer.Columns;
		var height = _gridContainer.Columns;

		var hasAnyMatches = false;

		for (var x = 0; x < width; x++) {
			for (var y = 0; y < height; y++) {
				var field = new Vector2I(x, y);

				TryGetSlot(field, out var slot);

				var hasMatches = TryProcessMatches(slot);
				if (hasMatches) {
					hasAnyMatches = true;
				}
			}
		}

		if (!hasAnyMatches) {
			return;
		}

		DropGems();
		FillGrid();
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

		var spawnField = new Vector2I(field.X, field.Y - 1);

		var success = TryGetFallToSlot(spawnField, out var targetSlot);
		if (!success) {
			return;
		}

		var blockedGemTypes = GetSpawnBlockedGemTypes(targetSlot);

		gem.DetermineType(blockedGemTypes); // Only on first spawn!
		gem.FallTo(targetSlot);

		// GD.Print($"TargetSlot: {targetSlot.field}");
	}

	public void DropGems() {
		var width = _gridContainer.Columns;
		var height = _gridContainer.Columns;

		for (var x = 0; x < width; x++) {
			for (var y = 0; y < height; y++) {
				var field = new Vector2I {
					X = x,
					Y = (height - 1) - y
				};

				TryGetSlot(field, out var slot);
				if (!slot.HasGem) {
					continue;
				}

				var success = TryGetFallToSlot(field, out var targetSlot);
				if (!success) {
					continue;
				}

				slot.TryDropGemTo(targetSlot);
			}
		}
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

		field.Y += 1;
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

	public bool TryProcessMatches(Slot slot) {
		var success = _TryGetMatches(slot, out var slots);
		if (!success) {
			return false;
		}

		foreach (var matchingSlot in slots) {
			matchingSlot.TryClearGem();
		}

		return true;
	}

	public bool _TryGetMatches(Slot slot, out List<Slot> slots) {
		slots = new List<Slot>();

		var horSlots = new List<Slot>();

		_AddSlotsInDirection(slot, horSlots, Vector2I.Right);
		_AddSlotsInDirection(slot, horSlots, Vector2I.Left);

		// Only 2, since the source is not added.
		if (horSlots.Count < 2) {
			horSlots.Clear();
		}

		slots.AddRange(horSlots);

		var verSlots = new List<Slot>();

		_AddSlotsInDirection(slot, verSlots, Vector2I.Up);
		_AddSlotsInDirection(slot, verSlots, Vector2I.Down);

		// Only 2, since the source is not added.
		if (verSlots.Count < 2) {
			verSlots.Clear();
		}

		slots.AddRange(verSlots);

		if (
			horSlots.Count > 0 ||
			verSlots.Count > 0
		) {
			slots.Add(slot);

			GD.Print($"We have a winner! ({slots.Count} gems)");

			return true;
		}

		return false;
	}

	private void _AddSlotsInDirection(Slot slot, List<Slot> checkSlots, Vector2I direction) {
		var size = _gridContainer.Columns;

		for (var n = 1; n < size; n++) {
			var field = slot.field + direction * n;
			var success = TryGetSlot(field, out var checkSlot);
			if (!success) {
				break;
			}

			if (!slot.HasMatchingGem(checkSlot)) {
				break;
			}

			checkSlots.Add(checkSlot);
		}
	}

	public void OnSelect(Slot slot) {
		_slot = slot;
	}

	public void OnEnter(Slot slot) {
		if (_slot == null) {
			return;
		}

		var isNext = _slot.IsNextTo(slot);

		// GD.Print($"Trying to switch {_slot.field} and {slot.field} are next to each other: {isNext}");
		if (isNext) {
			_gridSwitch.Switch(slot, _slot, (slot1, slot2) => {
				var hasMatches1 = TryProcessMatches(slot1);
				var hasMatches2 = TryProcessMatches(slot2);

				var hasMatches = hasMatches1 || hasMatches2;
				if (!hasMatches) {
					_gridSwitch.Switch(slot1, slot2);

					return;
				}

				DropGems();
				FillGrid();
			});
		}

		_slot = null;
	}
}