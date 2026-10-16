extends  Control

# === Signals === (snake_case)

# === Enums === (PascalCase, members CONSTANT_CASE)

# === Constants === (CONSTANT_CASE)

# === Exported Vars === (snake_case)
@export var symbol_size = 100.0
@export var symbol_complete_alpha = .5
# === Public Vars === (snake_case)

# === Private Vars === (_underscored_snake_case)
@onready var _symbol_container = $SymbolBounds/SymbolContainer
var _symbol_arr = []

# === Godot Methods ===
func  _ready() -> void:
	pass


# === Further Methods === (public snake_case, private _underscored_snake_case, local vars snake_case)
func display_symbols(arr : Array[Array]) -> void:
	clear_symbols()
	for i in range(len(arr)):
		var new_sym = TextureRect.new()
		new_sym.texture = arr[i][0]
		new_sym.self_modulate = arr[i][1]
		new_sym.custom_minimum_size = Vector2(symbol_size, symbol_size)
		new_sym.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
		new_sym.stretch_mode = TextureRect.STRETCH_KEEP_ASPECT
		_symbol_arr.append(new_sym)
		_symbol_container.add_child(new_sym)

func mark_symbol_complete(index : int) -> void:
	_symbol_arr[index].self_modulate.a = symbol_complete_alpha

func clear_symbols() -> void:
	for i in range(len(_symbol_arr)):
		_symbol_arr[i].queue_free()
	_symbol_arr.clear()
