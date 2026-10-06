using Godot;
using System;

namespace SlackingOff;

public partial class VirusPullback : Node2D
{
    // === Signals === (PascalCaseEventHandler())

    // === Enums === (PascalCase, members CONSTANT_CASE)

    // === Constants === (CONSTANT_CASE)

    // === Exported Vars === (PascalCase)

    // === Public Vars === (PascalCase)
    public int Index;
    public Vector2 Direction;
    public Vector3 WeightAndFalloffs;
    public GradientTexture1D FalloffTex;

    // === Private Vars === (_underscoredCamelCase)
    private bool _initialized;

    // === Godot Methods ===

    public VirusPullback()
    {
        //runs when the object is created in memory, Parent -> Child
        _initialized = false;
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

    /// <summary>
    /// Sets initial values. Must be called before this begins to animate properly
    /// </summary>
    public void Init(int ind, Vector2 localPos, Vector2 pullDir, Vector3 weightAndFalloffs, GradientTexture1D falloffTex)
    {
        Index = ind;
        Position = localPos;
        Direction = pullDir;
        WeightAndFalloffs = weightAndFalloffs;
        FalloffTex = falloffTex;

        //mark initialized
        _initialized = true;
    }

    public void UpdateIndexIfNeeded(int ind)
    {

    }
}
