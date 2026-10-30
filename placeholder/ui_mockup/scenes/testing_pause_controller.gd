extends  Node

# === Signals === (snake_case)

# === Enums === (PascalCase, members CONSTANT_CASE)

# === Constants === (CONSTANT_CASE)

# === Exported Vars === (snake_case)

# === Public Vars === (snake_case)

# === Private Vars === (_underscored_snake_case)
@onready var _menu = $PauseMenu
@onready var _options = $PauseMenu/OptionsMenu

# === Godot Methods ===

func  _ready() -> void:
	_menu.connect("on_close_requested", func(): _set_paused(false))
	pass
	
func _input(event: InputEvent) -> void:
	if(event.is_action_pressed("pause")):
		_set_paused(!get_tree().paused)
# === Further Methods === (public snake_case, private _underscored_snake_case, local vars snake_case)
func _set_paused(state : bool):
	set_visible(_menu, state)
	set_visible(_options, false)
	get_tree().paused = state

func set_visible(menu_layer, visibility : bool):
	menu_layer.visible = visibility
