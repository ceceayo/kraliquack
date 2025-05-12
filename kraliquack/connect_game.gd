extends Control

var SelectWorldScene = preload("res://select_world.tscn")

func _ready():
	var game = JSON.stringify(preload("res://spec.json").data)
	print(game)
	$HTTPRequest.request("http"+Globals.server+"games", PackedStringArray(["token: "+Globals.token, "Content-Type: application/json"]), HTTPClient.Method.METHOD_POST, game)


func _on_http_request_request_completed(result: int, response_code: int, headers: PackedStringArray, body: PackedByteArray) -> void:
	if response_code == 200:
		var json = JSON.parse_string(body.get_string_from_utf8())
		print(json)
		if json["success"] == true:
			Globals.change_scene_to_node(SelectWorldScene.instantiate())
		else:
			print("error")
	else:
		print("error")
