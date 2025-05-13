extends Node2D

var is_set_up = false
var is_local = false

@onready var speed: float = max_speed

@export var max_speed = 60.0
@export var range = 2*64

var next_point: Vector2
var moving = false
var pos = Vector2()

var last_sent_rotation = 0.0
var last_sent_speed = 0.0

var myid = 0

var player: String = ""

var target: Vector2 = Vector2.ZERO

var wander_timer_started = false

var health = 0

func set_data(
	id_: int,
	x: float,
	y: float,
	player_: String,
	rotation_: float,
	speed_: float,
	health_: int
):
	if not is_set_up:
		pos = Vector2(x * 64, y * 64)
		rotation_degrees = rotation_
		
		is_set_up = true
		myid = id_
		player = player_
		health = health_
		if player == Globals.token:
			is_local = true
			target = (pos / 64) + Vector2(randi_range(-2,2),randi_range(-2,2))
		else:
			speed = speed_
			
	elif not is_local:
		pos = Vector2(x * 64, y * 64)
		rotation_degrees = rotation_
		speed = speed_

func _process(delta):
	if is_local:
		var path = Globals.astar.get_point_path(position / 64, target, true)
		if len(path) > 1:
			next_point = path[1]
			var old_pos = pos
			_move_to(next_point, delta)
			
			# Check if direction (rotation) or speed has changed
			var new_rotation = (next_point - pos).angle()
			rotation = new_rotation
			if !is_equal_approx(new_rotation, last_sent_rotation) or !is_equal_approx(speed, last_sent_speed):
				_update_movement_with_server(new_rotation, speed, pos.x / 64, pos.y / 64)
				last_sent_rotation = new_rotation
				last_sent_speed = speed
		else:
			if not wander_timer_started:
				wander_timer_started = true
				$WanderTimer.start()
			if !is_equal_approx(rotation_degrees, last_sent_rotation) or !is_equal_approx(0, last_sent_speed):
				_update_movement_with_server(rotation, 0, pos.x / 64, pos.y / 64)
				last_sent_rotation = rotation_degrees
				last_sent_speed = 0
			
	else:
		var rot = Vector2.from_angle(rotation)
		pos += rot*speed*delta
		
	position = pos + Vector2(32, 32)
	$Label.text = str(health)

func _move_to(target: Vector2, delta):
	var direction = (target - pos).normalized()
	var distance = speed * delta
	wander_timer_started = false
	$WanderTimer.stop()
	if pos.distance_to(target) > distance:
		pos += direction * distance
	else:
		pos = target

func _update_movement_with_server(rotation_, speed, x, y):
	assert(is_set_up)
	assert(is_local)
	Globals.ws.send_text(JSON.stringify({
		"ActionName": "UpdateEntity",
		"Data": str(myid) + " " +str(float(rad_to_deg(rotation_))) + " " + str(float(speed)) + " " + str(float(x)) + " " + str(float(y))
	}))


func _shoot() -> void:
	if is_local:
		var enemies_in_range = []
		for child in $"..".get_children():
			if child.player != player:
				if position.distance_to(child.position) < range:
					enemies_in_range.append(child)
		if len (enemies_in_range) > 0 :
			var target = enemies_in_range.pick_random()
			Globals.ws.send_text(JSON.stringify({
				"ActionName": "ShootEntity",
				"Data": str(int(myid)) + " " + str(int(target.myid))
			}))
		else:
			var headquartersInRange = []
			for offsetX in [-2, -1, 0, 1, 2]:
				for offsetY in [-2, -1, 0, 1, 2]:
					if $"../../TileMapLayer".get_cell_atlas_coords(Vector2i(position/64)+Vector2i(offsetX, offsetY)) in [Vector2i(3,0),Vector2i(4,0)]:
						headquartersInRange.append(Vector2i(position/64)+Vector2i(offsetX, offsetY))
			var shootHQOptions = []
			if len(headquartersInRange) > 0:
				for hq in headquartersInRange:
					if (str(int(hq.y*$"../..".width+hq.x))) in $"../..".owners.keys():
						if $"../..".owners[str(int(hq.y*$"../..".width+hq.x))] != Globals.token:
							shootHQOptions.append(hq)
					else:
						print(str(int(hq.y*$"../..".width+hq.x)))
			if len(shootHQOptions) > 0:
				var hq = shootHQOptions.pick_random()
				Globals.ws.send_text(JSON.stringify({
					"ActionName": "ShootHQ",
					"Data": str(int(myid)) + " " + str(int(hq.x)) + " " + str(int(hq.y))
				}))


func _wander() -> void:
	wander_timer_started = false
	target = target + Vector2(randi_range(-1,1),randi_range(-1,1))

func _damage(new_health: int) -> void:
	health = new_health
