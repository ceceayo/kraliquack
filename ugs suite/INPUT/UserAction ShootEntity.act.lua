values = split(actionData, " ")

if #values ~= 2 then
    Log("Invalid data length")
    return
end

shooter = tonumber(values[1])
target = tonumber(values[2])

if shooter == nil or target == nil then
    Log("Invalid shooter or target")
    return
end

if entities[shooter] == nil or entities[target] == nil then
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

damage = 0

if entities[shooter].type == "duck" then
    damage = 1
elseif entities[shooter].type == "bug" then
    damage = 1
end


entities[target].health = entities[target].health - damage

if entities[target].health <= 0 then
    Log("Entity " .. tostring(target) .. " has been destroyed.")
    for k,v in iterateDict(users) do
        MessageUser(k, "entityDestroyed", tostring(target))
    end
    entities[target] = nil
else
    for k,v in iterateDict(users) do
        MessageUser(k, "entityDamage", tostring(target) .. "," .. tostring(damage) .. "," .. tostring(entities[target].health))
    end
end
