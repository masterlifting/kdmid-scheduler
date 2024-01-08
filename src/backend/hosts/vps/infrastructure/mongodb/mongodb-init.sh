#!/bin/bash

echo "Waiting for keyfile to be created..."
sleep 2

chmod 400 /opt/keyfiles/mongodb-keyfile
mongod --logpath /var/log/mongodb.log --bind_ip_all --replSet rs0 --keyFile /opt/keyfiles/mongodb-keyfile

echo "Keyfile created successfully"