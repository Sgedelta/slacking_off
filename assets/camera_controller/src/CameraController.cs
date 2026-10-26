using Godot;
using System;

namespace SlackingOff;

public partial class CameraController : Camera3D
{
    // === Signals === (PascalCaseEventHandler())

    // === Enums === (PascalCase, members CONSTANT_CASE)

    // === Constants === (CONSTANT_CASE)

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
	}
	
	// === Further Methods === (PascalCase, local variables camelCase)
}
