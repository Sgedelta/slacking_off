using Godot;
using System;

namespace SlackingOff;

public partial class EmailTask : Control
{
	// === Signals === (PascalCaseEventHandler())

	// === Enums === (PascalCase, members CONSTANT_CASE)

	// === Constants === (CONSTANT_CASE)

	// === Exported Vars === (PascalCase)

	// === Public Vars === (PascalCase)

	// === Private Vars === (_underscoredCamelCase)


	private ItemList _tabs;
	private TabContainer _display;
	
	// === Godot Methods ===
	
	public EmailTask()
	{
		//runs when the object is created in memory, Parent -> Child
	}
	
	public override void _Ready()
	{
		//runs when the object and its children are ready, Child -> Parent

		_tabs = GetNode<ItemList>("EmailList");
		_display = GetNode<TabContainer>("EmailDisplay");

		_tabs.Select(0);
	}
	
	// Prefer using PhysicsProcess over Process
	public override void _PhysicsProcess(double delta)
	{
		
	}
	
	// === Further Methods === (PascalCase, local variables camelCase)

	public void SetTabVisible(int index)
	{
		_display.CurrentTab = index;
	}
}
