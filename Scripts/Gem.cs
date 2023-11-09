using System.Collections.Generic;
using Godot;
using Match3.Extends;

namespace Match3;

public partial class Gem : Node2D {
	public const float FallSpeed = 650f;

	public int type;

	[Export]
	public Sprite2D _sprite;

	[Export]
	public Texture2D[] _skins;

	public Slot _slot;

	public Vector2 _targetPosition;

	public Vector2 _speed;

	public override void _Ready() {
		SetProcess(false);
	}

	public override void _ExitTree() {
		Grid.instance.RemoveFallingGem(this);
	}

	public override void _Process(double delta) {
		GlobalPosition += _speed * (float)delta;

		if (GlobalPosition.Y > _targetPosition.Y) {
			GlobalPosition = _targetPosition;
			// GD.Print($"Arrived at: {_targetPosition}");

			Grid.instance.RemoveFallingGem(this);

			SetProcess(false);
		}
	}

	public void MoveTo(Vector2 position, float weight) {
		// GD.Print($"Moving {Position} to {position}");
		Position = Position.Lerp(position, weight);
	}

	public bool IsSame(Gem gem) {
		return type == gem.type;
	}

	public void DetermineType(int[] blockedTypes) {
		var allowedTypes = new List<int>();

		for (var i = 0; i < _skins.Length; i++) {
			var isBlocked = System.Array.IndexOf(blockedTypes, i) > -1;
			if (isBlocked) {
				continue;
			}

			allowedTypes.Add(i);
		}

		type = allowedTypes.ToArray().GetRandomElement();
		_sprite.Texture = _skins[type];
	}

	public void FallTo(Slot slot) {
		slot.gem = this;

		Grid.instance.AddFallingGem(this);

		_speed = Vector2.Down * FallSpeed;
		_targetPosition = slot.GlobalPosition;

		// GD.Print($"Fall to: {_targetPosition}");
		SetProcess(true);
	}
}