s = "<" .. userToken .. "> " .. actionData

for k,v in iterateDict(users) do
    MessageUser(k, "chat", s)
end