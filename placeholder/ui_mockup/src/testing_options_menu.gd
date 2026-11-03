extends  Node

# === Signals === (snake_case)
@onready var option_button: OptionButton = $MarginContainer/PanelContainer/VBoxContainer2/BottomBox/Options/Resolution/OptionButton
@onready var check_box: CheckBox = $MarginContainer/PanelContainer/VBoxContainer2/BottomBox/Options/FullScreen/CheckBox

# === Enums === (PascalCase, members CONSTANT_CASE)

# === Constants === (CONSTANT_CASE)

# === Exported Vars === (snake_case)

# === Public Vars === (snake_case)

# === Private Vars === (_underscored_snake_case)
var _resolutions = { 
	0: Vector2i(1920, 1080), 
	1: Vector2i(1280, 720)}

# === Godot Methods ===

func  _ready() -> void:
	pass

func  _physics_process(delta: float) -> void:
	pass

# === Further Methods === (public snake_case, private _underscored_snake_case, local vars snake_case)
func set_visible(menu_layer):
	var menu = get_node(menu_layer)
	menu.visible = false
	
func change_resolution(index):
	if(!check_box.button_pressed):
		get_window().size = (_resolutions[index])
		center_window()
	
func center_window():
	var center = DisplayServer.screen_get_position()+DisplayServer.screen_get_size()/2
	var size = get_window().get_size_with_decorations()
	get_window().set_position(center-size/2)

func _on_check_box_toggled(toggled_on: bool) -> void:
	if toggled_on:
		get_window().set_mode(Window.MODE_FULLSCREEN)
	else:
		get_window().set_mode(Window.MODE_WINDOWED)
		change_resolution(option_button.selected)
	pass # Replace with function body.
