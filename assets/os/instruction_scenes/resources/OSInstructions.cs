using Godot;
using Godot.Collections;
using System;

[GlobalClass]
public partial class OSInstructions : Resource
{
    [Export] public int GridCols = 1;
    [Export] public Array<Texture2D> InstructionsInOrder;
    [Export] public PackedScene GameToLoad;

    public OSInstructions()
    {
        InstructionsInOrder = new Array<Texture2D>();
    }

}
