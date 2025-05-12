extends Control

var PlayScene = preload("res://play.tscn")

func _ready():
	Globals.ws = WebSocketPeer.new()
	Globals.ws.handshake_headers = PackedStringArray(["token: "+Globals.token])
	var err = Globals.ws.connect_to_url("ws"+Globals.server+str(Globals.worldId)+".w/ws")
	if err != OK:
		print("error")
		set_process(false)

func _process(delta: float) -> void:
	Globals.ws.poll()
	if Globals.ws.get_ready_state() == WebSocketPeer.STATE_OPEN:
		Globals.change_scene_to_node(PlayScene.instantiate())
