extends Node

# === Signals === (snake_case)

# === Enums === (PascalCase, members CONSTANT_CASE)

# === Constants === (CONSTANT_CASE)
const SNAKE = 0
const CONE_DEPTH := 1
const START_BODY: Array[Vector2i] = [
				Vector2i(18,10), 
				Vector2i(20-3,10), Vector2i(19-3,10), Vector2i(18-3,10), 
				Vector2i(20-6,10), Vector2i(19-6,10), Vector2i(18-6,10), 
				Vector2i(20-9,10), Vector2i(19-9,10), Vector2i(18-9,10), 
				Vector2i(20-12,10), Vector2i(19-12,10), Vector2i(18-12,10), 
				]
const SNAKE_DIR = Vector2i(1, 0)
# === Exported Vars === (snake_case)
@export var snake_body: Array[Vector2i] #= [Vector2i(5,10),Vector2i(4,10),Vector2i(3,10),Vector2i(2,10), Vector2i(1,10)]
@export var snake_direction = Vector2i(1,0)
# === Public Vars === (snake_case)
@onready var grid: TileMapLayer = $Grid
@onready var tick: Timer = $SnakeTick
var grid_size: Vector2i
# === Private Vars === (_underscored_snake_case)
# === Godot Methods ===
func  _ready() -> void:
	grid_size = $Grid.grid_size
	_reset_snake_state()
	# TODO: replace with something that isn't literally the laziest way to do this
	#snake_body = [ Vector2i(18,10), 
				#Vector2i(20-3,10), Vector2i(19-3,10), Vector2i(18-3,10), 
				#Vector2i(20-6,10), Vector2i(19-6,10), Vector2i(18-6,10), 
				#Vector2i(20-9,10), Vector2i(19-9,10), Vector2i(18-9,10), 
				#Vector2i(20-12,10), Vector2i(19-12,10), Vector2i(18-12,10), 
				#]
	#snake_direction = Vector2i(1, 0)
	
func _on_snake_tick_timeout() -> void:
	snake_direction = _find_direction()
	move_snake()
	draw_snake()
	if _player_lose():
		show_win(false)
	elif _snake_lose():
		show_win(true)

# === Further Methods === (public snake_case, private _underscored_snake_case, local vars snake_case)
func _reset_snake_state() -> void:
	snake_body = START_BODY.duplicate()
	snake_direction = SNAKE_DIR

func _find_direction() -> Vector2i:
	var left := Vector2i(snake_direction.y, -snake_direction.x)
	var paths: Array[Vector2i] = [_move_to_mouse(), snake_direction, left, -left]
	for dir in paths:
		if dir != -snake_direction && not _is_cone_blocked(dir):
			return dir 
	return snake_direction

func _move_to_mouse() -> Vector2i:
	var displacement : Vector2i = grid.mouse_tile - snake_body[0]
	var x_norm := Vector2i(signi(displacement.x), 0)
	var y_norm := Vector2i(0, signi(displacement.y))
	var x_big := absi(displacement.x) >= absi(displacement.y)
	var big_dir := x_norm if x_big else y_norm
	var small_dir := y_norm if x_big else x_norm
	return big_dir if big_dir != -snake_direction else small_dir
	
func _is_cone_blocked(dir: Vector2i) -> bool:
	var head: Vector2i = snake_body[0]
	var body := snake_body.slice(0, snake_body.size() - 1)
	var side := Vector2i(-dir.y, dir.x)
	for d in range(1, CONE_DEPTH + 1):
		for w in range(-(d - 1), d):
			var cell := head + dir * d + side * w
			if cell in body || _is_wall(cell):
				return true
	return false

func _is_wall(cell: Vector2i) -> bool:
	return cell.x < 0 or cell.y < 0 or cell.x >= grid_size.x or cell.y >= grid_size.y

func move_snake():
	$Snake.clear()
	var new_head = snake_body[0] + snake_direction
	snake_body.pop_back()
	snake_body.push_front(new_head)
	
