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
	BOS_VIRUS_SWAT,
	BOS_SEQUENCE_FAIL,
	BOS_SEQUENCE_FINISH,
	BOS_VIRUS_APPEAR,
	BOS_VIRUS_JUMP,
	BOS_BGM_KEYS
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
	
func adjust_volume(sound_name : SoundNames, new_vol : float = 0):
	if sound_players.has(sound_name):
		sound_players[sound_name].volume_db = new_vol
	else: 
		push_warning("requested sound ", str(SoundNames.keys()[sound_name]), " but did not have that as key in sound_player dictionary ", sound_players)
	pass
		
func change_clip(sound_name : SoundNames, clip_name : String):
	if sound_players.has(sound_name):
		var clip_player = sound_players[sound_name]
		if clip_player.stream is AudioStreamInteractive:
			clip_player.get_stream_playback().switch_to_clip_by_name(clip_name)
			pass
	else:
		push_warning("requested sound clip change ", str(SoundNames.keys()[sound_name]), " but did not have that as key in sound_player dictionary ", sound_players, " or sound is not an interactive clip")
	
func fade_stop(sound_name : SoundNames, duration : float = 4):
	if sound_players.has(sound_name):
		var start_volume = sound_players[sound_name].volume_db
		
		var fade_tween = create_tween()
		fade_tween.tween_property(sound_players[sound_name], "volume_db", -80.0, duration)
		fade_tween.set_trans(Tween.TRANS_SINE)
		fade_tween.set_ease(Tween.EASE_IN)
		
		await fade_tween.finished
		sound_players[sound_name].stop();
		sound_players[sound_name].volume_db = start_volume
	else:
		push_warning("requested sound ", str(SoundNames.keys()[sound_name]), " but did not have that as key in sound_player dictionary ", sound_players)
