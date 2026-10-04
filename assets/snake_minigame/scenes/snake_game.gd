extends Node

# === Signals === (snake_case)

# === Enums === (PascalCase, members CONSTANT_CASE)

# === Constants === (CONSTANT_CASE)
const SNAKE = 0
# === Exported Vars === (snake_case)
@export var snake_body = [Vector2(5,10),Vector2(4,10),Vector2(3,10),Vector2(2,10), Vector2(1,10)]
@export var snake_direction = Vector2(1,0)
# === Public Vars === (snake_case)
@onready var grid: TileMapLayer = $Grid
@onready var snake_layer: TileMapLayer = $Snake
@onready var tick: Timer = $SnakeTick
var grid_size: Vector2i
# === Private Vars === (_underscored_snake_case)
# === Godot Methods ===
func  _ready() -> void:
	grid_size = $Grid.grid_size


	
func _input(_event):
	if Input.is_action_just_pressed("up"): 
		if not snake_direction == Vector2(0,1):
			snake_direction = Vector2(0,-1)
	if Input.is_action_just_pressed("right"): 
		if not snake_direction == Vector2(-1,0):
			snake_direction = Vector2(1,0)
	if Input.is_action_just_pressed("left"): 
		if not snake_direction == Vector2(1,0):
			snake_direction = Vector2(-1,0)
	if Input.is_action_just_pressed("down"): 
		if not snake_direction == Vector2(0,-1):
			snake_direction = Vector2(0,1)
# === Further Methods === (public snake_case, private _underscored_snake_case, local vars snake_case)
func draw_snake():
#	for block in snake_body:
#		$Snake.set_cell(Vector2(block.x,block.y),SNAKE,false,false,false,Vector2(8,0))

	for block_index in snake_body.size():
		var block = snake_body[block_index]
		
		if block_index == 0:
			var head_dir = relation2(snake_body[1],snake_body[0])
			if head_dir == 'right': 
				$Snake.set_cell(Vector2(block.x,block.y),SNAKE,Vector2(2,0), 0)
			if head_dir == 'left': 
				$Snake.set_cell(Vector2(block.x,block.y),SNAKE,Vector2(2,0), 1)
			if head_dir == 'top': 
				$Snake.set_cell(Vector2(block.x,block.y),SNAKE,Vector2(2,0), 3)
			if head_dir == 'bottom': 
				$Snake.set_cell(Vector2(block.x,block.y),SNAKE,Vector2(2,0), 2)
		elif block_index == snake_body.size() - 1:
			var tail_dir = relation2(snake_body[-1],snake_body[-2])
			if tail_dir == 'right': 
				$Snake.set_cell(Vector2(block.x,block.y),SNAKE,Vector2(0,0), 0)
			if tail_dir == 'left': 
				$Snake.set_cell(Vector2(block.x,block.y),SNAKE,Vector2(0,0), 1)
			if tail_dir == 'top': 
				$Snake.set_cell(Vector2(block.x,block.y),SNAKE,Vector2(0,0), 3)
			if tail_dir == 'bottom': 
				$Snake.set_cell(Vector2(block.x,block.y),SNAKE,Vector2(0,0), 2)
		
		else:
			var previous_block = snake_body[block_index + 1] - block
			var next_block = snake_body[block_index - 1] - block
			
			if previous_block.x == next_block.x:
				$Snake.set_cell(Vector2(block.x,block.y),SNAKE,Vector2(4,0), 1)
			elif previous_block.y == next_block.y:
				$Snake.set_cell(Vector2(block.x,block.y),SNAKE,Vector2(4,0), 0)
			else:
				if previous_block.x == -1 and next_block.y == -1 or next_block.x == -1 and previous_block.y == -1:
					$Snake.set_cell(Vector2(block.x,block.y),SNAKE,Vector2(5,0), 3)
				if previous_block.x == -1 and next_block.y == 1 or next_block.x == -1 and previous_block.y == 1:
					$Snake.set_cell(Vector2(block.x,block.y),SNAKE,Vector2(5,0), 1)
				if previous_block.x == 1 and next_block.y == -1 or next_block.x == 1 and previous_block.y == -1:
					$Snake.set_cell(Vector2(block.x,block.y),SNAKE,Vector2(5,0), 2)
				if previous_block.x == 1 and next_block.y == 1 or next_block.x == 1 and previous_block.y == 1:
					$Snake.set_cell(Vector2(block.x,block.y),SNAKE,Vector2(5,0), 0)

func relation2(first_block:Vector2,second_block:Vector2):
	var block_relation = second_block - first_block
	if block_relation == Vector2(-1,0): return 'left'
	if block_relation == Vector2(1,0): return 'right'
	if block_relation == Vector2(0,1): return 'bottom'
	if block_relation == Vector2(0,-1): return 'top'

func move_snake():
	delete_tiles(SNAKE)
	var new_head = snake_body[0] + snake_direction
	snake_body.pop_back()
	snake_body.push_front(new_head)

func delete_tiles(id:int):
	var cells = $Snake.get_used_cells_by_id(id)
	for cell in cells:
		$Snake.set_cell(Vector2(cell.x,cell.y),-1)

func is_game_over():
	var head = snake_body[0]
	# snake leaves the screen
	if head.x > grid_size.x - 1 or head.x < 0 or head.y < 0 or head.y > grid_size.y - 1:
		return true
		
	# snake bites its own tail
	for block in snake_body.slice(1):
		if block == head:
			return true
	return false

func reset():
	snake_body = [Vector2(5,10),Vector2(4,10),Vector2(3,10), Vector2(2,10), Vector2(1,10)]
	snake_direction = Vector2(1,0)	

func _on_snake_tick_timeout() -> void:
	move_snake()
	draw_snake()
	if is_game_over():
		reset()
