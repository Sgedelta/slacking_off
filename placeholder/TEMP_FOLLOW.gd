extends CharacterBody2D

const ACCEL  = 2500
const SPEED = 1500
const ARRIVED_RADIUS = 50;



func _physics_process(delta: float) -> void:
	var dist_to_mouse = get_global_mouse_position() - global_position
	var desired = dist_to_mouse.normalized() * SPEED #if dist_to_mouse.length() > ARRIVED_RADIUS else Vector2.ZERO
	velocity = velocity.move_toward(desired, ACCEL * delta)
	
	move_and_slide()
