extends Area2D


@export var bounded_bodies : Array[Node2D];
@export var local_return_pos : Vector2;

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	pass


func _on_body_exited(body: Node2D) -> void:
	if bounded_bodies.has(body):
		body.global_position = to_global(local_return_pos)
