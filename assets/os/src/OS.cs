using Godot;
using Godot.Collections;
using System;

namespace SlackingOff;

public partial class OS : Control
{
	// === Signals === (PascalCaseEventHandler())

	// === Enums === (PascalCase, members CONSTANT_CASE)
	public enum ProcessNames
	{
		NONE,
		WORK,
		SNAKE,

		PROCESS_MAX
	}

	// === Constants === (CONSTANT_CASE)

	// === Exported Vars === (PascalCase)
	[Export] public PackedScene AppWindow { get; set; }
	[Export] public Dictionary<ProcessNames, PackedScene> ProcessPackedScenes = new();
	
	// === Public Vars === (PascalCase)
	
	// === Private Vars === (_underscoredCamelCase)



	
	// === Godot Methods ===
	
	public OS()
	{
		//runs when the object is created in memory, Parent -> Child
	}
	
	public override void _Ready()
	{
		//runs when the object and its children are ready, Child -> Parent
	}
	
	// Prefer using PhysicsProcess over Process
	public override void _PhysicsProcess(double delta)
	{
		
	}
	
	// === Further Methods === (PascalCase, local variables camelCase)
}
