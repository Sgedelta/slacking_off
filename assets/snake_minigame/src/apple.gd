extends CharacterBody2D

@export var speed: float = 300.0

var active := true

func _ready() -> void:
	motion_mode = CharacterBody2D.MOTION_MODE_FLOATING
	collision_layer = 2  # apple
	collision_mask = 1   # snake body

func _physics_process(delta: float) -> void:
	if not active:
		velocity = Vector2.ZERO
	else:
		var to_mouse := get_global_mouse_position() - global_position
		velocity = to_mouse.normalized() * minf(speed, to_mouse.length() / delta)
	move_and_slide()
