using System;
using Godot;

namespace Match3;

public partial class GridSwitch : Node2D {
	[Export]
	public float _switchDuration = 0.65f;

	public float _timer;
	public Slot _slot1;
	public Slot _slot2;

	public Action<Slot, Slot> _onFinished;

	public override void _Ready() {
		SetProcess(false);
	}

	public override void _Process(double delta) {
		_timer += (float)delta / _switchDuration;
		if (_timer < 1f) {
			// References are already switched, move gems to their target slot.
			_slot1.gem.MoveTo(_slot1.Position, _timer);
			_slot2.gem.MoveTo(_slot2.Position, _timer);

			return;
		}

		// GD.Print("Finished!");
		SetProcess(false);

		_slot1.gem.Position = _slot1.Position;
		_slot2.gem.Position = _slot2.Position;

		_onFinished?.Invoke(_slot1, _slot2);
	}

	public void Switch(Slot slot1, Slot slot2, Action<Slot, Slot> onFinished = null) {
		_timer = 0f;
		_slot1 = slot1;
		_slot2 = slot2;
		_onFinished = onFinished;

		(_slot2.gem, _slot1.gem) = (_slot1.gem, _slot2.gem);

		SetProcess(true);
	}
}