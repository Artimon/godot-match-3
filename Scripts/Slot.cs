using System;
using Godot;
using Match3.Extends;

namespace Match3;

public partial class Slot : Control {
	[Export]
	public Grid _grid;

	[Export]
	public Button _button;

	public Vector2I field;

	public Gem gem;

	public bool HasGem => gem != null;

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
	public void Setup(Grid grid, Vector2I field) {
		this._grid = grid;
		this.field = field;
	}

	public bool IsNextTo(Slot slot) {
		var delta = field - slot.field;

		delta.X = Math.Abs(delta.X);
		delta.Y = Math.Abs(delta.Y);

		return delta == Vector2I.Right ||
		       delta == Vector2I.Down;
	}

	public bool HasMatchingGem(Slot slot) {
		if (
			gem == null ||
			slot.gem == null
		) {
			return false;
		}

		return gem.IsSame(slot.gem);
	}

	public bool TryClearGem() {
		if (!HasGem) {
			return false;
		}

		gem.Remove();

		return true;
	}
}
