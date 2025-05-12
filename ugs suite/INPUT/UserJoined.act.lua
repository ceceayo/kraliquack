Log("Hello from UserJoined")
Log(tostring(tiles))
MessageUser(joinedUserToken, "test", "HELLO REDIS WORLD")
--informUserFullBoard(joinedUserToken)
Log("User joined successfully")
Log(tostring(users[joinedUserToken]))
Log(tostring(users[joinedUserToken].Data))
users[joinedUserToken].Data["progressionState"] = "joined"
MessageUser(joinedUserToken, "progressionState", "joined")
users[joinedUserToken].Data["cash"] = "5000"
MessageUser(joinedUserToken, "cash", "5000")
--users[joinedUserToken].Data["cash"] = "99999999"
--MessageUser(joinedUserToken, "cash", "99999999")
users[joinedUserToken].Data["powerGeneration"] = "0"
users[joinedUserToken].Data["powerConsumption"] = "0"
MessageUser(joinedUserToken, "power", "0 0")
users[joinedUserToken].Data["cashGeneration"] = "0" -- per second
MessageUser(joinedUserToken, "cashGeneration", "0")
for k, v in pairs(entities) do
    MessageUser(joinedUserToken, "entity", tostring(v.id) .. "," .. tostring(v.x) .. "," .. tostring(v.y) .. "," .. tostring(v.type) .. "," .. tostring(v.owner) .. "," .. tostring(v.rotation) .. "," .. tostring(v.speed) .. "," .. tostring(v.health))
end
users[joinedUserToken].Data["hqHealth"] = "0"
MessageUser(joinedUserToken, "hqHealth", "0")