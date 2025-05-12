import httpx
import httpx_ws
import json
import time

username = 'root'
password = 'toor'

hostname = '://localhost:5418/'

request = httpx.post(f"http{hostname}session/start?userName={username}&password={password}")
assert request.status_code == 200
sessionToken = request.json()['sessionId']
print("Session Token:", sessionToken)

request = httpx.get(f"http{hostname}session/status", headers={'token': sessionToken})
assert request.status_code == 200
assert request.json()['state'] == 'sessionStartedNotInGame'
print("Session Status:", request.json())

request = httpx.post(f"http{hostname}games", headers={'token': sessionToken, 'Content-Type': 'application/json'}, data=json.dumps({
  "actions": [
    {
      "name": "GameStarted",
      "code": "Log(\"Game Started\")"
    },
    {
      "name": "UserJoined",
      "code": "Log(\"User Joined\")"
    },
    {
      "name": "UserLeft",
      "code": "Log(\"User Left\")"
    },
    {
      "name": "Tick",
      "code": ""
    },
    {
      "name": "UserAction Test",
      "code": "Log(actionData)"
    },
    {
      "name": "UserAction Test2",
      "code": "MessageUser(userToken, actionData)"
    }
  ],
  "name": "My First Game",
  "version": "1.0.0"
}))
assert request.status_code == 200
print("Connection Result:", request.json())
assert request.json()['success'] == True
assert request.json()['message'] == "Welcome"
assert request.json()['verificiationResult']['success'] == True
#assert request.json()['verificiationResult']['hash'] == '"HPgKNiD1ehdABOY+ipQ43S1XEveKcWsO6IQq6BOK8QEQ+AmOz76rsNafwaBljaDdxkmrn5hRt3p9PooFbuY3UA=="'

request = httpx.get(f"http{hostname}session/status", headers={'token': sessionToken})
assert request.status_code == 200
assert request.json()['state'] == 'sessionStartedGameSet'
print("Session Status:", request.json())

request = httpx.post(f"http{hostname}worlds", headers={'token': sessionToken, 'Content-Type': 'application/json'}, data=json.dumps({
  "worldName": "string"
}))
assert request.status_code == 200
assert request.json()['success'] == True
worldId = int(request.json()['message'])
print("World ID:", worldId)


with httpx_ws.connect_ws(f"ws{hostname}{worldId}.w/ws", headers={'token': sessionToken}) as ws:
    print("WebSocket connected")
    time.sleep(1)
    ws.send_json({"ActionName": "Test", "Data": ""})
    print("Sent: Test")
    time.sleep(1)
    ws.send_json({"ActionName": "Test", "Data": "Test"})
    print("Sent: Test")
    time.sleep(1)
    ws.send_json({"ActionName": "Test2", "Data": "Hello World"})
    print("Sent: Test2")
    recv = ws.receive_json()
    print("Received:", recv)
    