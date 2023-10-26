using Godot;

namespace Match3;

public partial class Grid : Node2D {
	[Export]
	public GridContainer _gridContainer;

	[Export]
	public PackedScene _slotPrefab;

	public Slot[,] _slots;

	public Slot _slot;

	public override void _Ready() {
		_CreateSlots();
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
			var position = new Vector2I {
				X = i % width,
				Y = i / width
			};

			var slot = _slotPrefab.Instantiate<Slot>();
			slot.Setup(this, position);

			_slots[position.X, position.Y] = slot;
			_gridContainer.AddChild(slot);
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

		GD.Print($"Trying to switch {_slot.position} and {slot.position} are next to each other: {isNext}");

		_slot = null;
	}
}