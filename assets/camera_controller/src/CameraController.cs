using Godot;
using Godot.Collections;
using System;

namespace SlackingOff;

public partial class CameraController : Camera3D
{
    // === Signals === (PascalCaseEventHandler())

    // === Enums === (PascalCase, members CONSTANT_CASE)
	public enum CameraPositions
	{
		COMPUTER,
		CLOCK,
		BOSS
	}

	// === Constants === (CONSTANT_CASE)
	private readonly Dictionary<CameraPositions, String> _cameraStateNames =
	new() {
		{ CameraPositions.COMPUTER, "computer_fullscreen" },
		{ CameraPositions.BOSS, "boss_game_view"}
	};

    // === Exported Vars === (PascalCase)

    // === Public Vars === (PascalCase)
    private AnimationNodeStateMachinePlayback SM;

    // === Private Vars === (_underscoredCamelCase)
    private AnimationPlayer _player;
	private AnimationTree _tree;

    // === Godot Methods ===

    public CameraController()
	{
		//runs when the object is created in memory, Parent -> Child
	}
	
	public override void _Ready()
	{
		//runs when the object and its children are ready, Child -> Parent
		_player = GetNode<AnimationPlayer>("AnimationPlayer");
		_tree = GetNode<AnimationTree>("AnimationTree");
        SM = (AnimationNodeStateMachinePlayback)_tree.Get("parameters/playback");
		GoToState(CameraPositions.BOSS);
	}
	
	// === Further Methods === (PascalCase, local variables camelCase)

	public void GoToState(CameraPositions state)
	{
		if (!_cameraStateNames.ContainsKey(state))
		{
			throw new Exception("string name for that state not registered!");
		}

		SM.Travel(_cameraStateNames[state]);
	}

}
