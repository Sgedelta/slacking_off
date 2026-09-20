// meta-name: Default
// meta-description: Custom base template for Node
// meta-default: true

using _BINDINGS_NAMESPACE_;
using System;

namespace SlackingOff;

public partial class _CLASS_ : _BASE_
{
	// === Signals === (PascalCaseEventHandler())
	
	// === Enums === (PascalCase, members CONSTANT_CASE)
	
	// === Constants === (CONSTANT_CASE)
	
	// === Exported Vars === (PascalCase)
	
	// === Public Vars === (PascalCase)
	
	// === Private Vars === (_underscoredCamelCase)
	
	// === Godot Methods ===
	
	public _CLASS_()
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