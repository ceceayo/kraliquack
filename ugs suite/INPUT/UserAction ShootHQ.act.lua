values = split(actionData, " ")

if #values ~= 3 then
    Log("Invalid data length")
    return
end

shooter = tonumber(values[1])
targetx = tonumber(values[2])
targety = tonumber(values[3])

if shooter == nil or targetx == nil or targety == nil then
    Log("Invalid shooter or target")
    return
end

if targetx < 0 or targetx >= width or targety < 0 or targety >= height then
    Log("Target out of bounds")
    return
end

target = tiles[targety * width + targetx]

if entities[shooter] == nil or target == nil then
    Log("Invalid shooter or target entity")
    return
end

if entities[shooter].owner ~= userToken then
    Log("Shooter does not own the entity")
    return
end

if entities[shooter].type ~= "duck" and entities[shooter].type ~= "bug" then
    Log("Shooter is not of a known type.")
    return
end

if target.tile ~= "h" and target.tile ~= "H" then
    Log("Target is not a HQ")
    return
end

if target.owner == entities[shooter].owner then
    Log("Target is owned by the shooter")
    return
end

damage = 0

if entities[shooter].type == "duck" then
    damage = 1
elseif entities[shooter].type == "bug" then
    damage = 1
end


targetuser = target.owner

if targetuser == nil then
    Log("Target has no owner")
    return
end

if users[targetuser] == nil then
    Log("Target user does not exist")
    return
end

users[targetuser].Data["hqHealth"] = tostring(tonumber(users[targetuser].Data["hqHealth"]) - damage)

MessageUser(targetuser, "hqHealth", tostring(tonumber(users[targetuser].Data["hqHealth"])))


if tonumber(users[targetuser].Data["hqHealth"]) <= 0 then
    Log("User " .. tostring(targetuser) .. " has been destroyed.")
    for k,v in iterateDict(users) do
        MessageUser(k, "userDestroyed", tostring(targetuser))
    end
    users[targetuser].Data["progressionState"] = "destroyed"
    MessageUser(targetuser, "progressionState", "destroyed")
    users[targetuser].Data["hqHealth"] = "0"
    tiles[tonumber(users[targetuser].Data["hqPos"])].tile = "."
    tiles[tonumber(users[targetuser].Data["hqPos"])].owner = ""
    users[targetuser].Data["hqPos"] = nil
    for k,v in iterateDict(users) do
        MessageUser(k, "board", tostring(tonumber(users[targetuser].Data["hqPos"])) .. "," .. ".")
        MessageUser(k, "owner", tostring(tonumber(users[targetuser].Data["hqPos"])) .. "," .. "")
        MessageUser(k, "chat", "User " .. targetuser .. " has been destroyed.")
    end
else
    for k,v in iterateDict(users) do
        MessageUser(k, "userDamage", tostring(targetuser) .. "," .. tostring(users[targetuser].Data["hqHealth"]))
    end
end
