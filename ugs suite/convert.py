import json
import os

actions = []

NAME = 'KRALIQUACK'
VERSION = '0.0.1'

for file in os.listdir('INPUT'):
    if file.endswith('.act.lua'):
        with open(os.path.join('INPUT', file), 'r') as f:
            actions.append({"name": file[:-8], "code": f.read()})


data = {
    "actions": actions,
    "name": NAME,
    "version": VERSION
}

with open('../kraliquack/spec.json', 'w') as f:
    f.write(json.dumps(data))
