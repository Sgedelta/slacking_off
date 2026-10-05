extends  Node3D

# === Signals === (snake_case)

# === Enums === (PascalCase, members CONSTANT_CASE)

# === Constants === (CONSTANT_CASE)

# === Exported Vars === (snake_case)

# === Public Vars === (snake_case)

# === Private Vars === (_underscored_snake_case)
@onready var _cc = $CameraController
@onready var _path_a = $PathA
var _running_tween;

# === Further Methods === (public snake_case, private _underscored_snake_case, local vars snake_case)

func placeCameraOnButtonACallback(spinbox: NodePath):
	_cc.SetNewPath(_path_a, (get_node(spinbox) as SpinBox).value)

func runTweenA(from_progress:NodePath, to_progress:NodePath, in_time:NodePath):
	if(is_instance_valid(_running_tween)):
		_running_tween.kill()
	var start = (get_node(from_progress) as SpinBox).value
	var end = (get_node(to_progress) as SpinBox).value
	var time = (get_node(in_time) as SpinBox).value
	
	_cc.TweenCameraToProgressRatioInTimeWithStart(end, time, start);
	_cc.TweenCameraToProgressRatioInTimeWithStart(start, time, end);

func setPositionToIndex(index_spinner:NodePath):
	_cc.SetCameraToPathIndex((get_node(index_spinner) as SpinBox).value)
