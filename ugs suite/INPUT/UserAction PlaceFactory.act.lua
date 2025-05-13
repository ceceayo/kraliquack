
if users[userToken].Data["progressionState"] ~= "placedBarracks" and users[userToken].Data["progressionState"] ~= "placedFactory" then
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

if userMakePurchase(userToken, 5000) == false then
    Log("User cannot afford Barracks")
    return
end

if userRequirePower(userToken, 60) == false then
    Log("User does not have enough power")
    return
end

Log("All checks passed")

userMakePurchaseApply(userToken, 5000)
userRequirePowerApply(userToken, 60)

tiles[y * width + x].tile = "f"
tiles[y * width + x].owner = userToken


if users[userToken].Data["progressionState"] == "placedBarracks" then
    users[userToken].Data["progressionState"] = "placedFactory"
    MessageUser(userToken, "progressionState", "placedFactory")
end



for k, v in iterateDict(users) do
    MessageUser(k, "board", tostring(y*width+x) .. "," .. tostring(tiles[y * width + x].tile))
    MessageUser(k, "owner", tostring(y*width+x) .. "," .. tostring(tiles[y * width + x].owner))	
end

