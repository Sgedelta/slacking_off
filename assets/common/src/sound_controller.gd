class_name SoundController
extends  Node

# === Constants === (CONSTANT_CASE)

enum SoundNames {
	BGM,
	WON,
	LOST,
	SNK_TURNED_LEFT,
	SNK_TURNED_RIGHT,
	SNK_HURT_A,
	SNK_HURT_B,
	SNK_EAT_PLAYER,
	BOS_VIRUS_SWAT
}

# === Exported Vars === (snake_case)
@export var sound_players : Dictionary[SoundNames, AudioStreamPlayer] = {}
# === Public Vars === (snake_case)

# === Private Vars === (_underscored_snake_case)

# === Godot Methods ===

# === Further Methods === (public snake_case, private _underscored_snake_case, local vars snake_case)
func play_sound(sound_name : SoundNames):
	if sound_players.has(sound_name):
		sound_players[sound_name].play();
	else: 
		push_warning("requested sound ", str(SoundNames.keys()[sound_name]), " but did not have that as key in sound_player dictionary ", sound_players)

func stop_sound(sound_name : SoundNames):
	if sound_players.has(sound_name):
		sound_players[sound_name].stop();
	else: 
		push_warning("requested sound ", str(SoundNames.keys()[sound_name]), " but did not have that as key in sound_player dictionary ", sound_players)