func draw_snake():
#	for block in snake_body:
#		$Snake.set_cell(Vector2i(block.x,block.y),SNAKE,false,false,false,Vector2i(8,0))

	for block_index in snake_body.size():
		var block = snake_body[block_index]
		
		if block_index == 0:
			var head_dir = relation2(snake_body[1],snake_body[0])
			if head_dir == 'right': 
				$Snake.set_cell(Vector2i(block.x,block.y),SNAKE,Vector2i(2,0), 0)
			if head_dir == 'left': 
				$Snake.set_cell(Vector2i(block.x,block.y),SNAKE,Vector2i(2,0), 1)
			if head_dir == 'top': 
				$Snake.set_cell(Vector2i(block.x,block.y),SNAKE,Vector2i(2,0), 3)
			if head_dir == 'bottom': 
				$Snake.set_cell(Vector2i(block.x,block.y),SNAKE,Vector2i(2,0), 2)
		elif block_index == snake_body.size() - 1:
			var tail_dir = relation2(snake_body[-1],snake_body[-2])
			if tail_dir == 'right': 
				$Snake.set_cell(Vector2i(block.x,block.y),SNAKE,Vector2i(0,0), 0)
			if tail_dir == 'left': 
				$Snake.set_cell(Vector2i(block.x,block.y),SNAKE,Vector2i(0,0), 1)
			if tail_dir == 'top': 
				$Snake.set_cell(Vector2i(block.x,block.y),SNAKE,Vector2i(0,0), 3)
			if tail_dir == 'bottom': 
				$Snake.set_cell(Vector2i(block.x,block.y),SNAKE,Vector2i(0,0), 2)
		
		else:
			var previous_block = snake_body[block_index + 1] - block
			var next_block = snake_body[block_index - 1] - block
			
			if previous_block.x == next_block.x:
				$Snake.set_cell(Vector2i(block.x,block.y),SNAKE,Vector2i(4,0), 1)
			elif previous_block.y == next_block.y:
				$Snake.set_cell(Vector2i(block.x,block.y),SNAKE,Vector2i(4,0), 0)
			else:
				if previous_block.x == -1 and next_block.y == -1 or next_block.x == -1 and previous_block.y == -1:
					$Snake.set_cell(Vector2i(block.x,block.y),SNAKE,Vector2i(5,0), 3)
				if previous_block.x == -1 and next_block.y == 1 or next_block.x == -1 and previous_block.y == 1:
					$Snake.set_cell(Vector2i(block.x,block.y),SNAKE,Vector2i(5,0), 1)
				if previous_block.x == 1 and next_block.y == -1 or next_block.x == 1 and previous_block.y == -1:
					$Snake.set_cell(Vector2i(block.x,block.y),SNAKE,Vector2i(5,0), 2)
				if previous_block.x == 1 and next_block.y == 1 or next_block.x == 1 and previous_block.y == 1:
					$Snake.set_cell(Vector2i(block.x,block.y),SNAKE,Vector2i(5,0), 0)

func relation2(first_block:Vector2i,second_block:Vector2i):
	var block_relation = second_block - first_block
	if block_relation == Vector2i(-1,0): return 'left'
	if block_relation == Vector2i(1,0): return 'right'
	if block_relation == Vector2i(0,1): return 'bottom'
	if block_relation == Vector2i(0,-1): return 'top'
	return ''

func _player_lose():
	var head = snake_body[0]
	if head == grid.mouse_tile:
		print("player die")
		return true
		
	return false

func _snake_lose():
	var head = snake_body[0]
	# snake leaves the screen
	print(head)
	
	if _is_wall(head):
		print("snake die")
		return true
		
	# snake bites its own tail
	for block in snake_body.slice(1):
		if block == head:
			print("snake die")
			return true
	return false

func show_win(win: bool):
	tick.stop()
	%EndLabel.text = "YOU WIN" if win else "YOU LOSE"
	%EndLabel.add_theme_color_override("font_color", Color.GREEN if win else Color.RED)
	%EndLayer.visible = true

func _on_again_button_pressed() -> void:
	reset()

func reset():
	#snake_body = [Vector2i(5,10),Vector2i(4,10),Vector2i(3,10), Vector2i(2,10), Vector2i(1,10)]
	#snake_direction = Vector2i(1,0)	
	_reset_snake_state()
	$Snake.clear()
	draw_snake()
	%EndLayer.visible = false
	tick.start()
