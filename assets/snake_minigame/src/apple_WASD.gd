extends  Node2D

# === Signals === (snake_case)

# === Enums === (PascalCase, members CONSTANT_CASE)

# === Constants === (CONSTANT_CASE)

# === Exported Vars === (snake_case)
@export var continuous_move: bool = false
# === Public Vars === (snake_case)
@onready var grid: TileMapLayer = %Grid
@onready var game: Node2D = get_parent()
var input_dir = Vector2i(0,0)
var apple_tile: Vector2i = Vector2i(-1, -1)
# === Private Vars === (_underscored_snake_case)

# === Godot Methods ===

func  _ready() -> void:
	position = grid.map_to_local(Vector2i(5,5))
	Input.mouse_mode = Input.MOUSE_MODE_HIDDEN

func _physics_process(delta: float) -> void:
	if Input.is_action_pressed("up"): 
		input_dir = Vector2.UP
	if Input.is_action_pressed("right"): 
		input_dir = Vector2.RIGHT
	if Input.is_action_pressed("left"): 
		input_dir = Vector2.LEFT
	if Input.is_action_pressed("down"): 
		input_dir = Vector2.DOWN

# === Further Methods === (public snake_case, private _underscored_snake_case, local vars snake_case)
func move(direction: Vector2i):
	var current_tile: Vector2i = grid.local_to_map(global_position)
	var target_tile: Vector2i = Vector2i(
		current_tile + direction
	)
	if(target_tile in game.snake_body):
		return
	global_position = grid.map_to_local(target_tile)
	apple_tile = target_tile
	if not continuous_move: input_dir = Vector2i(0,0)
	
func _on_snake_tick_timeout() -> void:
	move(input_dir)
