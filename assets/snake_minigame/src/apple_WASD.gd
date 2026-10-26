extends  Node2D

# === Signals === (snake_case)

# === Enums === (PascalCase, members CONSTANT_CASE)

# === Constants === (CONSTANT_CASE)

# === Exported Vars === (snake_case)
@export var continuous_move: bool = false
@export var start_tile := Vector2i(5, 5)
# === Public Vars === (snake_case)
@onready var grid: TileMapLayer = %Grid
@onready var game: Node2D = get_parent()

var input_dir = Vector2i.ZERO
var apple_tile: Vector2i = Vector2i(-1, -1)
var _try_jump := false
# === Private Vars === (_underscored_snake_case)

# === Godot Methods ===

func  _ready() -> void:
	position = grid.map_to_local(Vector2i(5,5))
	apple_tile = start_tile
	input_dir = Vector2i.ZERO
	Input.mouse_mode = Input.MOUSE_MODE_HIDDEN

func _physics_process(delta: float) -> void:
	if Input.is_action_pressed("up"): 
		input_dir = Vector2.UP
	elif Input.is_action_pressed("right"): 
		input_dir = Vector2.RIGHT
	elif Input.is_action_pressed("left"): 
		input_dir = Vector2.LEFT
	elif Input.is_action_pressed("down"): 
		input_dir = Vector2.DOWN
	#might wanna make this 2 ticks? or cooldown
	if Input.is_action_pressed("space"):
		_try_jump = true

# === Further Methods === (public snake_case, private _underscored_snake_case, local vars snake_case)
func move(direction: Vector2i):
	if direction == Vector2i.ZERO: return
	if not continuous_move: input_dir = Vector2i.ZERO
	
	var target_tile := apple_tile + direction * (2 if _try_jump else 1)
	_try_jump = false
	var current_tile: Vector2i = grid.local_to_map(global_position)

	if(game._is_wall(target_tile) or target_tile in game.snake_body):
		return
	global_position = grid.map_to_local(target_tile)
	apple_tile = target_tile
	
func _on_apple_tick_timeout() -> void:
	move(input_dir)
