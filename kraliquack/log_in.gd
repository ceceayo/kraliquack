extends Control

var connect_game = preload("res://connect_game.tscn")

func _on_button_pressed() -> void:
	$Button.disabled = true
	Globals.server = $Server.text
	# TODO: String.uri_encode
	$HTTPRequest.request("http"+Globals.server+"session/start?userName="+$Username.text+"&password="+$Password.text, PackedStringArray(), HTTPClient.METHOD_POST)



func _on_http_request_request_completed(result: int, response_code: int, headers: PackedStringArray, body: PackedByteArray) -> void:
	if response_code == 200:
		var json = JSON.parse_string(body.get_string_from_utf8())
		print(json["sessionId"])
		Globals.token = json["sessionId"]
		Globals.change_scene_to_node(connect_game.instantiate())
	else:
		print("Error or smth?")
