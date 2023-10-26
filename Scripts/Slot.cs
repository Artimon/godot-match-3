using System;
using Godot;

namespace Match3;

public partial class Slot : Control {
	[Export]
	public Grid _grid;

	[Export]
	public Button _button;

	public Vector2I position;

	public Gem gem;

	public override void _Ready() {
		_button.Pressed += () => {
			_grid.OnSelect(this);
		};

		_button.MouseEntered += () => {
			_grid.OnEnter(this);
		};
	}

	/**
	 * Mandatory initialisation to be able to use the slot.
	 */
	public void Setup(Grid grid, Vector2I position) {
		this._grid = grid;
		this.position = position;
	}

	public bool IsNextTo(Slot slot) {
		var delta = position - slot.position;

		delta.X = Math.Abs(delta.X);
		delta.Y = Math.Abs(delta.Y);

		return delta == Vector2I.Right ||
		       delta == Vector2I.Down;
	}
}
