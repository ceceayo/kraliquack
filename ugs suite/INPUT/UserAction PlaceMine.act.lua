
if users[userToken].Data["progressionState"] ~= "placedHQ" and users[userToken].Data["progressionState"] ~= "placedBarracks" and users[userToken].Data["progressionState"] ~= "placedFactory" then
    Log("User in wrong state")
    return
end

data = {}
for u in string.gmatch(actionData, "%-?%d+") do
    data[#data + 1] = u
end

x = tonumber(data[1])
y = tonumber(data[2])


data = nil

Log("X: " .. tostring(x))
Log("Y: " .. tostring(y))


if isValidCell(x,y) == false then
    Log("Invalid cell")
    return
end

if cellCanBeBuiltOn(x,y) == false then
    Log("Cell cannot be built on")
    return
end

if userMakePurchase(userToken, 1500) == false then
    Log("User cannot afford Barracks")
    return
end

if userRequirePower(userToken, 30) == false then
    Log("User does not have enough power")
    return
end

Log("All checks passed")

userMakePurchaseApply(userToken, 1500)
userRequirePowerApply(userToken, 30)

tiles[y * width + x].tile = "m"
tiles[y * width + x].owner = userToken


users[userToken].Data["cashGeneration"] = tostring(tonumber(users[userToken].Data["cashGeneration"]) + 40)
MessageUser(userToken, "cashGeneration", users[userToken].Data["cashGeneration"])	

for k, v in iterateDict(users) do
    MessageUser(k, "board", tostring(y*width+x) .. "," .. tostring(tiles[y * width + x].tile))
    MessageUser(k, "owner", tostring(y*width+x) .. "," .. tostring(tiles[y * width + x].owner))	
end

