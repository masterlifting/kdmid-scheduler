#!/bin/bash

chmod 400 /opt/keyfiles/mongo-keyfile

mongod --fork --logpath /var/log/mongodb.log --bind_ip_all --replSet rs0 --keyFile /opt/keyfiles/mongo-keyfile

echo "Waiting for MongoDB to start..."
sleep 2

mongosh --eval 'rs.initiate({
    _id: "rs0",
    version: 1,
    members: [
        { _id: 1, host: "mongo1:27017", priority: 2 },
        { _id: 2, host: "mongo2:27017", priority: 1 },
        { _id: 3, host: "mongo3:27017", priority: 1 },
    ],
    },
    { force: true });'

echo "Waiting for the replica set to elect a primary..."
sleep 2

mongosh --eval 'rs.status();'

mongosh admin --eval 'db.createUser({
  user: "admin",
  pwd: "adminPassword",
  roles: [{ role: "root", db: "admin" }]
});'

tail -f /dev/null
