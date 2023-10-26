using Godot;

namespace Match3;

public partial class Gem : Node2D {
	[Export]
	public Sprite2D _sprite;

	public Slot _slot;

	public Vector2 _targetPosition;

	public Vector2 _speed;

	public override void _Ready() {
		SetProcess(false);
	}

	public override void _Process(double delta) {
		GlobalPosition += _speed * (float)delta;

		if (GlobalPosition.Y > _targetPosition.Y) {
			GlobalPosition = _targetPosition;
			GD.Print($"Arrived at: {_targetPosition}");

			SetProcess(false);
		}
	}

	public void FallTo(Slot slot) {
		slot.gem = this;

		_speed = Vector2.Down * 80f;
		_targetPosition = slot.GlobalPosition;

		GD.Print($"Fall to: {_targetPosition}");
		SetProcess(true);
	}
}