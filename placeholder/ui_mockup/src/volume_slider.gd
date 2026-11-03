extends  HSlider

# === Signals === (snake_case)

# === Enums === (PascalCase, members CONSTANT_CASE)

# === Constants === (CONSTANT_CASE)

# === Exported Vars === (snake_case)
@export var bus_name: String

# === Public Vars === (snake_case)

# === Private Vars === (_underscored_snake_case)
var _bus_index: int

# === Godot Methods ===

func  _ready() -> void:
	_bus_index = AudioServer.get_bus_index(bus_name)
	value_changed.connect(_on_value_changed)
	
	value = db_to_linear(AudioServer.get_bus_volume_db(_bus_index))

func  _physics_process(delta: float) -> void:
	pass

# === Further Methods === (public snake_case, private _underscored_snake_case, local vars snake_case)
func _on_value_changed(value:float):
	AudioServer.set_bus_volume_db(_bus_index, linear_to_db(value))
