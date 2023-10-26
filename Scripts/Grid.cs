using Godot;

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

		_timer.Timeout += () => {
			SpawnGem(Vector2I.Zero);
			SpawnGem(Vector2I.Zero);
		};
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

	public void SpawnGem(Vector2I field) {
		var gem = _gemPrefab.Instantiate<Gem>();
		var slot = _slots[field.X, field.Y];
		var position = slot.GlobalPosition;

		position.Y -= _slotSize.Y;

		gem.GlobalPosition = position;

		AddChild(gem);

		var success = TryGetFallToSlot(field, out var targetSlot);
		if (!success) {
			GD.Print("Sefuq!?");

			return;
		}

		gem.FallTo(targetSlot);

		GD.Print($"TargetSlot: {targetSlot.field}");
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